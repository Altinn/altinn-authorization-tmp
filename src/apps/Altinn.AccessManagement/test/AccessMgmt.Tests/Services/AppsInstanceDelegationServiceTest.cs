using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Implementation;

namespace Altinn.AccessManagement.Tests.Services;

/// <summary>
/// Pure-logic unit tests for <see cref="AppsInstanceDelegationService.BuildInstanceStorageId"/>.
///
/// Regression coverage for the instance-urn translation bug (issue #3276): when the EF persistence
/// path was active (at23), every instance delegation got the Altinn Apps instance-id prefix
/// (urn:altinn:instance-id:{partyId}/) prepended. That is correct for Altinn Apps, which supply a
/// bare instance guid, but wrong for Dialogporten/Correspondence, which already supply a fully
/// qualified instance urn. The malformed value could never be matched by the PDP on authorize, so
/// Permit decisions silently became NotApplicable.
/// </summary>
public class AppsInstanceDelegationServiceTest
{
    private const int PartyId = 50083510;

    [Fact]
    public void BuildInstanceStorageId_BareAppInstanceGuid_QualifiesWithAppsInstanceUrnAndPartyId()
    {
        // Altinn Apps deliver a bare instance guid that must be qualified so the PDP can match it.
        var result = AppsInstanceDelegationService.BuildInstanceStorageId(PartyId, "0191579e-72bc-7977-af5d-f9e92af4393b");

        result.Should().Be("urn:altinn:instance-id:50083510/0191579e-72bc-7977-af5d-f9e92af4393b");
    }

    [Fact]
    public void BuildInstanceStorageId_DialogPortenUrn_StoredVerbatim()
    {
        // Dialogporten dialogs arrive as a complete urn and must be stored as-is (issue #3276).
        const string dialogUrn = "urn:altinn:dialog-id:019e7305-5158-708f-b91d-e9af7a06b31b";

        var result = AppsInstanceDelegationService.BuildInstanceStorageId(PartyId, dialogUrn);

        result.Should().Be(dialogUrn);
    }

    [Fact]
    public void BuildInstanceStorageId_CorrespondenceUrn_StoredVerbatim()
    {
        const string correspondenceUrn = "urn:altinn:correspondence-id:019e7305-5158-708f-b91d-e9af7a06b31b";

        var result = AppsInstanceDelegationService.BuildInstanceStorageId(PartyId, correspondenceUrn);

        result.Should().Be(correspondenceUrn);
    }

    [Fact]
    public void BuildInstanceStorageId_AlreadyQualifiedAppsUrn_NotDoubleQualified()
    {
        // Guards against re-prefixing an already qualified apps instance urn.
        const string appsUrn = AuthzConstants.InstanceUrnPrefixes.Apps + "50083510/0191579e-72bc-7977-af5d-f9e92af4393b";

        var result = AppsInstanceDelegationService.BuildInstanceStorageId(PartyId, appsUrn);

        result.Should().Be(appsUrn);
    }

    [Theory]
    [InlineData("URN:ALTINN:DIALOG-ID:019e7305-5158-708f-b91d-e9af7a06b31b")]
    [InlineData("Urn:Altinn:Correspondence-Id:019e7305-5158-708f-b91d-e9af7a06b31b")]
    public void BuildInstanceStorageId_PrefixMatchIsCaseInsensitive_StoredVerbatim(string instanceUrn)
    {
        var result = AppsInstanceDelegationService.BuildInstanceStorageId(PartyId, instanceUrn);

        result.Should().Be(instanceUrn);
    }
}
