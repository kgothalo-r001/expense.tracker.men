using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for initializing application data
    /// </summary>
    public static class DataInitializationExtensions
    {
        /// <summary>
        /// Initialize default data (categories, etc.)
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task InitializeExpenseTrackerDataAsync(this IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
                
                // Initialize default categories on startup
                await categoryService.InitializeDefaultCategoriesAsync();
            }
            catch (Exception ex)
            {
                // Get logger if available, otherwise fall back to console
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    var logger = loggerFactory.CreateLogger(typeof(DataInitializationExtensions));
                    logger.LogError(ex, "Failed to initialize default expense tracker data");
                }
                else
                {
                    Console.WriteLine($"Failed to initialize default expense tracker data: {ex.Message}");
                }
                
                // Re-throw to ensure startup fails if initialization is critical
                throw;
            }
        }
    }
}
