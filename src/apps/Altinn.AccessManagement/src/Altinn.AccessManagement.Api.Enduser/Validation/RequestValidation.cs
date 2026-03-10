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
