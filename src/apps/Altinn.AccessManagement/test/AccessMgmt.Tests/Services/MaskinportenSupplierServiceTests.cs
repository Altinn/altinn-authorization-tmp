using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AccessMgmt.Tests.Services;

/// <summary>
/// Service-level tests for <see cref="MaskinportenSupplierService"/> against a real
/// <see cref="AppDbContext"/> (Testcontainers Postgres via <see cref="ApiFixture"/>).
/// The external collaborators (<see cref="IConnectionService"/>, <see cref="ISingleRightsService"/>,
/// <see cref="IEntityService"/>, <see cref="IAuditAccessor"/>) are mocked; the assertions exercise the
/// service's own branching: the shared organization validation (exists / not-self / both-organizations)
/// and the supplier-assignment lifecycle (create, idempotent create, idempotent delete).
///
/// This class owns an isolated, additively-seeded set of entities so each mutating test uses a distinct
/// consumer/supplier pair and the tests stay independent of execution order.
/// </summary>
public class MaskinportenSupplierServiceTests : IClassFixture<ApiFixture>
{
    // Distinct org pairs so order-independent: each mutating test owns its own pair.
    private static readonly Guid CreateConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000001");
    private static readonly Guid CreateSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000002");
    private static readonly Guid IdempotentConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000003");
    private static readonly Guid IdempotentSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000004");
    private static readonly Guid RemoveConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000005");
    private static readonly Guid RemoveSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000006");
    private static readonly Guid NoAssignmentConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000007");
    private static readonly Guid NoAssignmentSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000008");
    private static readonly Guid Person = Guid.Parse("2c839000-0000-0000-0000-0000000000f1");

    public MaskinportenSupplierServiceTests(ApiFixture fixture)
    {
        Fixture = fixture;
        Fixture.EnsureSeedOnce<MaskinportenSupplierServiceTests>(db =>
        {
            db.Entities.AddRange(
                Org(CreateConsumer, "910000001"),
                Org(CreateSupplier, "910000002"),
                Org(IdempotentConsumer, "910000003"),
                Org(IdempotentSupplier, "910000004"),
                Org(RemoveConsumer, "910000005"),
                Org(RemoveSupplier, "910000006"),
                Org(NoAssignmentConsumer, "910000007"),
                Org(NoAssignmentSupplier, "910000008"),
                new Entity
                {
                    Id = Person,
                    Name = "Maskinporten Test Person",
                    PersonIdentifier = "08900049013",
                    RefId = "08900049013",
                    TypeId = EntityTypeConstants.Person,
                    VariantId = EntityVariantConstants.Person,
                });

            // Pre-existing supplier assignment with no delegated resources, for the remove test.
            db.Assignments.Add(new Assignment
            {
                FromId = RemoveConsumer,
                ToId = RemoveSupplier,
                RoleId = RoleConstants.Supplier.Id,
            });

            db.SaveChanges();
        });
    }

    private ApiFixture Fixture { get; }

