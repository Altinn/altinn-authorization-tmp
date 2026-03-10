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
            migrationBuilder.DropTable(
                name: "auditrequestassignment",
                schema: "dbo_history");

            migrationBuilder.DropTable(
                name: "requestassignment",
                schema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditrequestassignment",
                schema: "dbo_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_validto = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deleteoperation = table.Column<string>(type: "text", nullable: true),
                    audit_deletedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_deletedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditrequestassignment", x => new { x.id, x.audit_validfrom, x.audit_validto });
                });

            migrationBuilder.CreateTable(
                name: "requestassignment",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fromid = table.Column<Guid>(type: "uuid", nullable: false),
                    roleid = table.Column<Guid>(type: "uuid", nullable: false),
                    toid = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_changeoperation = table.Column<string>(type: "text", nullable: true),
                    audit_changedby = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_changedbysystem = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_validfrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requestassignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_requestassignment_entity_fromid",
                        column: x => x.fromid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_requestassignment_entity_toid",
                        column: x => x.toid,
                        principalSchema: "dbo",
                        principalTable: "entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_requestassignment_role_roleid",
                        column: x => x.roleid,
                        principalSchema: "dbo",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_fromid",
                schema: "dbo",
                table: "requestassignment",
                column: "fromid");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_fromid_toid_roleid_status",
                schema: "dbo",
                table: "requestassignment",
                columns: new[] { "fromid", "toid", "roleid", "status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_roleid",
                schema: "dbo",
                table: "requestassignment",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "ix_requestassignment_toid",
                schema: "dbo",
                table: "requestassignment",
                column: "toid");
        }
    }
}
