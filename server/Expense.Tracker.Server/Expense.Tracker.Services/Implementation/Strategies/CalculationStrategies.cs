using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation.Strategies
{
    /// <summary>
    /// Strategy for calculating monthly averages
    /// </summary>
    public class MonthlyAverageCalculationStrategy : ICalculationStrategy
    {
        public async Task<decimal> CalculateAsync(ITransactionRepository transactionRepository, TransactionType type, int monthsBack)
        {
            AnalyticsHelpers.ValidateTransactionType(type);
            AnalyticsHelpers.ValidateMonthsBack(monthsBack);

            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-monthsBack).Date;
                var endDate = DateTime.UtcNow.Date;

                var totalTransactionAmount = await transactionRepository.GetTotalAmountByTypeAsync(type, startDate, endDate);
                return totalTransactionAmount / monthsBack;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating monthly average for transaction type {type}", ex);
            }
        }
    }

    /// <summary>
    /// Strategy for calculating yearly projections
    /// </summary>
    public class YearlyProjectionCalculationStrategy : ICalculationStrategy
    {
        private readonly ICalculationStrategy _monthlyAverageStrategy;

        public YearlyProjectionCalculationStrategy(ICalculationStrategy monthlyAverageStrategy)
        {
            _monthlyAverageStrategy = monthlyAverageStrategy ?? throw new ArgumentNullException(nameof(monthlyAverageStrategy));
        }

        public async Task<decimal> CalculateAsync(ITransactionRepository transactionRepository, TransactionType type, int monthsBack)
        {
            var monthlyAverage = await _monthlyAverageStrategy.CalculateAsync(transactionRepository, type, monthsBack);
            return monthlyAverage * AnalyticsConstants.MonthsInYear;
        }
    }

    /// <summary>
    /// Strategy for trend analysis calculations
    /// </summary>
    public class TrendAnalysisCalculationStrategy : ICalculationStrategy
    {
        public async Task<decimal> CalculateAsync(ITransactionRepository transactionRepository, TransactionType type, int monthsBack)
        {
            AnalyticsHelpers.ValidateTransactionType(type);
            AnalyticsHelpers.ValidateMonthsBack(monthsBack);

            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var totalTrend = 0m;

                // Calculate trend by comparing each month with the previous month
                for (int i = 1; i < monthsBack; i++)
                {
                    var currentMonth = currentDate.AddMonths(-i);
                    var previousMonth = currentDate.AddMonths(-(i + 1));
                    
                    var (currentStart, currentEnd) = AnalyticsHelpers.GetMonthDateRange(currentMonth);
                    var (previousStart, previousEnd) = AnalyticsHelpers.GetMonthDateRange(previousMonth);

                    var currentAmount = await transactionRepository.GetTotalAmountByTypeAsync(type, currentStart, currentEnd);
                    var previousAmount = await transactionRepository.GetTotalAmountByTypeAsync(type, previousStart, previousEnd);

                    if (previousAmount > 0)
                    {
                        var monthlyTrend = ((currentAmount - previousAmount) / previousAmount) * 100;
                        totalTrend += monthlyTrend;
                    }
                }

                return monthsBack > 1 ? totalTrend / (monthsBack - 1) : 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating trend analysis for transaction type {type}", ex);
            }
        }
    }
}
