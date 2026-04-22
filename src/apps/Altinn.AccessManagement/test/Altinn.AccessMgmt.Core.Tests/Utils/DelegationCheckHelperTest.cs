using Altinn.AccessManagement.Core.Enums.ResourceRegistry;
using Altinn.AccessMgmt.Core.Utils.Helper;
using Altinn.AccessMgmt.PersistenceEF.Constants;

namespace Altinn.AccessMgmt.Core.Tests.Utils;

/// <summary>
/// Pure unit tests for <see cref="DelegationCheckHelper.IsAccessListModeEnabledAndApplicable"/>.
/// </summary>
public class DelegationCheckHelperTest
{
    // ── IsAccessListModeEnabledAndApplicable ─────────────────────────────────

    [Fact]
    public void IsAccessListModeEnabledAndApplicable_EnabledAndOrg_ReturnsTrue()
    {
        var result = DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(
            ResourceAccessListMode.Enabled,
            EntityTypeConstants.Organization.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccessListModeEnabledAndApplicable_DisabledAndOrg_ReturnsFalse()
    {
        var result = DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(
            ResourceAccessListMode.Disabled,
            EntityTypeConstants.Organization.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccessListModeEnabledAndApplicable_EnabledAndPerson_ReturnsFalse()
    {
        var result = DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(
            ResourceAccessListMode.Enabled,
            EntityTypeConstants.Person.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccessListModeEnabledAndApplicable_DisabledAndPerson_ReturnsFalse()
    {
        var result = DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(
            ResourceAccessListMode.Disabled,
            EntityTypeConstants.Person.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccessListModeEnabledAndApplicable_EnabledAndEmptyGuid_ReturnsFalse()
    {
        var result = DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(
            ResourceAccessListMode.Enabled,
            Guid.Empty);

        result.Should().BeFalse();
    }
}
