using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private readonly IAssignmentResourceService assignmentResourceService;
    private readonly IAssignmentPackageService assignmentPackageService;
    private readonly IResourceService resourceService;
    private readonly IRoleResourceService roleResourceService;
    private readonly IRolePackageService rolePackageService;
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
    /// <param name="assignmentResourceService">IAssignmentResourceService</param>
    /// <param name="assignmentPackageService">IAssignmentPackageService</param>
    /// <param name="resourceService">IResourceService</param>
    /// <param name="roleResourceService">IRoleResourceService</param>
    /// <param name="rolePackageService">IRolePackageService</param>
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
    /// <param name="delegationPackageResourceService">IDelegationPackageService</param>
    /// <param name="delegationResourceService"></param>
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
        IAssignmentResourceService assignmentResourceService,
        IAssignmentPackageService assignmentPackageService,
        IResourceService resourceService,
        IRoleResourceService roleResourceService,
        IRolePackageService rolePackageService,
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
        IDelegationService delegationService
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
        this.assignmentResourceService = assignmentResourceService;
        this.assignmentPackageService = assignmentPackageService;
        this.resourceService = resourceService;
        this.roleResourceService = roleResourceService;
        this.rolePackageService = rolePackageService;
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

    private List<RoleResource> RoleResources { get; set; }

    private List<RolePackage> RolePackages { get; set; }

    private List<AssignmentResource> AssignmentResources { get; set; }

    private List<AssignmentPackage> AssignmentPackages { get; set; }

    private List<Role> Roles { get; set; }

    private List<Assignment> Assignments { get; set; }

    private List<EntityGroup> Groups { get; set; }

    private List<GroupAdmin> GroupAdmins { get; set; }

    private List<GroupMember> GroupMembers { get; set; }

    private List<Delegation> Delegations { get; set; }

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
        RoleResources = [.. await roleResourceService.Get()];
        RolePackages = [.. await rolePackageService.Get()];
        Resources = [.. await resourceService.Get()];

        AssignmentResources = [.. await assignmentResourceService.Get()];
        AssignmentPackages = [.. await assignmentPackageService.Get()];

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

    private List<Package> GetPackages()
    {
        return Packages;
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
        var res = Delegations.FirstOrDefault(t => t.FromId == obj.FromId && t.ToId == obj.ToId) ?? null;
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

    private async Task<Assignment> GetOrCreateAssignment(Assignment obj, bool verifyOnId = false)
    {
        Assignment? res;
        if (verifyOnId)
        {
            res = Assignments.FirstOrDefault(t => t.Id == obj.Id);
        }
        else
        {
            res = Assignments.FirstOrDefault(t => t.FromId == obj.FromId && t.ToId == obj.ToId && t.RoleId == obj.RoleId) ?? null;
        }

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

    private async Task<Entity> GetOrCreateEntity(Entity obj, bool verifyOnId = false)
    {
        Entity? res;
        if (verifyOnId)
        {
            res = Entities.FirstOrDefault(t => t.Id == obj.Id);
        }
        else
        {
            res = Entities.FirstOrDefault(t => t.Name == obj.Name && t.TypeId == obj.TypeId && t.RefId == obj.RefId);
        }

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

    private async Task<RoleResource> GetOrCreateRoleResource(Guid roleId, Guid resourceId)
    {
        var res = RoleResources.FirstOrDefault(t => t.RoleId == roleId && t.ResourceId == resourceId) ?? null;
        if (res == null)
        {
            var obj = new RoleResource() { Id = Guid.NewGuid(), RoleId = roleId, ResourceId = resourceId };
            await roleResourceService.Create(obj);
            RoleResources.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<AssignmentPackage> GetOrCreateAssignmentPackage(Guid assignmentId, Guid packageId)
    {
        var res = AssignmentPackages.FirstOrDefault(t => t.AssignmentId == assignmentId && t.PackageId == packageId) ?? null;
        if (res == null)
        {
            var obj = new AssignmentPackage() { Id = Guid.NewGuid(), AssignmentId = assignmentId, PackageId = packageId };
            await assignmentPackageService.Create(obj);
            AssignmentPackages.Add(obj);
            return obj;
        }
        else
        {
            return res;
        }
    }

    private async Task<AssignmentResource> GetOrCreateAssignmentResource(Guid assignmentId, Guid resourceId)
    {
        var res = AssignmentResources.FirstOrDefault(t => t.AssignmentId == assignmentId && t.ResourceId == resourceId) ?? null;
        if (res == null)
        {
            var obj = new AssignmentResource() { Id = Guid.NewGuid(), AssignmentId = assignmentId, ResourceId = resourceId };
            await assignmentResourceService.Create(obj);
            AssignmentResources.Add(obj);
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

        //var regnDelegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), AssignmentId = ass03.Id });

        //await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, PackageId = regnPakke1.Id });
        //await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, PackageId = regnPakke3.Id });

        //var reviDelegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), AssignmentId = ass03_2.Id });
        //await GetOrCreateDelegationPackage(new DelegationPackage() { Id = Guid.NewGuid(), DelegationId = reviDelegation.Id, PackageId = regnPakke2.Id });

        //var grp01 = await GetOrCreateGroup(new EntityGroup() { Id = Guid.NewGuid(), Name = "A-Klienter", OwnerId = regnskapNorge.Id, RequireRole = true });

        //var grpAdm01 = await GetOrCreateGroupAdmin(new GroupAdmin() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = gunnarJohnsen.Id });
        //var grpMem01 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = perHansen.Id });
        //var grpMem02 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = gunillaJonson.Id });
        //var grpMem03 = await GetOrCreateGroupMember(new GroupMember() { Id = Guid.NewGuid(), GroupId = grp01.Id, MemberId = viggoKristiansen.Id });

        //var regnDelGroup = await GetOrCreateDelegationGroup(new DelegationGroup() { Id = Guid.NewGuid(), DelegationId = regnDelegation.Id, GroupId = grp01.Id });

        //var reviDelegAss = await GetOrCreateDelegationAssignment(new DelegationAssignment() { Id = Guid.NewGuid(), DelegationId = reviDelegation.Id, AssignmentId = ass09.Id });

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
        //var delegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.Parse("7E8036B4-43AB-4535-9BA4-0FE745C50F79"), AssignmentId = assignment.Id });
        // var delegationResource = await GetOrCreateDelegationResource(new DelegationResource());
        //var delegateGunnar = await GetOrCreateDelegationAssignment(new DelegationAssignment() { Id = Guid.Parse("50F2C1F2-CEEB-4CEA-9FE0-138E3FEE6FFD"), DelegationId = delegation.Id, AssignmentId = gunnarAnsattAwesome.Id });
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




    //    /*

        //    Vi har nå 120 organisasjoner med ansatte og kjente roller

        //    Noen ansatte må få tildelt pakker og andre må få ressurser 
        //    (Pakker hentes fra bransje området. Ressurser også.) AssignmentPackage + AssignmentResource

        //    Vi trenger nå REGN og REVI som peker på Orgs i bransjene regn og revi
        //    Vi burde også ha noen som har personer som regn og revi

        //    Vi oppretter (klient)delegeringer av REGN og REVI.

        //    */



        //    /*

        //    Trenger en metode som oppretter X antall firmaer av typen Bransje med antall ansatte varierende. 
        //    Jeg har da en Dict med <bransje,list<entity>>
        //    Og en liste med assignments.

        //    En liten metode som bare oppretter entiteter med assignments
        //    En liten metode som oppretter Regn Assigment mellom to entiteter
        //    En metode som gir meg en Rnd Entity av gitt bransje

        //     */


        //    /*

        //    Ny modell

        //    - Opprett X regnskapsfirmaer med X antall ansatte
        //    - Opprett X revisorfirmaer med X antall ansatte
        //    - Opprett X firmaer med X antall ansatt

        //     */

        //    /*

        //    Scenario:
        //    3 Regnskapsfirmaer
        //    3 Revisorfirmaer
        //    5 Vanlige firmaer
        //    5-10 ansatte i hvert firma
        //    10 Personer uten tilhørigheter

        //    */
        //    await LoadCache();

        //    var firma00 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("090221DC-7E34-4D3A-8E1B-AA245FD71293")), true);
        //    var firma01 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("26D42267-096E-40A6-98EE-A4D0ACB2D5DA")), true);
        //    var firma02 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("F56F64D8-B535-4CED-AF7C-A5AA37E49F00")), true);
        //    var firma03 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("61D8AEC8-1C44-47D6-9ABF-A7B7EF512AB6")), true);
        //    var firma04 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("48DA1E55-24AD-4F8B-832D-AA18287A5039")), true);
        //    var firma05 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("090221DC-7E34-4D3A-8E1B-AA245FD71293")), true);
        //    var firma06 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("26D42267-096E-40A6-98EE-A4D0ACB2D5DA")), true);
        //    var firma07 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("F56F64D8-B535-4CED-AF7C-A5AA37E49F00")), true);
        //    var firma08 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("61D8AEC8-1C44-47D6-9ABF-A7B7EF512AB6")), true);
        //    var firma09 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("48DA1E55-24AD-4F8B-832D-AA18287A5039")), true);

        //    var regn1 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("33A07A2B-3499-4FCB-8A21-98C02D94B4FC"), "Regnskap"), true);
        //    var regn2 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("DB1F1112-2405-4CAB-9DDB-9964E1E2785A"), "Regnskapsførere"), true);
        //    var regn3 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("A2E24F2E-CAE3-4A6F-909C-9B1614EECA9D"), "Regnskap"), true);

        //    var revi1 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("338F1D8A-AC7A-4A7D-8BA8-9D01D895FF7C"), "Revisor"), true);
        //    var revi2 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("7E321C4F-075A-4732-B11E-9E96BDE2EF07"), "Revisjon"), true);
        //    var revi3 = await GetOrCreateEntity(GenerateOrgEntity(Guid.Parse("158B9FBA-91C2-46EA-B4D4-A0C03EFE1514"), "Revisor"), true);

        //    var pers00 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F4C7CFB8-0C6C-454C-BA09-66AA4EA11F8E")), true);
        //    var pers01 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("6FC79183-7753-4F39-910D-66AEC4C330E1")), true);
        //    var pers02 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("76BF013D-3BB7-49FD-AC69-681BD282FF30")), true);
        //    var pers03 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8DCFB7F1-7A7B-44E9-BB75-6996C26787C8")), true);
        //    var pers04 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("533E0678-C40E-423D-B3C7-6C31196FE2E3")), true);
        //    var pers05 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("FBCE49E1-215B-4029-A948-6C68DCC6E410")), true);
        //    var pers06 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("7F28F8F8-8E25-4BA1-B895-6DCA291DE8A7")), true);
        //    var pers07 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F9E7F134-D109-4F48-AD85-6FDB8CE17FCC")), true);
        //    var pers08 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("DCBB8926-40B9-42D2-A6E6-722FBC3140D0")), true);
        //    var pers09 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("B1810242-22F2-4B23-BEEB-72D84DB2E4AF")), true);
        //    var pers10 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8E438E67-3DFF-4DE4-9BE2-7354A4BBEAD0")), true);
        //    var pers11 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("56C1F0E4-2BA9-4E52-9F88-75A06495A1D4")), true);
        //    var pers12 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("57DC406B-548E-471C-8859-789E84B81816")), true);
        //    var pers13 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F9C864F1-29BA-4DE7-B695-7B516EEE7E08")), true);
        //    var pers14 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("29FF2CB4-AA5F-4D83-BAA1-7CC4A8C311D0")), true);
        //    var pers15 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0C309CD2-BDC6-46BC-A1BB-7EDC8335B603")), true);
        //    var pers16 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C799D02B-BC75-4632-92CC-7F57D9868A96")), true);
        //    var pers17 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("219F2C94-2D2D-43EB-81D4-80C1E71F64FD")), true);
        //    var pers18 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("63C036EE-62A9-46EA-8DC4-814720A373D1")), true);
        //    var pers19 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("EF847DD1-1E15-4708-9D89-8448099E09AE")), true);
        //    var pers20 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("AABAA496-3AB2-474C-B433-84D758517288")), true);
        //    var pers21 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0785EE30-B63C-49EA-BEDA-879674C33B94")), true);
        //    var pers22 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("13E4FD0E-AA5F-4716-9FA3-891E026FFEE2")), true);
        //    var pers23 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("CECCEB1F-DB80-4ADE-82AB-8C8B7B6662AF")), true);
        //    var pers24 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("AAA7535A-E97E-40F1-B1A1-90188A4FE64B")), true);
        //    var pers25 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("2FBA35A1-8062-4E30-B8F1-930649AB1CFB")), true);
        //    var pers26 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("4B35CB61-D5CB-463E-919F-93B6248293EC")), true);
        //    var pers27 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F2568934-58EC-49D3-9F09-94D56C1DFAF2")), true);
        //    var pers28 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("CEE87F54-EA0E-434B-BE72-955E576BE5E0")), true);
        //    var pers29 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8591D500-6AA6-46BE-885D-96405D2E2004")), true);
        //    var pers30 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("A3FB575B-0C88-48FC-A6FF-4FFEB7EC8463")), true);
        //    var pers31 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("55E50C9A-FA01-47DF-810A-51F398E02684")), true);
        //    var pers32 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("5789C32D-7113-48DA-9237-65F7FD71CB4D")), true);
        //    var pers33 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("BAF2C532-6CF2-4278-A74D-92AB63EC776F")), true);
        //    var pers34 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("71FCD423-2C7D-46BF-BCFC-95022053CB44")), true);
        //    var pers35 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("CCAD7019-D0CC-4DCA-AC20-C77E3EE1F43F")), true);
        //    var pers36 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("A832AC35-8734-449D-ADD3-D4E0A4E448E3")), true);
        //    var pers37 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("433B3DD7-E5C1-49E2-A138-D9AF44530ED0")), true);
        //    var pers38 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("668B960E-7A7C-4BF6-A74A-EBA232C608F9")), true);
        //    var pers39 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("6332E3C0-6C00-4FEC-AA01-FEBA142B5ABB")), true);
        //    var pers40 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("234E5905-DDE3-49F0-889B-09436B8CE27E")), true);
        //    var pers41 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("3D9D171F-104D-40BE-A71C-18372EF3BC57")), true);
        //    var pers42 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("D09784D3-2DCD-4B2D-AD34-19934466435F")), true);
        //    var pers43 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("4E64E8F7-0A2A-48C2-8605-75B710CEE46C")), true);
        //    var pers44 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("834A299C-6E26-4885-BEA4-8B562AB086A6")), true);
        //    var pers45 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("914FC41D-3595-42D1-8676-8DF80287DC95")), true);
        //    var pers46 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("2D7287A8-A3EE-49A8-AE22-AAA0159118C5")), true);
        //    var pers47 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("EA2873EC-D8C7-432C-8ACC-B47B88EC18C3")), true);
        //    var pers48 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8242DD0F-8E8F-480E-9BA1-C9BFD4408E8D")), true);
        //    var pers49 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("2E35B593-4E89-4FEA-9B11-DC357263AC50")), true);
        //    var pers50 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("5ABD8D0E-9ADD-443F-8D37-029EFBF03366")), true);
        //    var pers51 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C934BFE0-D8AA-40E3-AA90-1515BB287DC4")), true);
        //    var pers52 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("004E91CF-B537-4DC8-BEB3-267A329F4265")), true);
        //    var pers53 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("ECB7CE4A-A3E8-47A3-AA86-3B2510D00756")), true);
        //    var pers54 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F49ED52B-C590-43B8-8AC3-5419A7B1B4C5")), true);
        //    var pers55 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("24F6480B-8871-4121-BD2E-86512C7E7D7A")), true);
        //    var pers56 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("D0881023-1D2C-4722-B9AF-A9F85027987C")), true);
        //    var pers57 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8AC531B0-D55A-4865-8FB3-C6D6C8246FB8")), true);
        //    var pers58 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("EF9A6764-3433-41FD-AB03-CE5A9CF594AF")), true);
        //    var pers59 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("2379B2E6-FAA9-4A4F-95AE-EFCDBECE5BB7")), true);
        //    var pers60 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("7D75572B-AA47-4854-82BD-43E1C806FE7C")), true);
        //    var pers61 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("80B59177-E6E5-4713-95A3-4E76716C26CD")), true);
        //    var pers62 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("AE015C26-F9A5-4C7D-8584-4F502A5E8130")), true);
        //    var pers63 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8222C358-9CD2-42AF-BBDE-8AE519673B16")), true);
        //    var pers64 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0CD9969E-F7C4-45A6-B92A-8C664E14F6C2")), true);
        //    var pers65 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("07C50530-38B0-4CF8-AFB2-989D82688BEA")), true);
        //    var pers66 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("BA743DC1-255F-4A96-984A-9DD871AD8299")), true);
        //    var pers67 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0675491B-4A6C-4FFF-95B1-A8AFC5896FEF")), true);
        //    var pers68 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("4F719BA1-2EAC-4504-A2A7-AD9A5F4EBAE7")), true);
        //    var pers69 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("4233BC5D-2BCA-48A3-8214-EC7EE05DAA52")), true);
        //    var pers70 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F9E5C5BF-A484-411A-A9A6-3D83D3666814")), true);
        //    var pers71 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("20D09149-BE9C-46E6-887B-6F3B03B6BFF3")), true);
        //    var pers72 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("085B87C9-8000-4179-8A24-877A5E9C1F66")), true);
        //    var pers73 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8D281978-EA4C-4C55-94E7-87CA7E0CA87E")), true);
        //    var pers74 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("3A186212-23F8-414F-83B7-9752BA32CA8C")), true);
        //    var pers75 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("2429F2EF-685A-4F62-97CD-A1308D22582B")), true);
        //    var pers76 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8B4A8016-994D-471B-BF75-C08FF9E75712")), true);
        //    var pers77 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("765F6CD4-E222-486B-AD63-C6C46074E55B")), true);
        //    var pers78 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("FD95F51D-3594-4E3D-BEFC-D724F381E6F8")), true);
        //    var pers79 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("7C3F65F9-DECF-4672-9497-DCE372F398DE")), true);
        //    var pers80 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("AEFA6AAE-DA2D-4641-80FA-049A5DADF710")), true);
        //    var pers81 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("A5D81F18-2FBF-4409-8950-16CE60DED401")), true);
        //    var pers82 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("A070EA75-BF1F-4289-8944-2AC5323B31FF")), true);
        //    var pers83 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("8460CE26-0F32-4651-969E-3441EE5C6AA8")), true);
        //    var pers84 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("B3229B50-FCC6-4687-AEF3-7E7874F60AA7")), true);
        //    var pers85 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("E4A0EB1F-C228-4B54-94C0-7F397EF6DD6A")), true);
        //    var pers86 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("1A73290A-7856-4051-893B-9ED5D0756203")), true);
        //    var pers87 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("28BC3CFA-3BE7-497C-A9A3-AE3358E565B7")), true);
        //    var pers88 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("9BB5BDC5-A852-4852-BF99-D1400DE3D051")), true);
        //    var pers89 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("A230897E-D5DC-4158-A32B-D424CC51D1A4")), true);
        //    var pers90 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("5DBADE66-F173-4647-B7E5-047F20E884B6")), true);
        //    var pers91 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("6D83868E-2A74-43C3-B240-0D6CA97E9A1A")), true);
        //    var pers92 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("905551FB-03B7-4014-AEFF-37D2D30866BB")), true);
        //    var pers93 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C7D50A21-7867-4009-BF45-41CA4D1CFA2E")), true);
        //    var pers94 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C333034C-E1A9-4CE1-830B-4C3F09074759")), true);
        //    var pers95 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("B0C205DF-73BA-498B-BDC5-6C246A7B1539")), true);
        //    var pers96 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("1B6C05F3-1E49-4D67-910E-7B276A3D00F1")), true);
        //    var pers97 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("755DC800-10B1-471E-B6D7-AF1C31309D8C")), true);
        //    var pers98 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0C3C2E60-72B3-4C71-B88D-E6E9049F3C42")), true);
        //    var pers99 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("00282DB2-6264-4C41-8E0A-F3A3FAD727F9")), true);

        //    var pers100 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F5CC35DB-E522-4C5B-90A0-1EB86BA9F039")), true);
        //    var pers101 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("56178FEB-994E-469D-BD89-230984E34A3F")), true);
        //    var pers102 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C33B521E-A654-4971-9B19-3B940D5C471C")), true);
        //    var pers103 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0935833F-4C14-4AEB-9745-49D91953D4E9")), true);
        //    var pers104 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("22139EF3-2F9F-4965-80EA-81AE6D13CCA4")), true);
        //    var pers105 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("3CB558B8-0796-452F-A822-90D4C0878515")), true);
        //    var pers106 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F88C498C-D909-4EDA-AB1E-CB2F4A2177FD")), true);
        //    var pers107 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("FDBCAA56-2B0D-4160-B798-D9C976682434")), true);
        //    var pers108 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("92683693-D5B6-4625-BE3C-F0B792D0EE0D")), true);
        //    var pers109 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("3450EB83-7E50-4C24-90D6-F52965D73452")), true);

        //    var pers110 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("72D8B57E-6711-4434-AC4A-0925A722E159")), true);
        //    var pers111 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("0563BC5D-EF0D-48D1-9861-25A744C245D6")), true);
        //    var pers112 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("6CF772F5-A099-44D9-836E-44DB18F7A8B9")), true);
        //    var pers113 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("6A5D1C30-BB8B-445E-9775-525B19897312")), true);
        //    var pers114 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("15C80133-1EAF-4003-ABE5-5619C7A2BD35")), true);
        //    var pers115 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("24FC65F6-66C0-42D3-87DC-6E051B93BA05")), true);
        //    var pers116 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("C55BD254-3E1B-4584-A8BA-788842263184")), true);
        //    var pers117 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("F24879F9-45A1-492D-81FE-9B7EBAB1E6E1")), true);
        //    var pers118 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("D109637D-E536-4123-A0F6-9CD8D4001749")), true);
        //    var pers119 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("DBDFB4B5-B1CF-493E-BFD7-C029F8444798")), true);

        //    var pers120 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("4CFABE31-55E8-434C-9D8D-27D6642BC1BA")), true);
        //    var pers121 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("98E2ECFF-B7C6-45CA-9B1B-55BB5361DED6")), true);
        //    var pers122 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("FD3BFB2F-FC67-4B75-A62D-6C2774B07C53")), true);
        //    var pers123 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("DEB3D756-69C4-4CE6-B695-86FAA7B7F024")), true);
        //    var pers124 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("9FF2EC12-66B0-430D-8BA4-889B152BC906")), true);
        //    var pers125 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("3D376D4B-A335-4D6B-AA88-8BC24C000018")), true);
        //    var pers126 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("B7E70616-2025-4CF5-99C2-936401488AED")), true);
        //    var pers127 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("D1781ACB-5E56-4502-B563-9B416ACDC897")), true);
        //    var pers128 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("1E24E030-7E7B-474B-A903-C7370D90692A")), true);
        //    var pers129 = await GetOrCreateEntity(GeneratePersonEntity(Guid.Parse("70CA2312-B5C9-48E2-A90B-F7B155BAC100")), true);

        //    /*DAGL*/
        //    var ass01 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers00.Id, RoleId = dagl.Id });
        //    var ass02 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers01.Id, RoleId = dagl.Id });
        //    var ass03 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers02.Id, RoleId = dagl.Id });
        //    var ass04 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers03.Id, RoleId = dagl.Id });
        //    var ass05 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers04.Id, RoleId = dagl.Id });
        //    var ass06 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers05.Id, RoleId = dagl.Id });
        //    var ass07 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers06.Id, RoleId = dagl.Id });
        //    var ass08 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers07.Id, RoleId = dagl.Id });
        //    var ass09 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers08.Id, RoleId = dagl.Id });
        //    var ass10 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers09.Id, RoleId = dagl.Id });
        //    var ass11 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers10.Id, RoleId = dagl.Id });

        //    /*LEDE*/
        //    var ass12 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers11.Id, RoleId = lede.Id });
        //    var ass13 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers12.Id, RoleId = lede.Id });
        //    var ass14 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers13.Id, RoleId = lede.Id });
        //    var ass15 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers14.Id, RoleId = lede.Id });
        //    var ass16 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers15.Id, RoleId = lede.Id });
        //    var ass17 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers16.Id, RoleId = lede.Id });
        //    var ass18 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers17.Id, RoleId = lede.Id });
        //    var ass19 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers18.Id, RoleId = lede.Id });
        //    var ass20 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers19.Id, RoleId = lede.Id });
        //    var ass21 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers09.Id, RoleId = lede.Id });
        //    var ass22 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers10.Id, RoleId = lede.Id });

        //    /*REGN*/
        //    var ass23 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = regn1.Id, RoleId = regn.Id });
        //    var ass24 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers11.Id, RoleId = regn.Id });
        //    var ass25 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers11.Id, RoleId = regn.Id });
        //    var ass26 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = regn2.Id, RoleId = regn.Id });
        //    var ass27 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = regn3.Id, RoleId = regn.Id });
        //    var ass28 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = regn2.Id, RoleId = regn.Id });
        //    var ass29 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = regn1.Id, RoleId = regn.Id });
        //    var ass30 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = regn2.Id, RoleId = regn.Id });
        //    var ass31 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = regn3.Id, RoleId = regn.Id });
        //    var ass32 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = regn3.Id, RoleId = regn.Id });
        //    var ass33 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = regn3.Id, RoleId = regn.Id });

        //    /*REVI*/
        //    var ass34 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = revi1.Id, RoleId = revi.Id });
        //    var ass35 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = revi2.Id, RoleId = revi.Id });
        //    var ass36 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = revi2.Id, RoleId = revi.Id });
        //    var ass37 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = revi3.Id, RoleId = revi.Id });
        //    var ass38 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = revi3.Id, RoleId = revi.Id });
        //    var ass39 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = revi1.Id, RoleId = revi.Id });
        //    var ass40 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = revi1.Id, RoleId = revi.Id });
        //    var ass41 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = revi1.Id, RoleId = revi.Id });
        //    var ass42 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = revi3.Id, RoleId = revi.Id });
        //    var ass43 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = revi3.Id, RoleId = revi.Id });
        //    var ass44 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = revi2.Id, RoleId = revi.Id });

        //    /*ANSATT*/
        //    var ansattAssF1A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers20.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers21.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers22.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers23.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers24.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers25.Id, RoleId = ansatt.Id });
        //    var ansattAssF1A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers26.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF1A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers27.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF1A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers28.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF1A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma01.Id, ToId = pers29.Id, RoleId = ansatt.Id });

        //    var ansattAssF2A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers30.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers31.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers32.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers33.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers34.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers35.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers36.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers37.Id, RoleId = ansatt.Id });
        //    var ansattAssF2A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers38.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF2A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma02.Id, ToId = pers39.Id, RoleId = ansatt.Id });

        //    var ansattAssF3A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers40.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers41.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers42.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers43.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers44.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers45.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers46.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers47.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers48.Id, RoleId = ansatt.Id });
        //    var ansattAssF3A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma03.Id, ToId = pers49.Id, RoleId = ansatt.Id });

        //    var ansattAssF4A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers50.Id, RoleId = ansatt.Id });
        //    var ansattAssF4A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers51.Id, RoleId = ansatt.Id });
        //    var ansattAssF4A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers52.Id, RoleId = ansatt.Id });
        //    var ansattAssF4A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers53.Id, RoleId = ansatt.Id });
        //    var ansattAssF4A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers54.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF4A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers55.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF4A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers56.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF4A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers57.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF4A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers58.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF4A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma04.Id, ToId = pers59.Id, RoleId = ansatt.Id });

        //    var ansattAssF5A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers60.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers61.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers62.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers63.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers64.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers65.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers66.Id, RoleId = ansatt.Id });
        //    var ansattAssF5A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers67.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF5A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers68.Id, RoleId = ansatt.Id });
        //    //// var ansattAssF5A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = firma05.Id, ToId = pers69.Id, RoleId = ansatt.Id });

        //    var ansattAssG1A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers70.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers71.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers72.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers73.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers74.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers75.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers76.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers77.Id, RoleId = ansatt.Id });
        //    var ansattAssG1A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers78.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG1A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn1.Id, ToId = pers79.Id, RoleId = ansatt.Id });

        //    var ansattAssG2A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers80.Id, RoleId = ansatt.Id });
        //    var ansattAssG2A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers81.Id, RoleId = ansatt.Id });
        //    var ansattAssG2A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers82.Id, RoleId = ansatt.Id });
        //    var ansattAssG2A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers83.Id, RoleId = ansatt.Id });
        //    var ansattAssG2A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers84.Id, RoleId = ansatt.Id });
        //    var ansattAssG2A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers85.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG2A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers86.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG2A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers87.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG2A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers88.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG2A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn2.Id, ToId = pers89.Id, RoleId = ansatt.Id });

        //    var ansattAssG3A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers90.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers91.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers92.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers93.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers94.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers95.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers96.Id, RoleId = ansatt.Id });
        //    var ansattAssG3A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers97.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG3A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers98.Id, RoleId = ansatt.Id });
        //    //// var ansattAssG3A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = regn3.Id, ToId = pers99.Id, RoleId = ansatt.Id });

        //    var ansattAssV1A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers100.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers101.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers102.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers103.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers104.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers105.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers106.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers107.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers108.Id, RoleId = ansatt.Id });
        //    var ansattAssV1A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi1.Id, ToId = pers109.Id, RoleId = ansatt.Id });

        //    var ansattAssV2A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers110.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers111.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers112.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers113.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers114.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers115.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers116.Id, RoleId = ansatt.Id });
        //    var ansattAssV2A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers117.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV2A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers118.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV2A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi2.Id, ToId = pers119.Id, RoleId = ansatt.Id });

        //    var ansattAssV3A0 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers120.Id, RoleId = ansatt.Id });
        //    var ansattAssV3A1 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers121.Id, RoleId = ansatt.Id });
        //    var ansattAssV3A2 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers122.Id, RoleId = ansatt.Id });
        //    var ansattAssV3A3 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers123.Id, RoleId = ansatt.Id });
        //    var ansattAssV3A4 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers124.Id, RoleId = ansatt.Id });
        //    var ansattAssV3A5 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers125.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV3A6 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers126.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV3A7 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers127.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV3A8 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers128.Id, RoleId = ansatt.Id });
        //    //// var ansattAssV3A9 = await GetOrCreateAssignment(new Assignment() { Id = Guid.NewGuid(), FromId = revi3.Id, ToId = pers129.Id, RoleId = ansatt.Id });

        //    var packages = GetPackages();
        //    var randomRoleMap = new Dictionary<int, Guid>()
        //    {
        //        { 7, dagl.Id },
        //        { 12, lede.Id }
        //    };

        //    foreach (var pkg in packages)
        //    {
        //        await GeneratePackageResources(pkg, randomRoleMap);
        //    }

        //    // await GetOrCreateAssignmentResource(ass01.Id, GetResource(packages.First(t => t.Name.Contains("Skatt")),1).Id);
        //    // await GetOrCreateAssignmentPackage(ass01.Id, packages.First(t => t.Name.Contains("Skatt")).Id);

        //    /*
        //    MORE ASSIGNMENT RESOURCES/PACKAGES
        //    */

        //    var delRegnF1G1A5 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = firma01.Id, FromId = ass23.Id, ToId = ansattAssG1A5.Id, ViaId = regn1.Id });
        //    var delRegnF4G2A3 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = firma04.Id, FromId = ass26.Id, ToId = ansattAssG2A3.Id, ViaId = regn2.Id });
        //    var delRegnF5G3A3 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = firma05.Id, FromId = ass27.Id, ToId = ansattAssG3A3.Id, ViaId = regn3.Id });
        //    var delRegnG1G2A4 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = regn1.Id, FromId = ass28.Id, ToId = ansattAssG2A4.Id, ViaId = regn2.Id });
        //    var delRegnG2G1A1 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = regn2.Id, FromId = ass29.Id, ToId = ansattAssG1A4.Id, ViaId = regn1.Id });
        //    var delRegnG3G2A5 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = regn3.Id, FromId = ass30.Id, ToId = ansattAssG2A5.Id, ViaId = regn2.Id });
        //    var delRegnV1G0A1 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = revi1.Id, FromId = ass31.Id, ToId = ansattAssG3A4.Id, ViaId = regn3.Id });
        //    var delRegnV2G0A1 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = revi2.Id, FromId = ass32.Id, ToId = ansattAssG3A5.Id, ViaId = regn3.Id });
        //    var delRegnV3G0A1 = await GetOrCreateDelegation(new Delegation() { Id = Guid.NewGuid(), SourceId = revi3.Id, FromId = ass33.Id, ToId = ansattAssG3A5.Id, ViaId = regn3.Id });

        //    //await GetOrCreateDelegationPackage(new DelegationPackage()
        //    //{
        //    //    Id = Guid.NewGuid(),
        //    //    DelegationId = delAss01Ass02.Id,
        //    //    PackageResourceId = Guid.Empty
        //    //});

        //    //await GetOrCreateDelegationResource(new DelegationResource()
        //    //{
        //    //    Id = Guid.NewGuid(),
        //    //    DelegationId = delAss01Ass02.Id,
        //    //    AssignmentResourceId = Guid.Empty,
        //    //    RoleResourceId = Guid.Empty
        //    //});

        //}


    private static Guid GenerateFakeGuid(string objectType, int parentId, int id)
    {
        return GenerateFakeGuid($"{objectType}:{parentId}:{id}");
    }
    private static Guid GenerateFakeGuid(string key)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            return new Guid(hash);
        }
    }
    static T GetRandomItem<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("List is empty");
        }

        int index = MockTools.Rnd.Next(list.Count);
        return list[index];
    }
    static T GetRandomItem<T>(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        int count = collection.Count();
        if (count == 0)
            throw new ArgumentException("List is empty");

        int randomIndex = MockTools.Rnd.Next(count);
        return collection.ElementAt(randomIndex);
    }


    public async Task BasicMock()
    {
        await LoadCache();

        

        var dagl = GetRole("DAGL");
        var lede = GetRole("LEDE");
        var medl = GetRole("MEDL");
        var regn = GetRole("REGN");
        var revi = GetRole("REVI");
        var ansatt = await GetOrCreateRole(new Role() { Id = Guid.Parse("98f99fcb-21af-4155-a84a-9493c14144ac"), Code = "ANSATT", Name = "Ansatt", Description = "Ansatt i firma", Urn = "digdir:entity:ansatt", EntityTypeId = GetEntityType("Organisasjon").Id, ProviderId = GetProvider("Digdir").Id });

        var randomRoleMap = new Dictionary<int, Guid>()
            {
                { 7, dagl.Id },
                { 12, lede.Id }
            };
        foreach (var pkg in Packages)
        {
            await GeneratePackageResources(pkg, randomRoleMap);
        }

        int stdAntall = 10;
        int stdPeople = 15;

        var specs = new List<(string industry, int antall, int ansatte)>()
        {
            ("Bygg, anlegg og eiendom", stdAntall, stdPeople),
            ("Energi, vann,avløp og avfall", stdAntall, stdPeople),
            ("Handel, overnatting og servering", stdAntall, stdPeople),
            ("Helse, pleie, omsorg og vern", stdAntall, stdPeople),
            ("Industrier", stdAntall, stdPeople),
            ("Jordbruk, skogbruk, jakt, fiske og akvakultur", stdAntall, stdPeople),
            ("Kultur og frivillighet", stdAntall, stdPeople),
            ("Oppvekst og utdaning", stdAntall, stdPeople),
            ("Transport og lagring", stdAntall, stdPeople),
            ("Andre tjenesteytende næringer", stdAntall, stdPeople),
            ("Regnskap", stdAntall, stdPeople),
            ("Revisjon", stdAntall, stdPeople),
        };

        var orgs = new Dictionary<string, List<Entity>>();
        var people = new List<Entity>();
        var borgere = new List<Entity>(); // Folk uten direkte ansettelsesforhold
        var assignments = new List<Assignment>();
        int i = 0;

        foreach (var spec in specs)
        {
            orgs.Add(spec.industry, new List<Entity>());
            int medlCount = (int)Math.Round((10.0 / 100) * spec.ansatte, MidpointRounding.AwayFromZero);

            //// Om det er et stort regnskap/revisor-firma skal man assigne flere pakker? Eller om det er et stor basefirma (som da driver med mye stæsj ... Nei, det kommer til de vanlige pakkene og ressursene til ansatte...)

            /*
             Noen burde få ansvar for Personale området. Om det er et stort firma så fordel det på flere med mye over lapp. Om det er et lite firma så gir man mer til 1-2 personer.

                Ansetttelsesforhold => Ledelsen
                Lønn => HR, Ledelsen
                Pensjon => HR
                Permisjon => HR
                Sykefravær => Avdelingslederer, HR, Ledelsen

            Store firmaer har kanskje outsourcet Lønn og pensjon eller Sykefravær til en annen bedrift.
             */

            for (int j = 0; j < spec.antall; j++)
            {
                string orgKey = $"entity:org:{i}:{j}";
                var comp = await GetOrCreateEntity(GenerateOrgEntity(GenerateFakeGuid(orgKey), spec.industry), true);
                orgs[spec.industry].Add(comp);

                int employeeCount = spec.ansatte;
                int percentEmployeeAdjust = comp.Name.Count(c => c == 'e') + 1;
                if ((comp.Name.Length & 1) == 1)
                {
                    // Remove 2%
                    employeeCount -= employeeCount * (1 + percentEmployeeAdjust / 100);
                }
                else
                {
                    // Add 2%
                    employeeCount += employeeCount * (1 + percentEmployeeAdjust / 100);
                }

                /*PEOPLE + ASSIGNMENTS*/
                for (int k = 0; k < spec.ansatte; k++)
                {
                    var personKey = $"{orgKey}:person:{k}";
                    var e = await GetOrCreateEntity(GeneratePersonEntity(GenerateFakeGuid(personKey)), true);

                    people.Add(e);

                    await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:ansatt" + k), FromId = comp.Id, ToId = e.Id, RoleId = ansatt.Id }, true);

                    if (employeeCount <= 10)
                    {
                        if (k==0)
                        {
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:dagl" + k), FromId = comp.Id, ToId = e.Id, RoleId = dagl.Id }, true);
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:lede" + k), FromId = comp.Id, ToId = e.Id, RoleId = lede.Id }, true);
                        }
                    }
                    else
                    {
                        if (k==0)
                        {
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:dagl" + k), FromId = comp.Id, ToId = e.Id, RoleId = dagl.Id }, true);
                        }
                        if (k==1)
                        {
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:lede" + k), FromId = comp.Id, ToId = e.Id, RoleId = lede.Id }, true);
                        }
                    }

                    if (k < medlCount)
                    {
                        await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:medl" + k), FromId = comp.Id, ToId = e.Id, RoleId = medl.Id }, true);
                    }
                }
            }
            i++;
        }

        var pck1 = Packages.FirstOrDefault(t => t.Name == "Skatt næring");
        var pck2 = Packages.FirstOrDefault(t => t.Name == "Skattegrunnlag");
        var pck3 = Packages.FirstOrDefault(t => t.Name == "Merverdiavgift");
        var pck4 = Packages.FirstOrDefault(t => t.Name == "Regnskap og økonomirapportering");
        var pck5 = Packages.FirstOrDefault(t => t.Name == "Revisorattesterer");

        #region Regn+Revi Assignments
        /*Regn+Revi Assignment*/
        foreach (var org in orgs)
        {
            int a = 0;
            foreach (var entity in org.Value)
            {
                /*
                TODO: Om Regn/Revi er Person, ikke Org ...
                Mindre selskaper har kanskje bare en person inne på deltid for regnskap og ingen revisor.
                Disse vil ikke ha REGN REVI rollen
                */

                //// TODO: Move to AssignRegn Method ... 

                var size = assignments.Count(t => t.FromId == entity.Id && t.RoleId == ansatt.Id);
                var oddEvenSize = (size & 1) == 1;
                var oddEvenName = (entity.Name.Length & 1) == 1;

                if (size > 10)
                {
                    var revisor = GetRandomItem(orgs.First(t => t.Key == "Revisjon").Value.Where(t => t.Id != entity.Id));
                    var reviAss = await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{entity.Id}:assign:revi:" + a), FromId = entity.Id, ToId = revisor.Id, RoleId = revi.Id }, true);
                    assignments.Add(reviAss);

                    if (oddEvenName)
                    {
                        // Add package
                        GetOrCreateAssignmentPackage(reviAss.Id, pck5.Id);
                        if (oddEvenSize)
                        {
                            //Add resources
                            var res = GetRandomItem(PackageResources.Where(t => t.PackageId == pck2.Id));
                            GetOrCreateAssignmentResource(reviAss.Id, res.ResourceId);
                        }
                    }
                    else
                    {
                        GetOrCreateAssignmentPackage(reviAss.Id, pck2.Id);
                        if (oddEvenSize)
                        {
                            //Add resources
                            var res = GetRandomItem(PackageResources.Where(t => t.PackageId == pck5.Id));
                            GetOrCreateAssignmentResource(reviAss.Id, res.ResourceId);
                        }
                    }
                }

                var regnskap = GetRandomItem(orgs.First(t => t.Key == "Regnskap").Value.Where(t => t.Id != entity.Id));
                var regnAss = await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{entity.Id}:assign:regn:" + a), FromId = entity.Id, ToId = regnskap.Id, RoleId = regn.Id }, true);
                assignments.Add(regnAss);

                if (oddEvenName)
                {
                    // Add package
                    GetOrCreateAssignmentPackage(regnAss.Id, pck1.Id);
                    GetOrCreateAssignmentPackage(regnAss.Id, pck2.Id);
                    if (oddEvenSize)
                    {
                        //Add resources
                        var res1 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck3.Id));
                        GetOrCreateAssignmentResource(regnAss.Id, res1.ResourceId);
                        var res2 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck4.Id));
                        GetOrCreateAssignmentResource(regnAss.Id, res2.ResourceId);
                    }
                }
                else
                {
                    // Add package
                    GetOrCreateAssignmentPackage(regnAss.Id, pck3.Id);
                    GetOrCreateAssignmentPackage(regnAss.Id, pck4.Id);
                    if (oddEvenSize)
                    {
                        //Add resources
                        var res1 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck1.Id));
                        GetOrCreateAssignmentResource(regnAss.Id, res1.ResourceId);
                        var res2 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck2.Id));
                        GetOrCreateAssignmentResource(regnAss.Id, res2.ResourceId);
                    }
                }
            }
        }
        #endregion

        /*Regnskapsfører Delegations*/
        foreach (var assignment in assignments.Where(t => t.RoleId == regn.Id))
        {
            /* Om From har mange ansatte og To har mange ansatte => Sett flere delegations */
            var fromSize = assignments.Count(t => t.FromId == assignment.FromId && t.RoleId == ansatt.Id);
            var toSize = assignments.Count(t => t.FromId == assignment.ToId && t.RoleId == ansatt.Id);
            var diff = Math.Abs(fromSize - toSize);

            if (fromSize >= toSize)
            {
                // Firma er større eller like stort som regnskapsfirma => 1 + Diff Delegation
                //var entity = GetRandomItem(orgs["Regnskap"]);

                for (int ra = 0; ra < 1 + diff; ra++)
                {
                    if (assignments.Count(t => t.FromId == assignment.ToId && t.RoleId == ansatt.Id) == 0)
                    {
                        // TODO: Never ?? 
                        Console.WriteLine($"Employee not found for assignment.ToId:'{assignment.ToId}'");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"Employee found for assignment.ToId:'{assignment.ToId}'!");
                    }
                    var entityAnsatt = GetRandomItem(assignments.Where(t => t.FromId == assignment.ToId && t.RoleId == ansatt.Id));
                    var delegation = new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{entityAnsatt.Id}"), FromId = assignment.Id, ToId = entityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId };
                    GetOrCreateDelegation(delegation);

                    /*Mulige ressurser å delegere, man delegerer ikke hele pakker ... */
                    var rolePacks = RolePackages.Where(t => t.RoleId == assignment.RoleId);
                    var assPacks = AssignmentPackages.Where(t => t.AssignmentId == assignment.Id);

                    var packResources = new List<PackageResource>();
                    foreach(var rPack in rolePacks)
                    {
                        packResources.AddRange(PackageResources.Where(t => t.PackageId == rPack.PackageId));
                    }
                    foreach (var aPack in assPacks)
                    {
                        packResources.AddRange(PackageResources.Where(t => t.PackageId == aPack.PackageId));
                    }
                    var roleResources = RoleResources.Where(t => t.RoleId == assignment.RoleId);
                    var assResources = AssignmentResources.Where(t => t.AssignmentId == assignment.Id);
                    

                    /*Finn overlappende pakker*/
                    foreach (var rolePack in rolePacks)
                    {
                        if (assPacks.Count(t => t.PackageId == rolePack.PackageId) > 0)
                        {
                            //GetOrCreateDelegationPackage(new DelegationPackage() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RolePackageId = rolePack.Id, AssignmentPackageId = assPacks.First(t => t.PackageId == rolePack.PackageId).Id });
                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            //GetOrCreateDelegationPackage(new DelegationPackage() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RolePackageId = rolePack.Id });
                        }
                    }
                    /*Tilfeldig Assignment Package*/
                    foreach (var assPack in assPacks)
                    {
                        if (rolePacks.Count(t => t.PackageId == assPack.PackageId) > 0)
                        {
                            // Added in prev loop
                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            ////GetOrCreateDelegationPackage(new DelegationPackage() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, AssignmentPackageId = assPack.Id });
                            continue;
                        }
                    }
                    /*Finn overlappende ressurser*/
                    foreach (var roleRes in roleResources)
                    {
                        if (assResources.Count(t => t.ResourceId == roleRes.ResourceId) > 0)
                        {
                            if (true)
                            {
                                //pacres...
                            }
                            else
                            {
                                //GetOrCreateDelegationResource(new DelegationResource() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RoleResourceId = roleRes.Id, AssignmentResourceId =  assResources.First(t => t.ResourceId == roleRes.ResourceId).Id });
                            }
                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            //resourcesToDelegate.Add(roleRes.ResourceId);
                        }
                    }
                    /*Tilfeldig Assignment Resources*/
                    foreach (var assRes in assResources)
                    {
                        if (roleResources.Count(t => t.ResourceId == assRes.ResourceId) > 0)
                        {
                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            //resourcesToDelegate.Add(assRes.ResourceId);
                            continue;
                        }
                    }

                    var pckId1 = GetRandomItem(rolePacks.Select(t => t.PackageId));
                    var pckId2 = GetRandomItem(assPacks.Select(t => t.PackageId));
                    var resId1 = GetRandomItem(roleResources.Select(t => t.ResourceId));
                    var resId2 = GetRandomItem(assResources.Select(t => t.ResourceId));
                }
            }
            else
            {
                // Firma er mindre enn regnskapsfirma => 1 Delegation
                var entity = GetRandomItem(orgs["Regnskap"]);
                var entityAnsatt = GetRandomItem(assignments.Where(t => t.FromId == entity.Id && t.RoleId == ansatt.Id));

                GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{entityAnsatt.Id}"), FromId = assignment.Id, ToId = entityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });
                /*
                Delegate Packages and Resources
                */

                /*
                Delegate Packages and Resources

                Find Packages and Resources on Assignment (RolePackage, RoleResource, AssignmentPackage, AssignmentResource)
                Delegate some of them... If RolePack and AssPack ref same Pack. Add!
                */
            }
        }






        /*Revisor Delegations*/
        foreach (var assignment in assignments.Where(t => t.RoleId == revi.Id))
        {
            /* Om From har mange ansatte og To har mange ansatte => Sett flere delegations */
            var fromSize = assignments.Count(t => t.FromId == assignment.FromId && t.RoleId == ansatt.Id);
            var toSize = assignments.Count(t => t.FromId == assignment.ToId && t.RoleId == ansatt.Id);
            var diff = Math.Abs(fromSize - toSize);

            if (fromSize >= toSize)
            {
                // Firma er større eller like stort som revisjonsfirma => 1 + Diff Delegation
                var entity = GetRandomItem(orgs["Revisjon"]);

                for (int ra = 0; ra < 1 + diff; ra++)
                {
                    var enityAnsatt = GetRandomItem(assignments.Where(t => t.FromId == entity.Id && t.RoleId == ansatt.Id));
                    GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{enityAnsatt.Id}"), FromId = assignment.Id, ToId = enityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });
                    /*
                    Delegate Packages and Resources
                    */
                }
            }
            else
            {
                // Firma er mindre enn revisjonsfirma => 1 Delegation
                var entity = GetRandomItem(orgs["Regnskap"]);
                var entityAnsatt = GetRandomItem(assignments.Where(t => t.FromId == entity.Id && t.RoleId == ansatt.Id));

                GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{entityAnsatt.Id}"), FromId = assignment.Id, ToId = entityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });
                /*
                Delegate Packages and Resources
                */
            }
        }
    }


    private Resource GetResource(Package package, int no)
    {
        return Resources.First(t => t.Name == GenerateResourceName(package, "Skjema", no));
    }

    private string GenerateResourceName(Package package, string type, int no)
    {
        return package.Name + " - " + type + " " + no;
    }

    private async Task GeneratePackageResources(Package package, Dictionary<int, Guid> randomRoleMap)
    {
        var provider = GetProvider("Digdir");
        var rg = await GetOrCreateResourceGroup("Default", provider.Id);
        var rt = await GetOrCreateResourceType("Form");

        for (int i = 0; i < package.Name.Length / 2; i++)
        {
            string newName = GenerateResourceName(package, "Skjema", i);

            var r = await GetOrCreateResource(new Resource()
            {
                Id = Guid.NewGuid(),
                Name = newName,
                Description = package.Description + " og det samme for denne ressursen",
                RefId = package.Urn + newName.ToLower().Replace(" ", string.Empty),
                ProviderId = provider.Id,
                GroupId = rg.Id,
                TypeId = rt.Id
            });

            await GetOrCreatePackageResource(package.Id, r.Id);
            if (randomRoleMap.ContainsKey(i))
            {
                await GetOrCreateRoleResource(randomRoleMap[i], r.Id);
            }
        }
    }

    private Entity GenerateOrgEntity(Guid id, string industry = "")
    {
        var comp = MockTools.GenerateCompanyName(industry);
        var type = GetEntityType("Organisasjon");
        var variant = GetEntityVariant("Organisasjon", "AS");
        return new Entity { Id = id, Name = comp, RefId = MockTools.GenerateOrganizationNumber(), TypeId = type.Id, VariantId = variant.Id };
    }

    private Entity GeneratePersonEntity(Guid id, string overrideConcept = "")
    {
        var pers = MockTools.GeneratePerson();
        var type = GetEntityType("Person");
        var variant = GetEntityVariant("Person", "Person");
        return new Entity { Id = id, Name = pers.FirstName + " " + pers.LastName, RefId = pers.BirthDate.ToString("yyyy-MM-dd"), TypeId = type.Id, VariantId = variant.Id };
    }

}

