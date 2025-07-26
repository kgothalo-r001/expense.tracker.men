using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class Category
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(7)] // For hex color codes
        public string Color { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Icon { get; set; }
        
        public CategoryType Type { get; set; }
        
        public bool IsDefault { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
