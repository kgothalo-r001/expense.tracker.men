using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;

namespace Expense.Tracker.Tests.Services;

public class AuthenticationServiceTests : BaseTestHelper
{
    private readonly IAuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _authService = GetService<IAuthenticationService>();
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(request.Username);
        result.User.Email.Should().Be(request.Email);
        result.User.IsActive.Should().BeTrue();

        // Verify user is created in database
        var dbUser = DbContext.Users.FirstOrDefault(u => u.Username == request.Username);
        dbUser.Should().NotBeNull();
        dbUser!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithMismatchedPasswords_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new RegisterRequest
        {
            Username = "testuser", // Same as seeded user
            Email = "different@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new RegisterRequest
        {
            Username = "differentuser",
            Email = "test@example.com", // Same as seeded user
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "invalid-email",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword" // Note: This depends on how password hashing is implemented
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User?.Username.Should().Be(request.UsernameOrEmail);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "anypassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        await SeedTestDataAsync();
        var user = DbContext.Users.First();
        user.IsActive = false;
        await DbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            UsernameOrEmail = user.Username,
            Password = "hashedpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "",
            Password = "password"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        await SeedTestDataAsync();
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword"
        };
        var loginResponse = await _authService.LoginAsync(loginRequest);
        
        var refreshRequest = new RefreshTokenRequest
        {
            Token = loginResponse?.Token
        };

        // Act
        var result = await _authService.RefreshTokenAsync(refreshRequest?.Token);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Token.Should().NotBe(loginResponse?.Token); // Should be a new token
        result.User.Should().NotBeNull();
        result.User?.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            Token = "invalid-token"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.RefreshTokenAsync(request?.Token));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        await SeedTestDataAsync();
        
        // Create an expired session
        var user = DbContext.Users.First();
        var expiredSession = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "expired-token",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsActive = true
        };
        DbContext.UserSessions.Add(expiredSession);
        await DbContext.SaveChangesAsync();

        var request = new RefreshTokenRequest
        {
            Token = expiredSession.Token
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.RefreshTokenAsync(request.Token));
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword"
        };
        var loginResponse = await _authService.LoginAsync(loginRequest);

        // Act
        var result = await _authService.ValidateTokenAsync(loginResponse.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid-token";

        // Act
        var result = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
    }
}
