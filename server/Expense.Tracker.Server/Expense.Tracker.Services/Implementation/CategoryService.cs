using Expense.Tracker.Services.Abstractions.Constants;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Helpers;
using Expense.Tracker.Services.Implementation.Factories;

namespace Expense.Tracker.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private static readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _categoryRepository.GetByUserIdAsync(userId);
        }

        public async Task<Category?> GetCategoryByIdAsync(string id, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            return await _categoryRepository.GetByUserIdAndIdAsync(userId, id);
        }

        public async Task<Category> CreateCategoryAsync(CreateCategoryRequest request, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            ValidationHelper.ValidateNotNull(request, nameof(request));
            ValidationHelper.ValidateString(request.Name, nameof(request.Name), ValidationHelper.ErrorMessages.CategoryNameRequired);

            var existingCategory = await _categoryRepository.GetByUserIdAndNameAsync(userId, request.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException(string.Format(ValidationHelper.ErrorMessages.CategoryAlreadyExists, request.Name));
            }

            var category = CategoryFactory.CreateCategory(request, userId);
            return await _categoryRepository.CreateAsync(category);
        }

        public async Task<Category?> UpdateCategoryAsync(UpdateCategoryRequest request, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            ValidationHelper.ValidateNotNull(request, nameof(request));
            ValidationHelper.ValidateString(request.Id, nameof(request.Id), "Category ID is required.");

            var existingCategory = await _categoryRepository.GetByUserIdAndIdAsync(userId, request.Id);
            if (existingCategory == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                var nameConflict = await _categoryRepository.GetByUserIdAndNameAsync(userId, request.Name);
                if (nameConflict != null && nameConflict.Id != request.Id)
                {
                    throw new InvalidOperationException(string.Format(ValidationHelper.ErrorMessages.CategoryAlreadyExists, request.Name));
                }
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                existingCategory.Name = request.Name;
            }
            if (!string.IsNullOrEmpty(request.Description))
            {
                existingCategory.Description = request.Description;
            }
            if (!string.IsNullOrEmpty(request.Color))
            {
                existingCategory.Color = request.Color;
            }
            if (!string.IsNullOrEmpty(request.Icon))
            {
                existingCategory.Icon = request.Icon;
            }
            if (request.Type.HasValue)
            {
                existingCategory.Type = request.Type.Value;
            }

            return await _categoryRepository.UpdateAsync(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(string id, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);

            var category = await _categoryRepository.GetByUserIdAndIdAsync(userId, id);
            if (category == null)
            {
                return false;
            }

            if (category.IsDefault)
            {
                throw new InvalidOperationException(ValidationHelper.ErrorMessages.CannotDeleteDefaultCategory);
            }

            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(string id, Requestor requestor)
        {
            var userId = Guid.Parse(requestor.UserId);
            var category = await _categoryRepository.GetByUserIdAndIdAsync(userId, id);
            return category != null;
        }

        public async Task InitializeDefaultCategoriesAsync(Requestor requestor)
        {
            await _initializationSemaphore.WaitAsync();
            try
            {
                var userId = Guid.Parse(requestor.UserId);
                var existingCategories = await _categoryRepository.GetByUserIdAsync(userId);
                if (existingCategories.Any())
                {
                    return;
                }

                var createTasks = ApiConstants.DefaultCategories.Categories
                    .Select(item => 
                    {
                        var category = CategoryFactory.CreateDefaultCategory(item.Name, item.Color, item.Icon, item.Type);
                        category.UserId = userId.ToString();
                        return category;
                    })
                    .Select(category => _categoryRepository.CreateAsync(category));

                await Task.WhenAll(createTasks);
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }
    }
}
