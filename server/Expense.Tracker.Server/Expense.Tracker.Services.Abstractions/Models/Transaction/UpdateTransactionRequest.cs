using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class UpdateTransactionRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public DateTime? Date { get; set; }
        
        public TransactionType? Type { get; set; }
        
        public string? CategoryId { get; set; }
        
        public List<string>? Tags { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public bool? IsRecurring { get; set; }
        
        public RecurringFrequency? RecurringFrequency { get; set; }
        
        public DateTime? RecurringEndDate { get; set; }
    }
}
