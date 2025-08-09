using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Dashboard}")]
    public class DashboardController : ExpenseManagerBaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
            : base(logger)
        {
            _dashboardService = dashboardService;
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
                _logger.LogError(ex, "Error retrieving dashboard summary for user {UserId}", Requestor.UserId);
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
                _logger.LogError(ex, "Error retrieving expense analytics for user {UserId}", Requestor.UserId);
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
                _logger.LogError(ex, "Error retrieving budget projection for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while retrieving budget projection");
            }
        }
    }
}
