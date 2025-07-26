namespace Expense.Tracker.Services.Abstractions.Models
{
    public class CategoryTrend
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentMonthAmount { get; set; }
        public decimal PreviousMonthAmount { get; set; }
        public decimal PercentageChange { get; set; }
    }
}
