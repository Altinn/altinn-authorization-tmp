namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Connection from one party to another
/// </summary>
public class ConnectionDto
{
    /// <summary>
    /// Party
    /// </summary>
<<<<<<<< HEAD:src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/AccessManagement/RelationDto.cs
    public CompactEntityDto Party { get; set; } = new();
========
    public Entity Party { get; set; } = new();
>>>>>>>> main:src/apps/Altinn.AccessManagement/src/Altinn.AccessMgmt.Core/Models/ConnectionDto.cs

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
<<<<<<<< HEAD:src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/AccessManagement/RelationDto.cs
    public List<CompactRoleDto> Roles { get; set; } = new();
========
    public List<Role> Roles { get; set; } = new();
>>>>>>>> main:src/apps/Altinn.AccessManagement/src/Altinn.AccessMgmt.Core/Models/ConnectionDto.cs

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<ConnectionDto> Connections { get; set; } = new();
}

/// <summary>
/// Connection from one party to another
/// </summary>
public class ConnectionPackageDto
{
    /// <summary>
    /// Party
    /// </summary>
<<<<<<<< HEAD:src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/AccessManagement/RelationDto.cs
    public CompactEntityDto Party { get; set; } = new();
========
    public Entity Party { get; set; } = new();
>>>>>>>> main:src/apps/Altinn.AccessManagement/src/Altinn.AccessMgmt.Core/Models/ConnectionDto.cs

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
<<<<<<<< HEAD:src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/AccessManagement/RelationDto.cs
    public List<CompactRoleDto> Roles { get; set; } = new();
========
    public List<Role> Roles { get; set; } = new();
>>>>>>>> main:src/apps/Altinn.AccessManagement/src/Altinn.AccessMgmt.Core/Models/ConnectionDto.cs

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<ConnectionDto> Connections { get; set; } = new();

    /// <summary>
    /// Packages the party has
    /// </summary>
<<<<<<<< HEAD:src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/AccessManagement/RelationDto.cs
    public List<CompactPackageDto> Packages { get; set; } = new();
========
    public List<Package> Packages { get; set; } = new();
>>>>>>>> main:src/apps/Altinn.AccessManagement/src/Altinn.AccessMgmt.Core/Models/ConnectionDto.cs
}
