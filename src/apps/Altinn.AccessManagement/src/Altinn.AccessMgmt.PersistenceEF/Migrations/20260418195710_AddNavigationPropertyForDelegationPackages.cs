using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertyForDelegationPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_delegationpackage_delegationid",
                schema: "dbo",
                table: "delegationpackage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_delegationpackage_delegationid",
                schema: "dbo",
                table: "delegationpackage",
                column: "delegationid");
        }
    }
}
