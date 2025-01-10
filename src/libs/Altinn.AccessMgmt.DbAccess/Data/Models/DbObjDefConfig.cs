using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.DbAccess.Data.Models;

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
}
