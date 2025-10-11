using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Superelf.Domain.Entities;

namespace Superelf.Application.Authentication;

/// <summary>
/// Service responsible for handling user authentication operations including registration,
/// login verification, and user management.
/// </summary>
public class AuthenticationService {
    private readonly IAuthenticationRepository _repository;
    private readonly ILogger<AuthenticationService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthenticationService.
    /// </summary>
    /// <param name="repository">Repository for user data access and management</param>
    /// <param name="logger">Logger for authentication operations</param>
    public AuthenticationService(
        IAuthenticationRepository repository,
        ILogger<AuthenticationService> logger) 
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user in the system with the provided registration details.
    /// </summary>
    /// <param name="request">The registration request containing user details and password</param>
    /// <returns>A result object containing the registration outcome and created user</returns>
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

    /// <summary>
    /// Authenticates a user using email/username and password credentials.
    /// </summary>
    /// <param name="request">Login request containing user credentials</param>
    /// <returns>The authenticated user if credentials are valid, null otherwise</returns>
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

    /// <summary>
    /// Retrieves the user associated with the current claims principal.
    /// </summary>
    /// <param name="userClaims">Claims principal containing user identity information</param>
    /// <returns>The user associated with the claims, or null if not found</returns>
    public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userClaims) {
        return await _repository.GetUserAsync(userClaims);
    }
}

/// <summary>
/// Represents the result of a user registration operation.
/// </summary>
public class RegisterResult {
    /// <summary>
    /// Initializes a new instance of the RegisterResult class.
    /// </summary>
    /// <param name="result">The Identity framework operation result</param>
    /// <param name="user">The created user, if registration was successful</param>
    public RegisterResult(IdentityResult result, ApplicationUser? user) {
        Result = result;
        User = user;
    }

    /// <summary>
    /// Gets the Identity framework result of the registration operation.
    /// </summary>
    public IdentityResult Result { get; }

    /// <summary>
    /// Gets the created user if registration was successful.
    /// </summary>
    public ApplicationUser? User { get; }
}