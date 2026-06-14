using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class EntityEmailIdentifierLowerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS dbo.ix_entity_emailidentifier_lower
                ON dbo.entity (lower(emailidentifier)) 
                INCLUDE (id)
                WHERE emailidentifier IS NOT NULL;",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP INDEX CONCURRENTLY IF EXISTS dbo.ix_entity_emailidentifier_lower;",
                suppressTransaction: true);
        }
    }
}
