using Microsoft.AspNetCore.Identity;
using rifa_csharp.Enums;

namespace rifa_csharp.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // Pega todos os valores do enum Role
        var roles = Enum.GetNames(typeof(Role));

        foreach (var roleName in roles)
        {
            // Verifica se a role já existe
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                // Cria a role se não existir
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}