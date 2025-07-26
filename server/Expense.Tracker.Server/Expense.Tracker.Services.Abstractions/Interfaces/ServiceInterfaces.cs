using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(string id);
        Task<Category> CreateCategoryAsync(CreateCategoryRequest request);
        Task<Category?> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(string id);
        Task<bool> CategoryExistsAsync(string id);
        Task InitializeDefaultCategoriesAsync();
    }

    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(string id);
        Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(string categoryId);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request);
        Task<Transaction?> UpdateTransactionAsync(UpdateTransactionRequest request);
        Task<bool> DeleteTransactionAsync(string id);
        Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync();
        Task ProcessRecurringTransactionsAsync();
    }

    public interface IDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ExpenseAnalytics> GetExpenseAnalyticsAsync(int monthsBack = 12);
        Task<BudgetProjection> GetBudgetProjectionAsync();
    }

    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(string id);
        Task<Tag> CreateTagAsync(CreateTagRequest request);
        Task<bool> DeleteTagAsync(string id);
        Task UpdateTagUsageAsync(string tagName);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10);
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
        Task<Category?> GetByNameAsync(string name);
    }

    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(string categoryId);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
        Task<IEnumerable<Transaction>> GetRecurringAsync();
        Task<decimal> GetTotalAmountByTypeAsync(TransactionType type, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Transaction>> GetRecentAsync(int limit);
    }

    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularAsync(int limit);
        Task IncrementUsageAsync(string tagName);
    }
}
