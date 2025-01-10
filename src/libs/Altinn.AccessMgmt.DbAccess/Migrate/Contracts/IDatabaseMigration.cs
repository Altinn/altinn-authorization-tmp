namespace Altinn.AccessMgmt.DbAccess.Migrate.Contracts;

/// <summary>
/// Database migration for schema
/// </summary>
public interface IDatabaseMigration
{
    /// <summary>
    /// Initiate migration
    /// </summary>
    /// <returns></returns>
    Task Init();
}
