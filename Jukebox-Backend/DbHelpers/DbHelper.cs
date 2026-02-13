using Jukebox_Backend.Exceptions;
using Jukebox_Backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.DbHelpers
{
    public class DbHelper
    {
        public static async Task InitializeDatabaseAsync<T>(WebApplication app) where T : DbContext
        {
            try
            {
                IServiceProvider services = app.Services;

                await RunMigrationAsync<T>(services);
                await SeedRoles(services);
                await SeedAdmin(services);
            }
            catch
            {
                throw;
            }
        }

        private static async Task RunMigrationAsync<T>(IServiceProvider services) where T : DbContext
        {
            try
            {
                using var scope = services.CreateAsyncScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<T>();

                await dbContext.Database.MigrateAsync();
            }
            catch
            {
                throw;
            }
        }

        private static async Task SeedRoles(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateAsyncScope();

                RoleManager<ApplicationRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                bool superAdminRoleExists = await roleManager.RoleExistsAsync(StringConstants.AdminRole);

                ApplicationRole? superAdminRole = null;
                ApplicationRole? userRole = null;

                if (!superAdminRoleExists)
                {
                    superAdminRole = new ApplicationRole()
                    {
                        Name = StringConstants.AdminRole,
                        Description = "Admin del sistema",
                        Active = true
                    };

                    IdentityResult superAdminRoleCreated = await roleManager.CreateAsync(superAdminRole);

                    if (!superAdminRoleCreated.Succeeded)
                    {
                        throw new DbInitializationException("Creating admin role error");
                    }
                }

                bool userRoleExists = await roleManager.RoleExistsAsync(StringConstants.UserRole);

                if (!userRoleExists)
                {
                    userRole = new ApplicationRole()
                    {
                        Name = StringConstants.UserRole,
                        Description = "User standard",
                        Active = true
                    };

                    IdentityResult userRoleCreated = await roleManager.CreateAsync(userRole);

                    if (!userRoleCreated.Succeeded)
                    {
                        if (superAdminRole != null)
                        {
                            await roleManager.DeleteAsync(superAdminRole);
                        }
                        throw new DbInitializationException("Creating user role error");
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static async Task SeedAdmin(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateAsyncScope();

                UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                ApplicationUser? existingSuperAdmin = await userManager.FindByEmailAsync(StringConstants.AdminEmail);

                if (existingSuperAdmin == null)
                {
                    ApplicationUser superAdmin = new ApplicationUser()
                    {
                        IsActive = true,
                        Birthday = new DateOnly(1988, 02, 28),
                        FirstName = "Dobri",
                        LastName = "Dobrev",
                        Email = StringConstants.AdminEmail,
                        UserName = StringConstants.AdminEmail,
                        CreatedAt = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    IdentityResult userCreated = await userManager.CreateAsync(superAdmin, StringConstants.AdminPassword);

                    if (!userCreated.Succeeded)
                    {
                        throw new DbInitializationException("Creating user Admin error");
                    }

                    IdentityResult roleAssigned = await userManager.AddToRoleAsync(superAdmin, StringConstants.AdminRole);

                    if (!roleAssigned.Succeeded)
                    {
                        await userManager.DeleteAsync(superAdmin);
                        throw new DbInitializationException("Creating standard user error");
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
