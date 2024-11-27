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

        await CreateKlientDelegeringMock();

    }

    private async Task CreateKlientDelegeringMock()
    {
        //// TODO: IVAR
        await _factory.CreateTable<Relation>(withHistory: UseHistory);
        await _factory.CreateColumn<Relation>("FromId", DataTypes.Guid);
        await _factory.CreateColumn<Relation>("RoleId", DataTypes.Guid);
        await _factory.CreateColumn<Relation>("IsDelegable", DataTypes.Bool);
        await _factory.CreateUniqueConstraint<Relation>(["FromId", "RoleId"]);
        await _factory.CreateForeignKeyConstraint<Relation, Entity>("FromId");
        await _factory.CreateForeignKeyConstraint<Relation, Role>("RoleId");

        await _factory.CreateTable<RelationAssignment>(withHistory: UseHistory);
        await _factory.CreateColumn<RelationAssignment>("RelationId", DataTypes.Guid);
        await _factory.CreateColumn<RelationAssignment>("ToId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<RelationAssignment>(["RelationId", "ToId"]);
        await _factory.CreateForeignKeyConstraint<RelationAssignment, Entity>("ToId");
        await _factory.CreateForeignKeyConstraint<RelationAssignment, Relation>("RelationId");

        /*GROUPS!*/
    }


    private async Task CreateSchema()
    {
        await _factory.CreateSchema("History");
        await _factory.CreateSchema("Translation");
    }

    private async Task CreateProvider()
    {
        await _factory.CreateTable<Provider>(withHistory: UseHistory);
        await _factory.CreateColumn<Provider>("Name", DataTypes.String(75));
        await _factory.CreateColumn<Provider>("RefId", DataTypes.String(15), nullable: true);
        await _factory.CreateUniqueConstraint<Provider>(["Name"]);
    }

    private async Task CreateEntity()
    {
        await _factory.CreateTable<EntityType>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<EntityType>("Name", DataTypes.String(50));
        await _factory.CreateColumn<EntityType>("ProviderId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<EntityType>(["ProviderId", "Name"]);
        await _factory.CreateForeignKeyConstraint<EntityType, Provider>("ProviderId");

        await _factory.CreateTable<EntityVariant>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<EntityVariant>("Name", DataTypes.String(50));
        await _factory.CreateColumn<EntityVariant>("Description", DataTypes.String(150));
        await _factory.CreateColumn<EntityVariant>("TypeId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<EntityVariant>(["TypeId", "Name"]);
        await _factory.CreateForeignKeyConstraint<EntityVariant, EntityType>("TypeId");

        await _factory.CreateTable<Entity>(withHistory: UseHistory);
        await _factory.CreateColumn<Entity>("Name", DataTypes.String(250));
        await _factory.CreateColumn<Entity>("TypeId", DataTypes.Guid);
        await _factory.CreateColumn<Entity>("VariantId", DataTypes.Guid);
        await _factory.CreateColumn<Entity>("RefId", DataTypes.String(50));
        await _factory.CreateUniqueConstraint<Entity>(["Name", "TypeId", "RefId"]);
        await _factory.CreateForeignKeyConstraint<Entity, EntityType>("TypeId");
        await _factory.CreateForeignKeyConstraint<Entity, EntityVariant>("VariantId");
    }
    
    private async Task CreateArea()
    {
        await _factory.CreateTable<AreaGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<AreaGroup>("Name", DataTypes.String(75));
        await _factory.CreateColumn<AreaGroup>("Description", DataTypes.String(750));
        await _factory.CreateUniqueConstraint<AreaGroup>(["Name"]);

        await _factory.CreateTable<Area>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Area>("Name", DataTypes.String(75));
        await _factory.CreateColumn<Area>("Description", DataTypes.String(750));
        await _factory.CreateColumn<Area>("IconName", DataTypes.String(50));
        await _factory.CreateColumn<Area>("GroupId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<Area>(["Name"]);
        await _factory.CreateForeignKeyConstraint<Area, AreaGroup>("GroupId");
    }

    private async Task CreateTag()
    {
        await _factory.CreateTable<TagGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<TagGroup>("Name", DataTypes.String(100));
        await _factory.CreateUniqueConstraint<TagGroup>(["Name"]);

        await _factory.CreateTable<Tag>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Tag>("Name", DataTypes.String(50));
        await _factory.CreateColumn<Tag>("GroupId", DataTypes.Guid, nullable: true);
        await _factory.CreateColumn<Tag>("ParentId", DataTypes.Guid, nullable: true);
        await _factory.CreateUniqueConstraint<Tag>(["Name"]);
        await _factory.CreateForeignKeyConstraint<Tag, TagGroup>("GroupId");
        await _factory.CreateForeignKeyConstraint<Tag, Tag>("ParentId");
    }

    private async Task CreatePackage()
    {
        await _factory.CreateTable<Package>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Package>("Name", DataTypes.String(100));
        await _factory.CreateColumn<Package>("Description", DataTypes.String(1500));
        await _factory.CreateColumn<Package>("IsDelegable", DataTypes.Bool);
        await _factory.CreateColumn<Package>("ProviderId", DataTypes.Guid);
        await _factory.CreateColumn<Package>("EntityTypeId", DataTypes.Guid);
        await _factory.CreateColumn<Package>("AreaId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<Package>(["ProviderId", "Name"]);
        await _factory.CreateForeignKeyConstraint<Package, Provider>("ProviderId");
        await _factory.CreateForeignKeyConstraint<Package, EntityType>("EntityTypeId");
        await _factory.CreateForeignKeyConstraint<Package, Area>("AreaId");

        await _factory.CreateTable<PackageTag>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageTag>("PackageId", DataTypes.Guid);
        await _factory.CreateColumn<PackageTag>("TagId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<PackageTag>(["PackageId", "TagId"]);
        await _factory.CreateForeignKeyConstraint<PackageTag, Package>("PackageId");
        await _factory.CreateForeignKeyConstraint<PackageTag, Tag>("TagId");

        await _factory.CreateTable<PackageDelegation>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageDelegation>("PackageId", DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>("ForId", DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>("ToId", DataTypes.Guid);
        await _factory.CreateColumn<PackageDelegation>("ById", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<PackageDelegation>(["ForId", "ToId", "PackageId"]);
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Package>("PackageId");
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>("ForId");
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>("ToId");
        await _factory.CreateForeignKeyConstraint<PackageDelegation, Entity>("ById");
    }

    private async Task CreateRole()
    {
        await _factory.CreateTable<Role>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Role>("EntityTypeId", DataTypes.Guid);
        await _factory.CreateColumn<Role>("ProviderId", DataTypes.Guid);
        await _factory.CreateColumn<Role>("Name", DataTypes.String(100));
        await _factory.CreateColumn<Role>("Code", DataTypes.String(15));
        await _factory.CreateColumn<Role>("Description", DataTypes.String(1500));
        await _factory.CreateColumn<Role>("Urn", DataTypes.String(250));
        await _factory.CreateUniqueConstraint<Role>(["Urn"]);
        await _factory.CreateUniqueConstraint<Role>(["EntityTypeId", "Name"]);
        await _factory.CreateUniqueConstraint<Role>(["EntityTypeId", "Code"]);
        await _factory.CreateForeignKeyConstraint<Role, EntityType>("EntityTypeId");
        await _factory.CreateForeignKeyConstraint<Role, Provider>("ProviderId");

        await _factory.CreateTable<RolePackage>(withHistory: UseHistory);
        await _factory.CreateColumn<RolePackage>("RoleId", DataTypes.Guid);
        await _factory.CreateColumn<RolePackage>("PackageId", DataTypes.Guid);
        await _factory.CreateColumn<RolePackage>("IsActor", DataTypes.Bool);
        await _factory.CreateColumn<RolePackage>("IsAdmin", DataTypes.Bool);
        await _factory.CreateColumn<RolePackage>("EntityVariantId", DataTypes.Guid, nullable: true);
        await _factory.CreateUniqueConstraint<RolePackage>(["RoleId", "PackageId", "EntityVariantId"]);
        await _factory.CreateForeignKeyConstraint<RolePackage, Role>("RoleId");
        await _factory.CreateForeignKeyConstraint<RolePackage, Package>("PackageId");
        await _factory.CreateForeignKeyConstraint<RolePackage, EntityVariant>("EntityVariantId");

        await _factory.CreateTable<EntityVariantRole>(withHistory: UseHistory);
        await _factory.CreateColumn<EntityVariantRole>("VariantId", DataTypes.Guid);
        await _factory.CreateColumn<EntityVariantRole>("RoleId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<EntityVariantRole>(["VariantId", "RoleId"]);
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, EntityVariant>("VariantId");
        await _factory.CreateForeignKeyConstraint<EntityVariantRole, Role>("RoleId");

        await _factory.CreateTable<RoleAssignment>(withHistory: UseHistory);
        await _factory.CreateColumn<RoleAssignment>("RoleId", DataTypes.Guid);
        await _factory.CreateColumn<RoleAssignment>("ForId", DataTypes.Guid);
        await _factory.CreateColumn<RoleAssignment>("ToId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<RoleAssignment>(["ForId", "RoleId", "ToId"]);
        await _factory.CreateForeignKeyConstraint<RoleAssignment, Role>("RoleId");
        await _factory.CreateForeignKeyConstraint<RoleAssignment, Entity>("ForId");
        await _factory.CreateForeignKeyConstraint<RoleAssignment, Entity>("ToId");

        await _factory.CreateTable<RoleMap>(withHistory: UseHistory);
        await _factory.CreateColumn<RoleMap>("HasRoleId", DataTypes.Guid);
        await _factory.CreateColumn<RoleMap>("GetRoleId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<RoleMap>(["HasRoleId", "GetRoleId"]);
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>("HasRoleId");
        await _factory.CreateForeignKeyConstraint<RoleMap, Role>("GetRoleId");
    }

    private async Task CreateResource()
    {
        await _factory.CreateTable<ResourceType>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceType>("Name", DataTypes.String(50));
        await _factory.CreateUniqueConstraint<ResourceType>(["Name"]);

        await _factory.CreateTable<ResourceGroup>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceGroup>("Name", DataTypes.String(50));
        await _factory.CreateColumn<ResourceGroup>("ProviderId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<ResourceGroup>(["ProviderId", "Name"]);
        await _factory.CreateForeignKeyConstraint<ResourceGroup, Provider>("ProviderId");

        /*
        await _factory.CreateTable<CustomAction>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<CustomAction>("Name", DataTypes.String(15));
        await _factory.CreateColumn<CustomAction>("ResourceGroupId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<CustomAction>(["ResourceGroupId", "Name"]);
        await _factory.CreateForeignKeyConstraint<CustomAction, ResourceGroup>("ResourceGroupId");
        */

        await _factory.CreateTable<Resource>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<Resource>("Name", DataTypes.String(500));
        await _factory.CreateColumn<Resource>("ProviderId", DataTypes.Guid);
        await _factory.CreateColumn<Resource>("TypeId", DataTypes.Guid);
        await _factory.CreateColumn<Resource>("GroupId", DataTypes.Guid);
        await _factory.CreateColumn<Resource>("RefId", DataTypes.String(150));
        await _factory.CreateColumn<Resource>("Description", DataTypes.String(150), nullable: true);
        await _factory.CreateUniqueConstraint<Resource>(["ProviderId", "GroupId", "RefId"]);
        await _factory.CreateForeignKeyConstraint<Resource, Provider>("ProviderId");
        await _factory.CreateForeignKeyConstraint<Resource, ResourceType>("TypeId");
        await _factory.CreateForeignKeyConstraint<Resource, ResourceGroup>("GroupId");

        /*
        await _factory.CreateTable<ResourceAction>(withHistory: UseHistory, withTranslation: UseTranslation);
        await _factory.CreateColumn<ResourceAction>("ResourceId", DataTypes.Guid);
        await _factory.CreateColumn<ResourceAction>("ActionId", DataTypes.Guid);
        await _factory.CreateUniqueConstraint<ResourceAction>(["ResourceId", "ActionId"]);
        await _factory.CreateForeignKeyConstraint<ResourceAction, Resource>("ResourceId");
        await _factory.CreateForeignKeyConstraint<ResourceAction, CustomAction>("ActionId");
        */

        await _factory.CreateTable<PackageResource>(withHistory: UseHistory);
        await _factory.CreateColumn<PackageResource>("PackageId", DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>("ResourceId", DataTypes.Guid);
        await _factory.CreateColumn<PackageResource>("Read", DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>("Write", DataTypes.Bool);
        await _factory.CreateColumn<PackageResource>("Sign", DataTypes.Bool);
    }
}
