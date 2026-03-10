using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Contracts;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Api.ServiceOwner.Validation;

internal static class RequestValidation
{
    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to) =>
        ValidationComposer.All(
            ParameterValidation.IEntityHasId(from, "from"),
            ParameterValidation.IEntityHasId(to, "to")
        );

    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role) =>
        ValidationComposer.All(
            ParameterValidation.IEntityHasId(from, "from"),
            ParameterValidation.IEntityHasId(to, "to"),
            ParameterValidation.IEntityHasId(role, "role")
        );

    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role, Resource resource) =>
        ValidationComposer.All(
            ValidateRequestServiceInput(from, to, role),
            ParameterValidation.IEntityHasId(resource, "resource")
        );

    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role, PackageDto package) =>
        ValidationComposer.All(
            ValidateRequestServiceInput(from, to, role),
            ParameterValidation.IPackageDtoHasId(package, "package")
        );

    internal static RuleExpression ValidateRequestServiceInput(Entity from, Entity to, Role role, Resource resource, PackageDto package) =>
       ValidationComposer.All(
           ValidateRequestServiceInput(from, to, role),
           ValidationComposer.Any(
               ParameterValidation.IEntityHasId(resource, "resource"),
               ParameterValidation.IPackageDtoHasId(package, "package")
            )
       );

    internal static RuleExpression ValidateRequestInput(CreateRequestInput input) =>
        ValidationComposer.All(
            ParameterValidation.ValidFromUrnInput(input.Connection.From, ValidUrns),
            ParameterValidation.ValidToUrnInput(input.Connection.To, ValidUrns)
        );

    internal static RuleExpression ValidateRequestInput(RequestServiceOwnerQuery input) =>
       ValidationComposer.All(
           ParameterValidation.ValidFromUrnInput(input.From, ValidUrns),
           ParameterValidation.ValidToUrnInput(input.To, ValidUrns)
       );

    internal static RuleExpression ValidateRequestResource(CreateResourceRequestInput input) =>
       ValidationComposer.All(
           ValidateRequestInput(input),
           ValidateResourceInput(input.Resource)
       );

    internal static RuleExpression ValidateResourceInput(ResourceReferenceDto input) =>
      ValidationComposer.All(
          ParameterValidation.ValidResourceId(input.ResourceId)
      );

    internal static RuleExpression ValidateRequestResourceDto(RequestAssignmentResource input) =>
        ValidationComposer.All(
            ValidateAssignment(input.Assignment),
            ParameterValidation.IEntityHasId(input.Resource, "resource")
        );

    internal static RuleExpression ValidateRequestPackage(CreatePackageRequestInput input) =>
       ValidationComposer.All(
           ValidateRequestInput(input),
           ValidatePackageInput(input.Package)
       );

    internal static RuleExpression ValidatePackageInput(PackageReferenceDto input) =>
        ValidationComposer.All(
            ParameterValidation.ValidPackageUrn(input.Urn)
        );

    internal static RuleExpression ValidateRequestPackageDto(RequestAssignmentPackage input) =>
        ValidationComposer.All(
            ValidateAssignment(input.Assignment),
            ParameterValidation.IEntityHasId(input.Package, "package")
        );

    internal static RuleExpression ValidateAssignment(Assignment input) =>
       ValidationComposer.All(
           ParameterValidation.IEntityHasId(input.From, "assignment/from"),
           ParameterValidation.IEntityHasId(input.To, "assignment/to"),
           ParameterValidation.IEntityHasId(input.Role, "assignment/role")
       );

    internal static string[] ValidUrns =>
    [
        "urn:altinn:person:identifier-no",
        "urn:altinn:organization:identifier-no",
        "urn:altinn:systemuser:uuid",
        "urn:altinn:party:uuid"
    ];
}

internal static class ParameterValidation
{
    internal static RuleExpression ValidFromUrnInput(string urn, string[] validUrns) => () =>
    {
        if (!ValidUrn(urn, validUrns))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidUrn, "from", [new("from", ValidationErrorMessageTexts.FromValidUrn)]);
        }

        return null;
    };

    internal static RuleExpression ValidToUrnInput(string urn, string[] validUrns) => () =>
    {
        if (!ValidUrn(urn, validUrns))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidUrn, "to", [new("to", ValidationErrorMessageTexts.ToValidUrn)]);
        }

        return null;
    };

    internal static RuleExpression ValidResourceId(string resourceId) => () =>
    {
        if (string.IsNullOrEmpty(resourceId))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidResourceId, "resource/resourceId", [new("resource/resourceId", ValidationErrorMessageTexts.ResourceIdValid)]);
        }

        return null;
    };

    internal static RuleExpression ValidPackageUrn(string urn) => () =>
    {
        if (string.IsNullOrEmpty(urn))
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.InvalidPackageUrn, "package/urn", [new("package/urn", ValidationErrorMessageTexts.PackageUrnValid)]);
        }

        return null;
    };

    internal static RuleExpression IEntityHasId(IEntityId entity, string paramPath) => () =>
    {
        if (entity == null || entity.Id == Guid.Empty)
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.NotFound, paramPath, [new(paramPath, ValidationErrorMessageTexts.NotFound)]);
        }

        return null;
    };

    internal static RuleExpression IPackageDtoHasId(PackageDto package, string paramPath) => () =>
    {
        if (package == null || package.Id == Guid.Empty)
        {
            return (ref ValidationErrorBuilder errors) =>
                errors.Add(ValidationErrors.NotFound, paramPath, [new(paramPath, ValidationErrorMessageTexts.NotFound)]);
        }

        return null;
    };

    private static bool ValidUrn(string urn, string[] validUrn) => validUrn.Any(t => urn.StartsWith(t));
}

public static class ValidationErrors
{
    private static readonly ValidationErrorDescriptorFactory _factory = ValidationErrorDescriptorFactory.New("AM");

    public static ValidationErrorDescriptor Required => StdValidationErrors.Required;

    public static ValidationErrorDescriptor InvalidUrn { get; } = _factory.Create(1, "Invalid URN");

    public static ValidationErrorDescriptor InvalidResourceId { get; } = _factory.Create(2, "Invalid ResourceId");

    public static ValidationErrorDescriptor NotFound { get; } = _factory.Create(3, "Object not found");

    public static ValidationErrorDescriptor InvalidPackageUrn { get; } = _factory.Create(4, "Invalid Package URN");
}

internal static class ValidationErrorMessageTexts
{
    internal const string FromValidUrn = "From must be identified by valid urn";
    internal const string ToValidUrn = "To must be identified by valid urn";
    internal const string ResourceIdValid = "ResourceId must be valid";
    internal const string PackageUrnValid = "Package must be identified by valid urn";
    internal const string NotFound = "Object not found";
}
