using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Implementation;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Expense.Tracker.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<TokenService>> _mockLogger;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;

    public TokenServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<TokenService>>();
        _jwtSecret = new string('a', 32); // valid secret
        _jwtExpiryMinutes = 60;
        Environment.SetEnvironmentVariable("JWT_SECRET", _jwtSecret);
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns(_jwtSecret);
        _mockConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns(_jwtExpiryMinutes.ToString());
    }

    [Fact]
    public void Constructor_WithShortSecret_Throws()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "shortsecret");
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("shortsecret");
        Assert.Throws<InvalidOperationException>(() => new TokenService(_mockConfig.Object, _mockLogger.Object));
        Environment.SetEnvironmentVariable("JWT_SECRET", _jwtSecret); // restore
    }

    [Fact]
    public void Constructor_WithMissingSecret_Throws()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", null);
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns((string?)null);
        Assert.Throws<InvalidOperationException>(() => new TokenService(_mockConfig.Object, _mockLogger.Object));
        Environment.SetEnvironmentVariable("JWT_SECRET", _jwtSecret); // restore
    }

    [Fact]
    public void GenerateJwtToken_ReturnsValidToken()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        var userId = Guid.NewGuid();
        var token = service.GenerateJwtToken(userId, "user", "user@email.com");
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Length.Should().Be(3); // JWT format
    }

    [Fact]
    public void ValidateJwtToken_WithValidToken_ReturnsTrue()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        var userId = Guid.NewGuid();
        var token = service.GenerateJwtToken(userId, "user", "user@email.com");
        var result = service.ValidateJwtToken(token);
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateJwtToken_WithInvalidToken_ReturnsFalse()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        var result = service.ValidateJwtToken("invalid.token.value");
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPrincipalFromToken_WithValidToken_ReturnsPrincipal()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        var userId = Guid.NewGuid();
        var token = service.GenerateJwtToken(userId, "user", "user@email.com");
        var principal = service.GetPrincipalFromToken(token);
        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
        principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString()).Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromToken_WithInvalidToken_ReturnsNull()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        var principal = service.GetPrincipalFromToken("invalid.token.value");
        principal.Should().BeNull();
    }

    [Fact]
    public void GetJwtExpiryMinutes_ReturnsConfiguredValue()
    {
        var service = new TokenService(_mockConfig.Object, _mockLogger.Object);
        service.GetJwtExpiryMinutes().Should().Be(_jwtExpiryMinutes);
    }
}
