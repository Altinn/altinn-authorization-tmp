using Altinn.AccessMgmt.Integration.Models;

namespace Altinn.AccessMgmt.Integration.Contracts;

/// <summary>
/// Service for getting connections
/// </summary>
public interface IConnectionV2Service
{
    /// <summary>
    /// Get Connections given from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="packageId">Filter for package</param>
    /// <returns></returns>
    Task<List<ConnectionV2Dto>> GetConnectionsFrom(Guid? partyId = null, Guid? roleId = null, Guid? packageId = null);

    /// <summary>
    /// Get Connections recived from party
    /// </summary>
    /// <param name="partyId">Filter for party</param>
    /// <param name="roleId">Filter for role</param>
    /// <param name="packageId">Filter for package</param>
    /// <returns></returns>
    Task<List<ConnectionV2Dto>> GetConnectionsTo(Guid? partyId = null, Guid? roleId = null, Guid? packageId = null);

    /// <summary>
    /// Get parties with access to package on party
    /// </summary>
    /// <param name="from">Party to check</param>
    /// <param name="package">Filter for packag</param>
    /// <returns></returns>
    Task<List<ConnectionPermission>> GetPackagePermissions(Guid from, Guid package);
}
