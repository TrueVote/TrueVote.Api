using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public static class DatabaseSeederExtensions
    {
        public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services)
        {
            services.AddScoped<DatabaseSeeder>();
            return services;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<TrueVoteDbContext>();
                var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
                var configuration = services.GetRequiredService<IConfiguration>();

                // Seed the DB
                var seeder = new DatabaseSeeder(context, logger, configuration);
                await seeder.SeedRolesAsync();
                await seeder.SeedUserRolesAsync();
            }

            return app;
        }
    }

    [ExcludeFromCodeCoverage]
    public class DatabaseSeeder
    {
        private readonly ITrueVoteDbContext _context;
        private readonly ILogger<DatabaseSeeder> _log;
        private readonly IConfiguration _configuration;

        public DatabaseSeeder(ITrueVoteDbContext trueVoteDbContext, ILogger<DatabaseSeeder> log, IConfiguration configuration)
        {
            _context = trueVoteDbContext;
            _log = log;
            _configuration = configuration;
        }

        public async Task SeedUserRolesAsync()
        {
            var now = UtcNowProviderFactory.GetProvider().UtcNow;
            try
            {
                _log.LogInformation("Checking if user roles need to be seeded...");

                // Get system admin and service user IDs from configuration
                var systemAdminUserIds = _configuration.GetSection("SystemAdminUserIds").Get<string[]>() ?? Array.Empty<string>();
                var serviceUserIds = _configuration.GetSection("ServiceUserIds").Get<string[]>() ?? Array.Empty<string>();

                if (!systemAdminUserIds.Any() && !serviceUserIds.Any())
                {
                    _log.LogWarning("No system admin or service user IDs configured for seeding");
                    return;
                }

                // Get all configured users that exist in the database
                var allConfiguredUserIds = systemAdminUserIds.Union(serviceUserIds).ToArray();
                var existingUsers = await _context.Users
                    .Where(u => allConfiguredUserIds.Contains(u.UserId))
                    .Select(u => u.UserId)
                    .ToListAsync();

                if (!existingUsers.Any())
                {
                    _log.LogWarning("None of the configured user IDs exist in the database");
                    return;
                }

                // Get existing user roles to avoid duplicates
                var existingUserRoles = await _context.UserRoles
                    .Where(ur => existingUsers.Contains(ur.UserId))
                    .Select(ur => new { ur.UserId, ur.RoleId })
                    .ToListAsync();

                var newUserRoles = new List<UserRoleModel>();

                // Process system admin users - they get all roles
                foreach (var userId in existingUsers.Where(u => systemAdminUserIds.Contains(u)))
                {
                    var userNewRoles = UserRoles.AllRoles
                        .Where(role => !existingUserRoles.Any(er => er.UserId == userId && er.RoleId == role.Id))
                        .Select(role => new UserRoleModel
                        {
                            UserId = userId,
                            RoleId = role.Id,
                            DateCreated = now,
                            UserRoleId = Guid.NewGuid().ToString()
                        });

                    newUserRoles.AddRange(userNewRoles);
                }

                // Process service users - they only get the Service role
                foreach (var userId in existingUsers.Where(u => serviceUserIds.Contains(u)))
                {
                    // Only add Service role if it doesn't exist
                    if (!existingUserRoles.Any(er => er.UserId == userId && er.RoleId == UserRoles.Service.Id))
                    {
                        newUserRoles.Add(new UserRoleModel
                        {
                            UserId = userId,
                            RoleId = UserRoles.Service.Id,
                            DateCreated = now,
                            UserRoleId = Guid.NewGuid().ToString()
                        });
                    }
                }

                if (newUserRoles.Any())
                {
                    foreach (var userRole in newUserRoles)
                    {
                        _log.LogInformation("Seeding user role: User {UserId} with Role {RoleId}", userRole.UserId, userRole.RoleId);
                    }

                    await _context.UserRoles.AddRangeAsync(newUserRoles);
                    await _context.SaveChangesAsync();
                }

                _log.LogInformation("User Role seeding completed successfully. Added {Count} new role assignments", newUserRoles.Count);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while seeding user roles");
                throw;
            }
        }

        public async Task SeedRolesAsync()
        {
            try
            {
                _log.LogInformation("Checking if roles need to be seeded...");

                foreach (var roleInfo in UserRoles.AllRoles)
                {
                    var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleInfo.Id);
                    if (existingRole == null)
                    {
                        await _context.Roles.AddAsync(new RoleModel
                        {
                            RoleId = roleInfo.Id,
                            RoleName = roleInfo.Name,
                            Description = roleInfo.Description
                        });
                    }
                }
                await _context.SaveChangesAsync();
                _log.LogInformation("Role seeding completed successfully");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while seeding roles");
                throw;
            }
        }
    }
}
