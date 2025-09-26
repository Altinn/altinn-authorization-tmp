using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Validation;

/// <summary>
/// A utility class that provides methods for validating data using a series of rules.
/// </summary>
public static class EntityValidation
{
    internal static RuleExpression EntityExists(Entity party, string paramName) => () =>
    {
        if (party is { })
        {
            return null;
        }

        return (ref ValidationErrorBuilder errors) => errors.Add(ValidationErrors.EntityNotExists, $"QUERY/{paramName}");
    };

    internal static RuleExpression FromExists(Entity party) => () =>
    {
        return EntityExists(party, "from")();
    };

    internal static RuleExpression ToExists(Entity party) => () =>
    {
        return EntityExists(party, "to")();
    };
}
