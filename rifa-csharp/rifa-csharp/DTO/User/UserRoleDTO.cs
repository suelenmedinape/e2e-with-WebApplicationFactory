using System.ComponentModel.DataAnnotations;

namespace rifa_csharp.Jwt.DTO;

public class UserRoleDTO
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome da role é obrigatório")]
    public string RoleName { get; set; } = string.Empty;
}