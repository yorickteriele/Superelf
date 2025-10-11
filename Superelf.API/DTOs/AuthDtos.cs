namespace Superelf.API.DTOs;

/// <summary>
/// Data transfer object for user login requests.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email address or username of the user attempting to log in
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// User's password in plain text (will be hashed by the authentication service)
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to persist the login session
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// Data transfer object for user registration requests.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Desired username for the new account
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address for the new account
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional phone number for the account
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Password in plain text (will be hashed before storage)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for authentication responses, used for both login and registration.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Whether the authentication operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// JWT token for successful authentication
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Status message describing the result
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// User information if authentication was successful
    /// </summary>
    public UserDto? User { get; set; }
}

/// <summary>
/// Data transfer object for user information.
/// Contains only the basic, non-sensitive user details.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
