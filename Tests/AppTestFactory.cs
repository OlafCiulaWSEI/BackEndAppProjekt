using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Configuration;

namespace Tests;

public class AppTestFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure MongoDB for testing
            services.Configure<MongoDbSettings>(options =>
            {
                options.ConnectionString = "mongodb://localhost:27017";
                options.DatabaseName = "AppTestDb";
            });
        });
        builder.UseEnvironment("Development");
    }
}