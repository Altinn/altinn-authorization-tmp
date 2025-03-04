using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Services.Contracts;

namespace Altinn.AccessMgmt.Persistence.Services;

public class RoleService : IRoleService
{
    public Task<Role> CreateRole(string name, string code, string description)
    {
        // GetProvider
        // GetEntityType
        var role = new Role()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            Urn = "",
            EntityTypeId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),            
        }
        throw new NotImplementedException();
    }

    public Task<Role> GetRole(string name)
    {
        throw new NotImplementedException();
    }
}
