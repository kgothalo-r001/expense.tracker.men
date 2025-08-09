using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Implementation.Factories
{
    public static class CategoryFactory
    {
        public static Category CreateCategory(CreateCategoryRequest request, Guid userId)
        {
            return new Category
            {
                UserId = userId.ToString(),
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                Icon = request.Icon,
                Type = request.Type,
                IsDefault = false
            };
        }

        public static Category CreateDefaultCategory(string name, string color, string icon, CategoryType type)
        {
            return new Category
            {
                Name = name,
                Color = color,
                Icon = icon,
                Type = type,
                IsDefault = true
            };
        }
    }
}
