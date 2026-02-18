using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace rifa_csharp.JwtSecurity.Interface;

public interface ITokenService
{
    JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config);
    
    string GenerateRefreshToken();
    
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config);
    
    bool VerifyToken(string? token, IConfiguration configuration);
}