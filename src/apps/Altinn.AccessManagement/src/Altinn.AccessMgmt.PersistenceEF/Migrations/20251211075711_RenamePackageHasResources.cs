using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class RenamePackageHasResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hasresources",
                schema: "dbo",
                table: "package");

            migrationBuilder.RenameColumn(
                name: "hasresources",
                schema: "dbo_history",
                table: "auditpackage",
                newName: "isavailableforserviceowners");

            migrationBuilder.AddColumn<bool>(
                name: "isavailableforserviceowners",
                schema: "dbo",
                table: "package",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isavailableforserviceowners",
                schema: "dbo",
                table: "package");

            migrationBuilder.RenameColumn(
                name: "isavailableforserviceowners",
                schema: "dbo_history",
                table: "auditpackage",
                newName: "hasresources");

            migrationBuilder.AddColumn<bool>(
                name: "hasresources",
                schema: "dbo",
                table: "package",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
