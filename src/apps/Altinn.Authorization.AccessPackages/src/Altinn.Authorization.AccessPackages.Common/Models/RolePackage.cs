namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// RolePackage
/// </summary>
public class RolePackage
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
    /// IsActor
    /// TODO : Rename => HasAccess
    /// </summary>
    public bool IsActor { get; set; } // Is this the right place? Or is it Package Permission? No.... Or PackageResource?

    /// <summary>
    /// IsAdmin
    /// TODO : Rename => CanDelegate
    /// </summary>
    public bool IsAdmin { get; set; } // Is this the right place?
}

/// <summary>
/// Extended RolePackage
/// </summary>
public class ExtRolePackage : RolePackage
{
    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    public Package Package { get; set; }

    /// <summary>
    /// Variant (optional)
    /// </summary>
    public EntityVariant? EntityVariant { get; set; }
}
