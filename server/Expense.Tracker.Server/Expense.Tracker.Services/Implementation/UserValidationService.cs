using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Expense.Tracker.Services.Abstractions.Interfaces;

namespace Expense.Tracker.Services.Implementation;

public class UserValidationService : IUserValidationService, IDisposable
{
    private const int BcryptWorkFactor = 12;
    private const int MaxUsernameSuggestions = 5;

    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserValidationService> _logger;
    private readonly RandomNumberGenerator _randomGenerator;

    public UserValidationService(IUserRepository userRepository, ILogger<UserValidationService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
        _randomGenerator = RandomNumberGenerator.Create();
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        try
        {
            return await _userRepository.IsUsernameAvailableAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check username availability for: {Username}", username);
            return false;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        try
        {
            return await _userRepository.IsEmailAvailableAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check email availability for: {Email}", email);
            return false;
        }
    }

    public async Task<bool> IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            await Task.Yield();
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password verification failed - hash may be corrupted");
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
                $"{baseUsername}_2025",
                $"{baseUsername}_{DateTime.Now.Year}",
                $"{baseUsername}_{GetSecureRandomNumber(100, 999)}"
            };

            foreach (var variation in variations)
            {
                if (await _userRepository.IsUsernameAvailableAsync(variation))
                {
                    suggestions.Add(variation);
                    if (suggestions.Count >= MaxUsernameSuggestions) break;
                }
            }

            // If we don't have enough suggestions, generate random ones using secure random
            while (suggestions.Count < MaxUsernameSuggestions)
            {
                var randomSuffix = GetSecureRandomNumber(1000, 9999);
                var suggestion = $"{baseUsername}{randomSuffix}";
                
                if (await _userRepository.IsUsernameAvailableAsync(suggestion) && !suggestions.Contains(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate username suggestions for: {BaseUsername}", baseUsername);
            // Return basic suggestions if database fails
            suggestions.Add($"{baseUsername}1");
            suggestions.Add($"{baseUsername}2");
            suggestions.Add($"{baseUsername}_user");
        }

        return suggestions;
    }

    /// <summary>
    /// Generates a cryptographically secure random number within the specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
    /// <returns>A secure random number within the specified range.</returns>
    private int GetSecureRandomNumber(int minValue, int maxValue)
    {
        var range = maxValue - minValue;
        var bytes = new byte[4];
        _randomGenerator.GetBytes(bytes);
        var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return (randomValue % range) + minValue;
    }

    /// <summary>
    /// Disposes of the cryptographic random number generator when the service is disposed.
    /// </summary>
    public void Dispose()
    {
        _randomGenerator?.Dispose();
    }
}
