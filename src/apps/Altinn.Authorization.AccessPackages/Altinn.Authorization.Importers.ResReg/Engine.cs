using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.Importers.ResReg.Models;

namespace Altinn.Authorization.Importers.ResReg;

/// <summary>
/// Import Engine
/// </summary>
public class Engine
{
    private readonly IProviderService providerService;
    private readonly IResourceTypeService resourceTypeService;
    private readonly IResourceService resourceService;
    private readonly IResourceGroupService resourceGroupService;
    private Dictionary<string, Provider> cacheProviders = new Dictionary<string, Provider>();
    private Dictionary<string, ResourceType> cacheResourceTypes = new Dictionary<string, ResourceType>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="providerService">IProviderService</param>
    /// <param name="resourceTypeService">IResourceTypeService</param>
    /// <param name="resourceService">IResourceService</param>
    /// <param name="resourceGroupService">IResourceGroupService</param>
    public Engine(
        IProviderService providerService,
        IResourceTypeService resourceTypeService,
        IResourceService resourceService,
        IResourceGroupService resourceGroupService
        )
    {
        this.providerService = providerService;
        this.resourceTypeService = resourceTypeService;
        this.resourceService = resourceService;
        this.resourceGroupService = resourceGroupService;
    }

    /// <summary>
    /// Import Resources
    /// </summary>
    /// <param name="resources">RawResources</param>
    /// <returns></returns>
    public async Task ImportResource(List<RawResource> resources) 
    {
        // Preload Providers
        foreach (var authority in resources.Select(t => t.HasCompetentAuthority).DistinctBy(t => t.Organization))
        {
            //// Console.WriteLine($"{authority.Name} ({authority.Organization}) : {resources.Count(t => t.HasCompetentAuthority.Organization == authority.Organization)}");
            cacheProviders.Add(authority.Organization ?? "N/A", await GetOrCreateProvider(authority.Name.ToString(), authority.Organization));
        }

        // Preload ResourceTypes
        foreach (var type in resources.Select(t => t.ResourceType).Distinct())
        {
            //// Console.WriteLine($"{type} : {resources.Count(t => t.ResourceType == type)}");
            cacheResourceTypes.Add(type, await GetOrCreateResourceType(type));
        }

        foreach (var rawResource in resources)
        {
            await GetOrCreateResource(rawResource);
        }
    }

    private async Task<Resource> GetOrCreateResource(RawResource rawResource)
    {
        var res = await resourceService.Get("RefId", rawResource.Identifier);

        var rg = await GetOrCreateResourceGroup(rawResource.ResourceType, (await GetOrCreateProvider(rawResource.HasCompetentAuthority.Name.ToString(), rawResource.HasCompetentAuthority.Organization)).Id);

        if (res == null || res.Count() == 0)
        {
            var newObj = new Resource();
            newObj.Id = Guid.NewGuid();
            newObj.RefId = rawResource.Identifier;
            newObj.ProviderId = (await GetOrCreateProvider(rawResource.HasCompetentAuthority.Name.ToString(), rawResource.HasCompetentAuthority.Organization)).Id;
            newObj.GroupId = rg.Id;
            newObj.Name = rawResource.Title.Nb;
            //// newObj.Description = rawResource.Description.ToString();
            newObj.TypeId = (await GetOrCreateResourceType(rawResource.ResourceType)).Id;
            await resourceService.Create(newObj);
            return newObj;
        }
        else
        {
            /* Check for change... */
            return res.First();
        }
    }
    
    private async Task<ResourceType> GetOrCreateResourceType(string name)
    {        
        if (string.IsNullOrEmpty(name))
        {
            name = "Unknown";
        }

        var res = await resourceTypeService.Get("Name", name);
        if (res == null || res.Count() == 0)
        {
            var newObj = new ResourceType() { Id = Guid.NewGuid(), Name = name };
            await resourceTypeService.Create(newObj);
            return newObj;
        }
        else
        {
            /* Check for change... */
            return res.First();
        }
    }
   
    private async Task<ResourceGroup> GetOrCreateResourceGroup(string name, Guid providerId)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "Unknown";
        }

        var res = await resourceGroupService.Get(new Dictionary<string, object>() { { "name", name }, { "providerid", providerId } });
        if (res == null || res.Count() == 0)
        {
            var newObj = new ResourceGroup() { Id = Guid.NewGuid(), Name = name, ProviderId = providerId };
            await resourceGroupService.Create(newObj);
            return newObj;
        }
        else
        {
            /* Check for change... */
            return res.First();
        }
    }

    private async Task<Provider> GetOrCreateProvider(string name, string refId)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "Unknown";
        }

        if (string.IsNullOrEmpty(refId))
        {
            refId = "N/A";
        }

        var res = await providerService.Get("Name", name); //// RefId?


        if (res == null || res.Count() == 0)
        {
            var newObj = new Provider() { Id = Guid.NewGuid(), Name = name, RefId = refId };
            await providerService.Create(newObj);
            return newObj;
        }
        else
        {
            /* Check for change... */
            return res.First();
        }
    }
}
