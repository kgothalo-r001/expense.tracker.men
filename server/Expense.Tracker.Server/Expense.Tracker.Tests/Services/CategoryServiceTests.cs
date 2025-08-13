using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Implementation;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class CategoryServiceTests
{
    private readonly CategoryService _categoryService;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Requestor _requestor;

    public CategoryServiceTests()
    {
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _requestor = new Requestor { UserId = Guid.NewGuid().ToString() };
        _categoryService = new CategoryService(_mockCategoryRepo.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenCategoriesExist_ReturnsUserCategories()
    {
        var categories = new List<Category>
        {
            new Category { Id = "cat1", UserId = _requestor.UserId },
            new Category { Id = "cat2", UserId = _requestor.UserId }
        };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(categories);

        var result = await _categoryService.GetAllCategoriesAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().OnlyContain(c => c.UserId == _requestor.UserId);
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenNoCategoriesExist_ReturnsEmptyCollection()
    {
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Category>());

        var result = await _categoryService.GetAllCategoriesAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
    {
        var categoryId = "cat1";
        var category = new Category { Id = categoryId, UserId = _requestor.UserId };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), categoryId)).ReturnsAsync(category);

        var result = await _categoryService.GetCategoryByIdAsync(categoryId, _requestor);
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.UserId.Should().Be(_requestor.UserId);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsNull()
    {
        var nonExistentId = "cat999";
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), nonExistentId)).ReturnsAsync((Category?)null);

        var result = await _categoryService.GetCategoryByIdAsync(nonExistentId, _requestor);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidRequest_CreatesAndReturnsCategory()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Test Category",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000",
            Icon = "test-icon",
            Description = "Test description"
        };
        var category = new Category
        {
            Id = "cat1",
            Name = request.Name,
            Type = request.Type,
            Color = request.Color,
            Icon = request.Icon,
            Description = request.Description,
            UserId = _requestor.UserId,
            IsDefault = false
        };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndNameAsync(It.IsAny<Guid>(), request.Name)).ReturnsAsync((Category?)null);
        _mockCategoryRepo.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(category);

        var result = await _categoryService.CreateCategoryAsync(request, _requestor);
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.Color.Should().Be(request.Color);
        result.Icon.Should().Be(request.Icon);
        result.Description.Should().Be(request.Description);
        result.UserId.Should().Be(_requestor.UserId);
        result.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCategoryAsync_WithEmptyName_ThrowsArgumentException()
    {
        var request = new CreateCategoryRequest
        {
            Name = "",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _categoryService.CreateCategoryAsync(request, _requestor));
    }

    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Duplicate",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };
        var existingCategory = new Category { Id = "cat1", Name = request.Name, UserId = _requestor.UserId };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndNameAsync(It.IsAny<Guid>(), request.Name)).ReturnsAsync(existingCategory);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _categoryService.CreateCategoryAsync(request, _requestor));
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidRequest_UpdatesAndReturnsCategory()
    {
        var request = new UpdateCategoryRequest
        {
            Id = "cat1",
            Name = "Updated Category",
            Type = CategoryType.EXPENSE,
            Color = "#00FF00",
            Icon = "updated-icon",
            Description = "Updated description"
        };
        var existingCategory = new Category { Id = request.Id, Name = "Old Name", UserId = _requestor.UserId };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), request.Id)).ReturnsAsync(existingCategory);
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndNameAsync(It.IsAny<Guid>(), request.Name)).ReturnsAsync((Category?)null);
        _mockCategoryRepo.Setup(r => r.UpdateAsync(It.IsAny<Category>())).ReturnsAsync(existingCategory);

        var result = await _categoryService.UpdateCategoryAsync(request, _requestor);
        result.Should().NotBeNull();
        result!.Id.Should().Be(request.Id);
        result.Name.Should().Be(request.Name);
        result.Color.Should().Be(request.Color);
        result.Icon.Should().Be(request.Icon);
        result.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithInvalidId_ReturnsNull()
    {
        var request = new UpdateCategoryRequest
        {
            Id = "cat999",
            Name = "Updated Category",
            Type = CategoryType.EXPENSE,
            Color = "#00FF00"
        };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), request.Id)).ReturnsAsync((Category?)null);

        var result = await _categoryService.UpdateCategoryAsync(request, _requestor);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithValidId_DeletesCategoryAndReturnsTrue()
    {
        var categoryId = "cat1";
        var category = new Category { Id = categoryId, UserId = _requestor.UserId, IsDefault = false };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), categoryId)).ReturnsAsync(category);
        _mockCategoryRepo.Setup(r => r.DeleteAsync(categoryId)).ReturnsAsync(true);

        var result = await _categoryService.DeleteCategoryAsync(categoryId, _requestor);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithInvalidId_ReturnsFalse()
    {
        var nonExistentId = "cat999";
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), nonExistentId)).ReturnsAsync((Category?)null);

        var result = await _categoryService.DeleteCategoryAsync(nonExistentId, _requestor);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithExistingId_ReturnsTrue()
    {
        var categoryId = "cat1";
        var category = new Category { Id = categoryId, UserId = _requestor.UserId };
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), categoryId)).ReturnsAsync(category);

        var result = await _categoryService.CategoryExistsAsync(categoryId, _requestor);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        var nonExistentId = "cat999";
        _mockCategoryRepo.Setup(r => r.GetByUserIdAndIdAsync(It.IsAny<Guid>(), nonExistentId)).ReturnsAsync((Category?)null);

        var result = await _categoryService.CategoryExistsAsync(nonExistentId, _requestor);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeDefaultCategoriesAsync_CreatesDefaultCategories()
    {
        _mockCategoryRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Category>());
        _mockCategoryRepo.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

        await _categoryService.InitializeDefaultCategoriesAsync(_requestor);
        _mockCategoryRepo.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.AtLeastOnce());
    }
}
