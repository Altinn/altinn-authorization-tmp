using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class AddedAssignmentResourceFKToDelegationResourceAndCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "assigmentresourceid",
                schema: "dbo",
                table: "delegationresource",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "assigmentresourceid",
                schema: "dbo_history",
                table: "auditdelegationresource",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_delegationresource_assigmentresourceid",
                schema: "dbo",
                table: "delegationresource",
                column: "assigmentresourceid");

            migrationBuilder.AddForeignKey(
                name: "fk_delegationresource_assignmentresource_assigmentresourceid",
                schema: "dbo",
                table: "delegationresource",
                column: "assigmentresourceid",
                principalSchema: "dbo",
                principalTable: "assignmentresource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_delegationresource_assignmentresource_assigmentresourceid",
                schema: "dbo",
                table: "delegationresource");

            migrationBuilder.DropIndex(
                name: "ix_delegationresource_assigmentresourceid",
                schema: "dbo",
                table: "delegationresource");

            migrationBuilder.DropColumn(
                name: "assigmentresourceid",
                schema: "dbo",
                table: "delegationresource");

            migrationBuilder.DropColumn(
                name: "assigmentresourceid",
                schema: "dbo_history",
                table: "auditdelegationresource");
        }
    }
}
