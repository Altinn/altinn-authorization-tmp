using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Altinn.Authorization.Host.Database;

internal class AltinnHostDatabaseFactory(IServiceProvider serviceProvider) : IAltinnDatabase
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public NpgsqlConnection CreatePgsqlConnection(SourceType sourceType)
    {
        return ServiceProvider.GetKeyedService<NpgsqlConnection>(sourceType);
    }
}

public interface IAltinnDatabase
{
    NpgsqlConnection CreatePgsqlConnection(SourceType sourceType);
}