    [Fact]
    public async Task AddSupplier_WithValidOrganizations_CreatesSupplierAssignment()
    {
        var result = await RunService(s => s.AddSupplier(CreateConsumer, CreateSupplier, TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeFalse();

        var assignments = await SupplierAssignments(CreateConsumer, CreateSupplier);
        assignments.Should().ContainSingle();
        assignments[0].RoleId.Should().Be(RoleConstants.Supplier.Id);
        assignments[0].FromId.Should().Be(CreateConsumer);
        assignments[0].ToId.Should().Be(CreateSupplier);
    }

    [Fact]
    public async Task AddSupplier_WhenAssignmentAlreadyExists_ReturnsExistingWithoutCreatingDuplicate()
    {
        var first = await RunService(s => s.AddSupplier(IdempotentConsumer, IdempotentSupplier, TestContext.Current.CancellationToken));
        var second = await RunService(s => s.AddSupplier(IdempotentConsumer, IdempotentSupplier, TestContext.Current.CancellationToken));

        first.IsProblem.Should().BeFalse();
        second.IsProblem.Should().BeFalse();

        var assignments = await SupplierAssignments(IdempotentConsumer, IdempotentSupplier);
        assignments.Should().ContainSingle("a second AddSupplier for the same pair must not create a duplicate assignment");
    }

    [Fact]
    public async Task AddSupplier_WhenConsumerDoesNotExist_ReturnsProblem()
    {
        var result = await RunService(s => s.AddSupplier(Guid.NewGuid(), CreateSupplier, TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeTrue();
    }

    [Fact]
    public async Task AddSupplier_WhenSupplierDoesNotExist_ReturnsProblem()
    {
        var result = await RunService(s => s.AddSupplier(CreateConsumer, Guid.NewGuid(), TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeTrue();
    }

    [Fact]
    public async Task AddSupplier_WhenConsumerIsNotOrganization_ReturnsProblem()
    {
        var result = await RunService(s => s.AddSupplier(Person, CreateSupplier, TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeTrue();
    }

    [Fact]
    public async Task AddSupplier_WhenSupplierIsNotOrganization_ReturnsProblem()
    {
        var result = await RunService(s => s.AddSupplier(CreateConsumer, Person, TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeTrue();
    }

    [Fact]
    public async Task AddSupplier_WhenConsumerEqualsSupplier_ReturnsProblem()
    {
        var result = await RunService(s => s.AddSupplier(CreateConsumer, CreateConsumer, TestContext.Current.CancellationToken));

        result.IsProblem.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveSupplier_WhenAssignmentExistsWithoutResources_RemovesAssignment()
    {
        var problem = await RunService(s => s.RemoveSupplier(RemoveConsumer, RemoveSupplier, cascade: false, TestContext.Current.CancellationToken));

        problem.Should().BeNull();

        var assignments = await SupplierAssignments(RemoveConsumer, RemoveSupplier);
        assignments.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveSupplier_WhenAssignmentDoesNotExist_ReturnsNull()
    {
        var problem = await RunService(s => s.RemoveSupplier(NoAssignmentConsumer, NoAssignmentSupplier, cascade: false, TestContext.Current.CancellationToken));

        problem.Should().BeNull("removing a non-existent supplier assignment is idempotent (204)");
    }

    [Fact]
    public async Task RemoveSupplier_WhenConsumerIsNotOrganization_ReturnsProblem()
    {
        var problem = await RunService(s => s.RemoveSupplier(Person, NoAssignmentSupplier, cascade: false, TestContext.Current.CancellationToken));

        problem.Should().NotBeNull();
    }

    private static Entity Org(Guid id, string orgNo) => new()
    {
        Id = id,
        Name = $"Maskinporten Test Org {orgNo}",
        OrganizationIdentifier = orgNo,
        RefId = orgNo,
        TypeId = EntityTypeConstants.Organization,
        VariantId = EntityVariantConstants.AS,
    };

    private async Task<TResult> RunService<TResult>(Func<MaskinportenSupplierService, Task<TResult>> act)
    {
        using var scope = Fixture.Services.CreateEFScope(SystemEntityConstants.StaticDataIngest);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = new MaskinportenSupplierService(
            db,
            Mock.Of<IAuditAccessor>(),
            Mock.Of<IConnectionService>(),
            Mock.Of<ISingleRightsService>(),
            Mock.Of<IEntityService>());

        return await act(service);
    }

    private async Task<List<Assignment>> SupplierAssignments(Guid consumerId, Guid supplierId)
    {
        List<Assignment> rows = null;
        await Fixture.QueryDb(async db =>
        {
            rows = await db.Assignments
                .AsNoTracking()
                .Where(a => a.FromId == consumerId && a.ToId == supplierId && a.RoleId == RoleConstants.Supplier.Id)
                .ToListAsync();
        });

        return rows;
    }
}
