namespace Expense.Tracker.Services.Abstractions.Models
{
    public class CategoryProjection
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal AverageMonthlySpending { get; set; }
        public decimal ProjectedYearlySpending { get; set; }
        public decimal RecommendedBudget { get; set; }
    }
}
