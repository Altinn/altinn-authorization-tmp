using System.Reflection;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Altinn.AccessMgmt.PersistenceEF.Tests.Extensions;

/// <summary>
/// Pins the EF1001 contract that <see cref="CustomMigrationsSqlGenerator"/>
/// depends on. The base class' constructor takes
/// <c>Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal.INpgsqlSingletonOptions</c>,
/// which Npgsql has marked internal — meaning it can move or be removed
/// without notice on a minor / patch upgrade of
/// <c>Npgsql.EntityFrameworkCore.PostgreSQL</c>. If that happens the
/// production project would still need to compile (it pins the type via the
/// generator subclass), so the failure mode is silent until something at
/// runtime trips. These tests run in CI on every package change and surface
/// the breakage as an explicit signal: re-shape the generator, find a
/// replacement extension point, or pin the package version.
/// </summary>
public class CustomMigrationsSqlGeneratorContractTests
{
    private const string InternalOptionsTypeFullName =
        "Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal.INpgsqlSingletonOptions";

    [Fact]
    public void CustomMigrationsSqlGenerator_constructor_signature_pins_INpgsqlSingletonOptions()
    {
        var ctors = typeof(CustomMigrationsSqlGenerator).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance);

        var ctor = Assert.Single(ctors);
        var parameters = ctor.GetParameters();

        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(MigrationsSqlGeneratorDependencies), parameters[0].ParameterType);
        Assert.Equal(InternalOptionsTypeFullName, parameters[1].ParameterType.FullName);
    }

    [Fact]
    public void NpgsqlMigrationsSqlGenerator_keeps_INpgsqlSingletonOptions_constructor()
    {
        // The base constructor is what we forward our argument into. If a future
        // Npgsql upgrade renames or removes the internal options type, this lookup
        // returns null before the production project's compile fails — same
        // failure mode, but framed as a test rather than a build error.
        var optionsType = typeof(NpgsqlMigrationsSqlGenerator).Assembly
            .GetType(InternalOptionsTypeFullName);

        Assert.NotNull(optionsType);

        var ctor = typeof(NpgsqlMigrationsSqlGenerator).GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: [typeof(MigrationsSqlGeneratorDependencies), optionsType!],
            modifiers: null);

        Assert.NotNull(ctor);
    }
}
