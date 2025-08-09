using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAnalyticsService _analyticsService;

        public DashboardService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IAnalyticsService analyticsService)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _analyticsService = analyticsService;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(Requestor requestor, DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUserId = Guid.Parse(requestor.UserId);

            startDate ??= DateTime.UtcNow.AddMonths(-1).Date;
            endDate ??= DateTime.UtcNow.Date;

            var transactions = await _transactionRepository.GetByUserIdAndDateRangeAsync(currentUserId, startDate.Value, endDate.Value);
            var categories = await _categoryRepository.GetByUserIdAsync(currentUserId);

            var totalIncome = transactions.Where(t => t.Type == TransactionType.INCOME).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.EXPENSE).Sum(t => t.Amount);

            var topCategories = await GetTopCategoriesAsync(transactions, categories);
            var recentTransactions = await _transactionRepository.GetRecentByUserIdAsync(currentUserId, BusinessConstants.RecentTransactionsLimit);

            return new DashboardSummary
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                TransactionCount = transactions.Count(),
                TopCategories = topCategories.ToList(),
                RecentTransactions = recentTransactions.ToList()
            };
        }

        public async Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(Requestor requestor, int monthsBack = 12)
        {
            var monthlyAverage = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, requestor, monthsBack);
            var yearlyProjection = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE, requestor);
            var monthlyTrends = await _analyticsService.GetMonthlySpendingTrendsAsync(requestor, monthsBack);
            var categoryTrends = await _analyticsService.GetCategoryTrendsAsync(requestor);

            return new ExpenseAnalytics
            {
                MonthlyAverage = monthlyAverage,
                YearlyProjection = yearlyProjection,
                MonthlySpendingTrends = monthlyTrends.ToList(),
                CategoryTrends = categoryTrends.ToList()
            };
        }

        public async Task<BudgetProjection> GetBudgetProjectionAsync(Requestor requestor)
        {
            return await _analyticsService.GenerateBudgetProjectionAsync(requestor);
        }

        private Task<IEnumerable<CategorySummary>> GetTopCategoriesAsync(
            IEnumerable<Transaction> transactions, 
            IEnumerable<Category> categories)
        {
            var categoryGroups = transactions
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.TotalAmount)
                .Take(BusinessConstants.TopCategoriesLimit);

            var totalAmount = transactions.Sum(t => t.Amount);

            var result = new List<CategorySummary>();
            foreach (var group in categoryGroups)
            {
                var category = categories.FirstOrDefault(c => c.Id == group.CategoryId);
                if (category != null)
                {
                    result.Add(new CategorySummary
                    {
                        CategoryId = group.CategoryId,
                        CategoryName = category.Name,
                        TotalAmount = group.TotalAmount,
                        TransactionCount = group.Count,
                        Percentage = totalAmount > 0 ? (group.TotalAmount / totalAmount) * 100 : 0
                    });
                }
            }

            return Task.FromResult<IEnumerable<CategorySummary>>(result);
        }
    }
}
