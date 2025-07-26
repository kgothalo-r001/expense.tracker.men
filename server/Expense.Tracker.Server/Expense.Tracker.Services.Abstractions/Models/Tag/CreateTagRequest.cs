using System.ComponentModel.DataAnnotations;

namespace Expense.Tracker.Services.Abstractions.Models
{
    public class CreateTagRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(7)]
        public string? Color { get; set; }
    }
}
