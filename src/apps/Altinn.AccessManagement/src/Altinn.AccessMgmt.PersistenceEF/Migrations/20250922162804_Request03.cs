using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class Request03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_fromid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_requestedbyid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_toid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_viaid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_requeststatus_statusid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_role_roleid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_role_viaroleid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_requestmessage_entity_authorid",
                schema: "dbo",
                table: "requestmessage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestmessage_request_requestid",
                schema: "dbo",
                table: "requestmessage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_package_packageid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_request_requestid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_requeststatus_statusid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_request_requestid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_requeststatus_statusid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_resource_resourceid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_fromid",
                schema: "dbo",
                table: "request",
                column: "fromid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_requestedbyid",
                schema: "dbo",
                table: "request",
                column: "requestedbyid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_toid",
                schema: "dbo",
                table: "request",
                column: "toid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_viaid",
                schema: "dbo",
                table: "request",
                column: "viaid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_requeststatus_statusid",
                schema: "dbo",
                table: "request",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_role_roleid",
                schema: "dbo",
                table: "request",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_request_role_viaroleid",
                schema: "dbo",
                table: "request",
                column: "viaroleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestmessage_entity_authorid",
                schema: "dbo",
                table: "requestmessage",
                column: "authorid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestmessage_request_requestid",
                schema: "dbo",
                table: "requestmessage",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_package_packageid",
                schema: "dbo",
                table: "requestpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_request_requestid",
                schema: "dbo",
                table: "requestpackage",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_requeststatus_statusid",
                schema: "dbo",
                table: "requestpackage",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_request_requestid",
                schema: "dbo",
                table: "requestresource",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_requeststatus_statusid",
                schema: "dbo",
                table: "requestresource",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_resource_resourceid",
                schema: "dbo",
                table: "requestresource",
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
                name: "fk_request_entity_fromid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_requestedbyid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_toid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_entity_viaid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_requeststatus_statusid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_role_roleid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_request_role_viaroleid",
                schema: "dbo",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "fk_requestmessage_entity_authorid",
                schema: "dbo",
                table: "requestmessage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestmessage_request_requestid",
                schema: "dbo",
                table: "requestmessage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_package_packageid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_request_requestid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestpackage_requeststatus_statusid",
                schema: "dbo",
                table: "requestpackage");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_request_requestid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_requeststatus_statusid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.DropForeignKey(
                name: "fk_requestresource_resource_resourceid",
                schema: "dbo",
                table: "requestresource");

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_fromid",
                schema: "dbo",
                table: "request",
                column: "fromid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_requestedbyid",
                schema: "dbo",
                table: "request",
                column: "requestedbyid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_toid",
                schema: "dbo",
                table: "request",
                column: "toid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_entity_viaid",
                schema: "dbo",
                table: "request",
                column: "viaid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_requeststatus_statusid",
                schema: "dbo",
                table: "request",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_role_roleid",
                schema: "dbo",
                table: "request",
                column: "roleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_request_role_viaroleid",
                schema: "dbo",
                table: "request",
                column: "viaroleid",
                principalSchema: "dbo",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestmessage_entity_authorid",
                schema: "dbo",
                table: "requestmessage",
                column: "authorid",
                principalSchema: "dbo",
                principalTable: "entity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestmessage_request_requestid",
                schema: "dbo",
                table: "requestmessage",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_package_packageid",
                schema: "dbo",
                table: "requestpackage",
                column: "packageid",
                principalSchema: "dbo",
                principalTable: "package",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_request_requestid",
                schema: "dbo",
                table: "requestpackage",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestpackage_requeststatus_statusid",
                schema: "dbo",
                table: "requestpackage",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_request_requestid",
                schema: "dbo",
                table: "requestresource",
                column: "requestid",
                principalSchema: "dbo",
                principalTable: "request",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_requeststatus_statusid",
                schema: "dbo",
                table: "requestresource",
                column: "statusid",
                principalSchema: "dbo",
                principalTable: "requeststatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_requestresource_resource_resourceid",
                schema: "dbo",
                table: "requestresource",
                column: "resourceid",
                principalSchema: "dbo",
                principalTable: "resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
