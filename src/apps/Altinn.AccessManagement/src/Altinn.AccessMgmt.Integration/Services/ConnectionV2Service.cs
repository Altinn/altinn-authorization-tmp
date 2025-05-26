using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Integration.Contracts;
using Altinn.AccessMgmt.Integration.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;

namespace Altinn.AccessMgmt.Integration.Services;

/// <inheritdoc />
public class ConnectionV2Service(INewConnectionRepository repository) : IConnectionV2Service
{
    /// <inheritdoc />
    public async Task<List<ConnectionV2Dto>> GetConnectionsFrom(Guid? partyId = null, Guid? roleId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (partyId.HasValue)
        {
            filter.Equal(t => t.FromId, partyId.Value);
        }

        if (roleId.HasValue)
        {
            filter.Equal(t => t.RoleId, roleId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return GetConnectionsFrom(res);
    }

    /// <inheritdoc />
    public async Task<List<ConnectionV2Dto>> GetConnectionsTo(Guid? partyId = null, Guid? roleId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (partyId.HasValue)
        {
            filter.Equal(t => t.ToId, partyId.Value);
        }

        if (roleId.HasValue)
        {
            filter.Equal(t => t.RoleId, roleId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return GetConnectionsTo(res);
    }

    /// <inheritdoc />
    public async Task<List<ConnectionV2Dto>> GetConnectionsVia(Guid? partyId = null, Guid? roleId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (partyId.HasValue)
        {
            filter.Equal(t => t.ViaId, partyId.Value);
        }

        if (roleId.HasValue)
        {
            filter.Equal(t => t.RoleId, roleId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return GetConnectionsTo(res);
    }

    /// <inheritdoc />
    public async Task<List<ConnectionPermission>> GetPackagePermissions(Guid partyId, Guid packageId)
    {
        var result = new List<ConnectionPermission>();

        var filter = repository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, partyId);
        filter.Equal(t => t.PackageId, packageId);
        var res = await repository.GetExtended(filter);

        foreach (var connection in res.Where(t => t.Reason == "Direct").Select(t => t.To))
        {
            var perm = new ConnectionPermission()
            {
                Party = connection,
                KeyRoles = new List<Permission>(),
                Delegations = new List<Permission>()
            };

            perm.KeyRoles = res.Where(t => t.Reason == "KeyRole" && t.Via.Id == connection.Id).Select(t => ConvertToPermission(t)).ToList();
            perm.Delegations = res.Where(t => t.Reason == "Delegation" && t.Via.Id == connection.Id).Select(t => ConvertToPermission(t)).ToList();

            result.Add(perm);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CompactPackage>> GetPackagesFrom(Guid? partyId = null, Guid? toId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (partyId.HasValue)
        {
            filter.Equal(t => t.FromId, partyId.Value);
        }

        if (toId.HasValue)
        {
            filter.Equal(t => t.ToId, toId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return res.Select(t => t.Package);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CompactPackage>> GetPackagesTo(Guid? partyId = null, Guid? fromId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (partyId.HasValue)
        {
            filter.Equal(t => t.ToId, partyId.Value);
        }

        if (fromId.HasValue)
        {
            filter.Equal(t => t.FromId, fromId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return res.Select(t => t.Package);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CompactResource>> GetResources(Guid? fromId = null, Guid? toId = null, Guid? packageId = null)
    {
        var result = new List<ConnectionV2Dto>();

        var filter = repository.CreateFilterBuilder();

        if (fromId.HasValue)
        {
            filter.Equal(t => t.FromId, fromId.Value);
        }

        if (toId.HasValue)
        {
            filter.Equal(t => t.ToId, toId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await repository.GetExtended(filter);

        return res.Select(t => t.Resource);
    }

    private List<ConnectionV2Dto> GetConnectionsFrom(IEnumerable<ExtRelation> res)
    {
        var result = new List<ConnectionV2Dto>();

        var tempResult = new Dictionary<Guid, ConnectionV2Dto>(); // ViaId - Include Reason for Split in KeyRole & Delegation (?)
        foreach (var connection in res.Where(t => t.Reason != "Direct").DistinctBy(t => t.To.Id))
        {
            var party = connection.To;
            tempResult.Add(connection.Via.Id, new ConnectionV2Dto()
            {
                Party = party,
                Roles = res.Where(t => t.To.Id == party.Id).Select(t => t.Role).ToList(),
                Packages = res.Where(t => t.To.Id == party.Id).Select(t => t.Package).ToList(),
                Connections = new List<ConnectionV2Dto>(),
            });
        }

        foreach (var party in res.Where(t => t.Reason == "Direct").Select(t => t.To).DistinctBy(t => t.Id))
        {
            result.Add(new ConnectionV2Dto()
            {
                Party = party,
                Roles = res.Where(t => t.To.Id == party.Id).Select(t => t.Role).ToList(),
                Packages = res.Where(t => t.To.Id == party.Id).Select(t => t.Package).ToList(),
                Connections = tempResult.Where(t => t.Key == party.Id).Select(t => t.Value).ToList(), // Split in KeyRole & Delegation (?)
            });
        }

        return result;
    }
    
    private List<ConnectionV2Dto> GetConnectionsTo(IEnumerable<ExtRelation> res)
    {
        var result = new List<ConnectionV2Dto>();

        var tempResult = new Dictionary<Guid, ConnectionV2Dto>(); // ViaId
        foreach (var connection in res.Where(t => t.Reason != "Direct").DistinctBy(t => t.From.Id))
        {
            var fromParty = connection.From;
            tempResult.Add(connection.Via.Id, new ConnectionV2Dto()
            {
                Party = fromParty,
                Roles = res.Where(t => t.From.Id == fromParty.Id).Select(t => t.Role).ToList(),
                Packages = res.Where(t => t.From.Id == fromParty.Id).Select(t => t.Package).ToList(),
                Connections = new List<ConnectionV2Dto>(),
            });
        }

        foreach (var party in res.Where(t => t.Reason == "Direct").Select(t => t.From).DistinctBy(t => t.Id))
        {
            result.Add(new ConnectionV2Dto()
            {
                Party = party,
                Roles = res.Where(t => t.From.Id == party.Id).Select(t => t.Role).ToList(),
                Packages = res.Where(t => t.From.Id == party.Id).Select(t => t.Package).ToList(),
                Connections = tempResult.Where(t => t.Key == party.Id).Select(t => t.Value).ToList(),
            });
        }

        return result;
    }
    
    private Permission ConvertToPermission(ExtRelation connection)
    {
        return new Permission()
        {
            From = connection.From,
            To = connection.To,
            Via = connection.Via,
            ViaRole = connection.ViaRole,
            Role = connection.Role
        };
    }
}
