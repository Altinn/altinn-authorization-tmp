using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Package permissions
/// </summary>
public class PackagePermissionDto
{
    /// <summary>
    /// Package the permissions are for
    /// </summary>
    public CompactPackage Package { get; set; }

    /// <summary>
    /// Parties with permissions
    /// </summary>
    public IEnumerable<PermissionDto> Permissions { get; set; }
}
