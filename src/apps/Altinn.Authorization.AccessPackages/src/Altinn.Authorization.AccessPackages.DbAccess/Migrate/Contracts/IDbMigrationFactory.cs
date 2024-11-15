using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Migrate.Contracts;

/// <summary>
/// Database Migration Factory
/// </summary>
public interface IDbMigrationFactory
{
    /// <summary>
    /// Enable translation
    /// </summary>
    bool UseTranslation { get; set; }

    /// <summary>
    /// Enable history
    /// </summary>
    bool UseHistory { get; set; }

    /// <summary>
    /// Initialize
    /// </summary>
    Task Init();

    /// <summary>
    /// Create schema
    /// </summary>
    /// <param name="name">Name</param>
    Task CreateSchema(string name);

    /// <summary>
    /// Create function
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="query">Query</param>
    Task CreateFunction(string name, string query);

    /// <summary>
    /// Create table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="withHistory">Create History table</param>
    /// <param name="withTranslation">Create Translation table</param>
    /// <param name="primaryKeyColumns">PrimaryKeys</param>
    Task CreateTable<T>(bool withHistory = false, bool withTranslation = false, Dictionary<string, CommonDataType>? primaryKeyColumns = null);

    /// <summary>
    /// Create column
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="name">Name</param>
    /// <param name="dbType">Datatype</param>
    /// <param name="nullable">Nullable</param>
    /// <param name="defaultValue">Default value</param>
    Task CreateColumn<T>(string name, CommonDataType dbType, bool nullable = false, string? defaultValue = null);

    /// <summary>
    /// Create unique constraint
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyNames">Properties to be included in the constraint</param>
    Task CreateUniqueConstraint<T>(string[] propertyNames);

    /// <summary>
    /// Create foreign key constraint
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="sourceProperty">DbAccessSource property</param>
    /// <param name="targetProperty">Target property (default: Id)</param>
    Task CreateForeignKeyConstraint<TSource, TTarget>(string sourceProperty, string targetProperty = "Id");
}