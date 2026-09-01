using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Queries;
using Altinn.Authorization.Api.Contracts.AccessManagement.ActivityLog;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for the enduser activity log over assignments, delegations and requests.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/activitylog")]
[FeatureGate(AccessMgmtFeatureFlags.EnableEnduserActivityLogApi)]
public class ActivityLogController(IActivityLogService activityLogService) : ControllerBase
{
    private const int MaxPageSize = 1000;

    /// <summary>
    /// Get activity log entries involving the specified party, newest first. All filter
    /// parameters accept multiple values; paging follows the token in the next-link.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_ACTIVITYLOG_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<ActivityLogDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivityLog(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "type")] List<ActivityLogType> type = null,
        [FromQuery(Name = "subtype")] List<ActivityLogSubtype> subtype = null,
        [FromQuery(Name = "trigger")] List<ActivityLogTrigger> trigger = null,
        [FromQuery(Name = "status")] List<RequestStatus> status = null,
        [FromQuery(Name = "by")] List<Guid> by = null,
        [FromQuery(Name = "source")] List<Guid> source = null,
        [FromQuery(Name = "operation")] List<string> operation = null,
        [FromQuery(Name = "from")] List<Guid> from = null,
        [FromQuery(Name = "to")] List<Guid> to = null,
        [FromQuery(Name = "via")] List<Guid> via = null,
        [FromQuery(Name = "role")] List<Guid> role = null,
        [FromQuery(Name = "package")] List<Guid> package = null,
        [FromQuery(Name = "resource")] List<Guid> resource = null,
        [FromQuery(Name = "instance")] List<string> instance = null,
        [FromQuery(Name = "itemId")] List<Guid> itemId = null,
        [FromQuery(Name = "parentId")] List<Guid> parentId = null,
        [FromQuery(Name = "after")] DateTimeOffset? after = null,
        [FromQuery(Name = "before")] DateTimeOffset? before = null,
        [FromQuery(Name = "pageSize")] int pageSize = 100,
        [FromQuery(Name = "token")] string token = null,
        CancellationToken cancellationToken = default)
    {
        if (party == Guid.Empty)
        {
            ModelState.AddModelError("party", "party must be a non-empty guid.");
            return ValidationProblem(ModelState);
        }

        ActivityLogQueryCursor cursor = null;
        if (!string.IsNullOrEmpty(token) && !ActivityLogTokens.TryDecode(token, out cursor))
        {
            ModelState.AddModelError("token", "Invalid continuation token.");
            return ValidationProblem(ModelState);
        }

        var filter = new ActivityLogQueryFilter
        {
            Types = type,
            Subtypes = subtype,
            Triggers = trigger,
            Statuses = status,
            ByIds = by,
            SourceIds = source,
            OperationIds = operation,
            FromIds = from,
            ToIds = to,
            ViaIds = via,
            RoleIds = role,
            PackageIds = package,
            ResourceIds = resource,
            InstanceIds = instance,
            ItemIds = itemId,
            ParentIds = parentId,
            After = after,
            Before = before,
        };

        var result = await activityLogService.GetActivityLog(
            party,
            filter,
            Math.Clamp(pageSize, 1, MaxPageSize),
            cursor,
            cancellationToken);

        return Ok(PaginatedResult.Create(result.Items, NextLink(result.NextToken)));
    }

    private string NextLink(string nextToken)
    {
        if (nextToken is null)
        {
            return null;
        }

        var query = QueryHelpers.ParseQuery(Request.QueryString.Value);
        query["token"] = nextToken;
        return UriHelper.BuildAbsolute(Request.Scheme, Request.Host, Request.PathBase, Request.Path, QueryString.Create(query));
    }
}
