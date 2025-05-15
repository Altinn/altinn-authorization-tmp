using System.Net;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessManagement.HostedServices.Services;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AltinnRole;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices
{
    /// <summary>
    /// Hosted service for synchronizing Altinn roles.
    /// </summary>
    public partial class AltinnRoleHostedService(
        IAltinnLease lease,
        IAltinnRole role,
        IFeatureManager featureManager,
        ILogger<AltinnRoleHostedService> logger,
        IIngestService ingestService,
        IStatusService statusService,
        IEntityRepository entityRepository,
        IRoleRepository roleRepository,
        IPackageRepository packageRepository,
        IAssignmentRepository assignmentRepository,
        IEntityTypeRepository entityTypeRepository,
        IEntityVariantRepository entityVariantRepository,
        IProviderRepository providerRepository,        
        IDelegationService delegationService,
        IAllAltinnRoleSyncService allAltinnRoleSyncService,
        IAltinnAdminRoleSyncService altinnAdminRoleSyncService,
        IAltinnClientRoleSyncService altinnClientRoleSyncService) : IHostedService, IDisposable
    {
        private readonly IAltinnLease _lease = lease;
        private readonly ILogger<AltinnRoleHostedService> _logger = logger;
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
        private readonly IAllAltinnRoleSyncService allAltinnRoleSyncService = allAltinnRoleSyncService;
        private readonly IAltinnAdminRoleSyncService altinnAdminRoleSyncService = altinnAdminRoleSyncService;
        private readonly IAltinnClientRoleSyncService altinnClientRoleSyncService = altinnClientRoleSyncService;

        private int _executionCount = 0;
        private Timer _timerAltinnRoles = null;
        private Timer _timerAdminRoles = null;
        private Timer _timerClientRoles = null;
        private readonly CancellationTokenSource _stop = new();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.StartAltinnRoleSync(_logger);

            _timerAltinnRoles = new Timer(async state => await SyncAllAltinnRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _timerAdminRoles = new Timer(async state => await SyncAdminRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            _timerClientRoles = new Timer(async state => await SyncClientRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispatches the Altinn roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAllAltinnRoleDispatcher(object state)
        {
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAllAltinnRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("ral3_access_management_altinnrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await allAltinnRoleSyncService.SyncAllAltinnRoles(ls, cancellationToken);
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
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnClientRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("ral3_access_management_clientrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await altinnClientRoleSyncService.SyncClientRoles(ls, cancellationToken);
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
        /// Dispatches the Admin roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAdminRoleDispatcher(object state)
        {
            if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnAdminRoleSync))
            {
                return;
            }

            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("ral3_access_management_adminrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await altinnAdminRoleSyncService.SyncAdminRoles(ls, cancellationToken);
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
                _timerAdminRoles?.Change(Timeout.Infinite, 0);
                _timerAdminRoles?.Change(Timeout.Infinite, 0);
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
                _timerAltinnRoles?.Dispose();
                _timerAdminRoles?.Dispose();
                _timerAdminRoles?.Dispose();
                _stop?.Cancel();
                _stop?.Dispose();
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

        /*
        private async Task<Guid> FetchRoleIdFromAdminRoleTypeCode(string roleCode)
        {
            var role = await GetOrCreateRole(roleCode, "Altinn2"); // TODO: Fix fetch correct role based on roleTypeCode
            return role.Id;
        }
        */

        /*
        private async Task<Guid> FetchPackageIdFromAdminRoleTypeCode(string roleCode)
        {
            var role = await GetOrCreateRole(roleCode, "Altinn2"); //Todo: Fix fetch correct package based on roleTypeCode
            return role.Id;
        }
        */

        /*
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
        */

        /*
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
        */

        /*
        private List<Provider> Providers { get; set; } = [];

        private List<EntityType> EntityTypes { get; set; } = [];

        private List<EntityVariant> EntityVariants { get; set; } = [];

        private List<Role> Roles { get; set; } = [];

        private List<Package> Packages { get; set; } = [];
        */
    }
}
