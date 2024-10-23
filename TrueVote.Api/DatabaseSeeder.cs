using Microsoft.EntityFrameworkCore;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    public static class DatabaseSeederExtensions
    {
        public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services)
        {
            services.AddScoped<DatabaseSeeder>();
            return services;
        }
    }

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

                var userIds = _configuration.GetSection("SystemAdminUserIds").Get<string[]>();

                foreach (var u in userIds)
                {
                    var userRoles = new[]
                    {
                        new UserRoleModel
                        {
                           UserId = u,
                           RoleId = UserRolesId.ElectionAdminId,
                           DateCreated = now,
                           UserRoleId = Guid.NewGuid().ToString()
                        },
                        new UserRoleModel
                        {
                           UserId = u,
                           RoleId = UserRolesId.SystemAdminId,
                           DateCreated = now,
                           UserRoleId = Guid.NewGuid().ToString()
                        },
                        new UserRoleModel
                        {
                           UserId = u,
                           RoleId = UserRolesId.VoterId,
                           DateCreated = now,
                           UserRoleId = Guid.NewGuid().ToString()
                        }
                    };

                    // Determine if user even exists
                    var existingUser = await _context.Users.FirstOrDefaultAsync(r => r.UserId == u);
                    if (existingUser == null)
                    {
                        continue;
                    }

                    foreach (var userRole in userRoles)
                    {
                        var existingUserRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userRole.UserId && r.RoleId == userRole.RoleId);
                        if (existingUserRole == null)
                        {
                            _log.LogInformation("Seeding userRole: {UserRoleName}", userRole.RoleId);
                            await _context.UserRoles.AddAsync(userRole);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _log.LogInformation("User Role seeding completed successfully");
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

                var requiredRoles = new[]
                {
                    new RoleModel
                    {
                        RoleId = UserRolesId.ElectionAdminId,
                        RoleName = UserRoles.ElectionAdmin,
                        Description = UserRolesDescription.ElectionAdminDesc
                    },
                    new RoleModel
                    {
                        RoleId = UserRolesId.VoterId,
                        RoleName = UserRoles.Voter,
                        Description = UserRolesDescription.VoterDesc
                    },
                    new RoleModel
                    {
                        RoleId = UserRolesId.SystemAdminId,
                        RoleName = UserRoles.SystemAdmin,
                        Description = UserRolesDescription.SystemAdminDesc
                    }
                };

                foreach (var role in requiredRoles)
                {
                    var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role.RoleName);

                    if (existingRole == null)
                    {
                        _log.LogInformation("Seeding role: {RoleName}", role.RoleName);
                        await _context.Roles.AddAsync(role);
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
