using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Helpers;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Tests.Services.Helpers;

public class AnalyticsHelpersTests
{
    [Fact]
    public void GetMonthDateRange_ReturnsCorrectStartAndEnd()
    {
        var date = new DateTime(2025, 8, 13);
        var (start, end) = AnalyticsHelpers.GetMonthDateRange(date);
        start.Should().Be(new DateTime(2025, 8, 1));
        end.Should().Be(new DateTime(2025, 8, 31));
    }

    [Fact]
    public void ValidateMonthsBack_WithValidValue_DoesNotThrow()
    {
        var act = () => AnalyticsHelpers.ValidateMonthsBack(1);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateMonthsBack_WithZero_ThrowsArgumentOutOfRangeException()
    {
        var act = () => AnalyticsHelpers.ValidateMonthsBack(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValidateMonthsBack_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        var act = () => AnalyticsHelpers.ValidateMonthsBack(-5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValidateTransactionType_WithValidType_DoesNotThrow()
    {
        var act = () => AnalyticsHelpers.ValidateTransactionType(TransactionType.EXPENSE);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateTransactionType_WithInvalidType_ThrowsArgumentException()
    {
        var invalidType = (TransactionType)999;
        var act = () => AnalyticsHelpers.ValidateTransactionType(invalidType);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid transaction type*");
    }
}
