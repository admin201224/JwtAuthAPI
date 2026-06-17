using JwtAuthAPI.Models;
using System.Security.Claims;

namespace JwtAuthAPI.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }

}
