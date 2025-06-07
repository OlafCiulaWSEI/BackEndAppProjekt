using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationCore.Models
{
    public class MongoUser
    {
        [BsonId]
        [BsonElement("_id")]
        public string Id { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("normalizedEmail")]
        public string NormalizedEmail { get; set; } = null!;

        [BsonElement("userName")]
        public string UserName { get; set; } = null!;

        [BsonElement("normalizedUserName")]
        public string NormalizedUserName { get; set; } = null!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;

        [BsonElement("securityStamp")]
        public string SecurityStamp { get; set; } = null!;

        [BsonElement("concurrencyStamp")]
        public string ConcurrencyStamp { get; set; } = null!;

        [BsonElement("emailConfirmed")]
        public bool EmailConfirmed { get; set; }

        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [BsonElement("phoneNumberConfirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [BsonElement("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [BsonElement("lockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [BsonElement("lockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        [BsonElement("accessFailedCount")]
        public int AccessFailedCount { get; set; }

        [BsonElement("details")]
        public UserDetails Details { get; set; } = null!;
    }
} 