using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ApplicationCore.Models;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using WebApi.Dto;
using WebApi.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using WebApi.Configuration;

namespace Tests;

public class AppUsersControllerTests : IClassFixture<AppTestFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AppTestFactory<Program> _app;
    private readonly UserService _userService;

    public AppUsersControllerTests(AppTestFactory<Program> app)
    {
        _app = app;
        _client = app.CreateClient();
        using (var scope = app.Services.CreateScope())
        {
            _userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var mongoSettings = scope.ServiceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            var collection = database.GetCollection<MongoUser>("Users");
            collection.DeleteMany(Builders<MongoUser>.Filter.Empty);

            // Initialize test user
            InitializeAsync().Wait();
        }
    }

    private async Task InitializeAsync()
    {
        var adminUser = new MongoUser
        {
            Id = "0093c1f5-8a99-4262-a4cd-24003a8915be",
            Email = "admin@wsei.edu.pl",
            NormalizedEmail = "ADMIN@WSEI.EDU.PL",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            ConcurrencyStamp = "0093c1f5-8a99-4262-a4cd-24003a8915be",
            SecurityStamp = "0093c1f5-8a99-4262-a4cd-24003a8915be",
            EmailConfirmed = true
        };

        await _userService.CreateAsync(adminUser, "1234!");
    }

    [Fact]
    public async Task TestValidLogin()
    {
        // Arrange
        var loginBody = new LoginDto()
        {
            UserName = "admin",
            Password = "1234!"
        };

        // Act
        var result = await _client.PostAsJsonAsync("/api/users/login", loginBody);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(content);
        var token = node?["token"]?.ToString();
        Assert.NotNull(token);
    }

    [Fact]
    public async Task TestBookController()
    {
        // Act
        var result = await _client.GetAsync("/api/books");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}