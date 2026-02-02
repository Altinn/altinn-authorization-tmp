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

    public ResourceService(AppDbContext appDbContext, IAuditAccessor auditAccessor, IPolicyRetrievalPoint policyRetrievalPoint, IAMPartyService partyService, IContextRetrievalService contextRetrievalService, IAccessListsAuthorizationClient accessListsAuthorizationClient, IPolicyInformationPoint policyInformationPoint)
    {
        Db = appDbContext;
        AuditAccessor = auditAccessor;
        PolicyRetrievalPoint = policyRetrievalPoint;
        PartyService = partyService;
        ContextRetrievalService = contextRetrievalService;
        AccessListsAuthorizationClient = accessListsAuthorizationClient;
        PolicyInformationPoint = policyInformationPoint;
    }

    public async ValueTask<Resource> GetResource(Guid id, CancellationToken cancellationToken = default)
    {
        return await Db.Resources.AsNoTracking().Include(t => t.Type).Include(t => t.Provider).SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async ValueTask<Resource> GetResource(string refId, CancellationToken cancellationToken = default)
    {
        return await Db.Resources.AsNoTracking().Include(t => t.Type).Include(t => t.Provider).SingleOrDefaultAsync(r => r.RefId == refId, cancellationToken);
    }
}
