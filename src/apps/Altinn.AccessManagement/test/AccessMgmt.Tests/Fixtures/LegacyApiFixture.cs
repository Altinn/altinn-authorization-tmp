using Altinn.AccessManagement.TestUtils.Fixtures;
using Microsoft.Extensions.Configuration;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// Variant of <see cref="ApiFixture"/> for test classes that exercise the Dapper /
/// raw-Npgsql repositories (<c>ResourceMetadataRepo</c>, <c>ConsentRepository</c>,
/// <c>DelegationMetadataRepo</c>) and the enum types bound via
/// <c>PersistenceDependencyInjectionExtensions.AddDatabase</c>.
/// </summary>
/// <remarks>
/// <para>
/// The legacy <c>accessmanagement.*</c>, <c>consent.*</c> and <c>delegation.*</c>
/// schemas (plus their enum types) are created by the EF Core baseline migration
/// <c>LegacySchemas_Baseline</c>, so they are part of the shared template that
/// <see cref="Altinn.AccessManagement.TestUtils.Factories.EFPostgresFactory"/> builds
/// alongside the <c>dbo</c>/<c>dbo_history</c> schemas. Every per-test database is a
/// clone of that template, so the legacy schemas are always present — no separate
/// migration pipeline runs at host startup.
/// </para>
/// <para>
/// This subclass additionally loads <c>appsettings.test.json</c> (for non-database
/// test configuration) and sets <c>PostgreSQLSettings:EnableDBConnection=true</c> so
/// the host exercises the same <c>ConfigurePostgreSqlConfiguration</c> path as
/// production. It deliberately does <em>not</em> set <c>RunIntegrationTests</c>: that
/// would run <c>Program.Init()</c> (EF migrate + static-data ingest) again on a
/// database already cloned from the migrated and seeded template, so the template
/// stays the single seed source.
/// </para>
/// </remarks>
public class LegacyApiFixture : AccessMgmtApiFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyApiFixture"/> class.
    /// </summary>
    public LegacyApiFixture()
    {
        WithAppsettings(builder =>
        {
            builder.AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: false);
        });

        WithInMemoryAppsettings(dict =>
        {
            // Exercise the production PostgreSQL configuration path
            // (AccessManagementHost.ConfigurePostgreSqlConfiguration). The legacy
            // accessmanagement.*, consent.* and delegation.* schemas are provisioned
            // by the EF baseline migration in the cloned template, not here.
            //
            // RunIntegrationTests is intentionally NOT set: it would trigger
            // Program.Init() (EF migrate + StaticDataIngest + register import) on a
            // clone that already has the migrated, seeded schema, so it would only
            // repeat work (the register import is feature-flag gated off in tests).
            dict["PostgreSQLSettings:EnableDBConnection"] = "true";
        });
    }
}
