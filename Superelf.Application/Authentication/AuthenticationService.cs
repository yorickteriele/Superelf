using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Superelf.Domain.Entities;

namespace Superelf.Application.Authentication;

public class AuthenticationService {
    private readonly IAuthenticationRepository _repository;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IAuthenticationRepository repository,
        ILogger<AuthenticationService> logger) 
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request) {
        var user = new ApplicationUser {
            UserName = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            NormalizedEmail = request.Email.ToUpper(),
            EmailConfirmed = true
        };

        var result = await _repository.CreateUserAsync(user, request.Password);
        return new RegisterResult(result, user);
    }

    public async Task<ApplicationUser?> LoginAsync(LoginRequest request) {
        _logger.LogInformation("LoginAsync called for: {EmailOrUsername}", request.EmailOrUsername);
        
        try {
            var user = await _repository.FindUserByEmailOrUsernameAsync(request.EmailOrUsername);
            if (user == null) {
                _logger.LogWarning("No user found with email/username: {EmailOrUsername}", request.EmailOrUsername);
                return null;
            }

            _logger.LogInformation("User found, checking password for user: {UserId}, {Username}", user.Id, user.UserName);
            var isValidPassword = await _repository.CheckPasswordAsync(user, request.Password);
            
            if (isValidPassword) {
                _logger.LogInformation("Password validation successful for user: {UserId}", user.Id);
                return user;
            } else {
                _logger.LogWarning("Invalid password for user: {UserId}, {Username}", user.Id, user.UserName);
                return null;
            }
        } 
        catch (Exception ex) {
            _logger.LogError(ex, "Exception occurred during login authentication for: {EmailOrUsername}", request.EmailOrUsername);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userClaims) {
        return await _repository.GetUserAsync(userClaims);
    }
}

public class RegisterResult {
    public RegisterResult(IdentityResult result, ApplicationUser? user) {
        Result = result;
        User = user;
    }

    public IdentityResult Result { get; }
    public ApplicationUser? User { get; }
}