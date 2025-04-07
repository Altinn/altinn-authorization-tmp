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
    /// Missing role code in DB for assignments
    /// </summary>
    /// <returns></returns>
    public static ValidationErrorDescriptor MissingRoleCodeInDb { get; }
        = _factory.Create(6, $"Missing role code.");

    /// <summary>
    /// Party does not exists in DB.
    /// </summary>
    public static ValidationErrorDescriptor MissingPartyInDb { get; }
        = _factory.Create(7, $"Missing party.");

    /// <summary>
    /// Party does not exists in DB.
    /// </summary>
    public static ValidationErrorDescriptor InvalidPartyType { get; }
        = _factory.Create(8, $"Invalid party type.");

    /// <summary>
    /// Party does not exists in DB.
    /// </summary>
    public static ValidationErrorDescriptor AssignmentAlreadyExists { get; }
        = _factory.Create(9, $"Assignment already exists.");
}
