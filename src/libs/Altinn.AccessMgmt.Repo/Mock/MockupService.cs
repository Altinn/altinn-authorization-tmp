using System.Data;
using System.Security.Cryptography;
using System.Text;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo.Data.Contracts;
//// using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Repo.Mock;

/// <summary>
/// Generate mockup data
/// </summary>
public class MockupService
{
    #region Constructor
    private readonly ILogger<MockupService> logger;
    //// private readonly IAltinnLease lease;
    private readonly DbAccessConfig config;
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
    private readonly IPolicyElementService policyElementService;
    private readonly IElementTypeService elementTypeService;
    private readonly IElementService elementService;
    private readonly IRoleService roleService;
    private readonly IAssignmentService assignmentService;
    private readonly IGroupService groupService;
    private readonly IGroupAdminService groupAdminService;
    private readonly IGroupMemberService groupMemberService;
    private readonly IDelegationService delegationService;

    /// <summary>
    /// MockupService
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <param name="configOptions">DbAccessConfig</param>
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
    /// <param name="policyElementService">IPolicyElementService</param>
    /// <param name="elementTypeService">IElementTypeService</param>
    /// <param name="elementService">IElementService</param>
    /// <param name="roleService">IRoleService</param>
    /// <param name="assignmentService">IAssignmentService</param>
    /// <param name="groupService">IGroupService</param>
    /// <param name="groupAdminService">IGroupAdminService</param>
    /// <param name="groupMemberService">IGroupMemberService</param>
    /// <param name="delegationService">IDelegationService</param>
    public MockupService(
        ILogger<MockupService> logger,
        IOptions<DbAccessConfig> configOptions,
        //// IAltinnLease lease,
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
        IPolicyElementService policyElementService,
        IElementTypeService elementTypeService,
        IElementService elementService,
        IRoleService roleService,
        IAssignmentService assignmentService,
        IGroupService groupService,
        IGroupAdminService groupAdminService,
        IGroupMemberService groupMemberService,
        IDelegationService delegationService
        )
    {
        this.logger = logger;
        //// this.lease = lease;
        config = configOptions.Value;

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
        this.policyElementService = policyElementService;
        this.elementTypeService = elementTypeService;
        this.elementService = elementService;
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

    private List<PolicyElement> PolicyElements { get; set; }

    private List<ElementType> ElementTypes { get; set; }

    private List<Element> Elements { get; set; }

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
        PolicyElements = [.. await policyElementService.Get()];
        ElementTypes = [.. await elementTypeService.Get()];
        Elements = [.. await elementService.Get()];

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

    private async Task<ResourceGroup> GetOrCreateResourceGroup(ResourceGroup obj)
    {
        var res = ResourceGroups.FirstOrDefault(t => t.Name == obj.Name && t.ProviderId == obj.ProviderId) ?? null;
        if (res == null)
        {
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

    private async Task<PolicyElement> GetOrCreatePolicyElement(PolicyElement obj)
    {
        var res = PolicyElements.FirstOrDefault(t => t.PolicyId == obj.PolicyId && t.ElementId == obj.ElementId) ?? null;
        if (res == null)
        {
            await policyElementService.Create(obj);
            PolicyElements.Add(obj);
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

    private bool CheckRunConfig(string key)
    {
        return config.MockRun.ContainsKey(key) && config.MockRun[key];
    }

    /// <summary>
    /// Run mockup
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task Run(CancellationToken cancellationToken = default)
    {
        if (config.MockEnabled)
        {
            /*
            await using var ls = await lease.TryAquireNonBlocking<LeaseContent>("access_management_db_mock", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }
            */

            if (CheckRunConfig("KlientDelegering"))
            {
                await KlientDelegeringMock();
            }

            if (CheckRunConfig("SystemResources"))
            {
                await SystemResources();
            }
        }
    }

    /// <summary>
    /// Add data for Klientdelegering mock
    /// </summary>
    /// <returns></returns>
    private async Task KlientDelegeringMock()
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

        /*
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
        */

        #endregion

        logger.LogInformation("Mock - KlientDelegering - Complete");
    }

    /// <summary>
    /// En person vil dele en ressurs med en annen person
    /// </summary>
    /// <returns></returns>
    private async Task PersonToPersonDirectAssignment()
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
    private async Task PersonToOrganizationDirectAssignment()
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

        var avtaleRole = await GetOrCreateRole(new Role()
        {
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
        var gunnar = await GetOrCreateEntity(new Entity() { Id = Guid.Parse("A3CF140E-98A9-4AF6-8B3A-0C356A3C71F9"), Name = "Gunnar", RefId = "P2O-3", TypeId = personEntityType.Id, VariantId = personEntityVariant.Id });
        var ansattRole = await GetOrCreateRole(new Role() { Id = Guid.Parse("7F10C119-63EA-42B3-84C6-0CB1147C85C0"), Code = "ANSATT", Name = "Ansatt", Description = "En person er ansatt i et firma", Urn = "digdir:avtaleRole:organisasjon:ansatt", EntityTypeId = orgEntityType.Id, ProviderId = provider.Id });
        var gunnarAnsattAwesome = await GetOrCreateAssignment(new Assignment() { Id = Guid.Parse("6201EEFA-EE00-4712-A172-0E31DADC60AE"), FromId = awesomeFolk.Id, ToId = gunnar.Id, RoleId = ansattRole.Id, IsDelegable = false });

        // Vi kan nå opprette en delegering i Awesome Folk AS til Gunnar
        // var delegation = await GetOrCreateDelegation(new Delegation() { Id = Guid.Parse("7E8036B4-43AB-4535-9BA4-0FE745C50F79"), AssignmentId = assignment.Id });
        
        // var delegationResource = await GetOrCreateDelegationResource(new DelegationResource());
        // var delegateGunnar = await GetOrCreateDelegationAssignment(new DelegationAssignment() { Id = Guid.Parse("50F2C1F2-CEEB-4CEA-9FE0-138E3FEE6FFD"), DelegationId = delegation.Id, AssignmentId = gunnarAnsattAwesome.Id });
    }

    /// <summary>
    /// Mock for SystemResources - A3 Rights
    /// </summary>
    /// <returns></returns>
    private async Task SystemResources()
    {
        logger.LogInformation("Mock - SystemResources");
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
        var rt = await GetOrCreateResourceType("System");
        var a3Rg = await GetOrCreateResourceGroup("Altinn 3", digdir.Id);

        var elementType = await GetOrCreateElementType(new ElementType()
        {
            Id = Guid.Parse("A8023469-CAC3-49EF-9894-65560B09179A"),
            Name = "SubResource"
        });

        // ResourceGroup
        var accessmgmt = await GetOrCreateResourceGroup(new ResourceGroup()
        {
            Id = Guid.Parse("06CB9503-D35B-4731-8152-14FA87811B64"),
            ProviderId = digdir.Id,
            Name = "AccessMgmt",
            Description = "Tilgangsstyring i Altinn 3"
        });

        #region Group Mgmt
        var groupResource = await GetOrCreateResource(new Resource()
        {
            Id = Guid.Parse("D0B49E00-D938-4219-8BB4-15DAB9D5055A"),
            Name = "Group Management",
            Description = "",
            RefId = "digdir:a3:accessmgmt:groups",
            GroupId = accessmgmt.Id,
            TypeId = rt.Id,
            ProviderId = digdir.Id
        });

        var grpCreate = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("6B689317-0D7A-4176-B6BD-17188CDBDEBF"),
            Name = "Opprett gruppe",
            //// Description = "Gir muligheten til å opprette grupper",
            Urn = "digdir:a3:accessmgmt:groups:create",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });

        var grpDelete = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("4C6464C7-BF74-4877-9DBD-1866B8A48F17"),
            Name = "Slett gruppe",
            //// Description = "Gir muligheten til å slette grupper",
            Urn = "digdir:a3:accessmgmt:groups:delete",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });

        var grpAddAdmin = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("DD124895-522E-45BF-8E87-195709C60EA1"),
            Name = "Legg til gruppe administrator",
            //// Description = "Gir muligheten til å legge til administratorer i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:add-user",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });

        var grpRemoveAdmin = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("3DB6D752-F262-4987-B3A0-1C75AE29E3EA"),
            Name = "Fjern gruppe administrator",
            //// Description = "Gir muligheten til å fjerne administratorer i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:remove-user",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });

        var grpAddUser = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("338709E9-102C-40AE-BA20-1F9E245851D9"),
            Name = "Legg til bruker",
            //// Description = "Gir muligheten til å legge til brukere i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:add-user",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });

        var grpRemoveUser = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("A893D35A-4BE5-48F6-A82A-1FFE2A5C3291"),
            Name = "Fjern bruker",
            //// Description = "Gir muligheten til å fjerne brukere i en gruppe",
            Urn = "digdir:a3:accessmgmt:groups:remove-user",
            ResourceId = groupResource.Id,
            TypeId = elementType.Id
        });
        #endregion

