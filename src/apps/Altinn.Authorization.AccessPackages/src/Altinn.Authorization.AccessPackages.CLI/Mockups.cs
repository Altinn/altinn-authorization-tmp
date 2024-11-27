using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.AccessPackages.CLI;

/// <summary>
/// Generate mockup data
/// </summary>
public class Mockups
{
    private readonly ILogger<Mockups> logger;
    private readonly IEntityTypeService entityTypeService;
    private readonly IEntityVariantService entityVariantService;
    private readonly IEntityService entityService;
    private readonly IProviderService providerService;
    private readonly IResourceTypeService resourceTypeService;
    private readonly IResourceGroupService resourceGroupService;
    private readonly IPackageService packageService;
    private readonly IAreaService areaService;
    private readonly IPackageResourceService packageResourceService;
    private readonly IResourceService resourceService;
    private readonly IRoleService roleService;
    private readonly IRelationService relationService;
    private readonly IRelationAssignmentService relationAssignmentService;

    /// <summary>
    /// Mockups
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <param name="entityTypeService">IEntityTypeService</param>
    /// <param name="entityVariantService">IEntityVariantService</param>
    /// <param name="entityService">IEntityService</param>
    /// <param name="providerService">IProviderService</param>
    /// <param name="resourceTypeService">IResourceTypeService</param>
    /// <param name="resourceGroupService">IResourceGroupService</param>
    /// <param name="packageService">IPackageService</param>
    /// <param name="areaService">IAreaService</param>
    /// <param name="packageResourceService">IPackageResourceService</param>
    /// <param name="resourceService">IResourceService</param>
    /// <param name="roleService">IRoleService</param>
    /// <param name="relationService">IRelationService</param>
    /// <param name="relationAssignmentService">IRelationAssignmentService</param>
    public Mockups(
        ILogger<Mockups> logger,
        IEntityTypeService entityTypeService,
        IEntityVariantService entityVariantService,
        IEntityService entityService,
        IProviderService providerService,
        IResourceTypeService resourceTypeService,
        IResourceGroupService resourceGroupService,
        IPackageService packageService,
        IAreaService areaService,
        IPackageResourceService packageResourceService,
        IResourceService resourceService,
        IRoleService roleService,
        IRelationService relationService,
        IRelationAssignmentService relationAssignmentService
        )
    {
        this.logger = logger;
        this.entityTypeService = entityTypeService;
        this.entityVariantService = entityVariantService;
        this.entityService = entityService;
        this.providerService = providerService;
        this.resourceTypeService = resourceTypeService;
        this.resourceGroupService = resourceGroupService;
        this.packageService = packageService;
        this.areaService = areaService;
        this.packageResourceService = packageResourceService;
        this.resourceService = resourceService;
        this.roleService = roleService;
        this.relationService = relationService;
        this.relationAssignmentService = relationAssignmentService;
    }

    #region Cache

    private List<EntityType> entityTypes { get; set; }
    private List<EntityVariant> entityVariants { get; set; }
    private List<Entity> entities { get; set; }
    private List<Provider> providers { get; set; }
    private List<Resource> resources { get; set; }
    private List<ResourceType> resourceTypes { get; set; }
    private List<ResourceGroup> resourceGroups { get; set; }
    private List<Package> packages { get; set; }
    private List<Area> areas { get; set; }
    private List<PackageResource> packageResources { get; set; }
    public List<Role> roles { get; set; }
    public List<Relation> relations { get; set; }
    public List<RelationAssignment> relationAssignments { get; set; }

    public async Task LoadCache()
    {
        logger.LogInformation("Loading cache");
        entityTypes = [.. await entityTypeService.Get()];
        entityVariants = [.. await entityVariantService.Get()];
        entities = [.. await entityService.Get()];
        providers = [.. await providerService.Get()];
        resourceTypes = [.. await resourceTypeService.Get()];
        resourceGroups = [.. await resourceGroupService.Get()];
        packages = [.. await packageService.Get()];
        areas = [.. await areaService.Get()];
        packageResources = [.. await packageResourceService.Get()];
        resources = [.. await resourceService.Get()];
        roles = [.. await roleService.Get()];
        relations = [.. await relationService.Get()];
        relationAssignments = [.. await relationAssignmentService.Get()];
        logger.LogInformation("Loading cache - Complete");
    }

    private EntityType GetEntityType(string name)
    {
        return entityTypes.First(t => t.Name == name);
    }

    private EntityVariant GetEntityVariant(string typeName, string variantName)
    {
        var type = GetEntityType(typeName);
        return entityVariants.First(t => t.TypeId == type.Id && t.Name == variantName);
    }

