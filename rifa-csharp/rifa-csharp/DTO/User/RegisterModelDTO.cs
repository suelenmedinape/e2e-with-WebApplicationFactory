using System.ComponentModel.DataAnnotations;
using rifa_csharp.Enums;

namespace rifa_csharp.Jwt.DTO;

public class RegisterModelDTO
{
    [Required(ErrorMessage = "User Name is required")]
    public string? Username { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}