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
    /// <returns>A deferred rule expression that yields an error builder when invalid, otherwise null.</returns>
    internal static RuleExpression Party(string value) => () =>
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (IsKeyword(value, PartyKeywords))
            {
                return null;
            }

            if (Guid.TryParse(value, out var parsed) && parsed != Guid.Empty)
            {
                return null;
            }
        }

        return (ref ValidationErrorBuilder errors) =>
            errors.Add(ValidationErrors.InvalidQueryParameter, "QUERY/party", [new("party", ValidationErrorMessageTexts.InvalidPartyValue)]);
    };

    /// <summary>
    /// <paramref name="value"/> has to be non-empty Guid if valueRequired is true.
    /// </summary>
    /// <param name="value">Raw query parameter value.</param>
    /// <param name="valueRequired">True if a value must be non-empty Guid.</param>
    /// <param name="paramName">Parameter name used in error path ("from" or "to").</param>
    /// <returns>A deferred rule expression that yields an error builder when invalid, otherwise null.</returns>
    internal static RuleExpression PartyFrom(string value, bool valueRequired = false, string paramName = "from") =>
        ValidateFromOrToParty(value, valueRequired, paramName);

    /// <summary>
    /// <paramref name="value"/> has to be non-empty Guid if valueRequired is true.
    /// </summary>
    /// <param name="value">Raw query parameter value.</param>
    /// <param name="valueRequired">True if a value must be non-empty Guid.</param>
    /// <param name="paramName">Parameter name used in error path ("from" or "to").</param>
    /// <returns>A deferred rule expression that yields an error builder when invalid, otherwise null.</returns>
    internal static RuleExpression PartyTo(string value, bool valueRequired = false, string paramName = "to") =>
        ValidateFromOrToParty(value, valueRequired, paramName);

    /// <summary>
    /// personIdentifier must be present and pass <see cref="PersonIdentifier.TryParse(string?, IFormatProvider?, out PersonIdentifier)"/>,
    /// lastName must be non-empty.
    /// </summary>
    /// <param name="personIdentifier">The personIdentifier value of a PersonInput parameter</param>
    /// <param name="personLastName">The lastName value of a PersonInput parameter</param>
    /// <returns>A deferred rule expression that yields an error builder when invalid, otherwise null.</returns>
    internal static RuleExpression PersonInput(string personIdentifier, string personLastName) => () =>
    {
        if (string.IsNullOrWhiteSpace(personIdentifier))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/personIdentifier", [new("personIdentifier", ValidationErrorMessageTexts.PersonIdentifierRequired)]);
        }

        if (!PersonIdentifier.TryParse(personIdentifier, null, out _))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/personIdentifier", [new("personIdentifier", ValidationErrorMessageTexts.PersonIdentifierInvalid)]);
        }

        if (string.IsNullOrWhiteSpace(personLastName))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidQueryParameter, "BODY/lastName", [new("lastName", ValidationErrorMessageTexts.LastNameRequired)]);
        }

        return null;
    };

    private static RuleExpression ValidateFromOrToParty(string value, bool valueRequired, string paramName) => () =>
    {
        if (valueRequired)
        {
            if (Guid.TryParse(value, out var parsed) && parsed != Guid.Empty)
            {
                return null;
            }
            else if (IsKeyword(value, FromToPartyKeywords))
            {
                return null;
            }
            else
            {
                return (ref ValidationErrorBuilder errors) =>
                    errors.Add(ValidationErrors.InvalidQueryParameter, $"QUERY/{paramName}", [new(paramName, ValidationErrorMessageTexts.InvalidPartyValue)]);
            }
        }

        return null;
    };

    private static bool IsKeyword(string? value, string[] keywords) =>
        !string.IsNullOrWhiteSpace(value) &&
        Array.Exists(keywords, k => string.Equals(value, k, StringComparison.OrdinalIgnoreCase));
}
