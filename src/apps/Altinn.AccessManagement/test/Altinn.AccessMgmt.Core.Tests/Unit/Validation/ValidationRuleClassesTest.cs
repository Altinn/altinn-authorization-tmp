using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Tests.Unit.Validation;

/// <summary>
/// Direct unit tests for the internal validation-rule factory methods in
/// <see cref="EntityValidation"/>, <see cref="EntityTypeValidation"/>,
/// <see cref="RoleValidation"/>, <see cref="AssignmentPackageValidation"/>,
/// <see cref="DelegationValidation"/> and <see cref="PackageValidation"/>.
/// </summary>
[UnitTest]
public class ValidationRuleClassesTest
{
    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>Invoke rule and return whether it produced an error (non-null ValidationRule).</summary>
    private static bool Fails(RuleExpression rule) => rule() is not null;

    private static bool Passes(RuleExpression rule) => rule() is null;

    // ── EntityValidation.ReadOp (public) ─────────────────────────────────────
    [Fact]
    public void ReadOp_PartyMatchesFrom_ReturnsNull()
    {
        var id = Guid.NewGuid().ToString();
        Passes(EntityValidation.ReadOp(id, id, Guid.NewGuid().ToString())).Should().BeTrue();
    }

    [Fact]
    public void ReadOp_PartyMatchesTo_ReturnsNull()
    {
        var id = Guid.NewGuid().ToString();
        Passes(EntityValidation.ReadOp(id, Guid.NewGuid().ToString(), id)).Should().BeTrue();
    }

    [Fact]
    public void ReadOp_PartyIsNotGuid_ReturnsError()
    {
        Fails(EntityValidation.ReadOp("not-a-guid", Guid.NewGuid().ToString(), Guid.NewGuid().ToString())).Should().BeTrue();
    }

    [Fact]
    public void ReadOp_PartyMatchesNeither_ReturnsError()
    {
        Fails(EntityValidation.ReadOp(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString())).Should().BeTrue();
    }

    [Fact]
    public void ReadOp_FromAndToEmpty_ReturnsError()
    {
        Fails(EntityValidation.ReadOp(Guid.NewGuid().ToString(), string.Empty, string.Empty)).Should().BeTrue();
    }

    // ── EntityValidation.EntityExists (internal) ─────────────────────────────
    [Fact]
    public void EntityExists_NonNullEntity_ReturnsNull()
    {
        var entity = new Entity { Id = Guid.NewGuid() };
        Passes(EntityValidation.EntityExists(entity, "party")).Should().BeTrue();
    }

    [Fact]
    public void EntityExists_NullEntity_ReturnsError()
    {
        Fails(EntityValidation.EntityExists(null, "party")).Should().BeTrue();
    }

    // ── EntityValidation.FromExists / ToExists (internal) ────────────────────
    [Fact]
    public void FromExists_NonNullEntity_ReturnsNull()
    {
        var entity = new Entity { Id = Guid.NewGuid() };
        Passes(EntityValidation.FromExists(entity)).Should().BeTrue();
    }

    [Fact]
    public void FromExists_NullEntity_ReturnsError()
    {
        Fails(EntityValidation.FromExists(null)).Should().BeTrue();
    }

    [Fact]
    public void ToExists_NonNullEntity_ReturnsNull()
    {
        var entity = new Entity { Id = Guid.NewGuid() };
        Passes(EntityValidation.ToExists(entity)).Should().BeTrue();
    }

    [Fact]
    public void ToExists_NullEntity_ReturnsError()
    {
        Fails(EntityValidation.ToExists(null)).Should().BeTrue();
    }

    // ── EntityValidation.FromIsNotTo (internal) ───────────────────────────────
    [Fact]
    public void FromIsNotTo_DifferentGuids_ReturnsNull()
    {
        Passes(EntityValidation.FromIsNotTo(Guid.NewGuid(), Guid.NewGuid())).Should().BeTrue();
    }

    [Fact]
    public void FromIsNotTo_SameGuid_ReturnsError()
    {
        var id = Guid.NewGuid();
        Fails(EntityValidation.FromIsNotTo(id, id)).Should().BeTrue();
    }

    // ── EntityTypeValidation.IsOfType (internal) ─────────────────────────────
    [Fact]
    public void IsOfType_EntityTypeInAllowedList_ReturnsNull()
    {
        var orgId = EntityTypeConstants.Organization.Id;
        Passes(EntityTypeValidation.IsOfType(orgId, [orgId], "from")).Should().BeTrue();
    }

