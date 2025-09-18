using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Compact;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper { }

/// <summary>
/// The DtoMapper is a partial class for converting database models and dto models
/// Create a new file for the diffrent areas
/// </summary>
public partial class DtoMapper
{
    public IEnumerable<RelationPackageDto> ExtractRelationPackageDtoToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.To.Id).Select(relation => new RelationPackageDto()
        {
            Party = Convert(relation.To),
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.To.Id == relation.To.Id && t.Package != null).Select(t => ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.To.Id).ToList() : new()
        });
    }

    public IEnumerable<RelationPackageDto> ExtractSubRelationPackageDtoFromOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.From.Id).Select(relation => new RelationPackageDto()
        {
            Party = Convert(relation.From),
            Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.From.Id == relation.From.Id && t.Package != null).Select(t => ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractSubRelationDtoFromOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.From.Id).Select(relation => new RelationDto()
        {
            Party = Convert(relation.From),
            Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractRelationDtoToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
        {
            Party = Convert(relation.To),
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.To.Id).ToList() : new()
        });
    }

    public IEnumerable<RelationDto> ExtractSubRelationDtoToOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
        {
            Party = Convert(relation.To),
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationPackageDto> ExtractSubRelationPackageDtoToOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.To.Id).Select(relation => new RelationPackageDto()
        {
            Party = Convert(relation.To),
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.To.Id == relation.To.Id && t.Package != null).Select(t => ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractRelationDtoFromOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.From.Id).Select(relation => new RelationDto()
        {
            Party = Convert(relation.From),
            Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.From.Id).ToList() : new()
        });
    }

    public IEnumerable<RelationPackageDto> ExtractRelationPackageDtoFromOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.From.Id).Select(relation => new RelationPackageDto()
        {
            Party = Convert(relation.From),
            Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => Convert(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.From.Id == relation.From.Id && t.Package != null).Select(t => ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.From.Id).ToList() : new()
        });
    }

    public CompactPermission ConvertToCompactPermission(Relation connection)
    {
        return new CompactPermission()
        {
            From = connection.From,
            To = connection.To
        };
    }

    public PermissionDto ConvertToPermission(Relation connection)
    {
        return new PermissionDto()
        {
            From = Convert(connection.From),
            To = Convert(connection.To),
            Via = Convert(connection.Via),
            ViaRole = Convert(connection.ViaRole),
            Role = Convert(connection.Role)
        };
    }

    public CompactEntityDto Convert(CompactEntity compactEntity)
    {
        return new CompactEntityDto()
        {
            Id = compactEntity.Id,
            Name = compactEntity.Name,
            Type = compactEntity.Type,
            Variant = compactEntity.Variant,
            Parent = Convert(compactEntity.Parent),
            Children = compactEntity.Children.Select(Convert).ToList(),
        };
    }

    public CompactRoleDto Convert(CompactRole role) 
    {
        return new CompactRoleDto()
        {
            Id = role.Id,
            Children = role.Children.Select(Convert).ToList(),
            Code = role.Code
        };
    }

    public CompactPackageDto ConvertCompactPackage(CompactPackage package)
    {
        return new CompactPackageDto()
        {
            Id = package.Id,
            AreaId = package.AreaId,
            Urn = package.Urn
        };
    }
}
public interface IDtoMapper { }
