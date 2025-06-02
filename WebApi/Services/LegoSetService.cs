using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Dto;
using WebApi.Configuration;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace WebApi.Services
{
    public class LegoSetService
    {
        private readonly IMongoCollection<LegoSet> _legoSets;

        public LegoSetService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _legoSets = database.GetCollection<LegoSet>("LegoSets");
        }

        public List<LegoSet> GetAll() => _legoSets.Find(set => true).ToList();
        
        public LegoSet GetById(string id) => _legoSets.Find(set => set.Id == id).FirstOrDefault();
        
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

        public void Create(LegoSet set) => _legoSets.InsertOne(set);

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
        public override object ConvertFromString(string text, CsvHelper.IReaderRow row, CsvHelper.Configuration.MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text) || text == "NA")
                return null;
            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}
