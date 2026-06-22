using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <summary>
    /// Baseline migration for the legacy <c>delegation</c>, <c>accessmanagement</c> and
    /// <c>consent</c> schemas (the ones still backed by the Dapper / raw-Npgsql
    /// repositories), reproducing them exactly. These schemas are disjoint from the
    /// EF-modelled <c>dbo</c> / <c>dbo_history</c> schemas. The DDL is the canonical
    /// <c>pg_dump --schema-only</c> of the established database, shipped as the embedded
    /// <c>LegacySchemas.sql</c> resource. The objects are not modelled as EF entities, so
    /// the model snapshot stays unchanged and EF never manages or drops them; the
    /// repositories keep querying them unchanged.
    /// </summary>
    /// <inheritdoc />
    public partial class LegacySchemas_Baseline : Migration
    {
        private const string LegacySchemaScriptResource =
            "Altinn.AccessMgmt.PersistenceEF.Migrations.LegacySchemas.sql";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReadLegacySchemaScript());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS consent CASCADE;");
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS accessmanagement CASCADE;");
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS delegation CASCADE;");
        }

        private static string ReadLegacySchemaScript()
        {
            var assembly = typeof(LegacySchemas_Baseline).Assembly;
            using var stream = assembly.GetManifestResourceStream(LegacySchemaScriptResource)
                ?? throw new InvalidOperationException(
                    $"Embedded migration script '{LegacySchemaScriptResource}' was not found in assembly '{assembly.FullName}'.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
