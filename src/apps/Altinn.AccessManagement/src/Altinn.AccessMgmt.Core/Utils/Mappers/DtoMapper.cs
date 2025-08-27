using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.Core.Utils;

/// <summary>
/// Dto Mapping
/// </summary>
public partial class DtoMapper
{
    public IEnumerable<RelationPackageDto> ExtractRelationPackageDtoToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new RelationPackageDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }
    
    public IEnumerable<RelationPackageDto> ExtractSubRelationPackageDtoFromOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new RelationPackageDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractSubRelationDtoFromOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new RelationDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractRelationDtoToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new RelationDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }

    public IEnumerable<RelationDto> ExtractSubRelationDtoToOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationPackageDto> ExtractSubRelationPackageDtoToOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.ToId).Select(relation => new RelationPackageDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    public IEnumerable<RelationDto> ExtractRelationDtoFromOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new RelationDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
        });
    }

    public IEnumerable<RelationPackageDto> ExtractRelationPackageDtoFromOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new RelationPackageDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
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
            From = connection.From,
            To = connection.To,
            Via = connection.Via,
            ViaRole = connection.ViaRole,
            Role = connection.Role
        };
    }
}
