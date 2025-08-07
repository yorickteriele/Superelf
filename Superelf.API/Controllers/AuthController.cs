using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Superelf.Application.Authentication;
using Superelf.Domain.Entities;
using Superelf.API.DTOs;

namespace Superelf.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthenticationService authService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto 
            { 
                Success = false, 
                Message = "Invalid data provided" 
            });
        }

        var registerRequest = new RegisterRequest
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            Password = registerDto.Password
        };

        var result = await _authService.RegisterAsync(registerRequest);

        if (result.Result.Succeeded && result.User != null)
        {
            var token = await _jwtService.GenerateTokenAsync(result.User);
            
            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                User = new UserDto
                {
                    Id = result.User.Id,
                    Username = result.User.UserName!,
                    Email = result.User.Email!
                }
            });
        }

        var errors = string.Join(", ", result.Result.Errors.Select(e => e.Description));
        return BadRequest(new AuthResponseDto 
        { 
            Success = false, 
            Message = errors 
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for: {EmailOrUsername}", loginDto.EmailOrUsername);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login failed: Invalid model state");
            return BadRequest(new AuthResponseDto 
            { 
                Success = false, 
                Message = "Invalid data provided" 
            });
        }

        var loginRequest = new LoginRequest
        {
            EmailOrUsername = loginDto.EmailOrUsername,
            Password = loginDto.Password
        };

        try 
        {
            _logger.LogInformation("Attempting to authenticate user via AuthService");
            var user = await _authService.LoginAsync(loginRequest);

            if (user != null)
            {
                _logger.LogInformation("User found, generating JWT token for user: {UserId}", user.Id);
                var token = await _jwtService.GenerateTokenAsync(user);
                
                _logger.LogInformation("Login successful for user: {UserId}, {Username}", user.Id, user.UserName);
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.UserName!,
                        Email = user.Email!
                    }
                });
            }

            _logger.LogWarning("Login failed: User not found or invalid password for: {EmailOrUsername}", loginDto.EmailOrUsername);
            return Unauthorized(new AuthResponseDto 
            { 
                Success = false, 
                Message = "Invalid credentials" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during login for: {EmailOrUsername}", loginDto.EmailOrUsername);
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            });
        }
    }

    [HttpPost("logout")]
    public ActionResult<AuthResponseDto> Logout()
    {
        // With JWT tokens, logout is handled client-side by removing the token
        // There's no server-side session to clear
        return Ok(new AuthResponseDto 
        { 
            Success = true, 
            Message = "Logout successful" 
        });
    }
}
