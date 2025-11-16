using System.Net.Mime;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Controller responsible for all operations for retrieving AuthorizedParties list for a user / organization / system
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/resourceowner")]
public class ResourceOwnerAuthorizedPartiesController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IAuthorizedPartiesService _authorizedPartiesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceOwnerAuthorizedPartiesController"/> class.
    /// </summary>
    /// <param name="logger">logger service</param>
    /// <param name="mapper">mapper service</param>
    /// <param name="authorizedPartiesService">service implementation for authorized parties</param>
    public ResourceOwnerAuthorizedPartiesController(
        ILogger<ResourceOwnerAuthorizedPartiesController> logger,
        IMapper mapper,
        IAuthorizedPartiesService authorizedPartiesService)
    {
        _logger = logger;
        _mapper = mapper;
        _authorizedPartiesService = authorizedPartiesService;
    }

    /// <summary>
    /// Endpoint for retrieving all authorized parties (with option to include Authorized Parties, aka Reportees, from Altinn 2) for a given user or organization 
    /// </summary>
    /// <param name="subject">Subject input model identifying the user or organization to retrieve the list of authorized parties for</param>
    /// <param name="includeAltinn2">Optional (Default: False): Whether Authorized Parties from Altinn 2 should be included in the result set, and if access to Altinn 3 resources through having Altinn 2 roles should be included.</param>
    /// <param name="includeAltinn3">Optional (Default: True): Whether Authorized Parties from Altinn 3 should be included in the underlying result set.</param>
    /// <param name="anyOfResourceIds">Optional: List of resource Ids to filter the authorized parties on. If provided, only authorized parties with access to at least one of the provided resource Ids will be included in the result set.</param>
    /// <param name="allOfResourceIds">Optional: List of resource Ids to filter the authorized parties on. If provided, only authorized parties with access to all of the provided resource Ids will be included in the result set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <response code="200" cref="List{AuthorizedParty}">Ok</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_RESOURCEOWNER_AUTHORIZEDPARTIES)]
    [Route("authorizedparties")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<AuthorizedPartyExternal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [FeatureGate(FeatureFlags.RightsDelegationApi)]
    public async Task<ActionResult<List<AuthorizedPartyExternal>>> GetAuthorizedPartiesAsServiceOwner([FromBody] BaseAttributeExternal subject, [FromQuery] bool includeAltinn2 = false, [FromQuery] bool includeAltinn3 = true, [FromQuery] string[] anyOfResourceIds = null, [FromQuery] string[] allOfResourceIds = null, CancellationToken cancellationToken = default)
    {
        try
        {
            BaseAttribute subjectAttribute = _mapper.Map<BaseAttribute>(subject);

            var filters = new AuthorizedPartiesFilters { IncludeAltinn2 = includeAltinn2, IncludeAltinn3 = includeAltinn3, AnyOfResourceIds = anyOfResourceIds, AllOfResourceIds = allOfResourceIds };
            var isAdmin = User.FindFirst("scope")?.Value.Contains(AuthzConstants.SCOPE_AUTHORIZEDPARTIES_ADMIN);
            if (isAdmin != true)
            {
                var providerCode = User.FindFirst("urn:altinn:org")?.Value;
                if (string.IsNullOrEmpty(providerCode))
                {
                    /* 
                     * Not sure if we can introduce this as a hard limitation. Will require that all existing serviceowner use go through token exchange.
                     * Or this must be rewritten to only use organization number to identify provider.
                     */
                    ModelState.AddModelError(providerCode, "Provider code could not be determined from token for resourceowner access");
                    return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
                }

                filters.ProviderCode = providerCode;
            }

            List<AuthorizedParty> authorizedParties = await _authorizedPartiesService.GetAuthorizedParties(subjectAttribute, filters, cancellationToken);
            return _mapper.Map<List<AuthorizedPartyExternal>>(authorizedParties);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("Argument exception", ex.Message);
            return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
        }
        catch (Exception ex)
        {
            _logger.LogError(500, ex, "Unexpected internal exception occurred during ServiceOwnerGetAuthorizedParties");
            return new ObjectResult(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: "Internal Server Error"));
        }
    }
}
