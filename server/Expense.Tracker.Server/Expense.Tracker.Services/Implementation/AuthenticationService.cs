using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Implementation;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;

    public AuthenticationService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _configuration = configuration;
        
        // Try to get JWT secret from environment variable first, then configuration
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? _configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured. Set JWT_SECRET environment variable or Jwt:Secret in configuration.");
            
        // Validate JWT secret strength
        if (_jwtSecret.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long for security.");
        }
        
        _jwtExpiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username/email or password"
                };
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username/email or password"
                };
            }

            var token = GenerateJwtToken(user.Id, user.Username, user.Email);

            // Store token in database using repository
            var session = new UserSession
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
                IsActive = true
            };
            await _sessionRepository.CreateSessionAsync(session);

            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request)
    {
        var result = new AuthenticationResult();
        
        try
        {
            // Validate username availability using repository
            if (!await _userRepository.IsUsernameAvailableAsync(request.Username))
            {
                result.ValidationErrors.Add("Username is already taken");
            }

            // Validate email availability using repository
            if (!await _userRepository.IsEmailAvailableAsync(request.Email))
            {
                result.ValidationErrors.Add("Email is already registered");
            }

            if (result.ValidationErrors.Any())
            {
                result.Success = false;
                result.ErrorMessage = "Validation failed";
                return result;
            }

            var passwordHash = HashPassword(request.Password);
            
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true
            };

            // Create user using repository
            var createdUser = await _userRepository.CreateUserAsync(user);

            var token = GenerateJwtToken(createdUser.Id, createdUser.Username, createdUser.Email);
            
            // Store token using repository
            var session = new UserSession
            {
                UserId = createdUser.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
                IsActive = true
            };
            await _sessionRepository.CreateSessionAsync(session);

            result.Success = true;
            result.Token = token;
            result.User = MapToUserDto(createdUser);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = "An error occurred during registration";
            return result;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            return await _sessionRepository.ValidateSessionAsync(token);
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto?> GetUserByTokenAsync(string token)
    {
        try
        {
            var user = await _sessionRepository.GetUserBySessionTokenAsync(token);
            return user != null ? MapToUserDto(user) : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            return await _sessionRepository.DeactivateSessionAsync(token);
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> SuggestUsernamesAsync(string baseUsername)
    {
        var suggestions = new List<string>();

        try
        {
            // Generate variations
            var variations = new List<string>
            {
                baseUsername,
                $"{baseUsername}1",
                $"{baseUsername}2",
                $"{baseUsername}3",
                $"{baseUsername}_user",
                $"{baseUsername}_2024",
                $"{baseUsername}_{DateTime.Now.Year}",
                $"{baseUsername}_{new Random().Next(100, 999)}"
            };

            foreach (var variation in variations)
            {
                if (await _userRepository.IsUsernameAvailableAsync(variation))
                {
                    suggestions.Add(variation);
                    if (suggestions.Count >= 5) break;
                }
            }

            // If we don't have enough suggestions, generate random ones
            var random = new Random();
            while (suggestions.Count < 5)
            {
                var randomSuffix = random.Next(1000, 9999);
                var suggestion = $"{baseUsername}{randomSuffix}";
                
                if (await _userRepository.IsUsernameAvailableAsync(suggestion) && !suggestions.Contains(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }
        }
        catch
        {
            // Return basic suggestions if database fails
            suggestions.Add($"{baseUsername}1");
            suggestions.Add($"{baseUsername}2");
            suggestions.Add($"{baseUsername}_user");
        }

        return suggestions;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        try
        {
            return await _userRepository.IsUsernameAvailableAsync(username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        try
        {
            return await _userRepository.IsEmailAvailableAsync(email);
        }
        catch
        {
            return false;
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(Guid userId, string username, string email)
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

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
