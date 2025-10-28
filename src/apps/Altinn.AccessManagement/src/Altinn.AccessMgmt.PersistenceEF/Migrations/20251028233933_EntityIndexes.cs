using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class EntityIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_entity_dateofbirth",
                schema: "dbo",
                table: "entity",
                column: "dateofbirth",
                filter: "DateOfBirth IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_dateofdeath",
                schema: "dbo",
                table: "entity",
                column: "dateofdeath",
                filter: "DateOfDeath IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_isdeleted",
                schema: "dbo",
                table: "entity",
                column: "isdeleted",
                filter: "IsDeleted = true")
                .Annotation("Npgsql:IndexInclude", new[] { "id", "deletedat" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_organizationidentifier",
                schema: "dbo",
                table: "entity",
                column: "organizationidentifier",
                filter: "OrganizationIdentifier IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_partyid",
                schema: "dbo",
                table: "entity",
                column: "partyid",
                filter: "PartyId IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_personidentifier",
                schema: "dbo",
                table: "entity",
                column: "personidentifier",
                filter: "PersonIdentifier IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_userid",
                schema: "dbo",
                table: "entity",
                column: "userid",
                filter: "UserId IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_entity_dateofbirth",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_dateofdeath",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_isdeleted",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_organizationidentifier",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_partyid",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_personidentifier",
                schema: "dbo",
                table: "entity");

            migrationBuilder.DropIndex(
                name: "ix_entity_userid",
                schema: "dbo",
                table: "entity");
        }
    }
}
