using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Connection;

namespace Altinn.AccessManagement.Api.Enduser.Mappers;

/// <summary>
/// Temporary bridge mappers to convert from legacy persistence types to new DTOs
/// This will be removed once the service layer is refactored to return Core models
/// </summary>
public static class LegacyBridgeMappers
{
    /// <summary>
    /// Converts legacy CompactRelationDto to new CompactConnectionDto
    /// </summary>
    /// <param name="legacy">Legacy compact relation DTO</param>
    /// <returns>New compact connection DTO</returns>
    public static CompactConnectionDto ToDto(CompactRelationDto legacy)
    {
        return new CompactConnectionDto
        {
            Party = ToDto(legacy.Party),
            Roles = legacy.Roles.Select(ToDto).ToList(),
            Connections = legacy.Connections.Select(ToDto).ToList()
        };
    }

    /// <summary>
    /// Converts legacy CompactEntity to new PartyDto
    /// </summary>
    /// <param name="legacy">Legacy compact entity</param>
    /// <returns>New party DTO</returns>
    public static PartyDto ToDto(CompactEntity legacy)
    {
        return new PartyDto
        {
            Id = legacy.Id,
            Name = legacy.Name,
            PartyType = legacy.PartyType,
            OrganizationNumber = legacy.OrganizationNumber,
            PersonId = legacy.PersonId
        };
    }

    /// <summary>
    /// Converts legacy CompactRole to new RoleDto
    /// </summary>
    /// <param name="legacy">Legacy compact role</param>
    /// <returns>New role DTO</returns>
    public static RoleDto ToDto(CompactRole legacy)
    {
        return new RoleDto
        {
            Id = legacy.Id,
            Name = legacy.Name,
            Description = legacy.Description
        };
    }

    /// <summary>
    /// Converts legacy Assignment to new AssignmentDto
    /// </summary>
    /// <param name="legacy">Legacy assignment</param>
    /// <returns>New assignment DTO</returns>
    public static AssignmentDto ToDto(Assignment legacy)
    {
        return new AssignmentDto
        {
            Id = legacy.Id,
            RoleId = legacy.RoleId,
            FromId = legacy.FromId,
            ToId = legacy.ToId,
            Status = "Active", // Default value since legacy doesn't have status
            CreatedAt = legacy.CreatedAt,
            ModifiedAt = legacy.ModifiedAt
        };
    }

    /// <summary>
    /// Converts legacy PackagePermission to new PackagePermissionDto
    /// </summary>
    /// <param name="legacy">Legacy package permission</param>
    /// <returns>New package permission DTO</returns>
    public static PackagePermissionDto ToDto(PackagePermission legacy)
    {
        return new PackagePermissionDto
        {
            PackageId = legacy.PackageId,
            PackageName = legacy.PackageName,
            PackageDescription = legacy.PackageDescription,
            Permissions = legacy.Permissions.Select(ToDto).ToList(),
            Resources = legacy.Resources.Select(ToDto).ToList()
        };
    }

    /// <summary>
    /// Converts legacy CompactPermission to new PermissionDto
    /// </summary>
    /// <param name="legacy">Legacy compact permission</param>
    /// <returns>New permission DTO</returns>
    public static PermissionDto ToDto(CompactPermission legacy)
    {
        return new PermissionDto
        {
            Id = legacy.Id,
            Name = legacy.Name,
            Description = legacy.Description
        };
    }

    /// <summary>
    /// Converts legacy CompactResource to new ResourceDto
    /// </summary>
    /// <param name="legacy">Legacy compact resource</param>
    /// <returns>New resource DTO</returns>
    public static ResourceDto ToDto(CompactResource legacy)
    {
        return new ResourceDto
        {
            Id = legacy.Id,
            Name = legacy.Name,
            ResourceType = legacy.ResourceType
        };
    }

    /// <summary>
    /// Converts legacy AssignmentPackage to new AssignmentPackageDto
    /// </summary>
    /// <param name="legacy">Legacy assignment package</param>
    /// <returns>New assignment package DTO</returns>
    public static AssignmentPackageDto ToDto(AssignmentPackage legacy)
    {
        return new AssignmentPackageDto
        {
            Id = legacy.Id,
            AssignmentId = legacy.AssignmentId,
            PackageId = legacy.PackageId,
            PackageName = legacy.PackageName,
            PackageDescription = legacy.PackageDescription,
            Status = "Active", // Default value since legacy doesn't have status
            CreatedAt = legacy.CreatedAt,
            ModifiedAt = legacy.ModifiedAt
        };
    }

    /// <summary>
    /// Converts list of legacy CompactRelationDto to list of new CompactConnectionDto
    /// </summary>
    /// <param name="legacyList">List of legacy compact relation DTOs</param>
    /// <returns>List of new compact connection DTOs</returns>
    public static List<CompactConnectionDto> ToDto(IEnumerable<CompactRelationDto> legacyList)
    {
        return legacyList.Select(ToDto).ToList();
    }

    /// <summary>
    /// Converts list of legacy PackagePermission to list of new PackagePermissionDto
    /// </summary>
    /// <param name="legacyList">List of legacy package permissions</param>
    /// <returns>List of new package permission DTOs</returns>
    public static List<PackagePermissionDto> ToDto(IEnumerable<PackagePermission> legacyList)
    {
        return legacyList.Select(ToDto).ToList();
    }
}