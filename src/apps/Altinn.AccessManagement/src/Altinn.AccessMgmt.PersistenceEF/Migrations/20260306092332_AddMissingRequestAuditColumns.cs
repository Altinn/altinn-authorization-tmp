using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRequestAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_requestassignment_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.DropForeignKey(
                name: "fk_requestassignmentpackage_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestassignmentresource_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentresource_assignmentid_resourceid_action_re~",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentresource_requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentpackage_assignmentid_packageid_requestedby~",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentpackage_requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropIndex(
                name: "ix_requestassignment_fromid_toid_roleid_requestedbyid_status",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.DropIndex(
                name: "ix_requestassignment_requestedbyid",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignmentresource");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignmentpackage");

            migrationBuilder.DropColumn(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignment");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentresource_assignmentid_resourceid_action_st~",
                schema: "dbo",
                table: "requestassignmentresource",
                columns: new[] { "assignmentid", "resourceid", "action", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentpackage_assignmentid_packageid_status",
                schema: "dbo",
                table: "requestassignmentpackage",
                columns: new[] { "assignmentid", "packageid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_fromid_toid_roleid_status",
                schema: "dbo",
                table: "requestassignment",
                columns: new[] { "fromid", "toid", "roleid", "status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_requestassignmentresource_assignmentid_resourceid_action_st~",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropIndex(
                name: "ix_requestassignmentpackage_assignmentid_packageid_status",
                schema: "dbo",
                table: "requestassignmentpackage");

            migrationBuilder.DropIndex(
                name: "ix_requestassignment_fromid_toid_roleid_status",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo",
                table: "requestassignment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignmentresource",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignmentpackage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "requestedbyid",
                schema: "dbo_history",
                table: "auditrequestassignment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentresource_assignmentid_resourceid_action_re~",
                schema: "dbo",
                table: "requestassignmentresource",
                columns: new[] { "assignmentid", "resourceid", "action", "requestedbyid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentresource_requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource",
                column: "requestedbyid");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentpackage_assignmentid_packageid_requestedby~",
                schema: "dbo",
                table: "requestassignmentpackage",
                columns: new[] { "assignmentid", "packageid", "requestedbyid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignmentpackage_requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage",
                column: "requestedbyid");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_fromid_toid_roleid_requestedbyid_status",
                schema: "dbo",
                table: "requestassignment",
                columns: new[] { "fromid", "toid", "roleid", "requestedbyid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_requestedbyid",
                schema: "dbo",
                table: "requestassignment",
                column: "requestedbyid");

            migrationBuilder.AddForeignKey(
                name: "fk_requestassignment_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignment",
                column: "requestedbyid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestassignmentpackage_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignmentpackage",
                column: "requestedbyid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestassignmentresource_entity_requestedbyid",
                schema: "dbo",
                table: "requestassignmentresource",
                column: "requestedbyid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
