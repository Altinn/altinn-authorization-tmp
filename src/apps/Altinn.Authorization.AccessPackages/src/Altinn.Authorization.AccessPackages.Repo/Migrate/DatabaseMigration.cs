using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Migrate;

/// <summary>
/// Access Package Migration
/// </summary>
public class DatabaseMigration : IDatabaseMigration
{
    private readonly IDbMigrationFactory _factory;

    private bool UseHistory { get { return _factory.UseHistory; } }

    private bool UseTranslation { get { return _factory.UseTranslation; } }

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
        await _factory.Init();
        await CreateSchema();

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

    private async Task CreateAssignment()
    {
        await _factory.CreateTable<Assignment>(withHistory: UseHistory);
        await _factory.CreateColumn<Assignment>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateColumn<Assignment>(t => t.FromId, DataTypes.Guid);
        await _factory.CreateColumn<Assignment>(t => t.ToId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Assignment, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.FromId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Assignment, Entity>(t => t.ToId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Assignment>([t => t.RoleId, t => t.ToId, t => t.FromId]);
    }

    private async Task CreateGroups()
    {
        await _factory.CreateTable<Group>(withHistory: UseHistory);
        await _factory.CreateColumn<Group>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<Group>(t => t.OwnerId, DataTypes.Guid);
        await _factory.CreateColumn<Group>(t => t.RequireRole, DataTypes.Bool);
        await _factory.CreateForeignKeyConstraint<Group, Entity>(t => t.OwnerId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Group>([t => t.OwnerId, t => t.Name]);

        await _factory.CreateTable<GroupMember>(withHistory: UseHistory);
        await _factory.CreateColumn<GroupMember>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<GroupMember>(t => t.MemberId, DataTypes.Guid);
        await _factory.CreateColumn<GroupMember>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateColumn<GroupMember>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateForeignKeyConstraint<GroupMember, Group>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<GroupMember, Entity>(t => t.MemberId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<GroupMember>([t => t.GroupId, t => t.MemberId]);

        await _factory.CreateTable<GroupAdmin>(withHistory: UseHistory);
        await _factory.CreateColumn<GroupAdmin>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<GroupAdmin>(t => t.MemberId, DataTypes.Guid);
        await _factory.CreateColumn<GroupAdmin>(t => t.ActiveFrom, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateColumn<GroupAdmin>(t => t.ActiveTo, DataTypes.DateTimeOffset, nullable: true);
        await _factory.CreateForeignKeyConstraint<GroupAdmin, Group>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<GroupAdmin, Entity>(t => t.MemberId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<GroupAdmin>([t => t.GroupId, t => t.MemberId]);
    }

    private async Task CreateDelegations()
    {
        await _factory.CreateTable<GroupDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<GroupDelegation>(t => t.AssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<GroupDelegation>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<GroupDelegation, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<GroupDelegation, Group>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<GroupDelegation>([t => t.AssignmentId, t => t.GroupId]);

        await _factory.CreateTable<RoleDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<RoleDelegation>(t => t.AssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<RoleDelegation>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<RoleDelegation, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<RoleDelegation, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<RoleDelegation>([t => t.AssignmentId, t => t.RoleId]);

        await _factory.CreateTable<AssignmentDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<AssignmentDelegation>(t => t.FromAssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<AssignmentDelegation>(t => t.ToAssignmentId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<AssignmentDelegation, Assignment>(t => t.FromAssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<AssignmentDelegation, Assignment>(t => t.ToAssignmentId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<AssignmentDelegation>([t => t.FromAssignmentId, t => t.ToAssignmentId]);

        await _factory.CreateTable<EntityDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<EntityDelegation>(t => t.AssignmentId, DataTypes.Guid);
        await _factory.CreateColumn<EntityDelegation>(t => t.EntityId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityDelegation, Assignment>(t => t.AssignmentId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<EntityDelegation, Role>(t => t.EntityId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityDelegation>([t => t.AssignmentId, t => t.EntityId]);

    }

    private async Task CreateSchema()
    {
        await _factory.CreateSchema("History");
        await _factory.CreateSchema("Translation");
    }

    private async Task CreateProvider()
    {
        await _factory.CreateTable<Provider>(withHistory: UseHistory);
        await _factory.CreateColumn<Provider>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<Provider>(t => t.RefId, DataTypes.String(15), nullable: true);
        await _factory.CreateUniqueConstraint<Provider>([t => t.Name]);
    }

    private async Task CreateEntity()
    {
        await _factory.CreateTable<EntityType>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<EntityType>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<EntityType>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityType, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityType>([t => t.ProviderId, t => t.Name]);

        await _factory.CreateTable<EntityVariant>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<EntityVariant>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<EntityVariant>(t => t.Description, DataTypes.String(150));
        await _factory.CreateColumn<EntityVariant>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityVariant, EntityType>(t => t.TypeId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityVariant>([t => t.TypeId, t => t.Name]);

        await _factory.CreateTable<Entity>(withHistory: UseHistory);
        await _factory.CreateColumn<Entity>(t => t.Name, DataTypes.String(250));
        await _factory.CreateColumn<Entity>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateColumn<Entity>(t => t.VariantId, DataTypes.Guid);
        await _factory.CreateColumn<Entity>(t => t.RefId, DataTypes.String(50));
        await _factory.CreateForeignKeyConstraint<Entity, EntityType>(t => t.TypeId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Entity, EntityVariant>(t => t.VariantId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Entity>([t => t.Name, t => t.TypeId, t => t.RefId]);
    }
    
    private async Task CreateArea()
    {
        await _factory.CreateTable<AreaGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<AreaGroup>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<AreaGroup>(t => t.Description, DataTypes.String(750));
        await _factory.CreateUniqueConstraint<AreaGroup>([t => t.Name]);

        await _factory.CreateTable<Area>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Area>(t => t.Name, DataTypes.String(75));
        await _factory.CreateColumn<Area>(t => t.Description, DataTypes.String(750));
        await _factory.CreateColumn<Area>(t => t.IconName, DataTypes.String(50));
        await _factory.CreateColumn<Area>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Area, AreaGroup>(t => t.GroupId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Area>([t => t.Name]);
    }

    private async Task CreateTag()
    {
        await _factory.CreateTable<TagGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<TagGroup>(t => t.Name, DataTypes.String(100));
        await _factory.CreateUniqueConstraint<TagGroup>([t => t.Name]);

        await _factory.CreateTable<Tag>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Tag>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<Tag>(t => t.GroupId, DataTypes.Guid, nullable: true);
        await _factory.CreateColumn<Tag>(t => t.ParentId, DataTypes.Guid, nullable: true);
        await _factory.CreateForeignKeyConstraint<Tag, TagGroup>(t => t.GroupId);
        await _factory.CreateForeignKeyConstraint<Tag, Tag>(t => t.ParentId);
        await _factory.CreateUniqueConstraint<Tag>([t => t.Name]);
    }

    private async Task CreatePackage()
    {
        await _factory.CreateTable<Package>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Package>(t => t.Name, DataTypes.String(100));
        await _factory.CreateColumn<Package>(t => t.Description, DataTypes.String(1500));
        await _factory.CreateColumn<Package>(t => t.IsDelegable, DataTypes.Bool);
        await _factory.CreateColumn<Package>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Package>(t => t.EntityTypeId, DataTypes.Guid);
        await _factory.CreateColumn<Package>(t => t.AreaId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<Package, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Package, EntityType>(t => t.EntityTypeId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Package, Area>(t => t.AreaId); //// TODO: cascadeDelete: true Or ???
        await _factory.CreateUniqueConstraint<Package>([t => t.ProviderId, t => t.Name]);

        await _factory.CreateTable<PackageTag>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageTag>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageTag>(t => t.TagId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<PackageTag, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageTag, Tag>(t => t.TagId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<PackageTag>([t => t.PackageId, t => t.TagId]);

        await _factory.CreateTable<PackageDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageDelegation>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>(t => t.ForId, DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>(t => t.ToId, DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>(t => t.ById, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>(t => t.ForId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>(t => t.ToId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>(t => t.ById); // TODO: cascadeDelete: true ???
        await _factory.CreateUniqueConstraint<PackageDelegation>([t => t.ForId, t => t.ToId, t => t.PackageId]);
    }

    private async Task CreateRole()
    {
        await _factory.CreateTable<Role>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Role>(t => t.EntityTypeId, DataTypes.Guid);
        await _factory.CreateColumn<Role>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Role>(t => t.Name, DataTypes.String(100));
        await _factory.CreateColumn<Role>(t => t.Code, DataTypes.String(15));
        await _factory.CreateColumn<Role>(t => t.Description, DataTypes.String(1500));
        await _factory.CreateColumn<Role>(t => t.Urn, DataTypes.String(250));
        await _factory.CreateForeignKeyConstraint<Role, EntityType>(t => t.EntityTypeId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Role, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<Role>([t => t.Urn]);
        await _factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Name]);
        await _factory.CreateUniqueConstraint<Role>([t => t.EntityTypeId, t => t.Code]);

        await _factory.CreateTable<RolePackage>(withHistory: UseHistory);
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

        await _factory.CreateTable<EntityVariantRole>(withHistory: UseHistory);
        await _factory.CreateColumn<EntityVariantRole>(t => t.VariantId, DataTypes.Guid);
        await _factory.CreateColumn<EntityVariantRole>(t => t.RoleId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, EntityVariant>(t => t.VariantId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, Role>(t => t.RoleId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<EntityVariantRole>([t => t.VariantId, t => t.RoleId]);

        ////await _factory.CreateTable<RoleAssignment>(withHistory: UseHistory);
        ////await _factory.CreateColumn<RoleAssignment>(t => t.RoleId, DataTypes.Guid);
        ////await _factory.CreateColumn<RoleAssignment>(t => t.ForId, DataTypes.Guid);
        ////await _factory.CreateColumn<RoleAssignment>(t => t.ToId, DataTypes.Guid);
        ////await _factory.CreateForeignKeyConstraint<RoleAssignment, Role>(t => t.RoleId);
        ////await _factory.CreateForeignKeyConstraint<RoleAssignment, Entity>(t => t.ForId);
        ////await _factory.CreateForeignKeyConstraint<RoleAssignment, Entity>(t => t.ToId);
        ////await _factory.CreateUniqueConstraint<RoleAssignment>([t => t.ForId, t => t.RoleId, t => t.ToId]);

        await _factory.CreateTable<RoleMap>(withHistory: UseHistory);
        await _factory.CreateColumn<RoleMap>(t => t.HasRoleId, DataTypes.Guid);
        await _factory.CreateColumn<RoleMap>(t => t.GetRoleId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.HasRoleId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>(t => t.GetRoleId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<RoleMap>([t => t.HasRoleId, t => t.GetRoleId]);
    }

    private async Task CreateResource()
    {
        await _factory.CreateTable<ResourceType>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceType>(t => t.Name, DataTypes.String(50));
        await _factory.CreateUniqueConstraint<ResourceType>([t => t.Name]);

        await _factory.CreateTable<ResourceGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceGroup>(t => t.Name, DataTypes.String(50));
        await _factory.CreateColumn<ResourceGroup>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<ResourceGroup, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateUniqueConstraint<ResourceGroup>([t => t.ProviderId, t => t.Name]);

        /*
        await _factory.CreateTable<CustomAction>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<CustomAction>(t => t.Name, DataTypes.String(15));
        await _factory.CreateColumn<CustomAction>(t => t.ResourceGroupId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<CustomAction, ResourceGroup>(t => t.ResourceGroupId);
        await _factory.CreateUniqueConstraint<CustomAction>([t => t.ResourceGroupId, t => t.Name]);
        */

        await _factory.CreateTable<Resource>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Resource>(t => t.Name, DataTypes.String(500));
        await _factory.CreateColumn<Resource>(t => t.ProviderId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.TypeId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.GroupId, DataTypes.Guid);
        await _factory.CreateColumn<Resource>(t => t.RefId, DataTypes.String(150));
        await _factory.CreateColumn<Resource>(t => t.Description, DataTypes.String(150), nullable: true);
        await _factory.CreateForeignKeyConstraint<Resource, Provider>(t => t.ProviderId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<Resource, ResourceType>(t => t.TypeId); //// TODO cascadeDelete: true ???
        await _factory.CreateForeignKeyConstraint<Resource, ResourceGroup>(t => t.GroupId); //// TODO: cascadeDelete: true ???
        await _factory.CreateUniqueConstraint<Resource>([t => t.ProviderId, t => t.GroupId, t => t.RefId]);

        /*
        await _factory.CreateTable<ResourceAction>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceAction>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateColumn<ResourceAction>(t => t.ActionId, DataTypes.Guid);
        await _factory.CreateForeignKeyConstraint<ResourceAction, Resource>(t => t.ResourceId);
        await _factory.CreateForeignKeyConstraint<ResourceAction, CustomAction>(t => t.ActionId);
        await _factory.CreateUniqueConstraint<ResourceAction>([t => t.ResourceId, t => t.ActionId]);
        */

        await _factory.CreateTable<PackageResource>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageResource>(t => t.PackageId, DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>(t => t.ResourceId, DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>(t => t.Read, DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>(t => t.Write, DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>(t => t.Sign, DataTypes.Bool);
        await _factory.CreateForeignKeyConstraint<PackageResource, Package>(t => t.PackageId, cascadeDelete: true);
        await _factory.CreateForeignKeyConstraint<PackageResource, Resource>(t => t.ResourceId, cascadeDelete: true);
    }
}
