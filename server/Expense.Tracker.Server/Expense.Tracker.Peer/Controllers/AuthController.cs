using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Constants;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Helpers;

namespace Expense.Tracker.Peer.Controllers;

[ApiController]
[Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Auth}")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("Login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        SetAuthCookie(result.Token);

        return Ok(new AuthenticationResult
        {
            Success = true,
            User = result.User,
            Token = null,
            ErrorMessage = null
        });
    }

    [HttpPost("Register")]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(new AuthenticationResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ValidationErrors = result.ValidationErrors
            });
        }

        SetAuthCookie(result.Token);

        return Ok(new AuthenticationResult
        {
            Success = true,
            User = result.User,
            Token = null,
            ErrorMessage = null
        });
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var token = GetTokenFromCookie();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "No authentication token found" });
        }

        var success = await _authService.LogoutAsync(token);
        
        // Clear the authentication cookie regardless of logout service result
        ClearAuthCookie();
        
        if (!success)
        {
            return BadRequest(new { message = "Failed to logout from server, but cookie cleared" });
        }

        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("GetCurrentUser")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var token = GetTokenFromCookie();
        if (string.IsNullOrEmpty(token))
        {
            return StatusCode(401, new { message = "No authentication token found" });
        }

        var user = await _authService.GetUserByTokenAsync(token);
        
        if (user == null)
        {
            // Clear invalid cookie
            ClearAuthCookie();
            return StatusCode(401, new { message = "Invalid or expired token" });
        }

        return Ok(user);
    }

    [HttpGet("CheckSession")]
    public async Task<ActionResult<UserDto>> CheckSession()
    {
        var token = GetTokenFromCookie();
        
        if (string.IsNullOrEmpty(token))
        {
            return Ok(null as UserDto);
        }

        var user = await _authService.GetUserByTokenAsync(token);
        
        if (user == null)
        {
            // Clear invalid cookie
            ClearAuthCookie();
            return Ok(null as UserDto);
        }

        return Ok(user);
    }

    [HttpGet("ValidateToken")]
    [Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized();
        }

        var isValid = await _authService.ValidateTokenAsync(token);
        
        if (!isValid)
        {
            return Unauthorized();
        }

        return Ok(new { valid = true });
    }

    [HttpPost("RefreshToken")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RefreshTokenAsync(request.Token);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result);
    }

    [HttpGet("GetUsernameSuggestions")]
    public async Task<ActionResult<List<string>>> GetUsernameSuggestions([FromQuery] string baseUsername)
    {
        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            return BadRequest(new { message = "Base username is required" });
        }

        var suggestions = await _authService.SuggestUsernamesAsync(baseUsername.Trim());
        return Ok(suggestions);
    }

    [HttpGet("CheckUsernameAvailability")]
    public async Task<ActionResult<bool>> CheckUsernameAvailability([FromQuery] string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var isAvailable = await _authService.IsUsernameAvailableAsync(username.Trim());
        return Ok(new { available = isAvailable });
    }

    [HttpGet("CheckEmailAvailability")]
    public async Task<ActionResult<bool>> CheckEmailAvailability([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var isAvailable = await _authService.IsEmailAvailableAsync(email.Trim());
        return Ok(new { available = isAvailable });
    }

    private const string AuthCookieName = "ExpenseTracker.Auth";
    
    private string? GetTokenFromCookie()
    {
        return HttpContext.Request.Cookies[AuthCookieName];
    }
    
    private string? GetTokenFromHeader()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
    
    private void SetAuthCookie(string token)
    {
        var options = CookieHelper.GetSecureCookieOptions();
        HttpContext.Response.Cookies.Append(AuthCookieName, token, options);
    }
    
    private void ClearAuthCookie()
    {
        HttpContext.Response.Cookies.Delete(AuthCookieName, CookieHelper.GetExpiredCookieOptions());
    }
}
