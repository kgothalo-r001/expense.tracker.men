using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Helpers;
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
        private readonly IAuthenticatedUserHelper _userHelper;
        private readonly ITransactionValidationService _transactionValidator;
        private readonly ITransactionFactory _transactionFactory;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            ITagRepository tagRepository,
            IAuthenticatedUserHelper userHelper,
            ITransactionValidationService transactionValidator,
            ITransactionFactory transactionFactory,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _userHelper = userHelper;
            _transactionValidator = transactionValidator;
            _transactionFactory = transactionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();
            return await _transactionRepository.GetByUserIdAsync(currentUserId);
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);
            return await _transactionRepository.GetByUserIdAsync(userId);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(string id)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();
            return await _transactionRepository.GetByUserIdAndIdAsync(currentUserId, id);
        }

        public async Task<Transaction?> GetUserTransactionByIdAsync(string id, Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);
            return await _transactionRepository.GetByUserIdAndIdAsync(userId, id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

            // Validate that the category belongs to the current user
            await _transactionValidator.ValidateCategoryExistsAsync(currentUserId, categoryId);

            return await _transactionRepository.GetByUserIdAndCategoryIdAsync(currentUserId, categoryId);
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsByCategoryAsync(string categoryId, Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);

            // Validate that the category belongs to the user
            await _transactionValidator.ValidateCategoryExistsAsync(userId, categoryId);

            return await _transactionRepository.GetByUserIdAndCategoryIdAsync(userId, categoryId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();
            return await _transactionRepository.GetByUserIdAndDateRangeAsync(currentUserId, startDate, endDate);
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);
            return await _transactionRepository.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
        }

        public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request)
        {
            try
            {
                var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

                // Validate category exists and belongs to the current user
                await _transactionValidator.ValidateCategoryExistsAsync(currentUserId, request.CategoryId);

                // Validate recurring transaction settings
                _transactionValidator.ValidateRecurringTransactionSettings(request);

                var transaction = _transactionFactory.CreateTransaction(request, currentUserId.ToString());

                var createdTransaction = await _transactionRepository.CreateAsync(transaction);

                // Update tag usage counts
                if (transaction.Tags?.Count > 0)
                {
                    var tagTasks = transaction.Tags.Select(tagName => _tagRepository.IncrementUsageAsync(tagName));
                    await Task.WhenAll(tagTasks);
                }

                _logger.LogInformation("Created transaction {TransactionId} for user {UserId}", 
                    createdTransaction.Id, currentUserId);

                return createdTransaction;
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning("Category validation failed for user {UserId}: {Error}", 
                    await _userHelper.GetAuthenticatedUserIdAsync(), ex.Message);
                throw;
            }
            catch (InvalidRecurringTransactionException ex)
            {
                _logger.LogWarning("Recurring transaction validation failed: {Error}", ex.Message);
                throw;
            }
        }

        public async Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

            var existingTransaction = await _transactionRepository.GetByUserIdAndIdAsync(currentUserId, request.Id);
            if (existingTransaction == null)
            {
                return null;
            }

            // Validate category if provided and ensure it belongs to the current user
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                await _transactionValidator.ValidateCategoryExistsAsync(currentUserId, request.CategoryId);
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

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

            var transaction = await _transactionRepository.GetByUserIdAndIdAsync(currentUserId, id);
            if (transaction == null)
            {
                return false;
            }

            return await _transactionRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync()
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();
            return await _transactionRepository.GetRecurringByUserIdAsync(currentUserId);
        }

        public async Task<IEnumerable<Transaction>> GetUserRecurringTransactionsAsync(Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);
            return await _transactionRepository.GetRecurringByUserIdAsync(userId);
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            // This method processes ALL users' recurring transactions
            // It should typically be called by a background service or system process
            var recurringTransactions = await _transactionRepository.GetRecurringAsync();
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
