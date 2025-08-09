using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Exceptions;

namespace Expense.Tracker.Services.Validators
{
    public class TransactionValidator : ITransactionValidationService
    {
        private readonly ICategoryRepository _categoryRepository;

        public TransactionValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task ValidateCategoryExistsAsync(Guid userId, string categoryId)
        {
            var category = await _categoryRepository.GetByUserIdAndIdAsync(userId, categoryId);
            if (category == null)
            {
                throw new CategoryNotFoundException(categoryId);
            }
        }

        public void ValidateRecurringTransactionSettings(CreateTransactionRequest request)
        {
            ValidateRecurringTransactionSettings(
                request.IsRecurring, 
                request.RecurringFrequency, 
                request.RecurringEndDate, 
                request.Date);
        }

        public void ValidateRecurringTransactionSettings(bool isRecurring, RecurringFrequency? frequency, DateTime? endDate, DateTime transactionDate)
        {
            if (!isRecurring) return;

            if (!frequency.HasValue)
            {
                throw new InvalidRecurringTransactionException(ErrorMessages.RecurringFrequencyRequired);
            }

            if (endDate.HasValue && endDate <= transactionDate)
            {
                throw new InvalidRecurringTransactionException(ErrorMessages.RecurringEndDateInvalid);
            }
        }
    }
}
