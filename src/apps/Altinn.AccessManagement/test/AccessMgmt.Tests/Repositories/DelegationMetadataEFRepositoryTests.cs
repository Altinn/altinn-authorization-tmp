using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.TestUtils.Factories;
using Altinn.AccessMgmt.Core.Services.Legacy;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.Host.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Altinn.AccessManagement.Tests.Repositories;

/// <summary>
/// Repository tests for <see cref="DelegationMetadataEF"/> focusing on cascading 
/// assignment revoke logic for MaskinportenSchema resources and Supplier role assignments.
/// </summary>
public class DelegationMetadataEFRepositoryTests : IAsyncLifetime
{
    private PostgresDatabase? _database;
    private ServiceProvider? _serviceProvider;

    public async ValueTask InitializeAsync()
    {
        _database = await EFPostgresFactory.Create();
        _serviceProvider = new ServiceCollection()
            .AddAccessManagementDatabase(opts =>
            {
                opts.MigrationConnectionString = _database.Admin.ToString();
                opts.Source = SourceType.Migration;
                opts.EnableEFPooling = false;
            })
            .BuildServiceProvider();
    }

    public ValueTask DisposeAsync()
    {
        _serviceProvider?.Dispose();
        NpgsqlConnection.ClearAllPools();
        return ValueTask.CompletedTask;
    }

    private AppDbContext CreateDbContext()
    {
        var audit = new AuditValues(SystemEntityConstants.StaticDataIngest);
        return _serviceProvider!.CreateEFScope(audit).ServiceProvider.GetRequiredService<AppDbContext>();
    }

    private DelegationMetadataEF CreateRepository(AppDbContext db)
    {
        var audit = new AuditValues(SystemEntityConstants.StaticDataIngest);
        var legacyRepo = new DelegationMetadataRepo(
            new NpgsqlDataSourceBuilder(_database!.User.ToString()).Build()
        );
        return new DelegationMetadataEF(
            new AuditAccessor { AuditValues = audit },
            db,
            legacyRepo
        );
    }

    private async Task<AccessMgmt.PersistenceEF.Models.ResourceType> EnsureResourceType(AppDbContext db, string typeName, CancellationToken cancellationToken = default)
    {
        var resourceType = await db.ResourceTypes.FirstOrDefaultAsync(rt => rt.Name == typeName, cancellationToken);
        if (resourceType == null)
        {
            resourceType = new AccessMgmt.PersistenceEF.Models.ResourceType
            {
                Id = Guid.CreateVersion7(),
                Name = typeName,
            };
            db.ResourceTypes.Add(resourceType);
            await db.SaveChangesAsync(cancellationToken);
        }
        return resourceType;
    }


    /// <summary>
    /// Test scenario 1: When revoking a MaskinportenSchema and it is NOT the last 
    /// AssignmentResource on the Supplier assignment, the assignment should be left behind.
    /// </summary>
    [Fact]
    public async Task InsertDelegation_RevokeMaskinportenSchema_NotLastAssignmentResource_AssignmentRemainsIntact()
    {
        // Arrange
        using var db = CreateDbContext();

        // Create test entities
        var fromOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 50001,
            Name = "Test Org From",
        };