    [Fact]
    public void IsOfType_EntityTypeNotInAllowedList_ReturnsError()
    {
        var orgId = EntityTypeConstants.Organization.Id;
        var personId = EntityTypeConstants.Person.Id;
        Fails(EntityTypeValidation.IsOfType(orgId, [personId], "from")).Should().BeTrue();
    }

    [Fact]
    public void IsOfType_UnknownGuid_ReturnsError()
    {
        Fails(EntityTypeValidation.IsOfType(Guid.NewGuid(), [Guid.NewGuid()], "from")).Should().BeTrue();
    }

    // ── EntityTypeValidation.FromIsOfType / ToIsOfType (internal) ────────────
    [Fact]
    public void FromIsOfType_MatchingType_ReturnsNull()
    {
        var orgId = EntityTypeConstants.Organization.Id;
        Passes(EntityTypeValidation.FromIsOfType(orgId, orgId)).Should().BeTrue();
    }

    [Fact]
    public void FromIsOfType_NonMatchingType_ReturnsError()
    {
        var orgId = EntityTypeConstants.Organization.Id;
        var personId = EntityTypeConstants.Person.Id;
        Fails(EntityTypeValidation.FromIsOfType(orgId, personId)).Should().BeTrue();
    }

    [Fact]
    public void ToIsOfType_MatchingType_ReturnsNull()
    {
        var personId = EntityTypeConstants.Person.Id;
        Passes(EntityTypeValidation.ToIsOfType(personId, personId)).Should().BeTrue();
    }

    [Fact]
    public void ToIsOfType_NonMatchingType_ReturnsError()
    {
        var orgId = EntityTypeConstants.Organization.Id;
        var personId = EntityTypeConstants.Person.Id;
        Fails(EntityTypeValidation.ToIsOfType(personId, orgId)).Should().BeTrue();
    }

    // ── RoleValidation.RoleExists (internal) ─────────────────────────────────
    [Fact]
    public void RoleExists_NonNullRole_ReturnsNull()
    {
        var role = new Role { Id = Guid.NewGuid() };
        Passes(RoleValidation.RoleExists(role, "role")).Should().BeTrue();
    }

    [Fact]
    public void RoleExists_NullRole_ReturnsError()
    {
        Fails(RoleValidation.RoleExists(null, "role")).Should().BeTrue();
    }

    // ── AssignmentPackageValidation.HasAssignedPackages (internal) ────────────
    [Fact]
    public void HasAssignedPackages_EmptyCollection_ReturnsNull()
    {
        Passes(AssignmentPackageValidation.HasAssignedPackages([])).Should().BeTrue();
    }

    [Fact]
    public void HasAssignedPackages_NullCollection_ReturnsNull()
    {
        Passes(AssignmentPackageValidation.HasAssignedPackages(null)).Should().BeTrue();
    }

    [Fact]
    public void HasAssignedPackages_NonEmptyCollection_ReturnsError()
    {
        var packages = new[] { new AssignmentPackage { Id = Guid.CreateVersion7() } };
        Fails(AssignmentPackageValidation.HasAssignedPackages(packages)).Should().BeTrue();
    }

    // ── DelegationValidation.HasDelegationsAssigned (internal) ───────────────
    [Fact]
    public void HasDelegationsAssigned_EmptyCollection_ReturnsNull()
    {
        Passes(DelegationValidation.HasDelegationsAssigned([])).Should().BeTrue();
    }

    [Fact]
    public void HasDelegationsAssigned_NullCollection_ReturnsNull()
    {
        Passes(DelegationValidation.HasDelegationsAssigned(null)).Should().BeTrue();
    }

    [Fact]
    public void HasDelegationsAssigned_NonEmptyCollection_ReturnsError()
    {
        var delegations = new[] { new Delegation() };
        Fails(DelegationValidation.HasDelegationsAssigned(delegations)).Should().BeTrue();
    }

    // ── PackageValidation.PackageExists (internal) ────────────────────────────
    [Fact]
    public void PackageExists_NonNullPackage_ReturnsNull()
    {
        var package = new Package { Id = Guid.NewGuid() };
        Passes(PackageValidation.PackageExists(package, "my-package")).Should().BeTrue();
    }

    [Fact]
    public void PackageExists_NullPackage_ReturnsError()
    {
        Fails(PackageValidation.PackageExists(null, "my-package")).Should().BeTrue();
    }

