using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class TagsControllerTests : BaseTestHelper
{
    private readonly TagsController _controller;
    private readonly ITagService _tagService;
    private readonly Mock<ILogger<TagsController>> _mockLogger;

    public TagsControllerTests()
    {
        _tagService = GetService<ITagService>();
        _mockLogger = new Mock<ILogger<TagsController>>();
        _controller = new TagsController(_tagService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTags_WhenTagsExist_ReturnsOkWithTags()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetTags();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetTags_WhenNoTagsExist_ReturnsOkWithEmptyList()
    {
        // Act
        var result = await _controller.GetTags();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTag_WithValidId_ReturnsOkWithTag()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();

        // Act
        var result = await _controller.GetTag(existingTag.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tag = okResult.Value.Should().BeOfType<Tag>().Subject;
        tag.Id.Should().Be(existingTag.Id);
        tag.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTag_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.GetTag(nonExistentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateTag_WithValidRequest_ReturnsCreatedWithTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "TestTag",
            Color = "#FF0000"
        };

        // Act
        var result = await _controller.CreateTag(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var tag = createdResult.Value.Should().BeOfType<Tag>().Subject;
        tag.Name.Should().Be(request.Name);
        tag.Color.Should().Be(request.Color);
        tag.UsageCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTag_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "" // Invalid empty name
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.CreateTag(request));
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();

        // Act
        var result = await _controller.DeleteTag(existingTag.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify tag is deleted
        var deletedTag = await DbContext.Tags.FindAsync(existingTag.Id);
        deletedTag.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTag_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _controller.DeleteTag(nonExistentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPopularTags_ReturnsOkWithPopularTags()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetPopularTags();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPopularTags_WithCustomLimit_ReturnsLimitedTags()
    {
        // Arrange
        await SeedTestDataAsync();
        var limit = 5;

        // Act
        var result = await _controller.GetPopularTags(limit);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Count().Should().BeLessOrEqualTo(limit);
    }
}
