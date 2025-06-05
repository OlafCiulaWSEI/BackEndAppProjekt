using ApplicationCore.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Configuration;

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

        public void Create(MongoUser user) => _users.InsertOne(user);

        public void CreateMany(List<MongoUser> users) => _users.InsertMany(users);

        public void Update(string id, MongoUser user) => 
            _users.ReplaceOne(u => u.Id == id, user);

        public void Delete(string id) => _users.DeleteOne(user => user.Id == id);
    }
} 