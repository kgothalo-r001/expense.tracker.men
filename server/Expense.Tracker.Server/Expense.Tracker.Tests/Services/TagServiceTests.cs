using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Implementation;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class TagServiceTests
{
    private readonly TagService _tagService;
    private readonly Mock<ITagRepository> _mockTagRepo;
    private readonly Requestor _requestor;

    public TagServiceTests()
    {
        _mockTagRepo = new Mock<ITagRepository>();
        _requestor = new Requestor { UserId = Guid.NewGuid().ToString() };
        _tagService = new TagService(_mockTagRepo.Object);
    }

    [Fact]
    public async Task GetAllTagsAsync_WhenTagsExist_ReturnsTags()
    {
        var tags = new List<Tag> { new Tag { Id = "tag1", Name = "Test", Color = "red", UsageCount = 1 } };
        _mockTagRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(tags);
        var result = await _tagService.GetAllTagsAsync();
        result.Should().NotBeNull();
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllTagsAsync_WhenNoTagsExist_ReturnsEmptyCollection()
    {
        _mockTagRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tag>());
        var result = await _tagService.GetAllTagsAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTagByIdAsync_WithValidId_ReturnsTag()
    {
        var tag = new Tag { Id = "tag1", Name = "Test", Color = "red", UsageCount = 1 };
        _mockTagRepo.Setup(r => r.GetByIdAsync(tag.Id)).ReturnsAsync(tag);
        var result = await _tagService.GetTagByIdAsync(tag.Id, _requestor);
        result.Should().NotBeNull();
        result!.Id.Should().Be(tag.Id);
    }

    [Fact]
    public async Task GetTagByIdAsync_WithInvalidId_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        _mockTagRepo.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Tag?)null);
        var result = await _tagService.GetTagByIdAsync(nonExistentId, _requestor);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateTagAsync_WithValidRequest_CreatesAndReturnsTag()
    {
        var request = new CreateTagRequest { Name = "TestTag", Color = "red" };
        _mockTagRepo.Setup(r => r.GetByNameAsync(request.Name)).ReturnsAsync((Tag?)null);
        var createdTag = new Tag { Id = "tag1", Name = request.Name, Color = request.Color, UsageCount = 0 };
        _mockTagRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>())).ReturnsAsync(createdTag);
        var result = await _tagService.CreateTagAsync(request, _requestor);
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Color.Should().Be(request.Color);
        result.UsageCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTagAsync_WithEmptyName_ThrowsArgumentException()
    {
        var request = new CreateTagRequest { Name = "", Color = "red" };
        // Simulate validation in service (if implemented)
        // If not implemented, this test will fail until validation is added
        await Assert.ThrowsAsync<ArgumentException>(async () => await _tagService.CreateTagAsync(request, _requestor));
    }

    [Fact]
    public async Task CreateTagAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var existingTag = new Tag { Id = "tag1", Name = "TestTag", Color = "red", UsageCount = 0 };
        var request = new CreateTagRequest { Name = existingTag.Name, Color = "red" };
        _mockTagRepo.Setup(r => r.GetByNameAsync(request.Name)).ReturnsAsync(existingTag);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _tagService.CreateTagAsync(request, _requestor));
    }

    [Fact]
    public async Task DeleteTagAsync_WithValidId_DeletesTagAndReturnsTrue()
    {
        var tagId = "tag1";
        _mockTagRepo.Setup(r => r.DeleteAsync(tagId)).ReturnsAsync(true);
        var result = await _tagService.DeleteTagAsync(tagId, _requestor);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTagAsync_WithInvalidId_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        _mockTagRepo.Setup(r => r.DeleteAsync(nonExistentId)).ReturnsAsync(false);
        var result = await _tagService.DeleteTagAsync(nonExistentId, _requestor);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTagUsageAsync_WithValidTagName_CallsIncrementUsage()
    {
        var tagName = "TestTag";
        _mockTagRepo.Setup(r => r.IncrementUsageAsync(tagName)).Returns(Task.CompletedTask).Verifiable();
        await _tagService.UpdateTagUsageAsync(tagName, _requestor);
        _mockTagRepo.Verify(r => r.IncrementUsageAsync(tagName), Times.Once);
    }

    [Fact]
    public async Task UpdateTagUsageAsync_WithNonExistentTag_DoesNotThrow()
    {
        var nonExistentTagName = "nonexistenttag";
        _mockTagRepo.Setup(r => r.IncrementUsageAsync(nonExistentTagName)).Returns(Task.CompletedTask);
        var exception = await Record.ExceptionAsync(async () => await _tagService.UpdateTagUsageAsync(nonExistentTagName, _requestor));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithDefaultLimit_ReturnsPopularTags()
    {
        var tags = new List<Tag>
        {
            new Tag { Id = "tag1", Name = "Test1", UsageCount = 10 },
            new Tag { Id = "tag2", Name = "Test2", UsageCount = 5 }
        };
        _mockTagRepo.Setup(r => r.GetPopularAsync(10)).ReturnsAsync(tags);
        var result = await _tagService.GetPopularTagsAsync(_requestor);
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(10);
        var resultList = result.ToList();
        if (resultList.Count > 1)
        {
            for (int i = 0; i < resultList.Count - 1; i++)
            {
                resultList[i].UsageCount.Should().BeGreaterOrEqualTo(resultList[i + 1].UsageCount);
            }
        }
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithCustomLimit_ReturnsLimitedTags()
    {
        var limit = 1;
        var tags = new List<Tag> { new Tag { Id = "tag1", Name = "Test1", UsageCount = 10 } };
        _mockTagRepo.Setup(r => r.GetPopularAsync(limit)).ReturnsAsync(tags);
        var result = await _tagService.GetPopularTagsAsync(_requestor, limit);
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(limit);
    }

    [Fact]
    public async Task GetPopularTagsAsync_WhenNoTags_ReturnsEmptyCollection()
    {
        _mockTagRepo.Setup(r => r.GetPopularAsync(10)).ReturnsAsync(new List<Tag>());
        var result = await _tagService.GetPopularTagsAsync(_requestor);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // The following tests are for methods not present in TagService and have been removed for clarity and correctness.
}
