namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// CLR mirror of the <c>consent.status_type</c> Postgres enum. Members are declared in
/// database label order and map one-to-one to the labels, so the runtime enum mapping
/// and the model annotation stay identical to the live type.
/// </summary>
public enum ConsentRequestStatusType
{
    /// <summary>The <c>unopened</c> label.</summary>
    Unopened = 0,

    /// <summary>The <c>opened</c> label.</summary>
    Opened = 1,

    /// <summary>The <c>accepted</c> label.</summary>
    Accepted = 2,

    /// <summary>The <c>rejected</c> label.</summary>
    Rejected = 3,

    /// <summary>The <c>deleted</c> label.</summary>
    Deleted = 4,

    /// <summary>The <c>created</c> label.</summary>
    Created = 5,

    /// <summary>The <c>revoked</c> label.</summary>
    Revoked = 6,
}
