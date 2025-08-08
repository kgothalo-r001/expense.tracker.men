using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Services.Abstractions.Interfaces
{
    /// <summary>
    /// Factory for creating calculation strategies
    /// </summary>
    public interface ICalculationStrategyFactory
    {
        /// <summary>
        /// Gets the appropriate calculation strategy
        /// </summary>
        /// <param name="strategyType">Type of calculation strategy needed</param>
        /// <returns>Calculation strategy instance</returns>
        ICalculationStrategy GetStrategy(CalculationStrategyType strategyType);
    }
}
