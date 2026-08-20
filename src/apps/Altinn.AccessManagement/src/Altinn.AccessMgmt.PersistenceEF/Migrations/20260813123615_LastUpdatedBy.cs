using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentresource_audit_changedby",
                schema: "dbo",
                table: "requestassignmentresource",
                column: "audit_changedby");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentpackage_audit_changedby",
                schema: "dbo",
                table: "requestassignmentpackage",
                column: "audit_changedby");

            migrationBuilder.AddForeignKey(
                name: "fk_requestassignmentpackage_entity_audit_changedby",
                schema: "dbo",
                table: "requestassignmentpackage",
                column: "audit_changedby",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_requestassignmentresource_entity_audit_changedby",
                schema: "dbo",
                table: "requestassignmentresource",
                column: "audit_changedby",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_requestassignmentpackage_entity_audit_changedby",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestassignmentresource_entity_audit_changedby",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentresource_audit_changedby",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentpackage_audit_changedby",
                schema: "dbo",
                table: "requestassignmentpackage");
        }
    }
}
