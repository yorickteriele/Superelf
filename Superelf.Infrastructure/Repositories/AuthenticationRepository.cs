using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Superelf.Application.Authentication;
using Superelf.Domain.Entities;

namespace Superelf.Infrastructure.Repositories;

public class AuthenticationRepository : IAuthenticationRepository {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthenticationRepository> _logger;

    public AuthenticationRepository(
        UserManager<ApplicationUser> userManager,
        ILogger<AuthenticationRepository> logger) 
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApplicationUser?> FindUserByEmailOrUsernameAsync(string emailOrUsername) {
        _logger.LogInformation("Finding user by email or username: {EmailOrUsername}", emailOrUsername);
        
        try {
            var userByEmail = await _userManager.FindByEmailAsync(emailOrUsername);
            if (userByEmail != null) {
                _logger.LogInformation("User found by email: {UserId}, {Username}", userByEmail.Id, userByEmail.UserName);
                return userByEmail;
            }

            _logger.LogInformation("No user found by email, trying username: {EmailOrUsername}", emailOrUsername);
            var userByName = await _userManager.FindByNameAsync(emailOrUsername);
            
            if (userByName != null) {
                _logger.LogInformation("User found by username: {UserId}", userByName.Id);
                return userByName;
            }
            
            _logger.LogWarning("No user found by email or username: {EmailOrUsername}", emailOrUsername);
            return null;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Database error occurred when finding user: {EmailOrUsername}", emailOrUsername);
            throw;
        }
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password) {
        _logger.LogInformation("Checking password for user: {UserId}", user.Id);
        
        try {
            var result = await _userManager.CheckPasswordAsync(user, password);
            _logger.LogInformation("Password check result for {UserId}: {Result}", user.Id, result ? "Success" : "Failed");
            return result;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error checking password for user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password) {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal userClaims) {
        return await _userManager.GetUserAsync(userClaims);
    }
}