using Altinn.Authorization.Api.Contracts.AccessManagement.Connection;
using CoreModels = Altinn.AccessManagement.Core.Models.Connection;

namespace Altinn.AccessManagement.Api.Enduser.Mappers;

/// <summary>
/// Mappers for converting between API DTOs and Core models for Connection functionality
/// </summary>
public static class ConnectionMappers
{
    /// <summary>
    /// Converts ConnectionInputDto to Core ConnectionInput model
    /// </summary>
    /// <param name="dto">The connection input DTO from API</param>
    /// <returns>Core model for connection input</returns>
    public static CoreModels.ConnectionInput ToCore(ConnectionInputDto dto)
    {
        return new CoreModels.ConnectionInput
        {
            Party = dto.Party,
            From = dto.From,
            To = dto.To
        };
    }

    /// <summary>
    /// Converts Core CompactConnection to API DTO
    /// </summary>
    /// <param name="core">The core compact connection model</param>
    /// <returns>API DTO for compact connection</returns>
    public static CompactConnectionDto ToDto(CoreModels.CompactConnection core)
    {
        return new CompactConnectionDto
        {
            Party = ToDto(core.Party),
            Roles = core.Roles.Select(ToDto).ToList(),
            Connections = core.Connections.Select(ToDto).ToList()
        };
    }

    /// <summary>
    /// Converts Core Party to API DTO
    /// </summary>
    /// <param name="core">The core party model</param>
    /// <returns>API DTO for party</returns>
    public static PartyDto ToDto(CoreModels.Party core)
    {
        return new PartyDto
        {
            Id = core.Id,
            Name = core.Name,
            PartyType = core.PartyType,
            OrganizationNumber = core.OrganizationNumber,
            PersonId = core.PersonId
        };
    }

    /// <summary>
    /// Converts Core Role to API DTO
    /// </summary>
    /// <param name="core">The core role model</param>
    /// <returns>API DTO for role</returns>
    public static RoleDto ToDto(CoreModels.Role core)
    {
        return new RoleDto
        {
            Id = core.Id,
            Name = core.Name,
            Description = core.Description
        };
    }

    /// <summary>
    /// Converts Core Assignment to API DTO
    /// </summary>
    /// <param name="core">The core assignment model</param>
    /// <returns>API DTO for assignment</returns>
    public static AssignmentDto ToDto(CoreModels.Assignment core)
    {
        return new AssignmentDto
        {
            Id = core.Id,
            RoleId = core.RoleId,
            FromId = core.FromId,
            ToId = core.ToId,
            Status = core.Status,
            CreatedAt = core.CreatedAt,
            ModifiedAt = core.ModifiedAt
        };
    }

    /// <summary>
    /// Converts Core PackagePermission to API DTO
    /// </summary>
    /// <param name="core">The core package permission model</param>
    /// <returns>API DTO for package permission</returns>
    public static PackagePermissionDto ToDto(CoreModels.PackagePermission core)
    {
        return new PackagePermissionDto
        {
            PackageId = core.PackageId,
            PackageName = core.PackageName,
            PackageDescription = core.PackageDescription,
            Permissions = [.. core.Permissions.Select(ToDto)],
            Resources = [.. core.Resources.Select(ToDto)]
        };
    }

    /// <summary>
    /// Converts Core Permission to API DTO
    /// </summary>
    /// <param name="core">The core permission model</param>
    /// <returns>API DTO for permission</returns>
    public static PermissionDto ToDto(CoreModels.Permission core)
    {
        return new PermissionDto
        {
            Id = core.Id,
            Name = core.Name,
            Description = core.Description
        };
    }

    /// <summary>
    /// Converts Core Resource to API DTO
    /// </summary>
    /// <param name="core">The core resource model</param>
    /// <returns>API DTO for resource</returns>
    public static ResourceDto ToDto(CoreModels.Resource core)
    {
        return new ResourceDto
        {
            Id = core.Id,
            Name = core.Name,
            ResourceType = core.ResourceType
        };
    }

    /// <summary>
    /// Converts Core AssignmentPackage to API DTO
    /// </summary>
    /// <param name="core">The core assignment package model</param>
    /// <returns>API DTO for assignment package</returns>
    public static AssignmentPackageDto ToDto(CoreModels.AssignmentPackage core)
    {
        return new AssignmentPackageDto
        {
            Id = core.Id,
            AssignmentId = core.AssignmentId,
            PackageId = core.PackageId,
            PackageName = core.PackageName,
            PackageDescription = core.PackageDescription,
            Status = core.Status,
            CreatedAt = core.CreatedAt,
            ModifiedAt = core.ModifiedAt
        };
    }

    /// <summary>
    /// Converts list of Core CompactConnection to list of API DTOs
    /// </summary>
    /// <param name="coreList">List of core compact connection models</param>
    /// <returns>List of API DTOs for compact connections</returns>
    public static List<CompactConnectionDto> ToDto(IEnumerable<CoreModels.CompactConnection> coreList)
    {
        return [.. coreList.Select(ToDto)];
    }

    /// <summary>
    /// Converts list of Core PackagePermission to list of API DTOs
    /// </summary>
    /// <param name="coreList">List of core package permission models</param>
    /// <returns>List of API DTOs for package permissions</returns>
    public static List<PackagePermissionDto> ToDto(IEnumerable<CoreModels.PackagePermission> coreList)
    {
        return [.. coreList.Select(ToDto)];
    }
}