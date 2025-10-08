using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class ResourceElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditrequestresourceelement",
                schema: "dbo_history",
                columns: table => new
                {
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_validto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    requestid = table.Column<Guid>(type: "uuid", nullable: false),
                    statusid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false),
                    elementid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequestresourceelement", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "auditresourceelement",
                schema: "dbo_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    refid = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditresourceelement", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "resourceelementtype",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resourceelementtype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resourceelement",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false),
                    typeid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    refid = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resourceelement", x => x.id);
                    table.ForeignKey(
                        name: "fk_resourceelement_resource_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "dbo",
                        principalTable: "resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resourceelement_resourceelementtype_typeid",
                        column: x => x.typeid,
                        principalTable: "resourceelementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requestresourceelement",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requestid = table.Column<Guid>(type: "uuid", nullable: false),
                    statusid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false),
                    elementid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requestresourceelement", x => x.id);
                    table.ForeignKey(
                        name: "fk_requestresourceelement_request_requestid",
                        column: x => x.requestid,
                        principalSchema: "dbo",
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestresourceelement_requeststatus_statusid",
                        column: x => x.statusid,
                        principalSchema: "dbo",
                        principalTable: "requeststatus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestresourceelement_resource_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "dbo",
                        principalTable: "resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestresourceelement_resourceelement_elementid",
                        column: x => x.elementid,
                        principalSchema: "dbo",
                        principalTable: "resourceelement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_requestresourceelement_elementid",
                schema: "dbo",
                table: "requestresourceelement",
                column: "elementid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresourceelement_requestid",
                schema: "dbo",
                table: "requestresourceelement",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresourceelement_requestid_elementid",
                schema: "dbo",
                table: "requestresourceelement",
                columns: new[] { "requestid", "elementid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestresourceelement_resourceid",
                schema: "dbo",
                table: "requestresourceelement",
                column: "resourceid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresourceelement_statusid",
                schema: "dbo",
                table: "requestresourceelement",
                column: "statusid");

            migrationBuilder.CreateIndex(
                name: "ix_resourceelement_resourceid",
                schema: "dbo",
                table: "resourceelement",
                column: "resourceid");

            migrationBuilder.CreateIndex(
                name: "ix_resourceelement_typeid",
                schema: "dbo",
                table: "resourceelement",
                column: "typeid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditrequestresourceelement",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditresourceelement",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "requestresourceelement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resourceelement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resourceelementtype");
        }
    }
}
