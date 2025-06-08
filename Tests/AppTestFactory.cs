using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Configuration;
using WebApi.Services;
using ApplicationCore.Models;
using MongoDB.Driver;

namespace Tests;

public class AppTestFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private static bool _databaseInitialized;
    private static readonly object _lock = new object();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure MongoDB for testing
            services.Configure<MongoDbSettings>(options =>
            {
                options.ConnectionString = "mongodb+srv://OlafCiula:BackEndApp@cluster0.hspl6w2.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
                options.DatabaseName = "LegoDbTest";
            });

            var sp = services.BuildServiceProvider();
            
            // Ensure database is initialized only once
            if (!_databaseInitialized)
            {
                lock (_lock)
                {
                    if (!_databaseInitialized)
                    {
                        using (var scope = sp.CreateScope())
                        {
                            var legoSetService = scope.ServiceProvider.GetRequiredService<LegoSetService>();
                            
                            // Clear existing data
                            var mongoSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
                            var client = new MongoClient(mongoSettings.ConnectionString);
                            var database = client.GetDatabase(mongoSettings.DatabaseName);
                            var collection = database.GetCollection<LegoSet>("LegoSets");
                            collection.DeleteMany(Builders<LegoSet>.Filter.Empty);

                            // Add test data
                            var testData = new List<LegoSet>
                            {
                                new LegoSet
                                {
                                    Id = "507f1f77bcf86cd799439011",
                                    Name = "Millennium Falcon",
                                    Theme = "Star Wars",
                                    Year = 2020,
                                    Pieces = 1000,
                                    USRetailPrice = 99.99m
                                },
                                new LegoSet
                                {
                                    Id = "507f1f77bcf86cd799439012",
                                    Name = "Death Star",
                                    Theme = "Star Wars",
                                    Year = 2021,
                                    Pieces = 2000,
                                    USRetailPrice = 199.99m
                                }
                            };

                            foreach (var set in testData)
                            {
                                legoSetService.Create(set);
                            }
                        }
                        _databaseInitialized = true;
                    }
                }
            }
        });
        builder.UseEnvironment("Development");
    }
}