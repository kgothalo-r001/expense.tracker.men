using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class CreateTransactionRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public TransactionType Type { get; set; }
        
        [Required]
        public string CategoryId { get; set; } = string.Empty;
        
        public List<string> Tags { get; set; } = new List<string>();
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public bool IsRecurring { get; set; }
        
        public RecurringFrequency? RecurringFrequency { get; set; }
        
        public DateTime? RecurringEndDate { get; set; }
    }
}
