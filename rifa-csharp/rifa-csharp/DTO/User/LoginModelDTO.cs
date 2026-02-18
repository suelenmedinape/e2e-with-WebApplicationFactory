using System.ComponentModel.DataAnnotations;

namespace rifa_csharp.Jwt.DTO;

public class LoginModelDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}