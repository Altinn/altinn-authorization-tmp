using System;
using Altinn.Platform.Authorization.Models;

namespace Altinn.Platform.Authorization.IntegrationTests.Data;

public static class DelegationChangesTestData
{
    public static DelegationChange Default(params Action<DelegationChange>[] actions)
    {
        var data = new DelegationChange()
        {
            DelegationChangeId = 1337,
            DelegationChangeType = DelegationChangeType.Grant,
            AltinnAppId = "apps-test",
            OfferedByPartyId = 0,
            CoveredByPartyId = null,
            CoveredByUserId = null,
            PerformedByUserId = 20001336,
            BlobStoragePolicyPath = "{altinnAppId}/{offeredByPartyId}/{coveredBy}/delegationpolicy.xml",
            BlobStorageVersionId = "CorrectLeaseId",
            Created = DateTime.Now
        };

        foreach (var action in actions)
        {
            action(data);
        }

        WithBlobStorage(data);
        return data;
    }

    public static void WithBlobStorage(DelegationChange data)
    {
        var coveredBy = data.CoveredByPartyId != null ? $"p{data.CoveredByPartyId}" : $"u{data.CoveredByUserId}";
        data.BlobStoragePolicyPath = $"{data.AltinnAppId}/{data.OfferedByPartyId}/{coveredBy}/delegationpolicy.xml";
    } 

    public static Action<DelegationChange> WithChangeID(int changeID) => (delegation) => delegation.DelegationChangeId = changeID;

    public static Action<DelegationChange> WithDelegationChangeType(DelegationChangeType changeType) => (delegation) => delegation.DelegationChangeType = changeType;

    public static Action<DelegationChange> WithPerformedByUserID(int userID) => (delegation) => delegation.PerformedByUserId = userID;

    public static Action<DelegationChange> WithAltinnAppID(string appID) => (delegation) => delegation.AltinnAppId = appID;

    public static Action<DelegationChange> WithCoveredByPartyID(int partyID) => (delegation) => delegation.CoveredByPartyId = partyID;

    public static Action<DelegationChange> WithCoveredByUserID(int userID) => (delegation) => delegation.CoveredByUserId = userID;

    public static Action<DelegationChange> WithOfferedByPartyID(int partyID) => (delegation) => delegation.OfferedByPartyId = partyID;
}