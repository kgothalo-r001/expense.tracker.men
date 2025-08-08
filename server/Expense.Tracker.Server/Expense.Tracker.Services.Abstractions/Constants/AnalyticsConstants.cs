namespace Expense.Tracker.Services.Abstractions.Constants
{
    public static class AnalyticsConstants
    {
        /// <summary>
        /// Default number of months to look back for calculating averages
        /// </summary>
        public const int DefaultMonthsBackForAverage = 6;

        /// <summary>
        /// Number of months in a year for projection calculations
        /// </summary>
        public const int MonthsInYear = 12;

        /// <summary>
        /// Default number of months to analyze for trend calculations
        /// </summary>
        public const int DefaultTrendMonths = 12;
    }
}
