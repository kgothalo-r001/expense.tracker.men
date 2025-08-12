using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Tests.Helpers;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class CategoriesControllerTests : BaseTestHelper
{
    private readonly CategoriesController _controller;
    private readonly ICategoryService _categoryService;
    private readonly Mock<ILogger<CategoriesController>> _mockLogger;

    public CategoriesControllerTests()
    {
        _categoryService = GetService<ICategoryService>();
        _mockLogger = new Mock<ILogger<CategoriesController>>();
        _controller = new CategoriesController(_categoryService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCategories_WhenCategoriesExist_ReturnsOkWithCategories()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>().Subject;
        categories.Should().HaveCountGreaterThan(0);
        categories.Should().OnlyContain(c => c.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetCategories_WhenNoCategoriesExist_ReturnsOkWithEmptyList()
    {
        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>().Subject;
        categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsOkWithCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();

        // Act
        var result = await _controller.GetCategory(existingCategory.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var category = okResult.Value.Should().BeOfType<Category>().Subject;
        category.Id.Should().Be(existingCategory.Id);
        category.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.GetCategory(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreatedWithCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Test Category",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000",
            Icon = "test",
            Description = "Test description"
        };

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var category = createdResult.Value.Should().BeOfType<Category>().Subject;
        category.Name.Should().Be(request.Name);
        category.Type.Should().Be(request.Type);
        category.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task CreateCategory_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "", // Invalid empty name
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.CreateCategory(request));
    }

    [Fact]
    public async Task UpdateCategory_WithValidRequest_ReturnsOkWithUpdatedCategory()
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
            Icon = "updated",
            Description = "Updated description"
        };

        // Act
        var result = await _controller.UpdateCategory(existingCategory.Id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var category = okResult.Value.Should().BeOfType<Category>().Subject;
        category.Name.Should().Be(request.Name);
        category.Color.Should().Be(request.Color);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
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
        var result = await _controller.UpdateCategory(request.Id, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingCategory = DbContext.Categories.First();

        // Act
        var result = await _controller.DeleteCategory(existingCategory.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify category is deleted
        var deletedCategory = await DbContext.Categories.FindAsync(existingCategory.Id);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.DeleteCategory(nonExistentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
