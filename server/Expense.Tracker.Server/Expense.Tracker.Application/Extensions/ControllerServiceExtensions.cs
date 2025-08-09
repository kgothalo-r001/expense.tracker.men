namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring MVC controllers
    /// </summary>
    public static class ControllerServiceExtensions
    {
        /// <summary>
        /// Add and configure controllers from the Peer assembly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerControllers(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddApplicationPart(typeof(Expense.Tracker.Peer.Controllers.CategoriesController).Assembly)
                .AddControllersAsServices()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization to use enum names instead of integers
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            return services;
        }
    }
}
