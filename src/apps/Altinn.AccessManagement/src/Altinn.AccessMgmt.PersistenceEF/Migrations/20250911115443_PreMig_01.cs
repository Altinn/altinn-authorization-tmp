using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class PreMig_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_entityvariant_name",
                schema: "dbo",
                table: "entityvariant");

            migrationBuilder.DropIndex(
                name: "ix_entitytype_name",
                schema: "dbo",
                table: "entitytype");

            migrationBuilder.DropIndex(
                name: "ix_entity_name",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_assignment_fromid_toid_roleid",
                schema: "dbo",
                table: "assignment");

            migrationBuilder.AlterColumn<bool>(
                name: "isprotected",
                schema: "dbo",
                table: "entitylookup",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "ix_entityvariant_name_typeid",
                schema: "dbo",
                table: "entityvariant",
                columns: new[] { "name", "typeid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entitytype_name_providerid",
                schema: "dbo",
                table: "entitytype",
                columns: new[] { "name", "providerid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entity_name_typeid_variantid",
                schema: "dbo",
                table: "entity",
                columns: new[] { "name", "typeid", "variantid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_fromid_toid_roleid",
                schema: "dbo",
                table: "assignment",
                columns: new[] { "fromid", "toid", "roleid" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_entityvariant_name_typeid",
                schema: "dbo",
                table: "entityvariant");

            migrationBuilder.DropIndex(
                name: "ix_entitytype_name_providerid",
                schema: "dbo",
                table: "entitytype");

            migrationBuilder.DropIndex(
                name: "ix_entity_name_typeid_variantid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_assignment_fromid_toid_roleid",
                schema: "dbo",
                table: "assignment");

            migrationBuilder.AlterColumn<bool>(
                name: "isprotected",
                schema: "dbo",
                table: "entitylookup",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_entityvariant_name",
                schema: "dbo",
                table: "entityvariant",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entitytype_name",
                schema: "dbo",
                table: "entitytype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entity_name",
                schema: "dbo",
                table: "entity",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_fromid_toid_roleid",
                schema: "dbo",
                table: "assignment",
                columns: new[] { "fromid", "toid", "roleid" },
                unique: true);
        }
    }
}
