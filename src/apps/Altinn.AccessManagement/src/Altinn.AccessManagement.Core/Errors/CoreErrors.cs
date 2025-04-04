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
    public static ProblemDescriptor MissingRoleCode(string rolecode) => _factory.Create(1, HttpStatusCode.BadRequest, $"Missing role code '{rolecode}' in database.");
}
