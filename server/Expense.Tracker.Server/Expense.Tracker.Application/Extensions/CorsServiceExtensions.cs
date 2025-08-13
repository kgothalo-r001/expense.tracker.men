using System.Diagnostics.CodeAnalysis;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring CORS policies
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CorsServiceExtensions
    {
        public const string ExpenseTrackerCorsPolicyName = "ExpenseTrackerCorsPolicy";
        public const string ExpenseTrackerCorsProductionPolicyName = "ExpenseTrackerCorsProduction";

        /// <summary>
        /// Add and configure CORS policies for the Expense Tracker application
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(ExpenseTrackerCorsPolicyName, builder =>
                {
                    builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });

                options.AddPolicy(ExpenseTrackerCorsProductionPolicyName, builder =>
                {
                    builder.WithOrigins("https://yourdomain.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Get the development CORS policy name
        /// </summary>
        public static string DevelopmentPolicyName => ExpenseTrackerCorsPolicyName;

        /// <summary>
        /// Get the production CORS policy name
        /// </summary>
        public static string ProductionPolicyName => ExpenseTrackerCorsProductionPolicyName;
    }
}
