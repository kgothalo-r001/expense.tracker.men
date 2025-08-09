using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface ITransactionValidationService
    {
        Task ValidateCategoryExistsAsync(Guid userId, string categoryId);
        void ValidateRecurringTransactionSettings(CreateTransactionRequest request);
        void ValidateRecurringTransactionSettings(bool isRecurring, RecurringFrequency? frequency, DateTime? endDate, DateTime transactionDate);
    }
}
