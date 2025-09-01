using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Peer.Helpers
{
    /// <summary>
    /// Helper class for centralized Application Insights telemetry logging
    /// </summary>
    public class TelemetryHelper : ITelemetryHelper
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<TelemetryHelper> _logger;

        public TelemetryHelper(TelemetryClient telemetryClient, ILogger<TelemetryHelper> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        /// <summary>
        /// Logs an exception with Application Insights telemetry and enhanced context
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="operationName">Name of the operation that failed</param>
        /// <param name="controllerAction">Controller and action name (e.g., "AnalyticsController.GetMonthlySpendingTrends")</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="additionalProperties">Additional custom properties for telemetry</param>
        /// <param name="userFriendlyMessage">User-friendly error message to return</param>
        public void LogErrorWithTelemetry(
            Exception exception,
            string operationName,
            string controllerAction,
            Requestor? requestor = null,
            Dictionary<string, string>? additionalProperties = null,
            string? userFriendlyMessage = null)
        {
            var exceptionTelemetry = new ExceptionTelemetry(exception)
            {
                SeverityLevel = SeverityLevel.Error
            };

            exceptionTelemetry.Properties["Operation"] = operationName;
            exceptionTelemetry.Properties["ControllerAction"] = controllerAction;
            exceptionTelemetry.Properties["UserId"] = requestor?.UserId ?? "Unknown";
            exceptionTelemetry.Properties["SessionId"] = requestor?.SessionId ?? "Unknown";
            exceptionTelemetry.Properties["Timestamp"] = DateTime.UtcNow.ToString("O");
            exceptionTelemetry.Properties["ExceptionType"] = exception.GetType().Name;
            
            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties)
                {
                    exceptionTelemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackException(exceptionTelemetry);

            _logger.LogError(exception, 
                "Error in operation {Operation} for user {UserId}. Controller: {ControllerAction}, SessionId: {SessionId}",
                operationName, requestor?.UserId, controllerAction, requestor?.SessionId);
        }

        /// <summary>
        /// Tracks a custom event with Application Insights
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="properties">Custom properties</param>
        /// <param name="metrics">Custom metrics</param>
        public void TrackEvent(
            string eventName,
            Requestor? requestor = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, double>? metrics = null)
        {
            var eventProperties = new Dictionary<string, string>
            {
                ["UserId"] = requestor?.UserId ?? "Unknown",
                ["SessionId"] = requestor?.SessionId ?? "Unknown",
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            };

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    eventProperties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackEvent(eventName, eventProperties, metrics);
        }

        /// <summary>
        /// Tracks a custom metric with Application Insights
        /// </summary>
        /// <param name="metricName">Name of the metric</param>
        /// <param name="value">Metric value</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="properties">Additional properties</param>
        public void TrackMetric(
            string metricName,
            double value,
            Requestor? requestor = null,
            Dictionary<string, string>? properties = null)
        {
            var metricProperties = new Dictionary<string, string>
            {
                ["UserId"] = requestor?.UserId ?? "Unknown",
                ["SessionId"] = requestor?.SessionId ?? "Unknown"
            };

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    metricProperties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackMetric(metricName, value, metricProperties);
        }

        /// <summary>
        /// Tracks operation duration and success/failure
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="additionalProperties">Additional properties</param>
        public void TrackDependency(
            string operationName,
            TimeSpan duration,
            bool success,
            Requestor? requestor = null,
            Dictionary<string, string>? additionalProperties = null)
        {
            var dependencyTelemetry = new DependencyTelemetry
            {
                Name = operationName,
                Duration = duration,
                Success = success,
                Timestamp = DateTime.UtcNow
            };

            dependencyTelemetry.Properties["UserId"] = requestor?.UserId ?? "Unknown";
            dependencyTelemetry.Properties["SessionId"] = requestor?.SessionId ?? "Unknown";

            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties)
                {
                    dependencyTelemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackDependency(dependencyTelemetry);
        }
    }
}
