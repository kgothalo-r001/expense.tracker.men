using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Expense.Tracker.Services.Implementation;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Tests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly CurrentUserService _currentUserService;
    private readonly ClaimsPrincipal _claimsPrincipal;
    private readonly DefaultHttpContext _httpContext;

    public CurrentUserServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContext = new DefaultHttpContext();
        _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "b1e1e1e1-e1e1-e1e1-e1e1-b1e1e1e1e1e1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuthType"));
        _httpContext.User = _claimsPrincipal;
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_httpContext);
        _currentUserService = new CurrentUserService(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetCurrentUserId_WithValidClaim_ReturnsGuid()
    {
        var result = _currentUserService.GetCurrentUserId();
        result.Should().NotBeNull();
        result.Should().Be(Guid.Parse("b1e1e1e1-e1e1-e1e1-e1e1-b1e1e1e1e1e1"));
    }

    [Fact]
    public void GetCurrentUserId_WithMissingClaim_ReturnsNull()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var result = _currentUserService.GetCurrentUserId();
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentUsername_WithValidClaim_ReturnsUsername()
    {
        var result = _currentUserService.GetCurrentUsername();
        result.Should().Be("testuser");
    }

    [Fact]
    public void GetCurrentUsername_WithMissingClaim_ReturnsNull()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var result = _currentUserService.GetCurrentUsername();
        result.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_WhenAuthenticated_ReturnsTrue()
    {
        var result = _currentUserService.IsAuthenticated();
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenNotAuthenticated_ReturnsFalse()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var result = _currentUserService.IsAuthenticated();
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentUser_ReturnsClaimsPrincipal()
    {
        var result = _currentUserService.GetCurrentUser();
        result.Should().BeSameAs(_httpContext.User);
    }

    [Fact]
    public void GetCurrentUser_WhenNoHttpContext_ReturnsNull()
    {
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var result = _currentUserService.GetCurrentUser();
        result.Should().BeNull();
    }
}
