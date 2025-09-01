using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Peer.Helpers;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Dashboard}")]
    public class DashboardController : ExpenseManagerBaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly ITelemetryHelper _telemetryHelper;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger, ITelemetryHelper telemetryHelper)
            : base(logger)
        {
            _dashboardService = dashboardService;
            _telemetryHelper = telemetryHelper;
        }

        /// <summary>
        /// Get dashboard summary data
        /// </summary>
        [HttpGet("GetDashboardSummary")]
        public async Task<ActionResult<DashboardSummary>> GetDashboardSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync(Requestor, startDate, endDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["StartDate"] = startDate?.ToString("O") ?? "null",
                    ["EndDate"] = endDate?.ToString("O") ?? "null"
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetDashboardSummary",
                    "DashboardController.GetDashboardSummary",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while retrieving dashboard summary");
            }
        }

        /// <summary>
        /// Get expense analytics
        /// </summary>
        [HttpGet("GetExpenseAnalytics")]
        public async Task<ActionResult<ExpenseAnalytics>> GetExpenseAnalytics([FromQuery] int monthsBack = 12)
        {
            try
            {
                var analytics = await _dashboardService.GetExpenseAnalyticsAsync(Requestor, monthsBack);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                var additionalProperties = new Dictionary<string, string>
                {
                    ["MonthsBack"] = monthsBack.ToString()
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetExpenseAnalytics",
                    "DashboardController.GetExpenseAnalytics",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while retrieving expense analytics");
            }
        }

        /// <summary>
        /// Get budget projection
        /// </summary>
        [HttpGet("GetBudgetProjection")]
        public async Task<ActionResult<BudgetProjection>> GetBudgetProjection()
        {
            try
            {
                var projection = await _dashboardService.GetBudgetProjectionAsync(Requestor);
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetBudgetProjection",
                    "DashboardController.GetBudgetProjection",
                    Requestor);

                return StatusCode(500, "An error occurred while retrieving budget projection");
            }
        }
    }
}
