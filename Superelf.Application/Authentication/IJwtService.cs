using System.Security.Claims;
using Superelf.Domain.Entities;

namespace Superelf.Application.Authentication;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    ClaimsPrincipal GetPrincipalFromToken(string token);
}
