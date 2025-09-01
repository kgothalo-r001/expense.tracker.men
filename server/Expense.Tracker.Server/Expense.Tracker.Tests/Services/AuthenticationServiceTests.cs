using Xunit;
using FluentAssertions;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Implementation;
using Microsoft.Extensions.Logging;
using Moq;

namespace Expense.Tracker.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly AuthenticationService _authService;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IUserValidationService> _mockUserValidationService;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;

    public AuthenticationServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockSessionService = new Mock<ISessionService>();
        _mockUserValidationService = new Mock<IUserValidationService>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _authService = new AuthenticationService(
            _mockUserRepo.Object,
            _mockTokenService.Object,
            _mockSessionService.Object,
            _mockUserValidationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailValid(request.Email)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.HashPassword(request.Password)).Returns("hashed");
        var user = new User { Id = Guid.NewGuid(), Username = request.Username, Email = request.Email, PasswordHash = "hashed", IsActive = true };
        _mockUserRepo.Setup(s => s.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateJwtToken(user.Id, user.Username, user.Email)).Returns("token123");
        _mockSessionService.Setup(s => s.CreateSessionAsync(user.Id, "token123")).ReturnsAsync(new UserSession { Id = Guid.NewGuid(), UserId = user.Id, Token = "token123" });

        var result = await _authService.RegisterAsync(request);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(request.Username);
        result.User!.Email.Should().Be(request.Email);
        result.User!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WithMismatchedPasswords_ReturnsValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "different@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(false);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        result.ValidationErrors.Should().Contain("Username is already taken");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "differentuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(false);

        var result = await _authService.RegisterAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        result.ValidationErrors.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyUsername_ReturnsValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ReturnsValidationError()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "invalid-email",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };
        _mockUserValidationService.Setup(s => s.IsUsernameAvailableAsync(request.Username)).ReturnsAsync(true);
        _mockUserValidationService.Setup(s => s.IsEmailAvailableAsync(request.Email)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword"
        };
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hashedpassword", IsActive = true };
        _mockUserRepo.Setup(s => s.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail)).ReturnsAsync(user);
        _mockUserValidationService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
        _mockTokenService.Setup(s => s.GenerateJwtToken(user.Id, user.Username, user.Email)).Returns("token123");
        _mockSessionService.Setup(s => s.CreateSessionAsync(user.Id, "token123")).ReturnsAsync(new UserSession { Id = Guid.NewGuid(), UserId = user.Id, Token = "token123" });
        _mockTokenService.Setup(s => s.GenerateJwtToken(user.Id, user.Username, user.Email, It.IsAny<string>())).Returns("token456");
        _mockSessionService.Setup(s => s.UpdateSessionTokenAsync(It.IsAny<Guid>(), "token456"))
            .ReturnsAsync(new UserSession { Id = Guid.NewGuid() });

        var result = await _authService.LoginAsync(request);
        result.Should().NotBeNull();
        result.Token.Should().Be("token456");
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(request.UsernameOrEmail);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsError()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "anypassword"
        };
        _mockUserRepo.Setup(s => s.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail)).ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username/email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "wrongpassword"
        };
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hashedpassword", IsActive = true };
        _mockUserRepo.Setup(s => s.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail)).ReturnsAsync(user);
        _mockUserValidationService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

        var result = await _authService.LoginAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username/email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsError()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "inactiveuser",
            Password = "hashedpassword"
        };
        var user = new User { Id = Guid.NewGuid(), Username = "inactiveuser", Email = "test@example.com", PasswordHash = "hashedpassword", IsActive = false };
        _mockUserRepo.Setup(s => s.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail)).ReturnsAsync(user);
        _mockUserValidationService.Setup(s => s.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);

        var result = await _authService.LoginAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account is inactive");
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsername_ReturnsError()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "",
            Password = "password"
        };

        var result = await _authService.LoginAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username or email is required");
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ReturnsError()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = ""
        };

        var result = await _authService.LoginAsync(request);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password is required");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        var token = "oldtoken";
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hashedpassword", IsActive = true };
        _mockSessionService.Setup(s => s.ValidateSessionAsync(token)).ReturnsAsync(true);
        _mockSessionService.Setup(s => s.GetUserBySessionTokenAsync(token)).ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateJwtToken(user.Id, user.Username, user.Email)).Returns("newtoken");
        _mockSessionService.Setup(s => s.RefreshSessionAsync(token, "newtoken", user.Id)).ReturnsAsync(new UserSession { Id = Guid.NewGuid(), UserId = user.Id, Token = "newtoken" });

        var result = await _authService.RefreshTokenAsync(token);
        result.Should().NotBeNull();
        result.Token.Should().Be("newtoken");
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ReturnsError()
    {
        var token = "invalid-token";
        _mockSessionService.Setup(s => s.ValidateSessionAsync(token)).ReturnsAsync(false);

        var result = await _authService.RefreshTokenAsync(token);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid or expired token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsError()
    {
        var token = "expired-token";
        _mockSessionService.Setup(s => s.ValidateSessionAsync(token)).ReturnsAsync(false);

        var result = await _authService.RefreshTokenAsync(token);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid or expired token");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        var token = "valid-token";
        _mockSessionService.Setup(s => s.ValidateSessionAsync(token)).ReturnsAsync(true);

        var result = await _authService.ValidateTokenAsync(token);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        var invalidToken = "invalid-token";
        _mockSessionService.Setup(s => s.ValidateSessionAsync(invalidToken)).ReturnsAsync(false);

        var result = await _authService.ValidateTokenAsync(invalidToken);
        result.Should().BeFalse();
    }
}
