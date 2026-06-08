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

    // Cascade pairs: each has a supplier assignment with one delegated resource seeded below.
    private static readonly Guid CascadeOkConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000011");
    private static readonly Guid CascadeOkSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000012");
    private static readonly Guid CascadeFailConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000013");
    private static readonly Guid CascadeFailSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000014");
    private static readonly Guid CascadeNoFlagConsumer = Guid.Parse("2c839000-0000-0000-0000-000000000015");
    private static readonly Guid CascadeNoFlagSupplier = Guid.Parse("2c839000-0000-0000-0000-000000000016");

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
                Org(CascadeOkConsumer, "910000011"),
                Org(CascadeOkSupplier, "910000012"),
                Org(CascadeFailConsumer, "910000013"),
                Org(CascadeFailSupplier, "910000014"),
                Org(CascadeNoFlagConsumer, "910000015"),
                Org(CascadeNoFlagSupplier, "910000016"),
                new Entity
                {
                    Id = Person,
                    Name = "Maskinporten Test Person",
                    PersonIdentifier = "08900049013",
                    RefId = "08900049013",
                    TypeId = EntityTypeConstants.Person,
                    VariantId = EntityVariantConstants.Person,
                });

            // A resource to attach to supplier assignments for the cascade tests. The cascade
            // path does not inspect the resource type, so a generic type/resource is sufficient.
            var resourceType = new ResourceType
            {
                Id = Guid.Parse("2c839000-0000-0000-0000-0000000000a1"),
                Name = "MaskinportenSupplierTestResourceType",
            };
            var resource = new Resource
            {
                Id = Guid.CreateVersion7(),
                Name = "Maskinporten Supplier Test Resource",
                Description = "Maskinporten Supplier Test Resource",
                RefId = "maskinporten-supplier-test-resource",
                ProviderId = ProviderConstants.ResourceRegistry.Id,
                TypeId = resourceType.Id,
            };
            db.ResourceTypes.Add(resourceType);
            db.Resources.Add(resource);

            // Pre-existing supplier assignment with no delegated resources, for the remove test.
            db.Assignments.Add(new Assignment
            {
                FromId = RemoveConsumer,
                ToId = RemoveSupplier,
                RoleId = RoleConstants.Supplier.Id,
            });

            // Supplier assignments that each have one delegated resource, for the cascade tests.
            var cascadeOk = SupplierAssignment(CascadeOkConsumer, CascadeOkSupplier);
            var cascadeFail = SupplierAssignment(CascadeFailConsumer, CascadeFailSupplier);
            var cascadeNoFlag = SupplierAssignment(CascadeNoFlagConsumer, CascadeNoFlagSupplier);
            db.Assignments.AddRange(cascadeOk, cascadeFail, cascadeNoFlag);
            db.AssignmentResources.AddRange(
                ResourceOn(cascadeOk, resource, "policies/cascade-ok.xml"),
                ResourceOn(cascadeFail, resource, "policies/cascade-fail.xml"),
                ResourceOn(cascadeNoFlag, resource, "policies/cascade-noflag.xml"));

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

    [Fact]
    public async Task RemoveSupplier_WhenResourcesExistAndCascadeFalse_ReturnsProblemAndKeepsAssignment()
    {
        var problem = await RunService(s => s.RemoveSupplier(CascadeNoFlagConsumer, CascadeNoFlagSupplier, cascade: false, TestContext.Current.CancellationToken));

        problem.Should().NotBeNull("removing a supplier with delegated resources requires cascade=true");

        (await SupplierAssignments(CascadeNoFlagConsumer, CascadeNoFlagSupplier)).Should().ContainSingle();
        (await DelegatedResourceCount(CascadeNoFlagConsumer, CascadeNoFlagSupplier)).Should().Be(1);
    }

    [Fact]
    public async Task RemoveSupplier_WhenCascadeTrueAndPolicyClearSucceeds_RemovesAssignmentAndResources()
    {
        var singleRights = new Mock<ISingleRightsService>();
        singleRights
            .Setup(r => r.ClearPolicyRules(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("v2");

        var problem = await RunService(
            s => s.RemoveSupplier(CascadeOkConsumer, CascadeOkSupplier, cascade: true, TestContext.Current.CancellationToken),
            singleRights: singleRights.Object);

        problem.Should().BeNull();

        (await SupplierAssignments(CascadeOkConsumer, CascadeOkSupplier)).Should().BeEmpty();
        (await DelegatedResourceCount(CascadeOkConsumer, CascadeOkSupplier)).Should().Be(0);
    }

    [Fact]
    public async Task RemoveSupplier_WhenCascadeTrueAndPolicyClearFails_ReturnsProblemAndKeepsRecords()
    {
        var singleRights = new Mock<ISingleRightsService>();
        singleRights
            .Setup(r => r.ClearPolicyRules(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        var problem = await RunService(
            s => s.RemoveSupplier(CascadeFailConsumer, CascadeFailSupplier, cascade: true, TestContext.Current.CancellationToken),
            singleRights: singleRights.Object);

        problem.Should().NotBeNull("a failed policy clear must abort the cascade");

        // Failure must not delete the database records.
        (await SupplierAssignments(CascadeFailConsumer, CascadeFailSupplier)).Should().ContainSingle();
        (await DelegatedResourceCount(CascadeFailConsumer, CascadeFailSupplier)).Should().Be(1);
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

    private static Assignment SupplierAssignment(Guid consumerId, Guid supplierId) => new()
    {
        FromId = consumerId,
        ToId = supplierId,
        RoleId = RoleConstants.Supplier.Id,
    };

    private static AssignmentResource ResourceOn(Assignment assignment, Resource resource, string policyPath) => new()
    {
        AssignmentId = assignment.Id,
        ResourceId = resource.Id,
        PolicyPath = policyPath,
        PolicyVersion = "v1",
    };

    private async Task<TResult> RunService<TResult>(
        Func<MaskinportenSupplierService, Task<TResult>> act,
        ISingleRightsService singleRights = null,
        IConnectionService connection = null,
        IEntityService entity = null,
        IAuditAccessor audit = null)
    {
        using var scope = Fixture.Services.CreateEFScope(SystemEntityConstants.StaticDataIngest);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = new MaskinportenSupplierService(
            db,
            audit ?? Mock.Of<IAuditAccessor>(),
            connection ?? Mock.Of<IConnectionService>(),
            singleRights ?? Mock.Of<ISingleRightsService>(),
            entity ?? Mock.Of<IEntityService>());

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

    private async Task<int> DelegatedResourceCount(Guid consumerId, Guid supplierId)
    {
        var count = 0;
        await Fixture.QueryDb(async db =>
        {
            count = await db.AssignmentResources
                .AsNoTracking()
                .CountAsync(ar =>
                    ar.Assignment.FromId == consumerId &&
                    ar.Assignment.ToId == supplierId &&
                    ar.Assignment.RoleId == RoleConstants.Supplier.Id);
        });

        return count;
    }
}
