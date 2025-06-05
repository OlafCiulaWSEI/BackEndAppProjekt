using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ApplicationCore.Models;
using Microsoft.Extensions.DependencyInjection;
using WebApi;
using WebApi.Dto;
using WebApi.Services;

namespace Tests;

public class AppUsersControllerTests: IClassFixture<AppTestFactory<Program>>
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
        }
    }

    public async Task InitializeAsync()
    {
        using (var scope = _app.Services.CreateScope())
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
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEN4Im6rGVZTx+s2fuPhH31UICA2T6sOMQ9YvPkEOj6a0zu0S+SnQKNg/jnOJM62/QA=="
            };

            await _userService.CreateAsync(adminUser, "1234!");
        }
    }

    [Fact]
    public async void TestValidLogin()
    {
        var loginBody = new LoginDto()
        {
            UserName = "admin",
            Password = "1234!"
        };
        var result = await _client.PostAsJsonAsync("/api/users/login", loginBody);
        Assert.NotNull(result);
        Assert.Equal(result.StatusCode, HttpStatusCode.OK);
        JsonNode node = JsonNode.Parse(await result.Content.ReadAsStringAsync());
        var token = node["token"].AsValue().ToString();
        Assert.NotNull(token);
    }

    [Fact]
    public async void TestBookController()
    {
        var result = await _client.GetAsync("/api/books");
        Assert.NotNull(result);
        Assert.Equal(result.StatusCode, HttpStatusCode.Unauthorized);
    }
}