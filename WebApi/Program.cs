using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Configuration;
using WebApi.Services;

namespace WebApi;
public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSingleton<JwtSettings>();
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
        
        // Configure JWT Authentication
        var jwtSettings = new JwtSettings(builder.Configuration);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.ValidIssuer,
                ValidAudience = jwtSettings.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.ConfigureCors();
        
        // Register services
        builder.Services.AddSingleton<LegoSetService>();
        builder.Services.AddSingleton<CommentService>();
        builder.Services.AddSingleton<UserService>();
        
        // Add OpenAPI
        builder.Services.AddOpenApi();
        
        var app = builder.Build();
        
        // Import CSV data
        var legoSetService = app.Services.GetRequiredService<LegoSetService>();
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "lego_sets.csv");
        if (File.Exists(csvPath))
        {
            await legoSetService.ImportFromCsv(csvPath);
        }
        
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        await app.RunAsync();
    }
}

