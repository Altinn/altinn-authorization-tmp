using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core; // TooManyFailedLookupsException
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models; // for PaginatedResult
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Utils; // AuditDefaults
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System.Net.Mime;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for enduser api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/connections")]
[FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionsController(IConnectionService connectionService, IUserProfileLookupService userProfileLookupService, IEntityService entityService) : ControllerBase
{
    private IConnectionService ConnectionService { get; } = connectionService;

    private IUserProfileLookupService UserProfileLookupService { get; } = userProfileLookupService;

    private IEntityService EntityService { get; } = entityService;

    private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
    {
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organisation];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organisation, EntityTypeConstants.Person];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organisation, EntityTypeConstants.Person];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organisation, EntityTypeConstants.Person];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnections(
        [FromQuery] ConnectionInput connection,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var validFromUuid = Guid.TryParse(connection.From, out var fromUuid);
        var validToUuid = Guid.TryParse(connection.To, out var toUuid);
        _ = Guid.TryParse(connection.Party, out var partyUuid);

        var result = await ConnectionService.Get(partyUuid, validFromUuid ? fromUuid : null, validToUuid ? toUuid : null, ConfigureConnections, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Add a new rightholder connection
    /// </summary>
    [HttpPost]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddAssignment([FromQuery] ConnectionInput connection, [FromBody] PersonInput person, CancellationToken cancellationToken = default)
    {
        bool hasPersonInputIdentifiers = person is { } && !string.IsNullOrWhiteSpace(person.PersonIdentifier) && !string.IsNullOrWhiteSpace(person.LastName);

        var validationErrors = hasPersonInputIdentifiers
            ? ValidationComposer.Validate(
                AddAssignmentValidation.ValidateConnectionInputIfPersonInputPresent(connection.Party, connection.From, connection.To),
                AddAssignmentValidation.ValidatePersonInput(person.PersonIdentifier, person.LastName))
            : ValidationComposer.Validate(
                AddAssignmentValidation.ValidateConnectionInputIfPersonInputNotPresent(connection.Party, connection.From, connection.To));

        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var connectionInputToUuid);

        Guid toUuid = Guid.Empty;

        if (!hasPersonInputIdentifiers)
        {
            // Ensure provided 'to' is not a person entity
            var entity = await EntityService.GetEntity(connectionInputToUuid, cancellationToken);
            if (entity == null)
            {
                return Problems.PartyNotFound.ToActionResult();
            }

            if (entity.TypeId == EntityTypeConstants.Person.Id)
            {
                return Problems.PersonInputRequiredForPersonAssignment.ToActionResult();
            }

            toUuid = connectionInputToUuid;
        }
        else
        {
            int authUserId = AuthenticationHelper.GetUserId(HttpContext);

            string identifier = person.PersonIdentifier.Trim();
            string lastName = person.LastName.Trim();

            bool looksNumeric11 = identifier.Length == 11 && identifier.All(char.IsDigit);
            bool treatAsSsn = false;

            if (looksNumeric11)
            {
                treatAsSsn = true;
            }

            UserProfileLookup lookup = new();
            if (treatAsSsn)
            {
                lookup.Ssn = identifier;
            }
            else
            {
                lookup.Username = identifier;
            }

            try
            {
                var profile = await UserProfileLookupService.GetUserProfile(authUserId, lookup, lastName);
                if (profile == null)
                {
                    return Problems.InvalidPersonIdentifier.ToActionResult();
                }

                Guid? resolvedUuid = profile.UserUuid != Guid.Empty ? profile.UserUuid : profile.Party?.PartyUuid;
                if (!resolvedUuid.HasValue || resolvedUuid.Value == Guid.Empty)
                {
                    return Problems.PartyNotFound.ToActionResult();
                }

                toUuid = resolvedUuid.Value;
            }
            catch (TooManyFailedLookupsException)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, Problems.InvalidPersonIdentifier.ToProblemDetails());
            }
        }

        var result = await ConnectionService.AddAssignment(fromUuid, toUuid, ConfigureConnections, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove an existing rightholder connection with option to cascade delete any assignments tied to the connection
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment([FromQuery] ConnectionInput connection, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserRemoveConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        problem = await ConnectionService.RemoveAssignment(fromUuid, toUuid, cascade, ConfigureConnections, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Gets all access packages between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<PackagePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var validFromUuid = Guid.TryParse(connection.From, out var fromUuid);
        var validToUuid = Guid.TryParse(connection.To, out var toUuid);
        _ = Guid.TryParse(connection.Party, out var partyUuid);

        var result = await ConnectionService.GetPackages(partyUuid, validFromUuid ? fromUuid : null, validToUuid ? toUuid : null, ConfigureConnections, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Add package to an existing rightholder connection
    /// </summary>
    [HttpPost("accesspackages")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserAddConnectionPackage(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<Result<AssignmentPackageDto>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(fromUuid, toUuid, packageId.Value, ConfigureConnections, cancellationToken);
            }

            return await ConnectionService.AddPackage(fromUuid, toUuid, package, ConfigureConnections, cancellationToken);
        }

        var result = await AddPackage();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove package from rightholder connection
    /// </summary>
    [HttpDelete("accesspackages")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserRemoveConnectionPacakge(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<ValidationProblemInstance> RemovePackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.RemovePackage(fromUuid, toUuid, packageId.Value, ConfigureConnections, cancellationToken);
            }

            return await ConnectionService.RemovePackage(fromUuid, toUuid, package, ConfigureConnections, cancellationToken);
        }

        problem = await RemovePackage();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Delegation check of access packages, for which packages the authenticated user has permission to assign to others on behalf of the specified party.
    /// </summary>
    [HttpGet("accesspackages/delegationcheck")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<PaginatedResult<AccessPackageDto.Check>>(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckPackage([FromQuery] Guid party, [FromQuery] IEnumerable<Guid>? packageIds, [FromQuery] IEnumerable<string>? packages, CancellationToken cancellationToken = default)
    {
        async Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage()
        {
            if (packages.Any())
            {
                return await ConnectionService.CheckPackage(party, packages, packageIds, ConfigureConnections, cancellationToken);
            }

            return await ConnectionService.CheckPackage(party, packageIds, ConfigureConnections, cancellationToken);
        }

        var result = await CheckPackage();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Gets all roles between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("roles")]
    [FeatureGate("connections/roles")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<RolePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        var validFromUuid = Guid.TryParse(connection.From, out var fromUuid);
        var validToUuid = Guid.TryParse(connection.To, out var toUuid);
        _ = Guid.TryParse(connection.Party, out var partyUuid);

        ////var result = await ConnectionService.GetRoles(partyUuid, validFromUuid ? fromUuid : null, validToUuid ? toUuid : null, ConfigureConnections, cancellationToken); ToDo: Implement when role service is ready

        var result = await Task.FromResult(new Result<IEnumerable<RolePermissionDto>>(Enumerable.Empty<RolePermissionDto>()));
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Remove Altinn 2 role from connection
    /// </summary>
    [HttpDelete("roles")]
    [FeatureGate("connections/roles")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole([FromQuery] ConnectionInput connection, [FromQuery] string roleCode, CancellationToken cancellationToken = default)
    {
        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<ValidationProblemInstance> RemoveRole()
        {
            return await Task.FromResult<ValidationProblemInstance>(null); // ToDo: Implement when role service is ready
        }

        var problem = await RemoveRole();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Gets all resources between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("resources")]
    [FeatureGate("connections/resources")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<ResourcePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResources([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        var validFromUuid = Guid.TryParse(connection.From, out var fromUuid);
        var validToUuid = Guid.TryParse(connection.To, out var toUuid);
        _ = Guid.TryParse(connection.Party, out var partyUuid);

        ////var result = await ConnectionService.GetResources(partyUuid, validFromUuid ? fromUuid : null, validToUuid ? toUuid : null, ConfigureConnections, cancellationToken); ToDo: Implement when resource service is ready

        var result = await Task.FromResult(new Result<IEnumerable<ResourcePermissionDto>>(Enumerable.Empty<ResourcePermissionDto>()));
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Add resource to an existing rightholder connection
    /// </summary>
    [HttpPost("resources")]
    [FeatureGate("connections/resources")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentResourceDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddResource([FromQuery] ConnectionInput connection, [FromQuery] string resourceId, CancellationToken cancellationToken = default)
    {
        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);

        async Task<Result<AssignmentResourceDto>> AddResource()
        {
            return await Task.FromResult(new Result<AssignmentResourceDto>(new AssignmentResourceDto())); // ToDo: Implement when resource service is ready
        }

        var result = await AddResource();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove resource from rightholder connection
    /// </summary>
    [HttpDelete("resources")]
    [FeatureGate("connections/resources")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveResource([FromQuery] ConnectionInput connection, [FromQuery] string resourceId, CancellationToken cancellationToken = default)
    {
        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<ValidationProblemInstance> RemovePackage()
        {
            return await Task.FromResult<ValidationProblemInstance>(null); // ToDo: Implement when resource service is ready
        }

        var problem = await RemovePackage();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Delegation check of resources, for which resources the authenticated user has permission to assign to others on behalf of the specified party.
    /// </summary>
    [HttpGet("resources/delegationcheck")]
    [FeatureGate("connections/resources")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<PaginatedResult<ResourceCheckDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckResource([FromQuery] Guid party, [FromQuery] string resourceId, CancellationToken cancellationToken = default)
    {
        async Task<Result<IEnumerable<ResourceCheckDto>>> CheckResource()
        {
            return await Task.FromResult(new Result<IEnumerable<ResourceCheckDto>>(Enumerable.Empty<ResourceCheckDto>())); // ToDo: Implement when resource service is ready
        }

        var result = await CheckResource();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }
}
