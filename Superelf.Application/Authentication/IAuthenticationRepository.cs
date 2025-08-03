using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Superelf.Domain.Entities;

namespace Superelf.Application.Authentication;

public interface IAuthenticationRepository {
    public Task<ApplicationUser?> FindUserByEmailOrUsernameAsync(string emailOrUsername);
    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
    public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal userClaims);
}