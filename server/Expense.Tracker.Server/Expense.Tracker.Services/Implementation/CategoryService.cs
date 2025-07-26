using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;

namespace Expense.Tracker.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(string id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Check if category with same name exists
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
            }

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                Icon = request.Icon,
                Type = request.Type,
                IsDefault = false
            };

            return await _categoryRepository.CreateAsync(category);
        }

        public async Task<Category?> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
            if (existingCategory == null)
            {
                return null;
            }

            // Check if another category with the same name exists
            if (!string.IsNullOrEmpty(request.Name))
            {
                var nameConflict = await _categoryRepository.GetByNameAsync(request.Name);
                if (nameConflict != null && nameConflict.Id != request.Id)
                {
                    throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
                }
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Name))
                existingCategory.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                existingCategory.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Color))
                existingCategory.Color = request.Color;
            if (!string.IsNullOrEmpty(request.Icon))
                existingCategory.Icon = request.Icon;
            if (request.Type.HasValue)
                existingCategory.Type = request.Type.Value;

            return await _categoryRepository.UpdateAsync(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Don't allow deletion of default categories
            if (category.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete default categories.");
            }

            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(string id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task InitializeDefaultCategoriesAsync()
        {
            var existingCategories = await _categoryRepository.GetAllAsync();
            if (existingCategories.Any())
            {
                return; // Default categories already exist
            }

            foreach (var (name, color, icon, type) in ApiConstants.DefaultCategories.Categories)
            {
                var category = new Category
                {
                    Name = name,
                    Color = color,
                    Icon = icon,
                    Type = type,
                    IsDefault = true
                };

                await _categoryRepository.CreateAsync(category);
            }
        }
    }
}
