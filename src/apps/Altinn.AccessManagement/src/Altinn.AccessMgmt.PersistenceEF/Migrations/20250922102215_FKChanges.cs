using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class FKChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entity_entity_parentid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropForeignKey(
                name: "fk_role_entitytype_entitytypeid",
                schema: "dbo",
                table: "role");

            migrationBuilder.DropForeignKey(
                name: "fk_role_provider_providerid",
                schema: "dbo",
                table: "role");

            migrationBuilder.DropForeignKey(
                name: "fk_roleresource_resource_resourceid",
                schema: "dbo",
                table: "roleresource");

            migrationBuilder.AddForeignKey(
                name: "fk_entity_entity_parentid",
                schema: "dbo",
                table: "entity",
                column: "parentid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_role_entitytype_entitytypeid",
                schema: "dbo",
                table: "role",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_role_provider_providerid",
                schema: "dbo",
                table: "role",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_roleresource_resource_resourceid",
                schema: "dbo",
                table: "roleresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_entity_entity_parentid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropForeignKey(
                name: "fk_role_entitytype_entitytypeid",
                schema: "dbo",
                table: "role");

            migrationBuilder.DropForeignKey(
                name: "fk_role_provider_providerid",
                schema: "dbo",
                table: "role");

            migrationBuilder.DropForeignKey(
                name: "fk_roleresource_resource_resourceid",
                schema: "dbo",
                table: "roleresource");

            migrationBuilder.AddForeignKey(
                name: "fk_entity_entity_parentid",
                schema: "dbo",
                table: "entity",
                column: "parentid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_role_entitytype_entitytypeid",
                schema: "dbo",
                table: "role",
                column: "entitytypeid",
                principalSchema: "dbo",
                principalTable: "entitytype",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_role_provider_providerid",
                schema: "dbo",
                table: "role",
                column: "providerid",
                principalSchema: "dbo",
                principalTable: "provider",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_roleresource_resource_resourceid",
                schema: "dbo",
                table: "roleresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
