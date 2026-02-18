using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using rifa_csharp.Entities;
using rifa_csharp.Jwt.DTO;
using rifa_csharp.Jwt.Service;
using rifa_csharp.JwtSecurity.Interface;
using rifa_csharp.JwtSecurity.Service;

namespace rifa_csharp.Jwt.Controller;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService service;
    private readonly ILogger logger;
    
    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        this.service = authService;
        this.logger = logger;
    }
    
    /*[HttpPost]
    [Route("createRole")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        logger.LogError($"============= GET auth/createRole =============");
        var item = await service.createRole(roleName);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(401, item.Errors.FirstOrDefault()!);
    }*/

    /*[HttpPost]
    [Route("addUserToRole")]
    public async Task<IActionResult> AddUserToRole(string email, string roleName)
    {
        logger.LogError($"============= GET auth/addUserToRole =============");
        var item = await service.AddUserRole(email, roleName);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(401, item.Errors.FirstOrDefault()!);
    }*/

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModelDTO dto)
    {
        logger.LogError($"============= GET auth/login =============");
        var item = await service.Login(dto);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(400, item.Errors.FirstOrDefault()!);
    }

    [Authorize(Policy = "ADMIN")]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModelDTO dto)
    {
        logger.LogError($"============= GET auth/register =============");
        var item = await service.Register(dto);
        return item.IsSuccess
            ? Created("Criado", item.Successes.FirstOrDefault())
            : StatusCode(400, item.Errors.FirstOrDefault());
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenModelDTO dto)
    {
        logger.LogError($"============= POST auth/refresh-token =============");
        var item = await service.RefreshToken(dto);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(400, item.Errors.FirstOrDefault()!);
    }

    [HttpPost]
    [Route("revoke/{email}")]
    public async Task<IActionResult> Revoke(string email)
    {
        logger.LogError($"============= POST auth/revoke/{email} =============");
        var item = await service.Revoke(email);
        return item.IsSuccess ? Ok(item.Successes.FirstOrDefault()) : StatusCode(400, item.Errors.FirstOrDefault()!);
    }
    
    [HttpGet("verify")]
    public async Task<IActionResult> VerifyToken()
    {
        logger.LogError($"============= GET auth/verify =============");
    
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var result = await service.VerifyToken(token);
        return result.IsSuccess ? Ok(result.Successes.FirstOrDefault()) : StatusCode(401, result.Errors.FirstOrDefault()!);
    }

    /*[Authorize(Policy = "ADMIN")]
    [HttpPost]
    [Route("add-role")]
    public async Task<IActionResult> AddUserRole([FromBody] UserRoleDTO dto)
    {
        logger.LogInformation($"POST auth/add-role - Email: {dto.Email}, Role: {dto.RoleName}");
        var result = await service.AddUserRole(dto.Email, dto.RoleName);
        return result.IsSuccess 
            ? Ok(result.Successes.FirstOrDefault()) 
            : BadRequest(result.Errors.FirstOrDefault()!);
    }

    [Authorize(Policy = "ADMIN")]
    [HttpPost]
    [Route("remove-role")]
    public async Task<IActionResult> RemoveUserRole([FromBody] UserRoleDTO dto)
    {
        logger.LogInformation($"POST auth/remove-role - Email: {dto.Email}, Role: {dto.RoleName}");
        var result = await service.RemoveUserRole(dto.Email, dto.RoleName);
        return result.IsSuccess 
            ? Ok(result.Successes.FirstOrDefault()) 
            : BadRequest(result.Errors.FirstOrDefault()!);
    }*/

    [Authorize(Policy = "ADMIN")]
    [HttpPut]
    [Route("change-role")]
    public async Task<IActionResult> ChangeUserRole([FromBody] UserRoleDTO dto)
    {
        logger.LogError($"auth/change-role");
        var result = await service.ChangeUserRole(dto.Email, dto.RoleName);
        return result.IsSuccess 
            ? Ok(result.Successes.FirstOrDefault()) 
            : BadRequest(result.Errors.FirstOrDefault()!);
    }
    
}