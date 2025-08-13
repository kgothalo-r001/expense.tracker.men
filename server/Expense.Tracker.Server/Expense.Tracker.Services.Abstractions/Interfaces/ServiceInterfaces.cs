using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(Requestor requestor);
        Task<Category?> GetCategoryByIdAsync(string id, Requestor requestor);
        Task<Category> CreateCategoryAsync(CreateCategoryRequest request, Requestor requestor);
        Task<Category?> UpdateCategoryAsync(UpdateCategoryRequest request, Requestor requestor);
        Task<bool> DeleteCategoryAsync(string id, Requestor requestor);
        Task<bool> CategoryExistsAsync(string id, Requestor requestor);
        Task InitializeDefaultCategoriesAsync(Requestor requestor);
    }

    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(Requestor requestor);
        Task<Transaction?> GetTransactionByIdAsync(string id, Requestor requestor);
        Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId, Requestor requestor);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, Requestor requestor);
        Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request, Requestor requestor);
        Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request, Requestor requestor);
        Task<bool> DeleteTransactionAsync(string id, Requestor requestor);
        Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync(Requestor requestor);
        Task ProcessRecurringTransactionsAsync(Requestor requestor);
    }

    public interface IDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(Requestor requestor, DateTime? startDate = null, DateTime? endDate = null);
        Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(Requestor requestor, int monthsBack = 12);
        Task<BudgetProjection> GetBudgetProjectionAsync(Requestor requestor);
    }

    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(string id, Requestor requestor);
        Task<Tag> CreateTagAsync(CreateTagRequest request, Requestor requestor);
        Task<bool> DeleteTagAsync(string id, Requestor requestor);
        Task UpdateTagUsageAsync(string tagName, Requestor requestor);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(Requestor requestor, int limit = 10);
    }

    public interface IAnalyticsService
    {
        Task<decimal> CalculateMonthlyAverageAsync(TransactionType type, int monthsBack = AnalyticsConstants.DefaultMonthsBackForAverage);
        Task<decimal> CalculateYearlyProjectionAsync(TransactionType type);
        Task<decimal> CalculateTrendAnalysisAsync(TransactionType type, int monthsBack = AnalyticsConstants.DefaultTrendMonths);
        Task<IEnumerable<MonthlySpending>> GetMonthlySpendingTrendsAsync(Requestor requestor, int monthsBack = AnalyticsConstants.DefaultTrendMonths);
        Task<IEnumerable<CategoryTrend>> GetCategoryTrendsAsync(Requestor requestor);
        Task<BudgetProjection> GenerateBudgetProjectionAsync(Requestor requestor);
    }

    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<T> CreateAsync(T entity);
        Task<T?> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetByTypeAsync(CategoryType type);
        Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Category>> GetByUserIdAndTypeAsync(Guid userId, CategoryType type);
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> GetByUserIdAndNameAsync(Guid userId, string name);
        Task<Category?> GetByUserIdAndIdAsync(Guid userId, string id);
    }

    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(string categoryId);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string? userId);
        Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
        Task<IEnumerable<Transaction>> GetRecurringAsync();
        Task<decimal> GetTotalAmountByTypeAsync(TransactionType type, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Transaction>> GetRecentAsync(int limit);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
        Task<Transaction?> GetByUserIdAndIdAsync(Guid userId, string id);
        Task<IEnumerable<Transaction>> GetByUserIdAndCategoryIdAsync(Guid userId, string categoryId);
        Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetRecurringByUserIdAsync(Guid userId);
        Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int limit);
    }

    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(LoginRequest request);
        Task<AuthenticationResult> RegisterAsync(RegisterRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserByTokenAsync(string token);
        Task<bool> LogoutAsync(string token);
        Task<AuthenticationResult> RefreshTokenAsync(string token);
        Task<List<string>> SuggestUsernamesAsync(string baseUsername);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
    }

    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularAsync(int limit);
        Task IncrementUsageAsync(string tagName);
    }
}
