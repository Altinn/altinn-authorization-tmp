using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altinn.AccessMgmt.PersistenceEF.Migrations
{
    /// <summary>
    /// Baseline migration that brings the <c>consent</c> schema under EF Core, reproducing
    /// it exactly: the enum types, the consent tables, their primary keys and indexes, the
    /// grants to <c>platform_authorization</c>, and the <c>hstore</c> extension the
    /// <c>consentrequest</c> table needs. The DDL is the canonical
    /// <c>pg_dump --schema-only --schema=consent</c> of the established database, made
    /// idempotent and shipped as the embedded <c>ConsentSchema.sql</c> resource, so it is a
    /// no-op on databases that already have the schema (provisioned by the earlier SQL
    /// scripts) and creates it on fresh ones.
    /// The objects are not modelled as EF entities, so the model snapshot stays unchanged
    /// and EF never manages or drops them; the existing raw-Npgsql consent repository keeps
    /// querying them unchanged.
    /// </summary>
    /// <inheritdoc />
    public partial class ConsentSchema_Baseline : Migration
    {
        private const string ConsentSchemaScriptResource =
            "Altinn.AccessMgmt.PersistenceEF.Migrations.ConsentSchema.sql";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ReadConsentSchemaScript());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS consent CASCADE;");
        }

        private static string ReadConsentSchemaScript()
        {
            var assembly = typeof(ConsentSchema_Baseline).Assembly;
            using var stream = assembly.GetManifestResourceStream(ConsentSchemaScriptResource)
                ?? throw new InvalidOperationException(
                    $"Embedded migration script '{ConsentSchemaScriptResource}' was not found in assembly '{assembly.FullName}'.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
