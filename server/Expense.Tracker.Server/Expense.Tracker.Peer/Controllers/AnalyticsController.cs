using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Peer.Helpers;

namespace Expense.Tracker.Peer.Controllers
{
    [Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Analytics}")]
    public class AnalyticsController : ExpenseManagerBaseController
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITelemetryHelper _telemetryHelper;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger, ITelemetryHelper telemetryHelper)
            : base(logger)
        {
            _analyticsService = analyticsService;
            _telemetryHelper = telemetryHelper;
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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["MonthsBack"] = monthsBack.ToString()
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetMonthlySpendingTrends",
                    "AnalyticsController.GetMonthlySpendingTrends",
                    Requestor,
                    additionalProperties);

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
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetCategoryTrends",
                    "AnalyticsController.GetCategoryTrends",
                    Requestor);

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
                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetBudgetProjection",
                    "AnalyticsController.GetBudgetProjection",
                    Requestor);

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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["TransactionType"] = type.ToString(),
                    ["MonthsBack"] = monthsBack.ToString()
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetMonthlyAverage",
                    "AnalyticsController.GetMonthlyAverage",
                    Requestor,
                    additionalProperties);

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
                var additionalProperties = new Dictionary<string, string>
                {
                    ["TransactionType"] = type.ToString()
                };

                _telemetryHelper.LogErrorWithTelemetry(
                    ex,
                    "GetYearlyProjection",
                    "AnalyticsController.GetYearlyProjection",
                    Requestor,
                    additionalProperties);

                return StatusCode(500, "An error occurred while calculating yearly projection");
            }
        }
    }
}
