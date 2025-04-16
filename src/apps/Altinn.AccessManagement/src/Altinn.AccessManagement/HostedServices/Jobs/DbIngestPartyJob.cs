using System.Diagnostics;
using Altinn.AccessManagement.Core.Telemetry;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Integration.Platform.Register;

namespace Altinn.AccessManagement.HostedServices.Jobs;

public class DbIngestPartyJob(
        IAltinnRegister register,
        IStatusService statusService,
        IIngestService ingestService,
        IEntityTypeRepository entityTypeRepository,
        IEntityVariantRepository entityVariantRepository) : IJob
{
    private IAltinnRegister Register { get; } = register;

    private IStatusService StatusService { get; } = statusService;

    private IIngestService IngestService { get; } = ingestService;

    private IEntityTypeRepository EntityTypeRepository { get; } = entityTypeRepository;

    private IEntityVariantRepository EntityVariantRepository { get; } = entityVariantRepository;

    private static readonly IReadOnlyList<string> GetEntityMergeMatchFilter = new List<string>() { "id" }.AsReadOnly();

    private static readonly IReadOnlyList<string> GetEntityLookupMergeMatchFilter = new List<string>() { "entityid", "key" }.AsReadOnly();

    private ChangeRequestOptions Audit => new()
    {
        ChangedBy = AuditDefaults.RegisterImportSystem,
        ChangedBySystem = AuditDefaults.RegisterImportSystem,
    };

    /// <inheritdoc/>
    public async Task<bool> ShouldRun(JobContext context, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity("ShouldRun", ActivityKind.Internal);

        var partyStatus = await StatusService.GetOrCreateRecord(Guid.Parse("C18B67F6-B07E-482C-AB11-7FE12CD1F48D"), "accessmgmt-sync-register-party", Audit, 5);
        return await StatusService.TryToRun(partyStatus, Audit, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken = default)
    {
        var activity = TelemetryConfig.ActivitySource.StartActivity("Run", ActivityKind.Internal);

        var bulk = new List<Entity>();
        var bulkLookup = new List<EntityLookup>();
        var options = Audit;

        var entityTypes = (await EntityTypeRepository.Get(cancellationToken: cancellationToken)).ToList();
        var entityVariants = (await EntityVariantRepository.Get(cancellationToken: cancellationToken)).ToList();

        await foreach (var page in await Register.StreamParties(RegisterClient.AvailableFields, null, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return JobResult.Success;
            }

            if (!page.IsSuccessful)
            {
                return JobResult.Failure;
            }

            var batchId = options.ChangeOperationId;
            var batchName = options.ChangeOperationId.ToString().ToLower().Replace("-", string.Empty);

            if (page.Content != null)
            {
                foreach (var item in page.Content.Data)
                {
                    if (item.PartyType.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var entity = await ConvertPartyModel(item, options: options, entityVariants, entityTypes, cancellationToken: cancellationToken);

                    if (bulk.Any(t => t.Id.Equals(entity.Id)))
                    {
                        await Flush(batchId);
                    }

                    bulk.Add(entity);
                    bulkLookup.AddRange(ConvertPartyModelToLookup(item));
                }
            }

            await Flush(batchId);

            if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
            {
                return JobResult.Success;
            }

            async Task Flush(Guid batchId)
            {
                try
                {
                    // _logger.LogInformation("Ingest and Merge Entity and EntityLookup batch '{0}' to db", batchName);

                    var ingestedEntities = await IngestService.IngestTempData(bulk, batchId, options: options, cancellationToken: cancellationToken);
                    var ingestedLookups = await IngestService.IngestTempData(bulkLookup, batchId, options: options, cancellationToken: cancellationToken);

                    if (ingestedEntities != bulk.Count || ingestedLookups != bulkLookup.Count)
                    {
                        // _logger.LogWarning("Ingest partial complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", ingestedEntities, bulk.Count, ingestedLookups, bulkLookup.Count);
                    }

                    var mergedEntities = await IngestService.MergeTempData<Entity>(batchId, options: options, GetEntityMergeMatchFilter, cancellationToken: cancellationToken);
                    var mergedLookups = await IngestService.MergeTempData<EntityLookup>(batchId, options: options, GetEntityLookupMergeMatchFilter, cancellationToken: cancellationToken);

                    // _logger.LogInformation("Merge complete: Entity ({0}/{1}) EntityLookup ({2}/{3})", mergedEntities, ingestedEntities, mergedLookups, ingestedLookups);
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "Failed to ingest and/or merge Entity and EntityLookup batch {0} to db", batchName);
                    await Task.Delay(2000, cancellationToken);
                }
                finally
                {
                    bulk.Clear();
                    bulkLookup.Clear();
                }
            }

            return JobResult.Success;
        }

        return JobResult.Success;
    }

    private async Task<Entity> ConvertPartyModel(PartyModel model, ChangeRequestOptions options, List<EntityVariant> entityVariants, List<EntityType> entityTypes, bool createTypeIfMissing = false, CancellationToken cancellationToken = default)
    {
        if (model.PartyType.Equals("person", StringComparison.OrdinalIgnoreCase))
        {
            var type = entityTypes.FirstOrDefault(t => t.Name == "Person") ?? throw new Exception(string.Format("Unable to find type '{0}'", "Person"));
            var variant = entityVariants.FirstOrDefault(t => t.TypeId == type.Id && t.Name == "Person");
            if (variant == null)
            {
                variant = new EntityVariant() { Id = Guid.NewGuid(), Name = model.UnitType, Description = "Unknown", TypeId = type.Id };
                var res = await EntityVariantRepository.Create(variant, options: options, cancellationToken: cancellationToken);
                if (res == 0)
                {
                    throw new Exception(string.Format("Unable to find or create variant '{0}' for type '{1}'", model.UnitType, type.Name));
                }

                entityVariants.Add(variant);
            }

            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.PersonIdentifier,
                TypeId = type.Id,
                VariantId = variant.Id
            };
        }
        else if (model.PartyType.Equals("organization", StringComparison.OrdinalIgnoreCase))
        {
            var type = entityTypes.FirstOrDefault(t => t.Name == "Organisasjon") ?? throw new Exception(string.Format("Unable to find type '{0}'", "Organisasjon"));
            var variant = entityVariants.FirstOrDefault(t => t.TypeId == type.Id && t.Name.Equals(model.UnitType, StringComparison.OrdinalIgnoreCase));
            if (variant == null)
            {
                variant = new EntityVariant() { Id = Guid.NewGuid(), Name = model.UnitType, Description = "Unknown", TypeId = type.Id };
                var res = await EntityVariantRepository.Create(variant, options: options, cancellationToken: cancellationToken);
                if (res == 0)
                {
                    throw new Exception(string.Format("Unable to find or create variant '{0}' for type '{1}'", model.UnitType, type.Name));
                }

                entityVariants.Add(variant);
            }

            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.OrganizationIdentifier,
                TypeId = type.Id,
                VariantId = variant.Id
            };
        }
        else if (model.PartyType.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase))
        {
            var type = entityTypes.FirstOrDefault(t => t.Name == "Person") ?? throw new Exception(string.Format("Unable to find type '{0}'", "Person"));
            var variant = entityVariants.FirstOrDefault(t => t.TypeId == type.Id && t.Name == "SI") ?? throw new Exception(string.Format("Unable to find variant '{0}' for type '{1}'", "SI", type.Name));
            if (variant == null)
            {
                variant = new EntityVariant() { Id = Guid.NewGuid(), Name = model.UnitType, Description = "Unknown", TypeId = type.Id };
                var res = await EntityVariantRepository.Create(variant, options: options, cancellationToken: cancellationToken);
                if (res == 0)
                {
                    throw new Exception(string.Format("Unable to find or create variant '{0}' for type '{1}'", model.UnitType, type.Name));
                }

                entityVariants.Add(variant);
            }

            return new Entity()
            {
                Id = Guid.Parse(model.PartyUuid),
                Name = model.DisplayName,
                RefId = model.VersionId.ToString(),
                TypeId = type.Id,
                VariantId = variant.Id
            };
        }
        else
        {
            if (createTypeIfMissing)
            {
                var type = entityTypes.FirstOrDefault(t => t.Name.Equals(model.PartyType, StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    type = entityTypes.FirstOrDefault(t => t.Name.Equals("Ukjent", StringComparison.OrdinalIgnoreCase)) ?? throw new Exception("EntityType 'Ukjent' not found");
                }

                var variant = entityVariants.FirstOrDefault(t => t.TypeId == type.Id && t.Name.Equals(model.UnitType, StringComparison.OrdinalIgnoreCase)) ??
                    throw new Exception(string.Format("Unable to fint variant '{0}' for type '{1}'", model.PartyType, model.UnitType));

                if (variant == null)
                {
                    variant = new EntityVariant() { Id = Guid.NewGuid(), Name = model.UnitType, Description = "Unknown", TypeId = type.Id };
                    var res = EntityVariantRepository.Create(variant, options: options, cancellationToken: cancellationToken).Result;
                    if (res == 0)
                    {
                        throw new Exception(string.Format("Unable to create variant '{0}' for type '{1}'", model.UnitType, type.Name));
                    }

                    entityVariants.Add(variant);
                }

                return new Entity()
                {
                    Id = Guid.Parse(model.PartyUuid),
                    Name = model.DisplayName,
                    RefId = string.Empty,
                    TypeId = type.Id,
                    VariantId = variant.Id
                };
            }
            else
            {
                throw new Exception(string.Format("Unable to find type '{0}' and variant '{1}'", model.PartyType, model.UnitType));
            }
        }
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
                Value = model.PersonIdentifier
            });
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
}
