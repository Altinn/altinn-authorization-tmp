using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <inheritdoc />
public class RoleService(IRoleRepository roleRepository, IRoleLookupRepository roleLookupRepository, IPackageRepository packageRepository) : IRoleService
{
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IRoleLookupRepository roleLookupRepository = roleLookupRepository;
    private readonly IPackageRepository packageRepository = packageRepository;

    /// <inheritdoc />
    public async Task<RoleDto> GetById(Guid id)
    {
        ExtRole extRole = await roleRepository.GetExtended(id);
        if (extRole == null)
        {
            return null;
        }

        return new RoleDto(extRole);
    }

    /// <inheritdoc/>
    public async Task<List<RoleDto>> GetAll()
    {
        var roles = await roleRepository.GetExtended(new RequestOptions());
        if (roles == null)
        { 
            return null; 
        }

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            roleDtos.Add(new RoleDto(role));
        }

        return roleDtos;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByProvider(Guid providerId)
    {
        var roles = await roleRepository.GetExtended(t => t.ProviderId, providerId);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            roleDtos.Add(new RoleDto(role));
        }

        return roleDtos;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByCode(string code)
    {
        var roles = await roleRepository.GetExtended(t => t.Code, code);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            roleDtos.Add(new RoleDto(role));
        }

        return roleDtos;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByKeyValue(string key, string value)
    {
        var filter = roleLookupRepository.CreateFilterBuilder();
        filter.Equal(t => t.Key, key);
        filter.Equal(t => t.Value, value);
        var res = await roleLookupRepository.GetExtended(filter);
        if (res == null || !res.Any())
        {
            return null;
        }

        var roleDtos = new List<RoleDto>();

        var roles = res.Select(t => t.Role);
        foreach (var role in roles)
        {
            roleDtos.Add(new RoleDto(role));
        }

        return roleDtos;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PackageDto>> GetPackagesForRole(Guid id)
    {
        var roles = await roleRepository.GetExtended(id);
        var packageDtos = new List<PackageDto>();
        var thing = await packageRepository.Get();

        return null;
    }
}
