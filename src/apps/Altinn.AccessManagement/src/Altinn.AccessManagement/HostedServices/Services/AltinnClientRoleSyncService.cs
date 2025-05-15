using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
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
        public AltinnClientRoleSyncService(
            IAltinnLease lease,
            IAltinnRole role,
            IDelegationService delegationService,
            ILogger<AltinnClientRoleSyncService> logger,
            IFeatureManager featureManager,
            IIngestService ingestService,
            IRoleRepository roleRepository,
            IProviderRepository providerRepository,
            IPackageRepository packageRepository,
            IAssignmentRepository assignmentRepository,
            IEntityRepository entityRepository,
            IEntityTypeRepository entityTypeRepository
        ) : base(lease, featureManager)
        {
            _role = role;
            _delegationService = delegationService;
            _logger = logger;
            _ingestService = ingestService;
            _roleRepository = roleRepository;
            _providerRepository = providerRepository;
            _packageRepository = packageRepository;
            _assignmentRepository = assignmentRepository;
            _entityRepository = entityRepository;
            _entityTypeRepository = entityTypeRepository;
        }

        private readonly IAltinnRole _role;
        private readonly IDelegationService _delegationService;
        private readonly ILogger<AltinnClientRoleSyncService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IEntityTypeRepository _entityTypeRepository;
        private readonly IIngestService _ingestService;

        /*
        private List<Provider> Providers { get; set; } = [];

        private List<EntityType> EntityTypes { get; set; } = [];

        private List<EntityVariant> EntityVariants { get; set; } = [];
        */

        /// <inheritdoc />
        public async Task SyncClientRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var batchData = new List<Assignment>();
            var clientDelegations = await _role.StreamRoles("12", ls.Data?.AltinnClientRoleStreamNextPageLink, cancellationToken);

            OrgType = (await _entityTypeRepository.Get(t => t.Name, "Organisasjon")).FirstOrDefault();
            Provider = (await _providerRepository.Get(t => t.Code, "Altinn2")).FirstOrDefault();
            Packages = await _packageRepository.Get(cancellationToken: cancellationToken);
            Roles = await _roleRepository.Get(cancellationToken: cancellationToken);
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
                        // TODO: Convert RoleDelegationModel to Client Delegation 
                        var delegationData = await CreateClientDelegationRequest(item, cancellationToken);

                        if (delegationData.Facilitator == null)
                        {
                            continue; // When we do not have a facilitator party id we should not create the delegation
                        }

                        ChangeRequestOptions options = new ChangeRequestOptions()
                        {
                            ChangedBy = item.PerformedByUserUuid ?? AuditDefaults.Altinn2ClientImportSystem,
                            ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                        };

                        IEnumerable<Delegation> delegations = await _delegationService.ImportClientDelegation(delegationData, options);
                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await UpdateLease(ls, data => data.AltinnAdminRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);
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
                DelegatedDateTimeOffset = delegationModel.DelegationChangeDateTime ?? throw new Exception($"'delegationModel.DelegatedDateTimeOffset' does not have value"),
            };

            var delegationContent = await CreateSystemDelegationRolePackageDtoForClientDelegation(delegationModel.RoleTypeCode, cancellationToken);
            request.RolePackages.Add(delegationContent.Package);
            request.AgentRole = delegationContent.AgentRole;

            return request;
        }

        private async Task<(CreateSystemDelegationRolePackageDto Package, string AgentRole)> CreateSystemDelegationRolePackageDtoForClientDelegation(string roleTypeCode, CancellationToken cancellationToken = default)
        {
            string urn = string.Empty;
            string agentRoleUrn = string.Empty;
            switch (roleTypeCode)
            {
                case "A0237":
                    urn = "urn:altinn:accesspackage:ansvarlig-revisor";
                    agentRoleUrn = "urn:altinn:external-role:ccr:revisor";
                    break;
                case "A0238":
                    urn = "urn:altinn:accesspackage:revisormedarbeider";
                    agentRoleUrn = "urn:altinn:external-role:ccr:revisor";
                    break;
                case "A0239":
                    urn = "urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet";
                    agentRoleUrn = "urn:altinn:external-role:ccr:regnskapsforer";
                    break;
                case "A0240":
                    urn = "urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet";
                    agentRoleUrn = "urn:altinn:external-role:ccr:regnskapsforer";
                    break;
                case "A0241":
                    urn = "urn:altinn:accesspackage:regnskapsforer-lonn";
                    agentRoleUrn = "urn:altinn:external-role:ccr:regnskapsforer";
                    break;
            }

            var package = Packages.FirstOrDefault(t => t.Urn == urn) ?? throw new Exception($"Unable to find package with urn '{urn}'");
            var role = Roles.FirstOrDefault(t => t.Urn == agentRoleUrn) ?? throw new Exception($"Unable to find role with urn '{agentRoleUrn}'");

            CreateSystemDelegationRolePackageDto accessPackage = new CreateSystemDelegationRolePackageDto()
            {
                RoleIdentifier = package.Id.ToString(),
                PackageUrn = package.Urn
            };

            return (accessPackage, role.Id.ToString());
        }

        private EntityType OrgType { get; set; }

        private Provider Provider { get; set; }

        private IEnumerable<Role> Roles { get; set; } = [];

        private IEnumerable<Package> Packages { get; set; } = [];
    }
}
