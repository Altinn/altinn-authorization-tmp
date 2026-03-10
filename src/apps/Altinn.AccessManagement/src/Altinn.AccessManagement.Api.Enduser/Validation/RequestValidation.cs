using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;

namespace Altinn.AccessManagement.Api.Enduser.Validation;

/// <summary>
/// Validation rules for request endpoints.
/// </summary>
internal static class RequestValidation
{
    internal static RuleExpression ValidateCreateRequest(string party, CreateRequestInput requestInput) =>
        ValidationComposer.All(
            ParameterValidation.PartyFrom(requestInput.Connection.From),
            ParameterValidation.PartyTo(requestInput.Connection.To),
            ConnectionCombinationRules.FromAndToMustBeDifferent(requestInput.Connection.From, requestInput.Connection.To),
            ConnectionCombinationRules.PartyMatchesFromOrTo(party, requestInput.Connection.From, requestInput.Connection.To),
            ValidationComposer.Any(
                ParameterValidation.PackageRefNotEmpty(requestInput.Package.Urn, "package.urn"),
                ParameterValidation.ResourceRefNotEmpty(requestInput.Resource.Urn, "resource.urn")
                )
            );

    internal static RuleExpression ValidateGetRequests(string party, string from, string to) =>
       ValidationComposer.All(
           ParameterValidation.Party(party),
           ValidationComposer.Any(ParameterValidation.PartyFrom(from), ParameterValidation.PartyTo(to)),
           ConnectionCombinationRules.PartyMatchesFromOrTo(party, from, to));

    internal static RuleExpression ValidateGetSentRequests(string party, string to) =>
        ValidationComposer.All(
            ParameterValidation.Party(party),
            ParameterValidation.PartyTo(to)
        );

    /// <summary>
    /// Validate RequestService input
    /// <seealso cref="AccessMgmt.Core.Services.Contracts.IRequestService"/>
    /// </summary>
    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to) =>
       ValidationComposer.All(
           ParameterValidation.EntityHasId(from, "from"),
           ParameterValidation.EntityHasId(to, "to")
       );

    /// <summary>
    /// Validate RequestService input
    /// <seealso cref="AccessMgmt.Core.Services.Contracts.IRequestService"/>
    /// </summary>
    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role) =>
        ValidationComposer.All(
            ValidateRequestServiceInput(from, to),
            ParameterValidation.EntityHasId(role, "role")
        );

    /// <summary>
    /// Validate RequestService input
    /// <seealso cref="AccessMgmt.Core.Services.Contracts.IRequestService"/>
    /// </summary>
    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role, Resource resource, PackageDto package) =>
       ValidationComposer.All(
           ValidateRequestServiceInput(from, to, role),
           ValidationComposer.Any(
                ParameterValidation.EntityHasId(resource, "resource"),
                ParameterValidation.EntityHasId(package, "package")
            )
       );

    /// <summary>
    /// Validate RequestService input
    /// <seealso cref="AccessMgmt.Core.Services.Contracts.IRequestService"/>
    /// </summary>
    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role, Resource resource) =>
        ValidationComposer.All(
            ValidateRequestServiceInput(from, to, role),
            ParameterValidation.EntityHasId(resource, "resource")
        );
}
