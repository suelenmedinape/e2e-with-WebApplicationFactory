using rifa_csharp.Enums;

namespace rifa_csharp.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public Role Role { get; set; }
}