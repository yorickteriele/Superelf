namespace Superelf.Application.Authentication;

public class RegisterRequest {
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required string Password { get; set; }
}