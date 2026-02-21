using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace slender_server.Infra.Database.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Seed roles first
        await SeedRolesAsync(roleManager);
        
        // Then seed users
        await SeedUsersAsync(userManager);
    }
    
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Member"];
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    
    private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        // Seed test users
        var testUsers = new[]
        {
            new { Email = "alice@slender.app", Password = "Test123!", Name = "Alice Smith" },
            new { Email = "bob@slender.app", Password = "Test123!", Name = "Bob Johnson" },
            new { Email = "charlie@slender.app", Password = "Test123!", Name = "Charlie Davis" },
        };
        
        foreach (var userData in testUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var identityUser = new IdentityUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(identityUser, userData.Password);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "Member");
                    Console.WriteLine($"✅ Created identity user: {userData.Email}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}