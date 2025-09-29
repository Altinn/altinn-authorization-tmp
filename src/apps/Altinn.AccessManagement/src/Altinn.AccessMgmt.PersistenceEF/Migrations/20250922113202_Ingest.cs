using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <inheritdoc />
    public partial class Ingest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA ingest;
                CREATE ROLE ingest_admins NOLOGIN;
                GRANT ingest_admins TO platform_authorization_admin, platform_authorization;
                ALTER SCHEMA ingest OWNER TO ingest_admins;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP SCHEMA ingest CASCADE;
                DROP ROLE ingest_admins;
                """);
        }
    }
}
