using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class DashboardControllerTests : BaseTestHelper
{
    private readonly DashboardController _controller;
    private readonly IDashboardService _dashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;

    public DashboardControllerTests()
    {
        _dashboardService = GetService<IDashboardService>();
        _mockLogger = new Mock<ILogger<DashboardController>>();
        _controller = new DashboardController(_dashboardService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutDateRange_ReturnsOkWithSummary()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetDashboardSummary();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().NotBeNull();
        summary.TotalIncome.Should().BeGreaterThan(0);
        summary.TotalExpenses.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDashboardSummary_WithDateRange_ReturnsFilteredSummary()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _controller.GetDashboardSummary(startDate, endDate);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().NotBeNull();
        summary.TotalIncome.Should().BeGreaterOrEqualTo(0);
        summary.TotalExpenses.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpenseAnalytics_WithDefaultMonths_ReturnsOkWithAnalytics()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetExpenseAnalytics();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeOfType<ExpenseAnalytics>().Subject;
        analytics.Should().NotBeNull();
        analytics.CategoryTrends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExpenseAnalytics_WithCustomMonths_ReturnsAnalyticsForPeriod()
    {
        // Arrange
        await SeedTestDataAsync();
        var monthsBack = 6;

        // Act
        var result = await _controller.GetExpenseAnalytics(monthsBack);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeOfType<ExpenseAnalytics>().Subject;
        analytics.Should().NotBeNull();
        analytics.MonthlySpendingTrends.Should().NotBeNull();
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
        projection.ProjectedMonthlyExpenses.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDashboardSummary_WhenNoData_ReturnsZeroValues()
    {
        // Act (no seed data)
        var result = await _controller.GetDashboardSummary();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().NotBeNull();
        summary.TotalIncome.Should().Be(0);
        summary.TotalExpenses.Should().Be(0);
        summary.NetAmount.Should().Be(0);
    }
}
