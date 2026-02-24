using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Principal;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for enduser api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/connections")]
[FeatureGate(AccessMgmtFeatureFlags.EnduserControllerConnections)]
public class ConnectionsController(
    IConnectionService ConnectionService,
    IInputValidation inputValidation,
    IEntityService EntityService,
    IResourceService resourceService
    ) : ControllerBase
{
    private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
    {
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDRECTIONAL_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnections(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "from")] Guid? from,
        [FromQuery(Name = "to")] Guid? to,
        [FromQuery, FromHeader] PagingInput paging,
        [FromQuery] bool includeClientDelegations = true,
        [FromQuery] bool includeAgentConnections = true,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidationComposer.Validate(ConnectionValidation.ValidateReadConnection(party.ToString(), from?.ToString(), to?.ToString()));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var result = await ConnectionService.Get(
            party,
            from,
            to,
            includeClientDelegations,
            includeAgentConnections,
            ConfigureConnections,
            cancellationToken
        );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    #region Assignment

    /// <summary>
    /// Adds a new rightholder connection.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_WRITE_TOOTHERS)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddRightholder(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromBody] PersonInput? person,
        CancellationToken cancellationToken = default)
    {
        var entity = await inputValidation.SanitizeToInput(
            party,
            to,
            person,
            options =>
            {
                options.AllowedToEntityTypes = [EntityTypeConstants.Person, EntityTypeConstants.Organization];
                options.EntitiesToValidateForAnyConnections = [EntityTypeConstants.Person];
            },
            cancellationToken);

        if (entity.IsProblem)
        {
            return entity.Problem.ToActionResult();
        }

        var result = await ConnectionService.AddRightholder(party, entity.Value.Id, ConfigureConnections, cancellationToken);
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
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "cascade")] bool cascade = false,
        CancellationToken cancellationToken = default)
    {
        var problem = await ConnectionService.RemoveAssignment(from, to, cascade, ConfigureConnections, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    #endregion

    #region Packages

    /// <summary>
    /// Gets all access packages between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDRECTIONAL_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<PackagePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "from")] Guid? from,
        [FromQuery(Name = "to")] Guid? to,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var result = await ConnectionService.GetPackages(party, from, to, ConfigureConnections, cancellationToken);
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
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_WRITE_TOOTHERS)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAssignmentPackage(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "packageId")] Guid? packageId,
        [FromQuery(Name = "package")] string package,
        [FromBody] PersonInput? person,
        CancellationToken cancellationToken = default)
    {
        var entity = await inputValidation.SanitizeToInput(
            party,
            to,
            person,
            options =>
            {
                options.AllowedToEntityTypes = [EntityTypeConstants.Person, EntityTypeConstants.Organization];
                options.EntitiesToValidateForAnyConnections = [EntityTypeConstants.Person];
            },
            cancellationToken);

        var result = await AddPackage();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);

        async Task<Result<AssignmentPackageDto>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(party, to, packageId.Value, ConfigureConnections, cancellationToken);
            }

            return await ConnectionService.AddPackage(party, to, package, ConfigureConnections, cancellationToken);
        }
    }

    /// <summary>
    /// Remove package from rightholder connection
    /// </summary>
    [HttpDelete("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "packageId")] Guid? packageId,
        [FromQuery(Name = "package")] string package,
        CancellationToken cancellationToken = default)
    {
        var problem = await RemovePackage();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        async Task<ValidationProblemInstance> RemovePackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.RemovePackage(from, to, packageId.Value, ConfigureConnections, cancellationToken);
            }

            return await ConnectionService.RemovePackage(from, to, package, ConfigureConnections, cancellationToken);
        }

        return NoContent();
    }

    /// <summary>
    /// Delegation check of access packages, for which packages the authenticated user has permission to assign to others on behalf of the specified party.
    /// </summary>
    [HttpGet("accesspackages/delegationcheck")]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_WRITE_TOOTHERS)]
    [ProducesResponseType<PaginatedResult<AccessPackageDto.AccessPackageDtoCheck>>(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckPackage(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "packageIds")] IEnumerable<Guid>? packageIds,
        [FromQuery(Name = "packages")] IEnumerable<string>? packages,
        CancellationToken cancellationToken = default)
    {
        async Task<Result<IEnumerable<AccessPackageDto.AccessPackageDtoCheck>>> CheckPackage()
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

    #endregion

    #region Roles

    /// <summary>
    /// Gets all roles between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("roles")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDRECTIONAL_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<RolePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var result = await ConnectionService.GetRoles(party, from, to, ConfigureConnections, cancellationToken);
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
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery] string roleCode,
        CancellationToken cancellationToken = default)
    {
        return NotFound();

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
    /// Delegation check of roles, for which roles the authenticated user has permissions for on behalf of the specified party.
    /// </summary>
    [HttpGet("roles/delegationcheck")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_WRITE_TOOTHERS)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ApiExplorerSettings(IgnoreApi = true)] //// Should stay hidden/closed in APIM unless we later on need to open for role delegation for endusers
    [ProducesResponseType<PaginatedResult<RoleDtoCheck>>(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DelegationCheckRoles(
        [Required][FromQuery(Name = "party")] Guid party,
        CancellationToken cancellationToken = default)
    {
        async Task<Result<IEnumerable<RoleDtoCheck>>> CheckRoles()
        {
            return await ConnectionService.RoleDelegationCheck(party, cancellationToken: cancellationToken);
        }

        var result = await CheckRoles();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    #endregion

    #region Resources

    /// <summary>
    /// Gets all resources between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("resources")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDRECTIONAL_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<IEnumerable<ResourcePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResources(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "from")] Guid? from,
        [FromQuery(Name = "to")] Guid? to,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var result = await ConnectionService.GetResources(
            party,
            fromId: from,
            toId: to,
            resourceId: null,
            configureConnections: ConfigureConnections,
            cancellationToken: cancellationToken
            );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all resources between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("resources/rules")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDRECTIONAL_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<ExternalResourceRuleDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResources(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "resource")] string resource,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var resourceObj = await resourceService.GetResource(resource, cancellationToken);
        if (resourceObj is null)
        {
            return NotFound($"Resource '{resource}' not found.");
        }

        var result = party == from
            ? await ConnectionService.GetResourceRulesToOthers(
                partyId: party,
                toId: to,
                resourceId: resourceObj.Id,
                configureConnection: ConfigureConnections,
                cancellationToken: cancellationToken
                )
            : await ConnectionService.GetResourceRulesFromOthers(
                partyId: party,
                fromId: from,
                resourceId: resourceObj.Id,
                configureConnection: ConfigureConnections,
                cancellationToken: cancellationToken
                );

        var externalResult = new ExternalResourceRuleDto
        {
            Resource = DtoMapper.Convert(resourceObj),
            DirectRules = [],
            IndirectRules = []
        };

        foreach (var rule in result?.Rules ?? [])
        {
            if (rule.Reason.Contains(AccessReasonFlag.Direct))
            {
                RulePermission rulePermission = new()
                {
                    Rule = rule.Rule,
                    Reason = AccessReasonFlag.Direct,
                    Permissions = rule.Permissions.Where(p => p.Reason == AccessReasonFlag.Direct).ToList()
                };
                externalResult.DirectRules.Add(rulePermission);
            }

            // if the rule contains any other reason than Direct, we consider it an indirect rule and include it in the IndirectRules list
            if (rule.Reason != AccessReasonFlag.Direct)
            {
                RulePermission rulePermission = new()
                {
                    Rule = rule.Rule,
                    Reason = rule.Reason & ~AccessReasonFlag.Direct, // Remove Direct flag from reason for indirect rules
                    Permissions = rule.Permissions.Where(p => p.Reason != AccessReasonFlag.Direct).ToList()
                };
                externalResult.IndirectRules.Add(rulePermission);
            }
        }

        return Ok(externalResult);
    }

    /// <summary>
    /// Add resource to an existing rightholder connection
    /// </summary>
    [HttpPost("resources/rules")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddResourceRules(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "resource")] string resource,
        [FromBody] RuleKeyListDto actionKeys,
        CancellationToken cancellationToken = default)
    {
        var byId = AuthenticationHelper.GetPartyUuid(HttpContext);
        var fromEntity = await EntityService.GetEntity(from, cancellationToken);
        var toEntity = await EntityService.GetEntity(to, cancellationToken);
        var by = await EntityService.GetEntity(byId, cancellationToken);
        var resourceObj = await resourceService.GetResource(resource, cancellationToken);
        var result = await ConnectionService.AddResource(fromEntity, toEntity, resourceObj, actionKeys, by, ConfigureConnections, cancellationToken);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Created();
    }

    /// <summary>
    /// Update resource to an existing rightholder connection
    /// </summary>
    [HttpPut("resources/rules")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateResourceRules(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "resource")] string resource,
        [FromBody] RuleKeyListDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var byId = AuthenticationHelper.GetPartyUuid(this.HttpContext);
        var fromEntity = await EntityService.GetEntity(from, cancellationToken);
        var toEntity = await EntityService.GetEntity(to, cancellationToken);
        var byEntity = await EntityService.GetEntity(byId, cancellationToken);
        var resourceObj = await resourceService.GetResource(resource, cancellationToken);

        var result = await ConnectionService.UpdateResource(fromEntity, toEntity, resourceObj, updateDto.DirectRuleKeys, byEntity, ConfigureConnections, cancellationToken);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok();
    }

    /// <summary>
    /// Remove resource from rightholder connection and all actions
    /// </summary>
    [HttpDelete("resources")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_BIDIRECTIONAL_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveResource(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [FromQuery(Name = "resource")] string resource,
        CancellationToken cancellationToken = default)
    {
        var byId = AuthenticationHelper.GetPartyUuid(this.HttpContext);
        var problem = await ConnectionService.RemoveResource(from, to, resource, ConfigureConnections, cancellationToken);

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
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_ENDUSER_CONNECTIONS_WRITE_TOOTHERS)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<ResourceCheckDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckResource(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "resource")] string resource,
        CancellationToken cancellationToken = default)
    {
        Guid authenticatedUserUuid = AuthenticationHelper.GetPartyUuid(HttpContext);

        var result = await ConnectionService.ResourceDelegationCheck(authenticatedUserUuid, party, resource, ConfigureConnections, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    #endregion
}
