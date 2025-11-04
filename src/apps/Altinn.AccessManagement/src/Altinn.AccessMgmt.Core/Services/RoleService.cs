using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class RoleService: IRoleService
{
    public RoleService(AppDbContext appDbContext)
    {
        Db = appDbContext;
    }

    public AppDbContext Db { get; }

    /// <inheritdoc />
    public async Task<RoleDto> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await Db.Roles.AsNoTracking().Include(t => t.Provider).Include(t => t.EntityType).SingleAsync(t => t.Id == id, cancellationToken);
        if (role == null)
        {
            return null;
        }

        var roleDto = DtoMapper.Convert(role);

        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);

        return roleDto;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RoleDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var roles = await Db.Roles.AsNoTracking().Include(t => t.Provider).Include(t => t.EntityType).ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(DtoMapper.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByProvider(Guid providerId, CancellationToken cancellationToken = default)
    {
        var roles = await Db.Roles.AsNoTracking().Include(t => t.Provider).Include(t => t.EntityType).Where(t => t.ProviderId == providerId).ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(DtoMapper.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByCode(string code, CancellationToken cancellationToken = default)
    {
        var roles = await Db.Roles.AsNoTracking().Include(t => t.Provider).Include(t => t.EntityType).Where(t => t.Code == code).ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(DtoMapper.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoleDto> GetByKeyValue(string key, string value, CancellationToken cancellationToken = default)
    {
        var res = (await Db.RoleLookups.AsNoTracking()
            .Include(t => t.Role).ThenInclude(t => t.Provider)
            .Include(t => t.Role).ThenInclude(t => t.EntityType)
            .ToListAsync(cancellationToken)
            ).FirstOrDefault(t => t.Key.Contains(key, StringComparison.OrdinalIgnoreCase) && t.Value.Contains(value, StringComparison.OrdinalIgnoreCase));

        if (res == null)
        {
            return null;
        }

        var roleDto = DtoMapper.Convert(res.Role);
        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);
        return roleDto;
    }

    private async Task<List<ResourceDto>> GetRoleResources(Guid roleId)
    {
        var resources = await (
            from rr in Db.RoleResources
            where rr.RoleId == roleId
            join res in Db.Resources on rr.ResourceId equals res.Id
            select res
        ).ToListAsync();

        return resources.Select(DtoMapper.Convert).ToList();
    }

    private async Task<List<PackageDto>> GetPackagesForRole(Guid roleId, Guid? variantId = null)
    {
        var rawData = await (
            from rp in Db.RolePackages
            where rp.RoleId == roleId && rp.EntityVariantId == variantId
            join p in Db.Packages on rp.PackageId equals p.Id
            join pr in Db.PackageResources on p.Id equals pr.PackageId into prGroup
            from pr in prGroup.DefaultIfEmpty() // Left join
            join res in Db.Resources on pr.ResourceId equals res.Id into resGroup
            from res in resGroup.DefaultIfEmpty() // Left join
            select new { Package = p, Resource = res }
        ).ToListAsync();

        return rawData
            .GroupBy(x => x.Package)
            .Select(g => new PackageDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Description = g.Key.Description,
                IsDelegable = g.Key.IsDelegable,
                IsAssignable = g.Key.IsAssignable,
                Urn = g.Key.Urn,
                Resources = g.Where(x => x.Resource != null)
                             .Select(x => DtoMapper.Convert(x.Resource))
                             .DistinctBy(r => r.Id)
                             .ToList()
            })
            .ToList();
    }

    public async Task<IEnumerable<RoleVariantPrivilegeDto>> GetPrivileges(Guid? roleId = null, Guid? variantId = null)
    {
        var result = new List<RoleVariantPrivilegeDto>();

        // Hent roller
        var roles = await Db.Roles
            .AsNoTracking()
            .WhereIf(roleId.HasValue, r => r.Id == roleId.Value)
            .ToListAsync();

        // Hent varianter for disse rollene
        var variants = await Db.EntityVariantRoles
            .AsNoTracking()
            .Include(t => t.Variant)
            .Include(t => t.Role)
            .WhereIf(variantId.HasValue, v => v.Id == variantId.Value)
            .Where(v => roles.Select(r => r.Id).Contains(v.RoleId))
            .ToListAsync();

        foreach (var role in roles)
        {
            var roleResources = await GetRoleResources(role.Id);
            var rolePackages = await GetPackagesForRole(role.Id, null);

            var roleVariants = variants.Where(v => v.RoleId == role.Id).ToList();

            if (roleVariants.Any())
            {
                foreach (var variant in roleVariants)
                {
                    var variantPackages = await GetPackagesForRole(role.Id, variant.Id);

                    // Kombiner variant-pakker med generelle pakker og fjern duplikater
                    var combinedPackages = variantPackages
                        .Concat(rolePackages)
                        .GroupBy(p => p.Id)
                        .Select(g => g.First())
                        .ToList();

                    if ((roleResources.Any() || rolePackages.Any()) || (variantPackages.Any()))
                    {
                        result.Add(new RoleVariantPrivilegeDto
                        {
                            Role = DtoMapper.Convert(role),
                            Variant = DtoMapper.Convert(variant.Variant),
                            Resources = roleResources,
                            Packages = combinedPackages
                        });
                    }
                }
            }
            else
            {
                if (roleResources.Any() || rolePackages.Any())
                {
                    result.Add(new RoleVariantPrivilegeDto
                    {
                        Role = DtoMapper.Convert(role),
                        Variant = null,
                        Resources = roleResources,
                        Packages = rolePackages
                    });
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetLookupKeys(CancellationToken cancellationToken = default)
    {
        return await Db.RoleLookups.AsNoTracking().Select(t => t.Key).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetRoleResources(Guid id, CancellationToken cancellationToken)
    {
        return await Db.RoleResources.AsNoTracking().Where(t => t.RoleId == id).Include(t => t.Resource).Select(t => t.Resource).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetRolePackageResources(Guid id, CancellationToken cancellationToken)
    {
        var packages = await GetPackagesForRole(id, cancellationToken);
        return await Db.PackageResources.AsNoTracking().Where(t => packages.Select(p => p.Id).Contains(t.PackageId)).Select(r => r.Resource).ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RolePackageDto>> GetPackagesForRole(Guid id, CancellationToken cancellationToken = default)
    {
        var rolePackages = await Db.RolePackages.AsNoTracking().Where(t => t.RoleId == id)
            .Include(t => t.Role)
            .Include(t => t.Package)
            .Include(t => t.EntityVariant)
            .ToListAsync(cancellationToken);

        if (rolePackages == null)
        {
            return null;
        }

        var roleDto = DtoMapper.Convert(rolePackages.First().Role);
        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);

        var rolePackageDtos = new List<RolePackageDto>();
        foreach (var rolePackage in rolePackages)
        {
            var rolePackageDto = DtoMapper.Convert(rolePackage);
            rolePackageDto.Role = roleDto;
            rolePackageDtos.Add(rolePackageDto);
        }

        return rolePackageDtos;
    }

    private async Task<IEnumerable<RoleDto>> GetLegacyRoleCodeAndUrn(IEnumerable<RoleDto> roles, CancellationToken cancellationToken = default)
    {
        var roleLookup = await Db.RoleLookups.AsNoTracking().ToListAsync();
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var legacyRoleCode = roleLookup.FirstOrDefault(x => x.RoleId == role.Id && x.Key == "LegacyCode");
            if (legacyRoleCode != null)
            {
                role.LegacyRoleCode = legacyRoleCode.Value;
                role.LegacyUrn = $"urn:altinn:rolecode:{legacyRoleCode.Value}";
            }

            roleDtos.Add(role);
        }

        return roleDtos;
    }

    private async Task GetSingleLegacyRoleCodeAndUrn(RoleDto extRole, CancellationToken cancellationToken = default)
    {
        var legacyRoleCode = await Db.RoleLookups.AsNoTracking().Where(t => t.RoleId == extRole.Id && t.Key == "LegacyCode").FirstOrDefaultAsync();
        if (legacyRoleCode != null)
        {
            extRole.LegacyRoleCode = legacyRoleCode.Value;
            extRole.LegacyUrn = $"urn:altinn:rolecode:{legacyRoleCode.Value}";
        }
    }
}
