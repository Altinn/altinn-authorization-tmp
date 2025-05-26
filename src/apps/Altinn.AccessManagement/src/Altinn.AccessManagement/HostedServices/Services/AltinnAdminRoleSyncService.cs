using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AltinnRole;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices.Services
{
    /// <inheritdoc />
    public class AltinnAdminRoleSyncService : BaseSyncService, IAltinnAdminRoleSyncService
    {
        public AltinnAdminRoleSyncService(
            IAltinnLease lease,
            IAltinnRole role,
            ILogger<RoleSyncService> logger,
            IFeatureManager featureManager,
            IIngestService ingestService,
            IRoleRepository roleRepository,
            IProviderRepository providerRepository,
            IAssignmentRepository assignmentRepository,
            IEntityRepository entityRepository,
            IEntityTypeRepository entityTypeRepository
        ) : base(lease, featureManager)
        {
            _role = role;
            _logger = logger;
            _ingestService = ingestService;
            _roleRepository = roleRepository;
            _providerRepository = providerRepository;
            _assignmentRepository = assignmentRepository;
            _entityRepository = entityRepository;
            _entityTypeRepository = entityTypeRepository;
        }

        private readonly IAltinnRole _role;
        private readonly ILogger<RoleSyncService> _logger;
        private readonly IRoleRepository _roleRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IEntityTypeRepository _entityTypeRepository;
        private readonly IIngestService _ingestService;

        /// <inheritdoc />
        public async Task SyncAdminRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var adminRoles = await _role.StreamRoles("11", ls.Data?.AltinnAdminRoleStreamNextPageLink, cancellationToken);

            await foreach (var page in adminRoles)
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

                ChangeRequestOptions previousOptions = null;

                if (page.Content != null)
                {
                    foreach (var item in page.Content.Data)
                    {
                        var assignment = await ConvertAdminRoleDelegationModelToAssignment(item);
                        if (assignment.Asignment == null)
                        {
                            throw new Exception("Failed to convert RoleModel to Assignment");
                        }

                        /*
                        if (batchData.Any(t => t.FromId == assignment.Asignment.FromId && t.ToId == assignment.Asignment.ToId && t.RoleId == assignment.Asignment.RoleId))
                        {
                            // If changes on same assignment then execute as-is before continuing.
                            await Flush(batchId);
                        }

                        if (previousOptions != null && assignment.Options.ChangedBy != previousOptions.ChangedBy)
                        {
                            // if performer changes flush batch
                            await Flush(batchId);
                        }

                        if (item.DelegationAction == DelegationAction.Delegate)
                        {
                            if (item.ToUserType == UserType.EnterpriseIdentified)
                            {
                                continue;
                            }

                            batchData.Add(assignment.Asignment);
                        }
                        else
                        {
                            var filter = _assignmentRepository.CreateFilterBuilder();
                            filter.Equal(t => t.FromId, assignment.Asignment.FromId);
                            filter.Equal(t => t.ToId, assignment.Asignment.ToId);
                            filter.Equal(t => t.RoleId, assignment.Asignment.RoleId);
                            await _assignmentRepository.Delete(filter, assignment.Options);
                        }

                        previousOptions = assignment.Options;
                        */
                    }
                }

                await Flush(batchId);

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await UpdateLease(ls, data => data.AltinnAdminRoleStreamNextPageLink = page.Content.Links.Next, cancellationToken);

                await Flush(batchId);

                async Task Flush(Guid batchId)
                {
                    /*
                    try
                    {
                        _logger.LogInformation("Ingest and Merge Assignment batch '{0}' to db", batchName);
                        var ingested = await _ingestService.IngestTempData<Assignment>(batchData, batchId, previousOptions, cancellationToken);

                        if (ingested != batchData.Count)
                        {
                            _logger.LogWarning("Ingest partial complete: Assignment ({0}/{1})", ingested, batchData.Count);
                        }

                        var merged = await _ingestService.MergeTempData<Assignment>(batchId, previousOptions, cancellationToken: cancellationToken);

                        _logger.LogInformation("Merge complete: Assignment ({0}/{1})", merged, ingested);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to ingest and/or merge Assignment and EntityLookup batch {0} to db", batchName);
                        throw new Exception(string.Format("Failed to ingest and/or merge Assignment and EntityLookup batch {0} to db", batchName), ex);
                    }
                    finally
                    {
                        batchId = Guid.NewGuid();
                        batchData.Clear();
                    }
                    */
                }
            }
        }

        private async Task<(Assignment Asignment, ChangeRequestOptions Options)> ConvertAdminRoleDelegationModelToAssignment(RoleDelegationModel model)
        {
            try
            {
                var role = await GetOrCreateAdminRole(model.RoleTypeCode, "Altinn2");

                if (role == null)
                {
                    return (null, null);
                }

                // TODO: Fix Datetime for Role based on data from model if not known use now.
                // TODO; Fix performedby When known actual performer when not known provider set as performer
                Assignment assignment = new Assignment()
                {
                    Id = Guid.CreateVersion7(),
                    FromId = model.FromPartyUuid,
                    ToId = model.ToUserPartyUuid.Value,
                    RoleId = role.Id
                };

                ChangeRequestOptions options = new ChangeRequestOptions()
                {
                    ChangedBy = model.PerformedByUserUuid ?? AuditDefaults.Altinn2ClientImportSystem,
                    ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                };

                return (assignment, options);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to convert model to Assignment. From:{0} To:{1} Role:{2}", model.FromPartyUuid, model.ToUserPartyUuid, model.RoleTypeCode));
            }
        }

        private async Task<Role> GetOrCreateAdminRole(string roleCode, string roleSource)
        {
            string roleIdentifier = await GetAdminRoleUrnFromRoleCode(roleCode);

            if (string.IsNullOrEmpty(roleIdentifier))
            {
                return null;
            }

            var role = (await _roleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
            if (role == null)
            {
                await _roleRepository.Create(
                    new Role()
                    {
                        Id = Guid.CreateVersion7(),
                        Name = roleIdentifier,
                        Description = roleIdentifier,
                        Code = roleIdentifier,
                        Urn = roleIdentifier,
                        EntityTypeId = OrgType.Id,
                        ProviderId = Provider.Id,
                    },
                    new ChangeRequestOptions()
                    {
                        ChangedBy = AuditDefaults.Altinn2ClientImportSystem,
                        ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                    });

                role = (await _roleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
                if (role == null)
                {
                    throw new Exception(string.Format("Unable to get or create role '{0}'", roleIdentifier));
                }
            }

            Roles.Add(role);
            return role;
        }

        private async Task<string> GetAdminRoleUrnFromRoleCode(string roleCode)
        {
            switch (roleCode)
            {
                case "APIADM":
                    return "urn:altinn:role:maskinporten-administrator";
                case "APIADMNUF":
                    return "urn:altinn:role:maskinporten-administrator";
                case "ADMAI":
                    return "urn:altinn:role:tilgangsstyrer";
                case "BOADM":
                    return null;
                case "HADM":
                    return "urn:altinn:role:hovedadministrator";
                case "KLADM":
                    return "urn:altinn:role:klientadministrator";
                default:
                    throw new Exception(string.Format("Unknown role type code '{0}'", roleCode));
            }
        }

        private EntityType OrgType { get; set; }

        private Provider Provider { get; set; }

        private List<AccessMgmt.Core.Models.Role> Roles { get; set; } = [];
    }
}
