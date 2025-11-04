using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class Requests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditrequest",
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
                    statusid = table.Column<Guid>(type: "uuid", nullable: false),
                    requestedbyid = table.Column<Guid>(type: "uuid", nullable: false),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    viaid = table.Column<Guid>(type: "uuid", nullable: true),
                    viaroleid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequest", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "auditrequestmessage",
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
                    authorid = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequestmessage", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "auditrequestpackage",
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
                    packageid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequestpackage", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "auditrequestresource",
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
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequestresource", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

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
                name: "auditrequeststatus",
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
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequeststatus", x => new { x.id, x.audit_validfrom, x.audit_validto });
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
                name: "auditresourceelementtype",
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
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditresourceelementtype", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "requeststatus",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requeststatus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resourceelementtype",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resourceelementtype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "request",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    statusid = table.Column<Guid>(type: "uuid", nullable: false),
                    requestedbyid = table.Column<Guid>(type: "uuid", nullable: false),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    viaid = table.Column<Guid>(type: "uuid", nullable: true),
                    viaroleid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_request", x => x.id);
                    table.ForeignKey(
                        name: "fk_request_entity_fromid",
                        column: x => x.fromid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_entity_requestedbyid",
                        column: x => x.requestedbyid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_entity_toid",
                        column: x => x.toid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_entity_viaid",
                        column: x => x.viaid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_requeststatus_statusid",
                        column: x => x.statusid,
                        principalSchema: "dbo",
                        principalTable: "requeststatus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_role_roleid",
                        column: x => x.roleid,
                        principalSchema: "dbo",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_request_role_viaroleid",
                        column: x => x.viaroleid,
                        principalSchema: "dbo",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalSchema: "dbo",
                        principalTable: "resourceelementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requestmessage",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requestid = table.Column<Guid>(type: "uuid", nullable: false),
                    authorid = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requestmessage", x => x.id);
                    table.ForeignKey(
                        name: "fk_requestmessage_entity_authorid",
                        column: x => x.authorid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestmessage_request_requestid",
                        column: x => x.requestid,
                        principalSchema: "dbo",
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requestpackage",
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
                    packageid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requestpackage", x => x.id);
                    table.ForeignKey(
                        name: "fk_requestpackage_package_packageid",
                        column: x => x.packageid,
                        principalSchema: "dbo",
                        principalTable: "package",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestpackage_request_requestid",
                        column: x => x.requestid,
                        principalSchema: "dbo",
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestpackage_requeststatus_statusid",
                        column: x => x.statusid,
                        principalSchema: "dbo",
                        principalTable: "requeststatus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "requestresource",
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
                    resourceid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requestresource", x => x.id);
                    table.ForeignKey(
                        name: "fk_requestresource_request_requestid",
                        column: x => x.requestid,
                        principalSchema: "dbo",
                        principalTable: "request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestresource_requeststatus_statusid",
                        column: x => x.statusid,
                        principalSchema: "dbo",
                        principalTable: "requeststatus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_requestresource_resource_resourceid",
                        column: x => x.resourceid,
                        principalSchema: "dbo",
                        principalTable: "resource",
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
                name: "ix_request_fromid",
                schema: "dbo",
                table: "request",
                column: "fromid");

            migrationBuilder.CreateIndex(
                name: "ix_request_fromid_toid_roleid_requestedbyid",
                schema: "dbo",
                table: "request",
                columns: new[] { "fromid", "toid", "roleid", "requestedbyid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_request_requestedbyid",
                schema: "dbo",
                table: "request",
                column: "requestedbyid");

            migrationBuilder.CreateIndex(
                name: "ix_request_roleid",
                schema: "dbo",
                table: "request",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "ix_request_statusid",
                schema: "dbo",
                table: "request",
                column: "statusid");

            migrationBuilder.CreateIndex(
                name: "ix_request_toid",
                schema: "dbo",
                table: "request",
                column: "toid");

            migrationBuilder.CreateIndex(
                name: "ix_request_viaid",
                schema: "dbo",
                table: "request",
                column: "viaid");

            migrationBuilder.CreateIndex(
                name: "ix_request_viaroleid",
                schema: "dbo",
                table: "request",
                column: "viaroleid");

            migrationBuilder.CreateIndex(
                name: "ix_requestmessage_authorid",
                schema: "dbo",
                table: "requestmessage",
                column: "authorid");

            migrationBuilder.CreateIndex(
                name: "ix_requestmessage_requestid",
                schema: "dbo",
                table: "requestmessage",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_requestpackage_packageid",
                schema: "dbo",
                table: "requestpackage",
                column: "packageid");

            migrationBuilder.CreateIndex(
                name: "ix_requestpackage_requestid",
                schema: "dbo",
                table: "requestpackage",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_requestpackage_requestid_packageid",
                schema: "dbo",
                table: "requestpackage",
                columns: new[] { "requestid", "packageid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestpackage_statusid",
                schema: "dbo",
                table: "requestpackage",
                column: "statusid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresource_requestid",
                schema: "dbo",
                table: "requestresource",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresource_requestid_resourceid",
                schema: "dbo",
                table: "requestresource",
                columns: new[] { "requestid", "resourceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestresource_resourceid",
                schema: "dbo",
                table: "requestresource",
                column: "resourceid");

            migrationBuilder.CreateIndex(
                name: "ix_requestresource_statusid",
                schema: "dbo",
                table: "requestresource",
                column: "statusid");

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
                name: "ix_requeststatus_name",
                schema: "dbo",
                table: "requeststatus",
                column: "name",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_resourceelementtype_name",
                schema: "dbo",
                table: "resourceelementtype",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditrequest",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditrequestmessage",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditrequestpackage",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditrequestresource",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditrequestresourceelement",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditrequeststatus",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditresourceelement",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "auditresourceelementtype",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "requestmessage",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "requestpackage",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "requestresource",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "requestresourceelement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "request",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resourceelement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "requeststatus",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "resourceelementtype",
                schema: "dbo");
        }
    }
}
