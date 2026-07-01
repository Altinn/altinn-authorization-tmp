using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <summary>
/// Used by cli `dotnet ef migration`
/// </summary>
public sealed class AppDbDesignTimeContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<AppDbDesignTimeContextFactory>()
            .Build();
        
        var path = "PostgreSQLSettings:AdminConnectionString";
        if (connectionString.GetValue<string>(path) is var cs && string.IsNullOrEmpty(cs))
        {
            Console.WriteLine($"The configuration path '{path}' is missing or empty. Please check your environment variables, User Secrets, or Environment Variables. Trying default values."); 
            cs = "Database=authorizationdb;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true";
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>()
            .Options;

        return new AppDbContext(options)
        {
            AuditAccessor = new AuditAccessor()
            {
                AuditValues = new AuditValues(Guid.Empty, Guid.Empty, "design-time", DateTimeOffset.UtcNow),
            }
        };
    }
}
