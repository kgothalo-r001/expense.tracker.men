using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Implementation.Strategies;

namespace Expense.Tracker.Services.Implementation.Factories
{
    /// <summary>
    /// Factory for creating calculation strategies
    /// </summary>
    public class CalculationStrategyFactory : ICalculationStrategyFactory
    {
        private readonly Dictionary<CalculationStrategyType, Func<ICalculationStrategy>> _strategies;

        public CalculationStrategyFactory()
        {
            _strategies = new Dictionary<CalculationStrategyType, Func<ICalculationStrategy>>
            {
                { CalculationStrategyType.MonthlyAverage, () => new MonthlyAverageCalculationStrategy() },
                { CalculationStrategyType.YearlyProjection, () => new YearlyProjectionCalculationStrategy(new MonthlyAverageCalculationStrategy()) },
                { CalculationStrategyType.TrendAnalysis, () => new TrendAnalysisCalculationStrategy() }
            };
        }

        public ICalculationStrategy GetStrategy(CalculationStrategyType strategyType)
        {
            if (_strategies.TryGetValue(strategyType, out var strategyFactory))
            {
                return strategyFactory();
            }

            throw new ArgumentException($"Unsupported calculation strategy type: {strategyType}", nameof(strategyType));
        }
    }
}
