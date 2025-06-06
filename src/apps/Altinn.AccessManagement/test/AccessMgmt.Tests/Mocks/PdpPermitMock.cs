using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Common.PEP.Interfaces;

namespace Altinn.AccessManagement.Tests.Mocks;

/// <summary>
/// tmp
/// </summary>
public class PdpPermitMock: IPDP
{
    /// <inheritdoc/>
    public Task<XacmlJsonResponse> GetDecisionForRequest(XacmlJsonRequestRoot xacmlJsonRequest)
    {
        var response = new XacmlJsonResponse
        {
            Response = new List<XacmlJsonResult>(new[] { new XacmlJsonResult { Decision = "Permit" } })
        };

        return Task.FromResult(response);
    }

    /// <inheritdoc/>
    public Task<bool> GetDecisionForUnvalidateRequest(XacmlJsonRequestRoot xacmlJsonRequest, ClaimsPrincipal user)
    {
        return Task.FromResult(true);
    }
}
