using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Services.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts;

/// <summary>
/// Service for getting connections
/// </summary>
public interface IRelationService
{
    /// <summary>
    /// Get Connections given from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="packageId">Filter for package</param>
    /// <param name="resourceId">Filter for resource</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<RelationDto>> GetConnectionsToOthers(Guid partyId, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Connections recived from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="packageId">Filter for package</param>
    /// <param name="resourceId">Filter for resource</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<RelationDto>> GetConnectionsFromOthers(Guid partyId, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get connections given from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<CompactRelationDto>> GetConnectionsToOthers(Guid partyId, Guid? roleId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Connections recived from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<CompactRelationDto>> GetConnectionsFromOthers(Guid partyId, Guid? roleId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of packages with a list of parties you have this permission at
    /// </summary>
    Task<IEnumerable<PackagePermission>> GetPackagePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? packageId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of packages with a list of parties that have this permission
    /// </summary>
    Task<IEnumerable<PackagePermission>> GetPackagePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? packageId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of resources with a list of parties you have this permission at
    /// </summary>
    Task<IEnumerable<ResourcePermission>> GetResourcePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of resources with a list of parties that have this permission
    /// </summary>
    Task<IEnumerable<ResourcePermission>> GetResourcePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? resourceId = null, CancellationToken cancellationToken = default);

    [Obsolete]
    /// <summary>
    /// List entities that has this package from given party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="packageId">Filter for package</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPermission>> GetPackagePermissionsFrom(Guid partyId, Guid packageId, CancellationToken cancellationToken = default);

    [Obsolete]
    /// <summary>
    /// List entities that the party has this package at
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="packageId">Filter for package</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPermission>> GetPackagePermissionsTo(Guid partyId, Guid packageId, CancellationToken cancellationToken = default);

    [Obsolete]
    /// <summary>
    /// Get packages the party has given
    /// </summary>
    /// <param name="partyId">Party</param>
    /// <param name="toId">Given to this party</param>
    /// <param name="packageId">Optional filter for single package</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<IEnumerable<CompactPackage>> GetPackagesFrom(Guid partyId, Guid? toId = null, Guid? packageId = null, CancellationToken cancellationToken = default);

    [Obsolete]
    /// <summary>
    /// Get packages the party has received
    /// </summary>
    /// <param name="partyId">Party</param>
    /// <param name="fromId">Given from this party</param>
    /// <param name="packageId">Optional filter for single package</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<IEnumerable<CompactPackage>> GetPackagesTo(Guid partyId, Guid? fromId = null, Guid? packageId = null, CancellationToken cancellationToken = default);
}
