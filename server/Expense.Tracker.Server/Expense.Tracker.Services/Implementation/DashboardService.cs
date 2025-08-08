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
        private readonly IAuthenticatedUserHelper _userHelper;

        public DashboardService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IAnalyticsService analyticsService,
            IAuthenticatedUserHelper userHelper)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _analyticsService = analyticsService;
            _userHelper = userHelper;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUserId = await _userHelper.GetAuthenticatedUserIdAsync();

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

        public async Task<DashboardSummary> GetUserDashboardSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            _userHelper.ValidateUserAccess(userId);

            startDate ??= DateTime.UtcNow.AddMonths(-1).Date;
            endDate ??= DateTime.UtcNow.Date;

            var transactions = await _transactionRepository.GetByUserIdAndDateRangeAsync(userId, startDate.Value, endDate.Value);
            var categories = await _categoryRepository.GetByUserIdAsync(userId);

            var totalIncome = transactions.Where(t => t.Type == TransactionType.INCOME).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.EXPENSE).Sum(t => t.Amount);

            var topCategories = await GetTopCategoriesAsync(transactions, categories);
            var recentTransactions = await _transactionRepository.GetRecentByUserIdAsync(userId, BusinessConstants.RecentTransactionsLimit);

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

        public async Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(int monthsBack = 12)
        {
            var monthlyAverage = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, monthsBack);
            var yearlyProjection = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE);
            var monthlyTrends = await _analyticsService.GetMonthlySpendingTrendsAsync(monthsBack);
            var categoryTrends = await _analyticsService.GetCategoryTrendsAsync();

            return new ExpenseAnalytics
            {
                MonthlyAverage = monthlyAverage,
                YearlyProjection = yearlyProjection,
                MonthlySpendingTrends = monthlyTrends.ToList(),
                CategoryTrends = categoryTrends.ToList()
            };
        }

        public async Task<BudgetProjection> GetBudgetProjectionAsync()
        {
            return await _analyticsService.GenerateBudgetProjectionAsync();
        }

        public async Task<ExpenseAnalytics> GetUserExpenseAnalyticsAsync(Guid userId, int monthsBack = 12)
        {
            _userHelper.ValidateUserAccess(userId);

            return await GetExpenseAnalyticsAsync(monthsBack);
        }

        public async Task<BudgetProjection> GetUserBudgetProjectionAsync(Guid userId)
        {
            _userHelper.ValidateUserAccess(userId);

            return await GetBudgetProjectionAsync();
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
