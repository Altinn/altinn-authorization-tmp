using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class RequestedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "byid",
                schema: "dbo",
                table: "requestassignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "byid",
                schema: "dbo_history",
                table: "auditrequestassignment",
                type: "uuid",
                nullable: true);

            // Removed FK and index
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "byid",
                schema: "dbo",
                table: "requestassignment");

            migrationBuilder.DropColumn(
                name: "byid",
                schema: "dbo_history",
                table: "auditrequestassignment");
        }
    }
}
