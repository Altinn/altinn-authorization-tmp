using System.Diagnostics;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.HostedServices.Services;

/// <inheritdoc />
public class PartySyncService : BaseSyncService, IPartySyncService
{
    private readonly ILogger<RegisterHostedService> _logger;
    private readonly IIngestService _ingestService;
    private readonly IAltinnRegister _register;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// PartySyncService Constructor
    /// </summary>
    public PartySyncService(
        IAltinnRegister register,
        ILogger<RegisterHostedService> logger,
        IIngestService ingestService,
        IServiceProvider serviceProvider
    )
    {
        _register = register;
        _logger = logger;
        _ingestService = ingestService;
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
        var options = new AuditValues(
            AuditDefaults.RegisterImportSystem,
            AuditDefaults.RegisterImportSystem,
            Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString()
        );
        var leaseData = await lease.Get<RegisterLease>(cancellationToken);

        var seen = new HashSet<Guid>();
        var bulk = new List<Entity>();
        var bulkLookup = new List<EntityLookup>();

        using var scope = _serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        EntityTypes = await appDbContext.EntityTypes.AsNoTracking().ToListAsync(cancellationToken);
        EntityVariants = await appDbContext.EntityVariants.AsNoTracking().ToListAsync(cancellationToken);

        await foreach (var page in await _register.StreamParties(AltinnRegisterClient.AvailableFields, leaseData?.PartyStreamNextPageLink, cancellationToken))
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

            _logger.LogInformation("Starting proccessing party page ({0}-{1})", page.Content.Stats.PageStart, page.Content.Stats.PageEnd);

            foreach (var item in page?.Content.Data ?? [])
            {
                try
                {
                    if (item.PartyType.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var entity = ConvertPartyModel(item, cancellationToken: cancellationToken);
                    if (entity is { })
                    {
                        if (!seen.Add(entity.Id))
                        {
                            await Flush();
                        }

                        bulk.Add(entity);
                        bulkLookup.AddRange(ConvertPartyModelToLookup(item));
                    }
                    else
                    {
                        _logger.LogWarning("skipped adding entity of type '{type}' with id '{id}'", item.PartyType, item.PartyUuid);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "failed to sync party {partyUuid}", item.PartyUuid);
                    throw;
                }
            }

            await Flush();

            if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
            {
                return;
            }

            leaseData.PartyStreamNextPageLink = page.Content.Links.Next;
            await lease.Update(leaseData, cancellationToken);

            async Task Flush()
            {
                var batchId = Guid.CreateVersion7();
                var batchName = batchId.ToString("N");

                try
                {
                    _logger.LogInformation("Ingest and Merge Entity and EntityLookup batch '{0}' to db", batchName);

                    var ingestedEntities = await _ingestService.IngestTempData(bulk, batchId, cancellationToken);
                    var ingestedLookups = await _ingestService.IngestTempData(bulkLookup, batchId, cancellationToken);

                    if (ingestedEntities != bulk.Count || ingestedLookups != bulkLookup.Count)
                    {
                        _logger.LogWarning("Ingest partial complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", ingestedEntities, bulk.Count, ingestedLookups, bulkLookup.Count);
                    }

                    var mergedEntities = await _ingestService.MergeTempData<Entity>(batchId, options, ["id"], cancellationToken);
                    var mergedLookups = await _ingestService.MergeTempData<EntityLookup>(batchId, options, ["entityid", "key"], cancellationToken);

                    _logger.LogInformation("Merge complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", mergedEntities, ingestedEntities, mergedLookups, ingestedLookups);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest and/or merge Entity and EntityLookup batch {0} to db", batchName);
                    await Task.Delay(2000, cancellationToken);
                }
                finally
                {
                    bulk.Clear();
                    bulkLookup.Clear();
                    seen.Clear();
                }
            }
        }
    }

