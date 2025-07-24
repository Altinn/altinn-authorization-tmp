using Altinn.AccessManagement.Core.Models.Authentication;
using Altinn.AccessManagement.Enduser.Services;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessManagement.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.SblBridge;
using Altinn.Platform.Register.Models;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices.Services
{
    /// <inheritdoc />
    public class AltinnBankruptcyEstateRoleSyncService : BaseSyncService, IAltinnBankruptcyEstateRoleSyncService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnClientRoleSyncService"/> class.
        /// </summary>
        /// <param name="lease">The lease service used for managing leases.</param>
        /// <param name="role">The role service used for streaming roles.</param>
        /// <param name="delegationService">The delegation service used for managing delegations.</param>
        /// <param name="connectionService">The connection service used for managing connections.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="featureManager">The feature manager for handling feature flags.</param>
        public AltinnBankruptcyEstateRoleSyncService(
            IAltinnLease lease,
            IAltinnSblBridge role,
            IDelegationService delegationService,
            IConnectionService connectionService,
            ILogger<AltinnBankruptcyEstateRoleSyncService> logger,
            IFeatureManager featureManager           
        ) : base(lease, featureManager)
        {
            _role = role;
            _delegationService = delegationService;
            _connectionService = connectionService;
            _logger = logger;
        }

        private readonly IAltinnSblBridge _role;
        private readonly IDelegationService _delegationService;
        private readonly IConnectionService _connectionService;
        private readonly ILogger<AltinnBankruptcyEstateRoleSyncService> _logger;

        /// <inheritdoc />
        public async Task SyncBankruptcyEstateRoles(LeaseResult<AltinnBankruptcyEstateRoleLease> ls, CancellationToken cancellationToken)
        {
            var clientDelegations = await _role.StreamRoles("14", ls.Data?.AltinnBankruptcyEstateRoleStreamNextPageLink, cancellationToken);

            await foreach (var page in clientDelegations)
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

                Guid batchId = Guid.NewGuid();
                var batchName = batchId.ToString().ToLower().Replace("-", string.Empty);
                _logger.LogInformation("Starting proccessing role page '{0}'", batchName);

                if (page.Content != null)
                {
                    foreach (var item in page.Content.Data)
                    {
                        ChangeRequestOptions options = new ChangeRequestOptions()
                        {
                            ChangedBy = item.PerformedByUserUuid ?? AuditDefaults.Altinn2RoleImportSystem,
                            ChangedBySystem = AuditDefaults.Altinn2RoleImportSystem,
                            ChangedAt = item.DelegationChangeDateTime ?? DateTime.UtcNow,
                        };

                        //TODO: Fix the actual work of BAnkruptcyEstate delegations
                        /*
                        string packageUrn = GetClientPackageFromRoleTypeCode(item.RoleTypeCode, cancellationToken);

                        // Convert RoleDelegationModel to Client Delegation 
                        var delegationData = await CreateClientDelegationRequest(item, cancellationToken);

                        if (item.DelegationAction == DelegationAction.Revoke)
                        {
                            // If the action is Revoke, we should delete the delegation
                            int deleted = await _delegationService.RevokeClientDelegation(delegationData, options, cancellationToken);
                            if (deleted <= 0)
                            {
                                _logger.LogWarning(
                                    "Failed to delete delegation for FromParty: {FromParty}, ToParty: {ToParty}, Facilitator: {Facilitator}, PackageUrn: {packageUrn}", 
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    item.PerformedByPartyUuid,
                                    packageUrn);
                            }
                        }
                        else
                        {
                            if (item.PerformedByPartyUuid == null)
                            {
                                // When we do not have a facilitator party id we should not create the delegation just log a warning
                                _logger.LogWarning(
                                    "The delegation is missing facilitator data so it is not a valid client delegation {FromParty}, ToParty: {ToParty}, PackageUrn: {PackageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    packageUrn);
                                continue;
                            }

                            IEnumerable<Delegation> delegations = await _delegationService.ImportClientDelegation(delegationData, options, cancellationToken);
                        }
                        */                            
                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await UpdateLease(ls, data => data.AltinnBankruptcyEstateRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);
            }
        }

        private async Task<ImportClientDelegationRequestDto> CreateClientDelegationRequest(RoleDelegationModel delegationModel, CancellationToken cancellationToken = default)
        {
            Guid? facilitatorPartyId = delegationModel.PerformedByPartyUuid;

            var request = new ImportClientDelegationRequestDto()
            {
                ClientId = delegationModel.FromPartyUuid,
                AgentId = delegationModel.ToUserPartyUuid ?? throw new Exception($"'delegationModel.ToUserPartyUuid' does not have value"),
                AgentRole = string.Empty,
                RolePackages = new List<CreateSystemDelegationRolePackageDto>(),
                Facilitator = facilitatorPartyId,
            };

            var delegationContent = CreateSystemDelegationRolePackageDtoForClientDelegation(delegationModel.RoleTypeCode, cancellationToken);
            request.RolePackages.Add(delegationContent);
            request.AgentRole = "agent";

            return request;
        }

        private CreateSystemDelegationRolePackageDto CreateSystemDelegationRolePackageDtoForClientDelegation(string roleTypeCode, CancellationToken cancellationToken = default)
        {
            string urn = string.Empty;
            string clientRoleCode = string.Empty;
            switch (roleTypeCode)
            {
                case "A0237":
                    urn = "urn:altinn:accesspackage:ansvarlig-revisor";
                    clientRoleCode = "revisor";
                    break;
                case "A0238":
                    urn = "urn:altinn:accesspackage:revisormedarbeider";
                    clientRoleCode = "revisor";
                    break;
                case "A0239":
                    urn = "urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet";
                    clientRoleCode = "regnskapsforer";
                    break;
                case "A0240":
                    urn = "urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet";
                    clientRoleCode = "regnskapsforer";
                    break;
                case "A0241":
                    urn = "urn:altinn:accesspackage:regnskapsforer-lonn";
                    clientRoleCode = "regnskapsforer";
                    break;
            }

            CreateSystemDelegationRolePackageDto accessPackage = new CreateSystemDelegationRolePackageDto()
            {
                RoleIdentifier = clientRoleCode,
                PackageUrn = urn
            };

            return accessPackage;
        }

        private string GetClientPackageFromRoleTypeCode(string roleTypeCode, CancellationToken cancellationToken = default)
        {
            string urn = string.Empty;
            switch (roleTypeCode)
            {
                case "A0237":
                    urn = "urn:altinn:accesspackage:ansvarlig-revisor";
                    break;
                case "A0238":
                    urn = "urn:altinn:accesspackage:revisormedarbeider";
                    break;
                case "A0239":
                    urn = "urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet";
                    break;
                case "A0240":
                    urn = "urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet";
                    break;
                case "A0241":
                    urn = "urn:altinn:accesspackage:regnskapsforer-lonn";
                    break;
            }

            return urn;
        }
    }
}
