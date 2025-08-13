using Xunit;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Expense.Tracker.Peer.Controllers;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Expense.Tracker.Tests.Controllers;

public class AuthControllerTests : BaseTestHelper
{
    private readonly AuthController _controller;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = DefaultHttpContext
        };
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithAuthResponse()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = true,
            User = new UserDto { Username = request.Username, Email = request.Email },
            Token = "token123"
        };
        _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Register(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.User.Should().NotBeNull();
        response.User.Username.Should().Be(request.Username);
        response.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Passwords do not match"
        };
        _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Register(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ((AuthenticationResult)badRequest.Value).ErrorMessage.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "newuser@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Username already exists"
        };
        _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Register(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ((AuthenticationResult)badRequest.Value).ErrorMessage.Should().Be("Username already exists");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "hashedpassword"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = true,
            User = new UserDto { Username = "testuser" },
            Token = "token123"
        };
        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Login(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.User.Should().NotBeNull();
        response.User.Username.Should().Be(request.UsernameOrEmail);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "nonexistentuser",
            Password = "wrongpassword"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Invalid credentials"
        };
        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Login(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;

        badRequest.Value.Should().NotBeNull();
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal("Invalid credentials", message);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "",
            Password = "TestPassword123!"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Username or email required"
        };
        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Login(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;

        badRequest.Value.Should().NotBeNull();
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal("Username or email required", message);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = ""
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Password required"
        };
        _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResult);

        var result = await _controller.Login(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;

        badRequest.Value.Should().NotBeNull();
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal("Password required", message);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewToken()
    {
        var refreshRequest = new RefreshTokenRequest
        {
            Token = "oldtoken"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = true,
            Token = "newtoken",
            User = new UserDto { Username = "testuser" }
        };
        _mockAuthService.Setup(s => s.RefreshTokenAsync(refreshRequest.Token)).ReturnsAsync(expectedResult);

        var result = await _controller.RefreshToken(refreshRequest);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthenticationResult>().Subject;
        response.Token.Should().Be("newtoken");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsBadRequest()
    {
        var request = new RefreshTokenRequest
        {
            Token = "invalid-token"
        };

        var expectedResult = new AuthenticationResult
        {
            Success = false,
            ErrorMessage = "Invalid token"
        };
        _mockAuthService.Setup(s => s.RefreshTokenAsync(request.Token)).ReturnsAsync(expectedResult);

        var result = await _controller.RefreshToken(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;

        badRequest.Value.Should().NotBeNull();
        var message = Assert.IsType<string>(badRequest.Value);
        Assert.Equal("Invalid token", message);
    }
}
