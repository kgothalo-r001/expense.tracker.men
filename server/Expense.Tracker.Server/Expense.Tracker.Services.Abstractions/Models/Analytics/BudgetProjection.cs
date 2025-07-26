namespace Expense.Tracker.Services.Abstractions.Models
{
    public class BudgetProjection
    {
        public decimal ProjectedMonthlyExpenses { get; set; }
        public decimal ProjectedYearlyExpenses { get; set; }
        public decimal RecommendedMonthlySavings { get; set; }
        public List<CategoryProjection> CategoryProjections { get; set; } = new List<CategoryProjection>();
    }
}
