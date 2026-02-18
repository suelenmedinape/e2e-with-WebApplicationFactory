using Microsoft.AspNetCore.Identity;

namespace rifa_csharp.Entities;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}