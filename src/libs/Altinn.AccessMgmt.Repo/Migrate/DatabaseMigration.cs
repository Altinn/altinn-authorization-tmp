using Altinn;
using Altinn.AccessMgmt;
using Altinn.AccessMgmt.AccessPackages;
using Altinn.AccessMgmt.Repo;
using Altinn.AccessMgmt.Repo.Migrate;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo;
using Altinn.AccessMgmt.Repo.Migrate;
using Altinn.AccessMgmt.Repo.temp;
using Altinn.Authorization.Host.Lease;
using System.Threading;

namespace Altinn.AccessMgmt.Repo.Migrate;

/// <summary>
/// Access Package Migration
/// </summary>
public class DatabaseMigration
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
    private readonly IAltinnLease lease;
    private readonly IDbMigrationFactory factory;

    private bool Enable { get { return factory.Enable; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="factory">IDbMigrationFactory</param>
    public DatabaseMigration(IAltinnLease lease, IDbMigrationFactory factory)
    {
        this.lease=lease;
        this.factory = factory;
    }
    public class LeaseContent()
    {
        /// <summary>
        /// Last update date
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <inheritdoc/>
    public async Task Init(CancellationToken cancellationToken = default)
    {
        if (Enable)
        {
            await using var ls = await lease.TryAquireNonBlocking<LeaseContent>("access_management_db_migrate", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await factory.Init();
            await CreateSchema();

            await CreateWorkerConfig();

            await CreateProvider();
            await CreateEntity();
            await CreateArea();
            await CreateTag();
            await CreateResource();
            await CreatePackage();


            await CreateRole();

            await CreateAssignment();
            await CreateGroups();
            await CreateDelegations();

            await lease.Put(ls, new() { UpdatedAt = DateTimeOffset.Now }, cancellationToken);
            // await lease.RefreshLease(ls, cancellationToken);
        }
    }

    private async Task CreateWorkerConfig()
    {
        await factory.CreateTable<WorkerConfig>();
        await factory.CreateColumn<WorkerConfig>(t => t.Key, DataTypes.String());
        await factory.CreateColumn<WorkerConfig>(t => t.Value, DataTypes.StringMax);
        await factory.CreateUniqueConstraint<WorkerConfig>([t => t.Key]);
    }

    private async Task CreateAssignment()
    {
        await factory.UseHistory<Assignment>();
        await factory.CreateTable<Assignment>();
        await factory.CreateColumn<Assignment>(t => t.RoleId, DataTypes.Guid);
        await factory.CreateColumn<Assignment>(t => t.FromId, DataTypes.Guid);
        await factory.CreateColumn<Assignment>(t => t.ToId, DataTypes.Guid);
        await factory.CreateColumn<Assignment>(t => t.IsDelegable, DataTypes.Bool, defaultValue: "false");
        await factory.CreateForeignKeyConstraint<Assignment, Role>(t => t.RoleId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.FromId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.ToId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<Assignment>([t => t.RoleId, t => t.ToId, t => t.FromId]);
        await factory.AddHistory<Assignment>();

        await factory.UseHistory<AssignmentPackage>();
        await factory.CreateTable<AssignmentPackage>();
        await factory.CreateColumn<AssignmentPackage>(t => t.AssignmentId, DataTypes.Guid);
        await factory.CreateColumn<AssignmentPackage>(t => t.PackageId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<AssignmentPackage, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<AssignmentPackage, Package>(t => t.PackageId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<AssignmentPackage>([t => t.AssignmentId, t => t.PackageId]);
        await factory.AddHistory<Assignment>();

        await factory.UseHistory<AssignmentResource>();
        await factory.CreateTable<AssignmentResource>();
        await factory.CreateColumn<AssignmentResource>(t => t.AssignmentId, DataTypes.Guid);
        await factory.CreateColumn<AssignmentResource>(t => t.ResourceId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<AssignmentResource, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<AssignmentResource, Resource>(t => t.ResourceId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<AssignmentResource>([t => t.AssignmentId, t => t.ResourceId]);
        await factory.AddHistory<AssignmentResource>();
    }

    private async Task CreateGroups()
    {
        await factory.UseHistory<EntityGroup>();
        await factory.CreateTable<EntityGroup>();
        await factory.CreateColumn<EntityGroup>(t => t.Name, DataTypes.String(75));
        await factory.CreateColumn<EntityGroup>(t => t.OwnerId, DataTypes.Guid);
        await factory.CreateColumn<EntityGroup>(t => t.RequireRole, DataTypes.Bool);
        await factory.CreateForeignKeyConstraint<EntityGroup, Entity>(t => t.OwnerId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<EntityGroup>([t => t.OwnerId, t => t.Name]);
        await factory.AddHistory<EntityGroup>();

        await factory.UseHistory<GroupMember>();
        await factory.CreateTable<GroupMember>();
        await factory.CreateColumn<GroupMember>(t => t.GroupId, DataTypes.Guid);
        await factory.CreateColumn<GroupMember>(t => t.MemberId, DataTypes.Guid);
        await factory.CreateColumn<GroupMember>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await factory.CreateColumn<GroupMember>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await factory.CreateForeignKeyConstraint<GroupMember, EntityGroup>(t => t.GroupId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<GroupMember, Entity>(t => t.MemberId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<GroupMember>([t => t.GroupId, t => t.MemberId]);
        await factory.AddHistory<GroupMember>();

        await factory.UseHistory<GroupAdmin>();
        await factory.CreateTable<GroupAdmin>();
        await factory.CreateColumn<GroupAdmin>(t => t.GroupId, DataTypes.Guid);
        await factory.CreateColumn<GroupAdmin>(t => t.MemberId, DataTypes.Guid);
        await factory.CreateColumn<GroupAdmin>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await factory.CreateColumn<GroupAdmin>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await factory.CreateForeignKeyConstraint<GroupAdmin, EntityGroup>(t => t.GroupId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<GroupAdmin, Entity>(t => t.MemberId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<GroupAdmin>([t => t.GroupId, t => t.MemberId]);
        await factory.AddHistory<GroupAdmin>();

        await factory.UseHistory<GroupDelegation>();
        await factory.CreateTable<GroupDelegation>();
        await factory.CreateColumn<GroupDelegation>(t => t.FromId, dbType: DataTypes.Guid);
        await factory.CreateColumn<GroupDelegation>(t => t.ToId, dbType: DataTypes.Guid);
        await factory.CreateColumn<GroupDelegation>(t => t.SourceId, dbType: DataTypes.Guid);
        await factory.CreateColumn<GroupDelegation>(t => t.ViaId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<GroupDelegation, Assignment>(t => t.FromId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<GroupDelegation, EntityGroup>(t => t.ToId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<GroupDelegation, Entity>(t => t.SourceId, t => t.Id);
        await factory.CreateForeignKeyConstraint<GroupDelegation, Entity>(t => t.ViaId, t => t.Id);
        //// await factory.CreateUniqueConstraint<Delegation>([t => t.FromId, t => t.ToId]);
        await factory.AddHistory<GroupDelegation>();
    }

    private async Task CreateDelegations()
    {
        await factory.UseHistory<Delegation>();
        await factory.CreateTable<Delegation>();
        await factory.CreateColumn<Delegation>(t => t.FromId, dbType: DataTypes.Guid);
        await factory.CreateColumn<Delegation>(t => t.ToId, dbType: DataTypes.Guid);
        await factory.CreateColumn<Delegation>(t => t.SourceId, dbType: DataTypes.Guid);
        await factory.CreateColumn<Delegation>(t => t.ViaId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<Delegation, Assignment>(t => t.FromId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<Delegation, Assignment>(t => t.ToId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<Delegation, Entity>(t => t.SourceId, t => t.Id);
        await factory.CreateForeignKeyConstraint<Delegation, Entity>(t => t.ViaId, t => t.Id);
        await factory.AddHistory<Delegation>();

        await factory.UseHistory<DelegationAssignmentPackageResource>();
        await factory.CreateTable<DelegationAssignmentPackageResource>();
        await factory.CreateColumn<DelegationAssignmentPackageResource>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationAssignmentPackageResource>(t => t.AssignmentPackageId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationAssignmentPackageResource>(t => t.PackageResourceId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentPackageResource, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentPackageResource, AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentPackageResource, PackageResource>(t => t.PackageResourceId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationAssignmentPackageResource>([t => t.DelegationId, t => t.AssignmentPackageId, t => t.PackageResourceId]);
        await factory.AddHistory<DelegationAssignmentPackageResource>();

        await factory.UseHistory<DelegationAssignmentResource>();
        await factory.CreateTable<DelegationAssignmentResource>();
        await factory.CreateColumn<DelegationAssignmentResource>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationAssignmentResource>(t => t.AssignmentResourceId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentResource, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentResource, AssignmentResource>(t => t.AssignmentResourceId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationAssignmentResource>([t => t.DelegationId, t => t.AssignmentResourceId]);
        await factory.AddHistory<DelegationAssignmentResource>();

        await factory.UseHistory<DelegationAssignmentPackage>();
        await factory.CreateTable<DelegationAssignmentPackage>();
        await factory.CreateColumn<DelegationAssignmentPackage>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationAssignmentPackage>(t => t.AssignmentPackageId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentPackage, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationAssignmentPackage, AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationAssignmentPackage>([t => t.DelegationId, t => t.AssignmentPackageId]);
        await factory.AddHistory<DelegationAssignmentPackage>();

        await factory.UseHistory<DelegationRolePackageResource>();
        await factory.CreateTable<DelegationRolePackageResource>();
        await factory.CreateColumn<DelegationRolePackageResource>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationRolePackageResource>(t => t.RolePackageId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationRolePackageResource>(t => t.PackageResourceId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationRolePackageResource, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationRolePackageResource, RolePackage>(t => t.RolePackageId, t => t.Id, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<DelegationRolePackageResource, PackageResource>(t => t.PackageResourceId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationRolePackageResource>([t => t.DelegationId, t => t.RolePackageId, t => t.PackageResourceId]);
        await factory.AddHistory<DelegationRolePackageResource>();

        await factory.UseHistory<DelegationRoleResource>();
        await factory.CreateTable<DelegationRoleResource>();
        await factory.CreateColumn<DelegationRoleResource>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationRoleResource>(t => t.RoleResourceId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationRoleResource, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationRoleResource, RoleResource>(t => t.RoleResourceId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationRoleResource>([t => t.DelegationId, t => t.RoleResourceId]);
        await factory.AddHistory<DelegationRoleResource>();

        await factory.UseHistory<DelegationRolePackage>();
        await factory.CreateTable<DelegationRolePackage>();
        await factory.CreateColumn<DelegationRolePackage>(t => t.DelegationId, dbType: DataTypes.Guid);
        await factory.CreateColumn<DelegationRolePackage>(t => t.RolePackageId, dbType: DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<DelegationRolePackage, Delegation>(t => t.DelegationId, t => t.Id);
        await factory.CreateForeignKeyConstraint<DelegationRolePackage, RolePackage>(t => t.RolePackageId, t => t.Id, cascadeDelete: true);
        await factory.CreateUniqueConstraint<DelegationRolePackage>([t => t.DelegationId, t => t.RolePackageId]);
        await factory.AddHistory<DelegationRolePackage>();
    }

    private async Task CreateSchema()
    {
        await factory.CreateSchema("translation");
        await factory.CreateSchema("dbo_history");
        await factory.CreateSchema("translation_history");
    }

    private async Task CreateProvider()
    {
        await factory.UseHistory<Provider>();
        await factory.CreateTable<Provider>();
        await factory.CreateColumn<Provider>(t => t.Name, DataTypes.String(75));
        await factory.CreateColumn<Provider>(t => t.RefId, DataTypes.String(15), nullable: true);
        await factory.CreateUniqueConstraint<Provider>([t => t.Name]);
        await factory.AddHistory<Provider>();
    }

    private async Task CreateEntity()
    {
        await factory.UseHistory<EntityType>();
        await factory.CreateTable<EntityType>(useTranslation: true);
        await factory.CreateColumn<EntityType>(t => t.Name, DataTypes.String(50));
        await factory.CreateColumn<EntityType>(t => t.ProviderId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<EntityType, Provider>(t => t.ProviderId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<EntityType>([t => t.ProviderId, t => t.Name]);
        await factory.AddHistory<EntityType>();

        await factory.UseHistory<EntityVariant>();
        await factory.CreateTable<EntityVariant>(useTranslation: true);
        await factory.CreateColumn<EntityVariant>(t => t.Name, DataTypes.String(50));
        await factory.CreateColumn<EntityVariant>(t => t.Description, DataTypes.String(150));
        await factory.CreateColumn<EntityVariant>(t => t.TypeId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<EntityVariant, EntityType>(t => t.TypeId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<EntityVariant>([t => t.TypeId, t => t.Name]);
        await factory.AddHistory<EntityVariant>();

        await factory.UseHistory<Entity>();
        await factory.CreateTable<Entity>();
        await factory.CreateColumn<Entity>(t => t.Name, DataTypes.String(250));
        await factory.CreateColumn<Entity>(t => t.TypeId, DataTypes.Guid);
        await factory.CreateColumn<Entity>(t => t.VariantId, DataTypes.Guid);
        await factory.CreateColumn<Entity>(t => t.RefId, DataTypes.String(50));
        await factory.CreateForeignKeyConstraint<Entity, EntityType>(t => t.TypeId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<Entity, EntityVariant>(t => t.VariantId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<Entity>([t => t.Name, t => t.TypeId, t => t.RefId]);
        await factory.AddHistory<Entity>();

        await factory.UseHistory<EntityLookup>();
        await factory.CreateTable<EntityLookup>();
        await factory.CreateColumn<EntityLookup>(t => t.EntityId, DataTypes.Guid);
        await factory.CreateColumn<EntityLookup>(t => t.Key, DataTypes.String(100));
        await factory.CreateColumn<EntityLookup>(t => t.Value, DataTypes.String(100));
        await factory.CreateForeignKeyConstraint<EntityLookup, Entity>(t => t.EntityId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<EntityLookup>([t => t.EntityId, t => t.Key]);
        await factory.AddHistory<EntityLookup>();
    }

    private async Task CreateArea()
    {
        await factory.UseHistory<AreaGroup>();
        await factory.CreateTable<AreaGroup>(useTranslation: true);
        await factory.CreateColumn<AreaGroup>(t => t.Name, DataTypes.String(75));
        await factory.CreateColumn<AreaGroup>(t => t.Description, DataTypes.String(750));
        await factory.CreateColumn<AreaGroup>(t => t.EntityTypeId, DataTypes.Guid, nullable: true);
        await factory.CreateColumn<AreaGroup>(t => t.Urn, DataTypes.String(75), nullable: true);
        await factory.CreateForeignKeyConstraint<AreaGroup, EntityType>(t => t.EntityTypeId, t => t.Id);
        await factory.CreateUniqueConstraint<AreaGroup>([t => t.Name]);
        await factory.AddHistory<AreaGroup>();

        await factory.UseHistory<Area>();
        await factory.CreateTable<Area>(useTranslation: true);
        await factory.CreateColumn<Area>(t => t.Name, DataTypes.String(75));
        await factory.CreateColumn<Area>(t => t.Description, DataTypes.String(750));
        await factory.CreateColumn<Area>(t => t.IconName, DataTypes.String(50));
        await factory.CreateColumn<Area>(t => t.GroupId, DataTypes.Guid);
        await factory.CreateColumn<Area>(t => t.Urn, DataTypes.String(75), nullable: true);
        await factory.CreateForeignKeyConstraint<Area, AreaGroup>(t => t.GroupId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<Area>([t => t.Name]);
        await factory.AddHistory<Area>();
    }

    private async Task CreateTag()
    {
        await factory.UseHistory<TagGroup>();
        await factory.CreateTable<TagGroup>(useTranslation: true);
        await factory.CreateColumn<TagGroup>(t => t.Name, DataTypes.String(100));
        await factory.CreateUniqueConstraint<TagGroup>([t => t.Name]);
        await factory.AddHistory<TagGroup>();

        await factory.UseHistory<Tag>();
        await factory.CreateTable<Tag>(useTranslation: true);
        await factory.CreateColumn<Tag>(t => t.Name, DataTypes.String(50));
        await factory.CreateColumn<Tag>(t => t.GroupId, DataTypes.Guid, nullable: true);
        await factory.CreateColumn<Tag>(t => t.ParentId, DataTypes.Guid, nullable: true);
        await factory.CreateForeignKeyConstraint<Tag, TagGroup>(t => t.GroupId);
        await factory.CreateForeignKeyConstraint<Tag, Tag>(t => t.ParentId);
        await factory.CreateUniqueConstraint<Tag>([t => t.Name]);
        await factory.AddHistory<Tag>();
    }

    private async Task CreatePackage()
    {
        await factory.UseHistory<Package>();
        await factory.CreateTable<Package>(useTranslation: true);
        await factory.CreateColumn<Package>(t => t.Name, DataTypes.String(100));
        await factory.CreateColumn<Package>(t => t.Description, DataTypes.String(1500));
        await factory.CreateColumn<Package>(t => t.IsDelegable, DataTypes.Bool);
        await factory.CreateColumn<Package>(t => t.ProviderId, DataTypes.Guid);
        await factory.CreateColumn<Package>(t => t.EntityTypeId, DataTypes.Guid);
        await factory.CreateColumn<Package>(t => t.AreaId, DataTypes.Guid);
        await factory.CreateColumn<Package>(t => t.HasResources, DataTypes.Bool, defaultValue: "true");
        await factory.CreateColumn<Package>(t => t.Urn, DataTypes.String(75), nullable: true);
        await factory.CreateForeignKeyConstraint<Package, Provider>(t => t.ProviderId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<Package, EntityType>(t => t.EntityTypeId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<Package, Area>(t => t.AreaId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<Package>([t => t.ProviderId, t => t.Name]);
        await factory.AddHistory<Package>();

        await factory.UseHistory<PackageTag>();
        await factory.CreateTable<PackageTag>();
        await factory.CreateColumn<PackageTag>(t => t.PackageId, DataTypes.Guid);
        await factory.CreateColumn<PackageTag>(t => t.TagId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<PackageTag, Package>(t => t.PackageId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<PackageTag, Tag>(t => t.TagId, cascadeDelete: false); // ??
        await factory.CreateUniqueConstraint<PackageTag>([t => t.PackageId, t => t.TagId]);
        await factory.AddHistory<PackageTag>();

        await factory.UseHistory<PackageResource>();
        await factory.CreateTable<PackageResource>();
        await factory.CreateColumn<PackageResource>(t => t.PackageId, DataTypes.Guid);
        await factory.CreateColumn<PackageResource>(t => t.ResourceId, DataTypes.Guid);
        await factory.CreateColumn<PackageResource>(t => t.Read, DataTypes.Bool);
        await factory.CreateColumn<PackageResource>(t => t.Write, DataTypes.Bool);
        await factory.CreateColumn<PackageResource>(t => t.Sign, DataTypes.Bool);
        await factory.CreateForeignKeyConstraint<PackageResource, Package>(t => t.PackageId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<PackageResource, Resource>(t => t.ResourceId, cascadeDelete: true);
        await factory.AddHistory<PackageResource>();
    }

    private async Task CreateRole()
    {
        await factory.UseHistory<Role>();
        await factory.CreateTable<Role>(useTranslation: true);
        await factory.CreateColumn<Role>(t => t.EntityTypeId, DataTypes.Guid);
        await factory.CreateColumn<Role>(t => t.ProviderId, DataTypes.Guid);
        await factory.CreateColumn<Role>(t => t.Name, DataTypes.String(100));
        await factory.CreateColumn<Role>(t => t.Code, DataTypes.String(15));
        await factory.CreateColumn<Role>(t => t.Description, DataTypes.String(1500));
        await factory.CreateColumn<Role>(t => t.Urn, DataTypes.String(250));
        await factory.CreateForeignKeyConstraint<Role, EntityType>(t => t.EntityTypeId, cascadeDelete: false); // ??
        await factory.CreateForeignKeyConstraint<Role, Provider>(t => t.ProviderId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<Role>([t => t.Urn]);
        await factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Name]);
        await factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Code]);
        await factory.AddHistory<Role>();

        await factory.UseHistory<RolePackage>();
        await factory.CreateTable<RolePackage>();
        await factory.CreateColumn<RolePackage>(t => t.RoleId, DataTypes.Guid);
        await factory.CreateColumn<RolePackage>(t => t.PackageId, DataTypes.Guid);
        await factory.RemoveColumn<RolePackage>("IsActor");
        await factory.RemoveColumn<RolePackage>("IsAdmin");
        await factory.CreateColumn<RolePackage>(t => t.HasAccess, DataTypes.Bool, defaultValue: "false");
        await factory.CreateColumn<RolePackage>(t => t.CanDelegate, DataTypes.Bool, defaultValue: "false");
        await factory.CreateColumn<RolePackage>(t => t.EntityVariantId, DataTypes.Guid, nullable: true);
        await factory.CreateForeignKeyConstraint<RolePackage, Role>(t => t.RoleId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<RolePackage, Package>(t => t.PackageId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<RolePackage, EntityVariant>(t => t.EntityVariantId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<RolePackage>([t => t.RoleId, t => t.PackageId, t => t.EntityVariantId]);
        await factory.AddHistory<RolePackage>();

        await factory.UseHistory<EntityVariantRole>();
        await factory.CreateTable<EntityVariantRole>();
        await factory.CreateColumn<EntityVariantRole>(t => t.VariantId, DataTypes.Guid);
        await factory.CreateColumn<EntityVariantRole>(t => t.RoleId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<EntityVariantRole, EntityVariant>(t => t.VariantId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<EntityVariantRole, Role>(t => t.RoleId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<EntityVariantRole>([t => t.VariantId, t => t.RoleId]);
        await factory.AddHistory<EntityVariantRole>();

        await factory.UseHistory<RoleMap>();
        await factory.CreateTable<RoleMap>();
        await factory.CreateColumn<RoleMap>(t => t.HasRoleId, DataTypes.Guid);
        await factory.CreateColumn<RoleMap>(t => t.GetRoleId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.HasRoleId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.GetRoleId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<RoleMap>([t => t.HasRoleId, t => t.GetRoleId]);
        await factory.AddHistory<RoleMap>();

        await factory.UseHistory<RoleResource>();
        await factory.CreateTable<RoleResource>();
        await factory.CreateColumn<RoleResource>(t => t.RoleId, DataTypes.Guid);
        await factory.CreateColumn<RoleResource>(t => t.ResourceId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<RoleResource, Role>(t => t.RoleId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<RoleResource, Resource>(t => t.ResourceId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<RoleResource>([t => t.RoleId, t => t.ResourceId]);
        await factory.AddHistory<RoleResource>();
    }

    private async Task CreateResource()
    {
        await factory.UseHistory<ResourceType>();
        await factory.CreateTable<ResourceType>(useTranslation: true);
        await factory.CreateColumn<ResourceType>(t => t.Name, DataTypes.String(50));
        await factory.CreateUniqueConstraint<ResourceType>([t => t.Name]);
        await factory.AddHistory<ResourceType>();

        await factory.UseHistory<ResourceGroup>();
        await factory.CreateTable<ResourceGroup>(useTranslation: true);
        await factory.CreateColumn<ResourceGroup>(t => t.Name, DataTypes.String(50));
        await factory.CreateColumn<ResourceGroup>(t => t.Description, DataTypes.String(500), nullable: true);
        await factory.CreateColumn<ResourceGroup>(t => t.ProviderId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<ResourceGroup, Provider>(t => t.ProviderId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<ResourceGroup>([t => t.ProviderId, t => t.Name]);
        await factory.AddHistory<ResourceGroup>();

        await factory.UseHistory<Resource>();
        await factory.CreateTable<Resource>(useTranslation: true);
        await factory.CreateColumn<Resource>(t => t.Name, DataTypes.String(500));
        await factory.CreateColumn<Resource>(t => t.ProviderId, DataTypes.Guid);
        await factory.CreateColumn<Resource>(t => t.TypeId, DataTypes.Guid);
        await factory.CreateColumn<Resource>(t => t.GroupId, DataTypes.Guid);
        await factory.CreateColumn<Resource>(t => t.RefId, DataTypes.String(150));
        await factory.CreateColumn<Resource>(t => t.Description, DataTypes.String(150), nullable: true);
        await factory.CreateForeignKeyConstraint<Resource, Provider>(t => t.ProviderId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<Resource, ResourceType>(t => t.TypeId, cascadeDelete: false);
        await factory.CreateForeignKeyConstraint<Resource, ResourceGroup>(t => t.GroupId, cascadeDelete: false);
        await factory.CreateUniqueConstraint<Resource>([t => t.ProviderId, t => t.GroupId, t => t.RefId]);
        await factory.AddHistory<Resource>();

        await factory.UseHistory<ElementType>();
        await factory.CreateTable<ElementType>();
        await factory.CreateColumn<ElementType>(t => t.Name, DataTypes.String(75));
        await factory.CreateUniqueConstraint<ElementType>([t => t.Name]);
        await factory.AddHistory<ElementType>();

        await factory.UseHistory<Element>();
        await factory.CreateTable<Element>();
        await factory.CreateColumn<Element>(t => t.Name, DataTypes.String(150));
        await factory.CreateColumn<Element>(t => t.Urn, DataTypes.String(1000));
        await factory.CreateColumn<Element>(t => t.TypeId, DataTypes.Guid);
        await factory.CreateColumn<Element>(t => t.ResourceId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<Element, ElementType>(t => t.TypeId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<Element, Resource>(t => t.ResourceId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<Element>([t => t.ResourceId, t => t.Name]);
        await factory.AddHistory<Element>();

        await factory.UseHistory<Policy>();
        await factory.CreateTable<Policy>();
        await factory.CreateColumn<Policy>(t => t.Name, DataTypes.String(150));
        await factory.CreateColumn<Policy>(t => t.Description, DataTypes.String(500));
        await factory.CreateColumn<Policy>(t => t.ResourceId, DataTypes.Guid);
        await factory.CreateUniqueConstraint<Policy>([t => t.ResourceId, t => t.Name]);
        await factory.AddHistory<Policy>();

        await factory.UseHistory<PolicyElement>();
        await factory.CreateTable<PolicyElement>();
        await factory.CreateColumn<PolicyElement>(t => t.PolicyId, DataTypes.Guid);
        await factory.CreateColumn<PolicyElement>(t => t.ElementId, DataTypes.Guid);
        await factory.CreateForeignKeyConstraint<PolicyElement, Policy>(t => t.PolicyId, cascadeDelete: true);
        await factory.CreateForeignKeyConstraint<PolicyElement, Element>(t => t.ElementId, cascadeDelete: true);
        await factory.CreateUniqueConstraint<PolicyElement>([t => t.PolicyId, t => t.ElementId]);
        await factory.AddHistory<PolicyElement>();
    }
}
