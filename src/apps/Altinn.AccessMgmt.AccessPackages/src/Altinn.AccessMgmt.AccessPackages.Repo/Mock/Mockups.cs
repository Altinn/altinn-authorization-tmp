using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Mock;

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
    private readonly IPolicyService policyService;
    private readonly IPolicyComponentService policyComponentService;
    private readonly IElementTypeService elementTypeService;
    private readonly IElementService elementService;
    private readonly IComponentService componentService;
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
    /// <param name="policyService">IPolicyService</param>
    /// <param name="policyComponentService">IPolicyComponentService</param>
    /// <param name="elementTypeService">IElementTypeService</param>
    /// <param name="elementService">IElementService</param>
    /// <param name="componentService">IComponentService</param>
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
        IPolicyService policyService,
        IPolicyComponentService policyComponentService,
        IElementTypeService elementTypeService,
        IElementService elementService,
        IComponentService componentService,
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
        this.policyService = policyService;
        this.policyComponentService = policyComponentService;
        this.elementTypeService = elementTypeService;
        this.elementService = elementService;
        this.componentService = componentService;
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

    private List<Policy> Policies { get; set; }

    private List<PolicyComponent> PolicyComponents { get; set; }

    private List<ElementType> ElementTypes { get; set; }

    private List<Element> Elements { get; set; }

    private List<Component> Components { get; set; }

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
    public async Task LoadCache(bool ignoreEntity = false, bool ignoreAssignment = false)
    {
        logger.LogInformation("Loading cache");
        EntityTypes = [.. await entityTypeService.Get()];
        EntityVariants = [.. await entityVariantService.Get()];

        if (!ignoreEntity)
        {
            Entities = [.. await entityService.Get()];
        }
        
        Providers = [.. await providerService.Get()];
        ResourceTypes = [.. await resourceTypeService.Get()];
        ResourceGroups = [.. await resourceGroupService.Get()];
        Packages = [.. await packageService.Get()];
        Areas = [.. await areaService.Get()];
        PackageResources = [.. await packageResourceService.Get()];
        Resources = [.. await resourceService.Get()];

        Policies = [.. await policyService.Get()];
        PolicyComponents = [.. await policyComponentService.Get()];
        ElementTypes = [.. await elementTypeService.Get()];
        Elements = [.. await elementService.Get()];
        Components = [.. await componentService.Get()];

        Roles = [.. await roleService.Get()];

        if (!ignoreAssignment)
        {
            Assignments = [.. await assignmentService.Get()];
        }
        
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

    private async Task<Policy> GetOrCreatePolicy(Policy obj)
    {
        var res = Policies.FirstOrDefault(t => t.ResourceId == obj.ResourceId && t.Name == obj.Name) ?? null;
        if (res == null)
        {
            await policyService.Create(obj);
            Policies.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<PolicyComponent> GetOrCreatePolicyComponent(PolicyComponent obj)
    {
        var res = PolicyComponents.FirstOrDefault(t => t.PolicyId == obj.PolicyId && t.ComponentId == obj.ComponentId) ?? null;
        if (res == null)
        {
            await policyComponentService.Create(obj);
            PolicyComponents.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<ElementType> GetOrCreateElementType(ElementType obj)
    {
        var res = ElementTypes.FirstOrDefault(t => t.Name == obj.Name) ?? null;
        if (res == null)
        {
            await elementTypeService.Create(obj);
            ElementTypes.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Element> GetOrCreateElement(Element obj)
    {
        var res = Elements.FirstOrDefault(t => t.Name == obj.Name && t.ResourceId == obj.ResourceId && t.Urn == obj.Urn) ?? null;
        if (res == null)
        {
            await elementService.Create(obj);
            Elements.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<Component> GetOrCreateComponent(Component obj)
    {
        var res = Components.FirstOrDefault(t => t.Name == obj.Name && t.ElementId == obj.ElementId && t.Urn == obj.Urn) ?? null;
        if (res == null)
        {
            await componentService.Create(obj);
            Components.Add(obj);
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
        var ansatt = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "Ansatt", Code = "ANSATT", Description = "Ansatt", Urn = "digdir:avtaleRole:ansatt", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });
        var sys = await GetOrCreateRole(new Role() { Id = Guid.NewGuid(), Name = "System", Code = "SYS", Description = "System", Urn = "digdir:avtaleRole:sys", EntityTypeId = orgType.Type.Id, ProviderId = digdirProvider.Id });

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

    /// <summary>
    /// En person vil dele en ressurs med en annen person
    /// </summary>
    /// <returns></returns>
    public async Task PersonToPersonDirectAssignment()
    {
        // Først trenger vi to personer
        var personEntityType = GetEntityType("Person");
        var personEntityVariant = GetEntityVariant("Person", "Person");

        var marius = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("7B3CDDEB-B6F2-4791-8270-0069705453A0"), Name = "Marius", RefId = "P2P-1", TypeId = personEntityType.Id, VariantId = personEntityVariant.Id });
        var fredrik = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("D7B7F49E-78D9-476F-AF10-02BE12CBE0F8"), Name = "Fredrik", RefId = "P2P-2", TypeId = personEntityType.Id, VariantId = personEntityVariant.Id });

        // Vi trenger å ha en rolle som knytter dem, Kontakt
        var provider = GetProvider("Digdir");
        var role = await GetOrCreateRole(new Role() { Id = Guid.Parse("B5E836BB-A2CA-4046-A247-04637BAE06FD"), Code = "KONTAKT", Name = "Kontakt", Description = "Rolle mellom to personer", Urn = "digdir:avtaleRole:person:kontakt", EntityTypeId = personEntityType.Id, ProviderId = provider.Id });

        // Vi trenger å etablere rollen med en tildeling
        var assignment = GetOrCreateAssignment(new Assignment() { Id = Guid.Parse("C726C092-12D2-44A1-BFAC-080F6AAE1D02"), FromId = marius.Id, ToId = fredrik.Id, RoleId = role.Id, IsDelegable = false });

        // Siden rollen Kontakt ikke gir noen pakker eller lignende direkte må Marius gi Fredrik dem direkte
        // var assignmentResource = new AssignmentResource() { };
    }

    /// <summary>
    /// En person vil dele en ressurs med en annen person
    /// </summary>
    /// <returns></returns>
    public async Task PersonToOrganizationDirectAssignment()
    {
        // Først trenger vi to personer
        var personEntityType = GetEntityType("Person");
        var personEntityVariant = GetEntityVariant("Person", "Person");

        var orgEntityType = GetEntityType("Organisasjon");
        var orgEntityVariant = GetEntityVariant("Organisasjon", "AS");

        var marius = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("7B3CDDEB-B6F2-4791-8270-0069705453A0"), Name = "Marius", RefId = "P2O-1", TypeId = personEntityType.Id, VariantId = personEntityVariant.Id });
        var awesomeFolk = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("EF887268-8048-4FC4-A808-08F094EC0E3A"), Name = "Awesome Folk AS", RefId = "P2O-2", TypeId = orgEntityType.Id, VariantId = orgEntityVariant.Id });

        // Vi trenger å ha en rolle som knytter dem, Kontakt
        var provider = GetProvider("Digdir");

        var avtaleRole = await GetOrCreateRole(new Role() { 
            Id = Guid.Parse("D411EF7B-1EB9-4EEB-9E1C-09CF41C57144"), 
            Code = "AVTALE", 
            Name = "Avtale", 
            Description = "Avtale gjort fra en person til et firma", 
            Urn = "digdir:avtaleRole:person:avtale", 
            EntityTypeId = personEntityType.Id, 
            ProviderId = provider.Id 
        });

        // Vi trenger å etablere rollen med en tildeling
        var assignment = await GetOrCreateAssignment(new Assignment() { Id = Guid.Parse("C056D3E6-B814-43E7-AF98-0A080C998425"), FromId = marius.Id, ToId = awesomeFolk.Id, RoleId = avtaleRole.Id, IsDelegable = true });

        // Siden rollen Kontakt ikke gir noen pakker eller lignende direkte må Marius gi Fredrik dem direkte
        // var assignmentResource = new AssignmentResource() { };

        // Awesome AS velger å delegere ressursen de fikk av Marius til en av dems ansatte Gunnar
        // Vi må først opprette Gunnar og gi han ansatt rollen i Awesome AS
        var gunnar = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("A3CF140E-98A9-4AF6-8B3A-0C356A3C71F9"), Name = "Gunnar", RefId = "P2O-3", TypeId= personEntityType.Id, VariantId = personEntityVariant.Id });
        var ansattRole = await GetOrCreateRole(new Role() { Id = Guid.Parse("7F10C119-63EA-42B3-84C6-0CB1147C85C0"), Code = "ANSATT", Name = "Ansatt", Description = "En person er ansatt i et firma", Urn = "digdir:avtaleRole:organisasjon:ansatt", EntityTypeId = orgEntityType.Id, ProviderId = provider.Id });
        var gunnarAnsattAwesome = await GetOrCreateAssignment(new Assignment() { Id = Guid.Parse("6201EEFA-EE00-4712-A172-0E31DADC60AE"), FromId = awesomeFolk.Id, ToId = gunnar.Id, RoleId = ansattRole.Id, IsDelegable = false });

        // Vi kan nå opprette en delegering i Awesome Folk AS til Gunnar
        var delegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.Parse("7E8036B4-43AB-4535-9BA4-0FE745C50F79"), AssignmentId = assignment.Id });
        // var delegationResource = await GetOrCreateDelegationResource(new DelegationResource());
        var delegateGunnar = await GetOrCreateDelegationAssignment(new DelegationAssignment() { Id = Guid.Parse("50F2C1F2-CEEB-4CEA-9FE0-138E3FEE6FFD"), DelegationId = delegation.Id, AssignmentId = gunnarAnsattAwesome.Id });
    }

    /// <summary>
    /// Mock for SystemResources - A3 Rights
    /// </summary>
    /// <returns></returns>
    public async Task SystemResourcesMock()
    {
        logger.LogInformation("Mock - SystemResourcesMock");
        await LoadCache(ignoreEntity: true, ignoreAssignment: true);

        /*
        Group Mgmt
        - Create/Delete Group
        - Add/Remove Admin
        - Add/Remove User

        Assignment Mgmt
        - Create/Delete Any Assignemnt
        - Create/Delete [Role] Assignment
        - Delegate/Revoke Package to Assignment
        - Delegate/Revoke Resource to Assignment
        - Delegate/Revoke Instance to Assignment

        Delegation Mgmt
        - Create/Delete Delegation
        - Add/Remove Package to/from Delegation
        - Add/Remove Resource to/from Delegation
        - Add/Remove Group to/from Delegation
        - Add/Remove Assignment to/from Delegation
         */

        var digdir = GetProvider("Digdir");
        var rt = await GetOrCreateResourceType("Web");
        var a3Rg = await GetOrCreateResourceGroup("Altinn 3", digdir.Id);

        var elementType = await GetOrCreateElementType(new ElementType()
        {
            Id = Guid.Parse("A8023469-CAC3-49EF-9894-65560B09179A"),
            Name = "SubResource"
        });

        // RESOURCE
        var accessmgmt = await GetOrCreateResource(new Resource() 
        { 
            Id = Guid.Parse("06CB9503-D35B-4731-8152-14FA87811B64"), 
            ProviderId = digdir.Id, 
            GroupId = a3Rg.Id,
            TypeId = rt.Id,
            RefId = "digdir:a3:accessmgmt",
            Name = "AccessMgmt",
            Description = "Tilgangsstyring i Altinn 3"
        });

        #region Group Mgmt
        var groupElement = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("D0B49E00-D938-4219-8BB4-15DAB9D5055A"),
            Name = "Group Management",
            Urn = "digdir:a3:accessmgmt:groups",
            ResourceId = accessmgmt.Id,
            TypeId = elementType.Id
        });

        var grpCreate = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("6B689317-0D7A-4176-B6BD-17188CDBDEBF"),
            Name = "Opprett gruppe",
            Description = "Gir muligheten til å opprette grupper",
            Urn = "digdir:a3:accessmgmt:groups:create",
            ElementId = groupElement.Id,
        });

        var grpDelete = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("4C6464C7-BF74-4877-9DBD-1866B8A48F17"),
            Name = "Slett gruppe",
            Description = "Gir muligheten til å slette grupper",
            Urn = "digdir:a3:accessmgmt:groups:delete",
            ElementId = groupElement.Id,
        });

        var grpAddAdmin = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("DD124895-522E-45BF-8E87-195709C60EA1"),
            Name = "Legg til gruppe administrator",
            Description = "Gir muligheten til å legge til administratorer i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:add-user",
            ElementId = groupElement.Id,
        });

        var grpRemoveAdmin = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("3DB6D752-F262-4987-B3A0-1C75AE29E3EA"),
            Name = "Fjern gruppe administrator",
            Description = "Gir muligheten til å fjerne administratorer i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:remove-user",
            ElementId = groupElement.Id,
        });

        var grpAddUser = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("338709E9-102C-40AE-BA20-1F9E245851D9"),
            Name = "Legg til bruker",
            Description = "Gir muligheten til å legge til brukere i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:add-user",
            ElementId = groupElement.Id,
        }); 

        var grpRemoveUser = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("A893D35A-4BE5-48F6-A82A-1FFE2A5C3291"),
            Name = "Fjern bruker",
            Description = "Gir muligheten til å fjerne brukere i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:remove-user",
            ElementId = groupElement.Id,
        });
        #endregion

        #region Assignment Mgmt
        var assignmentElement = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("E45A571C-144F-4B98-BAE5-21873FB6F310"),
            ResourceId = accessmgmt.Id,
            Name = "Assignment Mgmt",
            Urn = "digdir:a3:accessmgmt:assignment",
            TypeId = elementType.Id
        });

        var compAssCreateAny = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("8CA328C5-4F69-43ED-851B-2246945EEEE0"),
            ElementId = assignmentElement.Id,
            Name = "Opprett tildeling",
            Description = "Gir tilgang til å opprette tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:"
        });

        var compAssDeleteAny = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("365C851C-9FCC-4952-8CE8-22AA6757F7C2"),
            ElementId = assignmentElement.Id,
            Name = "Fjern tildeling",
            Description = "Gir tilgang til å fjerne tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:"
        });

        var compDelegatePack = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("20C83E97-FE3D-4531-A17B-258603DD5839"),
            ElementId = assignmentElement.Id,
            Name = "Gi pakke til tildeling",
            Description = "Gir tilgang til å gi pakker på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:delgate-package"
        });

        var compRevokePack = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("91508C64-494B-462E-8774-25CFCDEAA40E"),
            ElementId = assignmentElement.Id,
            Name = "Fjern pakke til tildeling",
            Description = "Gir tilgang til å fjerne pakker på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:revoke-package"
        });

        var compDelegateResource = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("7D4DEF93-706E-4A76-A487-260B272F275C"),
            ElementId = assignmentElement.Id,
            Name = "Gi ressurs til tildeling",
            Description = "Gir tilgang til å gi tjenester på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:delgate-resource"
        });

        var compRevokeresource = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("8E751971-69E9-4766-B42A-2710EDB1480B"),
            ElementId = assignmentElement.Id,
            Name = "Fjern ressurs til tildeling",
            Description = "Gir tilgang til å fjerne tjenester på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:revoke-resource"
        });

        #endregion

        #region Delegation Mgmt
        var delegationElement = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("F4716E33-C740-40CD-8C4D-29786A4E609A"),
            ResourceId = accessmgmt.Id,
            Name = "Delegation Mgmt",
            Urn = "digdir:a3:accessmgmt:delegation",
            TypeId = elementType.Id
        });

        var compDelegCreateAny = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("10624B1F-1021-4ED4-96D4-2C11C2CBA398"),
            ElementId = delegationElement.Id,
            Name = "Opprett delegering",
            Description = "Gir tilgang til å opprette delegeringer",
            Urn = "digdir:a3:accessmgmt:delegation:create"
        });

        var compDelegDeleteAny = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("E8BC5401-38C0-4CA8-BF8A-31BFEA56265E"),
            ElementId = delegationElement.Id,
            Name = "Fjern delegering",
            Description = "Gir tilgang til å fjerne delegeringer",
            Urn = "digdir:a3:accessmgmt:delegation:delete"
        });

        var compDelegPackAdd = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("187C7524-4D45-4194-9135-3208B9E6E5EA"),
            ElementId = delegationElement.Id,
            Name = "Gi pakke til delegering",
            Description = "Gir tilgang til å gi pakker på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:delgate-package"
        });

        var compDelegPackRemove = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("169D2D6F-9AC8-4B70-BD8E-371DF6AB6B56"),
            ElementId = delegationElement.Id,
            Name = "Fjern pakke fra delegering",
            Description = "Gir tilgang til å fjerne pakker på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:revoke-package"
        });

        var compDelegResourceAdd = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("9D0C7368-7348-492F-856B-3811217D1F54"),
            ElementId = delegationElement.Id,
            Name = "Gi ressurs til delegering",
            Description = "Gir tilgang til å gi tjenester på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:delgate-resource"
        });

        var compDelegResourceRemove = await GetOrCreateComponent(new Component()
        {
            Id = Guid.Parse("27F84AD1-7438-4BC4-B016-3939622097D3"),
            ElementId = delegationElement.Id,
            Name = "Fjern ressurs fra delegering",
            Description = "Gir tilgang til å fjerne tjenester på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:revoke-resource"
        });

        #endregion

        // POLICY 
        var adminPolicy = await GetOrCreatePolicy(new Policy()
        {
            Id = Guid.Parse("AB1BD4AC-1E6A-4F53-BDD2-3B8B91C8B9EB"),
            Name = "Admin",
            Description = "Gir tilgang til å administrere tilganger",
            ResourceId = accessmgmt.Id
        });

        #region Policy - Group Mgmt
        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("A506763C-175B-43B8-AC60-3D918BAA4A00"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpCreate.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("C641CD4D-ADAB-4B1F-B40F-48001150B38C"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpDelete.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("4E53BBBB-F62E-4EEE-B371-4BC6AE59A923"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpAddAdmin.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("EE7E229D-C0AB-4FCB-B8A9-4D5AEC04599C"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpRemoveAdmin.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("23698B11-53B4-41CB-9D43-5104953042B6"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpAddUser.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("4AC74DEE-51E6-4ACC-AF42-517DEF59AEAF"),
            PolicyId = adminPolicy.Id,
            ComponentId = grpRemoveUser.Id
        });
        #endregion

        #region Policy - Assignment
        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("A9CC1667-9E74-419B-9AE9-51C7A4E0985F"),
            PolicyId = adminPolicy.Id,
            ComponentId = compAssCreateAny.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("3A006E6A-D1E1-4941-B27C-56FB099E71AB"),
            PolicyId = adminPolicy.Id,
            ComponentId = compAssDeleteAny.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("CECB498B-F448-4896-B701-58E0CFC0919E"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegatePack.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("5999F9F2-B5E2-4061-A4E6-5CACB776C0B7"),
            PolicyId = adminPolicy.Id,
            ComponentId = compRevokePack.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("C406F4D9-9CA6-4167-9809-5E0306625AFB"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegateResource.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("1BA43739-506E-4253-9B6C-5FDBAC077419"),
            PolicyId = adminPolicy.Id,
            ComponentId = compRevokeresource.Id
        });
        #endregion

        #region Policy - Delegation
        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("31ADC81A-723E-4CDD-9E4D-60BEBCF2A81D"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegCreateAny.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("F823F0CD-54D9-4A52-81B0-60D9F5156E69"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegDeleteAny.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("94CBD1E8-4865-468C-9A4A-61387E5419A0"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegPackAdd.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("8A5079A1-838D-4B30-9E20-62727F70D37E"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegPackRemove.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("B2E41362-6A81-45F3-8425-6350DFEF054A"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegResourceAdd.Id
        });

        await GetOrCreatePolicyComponent(new PolicyComponent()
        {
            Id = Guid.Parse("A19AA11F-2389-4913-9C43-64A59B7A506E"),
            PolicyId = adminPolicy.Id,
            ComponentId = compDelegResourceRemove.Id
        });
        #endregion
    }
}


/* GUID

F4C7CFB8-0C6C-454C-BA09-66AA4EA11F8E
6FC79183-7753-4F39-910D-66AEC4C330E1
76BF013D-3BB7-49FD-AC69-681BD282FF30
8DCFB7F1-7A7B-44E9-BB75-6996C26787C8
533E0678-C40E-423D-B3C7-6C31196FE2E3
FBCE49E1-215B-4029-A948-6C68DCC6E410
7F28F8F8-8E25-4BA1-B895-6DCA291DE8A7
F9E7F134-D109-4F48-AD85-6FDB8CE17FCC
DCBB8926-40B9-42D2-A6E6-722FBC3140D0
B1810242-22F2-4B23-BEEB-72D84DB2E4AF
8E438E67-3DFF-4DE4-9BE2-7354A4BBEAD0
56C1F0E4-2BA9-4E52-9F88-75A06495A1D4
57DC406B-548E-471C-8859-789E84B81816
F9C864F1-29BA-4DE7-B695-7B516EEE7E08
29FF2CB4-AA5F-4D83-BAA1-7CC4A8C311D0
0C309CD2-BDC6-46BC-A1BB-7EDC8335B603
C799D02B-BC75-4632-92CC-7F57D9868A96
219F2C94-2D2D-43EB-81D4-80C1E71F64FD
63C036EE-62A9-46EA-8DC4-814720A373D1
EF847DD1-1E15-4708-9D89-8448099E09AE
AABAA496-3AB2-474C-B433-84D758517288
0785EE30-B63C-49EA-BEDA-879674C33B94
13E4FD0E-AA5F-4716-9FA3-891E026FFEE2
CECCEB1F-DB80-4ADE-82AB-8C8B7B6662AF
AAA7535A-E97E-40F1-B1A1-90188A4FE64B
2FBA35A1-8062-4E30-B8F1-930649AB1CFB
4B35CB61-D5CB-463E-919F-93B6248293EC
F2568934-58EC-49D3-9F09-94D56C1DFAF2
CEE87F54-EA0E-434B-BE72-955E576BE5E0
8591D500-6AA6-46BE-885D-96405D2E2004
33A07A2B-3499-4FCB-8A21-98C02D94B4FC
DB1F1112-2405-4CAB-9DDB-9964E1E2785A
A2E24F2E-CAE3-4A6F-909C-9B1614EECA9D
338F1D8A-AC7A-4A7D-8BA8-9D01D895FF7C
7E321C4F-075A-4732-B11E-9E96BDE2EF07
158B9FBA-91C2-46EA-B4D4-A0C03EFE1514
26D42267-096E-40A6-98EE-A4D0ACB2D5DA
F56F64D8-B535-4CED-AF7C-A5AA37E49F00
61D8AEC8-1C44-47D6-9ABF-A7B7EF512AB6
48DA1E55-24AD-4F8B-832D-AA18287A5039
090221DC-7E34-4D3A-8E1B-AA245FD71293
13615BCB-A060-4BE1-8A66-AA841D4AD154
69FBCAF0-4ACA-4AAC-952A-AB8435CDEE96
E3458D61-6AE8-476F-A6C9-B0749E114C2B
9D650DD8-887B-487F-AFD9-B25EAE3F94C7
2E5CF897-FB2D-4663-AC6D-B4D356C0CA8F
03FB21B9-333C-4CEA-97D2-C30EAD267A51
F0A0909A-53D4-4CC7-BE28-C3BEC7B0FA33
ACFF56B1-0045-42D3-8108-C4C8CCDB27C7
7C925AD0-39BA-4A8E-87A5-C6E4C9666DC9
0C741399-0711-4701-82D5-C72E88953EDC
F2177A6E-229D-4590-9570-CA2D22C61CB0
79F82416-635E-442E-B539-CB3AD49D61C6
FF604D29-8F93-4551-9A1C-CF4D98433FCD
764B9785-A570-43AB-921A-D05484DB6EFA
752BCCB9-3865-4105-ADBC-D3E700B626EB
817607EF-DC6E-4C0A-8CC4-D4D58CEA2C67
05DCD4FD-95F5-467A-A854-DB8614CF49D1
93F8316C-9C12-42F6-B38D-DFAE35203DCF
D3F98692-E4AB-4A75-BEE1-E0849B94152B
D747EF37-C945-4F1C-8583-E4626C185A16
20C6BDDD-F983-4D0C-9A04-E77E8E8D9D06
36DD7A01-849C-42B6-B3FB-EC4BCB026802
9D21CA30-FC2B-4B9A-8D62-EF34BEE66D51
604FCDF1-D537-43D6-B394-F0309CAAAC79
4E8473E2-2B68-46B0-B143-F270161996A4
6192721E-AC93-4E8C-A670-F47070431343
AFFDC92C-8CBD-46F1-B6BC-F5EC4D685D3F
7D71C489-DBEE-4ECB-86C1-F62F0226FE1A
F531FCA3-328E-45E5-B0A6-F7453F465471
5B51658E-F716-401C-9FC2-FE70BBE70F48
 
*/
