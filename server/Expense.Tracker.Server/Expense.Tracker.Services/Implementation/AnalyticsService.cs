using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthenticatedUserHelper _userHelper;

        public AnalyticsService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IAuthenticatedUserHelper userHelper)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _userHelper = userHelper;
        }

        public async Task<decimal> CalculateMonthlyAverageAsync(TransactionType type, int monthsBack = 6)
        {
            var startDate = DateTime.UtcNow.AddMonths(-monthsBack).Date;
            var endDate = DateTime.UtcNow.Date;

            var total = await _transactionRepository.GetTotalAmountByTypeAsync(type, startDate, endDate);
            return total / monthsBack;
        }

        public async Task<decimal> CalculateYearlyProjectionAsync(TransactionType type)
        {
            var monthlyAverage = await CalculateMonthlyAverageAsync(type, 6);
            return monthlyAverage * 12;
        }

        public async Task<IEnumerable<MonthlySpending>> GetMonthlySpendingTrendsAsync(int monthsBack = 12)
        {
            var results = new List<MonthlySpending>();
            var currentDate = DateTime.UtcNow.Date;

            for (int i = 0; i < monthsBack; i++)
            {
                var monthStart = currentDate.AddMonths(-i).AddDays(1 - currentDate.AddMonths(-i).Day);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var transactions = await _transactionRepository.GetByDateRangeAsync(monthStart, monthEnd);
                var expenses = transactions.Where(t => t.Type == TransactionType.EXPENSE);

                results.Add(new MonthlySpending
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    Amount = expenses.Sum(t => t.Amount),
                    TransactionCount = expenses.Count()
                });
            }

            return results.OrderBy(ms => ms.Month);
        }

        public async Task<IEnumerable<CategoryTrend>> GetCategoryTrendsAsync()
        {
            var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
            var previousMonth = currentMonth.AddMonths(-1);
            var categories = await _categoryRepository.GetAllAsync();

            var trends = new List<CategoryTrend>();

            foreach (var category in categories)
            {
                var currentMonthTransactions = await _transactionRepository.GetByDateRangeAsync(
                    currentMonth, currentMonth.AddMonths(1).AddDays(-1));
                var previousMonthTransactions = await _transactionRepository.GetByDateRangeAsync(
                    previousMonth, previousMonth.AddMonths(1).AddDays(-1));

                var currentAmount = currentMonthTransactions
                    .Where(t => t.CategoryId == category.Id)
                    .Sum(t => t.Amount);

                var previousAmount = previousMonthTransactions
                    .Where(t => t.CategoryId == category.Id)
                    .Sum(t => t.Amount);

                var percentageChange = previousAmount == 0 ? 0 : 
                    ((currentAmount - previousAmount) / previousAmount) * 100;

                trends.Add(new CategoryTrend
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CurrentMonthAmount = currentAmount,
                    PreviousMonthAmount = previousAmount,
                    PercentageChange = percentageChange
                });
            }

            return trends.OrderByDescending(ct => ct.CurrentMonthAmount);
        }

        public async Task<BudgetProjection> GenerateBudgetProjectionAsync()
        {
            var monthlyExpenseAverage = await CalculateMonthlyAverageAsync(TransactionType.EXPENSE, 6);
            var monthlyIncomeAverage = await CalculateMonthlyAverageAsync(TransactionType.INCOME, 6);
            var yearlyExpenseProjection = monthlyExpenseAverage * 12;

            var categories = await _categoryRepository.GetAllAsync();
            var categoryProjections = new List<CategoryProjection>();

            foreach (var category in categories)
            {
                var startDate = DateTime.UtcNow.AddMonths(-6);
                var transactions = await _transactionRepository.GetByCategoryIdAsync(category.Id);
                var recentTransactions = transactions.Where(t => t.Date >= startDate);
                
                var averageMonthly = recentTransactions.Sum(t => t.Amount) / 6;
                var projectedYearly = averageMonthly * 12;
                var recommendedBudget = averageMonthly * (1 + BusinessConstants.DefaultBudgetBuffer);

                categoryProjections.Add(new CategoryProjection
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    AverageMonthlySpending = averageMonthly,
                    ProjectedYearlySpending = projectedYearly,
                    RecommendedBudget = recommendedBudget
                });
            }

            return new BudgetProjection
            {
                ProjectedMonthlyExpenses = monthlyExpenseAverage,
                ProjectedYearlyExpenses = yearlyExpenseProjection,
                RecommendedMonthlySavings = Math.Max(0, monthlyIncomeAverage - monthlyExpenseAverage),
                CategoryProjections = categoryProjections.OrderByDescending(cp => cp.AverageMonthlySpending).ToList()
            };
        }
    }
}
