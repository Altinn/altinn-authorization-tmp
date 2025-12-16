using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AccessManagement;
using Altinn.Authorization.Integration.Platform.SblBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.HostedServices.Services
{
    public class SingleAppRightSyncService : BaseSyncService, ISingleAppRightSyncService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnAdminRoleSyncService"/> class.
        /// </summary>
        /// <param name="singleRights">The single rights service used for streaming roles.</param>
        /// <param name="serviceProvider">object used for creating a scope and fetching a scoped service (IDelegationService) based on this scope</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        public SingleAppRightSyncService(
            IServiceProvider serviceProvider,
            IAltinnAccessManagement singleRights,
            ILogger<SingleAppRightSyncService> logger
        )
        {
            _singleRights = singleRights;
            _serviceProivider = serviceProvider;
            _logger = logger;
        }

        private readonly IAltinnAccessManagement _singleRights;
        private readonly ILogger<SingleAppRightSyncService> _logger;
        private readonly IServiceProvider _serviceProivider;

        public async Task SyncSingleAppRights(ILease lease, CancellationToken cancellationToken)
        {
            var leaseData = await lease.Get<SingleAppRightLease>(cancellationToken);
            var singleAppRightDelegations = await _singleRights.StreamAppRightDelegations(leaseData.SingleAppRightStreamNextPageLink, cancellationToken);

            await foreach (var page in singleAppRightDelegations)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!page.IsSuccessful)
                {
                    Log.ResponseError(_logger, page.StatusCode);
                    throw new Exception("Stream page is not successful");
                }

                Guid batchId = Guid.CreateVersion7();
                var batchName = batchId.ToString().ToLower().Replace("-", string.Empty);
                _logger.LogInformation("Starting proccessing role page '{0}'", batchName);

                if (page.Content != null)
                {
                    foreach (var item in page.Content.Data)
                    {
                        await using var scope = _serviceProivider.CreateAsyncScope();
                        IAssignmentService assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                        
                        if (!Guid.TryParse(item.PerformedByUuid, out Guid performedByGuid))
                        {
                            performedByGuid = SystemEntityConstants.Altinn2RoleImportSystem.Id;
                        }

                        AuditValues values = new AuditValues(
                            performedByGuid,
                            SystemEntityConstants.Altinn2RoleImportSystem,
                            batchId.ToString(),
                            item.Created?.ToUniversalTime() ?? DateTime.UtcNow);

                        string[] appValues = item.ResourceId.Split('/', 2, StringSplitOptions.TrimEntries);
                        string resource = $"app_{appValues[0].ToLower()}_{appValues[1].ToLower()}";

                        if (item.DelegationChangeType == AccessManagement.Core.Models.DelegationChangeType.RevokeLast)
                        {
                            int revokes = await assignmentService.RevokeImportedAssignmentResource(
                                item.FromUuid.Value,
                                item.ToUuid.Value,
                                resource,
                                values,
                                cancellationToken);

                            if (revokes == 0)
                            {
                                _logger.LogWarning(
                                    "Failed to delete assignmentresource for FromParty: {FromParty}, ToParty: {ToParty}, Resource: {resource}",
                                    item.FromUuid,
                                    item.ToUuid,
                                    resource);
                            }
                        }
                        else
                        {
                            int adds = await assignmentService.ImportAssignmentResourceChange(
                                item.FromUuid.Value,
                                item.ToUuid.Value,
                                resource,
                                item.BlobStoragePolicyPath,
                                item.BlobStorageVersionId,
                                values,
                                cancellationToken);

                            if (adds == 0)
                            {
                                _logger.LogWarning(
                                    "Failed to import delegation for FromParty: {FromParty}, ToParty: {ToParty}, Resource: {resource}",
                                    item.FromUuid,
                                    item.ToUuid,
                                    resource);
                            }                                                        
                        }

                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await lease.Update<AltinnAdminRoleLease>(d => d.AltinnAdminRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);
            }
        }

        private List<string> GetAdminPackageFromRoleTypeCode(string roleTypeCode, CancellationToken cancellationToken = default)
        {
            List<string> packages = new List<string>();

            switch (roleTypeCode.ToUpper())
            {
                case "ADMAI":
                    packages.Add("urn:altinn:accesspackage:tilgangsstyrer");
                    break;
                case "APIADM":
                    packages.Add("urn:altinn:accesspackage:maskinporten-administrator");
                    packages.Add("urn:altinn:accesspackage:maskinporten-scopes");
                    break;
                case "APIADMNUF":
                    packages.Add("urn:altinn:accesspackage:maskinporten-administrator");
                    packages.Add("urn:altinn:accesspackage:maskinporten-scopes-nuf");
                    break;
                case "BOADM":
                    packages.Add("urn:altinn:accesspackage:konkursbo-tilgangsstyrer");
                    break;
                case "HADM":
                    packages.Add("urn:altinn:accesspackage:hovedadministrator");
                    break;
                case "KLADM":
                    packages.Add("urn:altinn:accesspackage:klientadministrator");
                    break;
            }

            return packages;
        }
    }
}