    // ── PackageValidation.AuthorizePackageAssignment (internal) ───────────────
    [Fact]
    public void AuthorizePackageAssignment_AllResultTrue_ReturnsNull()
    {
        var packages = new[]
        {
            new AccessPackageDto.AccessPackageDtoCheck { Package = new AccessPackageDto { Urn = "urn:altinn:accesspackage:p1" }, Result = true },
            new AccessPackageDto.AccessPackageDtoCheck { Package = new AccessPackageDto { Urn = "urn:altinn:accesspackage:p2" }, Result = true },
        };
        Passes(PackageValidation.AuthorizePackageAssignment(packages)).Should().BeTrue();
    }

    [Fact]
    public void AuthorizePackageAssignment_AnyResultFalse_ReturnsError()
    {
        var packages = new[]
        {
            new AccessPackageDto.AccessPackageDtoCheck { Package = new AccessPackageDto { Urn = "urn:altinn:accesspackage:p1" }, Result = true },
            new AccessPackageDto.AccessPackageDtoCheck { Package = new AccessPackageDto { Urn = "urn:altinn:accesspackage:p2" }, Result = false },
        };
        Fails(PackageValidation.AuthorizePackageAssignment(packages)).Should().BeTrue();
    }

    // ── PackageValidation.PackageIsAssignableTo (internal) ────────────────────
    [Fact]
    public void PackageIsAssignableTo_OrgWithMainAdminPackage_ReturnsError()
    {
        var toType = new EntityType { Id = EntityTypeConstants.Organization };
        var urns = new[] { PackageConstants.MainAdministrator.Entity.Urn };
        Fails(PackageValidation.PackageIsAssignableTo(urns, toType)).Should().BeTrue();
    }

    [Fact]
    public void PackageIsAssignableTo_OrgWithNonMainAdminPackage_ReturnsNull()
    {
        var toType = new EntityType { Id = EntityTypeConstants.Organization };
        var urns = new[] { "urn:altinn:accesspackage:some-other-package" };
        Passes(PackageValidation.PackageIsAssignableTo(urns, toType)).Should().BeTrue();
    }

    [Fact]
    public void PackageIsAssignableTo_PersonWithMainAdminPackage_ReturnsNull()
    {
        var toType = new EntityType { Id = EntityTypeConstants.Person };
        var urns = new[] { PackageConstants.MainAdministrator.Entity.Urn };
        Passes(PackageValidation.PackageIsAssignableTo(urns, toType)).Should().BeTrue();
    }

    // ── PackageValidation.ResourceIsAssignable (internal) ─────────────────────
    [Fact]
    public void ResourceIsAssignable_MaskinportenSchema_ReturnsError()
    {
        var resource = new Resource { Name = "x", Type = new ResourceType { Name = "MaskinportenSchema" } };
        Fails(PackageValidation.ResourceIsAssignable(resource)).Should().BeTrue();
    }

    [Fact]
    public void ResourceIsAssignable_OtherResourceType_ReturnsNull()
    {
        var resource = new Resource { Name = "x", Type = new ResourceType { Name = "GenericAccessResource" } };
        Passes(PackageValidation.ResourceIsAssignable(resource)).Should().BeTrue();
    }

    // ── PackageValidation.SelfAssignmentNotAllowed (internal) ─────────────────
    [Fact]
    public void SelfAssignmentNotAllowed_DifferentParties_ReturnsNull()
    {
        Passes(PackageValidation.SelfAssignmentNotAllowed(Guid.NewGuid(), Guid.NewGuid())).Should().BeTrue();
    }

    [Fact]
    public void SelfAssignmentNotAllowed_SameParty_ReturnsError()
    {
        var id = Guid.NewGuid();
        Fails(PackageValidation.SelfAssignmentNotAllowed(id, id)).Should().BeTrue();
    }

    // ── PackageValidation.PackageUrnLookup (internal) ─────────────────────────
    [Fact]
    public void PackageUrnLookup_EmptyLookupResult_ReturnsError()
    {
        Fails(PackageValidation.PackageUrnLookup(Array.Empty<Package>(), new[] { "some-package" })).Should().BeTrue();
    }

    [Fact]
    public void PackageUrnLookup_CountMismatch_ReturnsError()
    {
        var packages = new[] { new Package { Name = "pkg-a" } };
        Fails(PackageValidation.PackageUrnLookup(packages, new[] { "pkg-a", "pkg-b" })).Should().BeTrue();
    }

    [Fact]
    public void PackageUrnLookup_ExactMatch_ReturnsNull()
    {
        var packages = new[] { new Package { Name = "pkg-a" } };
        Passes(PackageValidation.PackageUrnLookup(packages, new[] { "pkg-a" })).Should().BeTrue();
    }
}
