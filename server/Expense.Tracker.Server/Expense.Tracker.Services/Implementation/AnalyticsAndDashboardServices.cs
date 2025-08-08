using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Implementation
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public AnalyticsService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUserService)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
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

    public class DashboardService : IDashboardService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAnalyticsService _analyticsService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IAnalyticsService analyticsService,
            ICurrentUserService currentUserService)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _analyticsService = analyticsService;
            _currentUserService = currentUserService;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            startDate ??= DateTime.UtcNow.AddMonths(-1).Date;
            endDate ??= DateTime.UtcNow.Date;

            var transactions = await _transactionRepository.GetByUserIdAndDateRangeAsync(currentUserId.Value, startDate.Value, endDate.Value);
            var categories = await _categoryRepository.GetByUserIdAsync(currentUserId.Value);

            var totalIncome = transactions.Where(t => t.Type == TransactionType.INCOME).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.EXPENSE).Sum(t => t.Amount);

            var topCategories = await GetTopCategoriesAsync(transactions, categories);
            var recentTransactions = await _transactionRepository.GetRecentByUserIdAsync(currentUserId.Value, BusinessConstants.RecentTransactionsLimit);

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
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own dashboard
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own dashboard.");
            }

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
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own analytics
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own expense analytics.");
            }

            // For now, delegate to the existing method since AnalyticsService needs more work
            // The AnalyticsService methods should be updated to use user-specific data in the future
            return await GetExpenseAnalyticsAsync(monthsBack);
        }

        public async Task<BudgetProjection> GetUserBudgetProjectionAsync(Guid userId)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own budget projection
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own budget projection.");
            }

            // For now, delegate to the existing method since AnalyticsService needs more work
            // The AnalyticsService methods should be updated to use user-specific data in the future
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

    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ICurrentUserService _currentUserService;

        public TagService(ITagRepository tagRepository, ICurrentUserService currentUserService)
        {
            _tagRepository = tagRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userId)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own tags
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own tags.");
            }

            // For now, tags are global - but this method ensures proper authorization
            // In a future version, tags could be user-specific
            return await _tagRepository.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(string id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag?> GetUserTagByIdAsync(string id, Guid userId)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own tags
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own tags.");
            }

            // For now, tags are global - but this method ensures proper authorization
            // In a future version, tags could be user-specific
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag> CreateTagAsync(CreateTagRequest request)
        {
            var existingTag = await _tagRepository.GetByNameAsync(request.Name);
            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag with name '{request.Name}' already exists.");
            }

            var tag = new Tag
            {
                Name = request.Name,
                Color = request.Color,
                UsageCount = 0
            };

            return await _tagRepository.CreateAsync(tag);
        }

        public async Task<bool> DeleteTagAsync(string id)
        {
            return await _tagRepository.DeleteAsync(id);
        }

        public async Task UpdateTagUsageAsync(string tagName)
        {
            await _tagRepository.IncrementUsageAsync(tagName);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10)
        {
            return await _tagRepository.GetPopularAsync(limit);
        }

        public async Task<IEnumerable<Tag>> GetUserPopularTagsAsync(Guid userId, int limit = 10)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Ensure the current user can only access their own popular tags
            if (currentUserId.Value != userId)
            {
                throw new UnauthorizedAccessException("You can only access your own popular tags.");
            }

            // For now, popular tags are global - but this method ensures proper authorization
            // In a future version, popular tags could be user-specific based on their transaction usage
            return await _tagRepository.GetPopularAsync(limit);
        }
    }
}
