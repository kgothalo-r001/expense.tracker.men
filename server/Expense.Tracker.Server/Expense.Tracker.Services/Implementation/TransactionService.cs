using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IAuthenticatedUserHelper _userHelper;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            ITagRepository tagRepository,
            IAuthenticatedUserHelper userHelper)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _userHelper = userHelper;
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
            var category = await _categoryRepository.GetByUserIdAndIdAsync(currentUserId, categoryId);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{categoryId}' does not exist or does not belong to the current user.");
            }

            return await _transactionRepository.GetByUserIdAndCategoryIdAsync(currentUserId, categoryId);
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsByCategoryAsync(string categoryId, Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);

            // Validate that the category belongs to the user
            var category = await _categoryRepository.GetByUserIdAndIdAsync(userId, categoryId);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{categoryId}' does not exist or does not belong to the user.");
            }

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
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

            // Validate category exists and belongs to the current user
            var category = await _categoryRepository.GetByUserIdAndIdAsync(currentUserId, request.CategoryId);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist or does not belong to the current user.");
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
                UserId = currentUserId.ToString(),
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
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

            var existingTransaction = await _transactionRepository.GetByUserIdAndIdAsync(currentUserId, request.Id);
            if (existingTransaction == null)
            {
                return null;
            }

            // Validate category if provided and ensure it belongs to the current user
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                var category = await _categoryRepository.GetByUserIdAndIdAsync(currentUserId, request.CategoryId);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist or does not belong to the current user.");
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
            // For now, we'll allow it to work with all recurring transactions
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
