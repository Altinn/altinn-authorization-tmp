using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessMgmt.Core.Utils.Models;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.Authorization.Api.Contracts.Register;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Api.Enduser.Validation;

/// <summary>
/// Validation rules for individual connection query parameters.
/// Keywords (e.g. 'me', 'all') are recognized for forward compatibility, but are not yet active.
/// Clients should currently only send GUID values.
/// </summary>
internal static class ConnectionParameterRules
{
    private static readonly string[] PartyKeywords = ["me"];
    private static readonly string[] FromToPartyKeywords = ["me", "all"];

    /// <summary>
    /// party must be either keyword 'me' or a non-empty Guid.
    /// </summary>
    internal static RuleExpression Party(string value, string paramName = "party") => () =>
    {
        var trimmed = value?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            if (IsKeyword(trimmed, PartyKeywords))
            {
                return null;
            }

            if (Guid.TryParse(trimmed, out var parsed) && parsed != Guid.Empty)
            {
                return null;
            }
        }

        return (ref ValidationErrorBuilder errors) =>
            errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new(paramName, ValidationMessageTexts.InvalidPartyValue)]);
    };

    /// <summary>
    /// from can be: 'me', 'all', blank, or a non-empty Guid.
    /// </summary>
    internal static RuleExpression PartyFrom(string value, string paramName = "from") =>
        ValidateFromOrTo(value, paramName);

    /// <summary>
    /// to can be: 'me', 'all', blank, or a non-empty Guid.
    /// </summary>
    internal static RuleExpression PartyTo(string value, string paramName = "to") =>
        ValidateFromOrTo(value, paramName);

    /// <summary>
    /// personIdentifier must be present and pass <see cref="PersonIdentifier.TryParse(string?, IFormatProvider?, out PersonIdentifier)"/>,
    /// lastName must be non-empty.
    /// </summary>
    internal static RuleExpression PersonInput(string personIdentifier, string personLastName) => () =>
    {
        var trimmed = personIdentifier?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/personIdentifier", [new("personIdentifier", ValidationMessageTexts.PersonIdentifierRequired)]);
        }

        if (!PersonIdentifier.TryParse(trimmed, null, out _))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/personIdentifier", [new("personIdentifier", ValidationMessageTexts.PersonIdentifierInvalid)]);
        }

        if (string.IsNullOrWhiteSpace(personLastName))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/lastName", [new("lastName", ValidationMessageTexts.LastNameRequired)]);
        }

        return null;
    };

    private static RuleExpression ValidateFromOrTo(string value, string paramName) => () =>
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (IsKeyword(trimmed, FromToPartyKeywords))
        {
            return null;
        }

        if (Guid.TryParse(trimmed, out var parsed) && parsed != Guid.Empty)
        {
            return null;
        }

        return (ref ValidationErrorBuilder errors) =>
            errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new(paramName, ValidationMessageTexts.InvalidPartyFromOrToValue)]);
    };

    private static bool IsKeyword(string? value, string[] keywords) =>
        !string.IsNullOrWhiteSpace(value) &&
        Array.Exists(keywords, k => string.Equals(value, k, StringComparison.OrdinalIgnoreCase));
}
