using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MindAeonDermaAI.Infrastructure.Identity;

namespace MindAeonDermaAI.Infrastructure.Seed;

/// <summary>Seeds the three application roles and a default administrator account.</summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var config = sp.GetRequiredService<IConfiguration>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = config["Seed:AdminEmail"] ?? "admin@mindaeondermaai.com";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            var adminPassword = config["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = "Adm1!" + Guid.NewGuid().ToString("N")[..16];
                Console.WriteLine(
                    "[Seed] Seed:AdminPassword was not configured. Generated a one-time administrator password: " +
                    adminPassword);
            }

            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator"
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
        else if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
