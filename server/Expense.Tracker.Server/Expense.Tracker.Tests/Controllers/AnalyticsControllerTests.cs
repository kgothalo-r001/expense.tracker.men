using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class AnalyticsControllerTests : BaseTestHelper
{
    private readonly AnalyticsController _controller;
    private readonly IAnalyticsService _analyticsService;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;

    public AnalyticsControllerTests()
    {
        _analyticsService = GetService<IAnalyticsService>();
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_analyticsService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetMonthlySpendingTrends_WithDefaultMonths_ReturnsOkWithTrends()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetMonthlySpendingTrends();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<MonthlySpending>>().Subject;
        trends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMonthlySpendingTrends_WithCustomMonths_ReturnsFilteredTrends()
    {
        // Arrange
        await SeedTestDataAsync();
        var monthsBack = 6;

        // Act
        var result = await _controller.GetMonthlySpendingTrends(monthsBack);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<MonthlySpending>>().Subject;
        trends.Should().NotBeNull();
        trends.Count().Should().BeLessOrEqualTo(monthsBack);
    }

    [Fact]
    public async Task GetCategoryTrends_ReturnsOkWithCategoryTrends()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetCategoryTrends();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryTrend>>().Subject;
        trends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBudgetProjection_ReturnsOkWithProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetBudgetProjection();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var projection = okResult.Value.Should().BeOfType<BudgetProjection>().Subject;
        projection.Should().NotBeNull();
        projection.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetMonthlyAverage_ForExpenses_ReturnsOkWithAverage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetMonthlyAverage(TransactionType.EXPENSE);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var average = okResult.Value.Should().BeOfType<decimal>().Subject;
        average.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetMonthlyAverage_ForIncome_ReturnsOkWithAverage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetMonthlyAverage(TransactionType.INCOME);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var average = okResult.Value.Should().BeOfType<decimal>().Subject;
        average.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetYearlyProjection_ForExpenses_ReturnsOkWithProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetYearlyProjection(TransactionType.EXPENSE);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var projection = okResult.Value.Should().BeOfType<decimal>().Subject;
        projection.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetCategoryTrends_WhenNoData_ReturnsEmptyTrends()
    {
        // Act (no seed data)
        var result = await _controller.GetCategoryTrends();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryTrend>>().Subject;
        trends.Should().BeEmpty();
    }
}
