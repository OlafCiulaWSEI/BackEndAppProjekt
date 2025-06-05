using Infrasctructure.EF;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using WebApi.Configuration;
using WebApi.Services;

namespace WebApi;
public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        builder.Services.AddSingleton<JwtSettings>();
        builder.Services.ConfigureJWT(new JwtSettings(builder.Configuration));
        builder.Services.ConfigureCors();
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
        builder.Services.AddSingleton<LegoSetService>();
        builder.Services.AddSingleton<CommentService>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddScoped<UserMigrationService>();
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        
        var app = builder.Build();
        
        // Import CSV data
        var legoSetService = app.Services.GetRequiredService<LegoSetService>();
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "lego_sets.csv");
        if (File.Exists(csvPath))
        {
            await legoSetService.ImportFromCsv(csvPath);
        }

        // Migrate users from SQLite to MongoDB
        using (var scope = app.Services.CreateScope())
        {
            var userMigrationService = scope.ServiceProvider.GetRequiredService<UserMigrationService>();
            await userMigrationService.MigrateUsers();
        }
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }
        
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        await app.RunAsync();
    }
}

