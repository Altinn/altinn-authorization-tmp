﻿using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// Database Object Configuration
/// </summary>
public class DbObjDefConfig
{
    /// <summary>
    /// Base schema
    /// </summary>
    public string BaseSchema { get; set; }

    /// <summary>
    /// Translation schema
    /// </summary>
    public string TranslationSchema { get; set; }

    /// <summary>
    /// History schema
    /// </summary>
    public string HistorySchema { get; set; }

    /// <summary>
    /// Ovbjects with translations
    /// </summary>
    public List<string> TranslateObjects { get; set; }

    /// <summary>
    /// Objects with history
    /// </summary>
    public List<string> HistoryObjects { get; set; }
}