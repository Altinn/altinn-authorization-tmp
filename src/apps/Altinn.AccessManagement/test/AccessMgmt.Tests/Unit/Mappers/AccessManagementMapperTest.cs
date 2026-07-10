using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Mappers;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.ABAC.Constants;

namespace Altinn.AccessManagement.Tests.Unit.Mappers;

/// <summary>
/// Pure-logic unit tests for <see cref="AccessManagementMapper"/>, focused on the mappings that need
/// custom configuration: reconstructing the action <see cref="AttributeMatch"/> from a plain string,
/// unwrapping/wrapping urn json values, the rename from Rights to RightDelegationResults, and the
/// fields left unset because the caller fills them in separately.
/// </summary>
[UnitTest]
public class AccessManagementMapperTest
{
    [Fact]
    public void ToInternal_RightsDelegationRequestWithRights_ReconstructsActionAttributeMatch()
    {
        RightsDelegationRequestExternal external = new()
        {
            To = [new AttributeMatchExternal { Id = "urn:altinn:person:uuid", Value = "to-value" }],
            Rights =
            [
                new BaseRightExternal
                {
                    Resource = [new AttributeMatchExternal { Id = "urn:altinn:resource", Value = "app_ttd_apps-test" }],
                    Action = "read"
                }
            ]
        };

        DelegationLookup result = external.ToInternal();

        result.From.Should().BeNull();
        result.To.Should().ContainSingle().Which.Value.Should().Be("to-value");
        result.Rights.Should().ContainSingle();
        result.Rights[0].Action.Id.Should().Be(XacmlConstants.MatchAttributeIdentifiers.ActionId);
        result.Rights[0].Action.Value.Should().Be("read");
        result.Rights[0].Resource.Should().ContainSingle().Which.Value.Should().Be("app_ttd_apps-test");
    }

    [Fact]
    public void ToInternal_RevokeOfferedDelegation_LeavesFromUnset()
    {
        RevokeOfferedDelegationExternal external = new()
        {
            To = [new AttributeMatchExternal { Id = "urn:altinn:person:uuid", Value = "to-value" }],
            Rights = [new BaseRightExternal { Resource = [], Action = "read" }]
        };

        DelegationLookup result = external.ToInternal();

        result.From.Should().BeNull();
        result.To.Should().NotBeNull();
    }

    [Fact]
    public void ToInternal_RevokeReceivedDelegation_LeavesToUnset()
    {
        RevokeReceivedDelegationExternal external = new()
        {
            From = [new AttributeMatchExternal { Id = "urn:altinn:person:uuid", Value = "from-value" }],
            Rights = [new BaseRightExternal { Resource = [], Action = "read" }]
        };

        DelegationLookup result = external.ToInternal();

        result.To.Should().BeNull();
        result.From.Should().NotBeNull();
    }

    [Fact]
    public void ToInternal_RightsDelegationCheckRequest_LeavesFromUnset()
    {
        RightsDelegationCheckRequestExternal external = new()
        {
            Resource = [new AttributeMatchExternal { Id = "urn:altinn:resource", Value = "app_ttd_apps-test" }]
        };

        RightsDelegationCheckRequest result = external.ToInternal();

        result.From.Should().BeNull();
        result.Resource.Should().ContainSingle().Which.Value.Should().Be("app_ttd_apps-test");
    }

    [Fact]
    public void ToExternal_RightDelegationCheckResults_ExtractsActionValue()
    {
        List<RightDelegationCheckResult> results =
        [
            new()
            {
                RightKey = "key1",
                Resource = [new AttributeMatch { Id = "urn:altinn:resource", Value = "app_ttd_apps-test" }],
                Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = "read" },
                Status = DelegableStatus.Delegable,
                Details = []
            }
        ];

        List<RightDelegationCheckResultExternal> external = results.ToExternal();

        external.Should().ContainSingle();
        external[0].RightKey.Should().Be("key1");
        external[0].Action.Should().Be("read");
        external[0].Resource.Should().ContainSingle().Which.Value.Should().Be("app_ttd_apps-test");
    }

    [Fact]
    public void ToExternal_DelegationActionResult_RenamesRightsToRightDelegationResultsAndExtractsAction()
    {
        DelegationActionResult result = new()
        {
            From = [new AttributeMatch { Id = "urn:altinn:person:uuid", Value = "from-value" }],
            To = [new AttributeMatch { Id = "urn:altinn:person:uuid", Value = "to-value" }],
            Rights =
            [
                new()
                {
                    Resource = [new AttributeMatch { Id = "urn:altinn:resource", Value = "app_ttd_apps-test" }],
                    Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = "read" },
                    Status = DelegationStatus.Delegated,
                    Details = []
                }
            ]
        };

        RightsDelegationResponseExternal external = result.ToExternal();

        external.To.Should().ContainSingle().Which.Value.Should().Be("to-value");
        external.RightDelegationResults.Should().ContainSingle();
        external.RightDelegationResults[0].Action.Should().Be("read");
        external.RightDelegationResults[0].RightKey.Should().Be(result.Rights[0].RightKey);
    }

    [Fact]
    public void ToInternal_AppsInstanceDelegationRequestDto_UnwrapsFromAndToAndLeavesRemainderUnset()
    {
        Guid fromUuid = Guid.NewGuid();
        Guid toUuid = Guid.NewGuid();
        PartyUrn from = PartyUrn.PartyUuid.Create(fromUuid);
        PartyUrn to = PartyUrn.PartyUuid.Create(toUuid);
        ActionUrn action = ActionUrn.ActionId.Create(ActionIdentifier.CreateUnchecked("read"));

        AppsInstanceDelegationRequestDto dto = new()
        {
            From = from,
            To = to,
            Rights =
            [
                new RightDto
                {
                    Resource = [],
                    Action = action
                }
            ]
        };

        AppsInstanceDelegationRequest result = dto.ToInternal();

        result.From.Should().Be(from);
        result.To.Should().Be(to);
        result.Rights.Should().ContainSingle().Which.Action.Should().Be(action);

        result.ResourceId.Should().BeNull();
        result.InstanceId.Should().BeNull();
        result.PerformedBy.Should().BeNull();
        result.InstanceDelegationMode.Should().Be(default(InstanceDelegationMode));
        result.InstanceDelegationSource.Should().Be(default(InstanceDelegationSource));
    }

    [Fact]
    public void ToInternal_BaseAttributeExternal_LeavesUrnUnset()
    {
        BaseAttributeExternal external = new() { Type = "urn:altinn:person:uuid", Value = "some-value" };

        BaseAttribute result = external.ToInternal();

        result.Type.Should().Be("urn:altinn:person:uuid");
        result.Value.Should().Be("some-value");
        result.Urn.Should().BeNull();
    }
}
