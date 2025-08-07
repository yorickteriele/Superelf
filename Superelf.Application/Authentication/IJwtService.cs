using System.Security.Claims;
using Superelf.Domain.Entities;

namespace Superelf.Application.Authentication;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    ClaimsPrincipal GetPrincipalFromToken(string token);
}
