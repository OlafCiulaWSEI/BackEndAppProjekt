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

        public async Task<PagedResponse<Comment>> GetCommentsByLegoSetPaged(string legoSetId, int pageNumber, int pageSize)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.LegoSetId, legoSetId);
            var totalCount = await _comments.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var comments = await _comments.Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var metadata = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = (int)totalCount,
                TotalPages = totalPages
            };

            return new PagedResponse<Comment>(comments, metadata);
        }

        public List<Comment> GetCommentsByLegoSet(string legoSetId) =>
            _comments.Find(c => c.LegoSetId == legoSetId)
                .SortByDescending(c => c.CreatedAt)
                .ToList();

        public Comment? GetById(string id) =>
            _comments.Find(c => c.Id == id).FirstOrDefault();

        public Comment Create(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            _comments.InsertOne(comment);
            return comment;
        }
        
        public bool Update(string id, string userId, string content)
        {
            var filter = Builders<Comment>.Filter.Where(c => c.Id == id);
            var update = Builders<Comment>.Update.Set(c => c.Content, content);
            var result = _comments.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }

        public void Delete(string id)
        {
            _comments.DeleteOne(c => c.Id == id);
        }
    }
} 