using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthenticatedUserHelper _userHelper;
        private readonly ICalculationStrategyFactory _calculationStrategyFactory;

        public AnalyticsService(
            ITransactionRepository transactionRepository, 
            ICategoryRepository categoryRepository, 
            IAuthenticatedUserHelper userHelper,
            ICalculationStrategyFactory calculationStrategyFactory)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _userHelper = userHelper ?? throw new ArgumentNullException(nameof(userHelper));
            _calculationStrategyFactory = calculationStrategyFactory ?? throw new ArgumentNullException(nameof(calculationStrategyFactory));
        }

        public async Task<decimal> CalculateMonthlyAverageAsync(TransactionType type, int monthsBack = AnalyticsConstants.DefaultMonthsBackForAverage)
        {
            var strategy = _calculationStrategyFactory.GetStrategy(CalculationStrategyType.MonthlyAverage);
            return await strategy.CalculateAsync(_transactionRepository, type, monthsBack);
        }

        public async Task<decimal> CalculateYearlyProjectionAsync(TransactionType type)
        {
            var strategy = _calculationStrategyFactory.GetStrategy(CalculationStrategyType.YearlyProjection);
            return await strategy.CalculateAsync(_transactionRepository, type, AnalyticsConstants.DefaultMonthsBackForAverage);
        }

        public async Task<decimal> CalculateTrendAnalysisAsync(TransactionType type, int monthsBack = AnalyticsConstants.DefaultTrendMonths)
        {
            var strategy = _calculationStrategyFactory.GetStrategy(CalculationStrategyType.TrendAnalysis);
            return await strategy.CalculateAsync(_transactionRepository, type, monthsBack);
        }

        public async Task<IEnumerable<MonthlySpending>> GetMonthlySpendingTrendsAsync(int monthsBack = AnalyticsConstants.DefaultTrendMonths)
        {
            AnalyticsHelpers.ValidateMonthsBack(monthsBack);

            try
            {
                var monthlySpendingList = new List<MonthlySpending>();
                var currentDate = DateTime.UtcNow.Date;

                // Create tasks for concurrent execution
                var monthlyTasks = new List<Task<MonthlySpending>>();

                for (int i = 0; i < monthsBack; i++)
                {
                    var monthIndex = i; // Capture for closure
                    var task = Task.Run(async () =>
                    {
                        var targetDate = currentDate.AddMonths(-monthIndex);
                        var (monthStart, monthEnd) = AnalyticsHelpers.GetMonthDateRange(targetDate);

                        var filteredTransactions = await _transactionRepository.GetByDateRangeAsync(monthStart, monthEnd);
                        var expenseTransactions = filteredTransactions.Where(t => t.Type == TransactionType.EXPENSE);

                        return new MonthlySpending
                        {
                            Month = monthStart.ToString("yyyy-MM"),
                            Amount = expenseTransactions.Sum(t => t.Amount),
                            TransactionCount = expenseTransactions.Count()
                        };
                    });
                    monthlyTasks.Add(task);
                }

                var monthlyResults = await Task.WhenAll(monthlyTasks);
                return monthlyResults.OrderBy(ms => ms.Month);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving monthly spending trends", ex);
            }
        }

        public async Task<IEnumerable<CategoryTrend>> GetCategoryTrendsAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var (currentMonthStart, currentMonthEnd) = AnalyticsHelpers.GetMonthDateRange(currentDate);
                var (previousMonthStart, previousMonthEnd) = AnalyticsHelpers.GetMonthDateRange(currentDate.AddMonths(-1));
                
                var categories = await _categoryRepository.GetAllAsync();

                // Get all transactions for both months concurrently
                var currentMonthTransactionsTask = _transactionRepository.GetByDateRangeAsync(currentMonthStart, currentMonthEnd);
                var previousMonthTransactionsTask = _transactionRepository.GetByDateRangeAsync(previousMonthStart, previousMonthEnd);

                await Task.WhenAll(currentMonthTransactionsTask, previousMonthTransactionsTask);

                var currentMonthTransactions = currentMonthTransactionsTask.Result;
                var previousMonthTransactions = previousMonthTransactionsTask.Result;

                var categoryTrends = new List<CategoryTrend>();

                foreach (var category in categories)
                {
                    var currentAmount = currentMonthTransactions
                        .Where(t => t.CategoryId == category.Id)
                        .Sum(t => t.Amount);

                    var previousAmount = previousMonthTransactions
                        .Where(t => t.CategoryId == category.Id)
                        .Sum(t => t.Amount);

                    // Calculate percentage change with proper handling of zero previous amount
                    var percentageChange = previousAmount == 0 ? 
                        (currentAmount > 0 ? decimal.MaxValue : 0) : 
                        ((currentAmount - previousAmount) / previousAmount) * 100;

                    categoryTrends.Add(new CategoryTrend
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        CurrentMonthAmount = currentAmount,
                        PreviousMonthAmount = previousAmount,
                        PercentageChange = percentageChange
                    });
                }

                return categoryTrends.OrderByDescending(ct => ct.CurrentMonthAmount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving category trends", ex);
            }
        }

        public async Task<BudgetProjection> GenerateBudgetProjectionAsync()
        {
            var monthlyExpenseAverage = await CalculateMonthlyAverageAsync(TransactionType.EXPENSE, AnalyticsConstants.DefaultMonthsBackForAverage);
            var monthlyIncomeAverage = await CalculateMonthlyAverageAsync(TransactionType.INCOME, AnalyticsConstants.DefaultMonthsBackForAverage);
            var yearlyExpenseProjection = monthlyExpenseAverage * AnalyticsConstants.MonthsInYear;

            var categories = await _categoryRepository.GetAllAsync();
            var categoryProjections = new List<CategoryProjection>();

            foreach (var category in categories)
            {
                var startDate = DateTime.UtcNow.AddMonths(-AnalyticsConstants.DefaultMonthsBackForAverage);
                var transactions = await _transactionRepository.GetByCategoryIdAsync(category.Id);
                var recentTransactions = transactions.Where(t => t.Date >= startDate);
                
                var averageMonthly = recentTransactions.Sum(t => t.Amount) / AnalyticsConstants.DefaultMonthsBackForAverage;
                var projectedYearly = averageMonthly * AnalyticsConstants.MonthsInYear;
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
