using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Tests.Integration.Services;

/// <summary>
/// Integration tests for the AccessManager guards on
/// <see cref="Altinn.AccessMgmt.Core.Services.AssignmentService"/>.
/// </summary>
/// <remarks>
/// The guards require the calling user to hold the AccessManager role on the From party of the
/// assignment being changed, and to hold the package or resource being granted or revoked.
/// </remarks>
[IntegrationTest]
public class AssignmentServiceAccessManagerGuardTest : IClassFixture<ApiFixture>
{
    private static readonly AuditValues TestAudit = new(SystemEntityConstants.StaticDataIngest, SystemEntityConstants.StaticDataIngest);

    private static readonly Entity Organization = new()
    {
        Id = Guid.Parse("01970002-0000-7000-8000-000000000001"),
        TypeId = EntityTypeConstants.Organization,
        VariantId = EntityVariantConstants.AS,
        Name = "Guard Test Org",
        RefId = "AMGUARD-TEST-ORG-01",
    };

    private static readonly Entity AccessManagerUser = new()
    {
        Id = Guid.Parse("01970002-0000-7000-8000-000000000002"),
        TypeId = EntityTypeConstants.Person,
        VariantId = EntityVariantConstants.Person,
        Name = "Guard Test Access Manager",
        RefId = "AMGUARD-TEST-PERSON-01",
    };

    private static readonly Entity Outsider = new()
    {
        Id = Guid.Parse("01970002-0000-7000-8000-000000000003"),
        TypeId = EntityTypeConstants.Person,
        VariantId = EntityVariantConstants.Person,
        Name = "Guard Test Outsider",
        RefId = "AMGUARD-TEST-PERSON-02",
    };

    private static readonly Entity RecipientA = new()
    {
        Id = Guid.Parse("01970002-0000-7000-8000-000000000004"),
        TypeId = EntityTypeConstants.Person,
        VariantId = EntityVariantConstants.Person,
        Name = "Guard Test Recipient A",
        RefId = "AMGUARD-TEST-PERSON-03",
    };

    private static readonly Entity RecipientB = new()
    {
        Id = Guid.Parse("01970002-0000-7000-8000-000000000005"),
        TypeId = EntityTypeConstants.Person,
        VariantId = EntityVariantConstants.Person,
        Name = "Guard Test Recipient B",
        RefId = "AMGUARD-TEST-PERSON-04",
    };

    private static readonly Guid ResourceTypeId = Guid.Parse("01970002-0000-7000-8000-000000000010");
    private static readonly Guid ResourceId = Guid.Parse("01970002-0000-7000-8000-000000000011");

    /// <summary>
    /// Package held by <see cref="AccessManagerUser"/> through a rightholder assignment from <see cref="Organization"/>.
    /// </summary>
    private static readonly Guid HeldPackageId = PackageConstants.Agriculture.Id;

    /// <summary>
    /// Package deliberately not held by <see cref="AccessManagerUser"/>.
    /// </summary>
    private static readonly Guid UnheldPackageId = PackageConstants.Fishing.Id;

    private static readonly Guid AccessManagerAssignmentId = Guid.Parse("01970002-0000-7000-8000-000000000020");
    private static readonly Guid ManagerRightholderAssignmentId = Guid.Parse("01970002-0000-7000-8000-000000000021");
    private static readonly Guid TargetAssignmentAId = Guid.Parse("01970002-0000-7000-8000-000000000022");
    private static readonly Guid TargetAssignmentBId = Guid.Parse("01970002-0000-7000-8000-000000000023");

    private ApiFixture Fixture { get; }

    public AssignmentServiceAccessManagerGuardTest(ApiFixture fixture)
    {
        Fixture = fixture;
        Fixture.EnsureSeedOnce<AssignmentServiceAccessManagerGuardTest>(db =>
        {
            db.Entities.AddRange(Organization, AccessManagerUser, Outsider, RecipientA, RecipientB);
            db.SaveChanges(TestAudit);

            db.Set<ResourceType>().Add(new ResourceType { Id = ResourceTypeId, Name = "AmGuardTestResourceType" });
            db.SaveChanges(TestAudit);

            db.Set<Resource>().Add(new Resource
            {
                Id = ResourceId,
                Name = "Guard Test Resource",
                Description = "Resource used by the access manager guard tests",
                RefId = "amguard-test-resource",
                TypeId = ResourceTypeId,
                ProviderId = ProviderConstants.Altinn3.Id,
            });
            db.SaveChanges(TestAudit);

            db.Assignments.AddRange(
                new Assignment { Id = AccessManagerAssignmentId, FromId = Organization.Id, ToId = AccessManagerUser.Id, RoleId = RoleConstants.AccessManager },
                new Assignment { Id = ManagerRightholderAssignmentId, FromId = Organization.Id, ToId = AccessManagerUser.Id, RoleId = RoleConstants.Rightholder },
                new Assignment { Id = TargetAssignmentAId, FromId = Organization.Id, ToId = RecipientA.Id, RoleId = RoleConstants.Rightholder },
                new Assignment { Id = TargetAssignmentBId, FromId = Organization.Id, ToId = RecipientB.Id, RoleId = RoleConstants.Rightholder });
            db.SaveChanges(TestAudit);

            db.AssignmentPackages.AddRange(
                new AssignmentPackage { AssignmentId = ManagerRightholderAssignmentId, PackageId = HeldPackageId },
                new AssignmentPackage { AssignmentId = TargetAssignmentAId, PackageId = HeldPackageId },
                new AssignmentPackage { AssignmentId = TargetAssignmentBId, PackageId = UnheldPackageId });

            db.Set<AssignmentResource>().Add(new AssignmentResource
            {
                AssignmentId = ManagerRightholderAssignmentId,
                ResourceId = ResourceId,
                PolicyPath = "amguard/manager/policy.xml",
                PolicyVersion = "1",
            });
            db.SaveChanges(TestAudit);
        });
    }

