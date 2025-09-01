using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Peer.Helpers;
using Expense.Tracker.Services.Abstractions.Enums;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Expense.Tracker.Tests.Controllers;

public class CategoriesControllerTests : BaseTestHelper
{
    private readonly CategoriesController _controller;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<ILogger<CategoriesController>> _mockLogger;
    private readonly Mock<ITelemetryHelper> _mockTelemetryHelper;

    public CategoriesControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _mockLogger = new Mock<ILogger<CategoriesController>>();
        _mockTelemetryHelper = new Mock<ITelemetryHelper>();
        
        _controller = new CategoriesController(_mockCategoryService.Object, _mockLogger.Object, _mockTelemetryHelper.Object);
    }

    [Fact]
    public async Task GetCategories_WhenCategoriesExist_ReturnsOkWithCategories()
    {
        var expectedCategories = new List<Category>
        {
            new Category { Id = "cat1", Name = "Food", UserId = TestUserId.ToString(), Type = CategoryType.EXPENSE },
            new Category { Id = "cat2", Name = "Transport", UserId = TestUserId.ToString(), Type = CategoryType.EXPENSE }
        };
        _mockCategoryService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<Requestor>())).ReturnsAsync(expectedCategories);

        var result = await _controller.GetCategories();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>().Subject;
        categories.Should().HaveCountGreaterThan(0);
        categories.Should().OnlyContain(c => c.UserId == TestUserId.ToString());
    }

    [Fact]
    public async Task GetCategories_WhenNoCategoriesExist_ReturnsOkWithEmptyList()
    {
        _mockCategoryService.Setup(s => s.GetAllCategoriesAsync(It.IsAny<Requestor>())).ReturnsAsync(new List<Category>());

        var result = await _controller.GetCategories();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<IEnumerable<Category>>().Subject;
        categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsOkWithCategory()
    {
        var categoryId = "cat1";
        var expectedCategory = new Category { Id = categoryId, Name = "Food", UserId = TestUserId.ToString(), Type = CategoryType.EXPENSE };
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(categoryId, It.IsAny<Requestor>())).ReturnsAsync(expectedCategory);

        var result = await _controller.GetCategory(categoryId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var category = okResult.Value.Should().BeOfType<Category>().Subject;
        category.Id.Should().Be(categoryId);
        category.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "cat999";
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync((Category?)null);

        var result = await _controller.GetCategory(nonExistentId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ReturnsCreatedWithCategory()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Test Category",
            Type = CategoryType.EXPENSE,
            Color = "#FF0000",
            Icon = "test",
            Description = "Test description"
        };
        var createdCategory = new Category
        {
            Id = "cat1",
            Name = request.Name,
            Type = request.Type,
            Color = request.Color,
            Icon = request.Icon,
            Description = request.Description,
            UserId = TestUserId.ToString()
        };
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(request, It.IsAny<Requestor>())).ReturnsAsync(createdCategory);

        var result = await _controller.CreateCategory(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var category = createdResult.Value.Should().BeOfType<Category>().Subject;
        category.Name.Should().Be(request.Name);
        category.Type.Should().Be(request.Type);
        category.UserId.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task CreateCategory_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreateCategoryRequest
        {
            Name = "", // Invalid empty name
            Type = CategoryType.EXPENSE,
            Color = "#FF0000"
        };
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(request, It.IsAny<Requestor>())).ThrowsAsync(new InvalidOperationException("Invalid category name"));

        var result = await _controller.CreateCategory(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateCategory_WithValidRequest_ReturnsOkWithUpdatedCategory()
    {
        var request = new UpdateCategoryRequest
        {
            Id = "cat1",
            Name = "Updated Category",
            Type = CategoryType.EXPENSE,
            Color = "#00FF00",
            Icon = "updated",
            Description = "Updated description"
        };
        var updatedCategory = new Category
        {
            Id = request.Id,
            Name = request.Name,
            Type = (CategoryType)request.Type,
            Color = request.Color,
            Icon = request.Icon,
            Description = request.Description,
            UserId = TestUserId.ToString()
        };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(request, It.IsAny<Requestor>())).ReturnsAsync(updatedCategory);

        var result = await _controller.UpdateCategory(request.Id, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var category = okResult.Value.Should().BeOfType<Category>().Subject;
        category.Name.Should().Be(request.Name);
        category.Color.Should().Be(request.Color);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
    {
        var request = new UpdateCategoryRequest
        {
            Id = "cat999",
            Name = "Updated Category",
            Type = CategoryType.EXPENSE,
            Color = "#00FF00"
        };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(request, It.IsAny<Requestor>())).ReturnsAsync((Category?)null);

        var result = await _controller.UpdateCategory(request.Id, request);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent()
    {
        var categoryId = "cat1";
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId, It.IsAny<Requestor>())).ReturnsAsync(true);

        var result = await _controller.DeleteCategory(categoryId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "cat999";
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync(false);

        var result = await _controller.DeleteCategory(nonExistentId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
