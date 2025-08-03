using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Superelf.Infrastructure.Data;

/// <summary>
/// Factory class to create DbContext instances for design-time operations like migrations
/// without requiring a full application startup.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();
        
        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback connection string for migrations
            connectionString = "Host=localhost;Port=5432;Database=SuperelfDb;Username=postgres;Password=postgres";
            Console.WriteLine("Using fallback connection string for migrations: " + connectionString);
        }
        else
        {
            Console.WriteLine("Using connection string from appsettings: " + connectionString);
        }
        
        // Create options builder
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
