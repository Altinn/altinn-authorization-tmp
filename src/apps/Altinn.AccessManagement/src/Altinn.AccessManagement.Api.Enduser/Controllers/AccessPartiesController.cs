using System.Net.Mime;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Filters;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.Authorization.Host.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for end user api operations for parties which have provided access to the user or it's organizations.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/access/parties")]
[FeatureGate(EnduserFeatureFlags.ControllerAccessParties)]
public class AccessPartiesController(IHttpContextAccessor accessor, IAssignmentService assignmentService) : ControllerBase
{
    private IHttpContextAccessor Accessor { get; } = accessor;

    private IAssignmentService AssignmentService { get; } = assignmentService;

    /// <summary>
    /// Get access parties
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> GetAccessParties()
    {
        return await Task.FromResult(Ok());
    }

    /// <summary>
    /// Somestuff
    /// </summary>
    /// <param name="party">Identifies the selected party the authenticated user is acting on behalf of.</param>
    /// <param name="to">Identifies the party authenticated user waants to create an assignment to</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType<Assignment2>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> PostAccessParty([FromQuery] Guid party, [FromQuery] Guid to)
    {
        // When history is enabled pass partyUuid downstream to GetOrCreateAssignment
        // var partyUuid = Accessor.GetPartyUuid();
        var assignment = await AssignmentService.GetOrCreateAssignmenteTest(party, to, "rightholders");

        return ServiceResultFactory.CreateActionResult(assignment.MapContent(MapAssignment))
            .Convert();
    }

    public Assignment2 MapAssignment(Assignment assignment)
    {
        return new()
        {
            Id = assignment.Id,
            ToId = assignment.ToId,
            FromId = assignment.FromId,
            RoleId = assignment.RoleId,
        };
    }

    public class Assignment2
    {
        /// <summary>
        /// Identity
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// RoleId
        /// </summary>
        [JsonPropertyName("roleId")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// FromId
        /// </summary>
        [JsonPropertyName("fromId")]
        public Guid FromId { get; set; }

        /// <summary>
        /// ToId
        /// </summary>
        [JsonPropertyName("toId")]
        public Guid ToId { get; set; }
    }
}
