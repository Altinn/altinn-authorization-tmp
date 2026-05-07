namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Service for looking up OED (Digitalt dødsbo) role assignments.
/// This is an external dependency that calls an external API and must be retained.
/// </summary>
public interface IOedRoleAssignmentService
{
    /// <summary>
    /// Gets OED role assignments between a deceased party and an inheriting party.
    /// </summary>
    /// <param name="fromPersonId">The deceased person's identifier (SSN).</param>
    /// <param name="toPersonId">The inheriting person's identifier (SSN).</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A list of OED role codes assigned between the parties.</returns>
    Task<List<string>> GetOedRoleCodes(string fromPersonId, string toPersonId, CancellationToken cancellationToken = default);
}
