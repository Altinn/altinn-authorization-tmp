using System;
using System.Reflection;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Executors;

using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Core.Utilities;
using Altinn.AccessMgmt.Repo.Ingest;
using Altinn.AccessMgmt.Repo.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.AccessMgmt.Repo.Extensions;

/// <summary>
/// Extensions for setting up DbAccess services
/// </summary>
public static class DbAccessHostExtensions
{
    /// <summary>
    /// Configure DbAccess services
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <returns></returns>
    public static IHostApplicationBuilder ConfigureDb(this IHostApplicationBuilder builder)
    {
        /*
        builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));
        builder.AddAltinnLease(opt =>
        {
            opt.Type = AltinnLeaseType.InMemory;
            //opt.Type = AltinnLeaseType.AzureStorageAccount;
            //opt.StorageAccount.Endpoint = new Uri("https://standreastest.blob.core.windows.net/");
        });
        */

        return builder;
    }

    /// <summary>
    /// Add DbAccess services
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDb(this IHostApplicationBuilder builder)
    {
        var dbType = "Postgres"; // TODO: Get from config
        builder.Services.AddSingleton<DbDefinitionRegistry>();

        if (dbType == "Postgres")
        {
            builder.Services.AddSingleton<IDbConverter, DbConverter>();
            builder.Services.AddSingleton<IDbExecutor, PostgresDbExecutor>();
        }
        else if (dbType == "MSSQL")
        {
            builder.Services.AddSingleton<IDbConverter, DbConverter>(); // TODO: Add MSSQL converter
            builder.Services.AddSingleton<IDbExecutor, MssqlDbExecutor>();
        }
        else
        {
            throw new Exception($"Unknown databasetype: {dbType}");
        }

        /*Add Repository*/
        var assembly = Assembly.GetExecutingAssembly();
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repoType in repositoryTypes)
        {
            var interfaceType = repoType.GetInterfaces().FirstOrDefault(i => i.Name == "I" + repoType.Name);
            if (interfaceType != null)
            {
                builder.Services.AddSingleton(interfaceType, repoType);
            }
        }

        builder.Services.AddSingleton<MigrationService>();
        builder.Services.AddSingleton<IngestService>();
        builder.Services.AddSingleton<MockupService>();

        return builder;
    }

    /// <summary>
    /// Use DbAccess services
    /// </summary>
    /// <param name="host">IHost</param>
    /// <returns></returns>
    public static async Task<IHost> UseDb(this IHost host)
    {
        /*Add Definitions to DbDefinitionRegistry*/
        DefineAllModels(host.Services);

        var migration = host.Services.GetRequiredService<MigrationService>();
        migration.GenerateAll();
        await migration.Migrate();


        /*
        var dbIngest = host.Services.GetRequiredService<IngestService>();
        if (dbIngest != null)
        {
            await dbIngest.IngestAll();
        }

        var mockService = host.Services.GetService<MockupService>();
        if (mockService != null)
        {
            await mockService.Run();
        }
        */

        return host;
    }

    private static void DefineAllModels(IServiceProvider serviceProvider, string? definitionNamespace = null)
    {
        var assemblies = string.IsNullOrEmpty(definitionNamespace)
     ? AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name!.StartsWith(Assembly.GetExecutingAssembly().GetName().Name!))
     : new[] { Assembly.Load(definitionNamespace) };

        var definitions = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IDbDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IDbDefinition)ActivatorUtilities.CreateInstance(serviceProvider, t)!)
            .ToList();

        definitions.ForEach(def => def.Define());
    }
}
