namespace Expense.Tracker.Services.Abstractions.Models
{
    public class DashboardSummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount { get; set; }
        public int TransactionCount { get; set; }
        public List<CategorySummary> TopCategories { get; set; } = new List<CategorySummary>();
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
    }
}
