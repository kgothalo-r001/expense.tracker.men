using System.ComponentModel.DataAnnotations;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class Tag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        public int UsageCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
