using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class DashboardServiceTests : BaseTestHelper
{
    private readonly IDashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _dashboardService = GetService<IDashboardService>();
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithoutDateRange_ReturnsValidSummary()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetDashboardSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().BeGreaterThan(0);
        result.TotalExpenses.Should().BeGreaterThan(0);
        result.NetAmount.Should().Be(result.TotalIncome - result.TotalExpenses);
        result.TransactionCount.Should().BeGreaterThan(0);
        result.RecentTransactions.Should().NotBeEmpty();
        result.RecentTransactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WithDateRange_ReturnsFilteredSummary()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _dashboardService.GetDashboardSummaryAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().BeGreaterOrEqualTo(0);
        result.TotalExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecentTransactions.Should().OnlyContain(t => 
            t.Date >= startDate && t.Date <= endDate && t.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_WhenNoTransactions_ReturnsZeroValues()
    {
        // Act (no seed data)
        var result = await _dashboardService.GetDashboardSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(0);
        result.TotalExpenses.Should().Be(0);
        result.NetAmount.Should().Be(0);
        result.TransactionCount.Should().Be(0);
        result.RecentTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WithDefaultPeriod_ReturnsValidAnalytics()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetExpenseAnalyticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.MonthlySpendingTrends.Should().NotBeNull();
        result.CategoryTrends.Should().NotBeNull();
        result.YearlyProjection.Should().BeGreaterThan(0);
        result.MonthlyAverage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WithCustomPeriod_ReturnsAnalyticsForPeriod()
    {
        // Arrange
        await SeedTestDataAsync();
        var monthsBack = 6;

        // Act
        var result = await _dashboardService.GetExpenseAnalyticsAsync(monthsBack);

        // Assert
        result.Should().NotBeNull();
        result.MonthlySpendingTrends.Should().NotBeNull();
        result.CategoryTrends.Should().NotBeNull();
        result.YearlyProjection.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_WhenNoExpenses_ReturnsZeroAnalytics()
    {
        // Act (no seed data)
        var result = await _dashboardService.GetExpenseAnalyticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.YearlyProjection.Should().Be(0);
        result.MonthlyAverage.Should().Be(0);
        result.CategoryTrends.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBudgetProjectionAsync_ReturnsValidProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetBudgetProjectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.ProjectedYearlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecommendedMonthlySavings.Should().BeGreaterOrEqualTo(0);
        result.CategoryProjections.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBudgetProjectionAsync_WhenNoData_ReturnsZeroProjection()
    {
        // Act (no seed data)
        var result = await _dashboardService.GetBudgetProjectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().Be(0);
        result.ProjectedYearlyExpenses.Should().Be(0);
        result.RecommendedMonthlySavings.Should().Be(0);
        result.CategoryProjections.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserDashboardSummaryAsync_WithValidUserId_ReturnsUserSpecificSummary()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetUserDashboardSummaryAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.RecentTransactions.Should().OnlyContain(t => t.UserId == TestUserId.ToString());
        result.TotalIncome.Should().BeGreaterThan(0);
        result.TotalExpenses.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUserExpenseAnalyticsAsync_WithValidUserId_ReturnsUserSpecificAnalytics()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetUserExpenseAnalyticsAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.CategoryTrends.Should().NotBeNull();
        result.YearlyProjection.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUserBudgetProjectionAsync_WithValidUserId_ReturnsUserSpecificProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _dashboardService.GetUserBudgetProjectionAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.ProjectedYearlyExpenses.Should().BeGreaterOrEqualTo(0);
    }
}
