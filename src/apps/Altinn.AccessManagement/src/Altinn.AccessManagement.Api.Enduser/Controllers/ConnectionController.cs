﻿using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Filters;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for en user api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/access/connections")]
[FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionController(IHttpContextAccessor accessor, IConnectionService connectionService, IAssignmentService assignmentService, IEntityRepository entityRepository) : ControllerBase
{
    private IHttpContextAccessor Accessor { get; } = accessor;

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="from">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="to">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType<AssignmentExternal>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnection([FromQuery] Guid party, [FromQuery] Guid? from, [FromQuery] Guid? to, CancellationToken cancellationToken = default)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return BadRequest();
        }

        if (!(from.HasValue && from.Value == party) && !(to.HasValue && to.Value == party))
        {
            // Party must match From or To
            return BadRequest();
        }

        var partyUuid = Accessor.GetPartyUuid();
        var audit = new ChangeRequestOptions()
        {
            ChangedBy = partyUuid,
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (from.HasValue && to.HasValue)
        {
            return Ok(await connectionService.GetSpecific(from.Value, to.Value));
        }

        if (from.HasValue)
        {
            return Ok(await connectionService.GetRecived(to.Value));
        }

        if (to.HasValue)
        {
            return Ok(await connectionService.GetGiven(to.Value));
        }
        
        return BadRequest();
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [Route("")]
    public async Task<IActionResult> AddConnection([FromQuery] Guid party, [FromQuery] Guid fromId, [FromQuery] Guid toId, CancellationToken cancellationToken = default)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = Accessor.GetPartyUuid(),
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (fromId != party)
        {
            throw new Exception("From party does not match from");
        }

        //// From must by Type:Organisasjon
        //// #550:AC:From party må være en organisasjon (skal ikke være mulig å legge til rightholder for privatperson el. andre entitetstyper)
        var fromEntity = await entityRepository.GetExtended(fromId);
        if (fromEntity == null)
        {
            return Problem("From party not found");
        }

        if (!fromEntity.Type.Name.Equals("Organisasjon", StringComparison.OrdinalIgnoreCase))
        {
            return Problem("From must be of type 'Organisasjon'");
        }

        //// To must be Type:Organisasjon
        //// #550:AC:Det skal bare være mulig å legge til Organisasjoner som ny Rightholder
        var toEntity = await entityRepository.GetExtended(toId);
        if (toEntity == null)
        {
            return Problem("To party not found");
        }

        if (!toEntity.Type.Name.Equals("Organisasjon", StringComparison.OrdinalIgnoreCase))
        {
            return Problem("To must be of type 'Organisasjon'");
        }

        var res = await assignmentService.GetOrCreateAssignmentInternal(fromId: fromId, toId: toId, roleCode: "rettighetshaver", options, cancellationToken: cancellationToken);

        if (res != null)
        {
            return Ok(res);
        }

        return Problem("Unable add connection");
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [Route("")]
    public async Task<ProblemInstance> RemoveConnection([FromQuery] Guid party, [FromQuery] Guid fromId, [FromQuery] Guid toId, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = Accessor.GetPartyUuid(),
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (!(fromId == party) && !(toId == party))
        {
            throw new Exception("From party does not match from or to");
        }

        //// From must by Type:Organisasjon
        //// #550:AC:From party må være en organisasjon (skal ikke være mulig å legge til rightholder for privatperson el. andre entitetstyper)
        var fromEntity = await entityRepository.GetExtended(fromId);
        if (fromEntity == null)
        {
            throw new Exception("From party not found");
        }

        if (!fromEntity.Type.Name.Equals("Organisasjon", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("From must be of type 'Organisasjon'");
        }

        return await assignmentService.DeleteAssignment(fromId: fromId, toId: toId, roleCode: "rettighetshaver", options, cascade: cascade, cancellationToken);
    }

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="fromId">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="toId">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet]
    [Route("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType<AssignmentExternal>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] Guid party, [FromQuery] Guid? fromId, [FromQuery] Guid? toId, CancellationToken cancellationToken = default)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            return BadRequest();
        }

        if (!(fromId.HasValue && fromId.Value == party) && !(toId.HasValue && toId.Value == party))
        {
            // Party must match From or To
            return BadRequest();
        }

        var audit = new ChangeRequestOptions()
        {
            ChangedBy = Accessor.GetPartyUuid(),
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        var res = await connectionService.GetPackages(fromId: fromId, toId: toId);

        return Ok(res);
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost]
    [Route("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    public async Task<IActionResult> AddPackages([FromQuery] Guid party, [FromQuery] Guid fromId, [FromQuery] Guid toId, [FromQuery] Guid? packageId, [FromQuery] string packageUrn, CancellationToken cancellationToken = default)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = Accessor.GetPartyUuid(),
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (fromId != party)
        {
            // Party must match From or To
            return BadRequest();
        }

        //// From must by Type:Organisasjon
        //// #568:AC:From party må være en Organisasjon (skal ikke være mulig å delegere fra privatperson el. andre entitetstyper enda)
        var fromEntity = await entityRepository.GetExtended(fromId);
        if (fromEntity == null)
        {
            throw new Exception("From party not found");
        }

        if (!fromEntity.Type.Name.Equals("Organisasjon", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("From must be of type 'Organisasjon'");
        }

        //// To must be Type:Organisasjon
        //// #568:AC:To party må være en Organisasjon (skal ikke være mulig å delegere fra privatperson el. andre entitetstyper enda)
        var toEntity = await entityRepository.GetExtended(toId);
        if (toEntity == null)
        {
            return Problem("To party not found");
        }

        if (!toEntity.Type.Name.Equals("Organisasjon", StringComparison.OrdinalIgnoreCase))
        {
            return Problem("To must be of type 'Organisasjon'");
        }

        if (packageId.HasValue)
        {
            var res = await connectionService.AddPackage(fromId: fromId, toId: toId, roleCode: "rettighetshaver", packageId: packageId.Value, options);
            if (res)
            {
                return Ok();
            }
        }
        else
        {
            packageUrn = packageUrn.ToLower().StartsWith("urn:") ? packageUrn : ":" + packageUrn;
            var res = await connectionService.AddPackage(fromId: fromId, toId: toId, roleCode: "rettighetshaver", packageUrn: packageUrn, options);
            if (res)
            {
                return Ok();
            }
        }

        return Problem("Unable to remove package");
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete]
    [Route("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    public async Task<IActionResult> RemovePackages([FromQuery] Guid party, [FromQuery] Guid fromId, [FromQuery] Guid toId, [FromQuery] Guid packageId, CancellationToken cancellationToken = default)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = Accessor.GetPartyUuid(),
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (!(fromId == party) && !(toId == party))
        {
            // Party must match From or To
            return BadRequest();
        }

        var res = await connectionService.RemovePackage(fromId: fromId, toId: toId, roleCode: "rettighetshaver", packageId: packageId, options);

        if (res)
        {
            return Ok();
        }

        return Problem("Unable to remove package");
    }
}
