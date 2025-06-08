using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Dto;
using WebApi.Configuration;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using ApplicationCore.Models;
using ApplicationCore.Models.Filtering;
using ApplicationCore.Models.Sorting;

namespace WebApi.Services
{
    public class LegoSetService
    {
        private readonly IMongoCollection<LegoSet> _legoSets;
        private readonly MongoClient _client;

        public LegoSetService(IOptions<MongoDbSettings> settings)
        {
            var mongoSettings = MongoClientSettings.FromUrl(new MongoUrl(settings.Value.ConnectionString));
            mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
            mongoSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
            mongoSettings.SocketTimeout = TimeSpan.FromSeconds(30);
            
            _client = new MongoClient(mongoSettings);
            var database = _client.GetDatabase(settings.Value.DatabaseName);
            _legoSets = database.GetCollection<LegoSet>("LegoSets");
        }

        public async Task<PagedResponse<LegoSet>> GetAllPaged(
            int pageNumber, 
            int pageSize, 
            LegoSetFilter? filter = null,
            LegoSetSortingField? sortBy = null,
            SortOrder sortOrder = SortOrder.Ascending)
        {
            var filterBuilder = Builders<LegoSet>.Filter;
            var filter1 = filterBuilder.Empty;

            if (filter != null)
            {
                var filters = new List<FilterDefinition<LegoSet>>();

                if (!string.IsNullOrWhiteSpace(filter.Name))
                    filters.Add(filterBuilder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(filter.Name, "i")));

                if (filter.YearFrom.HasValue)
                    filters.Add(filterBuilder.Gte(x => x.Year, filter.YearFrom.Value));

                if (filter.YearTo.HasValue)
                    filters.Add(filterBuilder.Lte(x => x.Year, filter.YearTo.Value));

                if (!string.IsNullOrWhiteSpace(filter.Theme))
                    filters.Add(filterBuilder.Eq(x => x.Theme, filter.Theme));

                if (!string.IsNullOrWhiteSpace(filter.Subtheme))
                    filters.Add(filterBuilder.Eq(x => x.Subtheme, filter.Subtheme));

                if (filter.MinPieces.HasValue)
                    filters.Add(filterBuilder.Gte(x => x.Pieces, filter.MinPieces.Value));

                if (filter.MaxPieces.HasValue)
                    filters.Add(filterBuilder.Lte(x => x.Pieces, filter.MaxPieces.Value));

                if (filter.MinPrice.HasValue)
                    filters.Add(filterBuilder.Gte(x => x.USRetailPrice, filter.MinPrice.Value));

                if (filter.MaxPrice.HasValue)
                    filters.Add(filterBuilder.Lte(x => x.USRetailPrice, filter.MaxPrice.Value));

                if (filters.Any())
                    filter1 = filterBuilder.And(filters);
            }

            var totalCount = await _legoSets.CountDocumentsAsync(filter1);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var query = _legoSets.Find(filter1);

            if (sortBy.HasValue)
            {
                var sortDefinition = GetSortDefinition(sortBy.Value, sortOrder);
                query = query.Sort(sortDefinition);
            }

            var sets = await query
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

            return new PagedResponse<LegoSet>(sets, metadata);
        }

        private SortDefinition<LegoSet> GetSortDefinition(LegoSetSortingField sortBy, SortOrder sortOrder)
        {
            var sortBuilder = Builders<LegoSet>.Sort;
            
            var sortDefinition = sortBy switch
            {
                LegoSetSortingField.Name => sortOrder == SortOrder.Ascending 
                    ? sortBuilder.Ascending(x => x.Name) 
                    : sortBuilder.Descending(x => x.Name),
                
                LegoSetSortingField.Year => sortOrder == SortOrder.Ascending 
                    ? sortBuilder.Ascending(x => x.Year) 
                    : sortBuilder.Descending(x => x.Year),
                
                LegoSetSortingField.Theme => sortOrder == SortOrder.Ascending 
                    ? sortBuilder.Ascending(x => x.Theme) 
                    : sortBuilder.Descending(x => x.Theme),
                
                LegoSetSortingField.Pieces => sortOrder == SortOrder.Ascending 
                    ? sortBuilder.Ascending(x => x.Pieces) 
                    : sortBuilder.Descending(x => x.Pieces),
                
                LegoSetSortingField.Price => sortOrder == SortOrder.Ascending 
                    ? sortBuilder.Ascending(x => x.USRetailPrice) 
                    : sortBuilder.Descending(x => x.USRetailPrice),
                
                _ => sortBuilder.Ascending(x => x.Name)
            };

            return sortDefinition;
        }

        public List<LegoSet> GetAll() => _legoSets.Find(set => true).ToList();
        
        public LegoSet? GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return null;
            }
            return _legoSets.Find(set => set.Id == id).FirstOrDefault();
        }
        
        public List<LegoSet> GetByTheme(string theme) => 
            _legoSets.Find(set => set.Theme == theme).ToList();
        
        public List<LegoSet> GetByYear(int year) => 
            _legoSets.Find(set => set.Year == year).ToList();
        
        public List<LegoSet> GetByPiecesRange(int min, int max) => 
            _legoSets.Find(set => set.Pieces >= min && set.Pieces <= max).ToList();
        
        public List<string> GetAllThemes() => 
            _legoSets.Distinct<string>("Theme", FilterDefinition<LegoSet>.Empty).ToList();
        
        public List<int> GetAllYears() => 
            _legoSets.Distinct<int>("Year", FilterDefinition<LegoSet>.Empty).ToList().OrderBy(y => y).ToList();

        public LegoSet Create(LegoSet set)
        {
            _legoSets.InsertOne(set);
            return set;
        }

        public async Task ImportFromCsv(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            
            try
            {
                var records = csv.GetRecords<LegoSet>().ToList();
                if (records.Any())
                {
                    // Usuń istniejące dane przed importem
                    await _legoSets.DeleteManyAsync(Builders<LegoSet>.Filter.Empty);
                    await _legoSets.InsertManyAsync(records);
                    Console.WriteLine($"Successfully imported {records.Count} records");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing CSV: {ex.Message}");
                throw;
            }
        }
    }

    public class NullableDecimalConverter : CsvHelper.TypeConversion.DecimalConverter
    {
        public override object? ConvertFromString(string? text, CsvHelper.IReaderRow row, CsvHelper.Configuration.MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text) || text == "NA")
                return null;
            return base.ConvertFromString(text!, row, memberMapData);
        }
    }
}
