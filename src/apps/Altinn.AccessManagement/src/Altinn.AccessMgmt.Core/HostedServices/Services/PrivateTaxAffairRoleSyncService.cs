using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.SblBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.HostedServices.Services
{
    public class PrivateTaxAffairRoleSyncService : BaseSyncService, IPrivateTaxAffairRoleSyncService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateTaxAffairRoleSyncService"/> class.
        /// </summary>
        /// <param name="role">The role service used for streaming roles.</param>
        /// <param name="serviceProvider">object used for creating a scope and fetching a scoped service (IDelegationService) based on this scope</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        public PrivateTaxAffairRoleSyncService(
            IAltinnSblBridge role,
            IServiceProvider serviceProvider,
            ILogger<PrivateTaxAffairRoleSyncService> logger
        )
        {
            _role = role;
            _serviceProivider = serviceProvider;
            _logger = logger;
        }

        private readonly IAltinnSblBridge _role;
        private readonly ILogger<PrivateTaxAffairRoleSyncService> _logger;
        private readonly IServiceProvider _serviceProivider;

        public async Task SyncPrivateTaxAffairRoles(ILease lease, CancellationToken cancellationToken)
        {
            var leaseData = await lease.Get<PrivateTaxAffairRoleLease>(cancellationToken);
            var adminDelegations = await _role.StreamRoles("13", leaseData.PrivateTaxAffairRoleStreamNextPageLink, cancellationToken);

            await foreach (var page in adminDelegations)
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
                        // Do not process admin roles for EC-Users
                        if (item.ToUserType == UserType.EnterpriseIdentified)
                        {
                            continue;
                        }

                        await using var scope = _serviceProivider.CreateAsyncScope();
                        IAssignmentService assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

                        AuditValues values = new AuditValues(
                            item.PerformedByUserUuid ?? SystemEntityConstants.Altinn2RoleImportSystem,
                            SystemEntityConstants.Altinn2RoleImportSystem,
                            batchId.ToString(),
                            item.DelegationChangeDateTime?.ToUniversalTime() ?? DateTime.UtcNow);

                        List<string> packageUrns = GetPrivateTaxAffairPackageFromRoleTypeCode(item.RoleTypeCode, cancellationToken);

                        if (item.DelegationAction == DelegationAction.Revoke)
                        {
                            // If the action is Revoke, we should delete the assignmentPackages
                            if (item.ToUserPartyUuid == null)
                            {
                                _logger.LogWarning(
                                    "The delegation is missing ToUserPartyUuid so it is not a valid admin delegation {FromParty}, ToParty: {ToParty}, PackageUrns: {PackageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    string.Join(", ", packageUrns));
                                continue;
                            }

                            int revokes = await assignmentService.RevokeAssignmentPackages(
                                item.FromPartyUuid,
                                item.ToUserPartyUuid.Value,
                                packageUrns,
                                values,
                                true,
                                cancellationToken);

                            if (revokes == 0)
                            {
                                _logger.LogWarning(
                                    "Failed to delete assignmentpackages for FromParty: {FromParty}, ToParty: {ToParty}, PackageUrns: {packageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    string.Join(", ", packageUrns));
                            }
                        }
                        else
                        {
                            if (!item.DelegationChangeDateTime.HasValue || item.DelegationChangeDateTime.Value < new DateTimeOffset(2021, 3, 1, 0, 0, 0, new TimeSpan(1, 0, 0)))
                            {
                                _logger.LogInformation(
                                    "Skipping privatetaxaffair delegation FromParty: {FromParty}, ToParty: {ToParty}, PackageUrns: {packageUrn} since it is before the cut-off date",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    string.Join(", ", packageUrns));
                                continue;
                            }
                            
                            if (item.ToUserPartyUuid == null)
                            {
                                _logger.LogWarning(
                                    "The delegation is missing ToUserPartyUuid so it is not a valid privatetaxaffair delegation {FromParty}, ToParty: {ToParty}, PackageUrns: {PackageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    string.Join(", ", packageUrns));
                                continue;
                            }

                            List<AssignmentPackageDto> adds = await assignmentService.ImportAssignmentPackages(item.FromPartyUuid, item.ToUserPartyUuid.Value, packageUrns, values, cancellationToken);
                            if (adds.Count == 0)
                            {
                                _logger.LogWarning(
                                    "Failed to import delegation for FromParty: {FromParty}, ToParty: {ToParty}, PackageUrns: {packageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    string.Join(", ", packageUrns));
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

        private List<string> GetPrivateTaxAffairPackageFromRoleTypeCode(string roleTypeCode, CancellationToken cancellationToken = default)
        {
            List<string> packages = new List<string>();

            switch (roleTypeCode.ToUpper())
            {
                case "A0282":
                    packages.Add("urn:altinn:accesspackage:tilgangsstyrer");
                    break;
            }

            return packages;
        }
    }
}
