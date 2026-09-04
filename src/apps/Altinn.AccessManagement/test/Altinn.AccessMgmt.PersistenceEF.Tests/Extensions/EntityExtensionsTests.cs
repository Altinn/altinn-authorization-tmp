using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Testing;

namespace Altinn.AccessMgmt.PersistenceEF.Tests.Extensions;

/// <summary>
/// Tests for <see cref="EntityExtensions"/>.
/// </summary>
[UnitTest]
public class EntityExtensionsTests
{
    [Fact]
    public void IsDeceasedPerson_PersonWithDateOfDeath_ReturnsTrue()
    {
        var entity = new Entity { TypeId = EntityTypeConstants.Person, DateOfDeath = new DateOnly(2025, 12, 7) };

        entity.IsDeceasedPerson().Should().BeTrue();
    }

    [Fact]
    public void IsDeceasedPerson_PersonWithoutDateOfDeath_ReturnsFalse()
    {
        var entity = new Entity { TypeId = EntityTypeConstants.Person, DateOfDeath = null };

        entity.IsDeceasedPerson().Should().BeFalse();
    }

    [Fact]
    public void IsDeceasedPerson_PersonWithPlaceholderDateOfDeath_ReturnsFalse()
    {
        var entity = new Entity { TypeId = EntityTypeConstants.Person, DateOfDeath = DateOnly.MinValue };

        entity.IsDeceasedPerson().Should().BeFalse();
    }

    [Fact]
    public void IsDeceasedPerson_OrganizationWithDateOfDeath_ReturnsFalse()
    {
        var entity = new Entity { TypeId = EntityTypeConstants.Organization, DateOfDeath = new DateOnly(2025, 12, 7) };

        entity.IsDeceasedPerson().Should().BeFalse();
    }
}
