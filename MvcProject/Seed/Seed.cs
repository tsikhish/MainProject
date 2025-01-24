using Microsoft.AspNetCore.Identity;
using static System.Formats.Asn1.AsnWriter;
using System.Data;

namespace MvcProject.Seed
{
    public static class Seed
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            await SeedRolesAsync(serviceProvider);

            await SeedAdminUserAsync(serviceProvider);
        }

        private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Player" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var email = "admin@admin.com";
                var password = "Admin_123";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = email,
                        Email = email
                    };

                    var createResult = await userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        throw new Exception($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(user, "Admin");
                    if (!addToRoleResult.Succeeded)
                    {
                        throw new Exception($"Failed to add user to role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while seeding admin user: {ex.Message}");
                throw;
            }
        }
    }
}