        var toOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 60001,
            Name = "Test Org To",
        };

        var maskinportenResourceType = await EnsureResourceType(db, "MaskinportenSchema", TestContext.Current.CancellationToken);

        var maskinportenResource1 = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "maskinporten_schema_test1",
            Name = "Maskinporten Schema Test 1",
            Description = "Test maskinporten schema resource 1",
            TypeId = maskinportenResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        var maskinportenResource2 = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "maskinporten_schema_test2",
            Name = "Maskinporten Schema Test 2",
            Description = "Test maskinporten schema resource 2",
            TypeId = maskinportenResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        db.Entities.AddRange(fromOrg, toOrg);
        db.Resources.AddRange(maskinportenResource1, maskinportenResource2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create a Supplier assignment with two AssignmentResources
        var assignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = fromOrg.Id,
            ToId = toOrg.Id,
            RoleId = RoleConstants.Supplier,
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var assignmentResource1 = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = maskinportenResource1.Id,
            PolicyPath = "policy/path/1.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 1,
        };

        var assignmentResource2 = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = maskinportenResource2.Id,
            PolicyPath = "policy/path/2.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 2,
        };

        db.AssignmentResources.AddRange(assignmentResource1, assignmentResource2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create the DelegationMetadataEF repository
        var repository = CreateRepository(db);

        // Act - Revoke the first MaskinportenSchema resource
        var delegationChange = new DelegationChange
        {
            DelegationChangeType = DelegationChangeType.RevokeLast,
            ResourceId = maskinportenResource1.RefId,
            FromUuid = fromOrg.Id,
            ToUuid = toOrg.Id,
            BlobStoragePolicyPath = "policy/path/revoke.xml",
            BlobStorageVersionId = "v2",
            DelegationChangeId = 3,
        };

        var result = await repository.InsertDelegation(
            ResourceAttributeMatchType.ResourceRegistry,
            delegationChange,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(result); // RevokeLast returns null

        // Verify the AssignmentResource was deleted
        var deletedAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource1.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignmentResource);

        // Verify the assignment still exists (because there's still assignmentResource2)
        var existingAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingAssignment);

        // Verify the second AssignmentResource still exists
        var remainingAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource2.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(remainingAssignmentResource);
    }

    /// <summary>
    /// Test scenario 2: When revoking the LAST AssignmentResource for a MaskinportenSchema,
    /// the Supplier assignment should also be revoked (cascading delete).
    /// </summary>
    [Fact]
    public async Task InsertDelegation_RevokeMaskinportenSchema_LastAssignmentResource_AssignmentAlsoRevoked()
    {
        // Arrange
        using var db = CreateDbContext();

        // Create test entities
        var fromOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 50002,
            Name = "Test Org From 2",
        };

        var toOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 60002,
            Name = "Test Org To 2",
        };

        var maskinportenResourceType = await EnsureResourceType(db, "MaskinportenSchema", TestContext.Current.CancellationToken);

        var maskinportenResource = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "maskinporten_schema_last_test",
            Name = "Maskinporten Schema Last Test",
            Description = "Test maskinporten schema resource for last assignment",
            TypeId = maskinportenResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        db.Entities.AddRange(fromOrg, toOrg);
        db.Resources.Add(maskinportenResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create a Supplier assignment with only ONE AssignmentResource
        var assignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = fromOrg.Id,
            ToId = toOrg.Id,
            RoleId = RoleConstants.Supplier,
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var assignmentResource = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = maskinportenResource.Id,
            PolicyPath = "policy/path/single.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 1,
        };

        db.AssignmentResources.Add(assignmentResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create the DelegationMetadataEF repository
        var repository = CreateRepository(db);

        // Act - Revoke the last (and only) MaskinportenSchema resource
        var delegationChange = new DelegationChange
        {
            DelegationChangeType = DelegationChangeType.RevokeLast,
            ResourceId = maskinportenResource.RefId,
            FromUuid = fromOrg.Id,
            ToUuid = toOrg.Id,
            BlobStoragePolicyPath = "policy/path/revoke.xml",
            BlobStorageVersionId = "v2",
            DelegationChangeId = 2,
        };

        var result = await repository.InsertDelegation(
            ResourceAttributeMatchType.ResourceRegistry,
            delegationChange,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(result); // RevokeLast returns null

        // Verify the AssignmentResource was deleted
        var deletedAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignmentResource);

        // Verify the assignment was also deleted (cascading delete)
        var deletedAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignment);
    }

    /// <summary>
    /// Test scenario 3: When revoking a non-MaskinportenSchema resource (e.g., AltinnApp)
    /// and the assignment is NOT a Supplier assignment, the assignment should NOT be deleted
    /// even when it's the last AssignmentResource.
    /// </summary>
    [Fact]
    public async Task InsertDelegation_RevokeNonMaskinportenSchema_RightholderAssignment_AssignmentNotDeleted()
    {
        // Arrange
        using var db = CreateDbContext();

        // Create test entities
        var fromOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 50003,
            Name = "Test Org From 3",
        };

        var toOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 60003,
            Name = "Test Org To 3",
        };

        var altinnAppResourceType = await EnsureResourceType(db, "AltinnApp", TestContext.Current.CancellationToken);

        var altinnAppResource = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "app_testorg_testapp",
            Name = "Test Org Test App",
            Description = "Test Altinn app resource",
            TypeId = altinnAppResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        db.Entities.AddRange(fromOrg, toOrg);
        db.Resources.Add(altinnAppResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create a Rightholder assignment (not Supplier) with one AssignmentResource
        var assignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = fromOrg.Id,
            ToId = toOrg.Id,
            RoleId = RoleConstants.Rightholder, // Not Supplier!
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var assignmentResource = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = altinnAppResource.Id,
            PolicyPath = "policy/path/app.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 1,
        };

        db.AssignmentResources.Add(assignmentResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create the DelegationMetadataEF repository
        var repository = CreateRepository(db);

        // Act - Revoke the AltinnApp resource
        var delegationChange = new DelegationChange
        {
            DelegationChangeType = DelegationChangeType.RevokeLast,
            ResourceId = "testorg/testapp", // Will be converted to app_testorg_testapp
            FromUuid = fromOrg.Id,
            ToUuid = toOrg.Id,
            BlobStoragePolicyPath = "policy/path/revoke.xml",
            BlobStorageVersionId = "v2",
            DelegationChangeId = 2,
        };

        var result = await repository.InsertDelegation(
            ResourceAttributeMatchType.ResourceRegistry,
            delegationChange,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(result); // RevokeLast returns null

        // Verify the AssignmentResource was deleted
        var deletedAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignmentResource);

        // Verify the assignment still exists (should NOT be deleted for non-MaskinportenSchema)
        var existingAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingAssignment);
    }

    /// <summary>
    /// Test scenario 4: When revoking a MaskinportenSchema but the assignment has other 
    /// dependencies (e.g., AssignmentPackage), the assignment should NOT be deleted.
    /// </summary>
    [Fact]
    public async Task InsertDelegation_RevokeMaskinportenSchema_AssignmentHasPackageDependency_AssignmentNotDeleted()
    {
        // Arrange
        using var db = CreateDbContext();

        // Create test entities
        var fromOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 50004,
            Name = "Test Org From 4",
        };

        var toOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 60004,
            Name = "Test Org To 4",
        };

        var maskinportenResourceType = await EnsureResourceType(db, "MaskinportenSchema", TestContext.Current.CancellationToken);

        var maskinportenResource = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "maskinporten_schema_with_package",
            Name = "Maskinporten Schema With Package",
            Description = "Test maskinporten schema resource with package dependency",
            TypeId = maskinportenResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        db.Entities.AddRange(fromOrg, toOrg);
        db.Resources.Add(maskinportenResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create a Supplier assignment with one AssignmentResource and one AssignmentPackage
        var assignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = fromOrg.Id,
            ToId = toOrg.Id,
            RoleId = RoleConstants.Supplier,
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var assignmentResource = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = maskinportenResource.Id,
            PolicyPath = "policy/path/with_package.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 1,
        };

        // Add an AssignmentPackage to create a dependency
        var package = await db.Packages.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        if (package == null)
        {
            throw new InvalidOperationException("No package found in database for test.");
        }

        var assignmentPackage = new AccessMgmt.PersistenceEF.Models.AssignmentPackage
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            PackageId = package.Id,
        };

        db.AssignmentResources.Add(assignmentResource);
        db.AssignmentPackages.Add(assignmentPackage);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create the DelegationMetadataEF repository
        var repository = CreateRepository(db);

        // Act - Revoke the MaskinportenSchema resource
        var delegationChange = new DelegationChange
        {
            DelegationChangeType = DelegationChangeType.RevokeLast,
            ResourceId = maskinportenResource.RefId,
            FromUuid = fromOrg.Id,
            ToUuid = toOrg.Id,
            BlobStoragePolicyPath = "policy/path/revoke.xml",
            BlobStorageVersionId = "v2",
            DelegationChangeId = 2,
        };

        var result = await repository.InsertDelegation(
            ResourceAttributeMatchType.ResourceRegistry,
            delegationChange,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(result); // RevokeLast returns null

        // Verify the AssignmentResource was deleted
        var deletedAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignmentResource);

        // Verify the assignment still exists (because it has AssignmentPackage dependency)
        var existingAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingAssignment);

        // Verify the AssignmentPackage still exists
        var existingPackage = await db.AssignmentPackages
            .FirstOrDefaultAsync(ap => ap.Id == assignmentPackage.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingPackage);
    }

    /// <summary>
    /// Test scenario 5: When revoking a MaskinportenSchema but the assignment has delegation 
    /// dependencies, the assignment should NOT be deleted.
    /// </summary>
    [Fact]
    public async Task InsertDelegation_RevokeMaskinportenSchema_AssignmentHasDelegationDependency_AssignmentNotDeleted()
    {
        // Arrange
        using var db = CreateDbContext();

        // Create test entities
        var fromOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 50005,
            Name = "Test Org From 5",
        };

        var toOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 60005,
            Name = "Test Org To 5",
        };

        var thirdOrg = new AccessMgmt.PersistenceEF.Models.Entity
        {
            Id = Guid.CreateVersion7(),
            TypeId = EntityTypeConstants.Organization,
            VariantId = EntityVariantConstants.AS,
            PartyId = 70005,
            Name = "Test Org Third 5",
        };

        var maskinportenResourceType = await EnsureResourceType(db, "MaskinportenSchema", TestContext.Current.CancellationToken);

        var maskinportenResource = new AccessMgmt.PersistenceEF.Models.Resource
        {
            Id = Guid.CreateVersion7(),
            RefId = "maskinporten_schema_with_delegation",
            Name = "Maskinporten Schema With Delegation",
            Description = "Test maskinporten schema resource with delegation dependency",
            TypeId = maskinportenResourceType.Id,
            ProviderId = ProviderConstants.Altinn3,
        };

        db.Entities.AddRange(fromOrg, toOrg, thirdOrg);
        db.Resources.Add(maskinportenResource);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create assignments
        var assignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = fromOrg.Id,
            ToId = toOrg.Id,
            RoleId = RoleConstants.Supplier,
        };

        var onwardAssignment = new AccessMgmt.PersistenceEF.Models.Assignment
        {
            Id = Guid.CreateVersion7(),
            FromId = toOrg.Id,
            ToId = thirdOrg.Id,
            RoleId = RoleConstants.Supplier,
        };

        db.Assignments.AddRange(assignment, onwardAssignment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var assignmentResource = new AccessMgmt.PersistenceEF.Models.AssignmentResource
        {
            Id = Guid.CreateVersion7(),
            AssignmentId = assignment.Id,
            ResourceId = maskinportenResource.Id,
            PolicyPath = "policy/path/with_delegation.xml",
            PolicyVersion = "v1",
            DelegationChangeId = 1,
        };

        // Add a Delegation from the assignment
        var delegation = new AccessMgmt.PersistenceEF.Models.Delegation
        {
            Id = Guid.CreateVersion7(),
            FromId = assignment.Id,
            ToId = onwardAssignment.Id,
            FacilitatorId = toOrg.Id,
        };

        db.AssignmentResources.Add(assignmentResource);
        db.Delegations.Add(delegation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create the DelegationMetadataEF repository
        var repository = CreateRepository(db);

        // Act - Revoke the MaskinportenSchema resource
        var delegationChange = new DelegationChange
        {
            DelegationChangeType = DelegationChangeType.RevokeLast,
            ResourceId = maskinportenResource.RefId,
            FromUuid = fromOrg.Id,
            ToUuid = toOrg.Id,
            BlobStoragePolicyPath = "policy/path/revoke.xml",
            BlobStorageVersionId = "v2",
            DelegationChangeId = 2,
        };

        var result = await repository.InsertDelegation(
            ResourceAttributeMatchType.ResourceRegistry,
            delegationChange,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Null(result); // RevokeLast returns null

        // Verify the AssignmentResource was deleted
        var deletedAssignmentResource = await db.AssignmentResources
            .FirstOrDefaultAsync(ar => ar.Id == assignmentResource.Id, TestContext.Current.CancellationToken);
        Assert.Null(deletedAssignmentResource);

        // Verify the assignment still exists (because it has Delegation dependency)
        var existingAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingAssignment);

        // Verify the Delegation still exists
        var existingDelegation = await db.Delegations
            .FirstOrDefaultAsync(d => d.Id == delegation.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(existingDelegation);
    }
}
