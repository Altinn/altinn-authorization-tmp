using Altinn.AccessManagement.Core.Errors;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Validation;

/// <summary>
/// A utility class that provides methods for validating data using a series of rules.
/// </summary>
public static class RequestValidation
{
    internal static RuleExpression RequestNotFromSelf(Guid fromId, Guid toId, string paramName = "from/to") => () =>
    {
        if (toId == fromId)
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.RequestFromSelfNotAllowed, $"QUERY/{paramName}", [new(paramName, $"Self-targeted requests are not allowed.")]
            );
        }

        return null;
    };
}
