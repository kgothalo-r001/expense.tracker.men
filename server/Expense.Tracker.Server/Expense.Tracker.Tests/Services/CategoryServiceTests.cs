using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class CategoryServiceTests : BaseTestHelper
{
    private readonly ICategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryService = GetService<ICategoryService>();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenCategoriesExist_ReturnsUserCategories()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(c => c.Id == TestUserId.ToString());
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenNoCategoriesExist_ReturnsEmptyCollection()
    {
        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(existingCategory.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingCategory.Id);
        result.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidRequest_CreatesAndReturnsCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Test Category",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000",
            Icon = "test-icon",
            Description = "Test description"
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.Color.Should().Be(request.Color);
        result.Icon.Should().Be(request.Icon);
        result.Description.Should().Be(request.Description);
        result.Id.Should().Be(TestUserId.ToString());
        result.IsDefault.Should().BeFalse();

        // Verify in database
        var dbCategory = await DbContext.Categories.FindAsync(result.Id);
        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _categoryService.CreateCategoryAsync(request));
    }

    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();
        var request = new CreateCategoryRequest
        {
            Name = existingCategory.Name,
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _categoryService.CreateCategoryAsync(request));
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidRequest_UpdatesAndReturnsCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();
        var request = new UpdateCategoryRequest
        {
            Id = existingCategory.Id,
            Name = "Updated Category",
            Type = existingCategory.Type,
            Color = "#00FF00",
            Icon = "updated-icon",
            Description = "Updated description"
        };

        // Act
        var result = await _categoryService.UpdateCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingCategory.Id);
        result.Name.Should().Be(request.Name);
        result.Color.Should().Be(request.Color);
        result.Icon.Should().Be(request.Icon);
        result.Description.Should().Be(request.Description);

        // Verify in database
        var dbCategory = await DbContext.Categories.FindAsync(existingCategory.Id);
        dbCategory!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateCategoryRequest
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Updated Category",
            Type = CategoryType.EXPENSE,
            Color = "#00FF00"
        };

        // Act
        var result = await _categoryService.UpdateCategoryAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithValidId_DeletesCategoryAndReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();

        // Act
        var result = await _categoryService.DeleteCategoryAsync(existingCategory.Id);

        // Assert
        result.Should().BeTrue();

        // Verify category is deleted
        var deletedCategory = await DbContext.Categories.FindAsync(existingCategory.Id);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _categoryService.DeleteCategoryAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();

        // Act
        var result = await _categoryService.CategoryExistsAsync(existingCategory.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _categoryService.CategoryExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeDefaultCategoriesAsync_CreatesDefaultCategories()
    {
        // Act
        await _categoryService.InitializeDefaultCategoriesAsync();

        // Assert
        var categories = await _categoryService.GetAllCategoriesAsync();
        categories.Should().NotBeEmpty();
        categories.Should().OnlyContain(c => c.IsDefault == true);
        categories.Should().Contain(c => c.Type == CategoryType.EXPENSE);
        categories.Should().Contain(c => c.Type == CategoryType.INCOME);
    }
}
