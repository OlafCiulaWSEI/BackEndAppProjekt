namespace WebApi.Configuration;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string ValidIssuer { get; set; } = string.Empty;
    public string ValidAudience { get; set; } = string.Empty;

    public JwtSettings(IConfiguration configuration)
    {
        Secret = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JwtSettings:Secret");
        ValidIssuer = configuration["JwtSettings:ValidIssuer"] ?? throw new ArgumentNullException("JwtSettings:ValidIssuer");
        ValidAudience = configuration["JwtSettings:ValidAudience"] ?? throw new ArgumentNullException("JwtSettings:ValidAudience");
    }
}