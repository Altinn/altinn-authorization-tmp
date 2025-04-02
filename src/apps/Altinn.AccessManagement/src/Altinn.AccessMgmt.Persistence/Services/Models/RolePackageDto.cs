using Altinn.AccessMgmt.Core.Models;
using System;

namespace Altinn.AccessMgmt.Persistence.Services.Models;

/// <summary>
/// RolePackage
/// </summary>
public class RolePackageDto
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// RoleId
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// PackageId
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// EntityVariantId (optional)
    /// </summary>
    public Guid? EntityVariantId { get; set; }

    /// <summary>
    /// HasAccess
    /// </summary>
    public bool HasAccess { get; set; }

    /// <summary>
    /// CanDelegate
    /// </summary>
    public bool CanDelegate { get; set; }

    /// <summary>
    /// Construct from RolePackage
    /// </summary>
    /// <param name="rolePackage"><see cref="RolePackage"/>Role</param>
    public RolePackageDto(RolePackage rolePackage)
    {
        Id = rolePackage.Id;
        RoleId = rolePackage.RoleId;
        PackageId = rolePackage.PackageId;
        EntityVariantId = rolePackage.EntityVariantId;
        HasAccess = rolePackage.HasAccess;
        CanDelegate = rolePackage.CanDelegate;

    }

    /// <summary>
    /// Construct from RolePackage
    /// </summary>
    /// <param name="rolePackage"><see cref="RolePackage"/>Role</param>
    public RolePackageDto(ExtRolePackage rolePackage)
    {
        Id = rolePackage.Id;
        RoleId = rolePackage.RoleId;
        PackageId = rolePackage.PackageId;
        EntityVariantId = rolePackage.EntityVariantId;
        HasAccess = rolePackage.HasAccess;
        CanDelegate = rolePackage.CanDelegate;

    }
}
