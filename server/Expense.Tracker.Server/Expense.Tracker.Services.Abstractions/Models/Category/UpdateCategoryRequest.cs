using System.ComponentModel.DataAnnotations;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class UpdateCategoryRequest
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Icon { get; set; }
        
        public CategoryType? Type { get; set; }
    }
}
