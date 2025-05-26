using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AltinnRole;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices.Services
{
    /// <inheritdoc />
    public class AltinnClientRoleSyncService : BaseSyncService, IAltinnClientRoleSyncService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lease"></param>
        /// <param name="role"></param>
        /// <param name="delegationService"></param>
        /// <param name="logger"></param>
        /// <param name="featureManager"></param>
        /// <param name="ingestService"></param>
        /// <param name="roleRepository"></param>
        /// <param name="providerRepository"></param>
        /// <param name="packageRepository"></param>
        /// <param name="assignmentRepository"></param>
        /// <param name="entityRepository"></param>
        /// <param name="entityTypeRepository"></param>
        public AltinnClientRoleSyncService(
            IAltinnLease lease,
            IAltinnRole role,
            IDelegationService delegationService,
            IConnectionService connectionService,
            ILogger<AltinnClientRoleSyncService> logger,
            IFeatureManager featureManager           
        ) : base(lease, featureManager)
        {
            _role = role;
            _delegationService = delegationService;
            _connectionService = connectionService;
            _logger = logger;

        }

        private readonly IAltinnRole _role;
        private readonly IDelegationService _delegationService;
        private readonly IConnectionService _connectionService;
        private readonly ILogger<AltinnClientRoleSyncService> _logger;


        /// <inheritdoc />
        public async Task SyncClientRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var clientDelegations = await _role.StreamRoles("12", ls.Data?.AltinnClientRoleStreamNextPageLink, cancellationToken);

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
                        // Convert RoleDelegationModel to Client Delegation 
                        var delegationData = await CreateClientDelegationRequest(item, cancellationToken);

                        if (delegationData.Facilitator == null)
                        {
                            continue; // When we do not have a facilitator party id we should not create the delegation
                        }

                        ChangeRequestOptions options = new ChangeRequestOptions()
                        {
                            ChangedBy = item.PerformedByUserUuid ?? AuditDefaults.Altinn2ClientImportSystem,
                            ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                            ChangedAt = item.DelegationChangeDateTime ?? DateTime.UtcNow,
                        };

                        IEnumerable<Delegation> delegations = await _delegationService.ImportClientDelegation(delegationData, options);
                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await UpdateLease(ls, data => data.AltinnClientRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);
            }
        }

        private async Task<bool> DeleteClientDelegation(Guid fromParty, Guid toParty, Guid facilitatorId, Guid packageId, CancellationToken cancellationToken = default)
        {
            IEnumerable<ExtConnection> connections = await _connectionService.Get(fromParty, toParty, facilitatorId, cancellationToken);
            if (connections?.Count() > 0)
            {
                // No connection found
                return false;
            }

            /*
            //find packages in the connections
            bool packageFound = false;
            foreach (var package in connections.Packages)
            {
                if (package.Id == packageId)
                {
                    packageFound = true;
                    break;
                }
            }
            if (!packageFound)
            {
                // No package found
                return false;
            }

            var res = await delegationRepository.Delete(delegationId, options: options);
            return res > 0;
            */

            return false;
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
            request.AgentRole = "rettighetshaver";

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
    }
}
