using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Validators;
using Expense.Tracker.Services.Factories;
using Expense.Tracker.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ITransactionValidationService _transactionValidator;
        private readonly ITransactionFactory _transactionFactory;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            ITagRepository tagRepository,
            ITransactionValidationService transactionValidator,
            ITransactionFactory transactionFactory,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _transactionValidator = transactionValidator;
            _transactionFactory = transactionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _transactionRepository.GetByUserIdAsync(userId);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(string id, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _transactionRepository.GetByUserIdAndIdAsync(userId, id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            // Validate that the category belongs to the current user
            await _transactionValidator.ValidateCategoryExistsAsync(userId, categoryId);

            return await _transactionRepository.GetByUserIdAndCategoryIdAsync(userId, categoryId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _transactionRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
        }

        public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request, Requestor requestor)
        {
            try
            {
                var userId = Guid.Parse(requestor.UserId);

                // Validate category exists and belongs to the current user
                await _transactionValidator.ValidateCategoryExistsAsync(userId, request.CategoryId);

                // Validate recurring transaction settings
                _transactionValidator.ValidateRecurringTransactionSettings(request);

                var transaction = _transactionFactory.CreateTransaction(request, requestor.UserId);

                var createdTransaction = await _transactionRepository.CreateAsync(transaction);

                // Update tag usage counts
                if (transaction.Tags?.Count > 0)
                {
                    var tagTasks = transaction.Tags.Select(tagName => _tagRepository.IncrementUsageAsync(tagName));
                    await Task.WhenAll(tagTasks);
                }

                _logger.LogInformation("Created transaction {TransactionId} for user {UserId}", 
                    createdTransaction.Id, userId);

                return createdTransaction;
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning("Category validation failed for user {UserId}: {Error}", 
                    requestor.UserId, ex.Message);
                throw;
            }
            catch (InvalidRecurringTransactionException ex)
            {
                _logger.LogWarning("Recurring transaction validation failed: {Error}", ex.Message);
                throw;
            }
        }

        public async Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            var existingTransaction = await _transactionRepository.GetByUserIdAndIdAsync(userId, request.Id);
            if (existingTransaction == null)
            {
                return null;
            }

            // Validate category if provided and ensure it belongs to the current user
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                await _transactionValidator.ValidateCategoryExistsAsync(userId, request.CategoryId);
            }

            // Update only provided fields
            if (request.Amount.HasValue)
                existingTransaction.Amount = request.Amount.Value;
            if (!string.IsNullOrEmpty(request.Description))
                existingTransaction.Description = request.Description;
            if (request.Date.HasValue)
                existingTransaction.Date = request.Date.Value;
            if (request.Type.HasValue)
                existingTransaction.Type = request.Type.Value;
            if (!string.IsNullOrEmpty(request.CategoryId))
                existingTransaction.CategoryId = request.CategoryId;
            if (request.Tags != null)
                existingTransaction.Tags = request.Tags ?? existingTransaction.Tags;
            if (request.Notes != null)
                existingTransaction.Notes = request.Notes;
            if (request.IsRecurring.HasValue)
                existingTransaction.IsRecurring = request.IsRecurring.Value;
            if (request.RecurringFrequency.HasValue)
                existingTransaction.RecurringFrequency = request.RecurringFrequency.Value;
            if (request.RecurringEndDate.HasValue)
                existingTransaction.RecurringEndDate = request.RecurringEndDate.Value;

            return await _transactionRepository.UpdateAsync(existingTransaction);
        }

        public async Task<bool> DeleteTransactionAsync(string id, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            var transaction = await _transactionRepository.GetByUserIdAndIdAsync(userId, id);
            if (transaction == null)
            {
                return false;
            }

            return await _transactionRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync(Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _transactionRepository.GetRecurringByUserIdAsync(userId);
        }

        public async Task ProcessRecurringTransactionsAsync(Requestor requestor)
        {
            // Process recurring transactions for the specific user
            var userId = Guid.Parse(requestor.UserId);
            var recurringTransactions = await _transactionRepository.GetRecurringByUserIdAsync(userId);
            var today = DateTime.UtcNow.Date;

            foreach (var transaction in recurringTransactions)
            {
                if (ShouldCreateRecurringInstance(transaction, today))
                {
                    var newTransaction = _transactionFactory.CreateRecurringInstance(transaction, today);
                    await _transactionRepository.CreateAsync(newTransaction);
                    
                    _logger.LogInformation("Created recurring transaction instance {TransactionId} from original {OriginalTransactionId}", 
                        newTransaction.Id, transaction.Id);
                }
            }
        }

        private bool ShouldCreateRecurringInstance(Transaction transaction, DateTime today)
        {
            if (!transaction.IsRecurring || !transaction.RecurringFrequency.HasValue)
                return false;

            if (transaction.RecurringEndDate.HasValue && today > transaction.RecurringEndDate.Value)
                return false;

            var nextDueDate = CalculateNextRecurringDate(transaction.Date, transaction.RecurringFrequency.Value);
            return today >= nextDueDate;
        }

        private DateTime CalculateNextRecurringDate(DateTime lastDate, RecurringFrequency frequency)
        {
            return frequency switch
            {
                RecurringFrequency.WEEKLY => lastDate.AddDays(7),
                RecurringFrequency.MONTHLY => lastDate.AddMonths(1),
                RecurringFrequency.QUARTERLY => lastDate.AddMonths(3),
                RecurringFrequency.YEARLY => lastDate.AddYears(1),
                _ => throw new ArgumentException($"Unknown recurring frequency: {frequency}")
            };
        }

        private Transaction CreateRecurringInstance(Transaction originalTransaction, DateTime newDate)
        {
            return new Transaction
            {
                Amount = originalTransaction.Amount,
                Description = originalTransaction.Description,
                Date = newDate,
                Type = originalTransaction.Type,
                CategoryId = originalTransaction.CategoryId,
                Tags = new List<string>(originalTransaction.Tags),
                Notes = originalTransaction.Notes,
                IsRecurring = false,
                RecurringFrequency = null,
                RecurringEndDate = null
            };
        }
    }
}
