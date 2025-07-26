using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            ITagRepository tagRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.GetAllAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(string id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId)
        {
            return await _transactionRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request)
        {
            // Validate category exists
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist.");
            }

            // Validate recurring transaction settings
            if (request.IsRecurring)
            {
                if (!request.RecurringFrequency.HasValue)
                {
                    throw new InvalidOperationException("Recurring frequency is required for recurring transactions.");
                }

                if (request.RecurringEndDate.HasValue && request.RecurringEndDate <= request.Date)
                {
                    throw new InvalidOperationException("Recurring end date must be after the transaction date.");
                }
            }

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Description = request.Description,
                Date = request.Date,
                Type = request.Type,
                CategoryId = request.CategoryId,
                Tags = request.Tags ?? new List<string>(),
                Notes = request.Notes,
                IsRecurring = request.IsRecurring,
                RecurringFrequency = request.RecurringFrequency,
                RecurringEndDate = request.RecurringEndDate
            };

            var createdTransaction = await _transactionRepository.CreateAsync(transaction);

            // Update tag usage counts
            foreach (var tagName in transaction.Tags)
            {
                await _tagRepository.IncrementUsageAsync(tagName);
            }

            return createdTransaction;
        }

        public async Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(request.Id);
            if (existingTransaction == null)
            {
                return null;
            }

            // Validate category if provided
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist.");
                }
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
                existingTransaction.Tags = request.Tags;
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
            return await _transactionRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync()
        {
            return await _transactionRepository.GetRecurringAsync();
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            var recurringTransactions = await _transactionRepository.GetRecurringAsync();
            var today = DateTime.UtcNow.Date;

            foreach (var transaction in recurringTransactions)
            {
                if (ShouldCreateRecurringInstance(transaction, today))
                {
                    var newTransaction = CreateRecurringInstance(transaction, today);
                    await _transactionRepository.CreateAsync(newTransaction);
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
                IsRecurring = false, // Individual instances are not recurring
                RecurringFrequency = null,
                RecurringEndDate = null
            };
        }
    }
}
