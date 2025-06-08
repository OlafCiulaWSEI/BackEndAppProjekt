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

        public async Task<List<MongoUser>> GetAllAsync() => 
            await _users.Find(user => true).ToListAsync();
        
        public async Task<MongoUser?> GetByIdAsync(string id) => 
            await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        
        public async Task<MongoUser?> GetByEmailAsync(string email) => 
            await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        
        public async Task<MongoUser?> FindByNameAsync(string userName) => 
            await _users.Find(user => user.UserName == userName).FirstOrDefaultAsync();

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

        public Task<bool> CheckPasswordAsync(MongoUser user, string password)
        {
            var passwordHasher = new PasswordHasher<MongoUser>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return Task.FromResult(result != PasswordVerificationResult.Failed);
        }

        public Task<IList<Claim>> GetClaimsAsync(MongoUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            return Task.FromResult<IList<Claim>>(claims);
        }
    }
} 