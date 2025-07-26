using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Category? Category { get; set; }
    }
}