    [Fact]
    public async Task AddAssignmentResource_UserIsAccessManagerForFromParty_PersistsAssignmentResource()
    {
        using var scope = Fixture.Services.CreateEFScope(TestAudit);
        var service = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        var result = await service.AddAssignmentResource(
            AccessManagerUser.Id,
            TargetAssignmentAId,
            ResourceId,
            "amguard/target-a/policy.xml",
            "1",
            TestContext.Current.CancellationToken);

        Assert.True(result);

        await Fixture.QueryDb(async db =>
        {
            var persisted = await db.Set<AssignmentResource>().AsNoTracking()
                .SingleOrDefaultAsync(t => t.AssignmentId == TargetAssignmentAId && t.ResourceId == ResourceId);

            Assert.NotNull(persisted);
            Assert.Equal("amguard/target-a/policy.xml", persisted.PolicyPath);
        });
    }

    [Fact]
    public async Task AddAssignmentResource_UserLacksAccessManagerOnFromParty_ThrowsUnauthorizedAccessException()
    {
        using var scope = Fixture.Services.CreateEFScope(TestAudit);
        var service = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AddAssignmentResource(
            Outsider.Id,
            TargetAssignmentBId,
            ResourceId,
            "amguard/target-b/policy.xml",
            "1",
            TestContext.Current.CancellationToken));

        await Fixture.QueryDb(async db =>
        {
            var persisted = await db.Set<AssignmentResource>().AsNoTracking()
                .AnyAsync(t => t.AssignmentId == TargetAssignmentBId && t.ResourceId == ResourceId);

            Assert.False(persisted);
        });
    }

    [Fact]
    public async Task RemoveAssignmentPackage_UserIsAccessManagerForFromPartyAndHoldsPackage_RemovesAssignmentPackage()
    {
        using var scope = Fixture.Services.CreateEFScope(TestAudit);
        var service = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        var result = await service.RemoveAssignmentPackage(
            AccessManagerUser.Id,
            TargetAssignmentAId,
            HeldPackageId,
            TestContext.Current.CancellationToken);

        Assert.True(result);

        await Fixture.QueryDb(async db =>
        {
            var remaining = await db.AssignmentPackages.AsNoTracking()
                .AnyAsync(t => t.AssignmentId == TargetAssignmentAId && t.PackageId == HeldPackageId);

            Assert.False(remaining);
        });
    }

    [Fact]
    public async Task RemoveAssignmentPackage_UserLacksAccessManagerOnFromParty_ThrowsUnauthorizedAccessException()
    {
        using var scope = Fixture.Services.CreateEFScope(TestAudit);
        var service = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RemoveAssignmentPackage(
            Outsider.Id,
            TargetAssignmentBId,
            UnheldPackageId,
            TestContext.Current.CancellationToken));

        await Fixture.QueryDb(async db =>
        {
            var remaining = await db.AssignmentPackages.AsNoTracking()
                .AnyAsync(t => t.AssignmentId == TargetAssignmentBId && t.PackageId == UnheldPackageId);

            Assert.True(remaining);
        });
    }

    [Fact]
    public async Task RemoveAssignmentPackage_UserIsAccessManagerWithoutThePackage_ThrowsUnauthorizedAccessException()
    {
        using var scope = Fixture.Services.CreateEFScope(TestAudit);
        var service = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RemoveAssignmentPackage(
            AccessManagerUser.Id,
            TargetAssignmentBId,
            UnheldPackageId,
            TestContext.Current.CancellationToken));

        await Fixture.QueryDb(async db =>
        {
            var remaining = await db.AssignmentPackages.AsNoTracking()
                .AnyAsync(t => t.AssignmentId == TargetAssignmentBId && t.PackageId == UnheldPackageId);

            Assert.True(remaining);
        });
    }
}