    private (EntityType Type, EntityVariant Variant) GetTypeAndVariant(string typeName, string variantName)
    {
        var type = GetEntityType(typeName);
        var vairant = entityVariants.First(t => t.TypeId == type.Id && t.Name == variantName);
        return (type, vairant);
    }

    private Provider GetProvider(string name)
    {
        return providers.First(t => t.Name == name);
    }

    private Area GetArea(string name)
    {
        return areas.First(t => t.Name == name);
    }
    
    private Package GetPackage(string name, Guid areaId)
    {
        return packages.First(t => t.Name == name && t.AreaId == areaId);
    }

    private Role GetRole(string code)
    {
        return roles.First(t => t.Code == code);
    }

    private async Task<Role> GetOrCreateRole(Role obj)
    {
        var res = roles.FirstOrDefault(t => t.Code == obj.Code) ?? null;
        if (res == null)
        {
            await roleService.Create(obj);
            roles.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Entity> GetOrCreateEntity(Entity obj)
    {
        var res = entities.FirstOrDefault(t => t.Name == obj.Name && t.TypeId == obj.TypeId && t.RefId == obj.RefId);
        if (res == null)
        {
            await entityService.Create(obj);
            entities.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<ResourceType> GetOrCreateResourceType(string name)
    {
        var res = resourceTypes.FirstOrDefault(t => t.Name == name) ?? null;
        if (res == null)
        {
            var obj = new ResourceType() { Id = Guid.NewGuid(), Name = name };
            await resourceTypeService.Create(obj);
            resourceTypes.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<ResourceGroup> GetOrCreateResourceGroup(string name, Guid providerId)
    {
        var res = resourceGroups.FirstOrDefault(t => t.Name == name && t.ProviderId == providerId) ?? null;
        if (res == null)
        {
            var obj = new ResourceGroup() { Id = Guid.NewGuid(), Name = name, ProviderId = providerId};
            await resourceGroupService.Create(obj);
            resourceGroups.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Resource> GetOrCreateResource(Resource obj)
    {
        var res = resources.FirstOrDefault(t => t.Name == obj.Name && t.ProviderId == obj.ProviderId && t.RefId == obj.RefId) ?? null;
        if (res == null)
        {
            await resourceService.Create(obj);
            resources.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<PackageResource> GetOrCreatePackageResource(Guid packageId, Guid resourceId)
    {
        var res = packageResources.FirstOrDefault(t => t.PackageId == packageId && t.ResourceId == resourceId) ?? null;
        if (res == null)
        {
            var obj = new PackageResource() { Id = Guid.NewGuid(), PackageId = packageId, ResourceId = resourceId, Read = true, Write = true, Sign = true };
            await packageResourceService.Create(obj);
            packageResources.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Relation> GetOrCreateRelation(Relation obj)
    {
        var res = relations.FirstOrDefault(t => t.FromId == obj.FromId && t.RoleId == obj.RoleId);
        if (res == null)
        {
            await relationService.Create(obj);
            relations.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<RelationAssignment> GetOrCreateRelationAssignment(RelationAssignment obj)
    {
        var res = relationAssignments.FirstOrDefault(t => t.ToId == obj.ToId && t.RelationId == obj.RelationId);
        if (res == null)
        {
            await relationAssignmentService.Create(obj);
            relationAssignments.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    #endregion

    /// <summary>
    /// Add data for Klientdelegering mock
    /// </summary>
    /// <returns></returns>
    public async Task KlientDelegeringMock()
    {
        logger.LogInformation("Mock - KlientDelegering");
        await LoadCache();

        #region Entities
        var orgType = GetTypeAndVariant("Organisasjon", "AS");
        var persType = GetTypeAndVariant("Person", "Person");

        var spirh = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Spirh AS", RefId = "O1", TypeId = orgType.Type.Id, VariantId = orgType.Variant.Id });
        var regnskapNorge = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Regnskap Norge AS", RefId = "O2", TypeId = orgType.Type.Id, VariantId = orgType.Variant.Id });

        var mariusThuen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Marius Thuen", RefId = "P1", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var gunnarJohnsen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Gunnar Johnsen", RefId = "P2", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var perHansen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Per Hansen", RefId = "P3", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var gunillaJonson = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Gunilla Jonson", RefId = "P4", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var ninaHessel = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Nina Hessel", RefId = "P5", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var viggoKristiansen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Viggo Kristiansen", RefId = "P6", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        #endregion

        #region Resources
        var digdirProvider = GetProvider("Digdir");
        var skattProvider = GetProvider("Skatteetaten");

        var skattArea = GetArea("Skatt, avgift, regnskap og toll");
        var skattegrunnlag = GetPackage("Skattegrunnlag", skattArea.Id);
        var resourceGroup = await GetOrCreateResourceGroup("Standard", skattProvider.Id);
        var resourceType = await GetOrCreateResourceType("Generisk");

        var resSkatt01 = await GetOrCreateResource(new Resource { Id = Guid.NewGuid(), Name = "Skattemelding - Privat", Description = "Noe veldig skattete", RefId = "Skatt-01", TypeId = resourceType.Id, GroupId = resourceGroup.Id, ProviderId = skattProvider.Id });
        var resSkatt02 = await GetOrCreateResource(new Resource { Id = Guid.NewGuid(), Name = "Skattemelding - Næring", Description = "Noe veldig skattete", RefId = "Skatt-02", TypeId = resourceType.Id, GroupId = resourceGroup.Id, ProviderId = skattProvider.Id });
        var resSkatt03 = await GetOrCreateResource(new Resource { Id = Guid.NewGuid(), Name = "Skattemelding - Offentlig", Description = "Noe veldig skattete", RefId = "Skatt-03", TypeId = resourceType.Id, GroupId = resourceGroup.Id, ProviderId = skattProvider.Id });

        await GetOrCreatePackageResource(skattegrunnlag.Id, resSkatt01.Id);
        await GetOrCreatePackageResource(skattegrunnlag.Id, resSkatt02.Id);
        await GetOrCreatePackageResource(skattegrunnlag.Id, resSkatt03.Id);

        #endregion

        #region Roles

        /*Roles*/

        var dagl = GetRole("DAGL");
        var lede = GetRole("LEDE");
        var regn = GetRole("REGN");
        var ansatt = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "Ansatt", Code = "ANSATT", Description = "Ansatt", Urn = "digdir:role:ansatt", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });
        var sys = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "System", Code = "SYS", Description = "System", Urn = "digdir:role:sys", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });

        var ra1 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = spirh.Id, ToId = mariusThuen.Id, RoleId = dagl.Id };
        var ra2 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = spirh.Id, ToId = mariusThuen.Id, RoleId = lede.Id };
        var ra3 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = spirh.Id, ToId = regnskapNorge.Id, RoleId = regn.Id };

        var ra4 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = regnskapNorge.Id, ToId = gunnarJohnsen.Id, RoleId = ansatt.Id };
        var ra5 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = regnskapNorge.Id, ToId = perHansen.Id, RoleId = ansatt.Id };
        var ra6 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = regnskapNorge.Id, ToId = gunillaJonson.Id, RoleId = ansatt.Id };
        var ra7 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = regnskapNorge.Id, ToId = viggoKristiansen.Id, RoleId = ansatt.Id };

        //var spirhDagl = await GetOrCreateRelation(new Relation() { Id = Guid.NewGuid(), FromId = spirh.Id, RoleId = dagl.Id, IsDelegable = false });
        //var spirhLede = await GetOrCreateRelation(new Relation() { Id = Guid.NewGuid(), FromId = spirh.Id, RoleId = lede.Id, IsDelegable = false });
        //var spirhRegn = await GetOrCreateRelation(new Relation() { Id = Guid.NewGuid(), FromId = spirh.Id, RoleId = regn.Id, IsDelegable = true });
        //var regnskapNorgeAnsatt = await GetOrCreateRelation(new Relation() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, RoleId = ansatt.Id, IsDelegable = false });

        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = spirhDagl.Id, ToId = mariusThuen.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = spirhLede.Id, ToId = mariusThuen.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = spirhRegn.Id, ToId = regnskapNorge.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = regnskapNorgeAnsatt.Id, ToId = gunnarJohnsen.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = regnskapNorgeAnsatt.Id, ToId = perHansen.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = regnskapNorgeAnsatt.Id, ToId = gunillaJonson.Id });
        //await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = regnskapNorgeAnsatt.Id, ToId = viggoKristiansen.Id });

        //// KilentDelegering
        // await GetOrCreateRelationAssignment(new RelationAssignment() { Id = Guid.NewGuid(), RelationId = spirhRegn.Id, ToId = perHansen.Id });

        //// KlientDelegering
        //// Nei ... Men joa ;)
        var ra8 = new RoleAssignment() { Id = Guid.NewGuid(), ForId = spirh.Id, ToId = gunnarJohnsen.Id, RoleId = regn.Id };

        #endregion

        logger.LogInformation("Mock - KlientDelegering - Complete");
    }

}
