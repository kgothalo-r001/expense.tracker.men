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
    private readonly Mock<ITagService> _mockTagService;
    private readonly Mock<ILogger<TagsController>> _mockLogger;

    public TagsControllerTests()
    {
        _mockTagService = new Mock<ITagService>();
        _mockLogger = new Mock<ILogger<TagsController>>();
        _controller = new TagsController(_mockTagService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTags_WhenTagsExist_ReturnsOkWithTags()
    {
        var expectedTags = new List<Tag>
        {
            new Tag { Id = "tag1", Name = "Food", Color = "#FF0000", UsageCount = 2 },
            new Tag { Id = "tag2", Name = "Transport", Color = "#00FF00", UsageCount = 1 }
        };
        _mockTagService.Setup(s => s.GetAllTagsAsync()).ReturnsAsync(expectedTags);

        var result = await _controller.GetTags();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetTags_WhenNoTagsExist_ReturnsOkWithEmptyList()
    {
        _mockTagService.Setup(s => s.GetAllTagsAsync()).ReturnsAsync(new List<Tag>());

        var result = await _controller.GetTags();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTag_WithValidId_ReturnsOkWithTag()
    {
        var tagId = "tag1";
        var expectedTag = new Tag { Id = tagId, Name = "Food", Color = "#FF0000", UsageCount = 2 };
        _mockTagService.Setup(s => s.GetTagByIdAsync(tagId, It.IsAny<Requestor>())).ReturnsAsync(expectedTag);

        var result = await _controller.GetTag(tagId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tag = okResult.Value.Should().BeOfType<Tag>().Subject;
        tag.Id.Should().Be(tagId);
        tag.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTag_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "tag999";
        _mockTagService.Setup(s => s.GetTagByIdAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync((Tag?)null);

        var result = await _controller.GetTag(nonExistentId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateTag_WithValidRequest_ReturnsCreatedWithTag()
    {
        var request = new CreateTagRequest
        {
            Name = "TestTag",
            Color = "#FF0000"
        };
        var createdTag = new Tag
        {
            Id = "tag1",
            Name = request.Name,
            Color = request.Color,
            UsageCount = 0
        };
        _mockTagService.Setup(s => s.CreateTagAsync(request, It.IsAny<Requestor>())).ReturnsAsync(createdTag);

        var result = await _controller.CreateTag(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var tag = createdResult.Value.Should().BeOfType<Tag>().Subject;
        tag.Name.Should().Be(request.Name);
        tag.Color.Should().Be(request.Color);
        tag.UsageCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTag_WithInvalidRequest_ReturnsBadRequest()
    {
        var request = new CreateTagRequest
        {
            Name = "" // Invalid empty name
        };
        _mockTagService.Setup(s => s.CreateTagAsync(request, It.IsAny<Requestor>())).ThrowsAsync(new InvalidOperationException("Invalid tag name"));

        var result = await _controller.CreateTag(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ReturnsNoContent()
    {
        var tagId = "tag1";
        _mockTagService.Setup(s => s.DeleteTagAsync(tagId, It.IsAny<Requestor>())).ReturnsAsync(true);

        var result = await _controller.DeleteTag(tagId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTag_WithInvalidId_ReturnsNotFound()
    {
        var nonExistentId = "tag999";
        _mockTagService.Setup(s => s.DeleteTagAsync(nonExistentId, It.IsAny<Requestor>())).ReturnsAsync(false);

        var result = await _controller.DeleteTag(nonExistentId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPopularTags_ReturnsOkWithPopularTags()
    {
        var expectedTags = new List<Tag>
        {
            new Tag { Id = "tag1", Name = "Food", Color = "#FF0000", UsageCount = 2 },
            new Tag { Id = "tag2", Name = "Transport", Color = "#00FF00", UsageCount = 1 }
        };
        _mockTagService.Setup(s => s.GetPopularTagsAsync(It.IsAny<Requestor>(), 10)).ReturnsAsync(expectedTags);

        var result = await _controller.GetPopularTags();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPopularTags_WithCustomLimit_ReturnsLimitedTags()
    {
        var limit = 5;
        var expectedTags = new List<Tag>
        {
            new Tag { Id = "tag1", Name = "Food", Color = "#FF0000", UsageCount = 2 },
            new Tag { Id = "tag2", Name = "Transport", Color = "#00FF00", UsageCount = 1 }
        };
        _mockTagService.Setup(s => s.GetPopularTagsAsync(It.IsAny<Requestor>(), limit)).ReturnsAsync(expectedTags);

        var result = await _controller.GetPopularTags(limit);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tags = okResult.Value.Should().BeAssignableTo<IEnumerable<Tag>>().Subject;
        tags.Count().Should().BeLessOrEqualTo(limit);
    }
}
