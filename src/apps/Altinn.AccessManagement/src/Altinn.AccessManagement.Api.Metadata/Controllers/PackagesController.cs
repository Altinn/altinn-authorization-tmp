using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Persistence.Core.Utilities.Search;
using Altinn.AccessMgmt.Persistence.Models;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.Api.Metadata.Controllers;

/// <summary>
/// Controller for å håndtere metadata og informasjon om access packages.
/// </summary>
[Route("/accessmanagement/api/v1/meta/info/accesspackages/")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly IPackageService corePackageService;
    private readonly IPackageService persistencePackageService;
    private readonly IFeatureManager featureManager;

    /// <summary>
    /// Initialiserer en ny instans av <see cref="PackagesController"/>.
    /// </summary>
    /// <param name="packageService">Service for håndtering av access packages.</param>
    public PackagesController(
        IPackageService corePackageService,
        IPackageService persistencePackageService,
        IFeatureManager featureManager)
    {
        this.corePackageService = corePackageService;
        this.persistencePackageService = persistencePackageService;
        this.featureManager = featureManager;
    }

    /// <summary>
    /// Søker etter access packages basert på et søkeord.
    /// </summary>
    /// <param name="term">Søketerm.</param>
    /// <param name="searchInResources">Søk i ressurs verdier</param>
    /// <returns>Liste over søkeresultater.</returns>
    [Route("search")]
    [HttpGet]
    public async Task<ActionResult<SearchObject<PackageDto>>> Search([FromQuery] string term, [FromQuery] bool searchInResources = false)
    {
        IEnumerable<SearchObject<PackageDto>> res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.Search(term, searchInResources);
        }
        else
        {
            res = await persistencePackageService.Search(term, searchInResources);
        }
        if (res == null || !res.Any())
        {
            return NoContent();
        }
        return Ok(res);
    }

    /// <summary>
    /// Eksporterer hierarkiet av area groups.
    /// </summary>
    /// <returns>Liste over area groups.</returns>
    [Route("export")]
    [HttpGet]
    public async Task<ActionResult<AreaGroupDto>> GetHierarchy()
    {
        IEnumerable<AreaGroupDto> res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetHierarchy();
        }
        else
        {
            res = await persistencePackageService.GetHierarchy();
        }
        if (res == null || !res.Any())
        {
            return NoContent();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter alle area groups.
    /// </summary>
    /// <returns>Liste over area groups.</returns>
    [Route("group/")]
    [HttpGet]
    public async Task<ActionResult<AreaGroup>> GetGroups()
    {
        IEnumerable<ExtAreaGroup> res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetAreaGroups();
        }
        else
        {
            res = await persistencePackageService.GetAreaGroups();
        }
        if (res == null || !res.Any())
        {
            return NoContent();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter en spesifikk area group basert på ID.
    /// </summary>
    /// <param name="id">Unik identifikator for area group.</param>
    /// <returns>Den spesifikke area group.</returns>
    [Route("group/{id}")]
    [HttpGet]
    public async Task<ActionResult<AreaGroup>> GetGroup(Guid id)
    {
        ExtAreaGroup res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetAreaGroup(id);
        }
        else
        {
            res = await persistencePackageService.GetAreaGroup(id);
        }
        if (res == null)
        {
            return NotFound();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter alle områder tilhørende en area group.
    /// </summary>
    /// <param name="id">ID til area group.</param>
    /// <returns>Liste over områder.</returns>
    [Route("group/{id}/areas")]
    [HttpGet]
    public async Task<ActionResult<Area>> GetGroupAreas(Guid id)
    {
        IEnumerable<Area> res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetAreas(id);
            if (res == null || !res.Any())
            {
                var grp = await corePackageService.GetAreaGroup(id);
                if (grp == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        else
        {
            res = await persistencePackageService.GetAreas(id);
            if (res == null || !res.Any())
            {
                var grp = await persistencePackageService.GetAreaGroup(id);
                if (grp == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter et spesifikt område basert på ID.
    /// </summary>
    /// <param name="id">ID til området.</param>
    /// <returns>Områdeobjekt.</returns>
    [Route("area/{id}")]
    [HttpGet]
    public async Task<ActionResult<Area>> GetArea(Guid id)
    {
        Area res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetArea(id);
        }
        else
        {
            res = await persistencePackageService.GetArea(id);
        }
        if (res == null)
        {
            return NotFound();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter alle access packages for et spesifikt område.
    /// </summary>
    /// <param name="id">ID til området.</param>
    /// <returns>Liste over access packages.</returns>
    [Route("area/{id}/packages")]
    [HttpGet]
    public async Task<ActionResult<PackageDto>> GetAreaPackages(Guid id)
    {
        IEnumerable<PackageDto> res;
        Area area;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetPackagesByArea(id);
            if (res == null || !res.Any())
            {
                area = await corePackageService.GetArea(id);
                if (area == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        else
        {
            res = await persistencePackageService.GetPackagesByArea(id);
            if (res == null || !res.Any())
            {
                area = await persistencePackageService.GetArea(id);
                if (area == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter en spesifikk access package basert på ID.
    /// </summary>
    /// <param name="id">ID til access package.</param>
    /// <returns>Access package objekt.</returns>
    [Route("package/{id}")]
    [HttpGet]
    public async Task<ActionResult<PackageDto>> GetPackage(Guid id)
    {
        PackageDto res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetPackage(id);
        }
        else
        {
            res = await persistencePackageService.GetPackage(id);
        }
        if (res == null)
        {
            return NotFound();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter en spesifikk access package basert på URN-verdi.
    /// </summary>
    /// <param name="urnValue">URN-verdi for access package.</param>
    /// <returns>Access package objekt.</returns>
    [Route("package/urn/{urnValue}")]
    [HttpGet]
    public async Task<ActionResult<PackageDto>> GetPackageByUrn(string urnValue)
    {
        PackageDto res;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetPackageByUrnValue(urnValue);
        }
        else
        {
            res = await persistencePackageService.GetPackageByUrnValue(urnValue);
        }
        if (res == null)
        {
            return NotFound();
        }
        return Ok(res);
    }

    /// <summary>
    /// Henter alle ressurser tilknyttet en spesifikk access package.
    /// </summary>
    /// <param name="id">ID til access package.</param>
    /// <returns>Liste over ressurser.</returns>
    [Route("package/{id}/resources")]
    [HttpGet]
    public async Task<ActionResult<Resource>> GetPackageResources(Guid id)
    {
        IEnumerable<Resource> res;
        PackageDto pkg;
        if (await featureManager.IsEnabledAsync("AccessMgmt.PackageService.EFCore"))
        {
            res = await corePackageService.GetPackageResources(id);
            if (res == null || !res.Any())
            {
                pkg = await corePackageService.GetPackage(id);
                if (pkg == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        else
        {
            res = await persistencePackageService.GetPackageResources(id);
            if (res == null || !res.Any())
            {
                pkg = await persistencePackageService.GetPackage(id);
                if (pkg == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
        return Ok(res);
    }
}
