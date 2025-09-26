using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Validation;

/// <summary>
/// A utility class that provides methods for validating data using a series of rules.
/// </summary>
public static class EntityTypeValidation
{
    internal static RuleExpression IsOfType(EntityType entityType, IEnumerable<string> entityTypeNames, string paramName) => () =>
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(entityTypeNames);

        if (entityTypeNames.Contains(entityType.Name))
        {
            return null;
        }

        var allowedEntityTypes = string.Join(",", entityTypeNames);
        return (ref ValidationErrorBuilder errors) => errors.Add(ValidationErrors.DisallowedEntityType, $"QUERY/{paramName}", [new("type", $"Got '{entityType.Name}', expected one of: {allowedEntityTypes}.")]);
    };

    internal static RuleExpression FromIsOfType(EntityType party, params string[] entityTypeNames) => () =>
    {
        return IsOfType(party, entityTypeNames, "from")();
    };

    internal static RuleExpression ToIsOfType(EntityType party, params string[] entityTypeNames) => () =>
    {
        return IsOfType(party, entityTypeNames, "to")();
    };
}