    private async Task<(EntityType Type, EntityVariant Variant)> GetOrCreateTypeAndVariant(AuditValues options, AppDbContext dbContext, string typeName, string variantName, bool autoCreateType, bool autoCreateVariant)
    {
        var tv = GetTypeAndVariant(typeName, variantName);
        if (tv.Type != null && tv.Variant != null)
        {
            return tv;
        }

        var type = tv.Type;
        if (type == null)
        {
            if (autoCreateType)
            {
                try
                {
                    type = new EntityType() { Id = Guid.NewGuid(), Name = typeName, ProviderId = AuditDefaults.RegisterImportSystem };
                    dbContext.EntityTypes.Add(type);
                    await dbContext.SaveChangesAsync(); // Add AuditValues on next merge
                    EntityTypes.Add(type);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to create type '{0}'", typeName));
                }
            }

            throw new Exception(string.Format("Unable to find type '{0}'", typeName));
        }

        var variant = tv.Variant;
        if (variant == null)
        {
            if (autoCreateVariant)
            {
                try
                {
                    variant = new EntityVariant() { Id = Guid.NewGuid(), Name = variantName, Description = "Unknown", TypeId = type.Id };
                    dbContext.EntityVariants.Add(variant);
                    await dbContext.SaveChangesAsync(); // Add AuditValues on next merge
                    EntityVariants.Add(variant);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to create variant '{0}' for type '{1}'", variantName, type.Name));
                }
            }

            throw new Exception(string.Format("Unable to find variant '{0}' for type '{1}'", variantName, type.Name));
        }

        return (type, variant);
    }

    private (EntityType Type, EntityVariant Variant) GetTypeAndVariant(string typeName, string variantName)
    {
        var type = EntityTypes.FirstOrDefault(t => t.Name == typeName) ?? throw new Exception(string.Format("Unable to find type '{0}'", typeName));
        var variant = EntityVariants.FirstOrDefault(t => t.TypeId == type.Id && t.Name.Equals(variantName, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Unable to find variant '{0}' for type '{1}'", variantName, type.Name));
        return (type, variant);
    }

    private Entity ConvertPartyModel(PartyModel model, CancellationToken cancellationToken = default)
    {
        /*
        // To add autoCreate for types and variants
        - Add AuditValues options, AppDbContext dbContext
        - Use GetOrCreateTypeAndVariant

        bool autoCreateTypes = false;
        bool autoCreateVariants = false;
        */

        if (model.PartyType.Equals("person", StringComparison.OrdinalIgnoreCase))
        {
            var tv = GetTypeAndVariant("Person", "Person");
            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.PersonIdentifier,
                TypeId = tv.Type.Id,
                VariantId = tv.Variant.Id
            };
        }
        else if (model.PartyType.Equals("organization", StringComparison.OrdinalIgnoreCase))
        {
            var tv = GetTypeAndVariant("Organisasjon", model.UnitType);
            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.OrganizationIdentifier,
                TypeId = tv.Type.Id,
                VariantId = tv.Variant.Id
            };
        }
        else if (model.PartyType.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase))
        {
            var tv = GetTypeAndVariant("Person", "SI");
            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.VersionId.ToString(),
                TypeId = tv.Type.Id,
                VariantId = tv.Variant.Id
            };
        }

        return null;
    }

    private List<EntityLookup> ConvertPartyModelToLookup(PartyModel model)
    {
        var res = new List<EntityLookup>();

        if (model.PartyType.Equals("person", StringComparison.OrdinalIgnoreCase))
        {
            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "DateOfBirth",
                Value = model.DateOfBirth
            });

            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "PartyId",
                Value = model.PartyId.ToString()
            });

            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "PersonIdentifier",
                Value = model.PersonIdentifier,
                IsProtected = true
            });
            if (model.User is { UserId: > 0 })
            {
                res.Add(new EntityLookup()
                {
                    EntityId = Guid.Parse(model.PartyUuid),
                    Key = "UserId",
                    Value = model.User.UserId.ToString(),
                    IsProtected = false,
                });
            }

            if (model.IsDeleted)
            {
                // DeletedAt missing in register. (18.juni. 2025)
                // res.Add(new EntityLookup()
                // {
                //     EntityId = Guid.Parse(model.PartyUuid),
                //     Key = "DeletedAt",
                //     Value = model.DeletedAt.ToUniversalTime().ToString(),
                //     IsProtected = false,
                // });
            }
        }
        else if (model.PartyType.Equals("organization", StringComparison.OrdinalIgnoreCase))
        {
            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "PartyId",
                Value = model.PartyId.ToString()
            });

            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "OrganizationIdentifier",
                Value = model.OrganizationIdentifier
            });
        }
        else if (model.PartyType.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase))
        {
            res.Add(new EntityLookup()
            {
                EntityId = Guid.Parse(model.PartyUuid),
                Key = "PartyId",
                Value = model.PartyId.ToString()
            });
        }

        return res;
    }

    private List<EntityType> EntityTypes { get; set; } = [];

    private List<EntityVariant> EntityVariants { get; set; } = [];
}
