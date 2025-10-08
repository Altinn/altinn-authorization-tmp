using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Migrations;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.ModelUtils;
using Altinn.Register.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Altinn.AccessMgmt.Core.HostedServices.Services;

/// <inheritdoc />
public class PartySyncService : BaseSyncService, IPartySyncService
{
    private readonly ILogger<RegisterHostedService> _logger;
    private readonly IAltinnRegister _register;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _bulkSize = 10_000;

    /// <summary>
    /// PartySyncService Constructor
    /// </summary>
    public PartySyncService(
        IAltinnRegister register,
        ILogger<RegisterHostedService> logger,
        IServiceProvider serviceProvider
    )
    {
        _register = register;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Synchronizes register data by first acquiring a remote lease and streaming register entries.
    /// Returns if lease is already taken.
    /// </summary>
    /// <param name="lease">The lease result containing the lease data and status.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task SyncParty(ILease lease, CancellationToken cancellationToken)
    {
        var options = new AuditValues(SystemEntityConstants.RegisterImportSystem);
        var leaseData = await lease.Get<RegisterLease>(cancellationToken);

        var seen = new HashSet<string>();
        var ingestEntities = new List<Entity>();
        var ingestEntitiesLookup = new List<EntityLookup>();

        using var scope = _serviceProvider.CreateEFScope(options);
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ingestService = scope.ServiceProvider.GetRequiredService<IIngestService>();

        await foreach (var page in await _register.StreamParties(AltinnRegisterClient.AvailableFields, leaseData?.PartyStreamNextPageLink, cancellationToken))
        {
            if (page.IsProblem)
            {
                Log.ResponseError(_logger, page.StatusCode);
                throw new Exception("Stream page is not successful");
            }

            _logger.LogInformation("Starting proccessing party page ({0}-{1})", page.Content.Stats.PageStart, page.Content.Stats.PageEnd);

            foreach (var item in page?.Content.Data ?? [])
            {
                if (item is SystemUser)
                {
                    continue;
                }
                
                var data = item switch
                {
                    Person person => MapPerson(person),
                    Organization organization => MapOrganization(organization),
                    SelfIdentifiedUser selfIdentifiedUser => MapSelfIdentifiedUser(selfIdentifiedUser),
                    SystemUser systemUser => MapSystemUser(systemUser),
                    EnterpriseUser enterpriseUser => MapEnterpriseUser(enterpriseUser),
                    _ => throw new InvalidDataException($"Unkown Party type {item.Type}"),
                };

                if (!seen.Add(data.Entity.RefId))
                {
                    await Flush();
                }

                ingestEntities.Add(data.Entity);
                ingestEntitiesLookup.AddRange(data.EntityLookups);
            }

            await Flush();

            if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
            {
                return;
            }

            leaseData.PartyStreamNextPageLink = page.Content.Links.Next;
            await lease.Update(leaseData, cancellationToken);
        }

        async Task Flush()
        {
            var batchId = Guid.CreateVersion7();
            var batchName = batchId.ToString("N");
            
            if (ingestEntities.Count == 0 && ingestEntitiesLookup.Count == 0)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Ingest and Merge Entity and EntityLookup batch '{0}' to db", batchName);

                var ingestedEntities = await ingestService.IngestTempData(ingestEntities, batchId, cancellationToken);
                var ingestedLookups = await ingestService.IngestTempData(ingestEntitiesLookup, batchId, cancellationToken);

                if (ingestedEntities != ingestEntities.Count || ingestedLookups != ingestEntitiesLookup.Count)
                {
                    _logger.LogWarning("Ingest partial complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", ingestedEntities, ingestEntities.Count, ingestedLookups, ingestEntitiesLookup.Count);
                }

                var mergedEntities = await ingestService.MergeTempData<Entity>(batchId, options, ["id"], cancellationToken);
                var mergedLookups = await ingestService.MergeTempData<EntityLookup>(batchId, options, ["entityid", "key"], cancellationToken);

                _logger.LogInformation("Merge complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", mergedEntities, ingestedEntities, mergedLookups, ingestedLookups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest and/or merge Entity and EntityLookup batch {0} to db", batchName);
                await Task.Delay(2000, cancellationToken);
            }
            finally
            {
                ingestEntities.Clear();
                ingestEntitiesLookup.Clear();
                seen.Clear();
            }
        }
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapPerson(Person person)
    {
        var entity = new Entity()
        {
            Id = person.Uuid,
            Name = person.DisplayName.ToString(),
            RefId = person.PersonIdentifier.ToString(),
            TypeId = EntityTypeConstants.Person,
            VariantId = EntityVariantConstants.Person,
        };

        List<EntityLookup> entityLookups = [
            new()
            {
                EntityId = person.Uuid,
                Key = "DateOfBirth",
                Value = person.DateOfBirth.ToString()
            },
            new()
            {
                EntityId = person.Uuid,
                Key = "PartyId",
                Value = person.PartyId.ToString(),
            },
            new()
            {
                EntityId = person.Uuid,
                Key = "PersonIdentifier",
                Value = person.PersonIdentifier.ToString(),
            },
        ];

        if (person.User.Value?.UserIds.Value is { } userIds)
        {
            entityLookups.AddRange(userIds.Select(userId => new EntityLookup()
            {
                EntityId = person.Uuid,
                Key = "UserId",
                Value = userId.ToString(),
                IsProtected = false,
            }));
        }

        return (entity, entityLookups);
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapOrganization(Organization organization)
    {
        if (!EntityVariantConstants.TryGetByName(organization.UnitType.Value, out var variant))
        {
            throw new InvalidDataException($"Invalid Unit Type {organization.UnitType}");
        }

        var entity = new Entity()
        {
            Id = organization.Uuid,
            Name = organization.DisplayName.ToString(),
            RefId = organization.OrganizationIdentifier.ToString(),
            TypeId = EntityTypeConstants.Organisation,
            VariantId = variant,
        };

        List<EntityLookup> entityLookups = [
            new EntityLookup()
            {
                EntityId = organization.Uuid,
                Key = "PartyId",
                Value = organization.PartyId.ToString()
            },

            new EntityLookup()
            {
                EntityId = organization.Uuid,
                Key = "OrganizationIdentifier",
                Value = organization.OrganizationIdentifier.ToString(),
            },
        ];

        return (entity, entityLookups);
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapSelfIdentifiedUser(SelfIdentifiedUser selfIdentifiedUser)
    {
        var entity = new Entity()
        {
            Id = selfIdentifiedUser.Uuid,
            Name = selfIdentifiedUser.DisplayName.Value,
            RefId = selfIdentifiedUser.User.Value.Username.Value,
            TypeId = EntityTypeConstants.Person,
            VariantId = EntityVariantConstants.SI
        };

        List<EntityLookup> entityLookups = [
            new()
            {
                EntityId = selfIdentifiedUser.Uuid,
                Key = "PartyId",
                Value = selfIdentifiedUser.PartyId.ToString(),
            },
        ];

        if (selfIdentifiedUser.User.Value?.UserIds.Value is { } userIds)
        {
            entityLookups.AddRange(userIds.Select(userId => new EntityLookup()
            {
                EntityId = selfIdentifiedUser.Uuid,
                Key = "UserId",
                Value = userId.ToString(),
                IsProtected = false,
            }));
        }

        return (entity, entityLookups);
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapSystemUser(SystemUser systemUser)
    {
        throw new NotImplementedException("Must get 'agent' and 'standard' type from register.");
        var entity = new Entity()
        {
            Id = systemUser.Uuid,
            Name = systemUser.DisplayName.ToString(),
            RefId = systemUser.Uuid.ToString(),
            TypeId = EntityTypeConstants.SystemUser,
            VariantId = EntityVariantConstants.AgentSystem,
        };

        return (entity, []);
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapEnterpriseUser(EnterpriseUser enterpriseUser)
    {
        var entity = new Entity()
        {
            Id = enterpriseUser.Uuid,
            Name = enterpriseUser.DisplayName.ToString(),
            RefId = enterpriseUser.User.Value.Username.Value,
            TypeId = EntityTypeConstants.EnterpriseUser,
            VariantId = EntityVariantConstants.EnterpriseUser,
        };

        List<EntityLookup> entityLookups = [
            new()
            {
                EntityId = enterpriseUser.Uuid,
                Key = "PartyId",
                Value = enterpriseUser.PartyId.Value.ToString(),
            }
        ];

        if (enterpriseUser.User.Value?.UserIds.Value is { } userIds)
        {
            entityLookups.AddRange(userIds.Select(userId => new EntityLookup()
            {
                EntityId = enterpriseUser.Uuid,
                Key = "UserId",
                Value = userId.ToString(),
                IsProtected = false,
            }));
        }

        return (entity, entityLookups);
    }
}
