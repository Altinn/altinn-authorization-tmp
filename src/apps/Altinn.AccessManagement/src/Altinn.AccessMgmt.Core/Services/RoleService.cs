﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class RoleService(AppDbContext db, DtoConverter dtoConverter, AuditValues auditValues) : IRoleService
{
    /// <inheritdoc />
    public async Task<RoleDto> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles.AsNoTracking().SingleAsync(t => t.Id == id, cancellationToken);
        if (role == null)
        {
            return null;
        }

        var roleDto = dtoConverter.Convert(role);

        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);

        return roleDto;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RoleDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var roles = await db.Roles.AsNoTracking().ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(dtoConverter.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByProvider(Guid providerId, CancellationToken cancellationToken = default)
    {
        var roles = await db.Roles.AsNoTracking().Where(t => t.ProviderId == providerId).ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(dtoConverter.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RoleDto>> GetByCode(string code, CancellationToken cancellationToken = default)
    {
        var roles = await db.Roles.AsNoTracking().Where(t => t.Code == code).ToListAsync(cancellationToken);
        if (roles == null)
        {
            return null;
        }

        var roleDtos = roles.Select(dtoConverter.Convert);
        return await GetLegacyRoleCodeAndUrn(roleDtos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoleDto> GetByKeyValue(string key, string value, CancellationToken cancellationToken = default)
    {
        var res = await db.RoleLookups.AsNoTracking().Where(t => t.Key.Contains(key) && t.Value.Contains(value)).Include(t => t.Role).SingleAsync(cancellationToken);
        if (res == null)
        {
            return null;
        }

        var roleDto = dtoConverter.Convert(res.Role);
        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);
        return roleDto;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetLookupKeys(CancellationToken cancellationToken = default)
    {
        return await db.RoleLookups.AsNoTracking().Select(t => t.Key).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetRoleResources(Guid id, CancellationToken cancellationToken)
    {
        return await db.RoleResources.AsNoTracking().Where(t => t.RoleId == id).Include(t => t.Resource).Select(t => t.Resource).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetRolePackageResources(Guid id, CancellationToken cancellationToken)
    {
        var packages = await GetPackagesForRole(id, cancellationToken);
        return await db.PackageResources.AsNoTracking().Where(t => packages.Select(p => p.Id).Contains(t.PackageId)).Select(r => r.Resource).ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RolePackageDto>> GetPackagesForRole(Guid id, CancellationToken cancellationToken = default)
    {
        var rolePackages = await db.RolePackages.AsNoTracking().Where(t => t.RoleId == id).ToListAsync(cancellationToken);
        if (rolePackages == null)
        {
            return null;
        }

        var roleDto = dtoConverter.Convert(rolePackages.First().Role);
        await GetSingleLegacyRoleCodeAndUrn(roleDto, cancellationToken);

        var rolePackageDtos = new List<RolePackageDto>();
        foreach (var rolePackage in rolePackages)
        {
            var rolePackageDto = new RolePackageDto(rolePackage);
            rolePackageDto.Role = roleDto;
            rolePackageDtos.Add(rolePackageDto);
        }

        return rolePackageDtos;
    }

    private async Task<IEnumerable<RoleDto>> GetLegacyRoleCodeAndUrn(IEnumerable<RoleDto> roles, CancellationToken cancellationToken = default)
    {
        var roleLookup = await db.RoleLookups.AsNoTracking().ToListAsync();
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
        var legacyRoleCode = await db.RoleLookups.AsNoTracking().Where(t => t.RoleId == extRole.Id && t.Key == "LegacyCode").SingleAsync();
        if (legacyRoleCode != null)
        {
            extRole.LegacyRoleCode = legacyRoleCode.Value;
            extRole.LegacyUrn = $"urn:altinn:rolecode:{legacyRoleCode.Value}";
        }
    }
}
