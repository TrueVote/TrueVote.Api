using Microsoft.EntityFrameworkCore;
using Nostr.Client.Keys;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;
using TrueVote.Api.Services;

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
                var serviceBus = services.GetRequiredService<IServiceBus>();

                // Seed the DB
                var seeder = new DatabaseSeeder(context, logger, configuration, serviceBus);
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
        private readonly IServiceBus _serviceBus;

        public DatabaseSeeder(ITrueVoteDbContext trueVoteDbContext, ILogger<DatabaseSeeder> log, IConfiguration configuration, IServiceBus serviceBus)
        {
            _context = trueVoteDbContext;
            _log = log;
            _configuration = configuration;
            _serviceBus = serviceBus;
        }

        private async Task<UserModel> AddNewUser(BaseUserModel baseUser, string userId)
        {
            var now = UtcNowProviderFactory.GetProvider().UtcNow;
            var user = new UserModel { FullName = baseUser.FullName, Email = baseUser.Email, UserId = userId, NostrPubKey = baseUser.NostrPubKey, DateCreated = now, DateUpdated = now, UserPreferences = new UserPreferencesModel() };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote User created: {user.FullName}");

            return user;
        }

        private async Task<List<string>> FindOrAddUsersByNsec(IEnumerable<string> nsecs, string userType)
        {
            var userIds = new List<string>();

            foreach (var nsec in nsecs)
            {
                var keys = NostrPrivateKey.FromBech32(nsec);
                var npub = keys.DerivePublicKey().Bech32;

                var user = await _context.Users.Where(u => u.NostrPubKey.Equals(npub)).FirstOrDefaultAsync();
                var userId = string.Empty;
                if (user == null)
                {
                    var baseUser = new BaseUserModel
                    {
                        Email = "unknown@truevote.org",
                        FullName = $"{userType} User",
                        NostrPubKey = npub
                    };
                    var newUser = await AddNewUser(baseUser, Guid.NewGuid().ToString());
                    userId = newUser.UserId;
                }
                else
                {
                    userId = user.UserId;
                }

                userIds.Add(userId);
            }

            return userIds;
        }

        public async Task SeedUserRolesAsync()
        {
            var now = UtcNowProviderFactory.GetProvider().UtcNow;
            try
            {
                _log.LogInformation("Checking if user roles need to be seeded...");

                // Get system admin and service user IDs from configuration
                var systemAdminNsecs = _configuration.GetSection("SystemAdminNsecs").Get<string[]>() ?? Array.Empty<string>();
                var serviceNsecs = _configuration.GetSection("ServiceNsecs").Get<string[]>() ?? Array.Empty<string>();
                if (!systemAdminNsecs.Any() && !serviceNsecs.Any())
                {
                    _log.LogWarning("No system admin or service nsecs configured for seeding");
                    return;
                }

                var systemAdminUserIds = await FindOrAddUsersByNsec(systemAdminNsecs, "System Admin");
                var serviceUserIds = await FindOrAddUsersByNsec(serviceNsecs, "Service Admin");

                // Get all configured users that exist in the database
                var allConfiguredUserIds = systemAdminUserIds.Union(serviceUserIds).ToArray();
                var existingUsers = await _context.Users.Where(u => allConfiguredUserIds.Contains(u.UserId)).Select(u => u.UserId).ToListAsync();
                if (!existingUsers.Any())
                {
                    _log.LogWarning("None of the configured Nsecs exist in the database");
                    return;
                }

                // Get existing user roles to avoid duplicates
                var existingUserRoles = await _context.UserRoles.Where(ur => existingUsers.Contains(ur.UserId)).Select(ur => new { ur.UserId, ur.RoleId }).ToListAsync();

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
