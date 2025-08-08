using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;

namespace Expense.Tracker.Services.Implementation;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly IUserValidationService _userValidationService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        ITokenService tokenService,
        ISessionService sessionService,
        IUserValidationService userValidationService,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _sessionService = sessionService;
        _userValidationService = userValidationService;
        _logger = logger;
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

            if (!_userValidationService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username/email or password"
                };
            }

            var token = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);

            // Create session using session service
            var session = await _sessionService.CreateSessionAsync(user.Id, token);

            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                User = ConvertUserToDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for user: {UsernameOrEmail}", request.UsernameOrEmail);
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
            // Validate username availability using validation service
            if (!await _userValidationService.IsUsernameAvailableAsync(request.Username))
            {
                result.ValidationErrors.Add("Username is already taken");
            }

            // Validate email availability using validation service
            if (!await _userValidationService.IsEmailAvailableAsync(request.Email))
            {
                result.ValidationErrors.Add("Email is already registered");
            }

            if (result.ValidationErrors.Any())
            {
                result.Success = false;
                result.ErrorMessage = "Validation failed";
                return result;
            }

            var passwordHash = _userValidationService.HashPassword(request.Password);
            
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

            var token = _tokenService.GenerateJwtToken(createdUser.Id, createdUser.Username, createdUser.Email);
            
            // Create session using session service
            var session = await _sessionService.CreateSessionAsync(createdUser.Id, token);

            result.Success = true;
            result.Token = token;
            result.User = ConvertUserToDto(createdUser);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration for user: {Username}", request.Username);
            result.Success = false;
            result.ErrorMessage = "An error occurred during registration";
            return result;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            return await _sessionService.ValidateSessionAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed for token: {TokenPrefix}", token[..Math.Min(token.Length, 10)] + "...");
            return false;
        }
    }

    public async Task<UserDto?> GetUserByTokenAsync(string token)
    {
        try
        {
            var user = await _sessionService.GetUserBySessionTokenAsync(token);
            return user != null ? ConvertUserToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user by token: {TokenPrefix}", token[..Math.Min(token.Length, 10)] + "...");
            return null;
        }
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            return await _sessionService.DeactivateSessionAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to logout user with token: {TokenPrefix}", token[..Math.Min(token.Length, 10)] + "...");
            return false;
        }
    }

    public async Task<List<string>> SuggestUsernamesAsync(string baseUsername)
    {
        return await _userValidationService.SuggestUsernamesAsync(baseUsername);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return await _userValidationService.IsUsernameAvailableAsync(username);
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return await _userValidationService.IsEmailAvailableAsync(email);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string token)
    {
        try
        {
            // Validate the existing token
            var isValidToken = await ValidateTokenAsync(token);
            if (!isValidToken)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired token"
                };
            }

            // Get user by token
            var user = await _sessionService.GetUserBySessionTokenAsync(token);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            // Generate new token
            var newToken = _tokenService.GenerateJwtToken(user.Id, user.Username, user.Email);

            // Refresh session using session service
            var session = await _sessionService.RefreshSessionAsync(token, newToken, user.Id);

            return new AuthenticationResult
            {
                Success = true,
                Token = newToken,
                User = ConvertUserToDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during token refresh for token: {TokenPrefix}", token[..Math.Min(token.Length, 10)] + "...");
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during token refresh"
            };
        }
    }

    private static UserDto ConvertUserToDto(User user)
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
