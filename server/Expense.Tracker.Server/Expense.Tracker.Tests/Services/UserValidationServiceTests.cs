using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Tests.Services;

public class UserValidationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ILogger<UserValidationService>> _mockLogger;
    private readonly UserValidationService _service;

    public UserValidationServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserValidationService>>();
        _service = new UserValidationService(_mockUserRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_ReturnsTrue()
    {
        _mockUserRepo.Setup(r => r.IsUsernameAvailableAsync("user1")).ReturnsAsync(true);
        var result = await _service.IsUsernameAvailableAsync("user1");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUsernameAvailableAsync_WhenException_ReturnsFalse()
    {
        _mockUserRepo.Setup(r => r.IsUsernameAvailableAsync("user1")).ThrowsAsync(new Exception("fail"));
        var result = await _service.IsUsernameAvailableAsync("user1");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_ReturnsTrue()
    {
        _mockUserRepo.Setup(r => r.IsEmailAvailableAsync("test@email.com")).ReturnsAsync(true);
        var result = await _service.IsEmailAvailableAsync("test@email.com");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_WhenException_ReturnsFalse()
    {
        _mockUserRepo.Setup(r => r.IsEmailAvailableAsync("test@email.com")).ThrowsAsync(new Exception("fail"));
        var result = await _service.IsEmailAvailableAsync("test@email.com");
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ReturnsHashedPassword()
    {
        var password = "password123";
        var hash = _service.HashPassword(password);
        hash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify(password, hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithValidPassword_ReturnsTrue()
    {
        var password = "password123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var result = _service.VerifyPassword(password, hash);
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithInvalidPassword_ReturnsFalse()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var result = _service.VerifyPassword("wrongpassword", hash);
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithCorruptedHash_ReturnsFalse()
    {
        var result = _service.VerifyPassword("password123", "corruptedhash");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SuggestUsernamesAsync_ReturnsSuggestions()
    {
        _mockUserRepo.Setup(r => r.IsUsernameAvailableAsync(It.IsAny<string>())).ReturnsAsync(true);
        var result = await _service.SuggestUsernamesAsync("baseuser");
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterOrEqualTo(1);
        result.Count.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task SuggestUsernamesAsync_WhenException_ReturnsBasicSuggestions()
    {
        _mockUserRepo.Setup(r => r.IsUsernameAvailableAsync(It.IsAny<string>())).ThrowsAsync(new Exception("fail"));
        var result = await _service.SuggestUsernamesAsync("baseuser");
        result.Should().Contain("baseuser1");
        result.Should().Contain("baseuser2");
        result.Should().Contain("baseuser_user");
    }

    [Fact]
    public void Dispose_DisposesRandomGenerator()
    {
        _service.Dispose();
        // No exception should be thrown
    }
}
