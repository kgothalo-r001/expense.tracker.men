using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Services.Implementation;

public class TokenService : ITokenService
{
    private const int MinJwtSecretLength = 32;
    private const int DefaultJwtExpiryMinutes = 60;

    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Try to get JWT secret from environment variable first, then configuration
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? _configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured. Set JWT_SECRET environment variable or Jwt:Secret in configuration.");
            
        // Validate JWT secret strength
        if (_jwtSecret.Length < MinJwtSecretLength)
        {
            throw new InvalidOperationException($"JWT Secret must be at least {MinJwtSecretLength} characters long for security.");
        }
        
        if (!int.TryParse(_configuration["Jwt:ExpiryMinutes"] ?? DefaultJwtExpiryMinutes.ToString(), out _jwtExpiryMinutes))
        {
            _jwtExpiryMinutes = DefaultJwtExpiryMinutes;
            _logger.LogWarning("Failed to parse JWT expiry minutes from configuration. Using default value: {DefaultValue}", DefaultJwtExpiryMinutes);
        }
    }

    public string GenerateJwtToken(Guid userId, string username, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("id", userId.ToString()),
                new Claim("username", username),
                new Claim("email", email),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return validatedToken != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return false;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract principal from token");
            return null;
        }
    }

    public int GetJwtExpiryMinutes() => _jwtExpiryMinutes;
}
