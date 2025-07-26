using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [ApiController]
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Analytics}")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get monthly spending trends
        /// </summary>
        [HttpGet("GetMonthlySpendingTrends")]
        public async Task<ActionResult<IEnumerable<MonthlySpending>>> GetMonthlySpendingTrends([FromQuery] int monthsBack = 12)
        {
            try
            {
                var trends = await _analyticsService.GetMonthlySpendingTrendsAsync(monthsBack);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly spending trends");
                return StatusCode(500, "An error occurred while retrieving monthly spending trends");
            }
        }

        /// <summary>
        /// Get category trends
        /// </summary>
        [HttpGet("GetCategoryTrends")]
        public async Task<ActionResult<IEnumerable<CategoryTrend>>> GetCategoryTrends()
        {
            try
            {
                var trends = await _analyticsService.GetCategoryTrendsAsync();
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category trends");
                return StatusCode(500, "An error occurred while retrieving category trends");
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
                var projection = await _analyticsService.GenerateBudgetProjectionAsync();
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget projection");
                return StatusCode(500, "An error occurred while retrieving budget projection");
            }
        }

        /// <summary>
        /// Calculate monthly average for a transaction type
        /// </summary>
        [HttpGet("GetMonthlyAverage")]
        public async Task<ActionResult<decimal>> GetMonthlyAverage(
            [FromQuery] TransactionType type,
            [FromQuery] int monthsBack = 6)
        {
            try
            {
                var average = await _analyticsService.CalculateMonthlyAverageAsync(type, monthsBack);
                return Ok(average);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating monthly average");
                return StatusCode(500, "An error occurred while calculating monthly average");
            }
        }

        /// <summary>
        /// Calculate yearly projection for a transaction type
        /// </summary>
        [HttpGet("GetYearlyProjection")]
        public async Task<ActionResult<decimal>> GetYearlyProjection([FromQuery] TransactionType type)
        {
            try
            {
                var projection = await _analyticsService.CalculateYearlyProjectionAsync(type);
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating yearly projection");
                return StatusCode(500, "An error occurred while calculating yearly projection");
            }
        }
    }
}
