﻿using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessManagement.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.SblBridge;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices.Services
{
    /// <inheritdoc />
    public class AltinnClientRoleSyncService : BaseSyncService, IAltinnClientRoleSyncService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AltinnClientRoleSyncService"/> class.
        /// </summary>
        /// <param name="lease">The lease service used for managing leases.</param>
        /// <param name="role">The role service used for streaming roles.</param>
        /// <param name="delegationService">The delegation service used for managing delegations.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="featureManager">The feature manager for handling feature flags.</param>
        public AltinnClientRoleSyncService(
            IAltinnSblBridge role,
            IDelegationService delegationService,
            ILogger<AltinnClientRoleSyncService> logger,
            IFeatureManager featureManager           
        )
        {
            _role = role;
            _delegationService = delegationService;
            _logger = logger;
        }

        private readonly IAltinnSblBridge _role;
        private readonly IDelegationService _delegationService;
        private readonly ILogger<AltinnClientRoleSyncService> _logger;

        /// <inheritdoc />
        public async Task SyncClientRoles(ILease lease, CancellationToken cancellationToken)
        {
            var leaseData = await lease.Get<AltinnClientRoleLease>(cancellationToken);
            var clientDelegations = await _role.StreamRoles("12", leaseData.AltinnClientRoleStreamNextPageLink, cancellationToken);
            string operationId = Guid.CreateVersion7().ToString();
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
                        AuditValues audit = new AuditValues(
                            item.PerformedByUserUuid ?? AuditDefaults.Altinn2RoleImportSystem,
                            AuditDefaults.Altinn2RoleImportSystem,
                            operationId,
                            item.DelegationChangeDateTime ?? DateTime.UtcNow);

                        // Convert RoleDelegationModel to Client Delegation 
                        var delegationData = await CreateClientDelegationRequest(item, cancellationToken);

                        if (item.DelegationAction == DelegationAction.Revoke)
                        {
                            // If the action is Revoke, we should delete the delegation
                            int deleted = await _delegationService.RevokeClientDelegation(delegationData, audit, cancellationToken);
                            if (deleted <= 0)
                            {
                                _logger.LogWarning(
                                    "Failed to delete delegation for FromParty: {FromParty}, ToParty: {ToParty}, Facilitator: {Facilitator}, PackageUrn: {packageUrn}",
                                    item.FromPartyUuid,
                                    item.ToUserPartyUuid,
                                    item.PerformedByPartyUuid,
                                    delegationData.RolePackages.First().PackageUrn);
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
                                    delegationData.RolePackages.First().PackageUrn);
                                continue;
                            }

                            IEnumerable<CreateDelegationResponseDto> delegations = await _delegationService.ImportClientDelegation(delegationData, audit, cancellationToken);
                        }

                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await lease.Update<AltinnClientRoleLease>(data => data.AltinnClientRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);
            }
        }

        private async Task<ImportClientDelegationRequestDto> CreateClientDelegationRequest(RoleDelegationModel delegationModel, CancellationToken cancellationToken = default)
        {
            Guid? facilitatorPartyId = delegationModel.PerformedByPartyUuid;

            var request = new ImportClientDelegationRequestDto()
            {
                ClientId = delegationModel.FromPartyUuid,
                AgentId = delegationModel.ToUserPartyUuid ?? throw new Exception($"'delegationModel.ToUserPartyUuid' does not have value"),
                AgentRole = "agent",
                RolePackages = [],
                Facilitator = facilitatorPartyId,
            };

            var delegationContent = await CreateSystemDelegationRolePackageDtoForClientDelegation(delegationModel.RoleTypeCode, cancellationToken);
            request.RolePackages.Add(delegationContent);

            return request;
        }

        private Task<ImportClientDelegationRolePackageDto> CreateSystemDelegationRolePackageDtoForClientDelegation(string roleTypeCode, CancellationToken cancellationToken = default)
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

            ImportClientDelegationRolePackageDto accessPackage = new ImportClientDelegationRolePackageDto()
            {
                RoleIdentifier = clientRoleCode,
                PackageUrn = urn
            };

            return Task.FromResult(accessPackage);
        }
    }
}
