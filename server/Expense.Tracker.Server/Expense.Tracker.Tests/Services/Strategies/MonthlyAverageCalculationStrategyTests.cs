using Xunit;
using FluentAssertions;
using Moq;
using Expense.Tracker.Services.Implementation.Strategies;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Strategies;

public class MonthlyAverageCalculationStrategyTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsMonthlyAverage()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetTotalAmountByTypeAsync(TransactionType.EXPENSE, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(1200m);
        var strategy = new MonthlyAverageCalculationStrategy();
        var result = await strategy.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 12);
        result.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateAsync_WhenException_ThrowsInvalidOperationException()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetTotalAmountByTypeAsync(TransactionType.EXPENSE, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception("fail"));
        var strategy = new MonthlyAverageCalculationStrategy();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await strategy.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 12));
    }
}
