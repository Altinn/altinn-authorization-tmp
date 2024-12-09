using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.AccessPackages.CLI;

/// <summary>
/// Generate mockup data
/// </summary>
public class Mockups
{
    #region Constructor
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
    private readonly IAssignmentService assignmentService;
    private readonly IGroupService groupService;
    private readonly IGroupAdminService groupAdminService;
    private readonly IGroupMemberService groupMemberService;
    private readonly IDelegationService delegationService;
    private readonly IDelegationPackageService delegationPackageService;
    private readonly IDelegationGroupService delegationGroupService;
    private readonly IDelegationAssignmentService delegationAssignmentService;

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
    /// <param name="assignmentService">IAssignmentService</param>
    /// <param name="groupService">IGroupService</param>
    /// <param name="groupAdminService">IGroupAdminService</param>
    /// <param name="groupMemberService">IGroupMemberService</param>
    /// <param name="delegationService">IDelegationService</param>
    /// <param name="delegationPackageService">IDelegationPackageService</param>
    /// <param name="delegationGroupService">IDelegationGroupService</param>
    /// <param name="delegationAssignmentService">IDelegationAssignmentService</param>
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
        IAssignmentService assignmentService,
        IGroupService groupService,
        IGroupAdminService groupAdminService,
        IGroupMemberService groupMemberService,
        IDelegationService delegationService,
        IDelegationPackageService delegationPackageService,
        IDelegationGroupService delegationGroupService,
        IDelegationAssignmentService delegationAssignmentService
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
        this.assignmentService = assignmentService;
        this.groupService = groupService;
        this.groupAdminService = groupAdminService;
        this.groupMemberService = groupMemberService;
        this.delegationService = delegationService;
        this.delegationPackageService = delegationPackageService;
        this.delegationGroupService = delegationGroupService;
        this.delegationAssignmentService = delegationAssignmentService;
    }
    #endregion

    #region Cache

    private List<EntityType> EntityTypes { get; set; }
    
    private List<EntityVariant> EntityVariants { get; set; }
    
    private List<Entity> Entities { get; set; }
    
    private List<Provider> Providers { get; set; }
    
    private List<Resource> Resources { get; set; }
    
    private List<ResourceType> ResourceTypes { get; set; }
    
    private List<ResourceGroup> ResourceGroups { get; set; }
    
    private List<Package> Packages { get; set; }
    
    private List<Area> Areas { get; set; }
    
    private List<PackageResource> PackageResources { get; set; }
    
    private List<Role> Roles { get; set; }

    private List<Assignment> Assignments { get; set; }

    private List<EntityGroup> Groups { get; set; }

    private List<GroupAdmin> GroupAdmins { get; set; }

    private List<GroupMember> GroupMembers { get; set; }

    private List<Delegation> Delegations { get; set; }

    private List<DelegationPackage> DelegationPackages { get; set; }

    private List<DelegationAssignment> DelegationAssignments { get; set; }

    private List<DelegationGroup> DelegationGroups { get; set; }

    /// <summary>
    /// Load cache
    /// </summary>
    /// <returns></returns>
    public async Task LoadCache()
    {
        logger.LogInformation("Loading cache");
        EntityTypes = [.. await entityTypeService.Get()];
        EntityVariants = [.. await entityVariantService.Get()];
        Entities = [.. await entityService.Get()];
        Providers = [.. await providerService.Get()];
        ResourceTypes = [.. await resourceTypeService.Get()];
        ResourceGroups = [.. await resourceGroupService.Get()];
        Packages = [.. await packageService.Get()];
        Areas = [.. await areaService.Get()];
        PackageResources = [.. await packageResourceService.Get()];
        Resources = [.. await resourceService.Get()];
        Roles = [.. await roleService.Get()];
        Assignments = [.. await assignmentService.Get()];
        Groups = [.. await groupService.Get()];
        GroupAdmins = [.. await groupAdminService.Get()];
        GroupMembers = [.. await groupMemberService.Get()];

        Delegations = [.. await delegationService.Get()];
        DelegationPackages = [.. await delegationPackageService.Get()];
        DelegationGroups = [.. await delegationGroupService.Get()];
        DelegationAssignments = [.. await delegationAssignmentService.Get()];

        logger.LogInformation("Loading cache - Complete");
    }

    private EntityType GetEntityType(string name)
    {
        return EntityTypes.First(t => t.Name == name);
    }

    private EntityVariant GetEntityVariant(string typeName, string variantName)
    {
        var type = GetEntityType(typeName);
        return EntityVariants.First(t => t.TypeId == type.Id && t.Name == variantName);
    }

    private (EntityType Type, EntityVariant Variant) GetTypeAndVariant(string typeName, string variantName)
    {
        var type = GetEntityType(typeName);
        var vairant = EntityVariants.First(t => t.TypeId == type.Id && t.Name == variantName);
        return (type, vairant);
    }

    private Provider GetProvider(string name)
    {
        return Providers.First(t => t.Name == name);
    }

    private Area GetArea(string name)
    {
        return Areas.First(t => t.Name == name);
    }
    
    private Package GetPackage(string name, Guid areaId)
    {
        return Packages.First(t => t.Name == name && t.AreaId == areaId);
    }

    private async Task<Package> GetOrCreatePackage(Package obj)
    {
        var res = Packages.FirstOrDefault(t => t.ProviderId == obj.ProviderId && t.Name == obj.Name) ?? null;
        if (res == null)
        {
            await packageService.Create(obj);
            Packages.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private Role GetRole(string code)
    {
        return Roles.First(t => t.Code == code);
    }

    private async Task<Delegation> GetOrCreateDelegation(Delegation obj)
    {
        var res = Delegations.FirstOrDefault(t => t.AssignmentId == obj.AssignmentId) ?? null;
        if (res == null)
        {
            await delegationService.Create(obj);
            Delegations.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<DelegationPackage> GetOrCreateDelegationPackage(DelegationPackage obj)
    {
        var res = DelegationPackages.FirstOrDefault(t => t.DelegationId == obj.DelegationId && t.PackageId == obj.PackageId) ?? null;
        if (res == null)
        {
            await delegationPackageService.Create(obj);
            DelegationPackages.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<DelegationGroup> GetOrCreateDelegationGroup(DelegationGroup obj)
    {
        var res = DelegationGroups.FirstOrDefault(t => t.DelegationId == obj.DelegationId && t.GroupId == obj.GroupId) ?? null;
        if (res == null)
        {
            await delegationGroupService.Create(obj);
            DelegationGroups.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<DelegationAssignment> GetOrCreateDelegationAssignment(DelegationAssignment obj)
    {
        var res = DelegationAssignments.FirstOrDefault(t => t.DelegationId == obj.DelegationId && t.AssignmentId == obj.AssignmentId) ?? null;
        if (res == null)
        {
            await delegationAssignmentService.Create(obj);
            DelegationAssignments.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Role> GetOrCreateRole(Role obj)
    {
        var res = Roles.FirstOrDefault(t => t.Code == obj.Code) ?? null;
        if (res == null)
        {
            await roleService.Create(obj);
            Roles.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<EntityGroup> GetOrCreateGroup(EntityGroup obj)
    {
        var res = Groups.FirstOrDefault(t => t.OwnerId == obj.OwnerId && t.Name == obj.Name) ?? null;
        if (res == null)
        {
            await groupService.Create(obj);
            Groups.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<GroupMember> GetOrCreateGroupMember(GroupMember obj)
    {
        var res = GroupMembers.FirstOrDefault(t => t.GroupId == obj.GroupId && t.MemberId == obj.MemberId) ?? null;
        if (res == null)
        {
            await groupMemberService.Create(obj);
            GroupMembers.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<GroupAdmin> GetOrCreateGroupAdmin(GroupAdmin obj)
    {
        var res = GroupAdmins.FirstOrDefault(t => t.GroupId == obj.GroupId && t.MemberId == obj.MemberId) ?? null;
        if (res == null)
        {
            await groupAdminService.Create(obj);
            GroupAdmins.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Assignment> GetOrCreateAssignment(Assignment obj)
    {
        var res = Assignments.FirstOrDefault(t => t.FromId == obj.FromId && t.ToId == obj.ToId && t.RoleId == obj.RoleId) ?? null;
        if (res == null)
        {
            await assignmentService.Create(obj);
            Assignments.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Entity> GetOrCreateEntity(Entity obj)
    {
        var res = Entities.FirstOrDefault(t => t.Name == obj.Name && t.TypeId == obj.TypeId && t.RefId == obj.RefId);
        if (res == null)
        {
            await entityService.Create(obj);
            Entities.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<ResourceType> GetOrCreateResourceType(string name)
    {
        var res = ResourceTypes.FirstOrDefault(t => t.Name == name) ?? null;
        if (res == null)
        {
            var obj = new ResourceType() { Id = Guid.NewGuid(), Name = name };
            await resourceTypeService.Create(obj);
            ResourceTypes.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<ResourceGroup> GetOrCreateResourceGroup(string name, Guid providerId)
    {
        var res = ResourceGroups.FirstOrDefault(t => t.Name == name && t.ProviderId == providerId) ?? null;
        if (res == null)
        {
            var obj = new ResourceGroup() { Id = Guid.NewGuid(), Name = name, ProviderId = providerId };
            await resourceGroupService.Create(obj);
            ResourceGroups.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Resource> GetOrCreateResource(Resource obj)
    {
        var res = Resources.FirstOrDefault(t => t.Name == obj.Name && t.ProviderId == obj.ProviderId && t.RefId == obj.RefId) ?? null;
        if (res == null)
        {
            await resourceService.Create(obj);
            Resources.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<PackageResource> GetOrCreatePackageResource(Guid packageId, Guid resourceId)
    {
        var res = PackageResources.FirstOrDefault(t => t.PackageId == packageId && t.ResourceId == resourceId) ?? null;
        if (res == null)
        {
            var obj = new PackageResource() { Id = Guid.NewGuid(), PackageId = packageId, ResourceId = resourceId, Read = true, Write = true, Sign = true };
            await packageResourceService.Create(obj);
            PackageResources.Add(obj);
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

        var spirh = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "SPIRH AS", RefId = "928236587", TypeId = orgType.Type.Id, VariantId = orgType.Variant.Id });
        var regnskapNorge = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Regnskap Norge AS", RefId = "985196087", TypeId = orgType.Type.Id, VariantId = orgType.Variant.Id });
        var revisorfolka = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Revisorfolka AS", RefId = "889955771", TypeId = orgType.Type.Id, VariantId = orgType.Variant.Id });

        var mariusThuen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Marius Thuen", RefId = "1984-10-23ThuenMarius", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        
        var gunnarJohnsen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Gunnar Johnsen", RefId = "P2", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var perHansen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Per Hansen", RefId = "P3", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var gunillaJonson = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Gunilla Jonson", RefId = "P4", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var ninaHessel = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Nina Hessel", RefId = "P5", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var viggoKristiansen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Viggo Kristiansen", RefId = "P6", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });

        var nilshansen = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Nils Hansen", RefId = "P7", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var franzFerdinan = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Franz Ferdinan", RefId = "P8", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var janOveKaizer = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Jan Ove Kaizer", RefId = "P9", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        var helgeMowinkel = await GetOrCreateEntity(new Entity { Id = Guid.NewGuid(), Name = "Helge Mowinkel", RefId = "P10", TypeId = persType.Type.Id, VariantId = persType.Variant.Id });
        #endregion

        #region Resources
        var digdirProvider = GetProvider("Digdir");
        var skattProvider = GetProvider("Skatteetaten");

        var skattArea = GetArea("Skatt, avgift, regnskap og toll");
        var skattegrunnlag = GetPackage("Skattegrunnlag", skattArea.Id);

        var regnPakke1 = await GetOrCreatePackage(new Package() { Id = Guid.NewGuid(), Name = "Regnskapsfører med signeringsrettighet", AreaId = skattArea.Id, Description = "Regnskapsfører med signeringsrettighet", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id, IsDelegable = true });
        var regnPakke2 = await GetOrCreatePackage(new Package() { Id = Guid.NewGuid(), Name = "Regnskapsfører uten signeringsrettighet", AreaId = skattArea.Id, Description = "Regnskapsfører uten signeringsrettighet", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id, IsDelegable = true });
        var regnPakke3 = await GetOrCreatePackage(new Package() { Id = Guid.NewGuid(), Name = "Regnskapsfører lønn", AreaId = skattArea.Id, Description = "Regnskapsfører lønn", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id, IsDelegable = true });

        var reviPakke1 = await GetOrCreatePackage(new Package() { Id = Guid.NewGuid(), Name = "Ansvarlig revisor", AreaId = skattArea.Id, Description = "Ansvarlig revisor", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id, IsDelegable = true });
        var reviPakke2 = await GetOrCreatePackage(new Package() { Id = Guid.NewGuid(), Name = "Revisormedarbeider", AreaId = skattArea.Id, Description = "Revisormedarbeider", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id, IsDelegable = true });

        /*
         Connect Package to Role
         */

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
        var revi = GetRole("REVI");
        var ha = GetRole("HA");
        var ts = GetRole("TS");
        var ansatt = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "Ansatt", Code = "ANSATT", Description = "Ansatt", Urn = "digdir:role:ansatt", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });
        var sys = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "System", Code = "SYS", Description = "System", Urn = "digdir:role:sys", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });

        var ass01 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = mariusThuen.Id, RoleId = dagl.Id });
        var ass01_2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = mariusThuen.Id, RoleId = ha.Id });
        var ass01_3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = mariusThuen.Id, RoleId = ts.Id });
        var ass02 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = mariusThuen.Id, RoleId = lede.Id });

        var ass03 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = regnskapNorge.Id, RoleId = regn.Id, IsDelegable = true });
        var ass03_2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = spirh.Id, ToId = revisorfolka.Id, RoleId = revi.Id, IsDelegable = true });

        var ass04 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = gunnarJohnsen.Id, RoleId = ansatt.Id });
        var ass04_2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = gunnarJohnsen.Id, RoleId = dagl.Id });
        var ass04_3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = gunnarJohnsen.Id, RoleId = ha.Id });
        var ass04_4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = gunnarJohnsen.Id, RoleId = ts.Id });
        var ass05 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = perHansen.Id, RoleId = ansatt.Id });
        var ass05_2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = perHansen.Id, RoleId = lede.Id });
        var ass05_3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = perHansen.Id, RoleId = ts.Id });
        var ass06 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = gunillaJonson.Id, RoleId = ansatt.Id });
        var ass07 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regnskapNorge.Id, ToId = viggoKristiansen.Id, RoleId = ansatt.Id });

        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = nilshansen.Id, RoleId = ansatt.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = nilshansen.Id, RoleId = dagl.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = nilshansen.Id, RoleId = ha.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = nilshansen.Id, RoleId = ts.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = franzFerdinan.Id, RoleId = ansatt.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = franzFerdinan.Id, RoleId = lede.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = franzFerdinan.Id, RoleId = ts.Id });
        var ass09 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = janOveKaizer.Id, RoleId = ansatt.Id });
        await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revisorfolka.Id, ToId = helgeMowinkel.Id, RoleId = ansatt.Id });

        var regnDelegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), AssignmentId = ass03.Id });

        await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, PackageId = regnPakke1.Id });
        await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, PackageId = regnPakke3.Id });

        var reviDelegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), AssignmentId = ass03_2.Id });
        await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = reviDelegation.Id, PackageId = regnPakke2.Id });

        var grp01 = await GetOrCreateGroup(new EntityGroup() { Id = Guid.NewGuid(), Name = "A-Klienter", OwnerId = regnskapNorge.Id, RequireRole = true });

        var grpAdm01 = await GetOrCreateGroupAdmin(new GroupAdmin() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = gunnarJohnsen.Id });
        var grpMem01 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = perHansen.Id });
        var grpMem02 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = gunillaJonson.Id });
        var grpMem03 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = viggoKristiansen.Id });

        var regnDelGroup = await GetOrCreateDelegationGroup(new DelegationGroup() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, GroupId = grp01.Id });

        var reviDelegAss = await GetOrCreateDelegationAssignment(new DelegationAssignment() { Id = Guid.NewGuid(), DelegationId = reviDelegation.Id, AssignmentId = ass09.Id });

        #endregion

        logger.LogInformation("Mock - KlientDelegering - Complete");
    }
}
