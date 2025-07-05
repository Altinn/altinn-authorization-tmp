using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations.AuditDb
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "history");

            migrationBuilder.CreateTable(
                name: "area",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Urn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_area", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "areagroup",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Urn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areagroup", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "AssignmentPackage",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentPackage", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "AssignmentResource",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentResource", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "Delegation",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    FromId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilitatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegation", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "DelegationPackage",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    DelegationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationPackage", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "DelegationResource",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    DelegationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationResource", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "Entity",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RefId = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "EntityLookup",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLookup", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "EntityType",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityType", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "EntityVariant",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityVariant", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "EntityVariantRole",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityVariantRole", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "package",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsAssignable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelegable = table.Column<bool>(type: "boolean", nullable: false),
                    HasResources = table.Column<bool>(type: "boolean", nullable: false),
                    Urn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "PackageResource",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageResource", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "Provider",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RefId = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provider", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "ProviderType",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderType", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "Resource",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RefId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resource", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "ResourceType",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceType", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    EntityTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsKeyRole = table.Column<bool>(type: "boolean", nullable: false),
                    Urn = table.Column<string>(type: "text", nullable: true),
                    IsAssignable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "RoleLookup",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleLookup", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "RoleMap",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    HasRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    GetRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMap", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "RolePackage",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasAccess = table.Column<bool>(type: "boolean", nullable: false),
                    CanAssign = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelegate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePackage", x => new { x.Id, x.audit_changeoperation });
                });

            migrationBuilder.CreateTable(
                name: "RoleResource",
                schema: "history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: false),
                    audit_validfrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleResource", x => new { x.Id, x.audit_changeoperation });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "area",
                schema: "history");

            migrationBuilder.DropTable(
                name: "areagroup",
                schema: "history");

            migrationBuilder.DropTable(
                name: "assignment",
                schema: "history");

            migrationBuilder.DropTable(
                name: "AssignmentPackage",
                schema: "history");

            migrationBuilder.DropTable(
                name: "AssignmentResource",
                schema: "history");

            migrationBuilder.DropTable(
                name: "Delegation",
                schema: "history");

            migrationBuilder.DropTable(
                name: "DelegationPackage",
                schema: "history");

            migrationBuilder.DropTable(
                name: "DelegationResource",
                schema: "history");

            migrationBuilder.DropTable(
                name: "Entity",
                schema: "history");

            migrationBuilder.DropTable(
                name: "EntityLookup",
                schema: "history");

            migrationBuilder.DropTable(
                name: "EntityType",
                schema: "history");

            migrationBuilder.DropTable(
                name: "EntityVariant",
                schema: "history");

            migrationBuilder.DropTable(
                name: "EntityVariantRole",
                schema: "history");

            migrationBuilder.DropTable(
                name: "package",
                schema: "history");

            migrationBuilder.DropTable(
                name: "PackageResource",
                schema: "history");

            migrationBuilder.DropTable(
                name: "Provider",
                schema: "history");

            migrationBuilder.DropTable(
                name: "ProviderType",
                schema: "history");

            migrationBuilder.DropTable(
                name: "Resource",
                schema: "history");

            migrationBuilder.DropTable(
                name: "ResourceType",
                schema: "history");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "history");

            migrationBuilder.DropTable(
                name: "RoleLookup",
                schema: "history");

            migrationBuilder.DropTable(
                name: "RoleMap",
                schema: "history");

            migrationBuilder.DropTable(
                name: "RolePackage",
                schema: "history");

            migrationBuilder.DropTable(
                name: "RoleResource",
                schema: "history");
        }
    }
}
