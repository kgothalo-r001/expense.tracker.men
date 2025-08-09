using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(CreateTransactionRequest request, string userId);
        Transaction CreateRecurringInstance(Transaction originalTransaction, DateTime newDate);
    }
}
