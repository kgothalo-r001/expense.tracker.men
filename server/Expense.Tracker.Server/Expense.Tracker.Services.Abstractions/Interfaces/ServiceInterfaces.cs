using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetUserCategoriesAsync(Guid userId);
        Task<Category?> GetCategoryByIdAsync(string id);
        Task<Category?> GetUserCategoryByIdAsync(string id, Guid userId);
        Task<Category> CreateCategoryAsync(CreateCategoryRequest request);
        Task<Category?> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(string id);
        Task<bool> CategoryExistsAsync(string id);
        Task InitializeDefaultCategoriesAsync();
    }

    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(Guid userId);
        Task<Transaction?> GetTransactionByIdAsync(string id);
        Task<Transaction?> GetUserTransactionByIdAsync(string id, Guid userId);
        Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId);
        Task<IEnumerable<Transaction>> GetUserTransactionsByCategoryAsync(string categoryId, Guid userId);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetUserTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid userId);
        Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request);
        Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request);
        Task<bool> DeleteTransactionAsync(string id);
        Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync();
        Task<IEnumerable<Transaction>> GetUserRecurringTransactionsAsync(Guid userId);
        Task ProcessRecurringTransactionsAsync();
    }

    public interface IDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<DashboardSummary> GetUserDashboardSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(int monthsBack = 12);
        Task<ExpenseAnalytics> GetUserExpenseAnalyticsAsync(Guid userId, int monthsBack = 12);
        Task<BudgetProjection> GetBudgetProjectionAsync();
        Task<BudgetProjection> GetUserBudgetProjectionAsync(Guid userId);
    }

    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<IEnumerable<Tag>> GetUserTagsAsync(Guid userId);
        Task<Tag?> GetTagByIdAsync(string id);
        Task<Tag?> GetUserTagByIdAsync(string id, Guid userId);
        Task<Tag> CreateTagAsync(CreateTagRequest request);
        Task<bool> DeleteTagAsync(string id);
        Task UpdateTagUsageAsync(string tagName);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10);
        Task<IEnumerable<Tag>> GetUserPopularTagsAsync(Guid userId, int limit = 10);
    }

    public interface IAnalyticsService
    {
        Task<decimal> CalculateMonthlyAverageAsync(TransactionType type, int monthsBack = 6);
        Task<decimal> CalculateYearlyProjectionAsync(TransactionType type);
        Task<IEnumerable<MonthlySpending>> GetMonthlySpendingTrendsAsync(int monthsBack = 12);
        Task<IEnumerable<CategoryTrend>> GetCategoryTrendsAsync();
        Task<BudgetProjection> GenerateBudgetProjectionAsync();
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
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
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
