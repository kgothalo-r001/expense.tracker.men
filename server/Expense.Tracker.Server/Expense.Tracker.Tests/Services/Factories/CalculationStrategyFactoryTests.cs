using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Implementation.Factories;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Implementation.Strategies;

namespace Expense.Tracker.Tests.Services.Factories;

public class CalculationStrategyFactoryTests
{
    [Fact]
    public void GetStrategy_MonthlyAverage_ReturnsMonthlyAverageCalculationStrategy()
    {
        var factory = new CalculationStrategyFactory();
        var strategy = factory.GetStrategy(CalculationStrategyType.MonthlyAverage);
        strategy.Should().NotBeNull();
        strategy.Should().BeOfType<MonthlyAverageCalculationStrategy>();
    }

    [Fact]
    public void GetStrategy_YearlyProjection_ReturnsYearlyProjectionCalculationStrategy()
    {
        var factory = new CalculationStrategyFactory();
        var strategy = factory.GetStrategy(CalculationStrategyType.YearlyProjection);
        strategy.Should().NotBeNull();
        strategy.Should().BeOfType<YearlyProjectionCalculationStrategy>();
    }

    [Fact]
    public void GetStrategy_TrendAnalysis_ReturnsTrendAnalysisCalculationStrategy()
    {
        var factory = new CalculationStrategyFactory();
        var strategy = factory.GetStrategy(CalculationStrategyType.TrendAnalysis);
        strategy.Should().NotBeNull();
        strategy.Should().BeOfType<TrendAnalysisCalculationStrategy>();
    }

    [Fact]
    public void GetStrategy_UnsupportedType_ThrowsArgumentException()
    {
        var factory = new CalculationStrategyFactory();
        var unsupportedType = (CalculationStrategyType)999;
        var act = () => factory.GetStrategy(unsupportedType);
        act.Should().Throw<ArgumentException>().WithMessage("*Unsupported calculation strategy type*");
    }
}
