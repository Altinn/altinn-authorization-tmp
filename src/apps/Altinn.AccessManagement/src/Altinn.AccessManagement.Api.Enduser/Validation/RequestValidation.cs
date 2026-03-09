using Altinn.AccessMgmt.Core.Validation;

namespace Altinn.AccessManagement.Api.Enduser.Validation;

/// <summary>
/// Validation rules for request endpoints.
/// </summary>
internal static class RequestValidation
{
    internal static RuleExpression ValidateGetRequests(string party, string from, string to) =>
        ValidationComposer.All(
            ParameterValidation.Party(party),
            ValidationComposer.Any(ParameterValidation.PartyFrom(from), ParameterValidation.PartyTo(to)),
            ConnectionCombinationRules.PartyMatchesFromOrTo(party, from, to));

    internal static RuleExpression ValidateCreatePackageRequest(string from, string to, Guid packageId) =>
        ValidationComposer.All(
            ParameterValidation.PartyFrom(from),
            ParameterValidation.PartyTo(to),
            ConnectionCombinationRules.FromAndToMustBeDifferent(from, to),
            ParameterValidation.PackageIdNotEmpty(packageId));

    internal static RuleExpression ValidateCreateResourceRequest(string from, string to, Guid resourceId) =>
        ValidationComposer.All(
            ParameterValidation.PartyFrom(from),
            ParameterValidation.PartyTo(to),
            ConnectionCombinationRules.FromAndToMustBeDifferent(from, to),
            ParameterValidation.ResourceIdNotEmpty(resourceId));
}
