using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueNameConstraintForProviderName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_provider_name",
                schema: "dbo",
                table: "provider");

            migrationBuilder.CreateIndex(
                name: "ix_provider_name",
                schema: "dbo",
                table: "provider",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_provider_name",
                schema: "dbo",
                table: "provider");

            migrationBuilder.CreateIndex(
                name: "ix_provider_name",
                schema: "dbo",
                table: "provider",
                column: "name",
                unique: true);
        }
    }
}
