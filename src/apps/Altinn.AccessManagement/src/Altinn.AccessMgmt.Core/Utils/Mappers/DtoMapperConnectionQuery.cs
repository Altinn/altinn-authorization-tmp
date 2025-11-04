using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper
{
    public static List<ConnectionDto> Convert(IEnumerable<ConnectionQueryExtendedRecord> res)
    {
        var connections = res.GroupBy(res => res.To.Id);
        var result = connections.Select(c =>
        {
            var connection = c.First();

            return new ConnectionDto()
            {
                Party = Convert(connection.To),
                Roles = [.. c.Select(c => ConvertCompactRole(c.Role)).DistinctBy(t => t.Id)],
                Resources = [.. c.SelectMany(c => c.Resources).Select(r => Convert(r)).DistinctBy(t => t.Id)],
                Packages = [.. c.SelectMany(c => c.Packages).Select(p => Convert(p)).DistinctBy(t => t.Id)],
                Connections = ConvertSubConnections(c, connection.To.Id),
            };
        });

        return [.. result];
    }

    public static List<ConnectionDto> ConvertSubConnections(IEnumerable<ConnectionQueryExtendedRecord> res, Guid party)
    {
        var result = res.Where(t => t.Reason != ConnectionReason.Delegation && t.ViaId == party)
            .DistinctBy(t => t.FromId)
            .Select(relation => new ConnectionDto()
            {
                Party = Convert(relation.From),
                Roles = res
                    .Where(t => t.FromId == relation.FromId)
                    .Select(t => ConvertCompactRole(t.Role))
                    .DistinctBy(t => t.Id).ToList(),
                Packages = res
                    .Where(t => t.FromId == relation.FromId && t.Packages != null)
                    .SelectMany(t => t.Packages)
                    .Select(p => Convert(p))
                    .DistinctBy(t => t.Id)
                    .ToList(),
                Resources = res
                    .Where(t => t.FromId == relation.FromId && t.Resources != null)
                    .SelectMany(t => t.Resources)
                    .Select(r => Convert(r))
                    .DistinctBy(t => t.Id)
                    .ToList(),

                Connections = new()
            });

        return result.ToList();
    }

    public static AccessPackageDto Convert(ConnectionQueryPackage package)
    {
        return new AccessPackageDto()
        {
            Id = package.Id,
            Urn = package.Urn,
            AreaId = package.AreaId,
        };
    }

    public static ResourceDto Convert(ConnectionQueryResource resource)
    {
        return new ResourceDto()
        {
            Id = resource.Id,
            Name = resource.Name,
        };
    }
}
