using Xunit;
using FluentAssertions;
using Moq;
using Expense.Tracker.Services.Implementation.Strategies;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Strategies;

public class TrendAnalysisCalculationStrategyTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsTrendAnalysis()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetTotalAmountByTypeAsync(TransactionType.EXPENSE, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((TransactionType type, DateTime start, DateTime end) =>
            {
                if (start.Month == DateTime.UtcNow.AddMonths(-2).Month) return 100m;
                if (start.Month == DateTime.UtcNow.AddMonths(-1).Month) return 120m;
                return 0m;
            });
        var strategy = new TrendAnalysisCalculationStrategy();
        var result = await strategy.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 2);
        result.Should().BeApproximately(20m, 0.1m);
    }

    [Fact]
    public async Task CalculateAsync_WhenException_ThrowsInvalidOperationException()
    {
        var mockRepo = new Mock<ITransactionRepository>();
        mockRepo.Setup(r => r.GetTotalAmountByTypeAsync(TransactionType.EXPENSE, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception("fail"));
        var strategy = new TrendAnalysisCalculationStrategy();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await strategy.CalculateAsync(mockRepo.Object, TransactionType.EXPENSE, 2));
    }
}
