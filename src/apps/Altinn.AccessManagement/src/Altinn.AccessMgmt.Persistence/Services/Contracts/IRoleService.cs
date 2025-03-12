using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts;

public interface IRoleService
{
    Task<Role> CreateRole(string name, string code, string description);
    Task<Role> GetRole(string name);
}
