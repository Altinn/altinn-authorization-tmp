using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Migrate;

/// <summary>
/// Access Package Migration
/// </summary>
public class DatabaseMigration : IDatabaseMigration
{
    private const string ExtAssignmentView = """
CREATE VIEW dbo.extassignment AS
select id, roleid, fromid, toid, isdelegable
from dbo.assignment
union all
select distinct on (a.fromid, map.getroleid, a.toid) a.id, map.getroleid, a.fromid, a.toid, a.isdelegable
from dbo.assignment as a
inner join dbo.rolemap as map on a.roleid = map.hasroleid;
""";

    private readonly IDbMigrationFactory _factory;

    private bool Enable { get { return _factory.Enable; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="factory">IDbMigrationFactory</param>
    public DatabaseMigration(IDbMigrationFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task Init()
    {
        if (Enable)
        {
            await _factory.Init();
            await CreateSchema();

            await CreateWorkerConfig();

            await CreateProvider();
            await CreateArea();
            await CreateEntity();
            await CreateTag();
            await CreatePackage();
            await CreateRole();
            await CreateResource();

            await CreateAssignment();
            await CreateGroups();
            await CreateDelegations();
        }
    }

    private async Task CreateWorkerConfig()
    {
        await _factory.CreateTable<WorkerConfig>();
        await _factory.CreateColumn<WorkerConfig>(t => t.Key, DataTypes.String());
        await _factory.CreateColumn<WorkerConfig>(t => t.Value, DataTypes.StringMax);
        await _factory.CreateUniqueConstraint<WorkerConfig>([t => t.Key]);
    }

    private async Task CreateAssignment()
    {
        await _factory.UseHistory<Assignment>();
        await _factory.CreateTable<Assignment>();
        await _factory.CreateColumn<Assignment>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateColumn<Assignment>(t => t.FromId, DataTypes.Guid);
        await _factory.CreateColumn<Assignment>(t => t.ToId, DataTypes.Guid);
        await _factory.CreateColumn<Assignment>(t => t.IsDelegable, DataTypes.Bool, defaultValue: "false");
        await _factory.CreateForeignKeyConstraint<Assignment, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.FromId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.ToId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Assignment>([t => t.RoleId, t => t.ToId, t => t.FromId]);
        await _factory.AddHistory<Assignment>();

        await _factory.UseHistory<AssignmentPackage>();
        await _factory.CreateTable<AssignmentPackage>();
        await _factory.CreateColumn<AssignmentPackage>(t => t.AssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<AssignmentPackage>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<AssignmentPackage, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<AssignmentPackage, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<AssignmentPackage>([t => t.AssignmentId, t => t.PackageId]);
        await _factory.AddHistory<Assignment>();

        await _factory.UseHistory<AssignmentResource>();
        await _factory.CreateTable<AssignmentResource>();
        await _factory.CreateColumn<AssignmentResource>(t => t.AssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<AssignmentResource>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<AssignmentResource, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<AssignmentResource, Resource>(t => t.ResourceId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<AssignmentResource>([t => t.AssignmentId, t => t.ResourceId]);
        await _factory.AddHistory<AssignmentResource>();
    }

    private async Task CreateGroups()
    {
        await _factory.UseHistory<EntityGroup>();
        await _factory.CreateTable<EntityGroup>();
        await _factory.CreateColumn<EntityGroup>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<EntityGroup>(t => t.OwnerId, DataTypes.Guid);
        await _factory.CreateColumn<EntityGroup>(t => t.RequireRole, DataTypes.Bool);
        await _factory.CreateForeignKeyConstraint<EntityGroup, Entity>(t => t.OwnerId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityGroup>([t => t.OwnerId, t => t.Name]);
        await _factory.AddHistory<EntityGroup>();

        await _factory.UseHistory<GroupMember>();
        await _factory.CreateTable<GroupMember>();
        await _factory.CreateColumn<GroupMember>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<GroupMember>(t => t.MemberId, DataTypes.Guid);
        await _factory.CreateColumn<GroupMember>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateColumn<GroupMember>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateForeignKeyConstraint<GroupMember, EntityGroup>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<GroupMember, Entity>(t => t.MemberId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<GroupMember>([t => t.GroupId, t => t.MemberId]);
        await _factory.AddHistory<GroupMember>();

        await _factory.UseHistory<GroupAdmin>();
        await _factory.CreateTable<GroupAdmin>();
        await _factory.CreateColumn<GroupAdmin>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<GroupAdmin>(t => t.MemberId, DataTypes.Guid);
        await _factory.CreateColumn<GroupAdmin>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateColumn<GroupAdmin>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateForeignKeyConstraint<GroupAdmin, EntityGroup>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<GroupAdmin, Entity>(t => t.MemberId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<GroupAdmin>([t => t.GroupId, t => t.MemberId]);
        await _factory.AddHistory<GroupAdmin>();
    }

    private async Task CreateDelegations()
    {
        await _factory.UseHistory<Delegation>();
        await _factory.CreateTable<Delegation>();
        await _factory.CreateColumn<Delegation>(t => t.AssignmentId, dbType: DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Delegation, Assignment>(t => t.AssignmentId, t => t.Id, cascadeDelete: true);
        await _factory.AddHistory<Delegation>();

        await _factory.UseHistory<DelegationPackage>();
        await _factory.CreateTable<DelegationPackage>();
        await _factory.CreateColumn<DelegationPackage>(t => t.DelegationId, dbType: DataTypes.Guid);
        await _factory.CreateColumn<DelegationPackage>(t => t.PackageId, dbType: DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<DelegationPackage, Delegation>(t => t.DelegationId, t => t.Id, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<DelegationPackage, Package>(t => t.PackageId, t => t.Id, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<DelegationPackage>([t => t.DelegationId, t => t.PackageId]);
        await _factory.AddHistory<DelegationPackage>();

        await _factory.UseHistory<DelegationGroup>();
        await _factory.CreateTable<DelegationGroup>();
        await _factory.CreateColumn<DelegationGroup>(t => t.DelegationId, dbType: DataTypes.Guid);
        await _factory.CreateColumn<DelegationGroup>(t => t.GroupId, dbType: DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<DelegationGroup, Delegation>(t => t.DelegationId, t => t.Id, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<DelegationGroup, EntityGroup>(t => t.GroupId, t => t.Id, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<DelegationGroup>([t => t.DelegationId, t => t.GroupId]);
        await _factory.AddHistory<DelegationGroup>();

        await _factory.UseHistory<DelegationAssignment>();
        await _factory.CreateTable<DelegationAssignment>();
        await _factory.CreateColumn<DelegationAssignment>(t => t.DelegationId, dbType: DataTypes.Guid);
        await _factory.CreateColumn<DelegationAssignment>(t => t.AssignmentId, dbType: DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<DelegationAssignment, Delegation>(t => t.DelegationId, t => t.Id, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<DelegationAssignment, Assignment>(t => t.AssignmentId, t => t.Id, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<DelegationAssignment>([t => t.DelegationId, t => t.AssignmentId]);
        await _factory.AddHistory<DelegationAssignment>();
    }

    private async Task CreateSchema()
    {
        await _factory.CreateSchema("translation");
        await _factory.CreateSchema("dbo_history");
        await _factory.CreateSchema("translation_history");
    }

    private async Task CreateProvider()
    {
        await _factory.UseHistory<Provider>();
        await _factory.CreateTable<Provider>();
        await _factory.CreateColumn<Provider>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<Provider>(t => t.RefId, DataTypes.String(15), nullable: true);
        await _factory.CreateUniqueConstraint<Provider>([t => t.Name]);
        await _factory.AddHistory<Provider>();
    }

    private async Task CreateEntity()
    {
        await _factory.UseHistory<EntityType>();
        await _factory.CreateTable<EntityType>(useTranslation: true);
        await _factory.CreateColumn<EntityType>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<EntityType>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityType, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityType>([t => t.ProviderId, t => t.Name]);
        await _factory.AddHistory<EntityType>();

        await _factory.UseHistory<EntityVariant>();
        await _factory.CreateTable<EntityVariant>(useTranslation: true);
        await _factory.CreateColumn<EntityVariant>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<EntityVariant>(t => t.Description, DataTypes.String(150));
        await _factory.CreateColumn<EntityVariant>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityVariant, EntityType>(t => t.TypeId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityVariant>([t => t.TypeId, t => t.Name]);
        await _factory.AddHistory<EntityVariant>();

        await _factory.UseHistory<Entity>();
        await _factory.CreateTable<Entity>();
        await _factory.CreateColumn<Entity>(t => t.Name, DataTypes.String(250));
        await _factory.CreateColumn<Entity>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateColumn<Entity>(t => t.VariantId, DataTypes.Guid);
        await _factory.CreateColumn<Entity>(t => t.RefId, DataTypes.String(50));
        await _factory.CreateForeignKeyConstraint<Entity, EntityType>(t => t.TypeId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<Entity, EntityVariant>(t => t.VariantId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<Entity>([t => t.Name, t => t.TypeId, t => t.RefId]);
        await _factory.AddHistory<Entity>();
    }

    private async Task CreateArea()
    {
        await _factory.UseHistory<AreaGroup>();
        await _factory.CreateTable<AreaGroup>(useTranslation: true);
        await _factory.CreateColumn<AreaGroup>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<AreaGroup>(t => t.Description, DataTypes.String(750));
        await _factory.CreateColumn<AreaGroup>(t => t.EntityTypeId, DataTypes.Guid);
        await _factory.CreateColumn<AreaGroup>(t => t.Urn, DataTypes.String(75));
        await _factory.CreateUniqueConstraint<AreaGroup>([t => t.Name]);
        await _factory.AddHistory<AreaGroup>();

        await _factory.UseHistory<Area>();
        await _factory.CreateTable<Area>(useTranslation: true);
        await _factory.CreateColumn<Area>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<Area>(t => t.Description, DataTypes.String(750));
        await _factory.CreateColumn<Area>(t => t.IconName, DataTypes.String(50));
        await _factory.CreateColumn<Area>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<Area>(t => t.Urn, DataTypes.String(75));
        await _factory.CreateForeignKeyConstraint<Area, AreaGroup>(t => t.GroupId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<Area>([t => t.Name]);
        await _factory.AddHistory<Area>();
    }

    private async Task CreateTag()
    {
        await _factory.UseHistory<TagGroup>();
        await _factory.CreateTable<TagGroup>(useTranslation: true);
        await _factory.CreateColumn<TagGroup>(t => t.Name, DataTypes.String(100));
        await _factory.CreateUniqueConstraint<TagGroup>([t => t.Name]);
        await _factory.AddHistory<TagGroup>();

        await _factory.UseHistory<Tag>();
        await _factory.CreateTable<Tag>(useTranslation: true);
        await _factory.CreateColumn<Tag>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<Tag>(t => t.GroupId, DataTypes.Guid, nullable: true);
        await _factory.CreateColumn<Tag>(t => t.ParentId, DataTypes.Guid, nullable: true);
        await _factory.CreateForeignKeyConstraint<Tag, TagGroup>(t => t.GroupId);
        await _factory.CreateForeignKeyConstraint<Tag, Tag>(t => t.ParentId);
        await _factory.CreateUniqueConstraint<Tag>([t => t.Name]);
        await _factory.AddHistory<Tag>();
    }

    private async Task CreatePackage()
    {
        await _factory.UseHistory<Package>();
        await _factory.CreateTable<Package>(useTranslation: true);
        await _factory.CreateColumn<Package>(t => t.Name, DataTypes.String(100));
        await _factory.CreateColumn<Package>(t => t.Description, DataTypes.String(1500));
        await _factory.CreateColumn<Package>(t => t.IsDelegable, DataTypes.Bool);
        await _factory.CreateColumn<Package>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Package>(t => t.EntityTypeId, DataTypes.Guid);
        await _factory.CreateColumn<Package>(t => t.AreaId, DataTypes.Guid);
        await _factory.CreateColumn<Package>(t => t.HasResources, DataTypes.Bool, defaultValue: "true");
        await _factory.CreateColumn<Package>(t => t.Urn, DataTypes.String(75));
        await _factory.CreateForeignKeyConstraint<Package, Provider>(t => t.ProviderId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<Package, EntityType>(t => t.EntityTypeId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<Package, Area>(t => t.AreaId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<Package>([t => t.ProviderId, t => t.Name]);
        await _factory.AddHistory<Package>();

        await _factory.UseHistory<PackageTag>();
        await _factory.CreateTable<PackageTag>();
        await _factory.CreateColumn<PackageTag>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageTag>(t => t.TagId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<PackageTag, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageTag, Tag>(t => t.TagId, cascadeDelete: false); // ??
        await _factory.CreateUniqueConstraint<PackageTag>([t => t.PackageId, t => t.TagId]);
        await _factory.AddHistory<PackageTag>();

        await _factory.UseHistory<PackageDelegation>();
        await _factory.CreateTable<PackageDelegation>();
        await _factory.CreateColumn<PackageDelegation>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>(t => t.ForId, DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>(t => t.ToId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>(t => t.ForId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>(t => t.ToId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<PackageDelegation>([t => t.ForId, t => t.ToId, t => t.PackageId]);
        await _factory.AddHistory<PackageDelegation>();
    }

    private async Task CreateRole()
    {
        await _factory.UseHistory<Role>();
        await _factory.CreateTable<Role>(useTranslation: true);
        await _factory.CreateColumn<Role>(t => t.EntityTypeId, DataTypes.Guid);
        await _factory.CreateColumn<Role>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Role>(t => t.Name, DataTypes.String(100));
        await _factory.CreateColumn<Role>(t => t.Code, DataTypes.String(15));
        await _factory.CreateColumn<Role>(t => t.Description, DataTypes.String(1500));
        await _factory.CreateColumn<Role>(t => t.Urn, DataTypes.String(250));
        await _factory.CreateForeignKeyConstraint<Role, EntityType>(t => t.EntityTypeId, cascadeDelete: false); // ??
        await _factory.CreateForeignKeyConstraint<Role, Provider>(t => t.ProviderId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<Role>([t => t.Urn]);
        await _factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Name]);
        await _factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Code]);
        await _factory.AddHistory<Role>();

        await _factory.UseHistory<RolePackage>();
        await _factory.CreateTable<RolePackage>();
        await _factory.CreateColumn<RolePackage>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateColumn<RolePackage>(t => t.PackageId, DataTypes.Guid);
        await _factory.RemoveColumn<RolePackage>("IsActor");
        await _factory.RemoveColumn<RolePackage>("IsAdmin");
        await _factory.CreateColumn<RolePackage>(t => t.HasAccess, DataTypes.Bool, defaultValue: "false");
        await _factory.CreateColumn<RolePackage>(t => t.CanDelegate, DataTypes.Bool, defaultValue: "false");
        await _factory.CreateColumn<RolePackage>(t => t.EntityVariantId, DataTypes.Guid, nullable: true);
        await _factory.CreateForeignKeyConstraint<RolePackage, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<RolePackage, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<RolePackage, EntityVariant>(t => t.EntityVariantId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<RolePackage>([t => t.RoleId, t => t.PackageId, t => t.EntityVariantId]);
        await _factory.AddHistory<RolePackage>();

        await _factory.UseHistory<EntityVariantRole>();
        await _factory.CreateTable<EntityVariantRole>();
        await _factory.CreateColumn<EntityVariantRole>(t => t.VariantId, DataTypes.Guid);
        await _factory.CreateColumn<EntityVariantRole>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, EntityVariant>(t => t.VariantId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityVariantRole>([t => t.VariantId, t => t.RoleId]);
        await _factory.AddHistory<EntityVariantRole>();

        await _factory.UseHistory<RoleMap>();
        await _factory.CreateTable<RoleMap>();
        await _factory.CreateColumn<RoleMap>(t => t.HasRoleId, DataTypes.Guid);
        await _factory.CreateColumn<RoleMap>(t => t.GetRoleId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.HasRoleId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.GetRoleId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<RoleMap>([t => t.HasRoleId, t => t.GetRoleId]);
        await _factory.AddHistory<RoleMap>();
    }

    private async Task CreateResource()
    {
        await _factory.UseHistory<ResourceType>();
        await _factory.CreateTable<ResourceType>(useTranslation: true);
        await _factory.CreateColumn<ResourceType>(t => t.Name, DataTypes.String(50));
        await _factory.CreateUniqueConstraint<ResourceType>([t => t.Name]);
        await _factory.AddHistory<ResourceType>();

        await _factory.UseHistory<ResourceGroup>();
        await _factory.CreateTable<ResourceGroup>(useTranslation: true);
        await _factory.CreateColumn<ResourceGroup>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<ResourceGroup>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<ResourceGroup, Provider>(t => t.ProviderId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<ResourceGroup>([t => t.ProviderId, t => t.Name]);
        await _factory.AddHistory<ResourceGroup>();

        await _factory.UseHistory<Resource>();
        await _factory.CreateTable<Resource>(useTranslation: true);
        await _factory.CreateColumn<Resource>(t => t.Name, DataTypes.String(500));
        await _factory.CreateColumn<Resource>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.RefId, DataTypes.String(150));
        await _factory.CreateColumn<Resource>(t => t.Description, DataTypes.String(150), nullable: true);
        await _factory.CreateForeignKeyConstraint<Resource, Provider>(t => t.ProviderId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<Resource, ResourceType>(t => t.TypeId, cascadeDelete: false);
        await _factory.CreateForeignKeyConstraint<Resource, ResourceGroup>(t => t.GroupId, cascadeDelete: false);
        await _factory.CreateUniqueConstraint<Resource>([t => t.ProviderId, t => t.GroupId, t => t.RefId]);
        await _factory.AddHistory<Resource>();

        await _factory.UseHistory<PackageResource>();
        await _factory.CreateTable<PackageResource>();
        await _factory.CreateColumn<PackageResource>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>(t => t.Read, DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>(t => t.Write, DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>(t => t.Sign, DataTypes.Bool);
        await _factory.CreateForeignKeyConstraint<PackageResource, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageResource, Resource>(t => t.ResourceId, cascadeDelete: true);
        await _factory.AddHistory<PackageResource>();

        await _factory.UseHistory<ElementType>();
        await _factory.CreateTable<ElementType>();
        await _factory.CreateColumn<ElementType>(t => t.Name, DataTypes.String(75));
        await _factory.CreateUniqueConstraint<ElementType>([t => t.Name]);
        await _factory.AddHistory<ElementType>();

        await _factory.UseHistory<Element>();
        await _factory.CreateTable<Element>();
        await _factory.CreateColumn<Element>(t => t.Name, DataTypes.String(150));
        await _factory.CreateColumn<Element>(t => t.Urn, DataTypes.String(1000));
        await _factory.CreateColumn<Element>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateColumn<Element>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Element, ElementType>(t => t.TypeId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Element, Resource>(t => t.ResourceId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Element>([t => t.ResourceId, t => t.Name]);
        await _factory.AddHistory<Element>();

        await _factory.UseHistory<Component>();
        await _factory.CreateTable<Component>();
        await _factory.CreateColumn<Component>(t => t.Name, DataTypes.String(150));
        await _factory.CreateColumn<Component>(t => t.Description, DataTypes.String(500));
        await _factory.CreateColumn<Component>(t => t.Urn, DataTypes.String(1000));
        await _factory.CreateColumn<Component>(t => t.ElementId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Component, Element>(t => t.ElementId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Component>([t => t.ElementId, t => t.Name]);
        await _factory.AddHistory<Component>();

        await _factory.UseHistory<Policy>();
        await _factory.CreateTable<Policy>();
        await _factory.CreateColumn<Policy>(t => t.Name, DataTypes.String(150));
        await _factory.CreateColumn<Policy>(t => t.Description, DataTypes.String(500));
        await _factory.CreateColumn<Policy>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateUniqueConstraint<Policy>([t => t.ResourceId, t => t.Name]);
        await _factory.AddHistory<Policy>();

        await _factory.UseHistory<PolicyComponent>();
        await _factory.CreateTable<PolicyComponent>();
        await _factory.CreateColumn<PolicyComponent>(t => t.PolicyId, DataTypes.Guid);
        await _factory.CreateColumn<PolicyComponent>(t => t.ComponentId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<PolicyComponent, Policy>(t => t.PolicyId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PolicyComponent, Component>(t => t.ComponentId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<PolicyComponent>([t => t.PolicyId, t => t.ComponentId]);
        await _factory.AddHistory<PolicyComponent>();
    }
}
