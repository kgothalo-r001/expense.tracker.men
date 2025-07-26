namespace Expense.Tracker.Services.Abstractions.Models
{
    public class ExpenseAnalytics
    {
        public decimal MonthlyAverage { get; set; }
        public decimal YearlyProjection { get; set; }
        public List<MonthlySpending> MonthlySpendingTrends { get; set; } = new List<MonthlySpending>();
        public List<CategoryTrend> CategoryTrends { get; set; } = new List<CategoryTrend>();
    }
}
