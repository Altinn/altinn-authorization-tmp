﻿#nullable enable

using Altinn;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Utils.Models;

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
    public static ValidationErrorDescriptor EntityNotExists { get; }
        = _factory.Create(6, $"Entity do not exists.");

    /// <summary>
    /// Invalid party type.
    /// </summary>
    public static ValidationErrorDescriptor InvalidQueryParameter { get; }
        = _factory.Create(7, $"One or more query parameters are invalid.");

    /// <summary>
    /// Assignment is active in one or more delegations.
    /// </summary>
    public static ValidationErrorDescriptor AssignmentIsActiveInOneOrMoreDelegations { get; }
        = _factory.Create(10, $"Assignment is active in one or more delegations.");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor TimeNotInFuture { get; }
        = _factory.Create(22, $"The time need to be in the future");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor EmptyList { get; }
        = _factory.Create(23, $"The list cant be empty");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor ConsentNotFound { get; }
    = _factory.Create(24, $"Incorrect consentId or wrong consent receiver");

    /// <summary>
    /// Gets a validation error descriptor
    /// </summary>
    public static ValidationErrorDescriptor InvalidResourceContext { get; }
    = _factory.Create(25, $"Resource context does not match consent request rights");

    /// <summary>
    /// Gets a validation error descriptor indicating that the user is not authorized to perform the operation.
    /// </summary>
    public static ValidationErrorDescriptor UserNotAuthorized { get; }
    = _factory.Create(26, $"User not authorized for operation.");

    /// <summary>
    /// Gets a validation error descriptor indicating that the operation could not be completed.
    /// </summary>
    public static ValidationErrorDescriptor UnableToCompleteOperation { get; }
    = _factory.Create(26, $"The operation could not be completed.");
}
