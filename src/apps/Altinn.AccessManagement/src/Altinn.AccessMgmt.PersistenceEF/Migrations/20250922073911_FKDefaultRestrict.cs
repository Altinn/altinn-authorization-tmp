using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class FKDefaultRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_areagroup_entitytype_entitytypeid",
                schema: "dbo",
                table: "areagroup");

            migrationBuilder.DropForeignKey(
                name: "fk_assignment_role_roleid",
                schema: "dbo",
                table: "assignment");

            migrationBuilder.DropForeignKey(
                name: "fk_assignmentpackage_package_packageid",
                schema: "dbo",
                table: "assignmentpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_assignmentresource_resource_resourceid",
                schema: "dbo",
                table: "assignmentresource");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_fromid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_toid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_viaid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_package_packageid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_resource_resourceid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_role_roleid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_role_viaroleid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_delegationpackage_package_packageid",
                schema: "dbo",
                table: "delegationpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_delegationresource_resource_resourceid",
                schema: "dbo",
                table: "delegationresource");

            migrationBuilder.DropForeignKey(
                name: "fk_entitytype_provider_providerid",
                schema: "dbo",
                table: "entitytype");

            migrationBuilder.DropForeignKey(
                name: "fk_entityvariantrole_role_roleid",
                schema: "dbo",
                table: "entityvariantrole");

            migrationBuilder.DropForeignKey(
                name: "fk_package_area_areaid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_package_entitytype_entitytypeid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_package_provider_providerid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_packageresource_resource_resourceid",
                schema: "dbo",
                table: "packageresource");

            migrationBuilder.DropForeignKey(
                name: "fk_provider_providertype_typeid",
                schema: "dbo",
                table: "provider");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_provider_providerid",
                schema: "dbo",
                table: "resource");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_resourcetype_typeid",
                schema: "dbo",
                table: "resource");

            migrationBuilder.DropForeignKey(
                name: "fk_rolepackage_entityvariant_entityvariantid",
                schema: "dbo",
                table: "rolepackage");

            migrationBuilder.DropForeignKey(
                name: "fk_rolepackage_package_packageid",
                schema: "dbo",
                table: "rolepackage");

            migrationBuilder.DropIndex(
                name: "ix_entity_name_typeid_variantid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.CreateIndex(
                name: "ix_entity_name_refid_typeid_variantid",
                schema: "dbo",
                table: "entity",
                columns: new[] { "name", "refid", "typeid", "variantid" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_areagroup_entitytype_entitytypeid",
                schema: "dbo",
                table: "areagroup",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_assignment_role_roleid",
                schema: "dbo",
                table: "assignment",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_assignmentpackage_package_packageid",
                schema: "dbo",
                table: "assignmentpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_assignmentresource_resource_resourceid",
                schema: "dbo",
                table: "assignmentresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_fromid",
                table: "connections",
                column: "fromid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_toid",
                table: "connections",
                column: "toid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_viaid",
                table: "connections",
                column: "viaid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_package_packageid",
                table: "connections",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_resource_resourceid",
                table: "connections",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_role_roleid",
                table: "connections",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_role_viaroleid",
                table: "connections",
                column: "viaroleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_delegationpackage_package_packageid",
                schema: "dbo",
                table: "delegationpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_delegationresource_resource_resourceid",
                schema: "dbo",
                table: "delegationresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_entitytype_provider_providerid",
                schema: "dbo",
                table: "entitytype",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_entityvariantrole_role_roleid",
                schema: "dbo",
                table: "entityvariantrole",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_package_area_areaid",
                schema: "dbo",
                table: "package",
                column: "areaid",
                principalSchema: "dbo",
                principalTable: "area",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_package_entitytype_entitytypeid",
                schema: "dbo",
                table: "package",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_package_provider_providerid",
                schema: "dbo",
                table: "package",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_packageresource_resource_resourceid",
                schema: "dbo",
                table: "packageresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_provider_providertype_typeid",
                schema: "dbo",
                table: "provider",
                column: "typeid",
                principalSchema: "dbo",
                principalTable: "providertype",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_provider_providerid",
                schema: "dbo",
                table: "resource",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_resourcetype_typeid",
                schema: "dbo",
                table: "resource",
                column: "typeid",
                principalSchema: "dbo",
                principalTable: "resourcetype",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_rolepackage_entityvariant_entityvariantid",
                schema: "dbo",
                table: "rolepackage",
                column: "entityvariantid",
                principalSchema: "dbo",
                principalTable: "entityvariant",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_rolepackage_package_packageid",
                schema: "dbo",
                table: "rolepackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_areagroup_entitytype_entitytypeid",
                schema: "dbo",
                table: "areagroup");

            migrationBuilder.DropForeignKey(
                name: "fk_assignment_role_roleid",
                schema: "dbo",
                table: "assignment");

            migrationBuilder.DropForeignKey(
                name: "fk_assignmentpackage_package_packageid",
                schema: "dbo",
                table: "assignmentpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_assignmentresource_resource_resourceid",
                schema: "dbo",
                table: "assignmentresource");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_fromid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_toid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_entity_viaid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_package_packageid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_resource_resourceid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_role_roleid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_connections_role_viaroleid",
                table: "connections");

            migrationBuilder.DropForeignKey(
                name: "fk_delegationpackage_package_packageid",
                schema: "dbo",
                table: "delegationpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_delegationresource_resource_resourceid",
                schema: "dbo",
                table: "delegationresource");

            migrationBuilder.DropForeignKey(
                name: "fk_entitytype_provider_providerid",
                schema: "dbo",
                table: "entitytype");

            migrationBuilder.DropForeignKey(
                name: "fk_entityvariantrole_role_roleid",
                schema: "dbo",
                table: "entityvariantrole");

            migrationBuilder.DropForeignKey(
                name: "fk_package_area_areaid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_package_entitytype_entitytypeid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_package_provider_providerid",
                schema: "dbo",
                table: "package");

            migrationBuilder.DropForeignKey(
                name: "fk_packageresource_resource_resourceid",
                schema: "dbo",
                table: "packageresource");

            migrationBuilder.DropForeignKey(
                name: "fk_provider_providertype_typeid",
                schema: "dbo",
                table: "provider");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_provider_providerid",
                schema: "dbo",
                table: "resource");

            migrationBuilder.DropForeignKey(
                name: "fk_resource_resourcetype_typeid",
                schema: "dbo",
                table: "resource");

            migrationBuilder.DropForeignKey(
                name: "fk_rolepackage_entityvariant_entityvariantid",
                schema: "dbo",
                table: "rolepackage");

            migrationBuilder.DropForeignKey(
                name: "fk_rolepackage_package_packageid",
                schema: "dbo",
                table: "rolepackage");

            migrationBuilder.DropIndex(
                name: "ix_entity_name_refid_typeid_variantid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.CreateIndex(
                name: "ix_entity_name_typeid_variantid",
                schema: "dbo",
                table: "entity",
                columns: new[] { "name", "typeid", "variantid" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_areagroup_entitytype_entitytypeid",
                schema: "dbo",
                table: "areagroup",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignment_role_roleid",
                schema: "dbo",
                table: "assignment",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignmentpackage_package_packageid",
                schema: "dbo",
                table: "assignmentpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignmentresource_resource_resourceid",
                schema: "dbo",
                table: "assignmentresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_fromid",
                table: "connections",
                column: "fromid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_toid",
                table: "connections",
                column: "toid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_entity_viaid",
                table: "connections",
                column: "viaid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_package_packageid",
                table: "connections",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_resource_resourceid",
                table: "connections",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_role_roleid",
                table: "connections",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_connections_role_viaroleid",
                table: "connections",
                column: "viaroleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_delegationpackage_package_packageid",
                schema: "dbo",
                table: "delegationpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_delegationresource_resource_resourceid",
                schema: "dbo",
                table: "delegationresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_entitytype_provider_providerid",
                schema: "dbo",
                table: "entitytype",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_entityvariantrole_role_roleid",
                schema: "dbo",
                table: "entityvariantrole",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_package_area_areaid",
                schema: "dbo",
                table: "package",
                column: "areaid",
                principalSchema: "dbo",
                principalTable: "area",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_package_entitytype_entitytypeid",
                schema: "dbo",
                table: "package",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_package_provider_providerid",
                schema: "dbo",
                table: "package",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_packageresource_resource_resourceid",
                schema: "dbo",
                table: "packageresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_provider_providertype_typeid",
                schema: "dbo",
                table: "provider",
                column: "typeid",
                principalSchema: "dbo",
                principalTable: "providertype",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_provider_providerid",
                schema: "dbo",
                table: "resource",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_resourcetype_typeid",
                schema: "dbo",
                table: "resource",
                column: "typeid",
                principalSchema: "dbo",
                principalTable: "resourcetype",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_rolepackage_entityvariant_entityvariantid",
                schema: "dbo",
                table: "rolepackage",
                column: "entityvariantid",
                principalSchema: "dbo",
                principalTable: "entityvariant",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_rolepackage_package_packageid",
                schema: "dbo",
                table: "rolepackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
