using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using rifa_csharp.JwtSecurity.Interface;

namespace rifa_csharp.JwtSecurity.Service;

public class TokenService : ITokenService
{
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
    {
        var key = _config.GetSection("JWT").GetValue<string>("SecretKey") ??
                  throw new InvalidOperationException("Invalid secret Key"); // Obter a chave secreta

        var privateKey = Encoding.UTF8.GetBytes(key); // converte para um array de bytes

        // criando as credenciais de assinatura
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature); 

        // criando o descritor do token - quando gerado o token contem essas informações
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), // claims relacionadas com o usuario
            Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")), // data de expiração do token
            Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"), // valor da audiencia
            Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"), // valor do emissor
            SigningCredentials = signingCredentials // atribuindo as credenciais
        };

        var tokenHandler = new JwtSecurityTokenHandler(); // intancia
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor); // Gerando o token

        return token;
    }

    public string GenerateRefreshToken()
    {
        var secureRandomBytes = new byte[128]; // array de bytes de tamnho 128

        using var randomNumberGenerator = RandomNumberGenerator.Create(); // grando numeros aleatorios

        randomNumberGenerator.GetBytes(secureRandomBytes); // preenchendo o array de bytes

        var refreshToken = Convert.ToBase64String(secureRandomBytes); // convertendo para base64, mais facil de ser lido

        return refreshToken;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config)
    {
        var secretKey = _config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid key"); // obtem a chave secreta

        // definindo os parametros de validação para o token jwt expirado
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), // gerando uma chave de assinatura
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler(); // instancia

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
           )
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
    
    public bool VerifyToken(string? token, IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["JWT:SecretKey"]!); // Mudado de "Secret" para "SecretKey"

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:ValidIssuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}