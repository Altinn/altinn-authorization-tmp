using System.Net;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Errors;

/// <summary>
/// Validation errors for the Access Management.
/// </summary>
public static class CoreErrors
{
    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New("AM");

    /// <summary>
    /// Creates a problem descriptor describing that an inherited assignment already exists.
    /// </summary>
    /// <returns>
    /// A <see cref="ProblemDescriptor"/> indicating a conflict due to an existing inherited assignment.
    /// </returns>
    public static ProblemDescriptor AssignmentCreateFailed { get; }
        = _factory.Create(1, HttpStatusCode.BadRequest, $"Failed to create assignment.");
}
