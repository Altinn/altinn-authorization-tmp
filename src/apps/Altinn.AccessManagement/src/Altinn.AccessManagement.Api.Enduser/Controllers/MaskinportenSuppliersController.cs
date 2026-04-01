using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Audit;
using Altinn.AccessMgmt.Core.Extensions;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for enduser api operations for Maskinporten supplier connections and scope delegations.
/// Restricted to organizations only and MaskinportenSchema resources.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/maskinportensuppliers")]
public class MaskinportenSuppliersController(
    IConnectionService connectionService,
    IEntityService entityService,
    IResourceService resourceService
    ) : ControllerBase
{
    private Action<ConnectionOptions> ConfigureSuppliers { get; } = options =>
    {
        // Organizations only - both party and supplier must be organizations
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    #region Supplier Management

    /// <summary>
    /// Adds a new supplier connection for Maskinporten scope delegation.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddSupplier(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "supplier")] string supplierOrgNo,
        CancellationToken cancellationToken = default)
    {
        // Resolve supplier entity from organization number
        var supplierEntity = await entityService.GetByOrgNo(supplierOrgNo, cancellationToken);
        if (supplierEntity is null)
        {
            ProblemDetails problem = Core.Errors.Problems.PartyNotFound.ToProblemDetails();
            problem.Extensions["supplier"] = supplierOrgNo;
            return problem.ToActionResult();
        }

        var result = await connectionService.AddSupplier(party, supplierEntity.Id, ConfigureSuppliers, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all supplier connections for the specified party.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSuppliers(
        [Required][FromQuery(Name = "party")] Guid party,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidationComposer.Validate(
            ParameterValidation.Party(party.ToString()));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var result = await connectionService.GetSuppliers(
            party,
            ConfigureSuppliers,
            cancellationToken
        );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Remove an existing supplier connection with cascade delete of any delegated scopes.
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveSupplier(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "supplier")] string supplierOrgNo,
        CancellationToken cancellationToken = default)
    {
        // Resolve supplier entity from organization number
        var supplierEntity = await entityService.GetByOrgNo(supplierOrgNo, cancellationToken);
        if (supplierEntity is null)
        {
            return NoContent(); // Already removed or doesn't exist
        }

        var problem = await connectionService.RemoveSupplier(party, supplierEntity.Id, cascade: true, ConfigureSuppliers, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    #endregion

    #region Scope Delegation

    /// <summary>
    /// Delegation check for Maskinporten scopes (MaskinportenSchema resources).
    /// </summary>
    [HttpGet("resources/delegationcheck")]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [ProducesResponseType<ResourceCheckDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckMaskinportenScope(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "resource")] string resource,
        CancellationToken cancellationToken = default)
    {
        Guid authenticatedUserUuid = AuthenticationHelper.GetPartyUuid(HttpContext);
        string languageCode = this.GetLanguageCode();

        // Validate that resource is MaskinportenSchema type
        var resourceObj = await resourceService.GetResource(resource, cancellationToken);
        if (resourceObj is null || !resourceObj.Type.Name.Equals("MaskinportenSchema", StringComparison.InvariantCultureIgnoreCase))
        {
            ProblemDetails problem = Core.Errors.Problems.InvalidResource.ToProblemDetails();
            problem.Extensions["resource"] = resource;
            problem.Extensions["message"] = "Resource must be of type MaskinportenSchema";
            return problem.ToActionResult();
        }

        var result = await connectionService.ResourceDelegationCheck(
            authenticatedUserUuid,
            party,
            resource,
            ConfigureSuppliers,
            languageCode,
            ignoreDelegableFlag: false,
            cancellationToken: cancellationToken);

        if (result.IsProblem)
        {
            if (result.Problem.Equals(Core.Errors.Problems.InvalidResource))
            {
                ProblemDetails problem = result.Problem.ToProblemDetails();
                problem.Extensions["resource"] = resource;
                return problem.ToActionResult();
            }

            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delegate a Maskinporten scope (MaskinportenSchema resource) to a supplier.
    /// All delegable rights from delegation check are automatically delegated.
    /// </summary>
    [HttpPost("resources")]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DelegateMaskinportenScope(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "supplier")] string supplierOrgNo,
        [Required][FromQuery(Name = "resource")] string resource,
        CancellationToken cancellationToken = default)
    {
        var byId = AuthenticationHelper.GetPartyUuid(HttpContext);

        // Resolve supplier entity from organization number
        var supplierEntity = await entityService.GetByOrgNo(supplierOrgNo, cancellationToken);
        if (supplierEntity is null)
        {
            ProblemDetails problem = Core.Errors.Problems.PartyNotFound.ToProblemDetails();
            problem.Extensions["supplier"] = supplierOrgNo;
            return problem.ToActionResult();
        }

        // Validate that resource is MaskinportenSchema type
        var resourceObj = await resourceService.GetResource(resource, cancellationToken);
        if (resourceObj is null || !resourceObj.Type.Name.Equals("MaskinportenSchema", StringComparison.InvariantCultureIgnoreCase))
        {
            ProblemDetails problem = Core.Errors.Problems.InvalidResource.ToProblemDetails();
            problem.Extensions["resource"] = resource;
            problem.Extensions["message"] = "Resource must be of type MaskinportenSchema";
            return problem.ToActionResult();
        }

        // Perform delegation check to get delegable rights
        string languageCode = this.GetLanguageCode();
        var delegationCheck = await connectionService.ResourceDelegationCheck(
            byId,
            party,
            resource,
            ConfigureSuppliers,
            languageCode,
            ignoreDelegableFlag: false,
            cancellationToken: cancellationToken);

        if (delegationCheck.IsProblem)
        {
            return delegationCheck.Problem.ToActionResult();
        }

        // Extract all delegable right keys from delegation check
        var delegableRightKeys = delegationCheck.Value.Rights
            .Select(r => r.Right.Key)
            .ToList();
        if (!delegableRightKeys.Any())
        {
            ProblemDetails problem = Problems.NotAuthorizedForDelegationRequest.ToProblemDetails();
            problem.Extensions["message"] = "No delegable rights found for this resource";
            return problem.ToActionResult();
        }

        // Create RightKeyListDto with auto-delegated rights
        var rightKeys = new RightKeyListDto { DirectRightKeys = delegableRightKeys };

        var fromEntity = await entityService.GetEntity(party, cancellationToken);
        var by = await entityService.GetEntity(byId, cancellationToken);

        var result = await connectionService.AddMaskinportenScopeToSupplier(
            fromEntity,
            supplierEntity,
            resourceObj,
            rightKeys,
            by,
            ConfigureSuppliers,
            cancellationToken);

        if (result.IsProblem)
        {
            if (result.Problem.Equals(Core.Errors.Problems.InvalidResource))
            {
                ProblemDetails problem = result.Problem.ToProblemDetails();
                problem.Extensions["resource"] = resource;
                return problem.ToActionResult();
            }

            return result.Problem.ToActionResult();
        }

        return Created();
    }

    /// <summary>
    /// Gets all delegated Maskinporten scopes for the specified party and optional supplier filter.
    /// </summary>
    [HttpGet("resources")]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [ProducesResponseType<IEnumerable<ResourcePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDelegatedMaskinportenScopes(
        [Required][FromQuery(Name = "party")] Guid party,
        [FromQuery(Name = "supplier")] string? supplierOrgNo = null,
        [FromQuery(Name = "resource")] string? resource = null,
        CancellationToken cancellationToken = default)
    {
        Guid? supplierId = null;
        if (!string.IsNullOrEmpty(supplierOrgNo))
        {
            var supplierEntity = await entityService.GetByOrgNo(supplierOrgNo, cancellationToken);
            if (supplierEntity is null)
            {
                return Ok(Enumerable.Empty<ResourcePermissionDto>()); // No results if supplier not found
            }
            supplierId = supplierEntity.Id;
        }

        Guid? resourceId = null;
        if (resource != null)
        {
            var resourceObj = await resourceService.GetResource(resource, cancellationToken);
            if (resourceObj is null)
            {
                ProblemDetails problem = Core.Errors.Problems.InvalidResource.ToProblemDetails();
                problem.Extensions["resource"] = resource;
                return problem.ToActionResult();
            }
            resourceId = resourceObj.Id;
        }

        var result = await connectionService.GetMaskinportenScopes(
            party,
            toId: supplierId,
            resourceId: resourceId,
            configureConnections: ConfigureSuppliers,
            cancellationToken: cancellationToken
        );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove a delegated Maskinporten scope from a supplier.
    /// </summary>
    [HttpDelete("resources")]
    [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_SCOPE_DELEGATION)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveMaskinportenScope(
        [Required][FromQuery(Name = "party")] Guid party,
        [Required][FromQuery(Name = "supplier")] string supplierOrgNo,
        [Required][FromQuery(Name = "resource")] string resource,
        CancellationToken cancellationToken = default)
    {
        // Resolve supplier entity from organization number
        var supplierEntity = await entityService.GetByOrgNo(supplierOrgNo, cancellationToken);
        if (supplierEntity is null)
        {
            return NoContent(); // Already removed or doesn't exist
        }

        var problem = await connectionService.RemoveMaskinportenScopeFromSupplier(party, supplierEntity.Id, resource, ConfigureSuppliers, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    #endregion
}
