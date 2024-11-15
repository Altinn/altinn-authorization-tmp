namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

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
