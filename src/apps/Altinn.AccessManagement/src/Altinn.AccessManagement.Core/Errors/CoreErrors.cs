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
    /// Missing role code in DB for assignments
    /// </summary>
    /// <param name="rolecode">role code</param>
    /// <returns></returns>
    public static ProblemDescriptor MissingRoleCode(string rolecode) =>
        _factory.Create(1, HttpStatusCode.BadRequest, $"Missing role code '{rolecode}' in database.");

    /// <summary>
    /// Missing Role ID in database
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns></returns>
    public static ProblemDescriptor MissingRoleId(Guid roleId) =>
        _factory.Create(2, HttpStatusCode.BadRequest, $"Missing role ID '{roleId}' in database.");

    /// <summary>
    /// Creates a problem descriptor describing that an inherited assignment already exists.
    /// </summary>
    /// <param name="from">assignemnt from UUID.</param>
    /// <param name="to">assignemnt to UUID.</param>
    /// <param name="roleId">role UUID.</param>
    /// <returns>
    /// A <see cref="ProblemDescriptor"/> indicating a conflict due to an existing inherited assignment.
    /// </returns>
    public static ProblemDescriptor AssignmentExists(Guid from, Guid to, Guid roleId) =>
        _factory.Create(3, HttpStatusCode.Conflict, $"Inherited assignment exists from party '{from}' to party '{to}' with role ID '{roleId}.'");
}
