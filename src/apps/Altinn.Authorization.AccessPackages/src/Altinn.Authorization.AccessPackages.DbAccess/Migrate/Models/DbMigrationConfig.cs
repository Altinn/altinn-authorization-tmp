﻿using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;

/// <summary>
/// Database Migration Configuration
/// </summary>
public class DbMigrationConfig
{

    public DbMigrationConfig()
    {
        
    }

    public DbMigrationConfig(Action<DbMigrationConfig> configureOptions)
    {
        configureOptions?.Invoke(this);
    }

    /// <summary>
    /// ConnectionString
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Collection Id
    /// </summary>
    public string CollectionId { get; set; }

    /// <summary>
    /// Default schema
    /// </summary>
    public string DefaultSchema { get; set; }

    /// <summary>
    /// Translation schema
    /// </summary>
    public string TranslationSchema { get; set; }

    /// <summary>
    /// History schema
    /// </summary>
    public string HistorySchema { get; set; }

    /// <summary>
    /// UseSqlServer
    /// </summary>
    public bool UseSqlServer { get; set; }
}