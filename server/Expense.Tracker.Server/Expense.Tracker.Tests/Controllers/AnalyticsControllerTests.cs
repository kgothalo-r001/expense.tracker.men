using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Peer.Helpers;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Expense.Tracker.Tests.Controllers;

public class AnalyticsControllerTests : BaseTestHelper
{
    private readonly AnalyticsController _controller;
    private readonly Mock<IAnalyticsService> _mockAnalyticsService;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;
    private readonly Mock<ITelemetryHelper> _mockTelemetryHelper;

    public AnalyticsControllerTests()
    {
        _mockAnalyticsService = new Mock<IAnalyticsService>();
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _mockTelemetryHelper = new Mock<ITelemetryHelper>();
        
        _controller = new AnalyticsController(_mockAnalyticsService.Object, _mockLogger.Object, _mockTelemetryHelper.Object);
    }

    [Fact]
    public async Task GetMonthlySpendingTrends_WithDefaultMonths_ReturnsOkWithTrends()
    {
        var expectedTrends = new List<MonthlySpending>
        {
            new MonthlySpending { Month = "2024-05", Amount = 100, TransactionCount = 2 }
        };
        _mockAnalyticsService
            .Setup(s => s.GetMonthlySpendingTrendsAsync(It.IsAny<Requestor>(), 12))
            .ReturnsAsync(expectedTrends);

        var result = await _controller.GetMonthlySpendingTrends();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<MonthlySpending>>().Subject;
        trends.Should().BeEquivalentTo(expectedTrends);
    }

    [Fact]
    public async Task GetMonthlySpendingTrends_WithCustomMonths_ReturnsFilteredTrends()
    {
        var monthsBack = 6;
        var expectedTrends = new List<MonthlySpending>
        {
            new MonthlySpending { Month = "2024-05", Amount = 50, TransactionCount = 1 }
        };
        _mockAnalyticsService
            .Setup(s => s.GetMonthlySpendingTrendsAsync(It.IsAny<Requestor>(), monthsBack))
            .ReturnsAsync(expectedTrends);

        var result = await _controller.GetMonthlySpendingTrends(monthsBack);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<MonthlySpending>>().Subject;
        trends.Should().BeEquivalentTo(expectedTrends);
    }

    [Fact]
    public async Task GetCategoryTrends_ReturnsOkWithCategoryTrends()
    {
        var expectedTrends = new List<CategoryTrend>
        {
            new CategoryTrend { CategoryId = "cat1", CategoryName = "Food", CurrentMonthAmount = 100, PreviousMonthAmount = 80, PercentageChange = 25 }
        };
        _mockAnalyticsService
            .Setup(s => s.GetCategoryTrendsAsync(It.IsAny<Requestor>()))
            .ReturnsAsync(expectedTrends);

        var result = await _controller.GetCategoryTrends();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryTrend>>().Subject;
        trends.Should().BeEquivalentTo(expectedTrends);
    }

    [Fact]
    public async Task GetBudgetProjection_ReturnsOkWithProjection()
    {
        var expectedProjection = new BudgetProjection
        {
            ProjectedMonthlyExpenses = 200,
            ProjectedYearlyExpenses = 2400,
            RecommendedMonthlySavings = 100,
            CategoryProjections = new List<CategoryProjection>()
        };
        _mockAnalyticsService
            .Setup(s => s.GenerateBudgetProjectionAsync(It.IsAny<Requestor>()))
            .ReturnsAsync(expectedProjection);

        var result = await _controller.GetBudgetProjection();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var projection = okResult.Value.Should().BeOfType<BudgetProjection>().Subject;
        projection.Should().BeEquivalentTo(expectedProjection);
    }

    [Fact]
    public async Task GetMonthlyAverage_ForExpenses_ReturnsOkWithAverage()
    {
        var expectedAverage = 150m;
        _mockAnalyticsService
            .Setup(s => s.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, 6))
            .ReturnsAsync(expectedAverage);

        var result = await _controller.GetMonthlyAverage(TransactionType.EXPENSE);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var average = okResult.Value.Should().BeOfType<decimal>().Subject;
        average.Should().Be(expectedAverage);
    }

    [Fact]
    public async Task GetMonthlyAverage_ForIncome_ReturnsOkWithAverage()
    {
        var expectedAverage = 3000m;
        _mockAnalyticsService
            .Setup(s => s.CalculateMonthlyAverageAsync(TransactionType.INCOME, 6))
            .ReturnsAsync(expectedAverage);

        var result = await _controller.GetMonthlyAverage(TransactionType.INCOME);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var average = okResult.Value.Should().BeOfType<decimal>().Subject;
        average.Should().Be(expectedAverage);
    }

    [Fact]
    public async Task GetYearlyProjection_ForExpenses_ReturnsOkWithProjection()
    {
        var expectedProjection = 1800m;
        _mockAnalyticsService
            .Setup(s => s.CalculateYearlyProjectionAsync(TransactionType.EXPENSE))
            .ReturnsAsync(expectedProjection);

        var result = await _controller.GetYearlyProjection(TransactionType.EXPENSE);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var projection = okResult.Value.Should().BeOfType<decimal>().Subject;
        projection.Should().Be(expectedProjection);
    }

    [Fact]
    public async Task GetCategoryTrends_WhenNoData_ReturnsEmptyTrends()
    {
        _mockAnalyticsService
            .Setup(s => s.GetCategoryTrendsAsync(It.IsAny<Requestor>()))
            .ReturnsAsync(new List<CategoryTrend>());

        var result = await _controller.GetCategoryTrends();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryTrend>>().Subject;
        trends.Should().BeEmpty();
    }
}