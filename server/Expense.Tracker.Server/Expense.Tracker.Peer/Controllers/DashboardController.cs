using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [ApiController]
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Dashboard}")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
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
                var summary = await _dashboardService.GetDashboardSummaryAsync(startDate, endDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard summary");
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
                var analytics = await _dashboardService.GetExpenseAnalyticsAsync(monthsBack);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expense analytics");
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
                var projection = await _dashboardService.GetBudgetProjectionAsync();
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget projection");
                return StatusCode(500, "An error occurred while retrieving budget projection");
            }
        }
    }
}
