using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;

/// <summary>
/// IngestService
/// </summary>
/// <typeparam name="T">Type to be ingested</typeparam>
/// <typeparam name="TRepo">ExtendedRepo to be used to ingest</typeparam>
public interface IIngestService<T, TRepo>
    where TRepo : IDbBasicDataService<T>
{
    /// <summary>
    /// ExtendedRepo to be used to ingest
    /// </summary>
    TRepo DataService { get; }

    /// <summary>
    /// Action to ingest data
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>IngestResult</returns>
    Task<IngestResult> IngestData(CancellationToken cancellationToken);

    /// <summary>
    /// Hints if translations are available
    /// </summary>
    bool LoadTranslations { get; set; }
}