#nullable enable

using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Errors;

/// <summary>
/// Validation errors for the Access Management.
/// </summary>
public static class ValidationErrors
{
    private static readonly ValidationErrorDescriptorFactory _factory
        = ValidationErrorDescriptorFactory.New("AM");

    /// <summary>
    /// The field is required.
    /// </summary>
    public static ValidationErrorDescriptor Required => StdValidationErrors.Required;

    /// <summary>
    /// Gets a validation error descriptor for when an invalid party URN is provided.
    /// </summary>
    public static ValidationErrorDescriptor InvalidPartyUrn { get; }
        = _factory.Create(1, "Invalid party URN.");

    /// <summary>
    /// Gets a validation error descriptor for when an invalid Resource URN is provided.
    /// </summary>
    public static ValidationErrorDescriptor InvalidResource { get; }
        = _factory.Create(2, $"Resource must be valid.");

    /// <summary>
    /// Gets a validation error descriptor for when a resource is missing policy file
    /// </summary>
    public static ValidationErrorDescriptor MissingPolicy { get; }
        = _factory.Create(3, $"Resource must have a policy.");

    /// <summary>
    /// Gets a validation error descriptor for when a Resource not has any delegable rights for the app
    /// </summary>
    public static ValidationErrorDescriptor MissingDelegableRights { get; }
        = _factory.Create(4, $"The Resource must have a policy giving the app rights to delegate at least one right.");

    /// <summary>
    /// Gets a validation error descriptor for when a Resource not has any delegable rights for the app
    /// </summary>
    public static ValidationErrorDescriptor ToManyDelegationsToRevoke { get; }
        = _factory.Create(5, $"There is to many policy files to update. Must delete individual delegations.");

    /// <summary>
    /// Missing party.
    /// </summary>
    public static ValidationErrorDescriptor MissingPartyInDb { get; }
        = _factory.Create(6, $"Missing party.");

    /// <summary>
    /// Invalid party type.
    /// </summary>
    public static ValidationErrorDescriptor InvalidPartyType { get; }
        = _factory.Create(7, $"Invalid party type.");

    /// <summary>
    /// Assignment already exists.
    /// </summary>
    public static ValidationErrorDescriptor AssignmentAlreadyExists { get; }
        = _factory.Create(8, $"Assignment already exists.");

    /// <summary>
    /// Assignment do not exists.
    /// </summary>
    public static ValidationErrorDescriptor AssignmentDoNotExists { get; }
        = _factory.Create(9, $"Assignment do not exist.");

    /// <summary>
    /// Assignment is active in one or more delegations.
    /// </summary>
    public static ValidationErrorDescriptor AssignmentIsActiveInOneOrMoreDelegations { get; }
        = _factory.Create(10, $"Assignment is active in one or more delegations.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor NotAuthorizedForConsentRequest { get; }
        = _factory.Create(19, "Not Authorized for ");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor InvalidPersonIdentifier { get; }
        = _factory.Create(20, "Invalid person identifier. No person with this identifier.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor InvalidOrganizationIdentifier { get; }
        = _factory.Create(21, $"Invalid organization identifier. No organization with this identifier.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor InvalidValidToTime { get; }
        = _factory.Create(22, $"The ValidTo time need to be in the future");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor MissingConsentRight { get; }
        = _factory.Create(23, $"The consentrequest needs to include at least 1 right");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor InvalidConsentResource { get; }
        = _factory.Create(24, $"Invalid resource for consent right.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor UnknownConsentMetadata { get; }
        = _factory.Create(25, $"Unknown consent metaddata.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor MissingMetadataValue { get; }
        = _factory.Create(26, $"Missing value for metadata");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor MissingMetadata { get; }
        = _factory.Create(27, $"Missing required metadata for consentright");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor MissingAction { get; }
        = _factory.Create(28, $"Missing required actions for consent request");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor MissMatchConsentParty { get; }
    = _factory.Create(29, $"The consented party does not match the party requested");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentNotFound { get; }
    = _factory.Create(30, $"Incorrect consentId or wrong consent receiver");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentExpired { get; }
    = _factory.Create(31, $"Consent is expired");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentNotAccepted { get; }
    = _factory.Create(32, $"Consent is not accepted");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentRevoked { get; }
    = _factory.Create(33, $"Consent is revoked");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentCantBeAccepted { get; }
    = _factory.Create(34, $"Consent cant be accepted. Wrong status");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentCantBeRevoked { get; }
    = _factory.Create(35, $"Consent cant be revoked. Wrong status");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentCantBeRejected { get; }
= _factory.Create(36, $"Consent cant be rejected. Wrong status");
}
