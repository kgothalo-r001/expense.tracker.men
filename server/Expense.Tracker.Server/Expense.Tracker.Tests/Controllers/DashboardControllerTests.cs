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
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _mockLogger = new Mock<ILogger<DashboardController>>();
        _controller = new DashboardController(_mockDashboardService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutDateRange_ReturnsOkWithSummary()
    {
        var expectedSummary = new DashboardSummary
        {
            TotalIncome = 1000,
            TotalExpenses = 500,
            NetAmount = 500,
            TransactionCount = 2,
            TopCategories = new List<CategorySummary>(),
            RecentTransactions = new List<Transaction>()
        };
        _mockDashboardService
            .Setup(s => s.GetDashboardSummaryAsync(It.IsAny<Requestor>(), null, null))
            .ReturnsAsync(expectedSummary);

        var result = await _controller.GetDashboardSummary();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().BeEquivalentTo(expectedSummary);
    }

    [Fact]
    public async Task GetDashboardSummary_WithDateRange_ReturnsFilteredSummary()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var expectedSummary = new DashboardSummary
        {
            TotalIncome = 2000,
            TotalExpenses = 1000,
            NetAmount = 1000,
            TransactionCount = 3,
            TopCategories = new List<CategorySummary>(),
            RecentTransactions = new List<Transaction>()
        };
        _mockDashboardService
            .Setup(s => s.GetDashboardSummaryAsync(It.IsAny<Requestor>(), startDate, endDate))
            .ReturnsAsync(expectedSummary);

        var result = await _controller.GetDashboardSummary(startDate, endDate);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().BeEquivalentTo(expectedSummary);
    }

    [Fact]
    public async Task GetExpenseAnalytics_WithDefaultMonths_ReturnsOkWithAnalytics()
    {
        var expectedAnalytics = new ExpenseAnalytics
        {
            MonthlyAverage = 100,
            YearlyProjection = 1200,
            MonthlySpendingTrends = new List<MonthlySpending>(),
            CategoryTrends = new List<CategoryTrend>()
        };
        _mockDashboardService
            .Setup(s => s.GetExpenseAnalyticsAsync(It.IsAny<Requestor>(), 12))
            .ReturnsAsync(expectedAnalytics);

        var result = await _controller.GetExpenseAnalytics();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeOfType<ExpenseAnalytics>().Subject;
        analytics.Should().BeEquivalentTo(expectedAnalytics);
    }

    [Fact]
    public async Task GetExpenseAnalytics_WithCustomMonths_ReturnsAnalyticsForPeriod()
    {
        var monthsBack = 6;
        var expectedAnalytics = new ExpenseAnalytics
        {
            MonthlyAverage = 200,
            YearlyProjection = 2400,
            MonthlySpendingTrends = new List<MonthlySpending>(),
            CategoryTrends = new List<CategoryTrend>()
        };
        _mockDashboardService
            .Setup(s => s.GetExpenseAnalyticsAsync(It.IsAny<Requestor>(), monthsBack))
            .ReturnsAsync(expectedAnalytics);

        var result = await _controller.GetExpenseAnalytics(monthsBack);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeOfType<ExpenseAnalytics>().Subject;
        analytics.Should().BeEquivalentTo(expectedAnalytics);
    }

    [Fact]
    public async Task GetBudgetProjection_ReturnsOkWithProjection()
    {
        var expectedProjection = new BudgetProjection
        {
            ProjectedMonthlyExpenses = 300,
            ProjectedYearlyExpenses = 3600,
            RecommendedMonthlySavings = 150,
            CategoryProjections = new List<CategoryProjection>()
        };
        _mockDashboardService
            .Setup(s => s.GetBudgetProjectionAsync(It.IsAny<Requestor>()))
            .ReturnsAsync(expectedProjection);

        var result = await _controller.GetBudgetProjection();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var projection = okResult.Value.Should().BeOfType<BudgetProjection>().Subject;
        projection.Should().BeEquivalentTo(expectedProjection);
    }

    [Fact]
    public async Task GetDashboardSummary_WhenNoData_ReturnsZeroValues()
    {
        var expectedSummary = new DashboardSummary
        {
            TotalIncome = 0,
            TotalExpenses = 0,
            NetAmount = 0,
            TransactionCount = 0,
            TopCategories = new List<CategorySummary>(),
            RecentTransactions = new List<Transaction>()
        };
        _mockDashboardService
            .Setup(s => s.GetDashboardSummaryAsync(It.IsAny<Requestor>(), null, null))
            .ReturnsAsync(expectedSummary);

        var result = await _controller.GetDashboardSummary();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummary>().Subject;
        summary.Should().BeEquivalentTo(expectedSummary);
    }
}