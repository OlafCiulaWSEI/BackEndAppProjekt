using ApplicationCore.Models;
using Infrasctructure.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public class UserMigrationService
    {
        private readonly AppDbContext _sqliteContext;
        private readonly UserService _mongoService;
        private readonly UserManager<UserEntity> _userManager;

        public UserMigrationService(
            AppDbContext sqliteContext,
            UserService mongoService,
            UserManager<UserEntity> userManager)
        {
            _sqliteContext = sqliteContext;
            _mongoService = mongoService;
            _userManager = userManager;
        }

        public async Task MigrateUsers()
        {
            // Get all users from SQLite
            var sqliteUsers = await _sqliteContext.Users
                .Include(u => u.Details)
                .ToListAsync();

            var mongoUsers = new List<MongoUser>();

            foreach (var sqliteUser in sqliteUsers)
            {
                var mongoUser = new MongoUser
                {
                    Id = sqliteUser.Id,
                    Email = sqliteUser.Email,
                    NormalizedEmail = sqliteUser.NormalizedEmail,
                    UserName = sqliteUser.UserName,
                    NormalizedUserName = sqliteUser.NormalizedUserName,
                    PasswordHash = sqliteUser.PasswordHash,
                    SecurityStamp = sqliteUser.SecurityStamp,
                    ConcurrencyStamp = sqliteUser.ConcurrencyStamp,
                    EmailConfirmed = sqliteUser.EmailConfirmed,
                    PhoneNumber = sqliteUser.PhoneNumber,
                    PhoneNumberConfirmed = sqliteUser.PhoneNumberConfirmed,
                    TwoFactorEnabled = sqliteUser.TwoFactorEnabled,
                    LockoutEnd = sqliteUser.LockoutEnd,
                    LockoutEnabled = sqliteUser.LockoutEnabled,
                    AccessFailedCount = sqliteUser.AccessFailedCount,
                    Details = sqliteUser.Details
                };

                mongoUsers.Add(mongoUser);
            }

            // Insert all users into MongoDB
            if (mongoUsers.Any())
            {
                _mongoService.CreateMany(mongoUsers);
            }
        }
    }
} 