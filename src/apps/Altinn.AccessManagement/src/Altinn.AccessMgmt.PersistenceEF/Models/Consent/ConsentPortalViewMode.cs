namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// CLR mirror of the <c>consent.portal_view_mode</c> Postgres enum. Members are declared
/// in database label order and map one-to-one to the labels.
/// </summary>
public enum ConsentPortalViewMode
{
    /// <summary>The <c>hide</c> label.</summary>
    Hide = 0,

    /// <summary>The <c>show</c> label.</summary>
    Show = 1,
}
