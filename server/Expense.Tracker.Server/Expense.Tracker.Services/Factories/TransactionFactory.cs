using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Services.Factories
{
    public class TransactionFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(CreateTransactionRequest request, string userId)
        {
            return new Transaction
            {
                UserId = userId,
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
        }

        public Transaction CreateRecurringInstance(Transaction originalTransaction, DateTime newDate)
        {
            return new Transaction
            {
                UserId = originalTransaction.UserId,
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
