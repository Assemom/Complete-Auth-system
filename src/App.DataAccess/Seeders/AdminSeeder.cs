using App.Domain.Entities;
using App.Domain.Enums;
using App.Shared.Settings;
using Microsoft.AspNetCore.Identity;

namespace App.DataAccess.Seeders;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, SeedSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AdminEmail) || string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(settings.AdminEmail);
        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new ApplicationUser
        {
            Email = settings.AdminEmail,
            UserName = settings.AdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, settings.AdminPassword);
        if (!createResult.Succeeded)
        {
            return;
        }

        await userManager.AddToRoleAsync(adminUser, nameof(Roles.Admin));
    }
}
