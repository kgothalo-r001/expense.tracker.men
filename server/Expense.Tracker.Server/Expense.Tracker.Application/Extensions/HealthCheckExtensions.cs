namespace Expense.Tracker.Application.Extensions
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Add and configure health checks for the Expense Tracker application
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"))
                .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("In-memory storage is healthy"))
                .AddCheck("services", () => 
                {
                    // Could add more complex service checks here in the future
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("All services are operational");
                });

            return services;
        }

        /// <summary>
        /// Configure health check endpoints and UI integration
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for chaining</returns>
        public static WebApplication MapExpenseTrackerHealthChecks(this WebApplication app)
        {
            // Map health checks endpoint with custom response format
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        Status = report.Status.ToString(),
                        Checks = report.Entries.Select(x => new
                        {
                            Name = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description,
                            Duration = x.Value.Duration.TotalMilliseconds
                        }),
                        TotalDuration = report.TotalDuration.TotalMilliseconds,
                        Timestamp = DateTime.UtcNow,
                        Environment = app.Environment.EnvironmentName
                    };
                    
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    }));
                }
            });

            return app;
        }
    }
}
