using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

/// <summary>
/// Database Ingest Service
/// </summary>
public interface IDatabaseIngest
{
    /// <summary>
    /// Ingest all
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<List<IngestResult>> IngestAll(CancellationToken cancellationToken = default);
}