using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class TagServiceTests : BaseTestHelper
{
    private readonly ITagService _tagService;

    public TagServiceTests()
    {
        _tagService = GetService<ITagService>();
    }

    [Fact]
    public async Task GetAllTagsAsync_WhenTagsExist_ReturnsUserTags()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _tagService.GetAllTagsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllTagsAsync_WhenNoTagsExist_ReturnsEmptyCollection()
    {
        // Act
        var result = await _tagService.GetAllTagsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTagByIdAsync_WithValidId_ReturnsTag()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();

        // Act
        var result = await _tagService.GetTagByIdAsync(existingTag.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingTag.Id);
        result.Id.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetTagByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _tagService.GetTagByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateTagAsync_WithValidRequest_CreatesAndReturnsTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "TestTag",
            Color = "Test tag description"
        };

        // Act
        var result = await _tagService.CreateTagAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Color.Should().Be(request.Color);
        result.Id.Should().Be(TestUserId.ToString());
        result.UsageCount.Should().Be(0);

        // Verify in database
        var dbTag = await DbContext.Tags.FindAsync(result.Id);
        dbTag.Should().NotBeNull();
        dbTag!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateTagAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "", // Invalid empty name
            Color = "Test description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _tagService.CreateTagAsync(request));
    }

    [Fact]
    public async Task CreateTagAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();
        var request = new CreateTagRequest
        {
            Name = existingTag.Name, // Duplicate name
            Color = "Test description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _tagService.CreateTagAsync(request));
    }

    [Fact]
    public async Task DeleteTagAsync_WithValidId_DeletesTagAndReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();

        // Act
        var result = await _tagService.DeleteTagAsync(existingTag.Id);

        // Assert
        result.Should().BeTrue();

        // Verify tag is deleted
        var deletedTag = await DbContext.Tags.FindAsync(existingTag.Id);
        deletedTag.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTagAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _tagService.DeleteTagAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTagUsageAsync_WithValidTagName_IncrementsUsageCount()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();
        var originalUsageCount = existingTag.UsageCount;

        // Act
        await _tagService.UpdateTagUsageAsync(existingTag.Name);

        // Assert
        // Refresh from database
        var updatedTag = await DbContext.Tags.FindAsync(existingTag.Id);
        updatedTag.Should().NotBeNull();
        updatedTag!.UsageCount.Should().Be(originalUsageCount + 1);
    }

    [Fact]
    public async Task UpdateTagUsageAsync_WithNonExistentTag_DoesNotThrow()
    {
        // Arrange
        var nonExistentTagName = "nonexistenttag";

        // Act & Assert
        var exception = await Record.ExceptionAsync(
            async () => await _tagService.UpdateTagUsageAsync(nonExistentTagName));
        
        exception.Should().BeNull(); // Should not throw
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithDefaultLimit_ReturnsPopularTags()
    {
        // Arrange
        await SeedTestDataAsync();
        
        // Update usage counts to create popularity
        var tags = DbContext.Tags.ToList();
        tags[0].UsageCount = 10;
        tags[1].UsageCount = 5;
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _tagService.GetPopularTagsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Id == TestUserId.ToString());
        result.Count().Should().BeLessOrEqualTo(10); // Default limit
        
        // Should be ordered by usage count descending
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
        // Arrange
        await SeedTestDataAsync();
        var limit = 1;

        // Act
        var result = await _tagService.GetPopularTagsAsync(limit);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeLessOrEqualTo(limit);
        result.Should().OnlyContain(t => t.Id == TestUserId.ToString());
    }

    [Fact]
    public async Task GetPopularTagsAsync_WhenNoTags_ReturnsEmptyCollection()
    {
        // Act
        var result = await _tagService.GetPopularTagsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserTagsAsync_WithValidUserId_ReturnsUserSpecificTags()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _tagService.GetUserTagsAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Id == TestUserId.ToString());
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUserTagByIdAsync_WithValidIdAndUserId_ReturnsTag()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();

        // Act
        var result = await _tagService.GetUserTagByIdAsync(existingTag.Id, TestUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingTag.Id);
        result.Id.Should().Be(TestUserId.ToString());
    }

    [Fact]
    public async Task GetUserTagByIdAsync_WithDifferentUserId_ReturnsNull()
    {
        // Arrange
        await SeedTestDataAsync();
        var existingTag = DbContext.Tags.First();
        var differentUserId = Guid.NewGuid();

        // Act
        var result = await _tagService.GetUserTagByIdAsync(existingTag.Id, differentUserId);

        // Assert
        result.Should().BeNull(); // Should not return tag for different user
    }

    [Fact]
    public async Task GetUserPopularTagsAsync_WithValidUserId_ReturnsUserSpecificPopularTags()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _tagService.GetUserPopularTagsAsync(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(t => t.Id == TestUserId.ToString());
    }
}