        #region Assignment Mgmt
        var assignmentResource = await GetOrCreateResource(new Resource()
        {
            Id = Guid.Parse("E45A571C-144F-4B98-BAE5-21873FB6F310"),
            Name = "Assignment Mgmt",
            Description = "",
            GroupId = accessmgmt.Id,
            ProviderId = digdir.Id,
            RefId = "digdir:a3:accessmgmt:assignment",
            TypeId = rt.Id
        });

        var compAssCreateAny = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("8CA328C5-4F69-43ED-851B-2246945EEEE0"),
            ResourceId = assignmentResource.Id,
            Name = "Opprett tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å opprette tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:"
        });

        var compAssDeleteAny = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("365C851C-9FCC-4952-8CE8-22AA6757F7C2"),
            ResourceId = assignmentResource.Id,
            Name = "Fjern tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:"
        });

        var compDelegatePack = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("20C83E97-FE3D-4531-A17B-258603DD5839"),
            ResourceId = assignmentResource.Id,
            Name = "Gi pakke til tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å gi pakker på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:delgate-package"
        });

        var compRevokePack = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("91508C64-494B-462E-8774-25CFCDEAA40E"),
            ResourceId = assignmentResource.Id,
            Name = "Fjern pakke til tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne pakker på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:revoke-package"
        });

        var compDelegateResource = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("7D4DEF93-706E-4A76-A487-260B272F275C"),
            ResourceId = assignmentResource.Id,
            Name = "Gi ressurs til tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å gi tjenester på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:delgate-resource"
        });

        var compRevokeresource = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("8E751971-69E9-4766-B42A-2710EDB1480B"),
            ResourceId = assignmentResource.Id,
            Name = "Fjern ressurs til tildeling",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne tjenester på tildelinger",
            Urn = "digdir:a3:accessmgmt:assignment:revoke-resource"
        });

        #endregion

        #region Delegation Mgmt
        var delegationResource = await GetOrCreateResource(new Resource()
        {
            Id = Guid.Parse("F4716E33-C740-40CD-8C4D-29786A4E609A"),
            GroupId = accessmgmt.Id,
            Description = "",
            ProviderId = digdir.Id,
            Name = "Delegation Mgmt",
            RefId = "digdir:a3:accessmgmt:delegation",
            TypeId = rt.Id
        });

        var compDelegCreateAny = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("10624B1F-1021-4ED4-96D4-2C11C2CBA398"),
            ResourceId = delegationResource.Id,
            Name = "Opprett delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å opprette delegeringer",
            Urn = "digdir:a3:accessmgmt:delegation:create"
        });

        var compDelegDeleteAny = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("E8BC5401-38C0-4CA8-BF8A-31BFEA56265E"),
            ResourceId = delegationResource.Id,
            Name = "Fjern delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne delegeringer",
            Urn = "digdir:a3:accessmgmt:delegation:delete"
        });

        var compDelegPackAdd = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("187C7524-4D45-4194-9135-3208B9E6E5EA"),
            ResourceId = delegationResource.Id,
            Name = "Gi pakke til delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å gi pakker på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:delgate-package"
        });

        var compDelegPackRemove = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("169D2D6F-9AC8-4B70-BD8E-371DF6AB6B56"),
            ResourceId = delegationResource.Id,
            Name = "Fjern pakke fra delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne pakker på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:revoke-package"
        });

        var compDelegResourceAdd = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("9D0C7368-7348-492F-856B-3811217D1F54"),
            ResourceId = delegationResource.Id,
            Name = "Gi ressurs til delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å gi tjenester på delegering",
            Urn = "digdir:a3:accessmgmt:delegation:delgate-resource"
        });

        var compDelegResourceRemove = await GetOrCreateElement(new Element()
        {
            Id = Guid.Parse("27F84AD1-7438-4BC4-B016-3939622097D3"),
            ResourceId = delegationResource.Id,
            Name = "Fjern ressurs fra delegering",
            TypeId = elementType.Id,
            //// Description = "Gir tilgang til å fjerne tjenester på delegering",
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
        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("A506763C-175B-43B8-AC60-3D918BAA4A00"),
            PolicyId = adminPolicy.Id,
            ElementId = grpCreate.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("C641CD4D-ADAB-4B1F-B40F-48001150B38C"),
            PolicyId = adminPolicy.Id,
            ElementId = grpDelete.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("4E53BBBB-F62E-4EEE-B371-4BC6AE59A923"),
            PolicyId = adminPolicy.Id,
            ElementId = grpAddAdmin.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("EE7E229D-C0AB-4FCB-B8A9-4D5AEC04599C"),
            PolicyId = adminPolicy.Id,
            ElementId = grpRemoveAdmin.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("23698B11-53B4-41CB-9D43-5104953042B6"),
            PolicyId = adminPolicy.Id,
            ElementId = grpAddUser.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("4AC74DEE-51E6-4ACC-AF42-517DEF59AEAF"),
            PolicyId = adminPolicy.Id,
            ElementId = grpRemoveUser.Id
        });
        #endregion

        #region Policy - Assignment
        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("A9CC1667-9E74-419B-9AE9-51C7A4E0985F"),
            PolicyId = adminPolicy.Id,
            ElementId = compAssCreateAny.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("3A006E6A-D1E1-4941-B27C-56FB099E71AB"),
            PolicyId = adminPolicy.Id,
            ElementId = compAssDeleteAny.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("CECB498B-F448-4896-B701-58E0CFC0919E"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegatePack.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("5999F9F2-B5E2-4061-A4E6-5CACB776C0B7"),
            PolicyId = adminPolicy.Id,
            ElementId = compRevokePack.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("C406F4D9-9CA6-4167-9809-5E0306625AFB"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegateResource.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("1BA43739-506E-4253-9B6C-5FDBAC077419"),
            PolicyId = adminPolicy.Id,
            ElementId = compRevokeresource.Id
        });
        #endregion

        #region Policy - Delegation
        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("31ADC81A-723E-4CDD-9E4D-60BEBCF2A81D"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegCreateAny.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("F823F0CD-54D9-4A52-81B0-60D9F5156E69"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegDeleteAny.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("94CBD1E8-4865-468C-9A4A-61387E5419A0"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegPackAdd.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("8A5079A1-838D-4B30-9E20-62727F70D37E"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegPackRemove.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("B2E41362-6A81-45F3-8425-6350DFEF054A"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegResourceAdd.Id
        });

        await GetOrCreatePolicyElement(new PolicyElement()
        {
            Id = Guid.Parse("A19AA11F-2389-4913-9C43-64A59B7A506E"),
            PolicyId = adminPolicy.Id,
            ElementId = compDelegResourceRemove.Id
        });
        #endregion
    }

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

    private static T GetRandomItem<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("List is empty");
        }

        int index = MockTools.Rnd.Next(list.Count);
        return list[index];
    }

    private static T GetRandomItem<T>(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        int count = collection.Count();
        if (count == 0)
        {
            throw new ArgumentException("List is empty");
        }

        int randomIndex = MockTools.Rnd.Next(count);
        return collection.ElementAt(randomIndex);
    }

    private async Task BasicMock()
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

        var specs = new List<(string Industry, int Antall, int Ansatte)>()
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
            orgs.Add(spec.Industry, new List<Entity>());
            int medlCount = (int)Math.Round(10.0 / 100 * spec.Ansatte, MidpointRounding.AwayFromZero);

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

            for (int j = 0; j < spec.Antall; j++)
            {
                string orgKey = $"entity:org:{i}:{j}";
                var comp = await GetOrCreateEntity(GenerateOrgEntity(GenerateFakeGuid(orgKey), spec.Industry), true);
                orgs[spec.Industry].Add(comp);

                int employeeCount = spec.Ansatte;
                int percentEmployeeAdjust = comp.Name.Count(c => c == 'e') + 1;
                if ((comp.Name.Length & 1) == 1)
                {
                    // Remove 2%
                    employeeCount -= employeeCount * (1 + (percentEmployeeAdjust / 100));
                }
                else
                {
                    // Add 2%
                    employeeCount += employeeCount * (1 + (percentEmployeeAdjust / 100));
                }

                /*PEOPLE + ASSIGNMENTS*/
                for (int k = 0; k < spec.Ansatte; k++)
                {
                    var personKey = $"{orgKey}:person:{k}";
                    var e = await GetOrCreateEntity(GeneratePersonEntity(GenerateFakeGuid(personKey)), true);

                    people.Add(e);

                    await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:ansatt" + k), FromId = comp.Id, ToId = e.Id, RoleId = ansatt.Id }, true);

                    if (employeeCount <= 10)
                    {
                        if (k == 0)
                        {
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:dagl" + k), FromId = comp.Id, ToId = e.Id, RoleId = dagl.Id }, true);
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:lede" + k), FromId = comp.Id, ToId = e.Id, RoleId = lede.Id }, true);
                        }
                    }
                    else
                    {
                        if (k == 0)
                        {
                            await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{personKey}:assign:dagl" + k), FromId = comp.Id, ToId = e.Id, RoleId = dagl.Id }, true);
                        }

                        if (k == 1)
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
                        await GetOrCreateAssignmentPackage(reviAss.Id, pck5.Id);
                        if (oddEvenSize)
                        {
                            // Add resources
                            var res = GetRandomItem(PackageResources.Where(t => t.PackageId == pck2.Id));
                            await GetOrCreateAssignmentResource(reviAss.Id, res.ResourceId);
                        }
                    }
                    else
                    {
                        await GetOrCreateAssignmentPackage(reviAss.Id, pck2.Id);
                        if (oddEvenSize)
                        {
                            // Add resources
                            var res = GetRandomItem(PackageResources.Where(t => t.PackageId == pck5.Id));
                            await GetOrCreateAssignmentResource(reviAss.Id, res.ResourceId);
                        }
                    }
                }

                var regnskap = GetRandomItem(orgs.First(t => t.Key == "Regnskap").Value.Where(t => t.Id != entity.Id));
                var regnAss = await GetOrCreateAssignment(new Assignment() { Id = GenerateFakeGuid($"{entity.Id}:assign:regn:" + a), FromId = entity.Id, ToId = regnskap.Id, RoleId = regn.Id }, true);
                assignments.Add(regnAss);

                if (oddEvenName)
                {
                    // Add package
                    await GetOrCreateAssignmentPackage(regnAss.Id, pck1.Id);
                    await GetOrCreateAssignmentPackage(regnAss.Id, pck2.Id);
                    if (oddEvenSize)
                    {
                        // Add resources
                        var res1 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck3.Id));
                        await GetOrCreateAssignmentResource(regnAss.Id, res1.ResourceId);
                        var res2 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck4.Id));
                        await GetOrCreateAssignmentResource(regnAss.Id, res2.ResourceId);
                    }
                }
                else
                {
                    // Add package
                    await GetOrCreateAssignmentPackage(regnAss.Id, pck3.Id);
                    await GetOrCreateAssignmentPackage(regnAss.Id, pck4.Id);
                    if (oddEvenSize)
                    {
                        // Add resources
                        var res1 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck1.Id));
                        await GetOrCreateAssignmentResource(regnAss.Id, res1.ResourceId);
                        var res2 = GetRandomItem(PackageResources.Where(t => t.PackageId == pck2.Id));
                        await GetOrCreateAssignmentResource(regnAss.Id, res2.ResourceId);
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
                //// var entity = GetRandomItem(orgs["Regnskap"]);

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
                    await GetOrCreateDelegation(delegation);

                    /*Mulige ressurser å delegere, man delegerer ikke hele pakker ... */
                    var rolePacks = RolePackages.Where(t => t.RoleId == assignment.RoleId);
                    var assPacks = AssignmentPackages.Where(t => t.AssignmentId == assignment.Id);

                    var packResources = new List<PackageResource>();
                    foreach (var rolePack in rolePacks)
                    {
                        packResources.AddRange(PackageResources.Where(t => t.PackageId == rolePack.PackageId));
                    }

                    foreach (var assPack in assPacks)
                    {
                        packResources.AddRange(PackageResources.Where(t => t.PackageId == assPack.PackageId));
                    }

                    var roleResources = RoleResources.Where(t => t.RoleId == assignment.RoleId);
                    var assResources = AssignmentResources.Where(t => t.AssignmentId == assignment.Id);

                    /*Finn overlappende pakker*/
                    foreach (var rolePack in rolePacks)
                    {
                        if (assPacks.Count(t => t.PackageId == rolePack.PackageId) > 0)
                        {
                            // GetOrCreateDelegationPackage(new DelegationPackage() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RolePackageId = rolePack.Id, AssignmentPackageId = assPacks.First(t => t.PackageId == rolePack.PackageId).Id });
                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            // GetOrCreateDelegationPackage(new DelegationPackage() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RolePackageId = rolePack.Id });
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
                                // pacres...
                            }
                            else
                            {
                                // GetOrCreateDelegationResource(new DelegationResource() { Id = GenerateFakeGuid(""), DelegationId = delegation.Id, RoleResourceId = roleRes.Id, AssignmentResourceId =  assResources.First(t => t.ResourceId == roleRes.ResourceId).Id });
                            }

                            continue;
                        }

                        if (diff % 1 == 1)
                        {
                            // resourcesToDelegate.Add(roleRes.ResourceId);
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
                            // resourcesToDelegate.Add(assRes.ResourceId);
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

                await GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{entityAnsatt.Id}"), FromId = assignment.Id, ToId = entityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });

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
                    await GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{enityAnsatt.Id}"), FromId = assignment.Id, ToId = enityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });
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

                await GetOrCreateDelegation(new Delegation() { Id = GenerateFakeGuid($"delegation:{assignment.Id}:{entityAnsatt.Id}"), FromId = assignment.Id, ToId = entityAnsatt.Id, SourceId = assignment.FromId, ViaId = assignment.ToId });
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
