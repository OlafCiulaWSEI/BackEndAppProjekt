using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrasctructure.EF;

public class AppDbContext:IdentityDbContext<UserEntity>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected AppDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var adminId = "0093c1f5-8a99-4262-a4cd-24003a8915be";
        var adminCreatedAt = new DateTime(2025, 04, 08);
        var adminUser = new UserEntity()
        {
            Id = adminId,
            Email = "admin@wsei.edu.pl",
            NormalizedEmail = "ADMIN@WSEI.EDU.PL", 
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            ConcurrencyStamp = adminId,
            SecurityStamp = adminId,
        };
        adminUser.PasswordHash = "AQAAAAIAAYagAAAAEN4Im6rGVZTx+s2fuPhH31UICA2T6sOMQ9YvPkEOj6a0zu0S+SnQKNg/jnOJM62/QA==";


        builder.Entity<UserEntity>()
            .HasData(adminUser);
        
        builder.Entity<UserEntity>()
            .OwnsOne(u => u.Details)
            .HasData(
                new
                {
                    UserEntityId = adminId,
                    CreatedAt = adminCreatedAt
                }
            );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=d:\\Data\\app.db");
    }
}