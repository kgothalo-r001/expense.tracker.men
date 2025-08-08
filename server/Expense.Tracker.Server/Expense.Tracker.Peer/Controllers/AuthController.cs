using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Expense.Tracker.Services.Abstractions.Interfaces;
using Expense.Tracker.Services.Abstractions.Models;
using Expense.Tracker.Services.Abstractions.Constants;

namespace Expense.Tracker.Peer.Controllers;

[ApiController]
[Route($"{ApiConstants.BaseApiRoute}/{ApiConstants.Routes.Auth}")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
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
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result);
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
            return BadRequest(new { 
                message = result.ErrorMessage,
                errors = result.ValidationErrors
            });
        }

        return Ok(result);
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var success = await _authService.LogoutAsync(token);
        
        if (!success)
        {
            return BadRequest(new { message = "Failed to logout" });
        }

        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("GetCurrentUser")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByTokenAsync(token);
        
        if (user == null)
        {
            return Unauthorized();
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
            return BadRequest(new { message = result.ErrorMessage });
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

    private string? GetTokenFromHeader()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}
