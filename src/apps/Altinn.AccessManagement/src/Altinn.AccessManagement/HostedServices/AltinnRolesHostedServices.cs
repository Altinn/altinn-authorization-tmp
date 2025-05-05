using System.Net;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AltinnRole;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices
{
    /// <summary>
    /// Hosted service for synchronizing Altinn roles.
    /// </summary>
    public partial class AltinnRoleHostedServices(
        IAltinnLease lease,
        IAltinnRole role,
        IFeatureManager featureManager,
        ILogger<AltinnRoleHostedServices> logger,
        IIngestService ingestService,
        IStatusService statusService,
        IEntityRepository entityRepository,
        IRoleRepository roleRepository,
        IPackageRepository packageRepository,
        IAssignmentRepository assignmentRepository,
        IEntityTypeRepository entityTypeRepository,
        IEntityVariantRepository entityVariantRepository,
        IProviderRepository providerRepository,
        IDelegationService delegationService) : IHostedService, IDisposable
    {
        private readonly IAltinnLease _lease = lease;
        private readonly ILogger<AltinnRoleHostedServices> _logger = logger;
        private readonly IFeatureManager _featureManager = featureManager;
        private readonly IAltinnRole _role = role;

        private readonly IIngestService ingestService = ingestService;
        private readonly IStatusService statusService = statusService;
        private readonly IEntityRepository entityRepository = entityRepository;
        private readonly IRoleRepository roleRepository = roleRepository;
        private readonly IPackageRepository packageRepository = packageRepository;
        private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
        private readonly IEntityTypeRepository entityTypeRepository = entityTypeRepository;
        private readonly IEntityVariantRepository entityVariantRepository = entityVariantRepository;
        private readonly IProviderRepository providerRepository = providerRepository;
        private readonly IDelegationService delegationService = delegationService;

        private int _executionCount = 0;
        private Timer _timerAltinnRoles = null;
        private Timer _timerAdminRoles = null;
        private Timer _timerClientRoles = null;
        private readonly CancellationTokenSource _stop = new();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.StartAltinnRoleSync(_logger);

            _timerAltinnRoles = new Timer(async state => await SyncAltinnRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            //_timerAdminRoles = new Timer(async state => await SyncAdminRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _timerClientRoles = new Timer(async state => await SyncClientRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispatches the Altinn roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAltinnRoleDispatcher(object state)
        {
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("ral_access_management_altinnrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await PrepareSync(); // do db setup
                
                await SyncAllRoles(ls, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                return;
            }
            finally
            {
                await _lease.Release(ls, default);
            }
        }

        /// <summary>
        /// Dispatches the Client roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncClientRoleDispatcher(object state)
        {
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_clientrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await PrepareSync(); // do db setup

                await SyncClientRoles(ls, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                return;
            }
            finally
            {
                await _lease.Release(ls, default);
            }
        }

        /*
        /// <summary>
        /// Dispatches the Admin roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAdminRoleDispatcher(object state)
        {
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_adminrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await PrepareSync(); // do db setup

                await SyncAdminRoles(ls, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                return;
            }
            finally
            {
                await _lease.Release(ls, default);
            }
        }
        */

        private async Task<ImportClientDelegationRequestDto> CreateClientDelegationRequest(RoleDelegationModel delegationModel)
        {
            Guid? facilitatorPartyId = delegationModel.PerformedByPartyUuid;

            var request = new ImportClientDelegationRequestDto()
            {
                ClientId = delegationModel.FromPartyUuid, //TODO: Fix this to use the correct client id
                AgentId = delegationModel.ToUserPartyUuid ?? throw new Exception($"'delegationModel.ToUserPartyUuid' does not have value"),
                AgentRole = string.Empty, // TODO: Fix this to use the correct role 
                RolePackages = new List<CreateSystemDelegationRolePackageDto>(),
                Facilitator = facilitatorPartyId,
                DelegatedDateTimeOffset = delegationModel.DelegationChangeDateTime ?? throw new Exception($"'delegationModel.DelegatedDateTimeOffset' does not have value"),
            };

            var delegationContent = await CreateSystemDelegationRolePackageDtoForClientDelegation(delegationModel.RoleTypeCode);
            request.RolePackages.Add(delegationContent.Package);
            request.AgentRole = delegationContent.AgentRole;

            return request;
        }

        private async Task<(CreateSystemDelegationRolePackageDto Package, string AgentRole)> CreateSystemDelegationRolePackageDtoForClientDelegation(string roleTypeCode)
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

        /// <summary>
        /// Synchronizes altinn role data by first acquiring a remote lease and streaming altinn role entries.
        /// Returns if lease is already taken.
        /// </summary>
        /// <param name="ls">The lease result containing the lease data and status.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        private async Task SyncClientRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var batchData = new List<Assignment>();

            var test = await _role.StreamRoles("12", ls.Data?.AltinnRoleStreamNextPageLink, cancellationToken);

            await foreach (var page in test)
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
                        var delegationData = await CreateClientDelegationRequest(item);

                        if (delegationData.Facilitator == null)
                        {
                            continue; // When we do not have a facilitator party id we should not create the delegation
                        }

                        ChangeRequestOptions options = new ChangeRequestOptions()
                        {
                            ChangedBy = item.PerformedByUserUuid ?? AuditDefaults.Altinn2ClientImportSystem,
                            ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                        };

                        IEnumerable<Delegation> delegations = await delegationService.ImportClientDelegation(delegationData, options);

                        Interlocked.Increment(ref _executionCount);
                    }
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await _lease.Put(ls, new() { AltinnRoleStreamNextPageLink = page.Content.Links.Next }, cancellationToken);
                await _lease.RefreshLease(ls, cancellationToken);
            }
        }

        /*
        /// <summary>
        /// Synchronizes altinn role data by first acquiring a remote lease and streaming altinn role entries.
        /// Returns if lease is already taken.
        /// </summary>
        /// <param name="ls">The lease result containing the lease data and status.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        private async Task SyncAdminRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var batchData = new List<Assignment>();

            var test = await _role.StreamRoles("11", ls.Data?.AltinnRoleStreamNextPageLink, cancellationToken);

            await foreach (var page in test)
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
                        var assignment = await ConvertRoleDelegationModelToAssignment(item) ?? throw new Exception("Failed to convert RoleModel to Assignment");

                        if (batchData.Any(t => t.FromId == assignment.FromId && t.ToId == assignment.ToId && t.RoleId == assignment.RoleId))
                        {
                            // If changes on same assignment then execute as-is before continuing.
                            await Flush(batchId);
                        }

                        if (item.DelegationAction == DelegationAction.Delegate)
                        {
                            if (item.ToUserType != UserType.EnterpriseIdentified)
                            {
                                batchData.Add(assignment);
                            }
                        }
                        else
                        {
                            var filter = assignmentRepository.CreateFilterBuilder();
                            filter.Equal(t => t.FromId, assignment.FromId);
                            filter.Equal(t => t.ToId, assignment.ToId);
                            filter.Equal(t => t.RoleId, assignment.RoleId);
                            await assignmentRepository.Delete(filter);
                        }

                        Interlocked.Increment(ref _executionCount);
                    }
                }

                await Flush(batchId);

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await _lease.Put(ls, new() { AltinnRoleStreamNextPageLink = page.Content.Links.Next }, cancellationToken);
                await _lease.RefreshLease(ls, cancellationToken);

                async Task Flush(Guid batchId)
                {
                    try
                    {
                        _logger.LogInformation("Ingest and Merge Assignment batch '{0}' to db", batchName);
                        var ingested = await ingestService.IngestTempData<Assignment>(batchData, batchId, cancellationToken);

                        if (ingested != batchData.Count)
                        {
                            _logger.LogWarning("Ingest partial complete: Assignment ({0}/{1})", ingested, batchData.Count);
                        }

                        var merged = await ingestService.MergeTempData<Assignment>(batchId, GetAssignmentMergeMatchFilter, cancellationToken);

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
                }
            }
        }
        */

        /// <summary>
        /// Synchronizes altinn role data by first acquiring a remote lease and streaming altinn role entries.
        /// Returns if lease is already taken.
        /// </summary>
        /// <param name="ls">The lease result containing the lease data and status.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        private async Task SyncAllRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var batchData = new List<Assignment>();
            
            var test = await _role.StreamRoles("10", ls.Data?.AltinnRoleStreamNextPageLink, cancellationToken);
            
            await foreach (var page in test)
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
                        var assignment = await ConvertRoleDelegationModelToAssignment(item);
                        if (assignment.Asignment == null)
                        {
                            throw new Exception("Failed to convert RoleModel to Assignment");
                        }

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

                        previousOptions = assignment.Options;

                        if (item.DelegationAction == DelegationAction.Delegate)
                        {
                            if (item.ToUserType != UserType.EnterpriseIdentified)
                            {
                                batchData.Add(assignment.Asignment);
                            }
                        }
                        else
                        {
                            var filter = assignmentRepository.CreateFilterBuilder();
                            filter.Equal(t => t.FromId, assignment.Asignment.FromId);
                            filter.Equal(t => t.ToId, assignment.Asignment.ToId);
                            filter.Equal(t => t.RoleId, assignment.Asignment.RoleId);
                            await assignmentRepository.Delete(filter, previousOptions);
                        }

                        Interlocked.Increment(ref _executionCount);
                    }
                }

                await Flush(batchId);

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await _lease.Put(ls, new() { AltinnRoleStreamNextPageLink = page.Content.Links.Next }, cancellationToken);
                await _lease.RefreshLease(ls, cancellationToken);

                async Task Flush(Guid batchId)
                {
                    try
                    {
                        _logger.LogInformation("Ingest and Merge Assignment batch '{0}' to db", batchName);
                        var ingested = await ingestService.IngestTempData<Assignment>(batchData, batchId, previousOptions, cancellationToken);

                        if (ingested != batchData.Count)
                        {
                            _logger.LogWarning("Ingest partial complete: Assignment ({0}/{1})", ingested, batchData.Count);
                        }

                        var merged = await ingestService.MergeTempData<Assignment>(batchId, previousOptions, cancellationToken: cancellationToken);

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
                }
            }
        }

        private static readonly IReadOnlyList<GenericParameter> GetAssignmentMergeMatchFilter = new List<GenericParameter>()
        {
            new GenericParameter("fromid", "fromid"),
            new GenericParameter("roleid", "roleid"),
            new GenericParameter("toid", "toid")
        }.AsReadOnly();

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Log.QuitAltinnRoleSync(_logger);
            }
            finally
            {
                _timerAltinnRoles?.Change(Timeout.Infinite, 0);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /*
        private async Task<(List<Assignment> Roles, AssignmentPackage Package)> ConvertRoleDelegationModelToAdminAssignment(RoleDelegationModel model)
        {
            List<Assignment> roles = [];
            AssignmentPackage package = null;

            Guid roleId = await FetchRoleIdFromAdminRoleTypeCode(model.RoleTypeCode);

            // TODO: Add both role assignment and package assignment
            switch (model.RoleTypeCode)
            {
                case "APIADM":
                    roles.Add(await ConvertRoleDelegationModelToAssignment(model)); 
                    package = new AssignmentPackage()
                    {
                        Id = Guid.CreateVersion7(),
                        AssignmentId = roles[0].Id,
                        PackageId = Guid.CreateVersion7() // TODO fetch package id from somewhere
                    };
                    break;
                case "APIADMNUF":
                    roles.Add(await ConvertRoleDelegationModelToAssignment(model)); // TODO: Fix this to do it right
                    package = new AssignmentPackage()
                    {
                        Id = Guid.CreateVersion7(),
                        AssignmentId = roles[0].Id,
                        PackageId = Guid.CreateVersion7() // TODO fetch package id from somewhere
                    };
                    break;
                case "ADMAI" or "BOADM" or "HADM" or "KLADM":
                    roles.Add(await ConvertRoleDelegationModelToAssignment(model)); // TODO: Fix this to do it right
                    break;
                default:
                    throw new Exception(string.Format("Unknown role type code '{0}'", model.RoleTypeCode));
            }

            return (roles, package);
        }
        */

        private async Task<Guid> FetchRoleIdFromAdminRoleTypeCode(string roleCode)
        {
            var role = await GetOrCreateRole(roleCode, "Altinn2"); // TODO: Fix fetch correct role based on roleTypeCode
            return role.Id;
        }

        /*
        private async Task<Guid> FetchPackageIdFromAdminRoleTypeCode(string roleCode)
        {
            var role = await GetOrCreateRole(roleCode, "Altinn2"); //Todo: Fix fetch correct package based on roleTypeCode
            return role.Id;
        }
        */

        private async Task<(Assignment Asignment, ChangeRequestOptions Options)> ConvertRoleDelegationModelToAssignment(RoleDelegationModel model)
        {
            try
            {
                var role = await GetOrCreateRole(model.RoleTypeCode, "Altinn2");


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

        private async Task<Role> GetOrCreateRole(string roleCode, string roleSource)
        {
            string roleIdentifier = $"urn:altinn:rolecode:{roleCode}";

            var role = (await roleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
            if (role == null)
            {
                var provider = Providers.FirstOrDefault(t => t.Name == "Altinn2") ?? throw new Exception(string.Format("Provider '{0}' not found while creating new role.", roleSource));
                var entityType = EntityTypes.FirstOrDefault(t => t.Name == "Organisasjon") ?? throw new Exception(string.Format("Unable to find type '{0}'", "Organisasjon"));

                await roleRepository.Create(
                    new Role()
                    {
                        Id = Guid.CreateVersion7(),
                        Name = roleIdentifier,
                        Description = roleIdentifier,
                        Code = roleIdentifier,
                        Urn = roleIdentifier,
                        EntityTypeId = entityType.Id,
                        ProviderId = provider.Id,
                    },
                    new ChangeRequestOptions()
                    {
                        ChangedBy = AuditDefaults.Altinn2ClientImportSystem,
                        ChangedBySystem = AuditDefaults.Altinn2ClientImportSystem,
                    });

                role = (await roleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
                if (role == null)
                {
                    throw new Exception(string.Format("Unable to get or create role '{0}'", roleIdentifier));
                }
            }

            Roles.Add(role);
            return role;
        }

        private static partial class Log
        {
            [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Error occured while fetching data from sbl bridge, got {statusCode}")]
            internal static partial void ResponseError(ILogger logger, HttpStatusCode statusCode);

            [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Starting AltinnRole hosted service")]
            internal static partial void StartAltinnRoleSync(ILogger logger);

            [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An error occured while streaming data from sbl bridge")]
            internal static partial void SyncError(ILogger logger, Exception ex);

            [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Quit AltinnRole hosted service")]
            internal static partial void QuitAltinnRoleSync(ILogger logger);
        }

        /// <summary>
        /// Represents lease content, including pagination link.
        /// </summary>
        public class LeaseContent()
        {
            /// <summary>
            /// The URL of the next page of Party data.
            /// </summary>
            public string AltinnRoleStreamNextPageLink { get; set; }            
        }

        private async Task PrepareSync()
        {
            if (EntityTypes.Count > 0 || EntityVariants.Count > 0 || Roles.Count > 0 || Providers.Count > 0)
            {
                return;
            }

            EntityTypes = [.. await entityTypeRepository.Get()];
            EntityVariants = [.. await entityVariantRepository.Get()];
            Roles = [.. await roleRepository.Get()];
            Packages = [.. await packageRepository.Get()];
            Providers = [.. await providerRepository.Get()];
        }

        private List<Provider> Providers { get; set; } = [];

        private List<EntityType> EntityTypes { get; set; } = [];

        private List<EntityVariant> EntityVariants { get; set; } = [];

        private List<Role> Roles { get; set; } = [];

        private List<Package> Packages { get; set; } = [];
    }
}
