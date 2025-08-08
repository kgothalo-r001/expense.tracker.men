using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Helpers
{
    public static class AnalyticsHelpers
    {
        /// <summary>
        /// Gets the start and end dates for a specific month
        /// </summary>
        /// <param name="date">The date within the month to calculate range for</param>
        /// <returns>A tuple containing the first and last day of the month</returns>
        public static (DateTime Start, DateTime End) GetMonthDateRange(DateTime date)
        {
            var start = new DateTime(date.Year, date.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }

        /// <summary>
        /// Validates that monthsBack parameter is within acceptable range
        /// </summary>
        /// <param name="monthsBack">Number of months to look back</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when monthsBack is less than or equal to 0</exception>
        public static void ValidateMonthsBack(int monthsBack)
        {
            if (monthsBack <= 0)
                throw new ArgumentOutOfRangeException(nameof(monthsBack), "Months back must be greater than 0");
        }

        /// <summary>
        /// Validates transaction type parameter
        /// </summary>
        /// <param name="type">Transaction type to validate</param>
        /// <exception cref="ArgumentException">Thrown when transaction type is invalid</exception>
        public static void ValidateTransactionType(TransactionType type)
        {
            if (!Enum.IsDefined(typeof(TransactionType), type))
                throw new ArgumentException($"Invalid transaction type: {type}", nameof(type));
        }
    }
}
