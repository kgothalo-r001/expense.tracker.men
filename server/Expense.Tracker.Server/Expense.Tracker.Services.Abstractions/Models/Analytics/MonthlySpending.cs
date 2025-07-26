namespace Expense.Tracker.Services.Abstractions.Models
{
    public class MonthlySpending
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
    }
}
