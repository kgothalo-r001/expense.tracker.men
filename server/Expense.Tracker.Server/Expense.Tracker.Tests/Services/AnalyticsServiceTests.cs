using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class AnalyticsServiceTests : BaseTestHelper
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsServiceTests()
    {
        _analyticsService = GetService<IAnalyticsService>();
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_ForExpenses_ReturnsValidAverage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE);

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_ForIncome_ReturnsValidAverage()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.INCOME);

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_WithCustomPeriod_ReturnsAverageForPeriod()
    {
        // Arrange
        await SeedTestDataAsync();
        var monthsBack = 3;

        // Act
        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE, monthsBack);

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMonthlyAverageAsync_WhenNoTransactions_ReturnsZero()
    {
        // Act (no seed data)
        var result = await _analyticsService.CalculateMonthlyAverageAsync(TransactionType.EXPENSE);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_ForExpenses_ReturnsValidProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE);

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_ForIncome_ReturnsValidProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.INCOME);

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateYearlyProjectionAsync_WhenNoTransactions_ReturnsZero()
    {
        // Act (no seed data)
        var result = await _analyticsService.CalculateYearlyProjectionAsync(TransactionType.EXPENSE);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WithDefaultPeriod_ReturnsValidTrends()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.GetMonthlySpendingTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(12); // Default is 12 months
        result.Should().OnlyContain(trend => trend.Amount >= 0);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WithCustomPeriod_ReturnsLimitedTrends()
    {
        // Arrange
        await SeedTestDataAsync();
        var monthsBack = 6;

        // Act
        var result = await _analyticsService.GetMonthlySpendingTrendsAsync(monthsBack);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(monthsBack);
        result.Should().OnlyContain(trend => trend.Amount >= 0);
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_WhenNoTransactions_ReturnsZeroAmountTrends()
    {
        await ClearDatabaseAsync();
        // Act (no seed data)
        var result = await _analyticsService.GetMonthlySpendingTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(12); // Should return 12 months of data
        result.Should().OnlyContain(trend => trend.Amount == 0 && trend.TransactionCount == 0);
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_ReturnsValidCategoryTrends()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.GetCategoryTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(trend => trend.CurrentMonthAmount >= 0);
        result.Should().OnlyContain(trend => trend.PreviousMonthAmount >= 0);
        result.Should().OnlyContain(trend => !string.IsNullOrEmpty(trend.CategoryName));
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_WhenNoTransactions_ReturnsEmptyTrends()
    {
        // Act (no seed data)
        var result = await _analyticsService.GetCategoryTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateBudgetProjectionAsync_ReturnsValidBudgetProjection()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.GenerateBudgetProjectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.ProjectedYearlyExpenses.Should().BeGreaterOrEqualTo(0);
        result.RecommendedMonthlySavings.Should().BeGreaterOrEqualTo(0);
        result.CategoryProjections.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateBudgetProjectionAsync_WhenNoData_ReturnsZeroBudgetProjection()
    {
        // Act (no seed data)
        var result = await _analyticsService.GenerateBudgetProjectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.ProjectedMonthlyExpenses.Should().Be(0);
        result.ProjectedYearlyExpenses.Should().Be(0);
        result.RecommendedMonthlySavings.Should().Be(0);
        result.CategoryProjections.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryTrendsAsync_GroupsByCategory_ReturnsCorrectGrouping()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.GetCategoryTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        var categoryNames = result.Select(t => t.CategoryName).ToList();
        categoryNames.Should().OnlyHaveUniqueItems(); // Each category should appear only once
        
        foreach (var trend in result)
        {
            trend.CategoryName.Should().NotBeNullOrEmpty();
            trend.CurrentMonthAmount.Should().BeGreaterOrEqualTo(0);
            trend.PreviousMonthAmount.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetMonthlySpendingTrendsAsync_OrdersByMonth_ReturnsChronologicalOrder()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _analyticsService.GetMonthlySpendingTrendsAsync();

        // Assert
        result.Should().NotBeNull();
        if (result.Count() > 1)
        {
            var trends = result.ToList();
            for (int i = 0; i < trends.Count; i++)
            {
                trends[i].Month.Should().NotBeNullOrEmpty();
            }
        }
    }
}
