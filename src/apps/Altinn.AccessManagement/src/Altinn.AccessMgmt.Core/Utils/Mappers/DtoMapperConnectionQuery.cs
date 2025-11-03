using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Utils;

/// <inheritdoc/>
public partial class DtoMapper : IDtoMapper
{
    public static List<ConnectionDto> Convert(IEnumerable<ConnectionQueryExtendedRecord> res, bool includeSubConnections = true)
    {
        var connections = res.GroupBy(res => res.To.Id);

        var result = connections.Select(c =>
        {
            var party = c.First().To;
            var connections = res.Where(r => r.ToId != r.ToId);

            return new ConnectionDto()
            {
                Party = Convert(party),
                Roles = [.. c.Select(c => ConvertCompactRole(c.Role)).DistinctBy(t => t.Id)],
                Resources = [.. c.SelectMany(c => c.Resources).Select(r => Convert(r)).DistinctBy(t => t.Id)],
                Packages = [.. c.SelectMany(c => c.Packages).Select(p => Convert(p)).DistinctBy(t => t.Id)],
                Connections = includeSubConnections ? Convert(connections, false) : [],
            };
        });

        return result.ToList();
    }

    public static AccessPackageDto Convert(ConnectionQueryPackage package)
    {
        return new AccessPackageDto()
        {
            Id = package.Id,
            Urn = package.Urn,
            AreaId = package.AreaId
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
