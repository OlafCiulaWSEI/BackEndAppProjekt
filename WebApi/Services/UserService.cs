using ApplicationCore.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace WebApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<MongoUser> _users;

        public UserService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<MongoUser>("Users");
        }

        public List<MongoUser> GetAll() => _users.Find(user => true).ToList();
        
        public MongoUser GetById(string id) => _users.Find(user => user.Id == id).FirstOrDefault();
        
        public MongoUser GetByEmail(string email) => _users.Find(user => user.Email == email).FirstOrDefault();
        
        public MongoUser GetByUserName(string userName) => _users.Find(user => user.UserName == userName).FirstOrDefault();

        public async Task<IdentityResult> CreateAsync(MongoUser user, string password)
        {
            var passwordHasher = new PasswordHasher<MongoUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            
            await _users.InsertOneAsync(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(MongoUser user)
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.IsAcknowledged ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.IsAcknowledged ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<MongoUser> FindByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<MongoUser> FindByNameAsync(string userName)
        {
            return await _users.Find(u => u.UserName == userName).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckPasswordAsync(MongoUser user, string password)
        {
            var passwordHasher = new PasswordHasher<MongoUser>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<IList<Claim>> GetClaimsAsync(MongoUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Dodaj rolę admina dla konkretnego użytkownika (możesz to dostosować)
            if (user.UserName == "admin")
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            return claims;
        }
    }
} 