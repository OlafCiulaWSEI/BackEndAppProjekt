using ApplicationCore.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Configuration;

namespace WebApi.Services
{
    public class CommentService
    {
        private readonly IMongoCollection<Comment> _comments;

        public CommentService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _comments = database.GetCollection<Comment>("Comments");
        }

        public List<Comment> GetByLegoSetId(string legoSetId) =>
            _comments.Find(c => c.LegoSetId == legoSetId).ToList();

        public Comment? GetById(string id) =>
            _comments.Find(c => c.Id == id).FirstOrDefault();

        public void Add(Comment comment) =>
            _comments.InsertOne(comment);

        public bool Update(string id, string userId, string content)
        {
            var filter = Builders<Comment>.Filter.Where(c => c.Id == id && c.UserId == userId);
            var update = Builders<Comment>.Update.Set(c => c.Content, content);
            var result = _comments.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }

        public bool Delete(string id, string userId)
        {
            var filter = Builders<Comment>.Filter.Where(c => c.Id == id && c.UserId == userId);
            var result = _comments.DeleteOne(filter);
            return result.DeletedCount > 0;
        }
    }
} 