using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Expense.Tracker.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring Application Insights telemetry services
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Add Application Insights telemetry services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddExpenseTrackerApplicationInsights(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Add Application Insights telemetry
            services.AddApplicationInsightsTelemetry(configuration);

            // Configure telemetry settings
            services.Configure<TelemetryConfiguration>(telemetryConfig =>
            {
                if (configuration.GetValue<bool>("ApplicationInsights:EnableAdaptiveSampling"))
                {
                    telemetryConfig.DefaultTelemetrySink.TelemetryProcessorChainBuilder.UseAdaptiveSampling();
                }
            });

            services.AddSingleton<ITelemetryInitializer, ExpenseTrackerTelemetryInitializer>();

            return services;
        }
    }

    /// <summary>
    /// Custom telemetry initializer for Expense Tracker application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExpenseTrackerTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["Application"] = "ExpenseTracker";
            telemetry.Context.GlobalProperties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        }
    }
}
