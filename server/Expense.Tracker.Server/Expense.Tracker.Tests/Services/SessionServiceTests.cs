using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<IUserSessionRepository> _mockSessionRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<SessionService>> _mockLogger;
    private readonly SessionService _sessionService;
    private readonly Guid _userId;
    private readonly string _token;

    public SessionServiceTests()
    {
        _mockSessionRepo = new Mock<IUserSessionRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<SessionService>>();
        _sessionService = new SessionService(_mockSessionRepo.Object, _mockTokenService.Object, _mockLogger.Object);
        _userId = Guid.NewGuid();
        _token = "test-token";
    }

    [Fact]
    public async Task CreateSessionAsync_CreatesAndReturnsSession()
    {
        _mockTokenService.Setup(t => t.GetJwtExpiryMinutes()).Returns(60);
        var expectedSession = new UserSession { UserId = _userId, Token = _token, ExpiresAt = DateTime.UtcNow.AddMinutes(60), IsActive = true };
        _mockSessionRepo.Setup(r => r.CreateSessionAsync(It.IsAny<UserSession>())).ReturnsAsync(expectedSession);
        var result = await _sessionService.CreateSessionAsync(_userId, _token);
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.Token.Should().Be(_token);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSessionAsync_ValidSessionAndToken_ReturnsTrue()
    {
        _mockSessionRepo.Setup(r => r.ValidateSessionAsync(_token)).ReturnsAsync(true);
        _mockTokenService.Setup(t => t.ValidateJwtToken(_token)).Returns(true);
        var result = await _sessionService.ValidateSessionAsync(_token);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSessionAsync_InvalidSession_ReturnsFalse()
    {
        _mockSessionRepo.Setup(r => r.ValidateSessionAsync(_token)).ReturnsAsync(false);
        var result = await _sessionService.ValidateSessionAsync(_token);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateSessionAsync_InvalidToken_ReturnsFalse()
    {
        _mockSessionRepo.Setup(r => r.ValidateSessionAsync(_token)).ReturnsAsync(true);
        _mockTokenService.Setup(t => t.ValidateJwtToken(_token)).Returns(false);
        var result = await _sessionService.ValidateSessionAsync(_token);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserBySessionTokenAsync_ReturnsUser()
    {
        var user = new User { Id = _userId, Username = "testuser" };
        _mockSessionRepo.Setup(r => r.GetUserBySessionTokenAsync(_token)).ReturnsAsync(user);
        var result = await _sessionService.GetUserBySessionTokenAsync(_token);
        result.Should().NotBeNull();
        result!.Id.Should().Be(_userId);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserBySessionTokenAsync_WhenException_ReturnsNull()
    {
        _mockSessionRepo.Setup(r => r.GetUserBySessionTokenAsync(_token)).ThrowsAsync(new Exception("fail"));
        var result = await _sessionService.GetUserBySessionTokenAsync(_token);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateSessionAsync_ReturnsTrue()
    {
        _mockSessionRepo.Setup(r => r.DeactivateSessionAsync(_token)).ReturnsAsync(true);
        var result = await _sessionService.DeactivateSessionAsync(_token);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateSessionAsync_WhenException_ReturnsFalse()
    {
        _mockSessionRepo.Setup(r => r.DeactivateSessionAsync(_token)).ThrowsAsync(new Exception("fail"));
        var result = await _sessionService.DeactivateSessionAsync(_token);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSessionTokenAsync_ReturnsUpdatedSession()
    {
        var newToken = "new-token";
        var sessionId = Guid.NewGuid();
        var updatedSession = new UserSession { Id = sessionId, UserId = _userId, Token = newToken };
        _mockSessionRepo.Setup(r => r.UpdateSessionTokenAsync(sessionId, newToken)).ReturnsAsync(updatedSession);
        var result = await _sessionService.UpdateSessionTokenAsync(sessionId, newToken);
        result.Should().NotBeNull();
        result!.Id.Should().Be(sessionId);
        result.Token.Should().Be(newToken);
    }

    [Fact]
    public async Task UpdateSessionTokenAsync_WhenException_ReturnsNull()
    {
        var sessionId = Guid.NewGuid();
        var newToken = "new-token";
        _mockSessionRepo.Setup(r => r.UpdateSessionTokenAsync(sessionId, newToken)).ThrowsAsync(new Exception("fail"));
        var result = await _sessionService.UpdateSessionTokenAsync(sessionId, newToken);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshSessionAsync_RefreshesSession()
    {
        var oldToken = "old-token";
        var newToken = "new-token";
        var expectedSession = new UserSession { UserId = _userId, Token = newToken, IsActive = true };
        _mockSessionRepo.Setup(r => r.DeactivateSessionAsync(oldToken)).ReturnsAsync(true);
        _mockTokenService.Setup(t => t.GetJwtExpiryMinutes()).Returns(60);
        _mockSessionRepo.Setup(r => r.CreateSessionAsync(It.IsAny<UserSession>())).ReturnsAsync(expectedSession);
        var result = await _sessionService.RefreshSessionAsync(oldToken, newToken, _userId);
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.Token.Should().Be(newToken);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshSessionAsync_WhenException_Throws()
    {
        var oldToken = "old-token";
        var newToken = "new-token";
        _mockSessionRepo.Setup(r => r.DeactivateSessionAsync(oldToken)).ThrowsAsync(new Exception("fail"));
        await Assert.ThrowsAsync<Exception>(async () => await _sessionService.RefreshSessionAsync(oldToken, newToken, _userId));
    }
}
