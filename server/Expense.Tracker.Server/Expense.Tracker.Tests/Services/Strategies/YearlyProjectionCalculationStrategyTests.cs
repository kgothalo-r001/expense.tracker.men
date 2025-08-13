using Xunit;
using FluentAssertions;
using Moq;
using Expense.Tracker.Services.Implementation.Strategies;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Strategies;

public class YearlyProjectionCalculationStrategyTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsYearlyProjection()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        var mockMonthlyStrategy = new Mock<ICalculationStrategy>();
        mockMonthlyStrategy.Setup(s => s.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 12)).ReturnsAsync(100m);
        var strategy = new YearlyProjectionCalculationStrategy(mockMonthlyStrategy.Object);
        var result = await strategy.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 12);
        result.Should().Be(1200m);
    }

    [Fact]
    public void Constructor_WithNullStrategy_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new YearlyProjectionCalculationStrategy(null!));
    }
}
