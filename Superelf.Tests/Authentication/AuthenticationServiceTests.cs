using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Superelf.Application.Authentication;
using Superelf.Domain.Entities;
using System.Security.Claims;

namespace Superelf.Tests.Authentication;

[TestFixture]
public class AuthenticationServiceTests
{
    private Mock<IAuthenticationRepository> _mockRepository;
    private Mock<ILogger<AuthenticationService>> _mockLogger;
    private AuthenticationService _authService;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IAuthenticationRepository>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _authService = new AuthenticationService(_mockRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            PhoneNumber = "1234567890"
        };

        var identityResult = IdentityResult.Success;
        _mockRepository.Setup(r => r.CreateUserAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result.Result.Succeeded, Is.True);
        Assert.That(result.User, Is.Not.Null);
        Assert.That(result.User.UserName, Is.EqualTo(request.Username));
        Assert.That(result.User.Email, Is.EqualTo(request.Email));
        Assert.That(result.User.PhoneNumber, Is.EqualTo(request.PhoneNumber));
        Assert.That(result.User.NormalizedEmail, Is.EqualTo(request.Email.ToUpper()));
        Assert.That(result.User.EmailConfirmed, Is.True);
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var request = new LoginRequest
        {
            EmailOrUsername = "test@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = "1",
            UserName = "testuser",
            Email = "test@example.com"
        };

        _mockRepository.Setup(r => r.FindUserByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);
        _mockRepository.Setup(r => r.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(user.Id));
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            EmailOrUsername = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new ApplicationUser
        {
            Id = "1",
            UserName = "testuser",
            Email = "test@example.com"
        };

        _mockRepository.Setup(r => r.FindUserByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync(user);
        _mockRepository.Setup(r => r.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            EmailOrUsername = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockRepository.Setup(r => r.FindUserByEmailOrUsernameAsync(request.EmailOrUsername))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetCurrentUserAsync_ValidClaims_ReturnsUser()
    {
        // Arrange
        var claims = new ClaimsPrincipal();
        var expectedUser = new ApplicationUser
        {
            Id = "1",
            UserName = "testuser",
            Email = "test@example.com"
        };

        _mockRepository.Setup(r => r.GetUserAsync(claims))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _authService.GetCurrentUserAsync(claims);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expectedUser.Id));
        Assert.That(result.UserName, Is.EqualTo(expectedUser.UserName));
        Assert.That(result.Email, Is.EqualTo(expectedUser.Email));
    }

    [Test]
    public async Task GetCurrentUserAsync_InvalidClaims_ReturnsNull()
    {
        // Arrange
        var claims = new ClaimsPrincipal();
        _mockRepository.Setup(r => r.GetUserAsync(claims))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _authService.GetCurrentUserAsync(claims);

        // Assert
        Assert.That(result, Is.Null);
    }
}