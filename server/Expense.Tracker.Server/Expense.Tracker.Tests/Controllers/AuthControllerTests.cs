using Xunit;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Expense.Tracker.Tests.Controllers;

public class AuthControllerTests : BaseTestHelper
{
    private readonly AuthController _controller;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthControllerTests()
    {
        _authService = GetService<IAuthenticationService>();
        _controller = new AuthController(_authService, _logger);
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        await ClearDatabaseAsync();
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
        response.User.Should().NotBeNull();
        response.User.Username.Should().Be(request.Username);
        response.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.Register(request));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new RegisterRequest
        {
            Username = "testuser", // Same as seeded user
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.Register(request));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword" // This should match the seeded user's password
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
        response.User.Should().NotBeNull();
        response.User.Username.Should().Be(request.UsernameOrEmail);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _controller.Login(request));
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "",
            Password = "TestPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.Login(request));
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _controller.Login(request));
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewToken()
    {
        // Arrange
        await SeedTestDataAsync();
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword"
        };
        var loginResult = await _controller.Login(loginRequest);
        var loginResponse = ((OkObjectResult)loginResult.Result!).Value as AuthenticationResult;
        
        var refreshRequest = new RefreshTokenRequest
        {
            Token = loginResponse!.Token
        };

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
        response.Token.Should().NotBe(loginResponse.Token); // Should be a new token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            Token = "invalid-token"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _controller.RefreshToken(request));
    }
}
