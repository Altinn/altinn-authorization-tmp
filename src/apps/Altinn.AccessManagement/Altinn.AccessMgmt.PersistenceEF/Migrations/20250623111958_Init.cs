using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "area",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    iconurl = table.Column<string>(type: "text", nullable: true),
                    groupid = table.Column<Guid>(type: "uuid", nullable: false),
                    urn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_area", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "areagroup",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    entitytypeid = table.Column<Guid>(type: "uuid", nullable: false),
                    urn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_areagroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assignmentpackage",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignmentid = table.Column<Guid>(type: "uuid", nullable: false),
                    packageid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignmentpackage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assignmentresource",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignmentid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignmentresource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delegation",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false),
                    facilitatorid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delegation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delegationpackage",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    delegationid = table.Column<Guid>(type: "uuid", nullable: false),
                    packageid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delegationpackage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delegationresource",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    delegationid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delegationresource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false),
                    variantid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    refid = table.Column<string>(type: "text", nullable: true),
                    parentid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entitylookup",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entityid = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entitylookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entitytype",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    providerid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entitytype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entityvariant",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entityvariant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entityvariantrole",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variantid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entityvariantrole", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    providerid = table.Column<Guid>(type: "uuid", nullable: false),
                    entitytypeid = table.Column<Guid>(type: "uuid", nullable: false),
                    areaid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    isassignable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    isdelegable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hasresources = table.Column<bool>(type: "boolean", nullable: false),
                    urn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "packageresource",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    packageid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_packageresource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    refid = table.Column<string>(type: "text", nullable: false),
                    logourl = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providertype",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_providertype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    providerid = table.Column<Guid>(type: "uuid", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    refid = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resourcetype",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resourcetype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entitytypeid = table.Column<Guid>(type: "uuid", nullable: false),
                    providerid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    iskeyrole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    urn = table.Column<string>(type: "text", nullable: false),
                    isassignable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rolelookup",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rolelookup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rolemap",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hasroleid = table.Column<Guid>(type: "uuid", nullable: false),
                    getroleid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rolemap", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rolepackage",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    packageid = table.Column<Guid>(type: "uuid", nullable: false),
                    entityvariantid = table.Column<Guid>(type: "uuid", nullable: true),
                    hasaccess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    canassign = table.Column<bool>(type: "boolean", nullable: false),
                    candelegate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rolepackage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roleresource",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roleresource", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_area_name",
                schema: "dbo",
                table: "area",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_areagroup_name",
                schema: "dbo",
                table: "areagroup",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_fromid_toid_roleid",
                schema: "dbo",
                table: "assignment",
                columns: new[] { "fromid", "toid", "roleid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignmentpackage_assignmentid_packageid",
                schema: "dbo",
                table: "assignmentpackage",
                columns: new[] { "assignmentid", "packageid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignmentresource_assignmentid_resourceid",
                schema: "dbo",
                table: "assignmentresource",
                columns: new[] { "assignmentid", "resourceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delegation_fromid_toid_facilitatorid",
                schema: "dbo",
                table: "delegation",
                columns: new[] { "fromid", "toid", "facilitatorid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delegationpackage_delegationid_packageid",
                schema: "dbo",
                table: "delegationpackage",
                columns: new[] { "delegationid", "packageid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delegationresource_delegationid_resourceid",
                schema: "dbo",
                table: "delegationresource",
                columns: new[] { "delegationid", "resourceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entity_name",
                schema: "dbo",
                table: "entity",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entitylookup_entityid_key",
                schema: "dbo",
                table: "entitylookup",
                columns: new[] { "entityid", "key" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "value", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entitytype_name",
                schema: "dbo",
                table: "entitytype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entityvariant_name",
                schema: "dbo",
                table: "entityvariant",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entityvariantrole_variantid_roleid",
                schema: "dbo",
                table: "entityvariantrole",
                columns: new[] { "variantid", "roleid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_package_providerid_name",
                schema: "dbo",
                table: "package",
                columns: new[] { "providerid", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_packageresource_packageid_resourceid",
                schema: "dbo",
                table: "packageresource",
                columns: new[] { "packageid", "resourceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_name",
                schema: "dbo",
                table: "provider",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_providertype_name",
                schema: "dbo",
                table: "providertype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resourcetype_name",
                schema: "dbo",
                table: "resourcetype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_providerid_code",
                schema: "dbo",
                table: "role",
                columns: new[] { "providerid", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_providerid_name",
                schema: "dbo",
                table: "role",
                columns: new[] { "providerid", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_urn",
                schema: "dbo",
                table: "role",
                column: "urn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rolelookup_roleid_key",
                schema: "dbo",
                table: "rolelookup",
                columns: new[] { "roleid", "key" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "value", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_rolemap_hasroleid_getroleid",
                schema: "dbo",
                table: "rolemap",
                columns: new[] { "hasroleid", "getroleid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rolepackage_roleid_packageid",
                schema: "dbo",
                table: "rolepackage",
                columns: new[] { "roleid", "packageid" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "entityvariantid" });

            migrationBuilder.CreateIndex(
                name: "ix_roleresource_roleid_resourceid",
                schema: "dbo",
                table: "roleresource",
                columns: new[] { "roleid", "resourceid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "area",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "areagroup",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "assignment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "assignmentpackage",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "assignmentresource",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "delegation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "delegationpackage",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "delegationresource",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entity",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entitylookup",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entitytype",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entityvariant",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entityvariantrole",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "package",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "packageresource",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "provider",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "providertype",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resource",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resourcetype",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "role",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "rolelookup",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "rolemap",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "rolepackage",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "roleresource",
                schema: "dbo");
        }
    }
}
