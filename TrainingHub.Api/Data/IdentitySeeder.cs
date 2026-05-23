using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Models;
using TrainingHub.Security;

namespace TrainingHub.Data
{
    public static class IdentitySeeder
    {
        private const string SeedPassword = "TrainingHub123!";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var roleName in RoleNames.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            await EnsureUserAsync(userManager, "coordinator@traininghub.local", "Training Coordinator", RoleNames.TrainingCoordinator);
            await EnsureUserAsync(userManager, "instructor@traininghub.local", "Training Instructor", RoleNames.Instructor);
            await EnsureUserAsync(userManager, "trainee@traininghub.local", "Training Trainee", RoleNames.Trainee);
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string roleName)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to seed user {email}: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var existingRoles = await userManager.GetRolesAsync(user);
                if (existingRoles.Count > 0)
                {
                    var removeResult = await userManager.RemoveFromRolesAsync(user, existingRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unable to update roles for {email}: {string.Join(", ", removeResult.Errors.Select(error => error.Description))}");
                    }
                }

                var addResult = await userManager.AddToRoleAsync(user, roleName);
                if (!addResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to assign role {roleName} to {email}: {string.Join(", ", addResult.Errors.Select(error => error.Description))}");
                }
            }
        }
    }
}