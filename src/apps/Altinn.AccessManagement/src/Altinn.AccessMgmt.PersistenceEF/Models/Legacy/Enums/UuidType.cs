namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public enum UuidType
{
    /// <summary>
    /// Placeholder when type is not specified should only happen when there is no Uuid to match it with
    /// </summary>
    NotSpecified,

    /// <summary>
    /// Defining a person this could also be identified with "Fødselsnummer"/"Dnummer"
    /// </summary>
    Person,

    /// <summary>
    /// Identifies a unit could also be identified with a Organization number
    /// </summary>
    Organization,

    /// <summary>
    /// Identifies a systemuser this is a identifier for machine integration it could also be identified with a unique name
    /// </summary>
    SystemUser,

    /// <summary>
    /// Identifies a enterpriseuser this is marked as obsolete and is used for existing integration is also identified with an unique username
    /// </summary>
    EnterpriseUser,

    /// <summary>
    /// Identifies a that this delegation is performed by a resource itself and not by a user of any type this is used when the resource performs delegations according to the flow of the resource like paralell signing
    /// </summary>
    Resource,

    /// <summary>
    /// Defining a party this could be any type from Person, Organization, SystemUser"
    /// </summary>
    Party
}
