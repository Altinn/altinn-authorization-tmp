using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class RequestAssignmentResourceRightKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "rightkeys",
                schema: "dbo",
                table: "requestassignmentresource",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "rightkeys",
                schema: "dbo_history",
                table: "auditrequestassignmentresource",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rightkeys",
                schema: "dbo",
                table: "requestassignmentresource");

            migrationBuilder.DropColumn(
                name: "rightkeys",
                schema: "dbo_history",
                table: "auditrequestassignmentresource");
        }
    }
}
