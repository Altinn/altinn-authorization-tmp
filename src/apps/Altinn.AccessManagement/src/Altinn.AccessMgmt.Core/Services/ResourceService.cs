using System.ComponentModel.DataAnnotations;
using System.Text;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Enums.ResourceRegistry;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models.AccessList;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils.Helper;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Enums;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class ResourceService : IResourceService
{
    public AppDbContext Db { get; }

    public IAuditAccessor AuditAccessor { get; }

    public IPolicyRetrievalPoint PolicyRetrievalPoint { get; }

    public IAMPartyService PartyService { get; }

    public IContextRetrievalService ContextRetrievalService { get; }

    public IAccessListsAuthorizationClient AccessListsAuthorizationClient { get; }

    public IPolicyInformationPoint PolicyInformationPoint { get; }

    public IConnectionService ConnectionService { get; set; }

    public ResourceService(AppDbContext appDbContext, IAuditAccessor auditAccessor, IPolicyRetrievalPoint policyRetrievalPoint, IAMPartyService partyService, IContextRetrievalService contextRetrievalService, IAccessListsAuthorizationClient accessListsAuthorizationClient, IPolicyInformationPoint policyInformationPoint, IConnectionService connectionService)
    {
        Db = appDbContext;
        AuditAccessor = auditAccessor;
        PolicyRetrievalPoint = policyRetrievalPoint;
        PartyService = partyService;
        ContextRetrievalService = contextRetrievalService;
        AccessListsAuthorizationClient = accessListsAuthorizationClient;
        PolicyInformationPoint = policyInformationPoint;
        ConnectionService = connectionService;
    }

    public async ValueTask<Resource> GetResource(Guid id, CancellationToken cancellationToken = default)
    {
        return await Db.Resources.AsNoTracking().Include(t => t.Type).Include(t => t.Provider).SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async ValueTask<Resource> GetResource(string refId, CancellationToken cancellationToken = default)
    {
        return await Db.Resources.AsNoTracking().Include(t => t.Type).Include(t => t.Provider).SingleOrDefaultAsync(r => r.RefId == refId, cancellationToken);
    }

    public async ValueTask<Result<ResourceCheckDto>> DelegationCheck(Guid authenticatedUserUuid, int authenticatedUserId, int authenticationLevel, Guid party, string resourceId, CancellationToken cancellationToken = default)
    {
        // Get fromParty
        MinimalParty fromParty = await PartyService.GetByUid(party, cancellationToken);

        // Fetch policy for the resource
        XacmlPolicy policy = await GetPolicy(resourceId, cancellationToken);

        // Fetch resource
        ResourceDto resource = await FetchResource(resourceId, cancellationToken);

        // Fetch Resourcemetadata
        bool isApp = DelegationCheckHelper.IsAppResourceId(resourceId, out string org, out string app);
        ServiceResource resourceMetadata = await ContextRetrievalService.GetResourceFromResourceList(resourceId, isApp ? org : null, isApp ? app : null);
        ResourceAccessListMode accessListMode = resourceMetadata.AccessListMode;
        bool isResourceDelegable = resourceMetadata.Delegable;
        int requiredAuthenticationLevel = PolicyHelper.GetMinimumAuthenticationLevelFromXacmlPolicy(policy);

        // Decompose policy into resource/tasks
        List<ActionAccess> actionAccesses = DelegationCheckHelper.DecomposePolicy(policy);

        // Fetch packages
        var packages = await ConnectionService.CheckPackage(party, null, ConfigureConnections, cancellationToken);

        // Fetch roles

        // Fetch resource rights

        ProcessTheAccessToTheAccessKeys(actionAccesses, packages.Value);

        // Map to result
        IEnumerable<ActionDto> actions = await MapFromInternalToExternalActions(actionAccesses, resourceId, accessListMode, fromParty, requiredAuthenticationLevel, authenticationLevel, isResourceDelegable, cancellationToken);

        // build reult with reason based on roles, packages, resource rights and users delegable
        ResourceCheckDto resourceCheckDto = new ResourceCheckDto
        {
            Resource = resource,
            Actions = actions
        };
       
        return resourceCheckDto;
    }

    private string GetActionNameFromKey(string key, string resourceId)
    {
        string[] parts = key.Split("urn:", options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        StringBuilder sb = new StringBuilder();

        sb.Append(UppercaseFirstLetter(resourceId));
        sb.Append(' ');

        foreach (string part in parts)
        {
            string currentPart = part;
            if (currentPart.Substring(currentPart.Length - 1, 1) == ":")
            {
                currentPart = currentPart.Substring(0, currentPart.Length - 1);
            }

            int removeBefore = currentPart.LastIndexOf(':');
            if (removeBefore > -1)
            {
                currentPart = currentPart.Substring(currentPart.LastIndexOf(':') + 1);
            }
            
            if (currentPart.Equals(resourceId, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            sb.Append(UppercaseFirstLetter(currentPart));
            sb.Append(' ');
        }

        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }

    private string UppercaseFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return char.ToUpper(input[0]) + input.Substring(1);
    }
    
    private async Task<ActionDto> MapFromInternalToExternalAction(ActionAccess actionAccess, string resourceId, ResourceAccessListMode accessListMode, MinimalParty fromParty, int requiredAuthenticationLevel, int authenticationLevel, bool isResourceDelegable, CancellationToken cancellationToken)
    {
        if (DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(accessListMode, fromParty.PartyType))
        {
            string actionValue = actionAccess.ActionKey.Substring(actionAccess.ActionKey.LastIndexOf(":") + 1);
            AccessListAuthorizationRequest accessListAuthorizationRequest = new AccessListAuthorizationRequest
            {
                Subject = PartyUrn.PartyUuid.Create(fromParty.PartyUuid),
                Resource = ResourceIdUrn.ResourceId.Create(ResourceIdentifier.CreateUnchecked(resourceId)),
                Action = ActionUrn.ActionId.Create(ActionIdentifier.CreateUnchecked(actionValue))
            };

            AccessListAuthorizationResponse accessListAuthorizationResponse = await AccessListsAuthorizationClient.AuthorizePartyForAccessList(accessListAuthorizationRequest, cancellationToken);
            AccessListAuthorizationResult accessListAuthorizationResult = accessListAuthorizationResponse.Result;
            if (accessListAuthorizationResult != AccessListAuthorizationResult.Authorized)
            {
                actionAccess.AccessListDenied = true;
            }
        }

        ActionDto currentAction = new ActionDto
        {
            ActionKey = actionAccess.ActionKey,
            ActionName = GetActionNameFromKey(actionAccess.ActionKey, resourceId)
        };

        if (requiredAuthenticationLevel > authenticationLevel)
        {
            currentAction.Result = false;
            ActionDto.Reason reason = new ActionDto.Reason
            {
                Description = $"User authentication level {authenticationLevel} is lower than required authentication level {requiredAuthenticationLevel}",
                ReasonKey = DelegationCheckReasonCode.InsufficientAuthenticationLevel,
            };
            currentAction.Reasons = currentAction.Reasons.Append(reason);
        }
        else if (!isResourceDelegable)
        {
            currentAction.Result = false;
            ActionDto.Reason reason = new ActionDto.Reason
            {
                Description = $"Resource is not delegable",
                ReasonKey = DelegationCheckReasonCode.ResourceNotDelegable,
            };
            currentAction.Reasons = currentAction.Reasons.Append(reason);
        }
        else if (actionAccess.AccessListDenied == true)
        {
            currentAction.Result = false;
            ActionDto.Reason reason = new ActionDto.Reason
            {
                Description = "AccesList is enabled and user has no access",
                ReasonKey = DelegationCheckReasonCode.AccessListValidationFail,
            };
            currentAction.Reasons = currentAction.Reasons.Append(reason);
        }
        else if (actionAccess.PackageAllowAccess.Count == 0 && actionAccess.RoleAllowAccess.Count == 0 && actionAccess.ResourceAllowAccess.Count == 0)
        {
            currentAction.Result = false;
            List<ActionDto.Reason> reasons = new List<ActionDto.Reason>();

            reasons.AddRange(actionAccess.PackageDenyAccess);
            reasons.AddRange(actionAccess.RoleDenyAccess);
            reasons.AddRange(actionAccess.ResourceDenyAccess);
            currentAction.Reasons = reasons;
        }
        else
        {
            currentAction.Result = true;
            if (actionAccess.PackageAllowAccess.Count > 0)
            {
                currentAction.Reasons = new List<ActionDto.Reason>();

                foreach (var packageAllowAccess in actionAccess.PackageAllowAccess)
                {
                    foreach (var packageReason in packageAllowAccess.Reasons)
                    {
                        ActionDto.Reason reason = new ActionDto.Reason
                        {
                            Description = packageReason.Description,
                            ReasonKey = DelegationCheckReasonCode.PackageAccess,
                            FromName = packageReason.FromName,
                            PackageId = packageAllowAccess.Package.Id,
                            PackageUrn = packageAllowAccess.Package.Urn,
                            FromId = packageReason.FromId,
                            ToId = packageReason.ToId,
                            RoleId = packageReason.RoleId,
                            RoleUrn = packageReason.RoleUrn,
                            ToName = packageReason.ToName,
                            ViaId = packageReason.ViaId,
                            ViaName = packageReason.ViaName,
                            ViaRoleId = packageReason.ViaRoleId,
                            ViaRoleUrn = packageReason.ViaRoleUrn,
                        };

                        currentAction.Reasons = currentAction.Reasons.Append(reason);
                    }
                }
            }
        }

        return currentAction;
    }

    private async Task<IEnumerable<ActionDto>> MapFromInternalToExternalActions(List<ActionAccess> actionAccesses, string resourceId, ResourceAccessListMode accessListMode, MinimalParty fromParty, int requiredAuthenticationLevel, int authenticationLevel, bool isResourceDelegable, CancellationToken cancellationToken = default)
    {
        List<ActionDto> actions = [];

        foreach (var actionAccess in actionAccesses)
        {
            actions.Add(await MapFromInternalToExternalAction(actionAccess, resourceId, accessListMode, fromParty, requiredAuthenticationLevel, authenticationLevel, isResourceDelegable, cancellationToken));
        }

        return actions;
    }

    private void ProcessTheAccessToTheAccessKeys(List<ActionAccess> actionAccesses, IEnumerable<AccessPackageDto.AccessPackageDtoCheck> packages)
    {
        foreach (var actionAccess in actionAccesses)
        {
            foreach (var accessorUrn in actionAccess.AccessorUrns)
            {
                var package = packages.FirstOrDefault(p => p.Package.Urn == accessorUrn);

                if (package != null)
                {
                    if (package.Result)
                    {
                        actionAccess.PackageAllowAccess.Add(package);
                    }
                    else
                    {
                        ActionDto.Reason reason = new ActionDto.Reason
                        {
                            Description = $"Access denied based on missing package urn: {accessorUrn}",
                            ReasonKey = DelegationCheckReasonCode.MissingPackageAccess,
                            PackageId = package.Package.Id,
                            PackageUrn = package.Package.Urn
                        };

                        actionAccess.PackageDenyAccess.Add(reason);
                    }
                }
            }
        }
    }

    private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
    {
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    private async Task<ResourceDto> FetchResource(string resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await Db.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.RefId == resourceId, cancellationToken);
        Provider provider = await Db.Providers.AsNoTracking().SingleOrDefaultAsync(p => p.Id == resource.ProviderId, cancellationToken);
        ProviderTypeConstants.TryGetById(provider.TypeId, out var providerType);
        PersistenceEF.Models.ResourceType resourceType = await Db.ResourceTypes.SingleOrDefaultAsync(rt => rt.Id == resource.TypeId, cancellationToken);

        ProviderDto providerDto = new ProviderDto
        {
            Id = provider.Id,
            Code = provider.Code,
            LogoUrl = provider.LogoUrl,
            Name = provider.Name,
            RefId = provider.RefId,
            TypeId = provider.TypeId,
            Type = new ProviderTypeDto { Id = providerType.Id, Name = providerType.Entity.Name }
        };

        ResourceDto resourceDto = new ResourceDto 
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Provider = providerDto,
            ProviderId = provider.Id,
            RefId = resource.RefId,
            TypeId = resource.TypeId,
            Type = new ResourceTypeDto { Id = resourceType.Id, Name = resourceType.Name }
        };

        return resourceDto;
    }

    private async Task<XacmlPolicy> GetPolicy(string resourceId, CancellationToken cancellationToken = default)
    {
        XacmlPolicy policy = null;
        
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ValidationException($"ResourceId cannot be null or empty");
        }

        bool isApp = DelegationCheckHelper.IsAppResourceId(resourceId, out string org, out string app);

        if (isApp)
        {
            policy = await PolicyRetrievalPoint.GetPolicyAsync(org, app, cancellationToken);
        }
        else
        {
            policy = await PolicyRetrievalPoint.GetPolicyAsync(resourceId, cancellationToken);            
        }

        if (policy == null)
        {
            throw new ValidationException($"No valid policy found for the specified resource");
        }

        return policy;
    }
}
