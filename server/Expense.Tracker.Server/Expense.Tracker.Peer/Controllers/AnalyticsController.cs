using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Analytics}")]
    public class AnalyticsController : ExpenseManagerBaseController
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
            : base(logger)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get monthly spending trends
        /// </summary>
        [HttpGet("GetMonthlySpendingTrends")]
        public async Task<ActionResult<IEnumerable<MonthlySpending>>> GetMonthlySpendingTrends([FromQuery] int monthsBack = 12)
        {
            try
            {
                var trends = await _analyticsService.GetMonthlySpendingTrendsAsync(Requestor, monthsBack);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly spending trends for user {UserId}", Requestor.UserId);
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
                var trends = await _analyticsService.GetCategoryTrendsAsync(Requestor);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category trends for user {UserId}", Requestor.UserId);
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
                var projection = await _analyticsService.GenerateBudgetProjectionAsync(Requestor);
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget projection for user {UserId}", Requestor.UserId);
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
                var average = await _analyticsService.CalculateMonthlyAverageAsync(type, Requestor, monthsBack);
                return Ok(average);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating monthly average for user {UserId}", Requestor.UserId);
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
                var projection = await _analyticsService.CalculateYearlyProjectionAsync(type, Requestor);
                return Ok(projection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating yearly projection for user {UserId}", Requestor.UserId);
                return StatusCode(500, "An error occurred while calculating yearly projection");
            }
        }
    }
}
