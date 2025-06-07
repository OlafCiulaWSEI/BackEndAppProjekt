using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationCore.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("legoSetId")]
        public string LegoSetId { get; set; } = null!;

        [BsonElement("userId")]
        public string? UserId { get; set; } // null jeśli anonimowy

        [BsonElement("content")]
        public string Content { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
