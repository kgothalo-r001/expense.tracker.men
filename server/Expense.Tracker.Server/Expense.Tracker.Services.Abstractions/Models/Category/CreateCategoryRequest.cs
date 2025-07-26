using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class CreateCategoryRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(7)]
        public string Color { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Icon { get; set; }
        
        public CategoryType Type { get; set; }
    }
}
