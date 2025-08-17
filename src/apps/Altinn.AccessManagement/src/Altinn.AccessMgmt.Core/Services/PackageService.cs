﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Utils.Models;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class PackageService(
    AppDbContext db, 
    DtoConverter dtoConverter,
    ISearchCache<PackageDto> searchPackageCache
    ) : IPackageService
{
    private readonly ISearchCache<PackageDto> searchPackageCache = searchPackageCache;

    /// <inheritdoc/>
    public async Task<IEnumerable<SearchObject<PackageDto>>> Search(string term, bool searchInResources = false, CancellationToken cancellationToken = default)
    {
        var data = await GetSearchData();

        if (string.IsNullOrEmpty(term))
        {
            return data.Select(t => new SearchObject<PackageDto>() { Object = t, Score = 0, Fields = [] });
        }

        bool detailed = false;

        var builder = new SearchPropertyBuilder<PackageDto>()
            .Add(pkg => pkg.Name, 2.0, FuzzynessLevel.High)
            .Add(pkg => pkg.Description, 0.8, FuzzynessLevel.Low)
            .Add(pkg => pkg.Area.Name, 1.5, FuzzynessLevel.Medium)
            .Add(pkg => pkg.Area.Group.Name, 1.3, FuzzynessLevel.Medium);

        if (searchInResources)
        {
            builder
                .AddCollection(pkg => pkg.Resources, r => r.Name, 1.2, FuzzynessLevel.High, detailed);
            //// .AddCollection(pkg => pkg.Resources, r => r.Description, 0.7, FuzzynessLevel.Low, detailed);
        }

        var results = FuzzySearch.PerformFuzzySearch(data, term, builder);

        foreach (var res in results.OrderByDescending(t => t.Score))
        {
            Console.WriteLine($"ID: {res.Object.Id}, Name: {res.Object.Name}, Score: {res.Score}");
            foreach (var f in res.Fields.OrderByDescending(t => t.Score))
            {
                if (f.Words.Count > 0)
                {
                    Console.WriteLine($"{f.Field}, Score:{f.Score} , Matches: " + string.Join(" ", f.Words.Select(h => h.IsMatch ? $"[{h.Content}({h.Score})]" : h.Content)));
                }
            }
        }

        return results.OrderByDescending(t => t.Score).ToList();
    }

    private async Task<List<PackageDto>> GetSearchData(CancellationToken cancellationToken = default)
    {
        var cache = searchPackageCache.GetData();
        if (cache != null)
        {
            return cache;
        }

        var areas = await db.Areas.AsNoTracking().ToListAsync(cancellationToken);
        var packages = await db.Packages.AsNoTracking().ToListAsync(cancellationToken);

        var result = new List<PackageDto>();
        foreach (var package in packages)
        {
            result.Add(new PackageDto(package)
            {
                Area = areas.First(t => t.Id == package.AreaId),
                Resources = await db.PackageResources.AsNoTracking().Where(t => t.PackageId == package.Id).Include(t => t.Resource).Select(t => t.Resource).ToListAsync(cancellationToken)
            });
        }

        searchPackageCache.SetData(result, TimeSpan.FromMinutes(5));

        return result;
    }

    /// <inheritdoc/>
    public async Task<PackageDto> GetPackageByUrnValue(string urnValue, CancellationToken cancellationToken = default)
    {
        if (!urnValue.StartsWith("urn:") && !urnValue.StartsWith(':'))
        {
            urnValue = ":" + urnValue;
        }

        var packages = await db.Packages.AsNoTracking().Where(t => t.Urn.EndsWith(urnValue)).Include(t => t.Area).ToListAsync(cancellationToken);
        if (packages == null || packages.Count() != 1)
        {
            return null;
        }

        var package = packages.First();

        var resources = await db.PackageResources.AsNoTracking().Where(t => t.PackageId == package.Id).Include(t => t.Resource).Select(t => t.Resource).ToListAsync(cancellationToken);

        return new PackageDto(package)
        {
            Area = package.Area,
            Resources = resources
        };
    }

    /// <inheritdoc/>
    public async Task<PackageDto> GetPackage(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await db.Packages.AsNoTracking().Include(t => t.Area).SingleAsync(t => t.Id == id, cancellationToken);

        if (package == null)
        {
            return null;
        }

        var resources = await db.PackageResources.AsNoTracking().Where(t => t.PackageId == package.Id).Include(t => t.Resource).Select(t => t.Resource).ToListAsync(cancellationToken);

        return new PackageDto(package)
        {
            Area = package.Area,
            Resources = resources
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PackageDto>> GetPackagesByArea(Guid areaId, CancellationToken cancellationToken)
    {
        var packages = await db.Packages.AsNoTracking().Where(t => t.AreaId == areaId).Include(t => t.Area).ToListAsync(cancellationToken);

        var result = new List<PackageDto>();
        foreach (var package in packages)
        {
            result.Add(new PackageDto(package)
            {
                Area = package.Area,
                Resources = await db.PackageResources.AsNoTracking().Where(t => t.PackageId == package.Id).Include(t => t.Resource).Select(t => t.Resource).ToListAsync(cancellationToken)
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AreaGroupDto>> GetHierarchy(CancellationToken cancellationToken = default)
    {
        var groups = await db.AreaGroups.AsNoTracking().ToListAsync(cancellationToken);
        var areas = await db.Areas.AsNoTracking().ToListAsync(cancellationToken);
        var packages = await db.Packages.AsNoTracking().ToListAsync(cancellationToken);

        var result = groups.Select(t => ConvertToDto(t)).ToList();
        foreach (var grp in result)
        {
            grp.Areas = areas.Where(t => t.GroupId == grp.Id).Select(t => ConvertToDto(t)).ToList();
            foreach (var area in grp.Areas)
            {
                area.Packages = packages.Where(t => t.AreaId == area.Id).Select(t => new PackageDto(t)).ToList();
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AreaGroup>> GetAreaGroups(CancellationToken cancellationToken = default)
    {
        return await db.AreaGroups.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AreaGroup> GetAreaGroup(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.AreaGroups.AsNoTracking().SingleAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Area>> GetAreas(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await db.Areas.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Area> GetArea(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Areas.AsNoTracking().SingleAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Resource>> GetPackageResources(Guid packageId, CancellationToken cancellationToken = default)
    {
        return await db.PackageResources.AsNoTracking().Where(t => t.PackageId == packageId).Select(t => t.Resource).ToListAsync();
    }

    #region Converters
    private AreaGroupDto ConvertToDto(AreaGroup areaGroup)
    {
        return new AreaGroupDto()
        {
            Id = areaGroup.Id,
            Name = areaGroup.Name,
            Description = areaGroup.Description,
            Type = areaGroup.EntityType.Name,
            Urn = areaGroup.Urn,
            Areas = new List<AreaDto>()
        };
    }

    private AreaDto ConvertToDto(Area area)
    {
        return new AreaDto()
        {
            Id = area.Id,
            Name = area.Name,
            Urn = area.Urn,
            Description = area.Description,
            Icon = area.IconUrl,
            Packages = new List<PackageDto>()
        };
    }
    #endregion
}
