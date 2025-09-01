using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Peer.Helpers
{
    /// <summary>
    /// Interface for telemetry operations to enable testing
    /// </summary>
    public interface ITelemetryHelper
    {
        /// <summary>
        /// Logs an exception with Application Insights telemetry and enhanced context
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="operationName">Name of the operation that failed</param>
        /// <param name="controllerAction">Controller and action name (e.g., "AnalyticsController.GetMonthlySpendingTrends")</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="additionalProperties">Additional custom properties for telemetry</param>
        /// <param name="userFriendlyMessage">User-friendly error message to return</param>
        void LogErrorWithTelemetry(
            Exception exception,
            string operationName,
            string controllerAction,
            Requestor? requestor = null,
            Dictionary<string, string>? additionalProperties = null,
            string? userFriendlyMessage = null);

        /// <summary>
        /// Tracks a custom event with Application Insights
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="properties">Custom properties</param>
        /// <param name="metrics">Custom metrics</param>
        void TrackEvent(
            string eventName,
            Requestor? requestor = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, double>? metrics = null);

        /// <summary>
        /// Tracks a custom metric with Application Insights
        /// </summary>
        /// <param name="metricName">Name of the metric</param>
        /// <param name="value">Metric value</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="properties">Additional properties</param>
        void TrackMetric(
            string metricName,
            double value,
            Requestor? requestor = null,
            Dictionary<string, string>? properties = null);

        /// <summary>
        /// Tracks operation duration and success/failure
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="requestor">Current user context</param>
        /// <param name="additionalProperties">Additional properties</param>
        void TrackDependency(
            string operationName,
            TimeSpan duration,
            bool success,
            Requestor? requestor = null,
            Dictionary<string, string>? additionalProperties = null);
    }
}
