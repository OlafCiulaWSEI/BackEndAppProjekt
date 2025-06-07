using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CsvHelper.Configuration.Attributes;

namespace ApplicationCore.Models
{
    public class LegoSet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("set_id")]
        [Name("set_id")]
        public string SetId { get; set; } = null!;

        [BsonElement("name")]
        [Name("name")]
        public string Name { get; set; } = null!;

        [BsonElement("year")]
        [Name("year")]
        public int Year { get; set; }

        [BsonElement("theme")]
        [Name("theme")]
        public string Theme { get; set; } = null!;

        [BsonElement("subtheme")]
        [Name("subtheme")]
        public string? Subtheme { get; set; }

        [BsonElement("themeGroup")]
        [Name("themeGroup")]
        public string? ThemeGroup { get; set; }

        [BsonElement("category")]
        [Name("category")]
        public string Category { get; set; } = null!;

        [BsonElement("pieces")]
        [Name("pieces")]
        public int? Pieces { get; set; }

        [BsonElement("minifigs")]
        [Name("minifigs")]
        public int? Minifigs { get; set; }

        [BsonElement("agerange_min")]
        [Name("agerange_min")]
        public int? AgeRangeMin { get; set; }

        [BsonElement("US_retailPrice")]
        [Name("US_retailPrice")]
        public decimal? USRetailPrice { get; set; }

        [BsonElement("bricksetURL")]
        [Name("bricksetURL")]
        public string? BricksetUrl { get; set; }

        [BsonElement("thumbnailURL")]
        [Name("thumbnailURL")]
        public string? ThumbnailUrl { get; set; }

        [BsonElement("imageURL")]
        [Name("imageURL")]
        public string? ImageUrl { get; set; }
    }
} 