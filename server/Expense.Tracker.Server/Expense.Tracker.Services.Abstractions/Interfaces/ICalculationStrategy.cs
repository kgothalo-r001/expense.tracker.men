using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    /// <summary>
    /// Strategy interface for different calculation types
    /// </summary>
    public interface ICalculationStrategy
    {
        /// <summary>
        /// Calculates a value based on the strategy implementation
        /// </summary>
        /// <param name="transactionRepository">Repository for transaction data</param>
        /// <param name="type">Transaction type to calculate for</param>
        /// <param name="monthsBack">Number of months to look back</param>
        /// <returns>Calculated value</returns>
        Task<decimal> CalculateAsync(ITransactionRepository transactionRepository, TransactionType type, int monthsBack);
    }
}
