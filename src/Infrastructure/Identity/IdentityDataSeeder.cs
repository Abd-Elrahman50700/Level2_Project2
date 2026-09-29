using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApplicationUser>>();

        // Seed Roles
        string[] roles = [Roles.Admin, Roles.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Default Admin User
        const string adminEmail = "admin@cleanarch.com";
        const string adminUserName = "admin";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(adminUser, [Roles.Admin, Roles.User]);
            }
            else
            {
                logger?.LogWarning("Failed to seed Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure admin has Admin and User roles
            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
            if (!await userManager.IsInRoleAsync(adminUser, Roles.User))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.User);
            }
        }

        // Seed Default Standard User
        const string standardUserEmail = "user@cleanarch.com";
        const string standardUserName = "testuser";
        var standardUser = await userManager.FindByEmailAsync(standardUserEmail);
        if (standardUser is null)
        {
            standardUser = new ApplicationUser
            {
                UserName = standardUserName,
                Email = standardUserEmail,
                FullName = "Standard User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(standardUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(standardUser, Roles.User);
            }
            else
            {
                logger?.LogWarning("Failed to seed standard user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(standardUser, Roles.User))
            {
                await userManager.AddToRoleAsync(standardUser, Roles.User);
            }
        }
    }
}
