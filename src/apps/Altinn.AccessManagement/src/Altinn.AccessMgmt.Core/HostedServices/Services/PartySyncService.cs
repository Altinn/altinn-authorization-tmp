using System.Runtime.Serialization;
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
using Microsoft.EntityFrameworkCore;
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

        await foreach (var page in await _register.StreamParties(AltinnRegisterClient.DefaultFields, leaseData?.PartyStreamNextPageLink, cancellationToken))
        {
            if (page.IsProblem)
            {
                Log.ResponseError(_logger, page.StatusCode);
                throw new Exception("Stream page is not successful");
            }

            _logger.LogInformation("Starting proccessing party page ({0}-{1})", page.Content.Stats.PageStart, page.Content.Stats.PageEnd);

            foreach (var item in page?.Content.Data ?? [])
            {
                var data = item switch
                {
                    Person person => MapPerson(person),
                    Organization organization => MapOrganization(organization),
                    SelfIdentifiedUser selfIdentifiedUser => MapSelfIdentifiedUser(selfIdentifiedUser),
                    SystemUser systemUser => MapSystemUser(systemUser),
                    EnterpriseUser enterpriseUser => MapEnterpriseUser(enterpriseUser),
                    _ => throw new InvalidDataException($"Unkown Party type {item.Type}"),
                };

                if ((item is SystemUser || item is EnterpriseUser) && item.IsDeleted.Value)
                {
                    await DeleteEntities(appDbContext, ingestEntities, ingestEntitiesLookup, item, cancellationToken);
                }

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

    private static async Task DeleteEntities(AppDbContext appDbContext, List<Entity> ingestEntities, List<EntityLookup> ingestEntitiesLookup, Party item, CancellationToken cancellationToken)
    {
        foreach (var e in ingestEntities.Where(e => e.Id == item.Uuid))
        {
            ingestEntities.Remove(e);
        }

        foreach (var e in ingestEntitiesLookup.Where(e => e.EntityId == item.Uuid))
        {
            ingestEntitiesLookup.Remove(e);
        }

        var entityLookups = await appDbContext.EntityLookups
            .AsTracking()
            .Where(e => e.EntityId == item.Uuid)
            .ToListAsync(cancellationToken);

        var entity = appDbContext.Entities
            .AsTracking()
            .FirstOrDefaultAsync(e => e.Id == item.Uuid, cancellationToken);

        if (entityLookups is { })
        {
            appDbContext.RemoveRange(entityLookups);
        }

        if (entity is { })
        {
            appDbContext.Remove(entity);
        }

        await appDbContext.SaveChangesAsync(cancellationToken);
    }

    private List<EntityLookup> AddDefaultEntityLookups(Party party)
    {
        var result = new List<EntityLookup>();
        if (party.PartyId.HasValue)
        {
            result.Add(NewEntityLookup("PartyId", party.PartyId.ToString()));
        }

        if (party.User.Value?.UserIds.Value is { } userIds)
        {
            result.AddRange(userIds.Select(userId => NewEntityLookup("UserId", userId.ToString())));
        }

        if (party.User.Value?.Username.Value is { } username)
        {
            result.Add(NewEntityLookup("Username", username));
        }

        return result;

        EntityLookup NewEntityLookup(string key, string value)
        {
            return new EntityLookup()
            {
                EntityId = party.Uuid,
                Key = key,
                Value = value,
                IsProtected = false,
            };
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
                Key = "PersonIdentifier",
                Value = person.PersonIdentifier.ToString(),
            },
        ];

        entityLookups.AddRange(AddDefaultEntityLookups(person));

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
                Key = "OrganizationIdentifier",
                Value = organization.OrganizationIdentifier.ToString(),
            },
        ];

        entityLookups.AddRange(AddDefaultEntityLookups(organization));

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

        List<EntityLookup> entityLookups = [];
        entityLookups.AddRange(AddDefaultEntityLookups(selfIdentifiedUser));

        return (entity, entityLookups);
    }

    private (Entity Entity, IEnumerable<EntityLookup> EntityLookups) MapSystemUser(SystemUser systemUser)
    {
        var systemTypeVariant = systemUser.SystemUserType.Value.Value switch
        {
            SystemUserType.ClientPartySystemUser => EntityVariantConstants.AgentSystem,
            SystemUserType.FirstPartySystemUser => EntityVariantConstants.StandardSystem,
            _ => throw new InvalidDataException($"Missing mapping for system type {systemUser.SystemUserType}")
        };

        var entity = new Entity()
        {
            Id = systemUser.Uuid,
            Name = systemUser.DisplayName.ToString(),
            RefId = systemUser.Uuid.ToString(),
            TypeId = EntityTypeConstants.SystemUser,
            VariantId = systemTypeVariant
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

        List<EntityLookup> entityLookups = [];
        entityLookups.AddRange(AddDefaultEntityLookups(enterpriseUser));
        return (entity, entityLookups);
    }
}
