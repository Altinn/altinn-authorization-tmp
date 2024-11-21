namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

public class DbAccessDataConfig
{
    public DbAccessDataConfig()
    {

    }

    public DbAccessDataConfig(Action<DbAccessDataConfig> configureOptions)
    {
        configureOptions?.Invoke(this);
    }

    public string ConnectionString { get; set; }

    public bool UseSqlServer { get; set; }
}
