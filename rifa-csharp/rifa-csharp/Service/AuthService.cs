using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using rifa_csharp.Entities;
using rifa_csharp.Enums;
using rifa_csharp.Jwt.DTO;
using rifa_csharp.JwtSecurity.Interface;

namespace rifa_csharp.Jwt.Service;

public class AuthService
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<Result> AddUserRole(string email, string roleName)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new Result().WithError(new Error("Usuário não encontrado"));
            }

            // Verifica se a role existe
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return new Result().WithError(new Error($"Role '{roleName}' não existe"));
            }

            // Verifica se o usuário já tem essa role
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                return new Result().WithError(new Error($"Usuário já possui a role '{roleName}'"));
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            
            if (result.Succeeded)
            {
                return new Result()
                    .WithSuccess(
                        new Success($"Role '{roleName}' atribuída ao usuário {user.Email}")
                            .WithMetadata("data", roleName)
                    );
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new Result().WithError(new Error($"Erro ao adicionar role: {errors}"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> RemoveUserRole(string email, string roleName)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new Result().WithError(new Error("Usuário não encontrado"));
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return new Result().WithError(new Error($"Usuário não possui a role '{roleName}'"));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            
            if (result.Succeeded)
            {
                return new Result()
                    .WithSuccess(
                        new Success($"Role '{roleName}' removida do usuário {user.Email}")
                    );
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new Result().WithError(new Error($"Erro ao remover role: {errors}"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> ChangeUserRole(string email, string newRoleName)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new Result().WithError(new Error("Usuário não encontrado"));
            }

            // Verifica se a nova role existe
            if (!await _roleManager.RoleExistsAsync(newRoleName))
            {
                return new Result().WithError(new Error($"Role '{newRoleName}' não existe"));
            }

            // Remove todas as roles atuais
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return new Result().WithError(new Error($"Erro ao remover roles antigas: {errors}"));
                }
            }

            // Adiciona a nova role
            var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
            
            if (addResult.Succeeded)
            {
                return new Result()
                    .WithSuccess(
                        new Success($"Role do usuário alterada para '{newRoleName}'")
                            .WithMetadata("data", newRoleName)
                    );
            }

            var addErrors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            return new Result().WithError(new Error($"Erro ao adicionar nova role: {addErrors}"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> Login(LoginModelDTO dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email!);

            if (user is not null && await _userManager.CheckPasswordAsync(user, dto.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"],
                    out int refreshTokenValidityInMinutes);

                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);
                user.RefreshToken = refreshToken;

                await _userManager.UpdateAsync(user);

                var result = new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                };

                return new Result()
                    .WithSuccess(
                        new Success("Login realizado com sucesso")
                            .WithMetadata("data", result)
                    );
            }
            
            return new Result().WithError(new Error("Email ou senha inválidos"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> Register(RegisterModelDTO dto)
    {
        try
        {
            var emailExists = await _userManager.FindByEmailAsync(dto.Email!);
            
            if (emailExists != null)
            {
                return new Result().WithError(new Error("Email já está em uso"));
            }

            ApplicationUser user = new()
            {
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = dto.Username
            };

            var result = await _userManager.CreateAsync(user, dto.Password!);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new Result().WithError(new Error($"Erro ao criar usuário: {errors}"));
            }
            
            // Adiciona a role automaticamente
            var addRoleResult = await _userManager.AddToRoleAsync(user,  Role.OPERATOR.ToString());
            
            if (!addRoleResult.Succeeded)
            {
                // Se falhar ao adicionar role, remove o usuário criado
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                return new Result().WithError(new Error($"Erro ao atribuir role: {errors}"));
            }

            return new Result()
                .WithSuccess(
                    new Success("Usuário criado com sucesso")
                        .WithMetadata("data", new { user.UserName, user.Email })
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> RefreshToken(TokenModelDTO dto)
    {
        try
        {
            if (dto is null)
            {
                return new Result().WithError(new Error("Requisição inválida"));
            }

            string? accessToken = dto.AccessToken ?? throw new ArgumentNullException(nameof(dto.AccessToken));
            string? refreshToken = dto.RefreshToken ?? throw new ArgumentNullException(nameof(dto.RefreshToken));

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if (principal == null)
            {
                return new Result().WithError(new Error("Token de acesso/refresh inválido"));
            }

            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new Result().WithError(new Error("Token de acesso/refresh inválido ou expirado"));
            }

            var newAccessToken = _tokenService.GenerateAccessToken(
                principal.Claims.ToList(), _configuration);

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            var result = new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken
            };
            
            return new Result()
                .WithSuccess(
                    new Success("Token atualizado com sucesso")
                        .WithMetadata("data", result)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> Revoke(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new Result().WithError(new Error("Usuário não encontrado"));
            }

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return new Result()
                .WithSuccess(new Success("Token revogado com sucesso"));
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }

    public async Task<Result> VerifyToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return new Result().WithError(new Error("Token não fornecido"));
            }

            // Remove "Bearer " se existir
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            // Verifica se o token é válido (assinatura, expiração, etc)
            if (!_tokenService.VerifyToken(token, _configuration))
            {
                return new Result().WithError(new Error("Token inválido ou expirado"));
            }

            // Extrai o principal do token
            var principal = _tokenService.GetPrincipalFromExpiredToken(token, _configuration);
        
            if (principal == null)
            {
                return new Result().WithError(new Error("Não foi possível extrair informações do token"));
            }

            // Extrai claims do token
            var username = principal.Identity?.Name;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            var tokenInfo = new
            {
                Username = username,
                Email = email,
                Role = role,
                IsValid = true
            };

            return new Result()
                .WithSuccess(
                    new Success("Token válido")
                        .WithMetadata("data", tokenInfo)
                );
        }
        catch (Exception e)
        {
            return new Result().WithError(new Error(e.Message));
        }
    }
}