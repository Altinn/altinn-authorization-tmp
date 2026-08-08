namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// CLR mirror of the <c>consent.event_type</c> Postgres enum. Members are declared in
/// database label order and map one-to-one to the labels, so the runtime enum mapping
/// and the model annotation stay identical to the live type.
/// </summary>
public enum ConsentRequestEventType
{
    /// <summary>The <c>accepted</c> label.</summary>
    Accepted = 0,

    /// <summary>The <c>rejected</c> label.</summary>
    Rejected = 1,

    /// <summary>The <c>deleted</c> label.</summary>
    Deleted = 2,

    /// <summary>The <c>created</c> label.</summary>
    Created = 3,

    /// <summary>The <c>revoked</c> label.</summary>
    Revoked = 4,

    /// <summary>The <c>used</c> label.</summary>
    Used = 5,
}
