using System.Net;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Errors;

/// <summary>
/// Problem descriptors for Register.
/// </summary>
public static class Problems
{
    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New("AM");

    /// <summary>Gets a <see cref="ProblemDescriptor"/>.</summary>
    public static ProblemDescriptor NotAuthorizedForConsentRequest { get; }
        = _factory.Create(0, HttpStatusCode.Forbidden, "Not authorized for consent");

    /// <summary>Gets a <see cref="ProblemDescriptor"/>.</summary>
    public static ProblemDescriptor ConsentNotFound { get; }
    = _factory.Create(1, HttpStatusCode.NotFound, "Consent not found");

    /// <summary>Gets a <see cref="ProblemDescriptor"/>.</summary>
    public static ProblemDescriptor ConsentCantBeAccepted { get; }  
    = _factory.Create(2, HttpStatusCode.BadRequest, "Consent have wrong status to be consented");

    /// <summary>Gets a <see cref="ProblemDescriptor"/>.</summary>
    public static ProblemDescriptor InvalidOrganizationIdentifier { get; }
    = _factory.Create(3, HttpStatusCode.BadRequest, "Unknown organization. Could not be found");

    /// <summary>Gets a <see cref="ProblemDescriptor"/>.</summary>
    public static ProblemDescriptor InvalidPersonIdentifier { get; }
    = _factory.Create(4, HttpStatusCode.BadRequest, "Unknown person. Could not be found");
}
