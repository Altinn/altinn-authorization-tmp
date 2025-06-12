﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.Extensions.Configuration;

namespace Altinn.AccessMgmt.Persistence.Data;

/// <summary>
/// Service for running data migrations
/// </summary>
public class DbDataMigrationService(
        IProviderRepository providerRepository,
        IProviderTypeRepository providerTypeRepository,
        IAreaRepository areaService,
        IAreaGroupRepository areaGroupService,
        IConfiguration configuration,
        IEntityTypeRepository entityTypeService,
        IEntityVariantRepository entityVariantService,
        IEntityRepository entityRepository,
        IPackageRepository packageService,
        IRoleRepository roleService,
        IRolePackageRepository rolePackageRepository,
        IMigrationService migrationService,
        IIngestService ingestService
        )
{
    private readonly IProviderRepository providerRepository = providerRepository;
    private readonly IProviderTypeRepository providerTypeRepository = providerTypeRepository;
    private readonly IAreaRepository areaService = areaService;
    private readonly IAreaGroupRepository areaGroupService = areaGroupService;
    private readonly IEntityTypeRepository entityTypeService = entityTypeService;
    private readonly IEntityVariantRepository entityVariantService = entityVariantService;
    private readonly IEntityRepository entityRepository = entityRepository;
    private readonly IPackageRepository packageService = packageService;
    private readonly IRoleRepository roleService = roleService;
    private readonly IRolePackageRepository rolePackageRepository = rolePackageRepository;
    private readonly IMigrationService migrationService = migrationService;
    private readonly IIngestService ingestService = ingestService;
    private readonly string iconBaseUrl = configuration["AltinnCDN:AccessPackageIconsBaseURL"];

    /// <summary>
    /// Ingest all static data
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestAll(CancellationToken cancellationToken = default)
    {
        //// TODO: Add featureflags
        //// TODO: Add Activity logging

        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.StaticDataIngest,
            ChangedBySystem = AuditDefaults.StaticDataIngest
        };

        await Cleanup(options, cancellationToken);

        string dataKey = "<data>";

        if (migrationService.NeedMigration<ProviderType>(dataKey, 1))
        {
            await IngestProviderType(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<ProviderType>(dataKey, string.Empty, 1);
        }

        if (migrationService.NeedMigration<Provider>(dataKey, 3))
        {
            await IngestProvider(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<Provider>(dataKey, string.Empty, 3);
        }

        if (migrationService.NeedMigration<EntityType>(dataKey, 4))
        {
            await IngestEntityType(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<EntityType>(dataKey, string.Empty, 4);
        }

        if (migrationService.NeedMigration<EntityVariant>(dataKey, 4))
        {
            await IngestEntityVariant(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<EntityVariant>(dataKey, string.Empty, 4);
        }

        if (migrationService.NeedMigration<Entity>(dataKey, 2))
        {
            await IngestSystemEntity(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<Entity>(dataKey, string.Empty, 2);
        }

        if (migrationService.NeedMigration<Role>(dataKey, 11))
        {
            await IngestRole(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<Role>(dataKey, string.Empty, 11);
        }

        if (migrationService.NeedMigration<RoleMap>(dataKey, 5))
        {
            await IngestRoleMap(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<RoleMap>(dataKey, string.Empty, 5);
        }

        if (migrationService.NeedMigration<AreaGroup>(dataKey, 4))
        {
            await IngestAreaGroup(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<AreaGroup>(dataKey, string.Empty, 4);
        }

        if (migrationService.NeedMigration<Area>(dataKey, 5))
        {
            await IngestArea(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<Area>(dataKey, string.Empty, 5);
        }

        if (migrationService.NeedMigration<Package>(dataKey, 6))
        {
            await IngestPackage(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<Package>(dataKey, string.Empty, 6);
        }

        if (migrationService.NeedMigration<RolePackage>(dataKey, 4))
        {
            await IngestRolePackage(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<RolePackage>(dataKey, string.Empty, 4);
        }

        if (migrationService.NeedMigration<EntityVariantRole>(dataKey, 2))
        {
            await IngestEntityVariantRole(options: options, cancellationToken: cancellationToken);
            await migrationService.LogMigration<EntityVariantRole>(dataKey, string.Empty, 2);
        }
    }

    /// <summary>
    /// Ingest all static providertype data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestProviderType(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var data = new List<ProviderType>()
        {
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-7bb5-a35c-11d58ea36695"), Name = "System" },
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-713e-ad96-a9896d12f444"), Name = "Tjenesteeier" }
        };

        var dataEng = new List<ProviderType>()
        {
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-7bb5-a35c-11d58ea36695"), Name = "System" },
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-713e-ad96-a9896d12f444"), Name = "ServiceOwner" }
        };

        var dataNno = new List<ProviderType>()
        {
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-7bb5-a35c-11d58ea36695"), Name = "System" },
            new ProviderType() { Id = Guid.Parse("0195efb8-7c80-713e-ad96-a9896d12f444"), Name = "Tenesteeigar" }
        };

        foreach (var d in data)
        {
            await providerTypeRepository.Upsert(d, options: options, cancellationToken: cancellationToken);
        }

        foreach (var d in dataEng)
        {
            await providerTypeRepository.UpdateTranslation(d.Id, d, "eng", options: options, cancellationToken: cancellationToken);
        }

        foreach (var d in dataNno)
        {
            await providerTypeRepository.UpdateTranslation(d.Id, d, "nno", options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static provider data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestProvider(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var systemType = (await providerTypeRepository.Get(t => t.Name, "System")).FirstOrDefault() ?? throw new Exception("Providertype 'System' not found.");

        var systemProviders = new List<Provider>()
        {
            new Provider() { Id = Guid.Parse("0195ea92-2080-777d-8626-69c91ea2a05d"), Name = "Altinn 2", Code = "sys-altinn2", TypeId = systemType.Id },
            new Provider() { Id = Guid.Parse("0195ea92-2080-7e7c-bbe3-bb0521c1e51a"), Name = "Altinn 3", Code = "sys-altinn3", TypeId = systemType.Id },
            new Provider() { Id = Guid.Parse("0195ea92-2080-79d8-9859-0b26375f145e"), Name = "Ressursregisteret", Code = "sys-resreg", TypeId = systemType.Id },
            new Provider() { Id = Guid.Parse("0195ea92-2080-758b-89db-7735c4f68320"), Name = "Enhetsregisteret", Code = "sys-ccr", TypeId = systemType.Id }
        };

        await ingestService.IngestAndMergeData(systemProviders, options: options, ["code"], cancellationToken);
    }

    /// <summary>
    /// Ingest all static entitytype data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestEntityType(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var providerA3 = (await providerRepository.Get(t => t.Code, "sys-altinn3")).FirstOrDefault() ?? throw new KeyNotFoundException("Altinn3 provider not found");

        var entityTypes = new List<EntityType>()
        {
            new EntityType() { Id = Guid.Parse("8C216E2F-AFDD-4234-9BA2-691C727BB33D"), Name = "Organisasjon", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("BFE09E70-E868-44B3-8D81-DFE0E13E058A"), Name = "Person", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("FE643898-2F47-4080-85E3-86BF6FE39630"), Name = "Systembruker", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("4557CC81-C10D-40B4-8134-F8825060016E"), Name = "Intern", ProviderId = providerA3.Id },
        };

        var entityTypesNno = new List<EntityType>()
        {
            new EntityType() { Id = Guid.Parse("8C216E2F-AFDD-4234-9BA2-691C727BB33D"), Name = "Organisasjon", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("BFE09E70-E868-44B3-8D81-DFE0E13E058A"), Name = "Person", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("FE643898-2F47-4080-85E3-86BF6FE39630"), Name = "Systembrukar", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("4557CC81-C10D-40B4-8134-F8825060016E"), Name = "Intern", ProviderId = providerA3.Id },
        };

        var entityTypesEng = new List<EntityType>()
        {
            new EntityType() { Id = Guid.Parse("8C216E2F-AFDD-4234-9BA2-691C727BB33D"), Name = "Organization", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("BFE09E70-E868-44B3-8D81-DFE0E13E058A"), Name = "Person", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("FE643898-2F47-4080-85E3-86BF6FE39630"), Name = "SystemUser", ProviderId = providerA3.Id },
            new EntityType() { Id = Guid.Parse("4557CC81-C10D-40B4-8134-F8825060016E"), Name = "Internal", ProviderId = providerA3.Id },
        };

        foreach (var item in entityTypes)
        {
            await entityTypeService.Upsert(item, options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in entityTypesNno)
        {
            await entityTypeService.UpdateTranslation(item.Id, item, "nno", options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in entityTypesEng)
        {
            await entityTypeService.UpdateTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static entityvariant data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestEntityVariant(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var orgTypeId = (await entityTypeService.Get(t => t.Name, "Organisasjon")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found", "Organisasjon"));
        var persTypeId = (await entityTypeService.Get(t => t.Name, "Person")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found", "Person"));
        var systemTypeId = (await entityTypeService.Get(t => t.Name, "Systembruker")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found", "System"));
        var internalTypeId = (await entityTypeService.Get(t => t.Name, "Intern")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType '{0}' not found", "Intern"));

        var entityVariants = new List<EntityVariant>()
        {
            new EntityVariant() { Id = Guid.Parse("d786bc0e-8e9e-4116-bfc2-0344207c9127"), TypeId = orgTypeId, Name = "SAM", Description = "Tingsrettslig sameie" },
            new EntityVariant() { Id = Guid.Parse("d0a08401-5ae0-4da9-a79a-1113a7746b60"), TypeId = orgTypeId, Name = "VPFO", Description = "Verdipapirfond" },
            new EntityVariant() { Id = Guid.Parse("c161a605-3c72-40f2-8a5a-15e57e49638c"), TypeId = orgTypeId, Name = "UTLA", Description = "Utenlandsk enhet" },
            new EntityVariant() { Id = Guid.Parse("752f87dc-b04f-42cb-becd-173935ec6164"), TypeId = orgTypeId, Name = "BO", Description = "Andre bo" },
            new EntityVariant() { Id = Guid.Parse("263762ec-54fc-4eae-b7a1-17e92eea9a5c"), TypeId = orgTypeId, Name = "AS", Description = "Aksjeselskap" },
            new EntityVariant() { Id = Guid.Parse("6b2449e7-af5a-4c4e-b475-1b75998ba804"), TypeId = orgTypeId, Name = "PK", Description = "Pensjonskasse" },
            new EntityVariant() { Id = Guid.Parse("ed5d05b6-588c-40fa-8885-2bd36f75ac34"), TypeId = orgTypeId, Name = "PERS", Description = "Andre enkeltpersoner som registreres i tilknyttet register" },
            new EntityVariant() { Id = Guid.Parse("e0444411-a021-4774-854c-2ed876ffd64e"), TypeId = orgTypeId, Name = "EOFG", Description = "Europeisk økonomisk foretaksgruppe" },
            new EntityVariant() { Id = Guid.Parse("441a2876-f15f-4007-9e2e-3d25acbd98ff"), TypeId = orgTypeId, Name = "SE", Description = "Europeisk selskap" },
            new EntityVariant() { Id = Guid.Parse("3d5a890a-51aa-4e8a-b53d-3ec4111fe9e9"), TypeId = orgTypeId, Name = "TVAM", Description = "Tvangsregistrert for MVA" },
            new EntityVariant() { Id = Guid.Parse("e5d4a90e-948a-4c61-965a-43dbbd0efddb"), TypeId = orgTypeId, Name = "GFS", Description = "Gjensidig forsikringsselskap" },
            new EntityVariant() { Id = Guid.Parse("a581992e-c9dd-4250-8a9e-4e91d9b55424"), TypeId = orgTypeId, Name = "FYLK", Description = "Fylkeskommune" },
            new EntityVariant() { Id = Guid.Parse("ab5013e9-4210-4ab3-9fc2-554fd78a1b03"), TypeId = orgTypeId, Name = "IKJP", Description = "Andre ikke-juridiske personer" },
            new EntityVariant() { Id = Guid.Parse("a90417b4-5fa9-4a01-bfd0-57a1069a000c"), TypeId = orgTypeId, Name = "NUF", Description = "Norskregistrert utenlandsk foretak" },
            new EntityVariant() { Id = Guid.Parse("fca69e4d-453c-4404-b057-5e188c603f4b"), TypeId = orgTypeId, Name = "ANS", Description = "Ansvarlig selskap med solidarisk ansvar" },
            new EntityVariant() { Id = Guid.Parse("64b05309-7b6e-40c8-bd29-62d8fa0bd5ec"), TypeId = orgTypeId, Name = "KS", Description = "Kommandittselskap" },
            new EntityVariant() { Id = Guid.Parse("90b3eb3b-87cb-4ec3-bc44-65630cd02a67"), TypeId = orgTypeId, Name = "SÆR", Description = "Annet foretak iflg. særskilt lov" },
            new EntityVariant() { Id = Guid.Parse("d0648c3e-1567-48dc-a7cf-6837653dbc12"), TypeId = orgTypeId, Name = "IKS", Description = "Interkommunalt selskap" },
            new EntityVariant() { Id = Guid.Parse("9d80264a-b968-45f8-b740-6a283cbc06ad"), TypeId = orgTypeId, Name = "STI", Description = "Stiftelse" },
            new EntityVariant() { Id = Guid.Parse("8aa09ac2-dd61-492f-9613-6fc0558ab6fb"), TypeId = orgTypeId, Name = "BBL", Description = "Boligbyggelag" },
            new EntityVariant() { Id = Guid.Parse("e9b021a9-b257-42c0-8460-717a95c883f6"), TypeId = orgTypeId, Name = "KTRF", Description = "Kontorfellesskap" },
            new EntityVariant() { Id = Guid.Parse("2587990f-b036-4a6b-a7d9-815853be1382"), TypeId = orgTypeId, Name = "ANNA", Description = "Annen juridisk person" },
            new EntityVariant() { Id = Guid.Parse("7d356f18-2f72-49b5-a6f2-83d7c0871991"), TypeId = orgTypeId, Name = "SA", Description = "Samvirkeforetak" },
            new EntityVariant() { Id = Guid.Parse("e57cac52-e401-4c0f-a1cf-8bb4628fe671"), TypeId = orgTypeId, Name = "ADOS", Description = "Administrativ enhet - offentlig sektor" },
            new EntityVariant() { Id = Guid.Parse("3ae468d4-ea92-471d-b7b1-924e49b0d619"), TypeId = orgTypeId, Name = "KF", Description = "Kommunalt foretak" },
            new EntityVariant() { Id = Guid.Parse("0c31bb8f-587a-416b-a3cd-980bb73c5612"), TypeId = orgTypeId, Name = "AAFY", Description = "Underenhet til ikke-næringsdrivende" },
            new EntityVariant() { Id = Guid.Parse("b3433097-38b9-4a47-bd50-a4bb794cab3d"), TypeId = orgTypeId, Name = "DA", Description = "Ansvarlig selskap med delt ansvar" },
            new EntityVariant() { Id = Guid.Parse("4effb14f-8a1f-4272-aefb-b92ee302050f"), TypeId = orgTypeId, Name = "OPMV", Description = "Særskilt oppdelt enhet, jf. mval. § 2-2" },
            new EntityVariant() { Id = Guid.Parse("4f6c04d2-7223-41cc-8135-bb91d79ed311"), TypeId = orgTypeId, Name = "ORGL", Description = "Organisasjonsledd" },
            new EntityVariant() { Id = Guid.Parse("28157281-cc8f-46e0-9e2a-c20cb3b72930"), TypeId = orgTypeId, Name = "STAT", Description = "Staten" },
            new EntityVariant() { Id = Guid.Parse("d5b0abc8-22e7-44bb-bb55-c33bd6d7df4d"), TypeId = orgTypeId, Name = "SF", Description = "Statsforetak" },
            new EntityVariant() { Id = Guid.Parse("3aded080-d0d4-4893-8d30-c45dff4d7656"), TypeId = orgTypeId, Name = "PRE", Description = "Partrederi" },
            new EntityVariant() { Id = Guid.Parse("7c0ae1b2-2fa9-4266-8911-c4cb82c1489b"), TypeId = orgTypeId, Name = "BRL", Description = "Borettslag" },
            new EntityVariant() { Id = Guid.Parse("ed82281c-a5a1-4a28-9046-c70d95ce4658"), TypeId = orgTypeId, Name = "KOMM", Description = "Kommune" },
            new EntityVariant() { Id = Guid.Parse("ecd6e878-9121-43e6-aec0-c74b562cd3da"), TypeId = orgTypeId, Name = "FLI", Description = "Forening/lag/innretning" },
            new EntityVariant() { Id = Guid.Parse("80acaf52-3bf5-48c7-ab79-cb6f141a5b6f"), TypeId = orgTypeId, Name = "SPA", Description = "Sparebank" },
            new EntityVariant() { Id = Guid.Parse("ca45ed3a-41b3-4d2b-add9-db0d41a4e42b"), TypeId = orgTypeId, Name = "ASA", Description = "Allmennaksjeselskap" },
            new EntityVariant() { Id = Guid.Parse("1e2e44c0-e5e6-4962-8beb-e0ce16760a04"), TypeId = orgTypeId, Name = "ESEK", Description = "Eierseksjonssameie" },
            new EntityVariant() { Id = Guid.Parse("d78400f0-27d9-488a-886c-e264cc5c77ba"), TypeId = orgTypeId, Name = "ENK", Description = "Enkeltpersonforetak" },
            new EntityVariant() { Id = Guid.Parse("6b798668-a98d-49f2-a6d9-e391bad99fb2"), TypeId = orgTypeId, Name = "FKF", Description = "Fylkeskommunalt foretak" },
            new EntityVariant() { Id = Guid.Parse("7b43c6c2-e8ce-4f63-bb46-e4eb830fa222"), TypeId = orgTypeId, Name = "KIRK", Description = "Den norske kirke" },
            new EntityVariant() { Id = Guid.Parse("1f1e3720-b8a8-490e-8304-e81da21e3d3b"), TypeId = orgTypeId, Name = "BEDR", Description = "Underenhet til næringsdrivende og offentlig forvaltning" },
            new EntityVariant() { Id = Guid.Parse("d7208d54-067d-4b5c-a906-f0da3d3de0f1"), TypeId = orgTypeId, Name = "KBO", Description = "Konkursbo" },
            new EntityVariant() { Id = Guid.Parse("ea460099-515f-4e54-88d8-fbe53a807276"), TypeId = orgTypeId, Name = "BA", Description = "Selskap med begrenset ansvar" },
            new EntityVariant() { Id = Guid.Parse("b0690e14-7a75-45a4-8c02-437f6705b5ee"), TypeId = persTypeId, Name = "Person", Description = "Person" },
            new EntityVariant() { Id = Guid.Parse("8CA2FFDB-B4A9-4C64-8A9A-ED0F8DD722A3"), TypeId = systemTypeId, Name = "System", Description = "System" },
            new EntityVariant() { Id = Guid.Parse("03D08113-40D0-48BD-85B6-BD4430CCC182"), TypeId = persTypeId, Name = "SI", Description = "Selvidentifisert bruker" },
            new EntityVariant() { Id = Guid.Parse("CBE2834D-3DB0-4A14-BAA2-D32DE004D6D7"), TypeId = internalTypeId, Name = "Standard", Description = "Standard intern entitet" },
        };

        var entityVariantsEng = new List<EntityVariant>()
        {
            new EntityVariant() { Id = Guid.Parse("d786bc0e-8e9e-4116-bfc2-0344207c9127"), TypeId = orgTypeId, Name = "SAM", Description = "Legal co-ownership" },
            new EntityVariant() { Id = Guid.Parse("d0a08401-5ae0-4da9-a79a-1113a7746b60"), TypeId = orgTypeId, Name = "VPFO", Description = "Securities fund" },
            new EntityVariant() { Id = Guid.Parse("c161a605-3c72-40f2-8a5a-15e57e49638c"), TypeId = orgTypeId, Name = "UTLA", Description = "Foreign entity" },
            new EntityVariant() { Id = Guid.Parse("752f87dc-b04f-42cb-becd-173935ec6164"), TypeId = orgTypeId, Name = "BO", Description = "Other estate" },
            new EntityVariant() { Id = Guid.Parse("263762ec-54fc-4eae-b7a1-17e92eea9a5c"), TypeId = orgTypeId, Name = "AS", Description = "Limited company" },
            new EntityVariant() { Id = Guid.Parse("6b2449e7-af5a-4c4e-b475-1b75998ba804"), TypeId = orgTypeId, Name = "PK", Description = "Pension fund" },
            new EntityVariant() { Id = Guid.Parse("ed5d05b6-588c-40fa-8885-2bd36f75ac34"), TypeId = orgTypeId, Name = "PERS", Description = "Other individuals registered in the associated register" },
            new EntityVariant() { Id = Guid.Parse("e0444411-a021-4774-854c-2ed876ffd64e"), TypeId = orgTypeId, Name = "EOFG", Description = "European Economic Interest Grouping" },
            new EntityVariant() { Id = Guid.Parse("441a2876-f15f-4007-9e2e-3d25acbd98ff"), TypeId = orgTypeId, Name = "SE", Description = "European company" },
            new EntityVariant() { Id = Guid.Parse("3d5a890a-51aa-4e8a-b53d-3ec4111fe9e9"), TypeId = orgTypeId, Name = "TVAM", Description = "Compulsory VAT registration" },
            new EntityVariant() { Id = Guid.Parse("e5d4a90e-948a-4c61-965a-43dbbd0efddb"), TypeId = orgTypeId, Name = "GFS", Description = "Mutual insurance company" },
            new EntityVariant() { Id = Guid.Parse("a581992e-c9dd-4250-8a9e-4e91d9b55424"), TypeId = orgTypeId, Name = "FYLK", Description = "County municipality" },
            new EntityVariant() { Id = Guid.Parse("ab5013e9-4210-4ab3-9fc2-554fd78a1b03"), TypeId = orgTypeId, Name = "IKJP", Description = "Other non-legal persons" },
            new EntityVariant() { Id = Guid.Parse("a90417b4-5fa9-4a01-bfd0-57a1069a000c"), TypeId = orgTypeId, Name = "NUF", Description = "Norwegian-registered foreign company" },
            new EntityVariant() { Id = Guid.Parse("fca69e4d-453c-4404-b057-5e188c603f4b"), TypeId = orgTypeId, Name = "ANS", Description = "Partnership with joint liability" },
            new EntityVariant() { Id = Guid.Parse("64b05309-7b6e-40c8-bd29-62d8fa0bd5ec"), TypeId = orgTypeId, Name = "KS", Description = "Limited partnership" },
            new EntityVariant() { Id = Guid.Parse("90b3eb3b-87cb-4ec3-bc44-65630cd02a67"), TypeId = orgTypeId, Name = "SÆR", Description = "Other company according to special law" },
            new EntityVariant() { Id = Guid.Parse("d0648c3e-1567-48dc-a7cf-6837653dbc12"), TypeId = orgTypeId, Name = "IKS", Description = "Inter-municipal company" },
            new EntityVariant() { Id = Guid.Parse("9d80264a-b968-45f8-b740-6a283cbc06ad"), TypeId = orgTypeId, Name = "STI", Description = "Foundation" },
            new EntityVariant() { Id = Guid.Parse("8aa09ac2-dd61-492f-9613-6fc0558ab6fb"), TypeId = orgTypeId, Name = "BBL", Description = "Housing cooperative" },
            new EntityVariant() { Id = Guid.Parse("e9b021a9-b257-42c0-8460-717a95c883f6"), TypeId = orgTypeId, Name = "KTRF", Description = "Shared office" },
            new EntityVariant() { Id = Guid.Parse("2587990f-b036-4a6b-a7d9-815853be1382"), TypeId = orgTypeId, Name = "ANNA", Description = "Other legal entity" },
            new EntityVariant() { Id = Guid.Parse("7d356f18-2f72-49b5-a6f2-83d7c0871991"), TypeId = orgTypeId, Name = "SA", Description = "Cooperative enterprise" },
            new EntityVariant() { Id = Guid.Parse("e57cac52-e401-4c0f-a1cf-8bb4628fe671"), TypeId = orgTypeId, Name = "ADOS", Description = "Administrative unit - public sector" },
            new EntityVariant() { Id = Guid.Parse("3ae468d4-ea92-471d-b7b1-924e49b0d619"), TypeId = orgTypeId, Name = "KF", Description = "Municipal enterprise" },
            new EntityVariant() { Id = Guid.Parse("0c31bb8f-587a-416b-a3cd-980bb73c5612"), TypeId = orgTypeId, Name = "AAFY", Description = "Subunit of non-commercial entity" },
            new EntityVariant() { Id = Guid.Parse("b3433097-38b9-4a47-bd50-a4bb794cab3d"), TypeId = orgTypeId, Name = "DA", Description = "Partnership with divided liability" },
            new EntityVariant() { Id = Guid.Parse("4effb14f-8a1f-4272-aefb-b92ee302050f"), TypeId = orgTypeId, Name = "OPMV", Description = "Specially divided unit, cf. VAT Act § 2-2" },
            new EntityVariant() { Id = Guid.Parse("4f6c04d2-7223-41cc-8135-bb91d79ed311"), TypeId = orgTypeId, Name = "ORGL", Description = "Organizational unit" },
            new EntityVariant() { Id = Guid.Parse("28157281-cc8f-46e0-9e2a-c20cb3b72930"), TypeId = orgTypeId, Name = "STAT", Description = "The State" },
            new EntityVariant() { Id = Guid.Parse("d5b0abc8-22e7-44bb-bb55-c33bd6d7df4d"), TypeId = orgTypeId, Name = "SF", Description = "State enterprise" },
            new EntityVariant() { Id = Guid.Parse("3aded080-d0d4-4893-8d30-c45dff4d7656"), TypeId = orgTypeId, Name = "PRE", Description = "Partnership for ship ownership" },
            new EntityVariant() { Id = Guid.Parse("7c0ae1b2-2fa9-4266-8911-c4cb82c1489b"), TypeId = orgTypeId, Name = "BRL", Description = "Housing association" },
            new EntityVariant() { Id = Guid.Parse("ed82281c-a5a1-4a28-9046-c70d95ce4658"), TypeId = orgTypeId, Name = "KOMM", Description = "Municipality" },
            new EntityVariant() { Id = Guid.Parse("ecd6e878-9121-43e6-aec0-c74b562cd3da"), TypeId = orgTypeId, Name = "FLI", Description = "Association/club/institution" },
            new EntityVariant() { Id = Guid.Parse("80acaf52-3bf5-48c7-ab79-cb6f141a5b6f"), TypeId = orgTypeId, Name = "SPA", Description = "Savings bank" },
            new EntityVariant() { Id = Guid.Parse("ca45ed3a-41b3-4d2b-add9-db0d41a4e42b"), TypeId = orgTypeId, Name = "ASA", Description = "Public limited company" },
            new EntityVariant() { Id = Guid.Parse("1e2e44c0-e5e6-4962-8beb-e0ce16760a04"), TypeId = orgTypeId, Name = "ESEK", Description = "Condominium" },
            new EntityVariant() { Id = Guid.Parse("d78400f0-27d9-488a-886c-e264cc5c77ba"), TypeId = orgTypeId, Name = "ENK", Description = "Sole proprietorship" },
            new EntityVariant() { Id = Guid.Parse("6b798668-a98d-49f2-a6d9-e391bad99fb2"), TypeId = orgTypeId, Name = "FKF", Description = "County municipal enterprise" },
            new EntityVariant() { Id = Guid.Parse("7b43c6c2-e8ce-4f63-bb46-e4eb830fa222"), TypeId = orgTypeId, Name = "KIRK", Description = "The Church of Norway" },
            new EntityVariant() { Id = Guid.Parse("1f1e3720-b8a8-490e-8304-e81da21e3d3b"), TypeId = orgTypeId, Name = "BEDR", Description = "Subunit of commercial and public administration" },
            new EntityVariant() { Id = Guid.Parse("d7208d54-067d-4b5c-a906-f0da3d3de0f1"), TypeId = orgTypeId, Name = "KBO", Description = "Bankruptcy estate" },
            new EntityVariant() { Id = Guid.Parse("ea460099-515f-4e54-88d8-fbe53a807276"), TypeId = orgTypeId, Name = "BA", Description = "Limited liability company" },
            new EntityVariant() { Id = Guid.Parse("b0690e14-7a75-45a4-8c02-437f6705b5ee"), TypeId = persTypeId, Name = "Person", Description = "Person" },
            new EntityVariant() { Id = Guid.Parse("8CA2FFDB-B4A9-4C64-8A9A-ED0F8DD722A3"), TypeId = systemTypeId, Name = "System", Description = "System" },
            new EntityVariant() { Id = Guid.Parse("03D08113-40D0-48BD-85B6-BD4430CCC182"), TypeId = persTypeId, Name = "SI", Description = "Self-identified user" },
            new EntityVariant() { Id = Guid.Parse("CBE2834D-3DB0-4A14-BAA2-D32DE004D6D7"), TypeId = internalTypeId, Name = "Default", Description = "Default internal entity" },
        };

        var entityVariantsNno = new List<EntityVariant>()
        {
            new EntityVariant() { Id = Guid.Parse("d786bc0e-8e9e-4116-bfc2-0344207c9127"), TypeId = orgTypeId,    Name = "SAM",    Description = "Tingsrettslig sameie" },
            new EntityVariant() { Id = Guid.Parse("d0a08401-5ae0-4da9-a79a-1113a7746b60"), TypeId = orgTypeId,    Name = "VPFO",   Description = "Verdipapirfond" },
            new EntityVariant() { Id = Guid.Parse("c161a605-3c72-40f2-8a5a-15e57e49638c"), TypeId = orgTypeId,    Name = "UTLA",   Description = "Utanlandsk eining" },
            new EntityVariant() { Id = Guid.Parse("752f87dc-b04f-42cb-becd-173935ec6164"), TypeId = orgTypeId,    Name = "BO",     Description = "Andre bo" },
            new EntityVariant() { Id = Guid.Parse("263762ec-54fc-4eae-b7a1-17e92eea9a5c"), TypeId = orgTypeId,    Name = "AS",     Description = "Aksjeselskap" },
            new EntityVariant() { Id = Guid.Parse("6b2449e7-af5a-4c4e-b475-1b75998ba804"), TypeId = orgTypeId,    Name = "PK",     Description = "Pensjonskasse" },
            new EntityVariant() { Id = Guid.Parse("ed5d05b6-588c-40fa-8885-2bd36f75ac34"), TypeId = orgTypeId,    Name = "PERS",   Description = "Andre enkeltpersonar som registrerast i tilknytta register" },
            new EntityVariant() { Id = Guid.Parse("e0444411-a021-4774-854c-2ed876ffd64e"), TypeId = orgTypeId,    Name = "EOFG",   Description = "Europeisk økonomisk foretaksgruppe" },
            new EntityVariant() { Id = Guid.Parse("441a2876-f15f-4007-9e2e-3d25acbd98ff"), TypeId = orgTypeId,    Name = "SE",     Description = "Europeisk selskap" },
            new EntityVariant() { Id = Guid.Parse("3d5a890a-51aa-4e8a-b53d-3ec4111fe9e9"), TypeId = orgTypeId,    Name = "TVAM",   Description = "Tvangsregistrert for MVA" },
            new EntityVariant() { Id = Guid.Parse("e5d4a90e-948a-4c61-965a-43dbbd0efddb"), TypeId = orgTypeId,    Name = "GFS",    Description = "Gjensidig forsikringsselskap" },
            new EntityVariant() { Id = Guid.Parse("a581992e-c9dd-4250-8a9e-4e91d9b55424"), TypeId = orgTypeId,    Name = "FYLK",   Description = "Fylkeskommune" },
            new EntityVariant() { Id = Guid.Parse("ab5013e9-4210-4ab3-9fc2-554fd78a1b03"), TypeId = orgTypeId,    Name = "IKJP",   Description = "Andre ikkje-juridiske personar" },
            new EntityVariant() { Id = Guid.Parse("a90417b4-5fa9-4a01-bfd0-57a1069a000c"), TypeId = orgTypeId,    Name = "NUF",    Description = "Norskregistrert utanlandsk foretak" },
            new EntityVariant() { Id = Guid.Parse("fca69e4d-453c-4404-b057-5e188c603f4b"), TypeId = orgTypeId,    Name = "ANS",    Description = "Ansvarleg selskap med solidarisk ansvar" },
            new EntityVariant() { Id = Guid.Parse("64b05309-7b6e-40c8-bd29-62d8fa0bd5ec"), TypeId = orgTypeId,    Name = "KS",     Description = "Kommandittselskap" },
            new EntityVariant() { Id = Guid.Parse("90b3eb3b-87cb-4ec3-bc44-65630cd02a67"), TypeId = orgTypeId,    Name = "SÆR",    Description = "Annet foretak i følgje særskild lov" },
            new EntityVariant() { Id = Guid.Parse("d0648c3e-1567-48dc-a7cf-6837653dbc12"), TypeId = orgTypeId,    Name = "IKS",    Description = "Interkommunalt selskap" },
            new EntityVariant() { Id = Guid.Parse("9d80264a-b968-45f8-b740-6a283cbc06ad"), TypeId = orgTypeId,    Name = "STI",    Description = "Stiftelse" },
            new EntityVariant() { Id = Guid.Parse("8aa09ac2-dd61-492f-9613-6fc0558ab6fb"), TypeId = orgTypeId,    Name = "BBL",    Description = "Boligbyggelag" },
            new EntityVariant() { Id = Guid.Parse("e9b021a9-b257-42c0-8460-717a95c883f6"), TypeId = orgTypeId,    Name = "KTRF",   Description = "Kontorfellesskap" },
            new EntityVariant() { Id = Guid.Parse("2587990f-b036-4a6b-a7d9-815853be1382"), TypeId = orgTypeId,    Name = "ANNA",   Description = "Annan juridisk person" },
            new EntityVariant() { Id = Guid.Parse("7d356f18-2f72-49b5-a6f2-83d7c0871991"), TypeId = orgTypeId,    Name = "SA",     Description = "Samvirkeforetak" },
            new EntityVariant() { Id = Guid.Parse("e57cac52-e401-4c0f-a1cf-8bb4628fe671"), TypeId = orgTypeId,    Name = "ADOS",   Description = "Administrativ eining - offentleg sektor" },
            new EntityVariant() { Id = Guid.Parse("3ae468d4-ea92-471d-b7b1-924e49b0d619"), TypeId = orgTypeId,    Name = "KF",     Description = "Kommunalt foretak" },
            new EntityVariant() { Id = Guid.Parse("0c31bb8f-587a-416b-a3cd-980bb73c5612"), TypeId = orgTypeId,    Name = "AAFY",   Description = "Underenhet til ikkje-næringsdrivande" },
            new EntityVariant() { Id = Guid.Parse("b3433097-38b9-4a47-bd50-a4bb794cab3d"), TypeId = orgTypeId,    Name = "DA",     Description = "Ansvarleg selskap med delt ansvar" },
            new EntityVariant() { Id = Guid.Parse("4effb14f-8a1f-4272-aefb-b92ee302050f"), TypeId = orgTypeId,    Name = "OPMV",   Description = "Særskild oppdelt eining, jf. mval. § 2-2" },
            new EntityVariant() { Id = Guid.Parse("4f6c04d2-7223-41cc-8135-bb91d79ed311"), TypeId = orgTypeId,    Name = "ORGL",   Description = "Organisasjonsledd" },
            new EntityVariant() { Id = Guid.Parse("28157281-cc8f-46e0-9e2a-c20cb3b72930"), TypeId = orgTypeId,    Name = "STAT",   Description = "Staten" },
            new EntityVariant() { Id = Guid.Parse("d5b0abc8-22e7-44bb-bb55-c33bd6d7df4d"), TypeId = orgTypeId,    Name = "SF",     Description = "Statsforetak" },
            new EntityVariant() { Id = Guid.Parse("3aded080-d0d4-4893-8d30-c45dff4d7656"), TypeId = orgTypeId,    Name = "PRE",    Description = "Partrederi" },
            new EntityVariant() { Id = Guid.Parse("7c0ae1b2-2fa9-4266-8911-c4cb82c1489b"), TypeId = orgTypeId,    Name = "BRL",    Description = "Borettslag" },
            new EntityVariant() { Id = Guid.Parse("ed82281c-a5a1-4a28-9046-c70d95ce4658"), TypeId = orgTypeId,    Name = "KOMM",   Description = "Kommune" },
            new EntityVariant() { Id = Guid.Parse("ecd6e878-9121-43e6-aec0-c74b562cd3da"), TypeId = orgTypeId,    Name = "FLI",    Description = "Forening/lag/innretting" },
            new EntityVariant() { Id = Guid.Parse("80acaf52-3bf5-48c7-ab79-cb6f141a5b6f"), TypeId = orgTypeId,    Name = "SPA",    Description = "Sparebank" },
            new EntityVariant() { Id = Guid.Parse("ca45ed3a-41b3-4d2b-add9-db0d41a4e42b"), TypeId = orgTypeId,    Name = "ASA",    Description = "Allmennaksjeselskap" },
            new EntityVariant() { Id = Guid.Parse("1e2e44c0-e5e6-4962-8beb-e0ce16760a04"), TypeId = orgTypeId,    Name = "ESEK",   Description = "Eigarseksjonssameie" },
            new EntityVariant() { Id = Guid.Parse("d78400f0-27d9-488a-886c-e264cc5c77ba"), TypeId = orgTypeId,    Name = "ENK",    Description = "Enkeltpersonforetak" },
            new EntityVariant() { Id = Guid.Parse("6b798668-a98d-49f2-a6d9-e391bad99fb2"), TypeId = orgTypeId,    Name = "FKF",    Description = "Fylkeskommunalt foretak" },
            new EntityVariant() { Id = Guid.Parse("7b43c6c2-e8ce-4f63-bb46-e4eb830fa222"), TypeId = orgTypeId,    Name = "KIRK",   Description = "Den norske kyrkja" },
            new EntityVariant() { Id = Guid.Parse("1f1e3720-b8a8-490e-8304-e81da21e3d3b"), TypeId = orgTypeId,    Name = "BEDR",   Description = "Underenhet til næringsdrivande og offentleg forvaltning" },
            new EntityVariant() { Id = Guid.Parse("d7208d54-067d-4b5c-a906-f0da3d3de0f1"), TypeId = orgTypeId,    Name = "KBO",    Description = "Konkursbo" },
            new EntityVariant() { Id = Guid.Parse("ea460099-515f-4e54-88d8-fbe53a807276"), TypeId = orgTypeId,    Name = "BA",     Description = "Selskap med avgrensa ansvar" },
            new EntityVariant() { Id = Guid.Parse("b0690e14-7a75-45a4-8c02-437f6705b5ee"), TypeId = persTypeId,   Name = "PERS",   Description = "Person" },
            new EntityVariant() { Id = Guid.Parse("8CA2FFDB-B4A9-4C64-8A9A-ED0F8DD722A3"), TypeId = systemTypeId, Name = "System", Description = "System" },
            new EntityVariant() { Id = Guid.Parse("03D08113-40D0-48BD-85B6-BD4430CCC182"), TypeId = persTypeId, Name = "SI", Description = "Sjølvidentifisert brukar" },
            new EntityVariant() { Id = Guid.Parse("CBE2834D-3DB0-4A14-BAA2-D32DE004D6D7"), TypeId = internalTypeId, Name = "Standard", Description = "Standard intern entitet" },
        };

        foreach (var item in entityVariants)
        {
            await entityVariantService.Upsert(item, options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in entityVariantsEng)
        {
            await entityVariantService.UpsertTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in entityVariantsNno)
        {
            await entityVariantService.UpsertTranslation(item.Id, item, "nno", options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static entity data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestSystemEntity(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var internalTypeId = (await entityTypeService.Get(t => t.Name, "Intern")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType '{0}' not found", "Intern"));
        var internalVariantId = (await entityVariantService.Get(t => t.TypeId, internalTypeId)).FirstOrDefault(t => t.Name.Equals("Standard", StringComparison.OrdinalIgnoreCase))?.Id ?? throw new KeyNotFoundException(string.Format("EntityVariant '{0}' not found", "Intern"));

        var systemEntities = new List<Entity>()
        {
            // Static data ingest
            new Entity() { Id = AuditDefaults.StaticDataIngest, Name = "StaticDataIngest", RefId = "sys-static-data-ingest", ParentId = null, TypeId = internalTypeId, VariantId = internalVariantId },
            new Entity() { Id = AuditDefaults.RegisterImportSystem, Name = "RegisterImportSystem", RefId = "sys-register-import-system", ParentId = null, TypeId = internalTypeId, VariantId = internalVariantId },
            new Entity() { Id = AuditDefaults.EnduserApi, Name = "EnduserApi", RefId = "accessmgmt-enduser-api", ParentId = null, TypeId = internalTypeId, VariantId = internalVariantId },
            new Entity() { Id = AuditDefaults.Altinn2ClientImportSystem, Name = "Altinn2ClientImportSystem", RefId = "sys-altinn2-client-import-system", ParentId = null, TypeId = internalTypeId, VariantId = internalVariantId },
        };

        foreach (var item in systemEntities)
        {
            await entityRepository.Upsert(item, options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static role data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestRole(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var orgEntityTypeId = (await entityTypeService.Get(t => t.Name, "Organisasjon")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found '{0}'", "Organisasjon"));
        var persEntityTypeId = (await entityTypeService.Get(t => t.Name, "Person")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found '{0}'", "Person"));
        var ccrProviderId = (await providerRepository.Get(t => t.Code, "sys-ccr")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Provider not found '{0}'", "Enhetsregisteret"));
        var a3ProviderId = (await providerRepository.Get(t => t.Code, "sys-altinn3")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Provider not found '{0}'", "Altinn 3"));
        var a2ProviderId = (await providerRepository.Get(t => t.Code, "sys-altinn2")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Provider not found '{0}'", "Altinn 2"));


        var roles = new List<Role>()
        {
            new Role() { Id = Guid.Parse("42CAE370-2DC1-4FDC-9C67-C2F4B0F0F829"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Rettighetshaver",              Code = "rettighetshaver",               Description = "Gir mulighet til å motta delegerte fullmakter for virksomheten", Urn = "urn:altinn:role:rettighetshaver", IsKeyRole = false, IsAssignable = true },
            new Role() { Id = Guid.Parse("FF4C33F5-03F7-4445-85ED-1E60B8AAFB30"), EntityTypeId = persEntityTypeId, ProviderId = a3ProviderId, Name = "Agent",                       Code = "agent",                         Description = "Gir mulighet til å motta delegerte fullmakter for virksomheten", Urn = "urn:altinn:role:agent", IsKeyRole = false, IsAssignable = true },
            new Role() { Id = Guid.Parse("6795081e-e69c-4efd-8d42-2bfccd346777"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Klientadministrator",          Code = "klientadministrator",           Description = "Gir mulighet til å administrere tilgang til tjenester videre til ansatte på vegne av deres kunder", Urn = "urn:altinn:role:klientadministrator", IsKeyRole = false, IsAssignable = true },
            new Role() { Id = Guid.Parse("6c1fbcb9-609c-4ab8-a048-3be8d7da5a82"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Tilgangsstyrer",               Code = "tilgangsstyrer",                Description = "Gir mulighet til å gi videre tilganger for virksomheten som man selv har mottatt", Urn = "urn:altinn:role:tilgangsstyrer", IsKeyRole = false, IsAssignable = true },
            new Role() { Id = Guid.Parse("ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Hovedadministrator",           Code = "hovedadministrator",            Description = "Gir mulighet til å administrere alle tilganger for virksomheten", Urn = "urn:altinn:role:hovedadministrator", IsKeyRole = false, IsAssignable = true },
            new Role() { Id = Guid.Parse("b3f5c1e8-4e3b-4d2a-8c3e-1f2b3d4e5f6a"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Maskinporten administrator",   Code = "maskinporten-administrator",    Description = "Gir bruker mulighet til å administrere tilgang til maskinporten scopes", Urn = "urn:altinn:role:maskinporten-administrator", IsKeyRole = false, IsAssignable = true },

            new Role() { Id = Guid.Parse("66ad5542-4f4a-4606-996f-18690129ce00"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Administrativ enhet - offentlig sektor",      /*"ADOS"*/  Code = "administrativ-enhet-offentlig-sektor",  Description = "Administrativ enhet - offentlig sektor", Urn = "urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("29a24eab-a25f-445d-b56d-e3b914844853"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Nestleder",                                   /*"NEST"*/  Code = "nestleder",                             Description = "Styremedlem som opptrer som styreleder ved leders fravær", Urn = "urn:altinn:external-role:ccr:nestleder", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("8c1e91c2-a71c-4abf-a74e-a600a98be976"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i kontorfellesskap",                   /*"KTRF"*/  Code = "kontorfelleskapmedlem",                 Description = "Inngår i kontorfellesskap", Urn = "urn:altinn:external-role:ccr:kontorfelleskapmedlem", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("cfc41a92-2061-4ff4-97dc-658ffba2c00e"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Organisasjonsledd i offentlig sektor",        /*"ORGL"*/  Code = "organisasjonsledd-offentlig-sektor",    Description = "Organisasjonsledd i offentlig sektor", Urn = "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("2fec6d4b-cead-419a-adf3-1bf482a3c9dc"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Særskilt oppdelt enhet",                      /*"OPMV"*/  Code = "saerskilt-oppdelt-enhet",               Description = "Særskilt oppdelt enhet", Urn = "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("55bd7d4d-08dd-46ee-ac8e-3a44d800d752"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Daglig leder",                                /*"DAGL"*/  Code = "daglig-leder",                          Description = "Fysisk- eller juridisk person som har ansvaret for den daglige driften i en virksomhet", Urn = "urn:altinn:external-role:ccr:daglig-leder", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("18baa914-ac43-4663-9fa4-6f5760dc68eb"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Deltaker delt ansvar",                        /*"DTPR"*/  Code = "deltaker-delt-ansvar",                  Description = "Fysisk- eller juridisk person som har personlig ansvar for deler av selskapets forpliktelser", Urn = "urn:altinn:external-role:ccr:deltaker-delt-ansvar", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("2651ed07-f31b-4bc1-87bd-4d270742a19d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Innehaver",                                   /*"INNH"*/  Code = "innehaver",                             Description = "Fysisk person som er eier av et enkeltpersonforetak", Urn = "urn:altinn:external-role:ccr:innehaver", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("f1021b8c-9fbc-4296-bd17-a05d713037ef"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Deltaker fullt ansvar",                       /*"DTSO"*/  Code = "deltaker-fullt-ansvar",                 Description = "Fysisk- eller juridisk person som har ubegrenset, personlig ansvar for selskapets forpliktelser", Urn = "urn:altinn:external-role:ccr:deltaker-fullt-ansvar", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("d41d67f2-15b0-4c82-95db-b8d5baaa14a4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Varamedlem",                                  /*"VARA"*/  Code = "varamedlem",                            Description = "Fysisk- eller juridisk person som er stedfortreder for et styremedlem", Urn = "urn:altinn:external-role:ccr:varamedlem", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("1f8a2518-9494-468a-80a0-7405f0daf9e9"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Observatør",                                  /*"OBS" */  Code = "observator",                            Description = "Fysisk person som deltar i styremøter i en virksomhet, men uten stemmerett", Urn = "urn:altinn:external-role:ccr:observator", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("f045ffda-dbdc-41da-b674-b9b276ad5b01"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Styremedlem",                                 /*"MEDL"*/  Code = "styremedlem",                           Description = "Fysisk- eller juridisk person som inngår i et styre", Urn = "urn:altinn:external-role:ccr:styremedlem", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Styrets leder",                               /*"LEDE"*/  Code = "styreleder",                            Description = "Fysisk- eller juridisk person som er styremedlem og leder et styre", Urn = "urn:altinn:external-role:ccr:styreleder", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("2e2fc06e-d9b7-4cd9-91bc-d5de766d20de"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Den personlige konkursen angår",              /*"KENK"*/  Code = "personlige-konkurs",                    Description = "Den personlige konkursen angår", Urn = "urn:altinn:external-role:ccr:personlige-konkurs", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("e852d758-e8dd-41ec-a1e2-4632deb6857d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Norsk representant for utenlandsk enhet",     /*"REPR"*/  Code = "norsk-representant",                    Description = "Fysisk- eller juridisk person som har ansvaret for den daglige driften i Norge", Urn = "urn:altinn:external-role:ccr:norsk-representant", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("db013059-4a8a-442d-bf90-b03539fe5dda"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson",                               /*"KONT"*/  Code = "kontaktperson",                         Description = "Fysisk person som representerer en virksomhet", Urn = "urn:altinn:external-role:ccr:kontaktperson", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("69c4397a-9e34-4e73-9f69-534bc1bb74c8"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson NUF",                           /*"KNUF"*/  Code = "kontaktperson-nuf",                     Description = "Fysisk person som representerer en virksomhet - NUF", Urn = "urn:altinn:external-role:ccr:kontaktperson-nuf", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("8f0cf433-954e-4680-a25d-a3cf9ffdf149"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Bestyrende reder",                            /*"BEST"*/  Code = "bestyrende-reder",                      Description = "Bestyrende reder", Urn = "urn:altinn:external-role:ccr:bestyrende-reder", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("9ce84a4d-4970-4ef2-8208-b8b8f4d45556"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Eierkommune",                                 /*"EIKM"*/  Code = "eierkommune",                           Description = "Eierkommune", Urn = "urn:altinn:external-role:ccr:eierkommune", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("2cacfb35-2346-4a8d-95f6-b6fa4206881c"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Bobestyrer",                                  /*"BOBE"*/  Code = "bostyrer",                              Description = "Bestyrer av et konkursbo eller dødsbo som er under offentlig skiftebehandling", Urn = "urn:altinn:external-role:ccr:bostyrer", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("e4674211-034a-45f3-99ac-b2356984968a"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Helseforetak",                                /*"HLSE"*/  Code = "helseforetak",                          Description = "Helseforetak", Urn = "urn:altinn:external-role:ccr:helseforetak", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("f76b997a-9bd8-4f7b-899f-fcd85d35669f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Revisor",                                     /*"REVI"*/  Code = "revisor",                               Description = "Revisor", Urn = "urn:altinn:external-role:ccr:revisor", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("348b2f47-47ee-4084-abf8-68aa54c2b27f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Forretningsfører",                            /*"FFØR"*/  Code = "forretningsforer",                      Description = "Forretningsfører", Urn = "urn:altinn:external-role:ccr:forretningsforer", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("cfcf75af-9902-41f7-ab47-b77ba60bcae5"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Komplementar",                                /*"KOMP"*/  Code = "komplementar",                          Description = "Komplementar", Urn = "urn:altinn:external-role:ccr:komplementar", IsKeyRole = true, IsAssignable = false },
            new Role() { Id = Guid.Parse("50cc3f41-4dde-4417-8c04-eea428f169dd"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Konkursdebitor",                              /*"KDEB"*/  Code = "konkursdebitor",                        Description = "Konkursdebitor", Urn = "urn:altinn:external-role:ccr:konkursdebitor", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("d78dd1d8-a3f3-4ae6-807e-ea5149f47035"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i kirkelig fellesråd",                 /*"KIRK"*/  Code = "kirkelig-fellesraad",                   Description = "Inngår i kirkelig fellesråd", Urn = "urn:altinn:external-role:ccr:kirkelig-fellesraad", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("185f623b-f614-4a83-839c-1788764bd253"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Opplysninger om foretaket i hjemlandet",      /*"HFOR"*/  Code = "hovedforetak",                          Description = "Opplysninger om foretaket i hjemlandet", Urn = "urn:altinn:external-role:ccr:hovedforetak", IsKeyRole = false, IsAssignable = false },
            new Role() { Id = Guid.Parse("46e27685-b3ba-423e-8b42-faab54de5817"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Regnskapsfører",                              /*"REGN"*/  Code = "regnskapsforer",                        Description = "Regnskapsfører", Urn = "urn:altinn:external-role:ccr:regnskapsforer", IsKeyRole = false, IsAssignable = false },

            new Role() { Id = Guid.Parse("17cb6a9e-5d27-4a8e-9647-f3a53c7a09c6"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Er regnskapsforeradresse for",                /*"RFAD"*/  Code = "regnskapsforeradressat",                Description = "Er regnskapsforeradresse for", Urn = "urn:altinn:external-role:ccr:regnskapsforeradressat", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("ea8f1038-9717-472d-a579-f32960f0eecb"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Signatur",                                    /*"SIGN"*/  Code = "signerer",                              Description = "Signatur", Urn = "urn:altinn:external-role:ccr:signerer", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("9822b632-3822-4a9e-b768-8411c046bb75"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Skal fusjoneres med",                         /*"FUSJ"*/  Code = "fusjonsovertaker",                      Description = "Skal fusjoneres med", Urn = "urn:altinn:external-role:ccr:fusjonsovertaker", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("e9292053-92ee-42e0-a30c-011667ee8db8"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Skal fisjoneres med",                         /*"FISJ"*/  Code = "fisjonsovertaker",                      Description = "Skal fisjoneres med", Urn = "urn:altinn:external-role:ccr:fisjonsovertaker", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("5f868b06-7531-448c-a275-a2dfa100f840"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Har som registreringsenhet BEDR",             /*"BEDR"*/  Code = "hovedenhet",                            Description = "Har som registreringsenhet", Urn = "urn:altinn:external-role:ccr:hovedenhet", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("a53c833b-6dc1-4ceb-b56c-00d333c211c0"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Har som registreringsenhet AAFY",             /*"AAFY"*/  Code = "ikke-naeringsdrivende-hovedenhet",      Description = "Har som registreringsenhet", Urn = "urn:altinn:external-role:ccr:ikke-naeringsdrivende-hovedenhet", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("f7c13f9b-8246-4a16-8b93-33e945b8cf5b"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Prokura i fellesskap",                        /*"POFE"*/  Code = "prokurist-fellesskap",                  Description = "Prokura i fellesskap", Urn = "urn:altinn:external-role:ccr:prokurist-fellesskap", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("e39b6f89-6e42-4ca4-8e21-913a632e9c95"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Prokura hver for seg",                        /*"POHV"*/  Code = "prokurist-hver-for-seg",                Description = "Prokura hver for seg", Urn = "urn:altinn:external-role:ccr:prokurist-hver-for-seg", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("6aa99128-c901-4ab4-86cd-b5d92aeb0b80"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Prokura",                                     /*"PROK"*/  Code = "prokurist",                             Description = "Prokura", Urn = "urn:altinn:external-role:ccr:prokurist", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("2c812df3-cbb8-46cf-9071-f5fbb6c28ad2"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Er revisoradresse for",                       /*"READ"*/  Code = "revisoradressat",                       Description = "Er revisoradresse for", Urn = "urn:altinn:external-role:ccr:revisoradressat", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("94df9e5c-7d52-43a2-91af-a50cf81fca2d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Sameiere",                                    /*"SAM"*/   Code = "sameier",                               Description = "Ekstern rolle", Urn = "urn:altinn:external-role:ccr:sameier", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("917dcbb9-8cb9-4d2d-984c-8f877b510747"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Signatur i fellesskap",                       /*"SIFE"*/  Code = "signerer-fellesskap",                   Description = "Signatur i fellesskap", Urn = "urn:altinn:external-role:ccr:signerer-fellesskap", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("a6a94254-7459-4096-b889-411793febbee"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Signatur hver for seg",                       /*"SIHV"*/  Code = "signerer-hver-for-seg",                 Description = "Signatur hver for seg", Urn = "urn:altinn:external-role:ccr:signerer-hver-for-seg", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("0fc0fc0b-d3e1-4360-982e-b1d0a798f374"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson i kommune",                     /*"KOMK"*/  Code = "kontaktperson-kommune",                 Description = "Ekstern rolle", Urn = "urn:altinn:external-role:ccr:kontaktperson-kommune", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("7f6c14f6-7809-4867-83ab-30c426b53d57"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson i Ad",                          /*"KEMN"*/  Code = "kontaktperson-ados",                    Description = "enhet - offentlig sektor", Urn = "urn:altinn:external-role:ccr:kontaktperson-ados", IsKeyRole = false, IsAssignable = false }, // Missing translation
            
            new Role() { Id = Guid.Parse("E9E25AEC-66AB-4C02-8737-21B79A5D9EB5"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Leder i partiets utovende organ",             /*"HLED"*/  Code = "parti-organ-leder",                     Description = "Leder i partiets utovende organ", Urn = "urn:altinn:external-role:ccr:parti-organ-leder", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("0BE0982C-6650-49F2-9A1E-364AD879472C"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Elektronisk signeringsrett",                  /*"ESGR"*/  Code = "elektronisk-signeringsrettig",          Description = "Elektronisk signeringsrett", Urn = "urn:altinn:external-role:ccr:elektronisk-signeringsrettig", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("EE453078-9A2A-4997-969E-40F6663379AB"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Tildeler av elektronisk signeringsrett",      /*"ETDL"*/  Code = "elektronisk-signeringsrett-tildeler",   Description = "Tildeler av elektronisk signeringsrett", Urn = "urn:altinn:external-role:ccr:elektronisk-signeringsrett-tildeler", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("156AE2E3-D9E8-4DAA-BB3C-5859A31BE8C9"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i foretaksgruppe med",                 /*"FGRP"*/  Code = "foretaksgruppe-med",                    Description = "Inngår i foretaksgruppe med", Urn = "urn:altinn:external-role:ccr:foretaksgruppe-med", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("A14D5CDD-A8C9-4E7B-AC90-5A008C0C6129"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Har som datter i konsern",                    /*"KDAT"*/  Code = "konsern-datter",                        Description = "Har som datter i konsern", Urn = "urn:altinn:external-role:ccr:konsern-datter", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("ACD90AC5-4A9D-4AB1-A5D9-5D33D1684A45"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Har som grunnlag for konsern",                /*"KGRL"*/  Code = "konsern-grunnlag",                      Description = "Har som grunnlag for konsern", Urn = "urn:altinn:external-role:ccr:konsern-grunnlag", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("BFA050A6-25BB-4AF8-8DE3-651D0C6FDDC2"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Har som mor i konsern",                       /*"KMOR"*/  Code = "konsern-mor",                           Description = "Har som mor i konsern", Urn = "urn:altinn:external-role:ccr:konsern-mor", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("E4A1253C-31C0-4E11-85BA-6E2E63627FB5"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Forestår avvikling",                          /*"AVKL"*/  Code = "forestaar-avvikling",                   Description = "Forestår avvikling", Urn = "urn:altinn:external-role:ccr:forestaar-avvikling", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("177B7290-DAEA-4368-9A7A-71DBE1EB3B1B"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i felles- registrering",               /*"FEMV"*/  Code = "felles-registrert-med",                 Description = "Inngår i felles- registrering", Urn = "urn:altinn:external-role:ccr:felles-registrert-med", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("920F602D-B82B-40EE-BFD2-856A1C6A26F2"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Er frivillig registrert utleiebygg for",      /*"UTBG"*/  Code = "utleiebygg",                            Description = "Er frivillig registrert utleiebygg for", Urn = "urn:altinn:external-role:ccr:utleiebygg", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("3A9E145D-3CE6-4DF4-85D4-8901AFFAF347"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Er virksomhet drevet i fellesskap av",        /*"VIFE"*/  Code = "virksomhet-fellesskap-drifter",         Description = "Er virksomhet drevet i fellesskap av", Urn = "urn:altinn:external-role:ccr:virksomhet-fellesskap-drifter", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("92651683-36B2-4604-9CE9-B5B688F68696"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Utfyller MVA-oppgaver",                       /*"MVAU"*/  Code = "mva-utfyller",                          Description = "Utfyller MVA-oppgaver", Urn = "urn:altinn:external-role:ccr:mva-utfyller", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("B5136A2C-F48C-40A7-8276-B74E121AB4EB"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Signerer MVA-oppgaver",                       /*"MVAG"*/  Code = "mva-signerer",                          Description = "Signerer MVA-oppgaver", Urn = "urn:altinn:external-role:ccr:mva-signerer", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("4B3AE668-5CAE-4416-9121-C20E81597B12"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Revisor registrert i revisorregisteret",      /*"SREVA"*/ Code = "kontaktperson-revisor",                 Description = "Rettigheter for revisjonsselskap", Urn = "urn:altinn:external-role:ccr:kontaktperson-revisor", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("CDD312F9-8A6E-4184-9374-D4AE4BAABE3E"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Stifter",                                     /*"STFT"*/  Code = "stifter",                               Description = "Stifter", Urn = "urn:altinn:external-role:ccr:stifter", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("F23B832A-CE0E-42F0-B314-E1B0751506F2"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Varamedlem i partiets utovende organ",        /*"HVAR"*/  Code = "parti-organ-varamedlem",                Description = "Varamedlem i partiets utovende organ", Urn = "urn:altinn:external-role:ccr:parti-organ-varamedlem", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("355BC5D6-C346-4B6B-BDB4-ED2CBDEE8318"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Nestleder i partiets utovende organ",         /*"HNST"*/  Code = "parti-organ-nestleder",                 Description = "Nestleder i partiets utovende organ", Urn = "urn:altinn:external-role:ccr:parti-organ-nestleder", IsKeyRole = false, IsAssignable = false }, // Missing translation
            new Role() { Id = Guid.Parse("4A596F51-199E-4586-8292-F9F84B079769"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Styremedlem i partiets utovende organ",       /*"HMDL"*/  Code = "parti-organ-styremedlem",               Description = "Styremedlem i partiets utovende organ", Urn = "urn:altinn:external-role:ccr:parti-organ-styremedlem", IsKeyRole = false, IsAssignable = false }, // Missing translation

            new Role() { Id = Guid.Parse("c497b499-7e98-423d-9fe7-ad5a6c3b71ad"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Primærnæring og næringsmiddel",                Code = "A0212",               Description = "Denne rollen gir rettighet til tjenester innen import, foredling, produksjon og/eller salg av primærnæringsprodukter og andre næringsmiddel, samt dyrehold, akvakultur, planter og kosmetikk. Ved regelverksendringer eller innføring av nye digitale tjenester", Urn = "urn:altinn:rolecode:A0212", IsKeyRole = false },
            new Role() { Id = Guid.Parse("151955ec-d8aa-4c14-a435-ffa96b26a9fb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Post/arkiv",                                   Code = "A0236",               Description = "Denne rollen gir rettighet til å lese meldinger som blir sendt til brukerens meldingsboks. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0236", IsKeyRole = false },
            new Role() { Id = Guid.Parse("c2884487-a634-4537-95b4-bafb917b62a8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Ansvarlig revisor",                            Code = "A0237",               Description = "Delegerbar revisorrolle med signeringsrettighet.Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0237", IsKeyRole = false },
            new Role() { Id = Guid.Parse("10fcad57-7a91-4e02-a921-63e5751fbc24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisormedarbeider",                           Code = "A0238",               Description = "Denne rollen gir revisor rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0238", IsKeyRole = false },
            new Role() { Id = Guid.Parse("ebed65a5-dd87-4180-b898-e1da249b128d"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Regnskapsfører med signeringsrettighet",       Code = "A0239",               Description = "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester, samt signeringsrettighet for tjenestene. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0239", IsKeyRole = false },
            new Role() { Id = Guid.Parse("9407620b-21b6-4538-b4d8-2b4eb339c373"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Regnskapsfører uten signeringsrettighet",      Code = "A0240",               Description = "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0240", IsKeyRole = false },
            new Role() { Id = Guid.Parse("723a43ab-13d8-4585-81e2-e4c734b2d4fc"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Regnskapsfører lønn",                          Code = "A0241",               Description = "Denne rollen gir regnskapsfører rettighet til lønnsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.  ", Urn = "urn:altinn:rolecode:A0241", IsKeyRole = false },
            new Role() { Id = Guid.Parse("6828080b-e846-4c51-b670-201af4917562"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Plan- og byggesak",                            Code = "A0278",               Description = "Rollen er forbeholdt skjemaer og tjenester som er godkjent av Direktoratet for byggkvalitet (DiBK). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0278", IsKeyRole = false },
            new Role() { Id = Guid.Parse("f4df0522-3034-405b-a9e5-83f971737033"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Skatteforhold for privatpersoner",             Code = "A0282",               Description = "Tillatelsen gjelder alle opplysninger vedrørende dine eller ditt enkeltpersonsforetaks skatteforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan Skatteetaten endre i tillatelsen.", Urn = "urn:altinn:rolecode:A0282", IsKeyRole = false },
            new Role() { Id = Guid.Parse("92ea5544-ca64-4e03-9532-646b9f86ff65"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetsbelagt post ",                         Code = "A0286",               Description = "Denne rollen gir tilgang til taushetsbelagt post fra stat og kommune. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0286", IsKeyRole = false },
            new Role() { Id = Guid.Parse("df34b69a-e0aa-4245-a840-3a850769b2bd"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetsbelagt post - oppvekst og utdanning",  Code = "A0287",               Description = "Gir tilgang til taushetsbelagt post fra det offentlige innen oppvekst og utdanning", Urn = "urn:altinn:rolecode:A0287", IsKeyRole = false },
            new Role() { Id = Guid.Parse("5fda4732-dd10-416d-b876-9e1715bbf21c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetsbelagt post - administrasjon",         Code = "A0288",               Description = "Gir tilgang til taushetsbelagt post fra det offentlige innen administrasjon", Urn = "urn:altinn:rolecode:A0288", IsKeyRole = false },
            new Role() { Id = Guid.Parse("4652e98f-7a6b-4dc2-b061-fc8d6840e456"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Algetestdata",                                 Code = "A0293",               Description = "Havforskningsinstituttet - registrering av algetestdata", Urn = "urn:altinn:rolecode:A0293", IsKeyRole = false },
            new Role() { Id = Guid.Parse("c22c6add-dd5d-4735-87de-b75491018e50"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Transportløyvegaranti",                        Code = "A0294",               Description = "Statens vegvesen - rolle som gir tilgang til app for transportløyvegarantister", Urn = "urn:altinn:rolecode:A0294", IsKeyRole = false },
            new Role() { Id = Guid.Parse("d8b9c47b-e5a7-4912-8aa8-1d2bab75e41c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorattesterer",                            Code = "A0298",               Description = "Rollen gir bruker tilgang til å attestere tjenester for avgiver som revisor.  Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:A0298", IsKeyRole = false },
            new Role() { Id = Guid.Parse("48f9e5ec-efd5-4863-baba-9697b8971666"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Tilgangsstyring",                              Code = "ADMAI",               Description = "Denne rollen gir administratortilgang til å gi videre rettigheter til andre.  ", Urn = "urn:altinn:rolecode:ADMAI", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e078bb18-f55a-4a2d-8964-c599f41b29b5"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Programmeringsgrensesnitt (API)",              Code = "APIADM",              Description = "Delegerbar rolle som gir  tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.", Urn = "urn:altinn:rolecode:APIADM", IsKeyRole = false },
            new Role() { Id = Guid.Parse("0ea4e5de-3fb4-499e-b013-1e1b4459af24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Programmeringsgrensesnitt for NUF (API)",      Code = "APIADMNUF",           Description = "Delegerbar rolle som gir kontaktperson for norskregistrert utenlandsk foretak (NUF) tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.", Urn = "urn:altinn:rolecode:APIADMNUF", IsKeyRole = false },
            new Role() { Id = Guid.Parse("60abf944-cf8c-4845-b310-83bcb6c77198"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorattesterer - MVA kompensasjon",         Code = "ATTST",               Description = "Denne rollen gir revisor rettighet til å attestere tjenesten Merverdiavgift - søknad om kompensasjon (RF-0009).", Urn = "urn:altinn:rolecode:ATTST", IsKeyRole = false },
            new Role() { Id = Guid.Parse("0a76304e-345b-4f22-bb31-4837a630eb7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbo tilgangsstyring",                    Code = "BOADM",               Description = "Denne rollen gir advokater mulighet til å styre hvem som har rettigheter til konkursbo.  ", Urn = "urn:altinn:rolecode:BOADM", IsKeyRole = false },
            new Role() { Id = Guid.Parse("7246639c-137b-4981-b172-6134c9fc1a7f"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbo lesetilgang",                        Code = "BOBEL",               Description = "Tilgang til å lese informasjon i tjenesten Konkursbehandling", Urn = "urn:altinn:rolecode:BOBEL", IsKeyRole = false },
            new Role() { Id = Guid.Parse("5f73b031-8b5b-45d8-a682-e9a7e75a7691"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbo skrivetilgang",                      Code = "BOBES",               Description = "Utvidet lesetilgang og innsendingsrett for tjenesten Konkursbehandling", Urn = "urn:altinn:rolecode:BOBES", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e0684f66-a46e-4706-a754-8889b532509c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "ECKEYROLE",                                    Code = "ECKEYROLE",           Description = "Nøkkelrolle for virksomhetsertifikatbrukere", Urn = "urn:altinn:rolecode:ECKEYROLE", IsKeyRole = true },
            new Role() { Id = Guid.Parse("1225bc46-4b03-4b63-b6e8-58926b29a97b"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Eksplisitt tjenestedelegering",                Code = "EKTJ",                Description = "Ikke-delegerbar roller for tjenester som kun skal delegeres enkeltvis", Urn = "urn:altinn:rolecode:EKTJ", IsKeyRole = false },
            new Role() { Id = Guid.Parse("cde501eb-0d23-410b-b728-00ab9d68fb2e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Godkjenning av bedriftshelsetjeneste",         Code = "GKBHT",               Description = "Godkjenning av bedriftshelsetjeneste", Urn = "urn:altinn:rolecode:GKBHT", IsKeyRole = false },
            new Role() { Id = Guid.Parse("d9e05d40-9849-4982-bf04-aa03b19e4a66"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Hovedadministrator",                           Code = "HADM",                Description = "Denne rollen gir mulighet for å delegere alle roller og rettigheter for en aktør, også de man ikke har selv. Hovedadministrator-rollen kan bare delegeres av daglig leder, styrets leder, innehaver og bestyrende reder.", Urn = "urn:altinn:rolecode:HADM", IsKeyRole = false },
            new Role() { Id = Guid.Parse("98bebcac-d6bb-4343-97b8-0fe8bc744d7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Økokrim rapportering",                         Code = "HVASK",               Description = "Tilgang til tjenester fra Økokrim. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:HVASK", IsKeyRole = false },
            new Role() { Id = Guid.Parse("27e1ef41-df4d-439e-b948-df136c139e81"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Klientadministrator",                          Code = "KLADM",               Description = "Tilgang til å administrere klientroller for regnskapsførere og revisorer", Urn = "urn:altinn:rolecode:KLADM", IsKeyRole = false },
            new Role() { Id = Guid.Parse("b8e6dd1c-ca10-4ce6-9c27-53cdb3c275b3"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Kommunale tjenester",                          Code = "KOMAB",               Description = "Rollen gir tilgang til kommunale tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:KOMAB", IsKeyRole = false },
            new Role() { Id = Guid.Parse("010b4c49-bf56-44e3-b73b-84be7b2a5eb6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Lønn og personalmedarbeider",                  Code = "LOPER",               Description = "Denne rollen gir rettighet til lønns- og personalrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.  ", Urn = "urn:altinn:rolecode:LOPER", IsKeyRole = false },
            new Role() { Id = Guid.Parse("0f276fc4-c201-4ff7-8e8a-caa3efe9c02a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Parallell signering",                          Code = "PASIG",               Description = "Denne rollen gir rettighet til å signere elementer fra andre avgivere.  ", Urn = "urn:altinn:rolecode:PASIG", IsKeyRole = false },
            new Role() { Id = Guid.Parse("23cade0a-287a-49e0-8957-22d5a14cb100"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Patent, varemerke og design",                  Code = "PAVAD",               Description = "Denne rollen gir rettighet til tjenester relatert til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:PAVAD", IsKeyRole = false },
            new Role() { Id = Guid.Parse("696478f4-c85b-4bda-ace0-caa058fe5def"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Privatperson begrensede rettigheter",          Code = "PRIUT",               Description = "Denne rollen gir mulighet til å benytte tjenester på vegne av en annen privatperson. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.  ", Urn = "urn:altinn:rolecode:PRIUT", IsKeyRole = false },
            new Role() { Id = Guid.Parse("633cde7d-3604-45b2-ba8c-e16161cf2cf8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Regnskapsmedarbeider",                         Code = "REGNA",               Description = "Denne rollen gir rettighet til regnskapsrelaterte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:REGNA", IsKeyRole = false },
            new Role() { Id = Guid.Parse("1d71e23d-91b6-44ca-b171-c179028e7cdf"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorrettighet",                             Code = "REVAI",               Description = "Denne rollen gir revisor rettighet til aktuelle skjema og tjenester", Urn = "urn:altinn:rolecode:REVAI", IsKeyRole = false },
            new Role() { Id = Guid.Parse("1a15b75c-2387-4278-ba3a-7eb1cffe1653"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetsbelagt post fra kommunen",             Code = "SENS01",              Description = "Rollen gir tilgang til tjenester med taushetsbelagt informasjon fra kommunen, og bør ikke delegeres i stort omfang", Urn = "urn:altinn:rolecode:SENS01", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e427a9fb-4b6b-44b3-b873-689d174283b8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Signerer av Samordnet registermelding",        Code = "SIGNE",               Description = "Denne rollen gir rettighet til tjenester på vegne av enheter/foretak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:SIGNE", IsKeyRole = false },
            new Role() { Id = Guid.Parse("16857e39-441f-4dd4-8592-aed94e816c04"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Begrenset signeringsrettighet",                Code = "SISKD",               Description = "Tilgang til å signere utvalgte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:SISKD", IsKeyRole = false },
            new Role() { Id = Guid.Parse("b1213d79-03fa-4837-9193-e4b9fe24eccb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Helse-, sosial- og velferdstjenester",         Code = "UIHTL",               Description = "Tilgang til helse-, sosial- og velferdsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:UIHTL", IsKeyRole = false },
            new Role() { Id = Guid.Parse("3c99647d-10b5-447e-9f0b-7bef1c7880f7"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Samferdsel",                                   Code = "UILUF",               Description = "Rollen gir rettighet til tjenester relatert til samferdsel. For eksempel tjenester fra Statens Vegvesen, Sjøfartsdirektoratet og Luftfartstilsynet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rolen gir.", Urn = "urn:altinn:rolecode:UILUF", IsKeyRole = false },
            new Role() { Id = Guid.Parse("dbaae9f8-107a-4222-9afd-d9f95cd5319c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Utfyller/Innsender",                           Code = "UTINN",               Description = "Denne rollen gir rettighet til et bredt utvalg skjema og tjenester som ikke har så strenge krav til autorisasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:UTINN", IsKeyRole = false },
            new Role() { Id = Guid.Parse("af338fd5-3f1d-4ab5-8326-9dfecad26f71"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Energi, miljø og klima",                       Code = "UTOMR",               Description = "Tilgang til tjenester relatert til energi, miljø og klima. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.", Urn = "urn:altinn:rolecode:UTOMR", IsKeyRole = false },
            new Role() { Id = Guid.Parse("478f710a-4af1-412d-9c67-de976fd0b229"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Hovedrolle for sensitive tjeneste",            Code = "SENS",                Description = "Hovedrolle for sensitive tjeneste", Urn = "urn:altinn:rolecode:SENS", IsKeyRole = false },

            new Role() { Id = Guid.Parse("1c6eeec1-fe70-4fc5-8b45-df4a2255dea6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Privatperson",                                 Code = "privatperson",        Description = "Denne rollen er hentet fra Folkeregisteret og gir rettighet til flere tjenester.", Urn = "urn:altinn:role:privatperson", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e16ab886-1e1e-4f45-8f79-46f06f720f3e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Selvregistrert bruker",                        Code = "selvregistrert",      Description = "Selvregistrert bruker", Urn = "urn:altinn:role:selvregistrert", IsKeyRole = false }
        };

        var rolesEng = new List<Role>()
        {
            new Role() { Id = Guid.Parse("42CAE370-2DC1-4FDC-9C67-C2F4B0F0F829"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,    Name = "Rightholder",                  Code = "rettighetshaver",               Description = "Allows receiving delegated authorizations for the business", Urn = "urn:altinn:role:rettighetshaver", IsKeyRole = false },
            new Role() { Id = Guid.Parse("FF4C33F5-03F7-4445-85ED-1E60B8AAFB30"), EntityTypeId = persEntityTypeId, ProviderId = a3ProviderId,   Name = "Agent",                        Code = "agent",                         Description = "Allows receiving delegated authorizations for the business", Urn = "urn:altinn:role:agent" },
            new Role() { Id = Guid.Parse("6795081e-e69c-4efd-8d42-2bfccd346777"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,    Name = "Client Administrator",         Code = "klientadministrator",           Description = "Allows managing access to services for employees on behalf of their clients", Urn = "urn:altinn:role:klientadministrator" },
            new Role() { Id = Guid.Parse("6c1fbcb9-609c-4ab8-a048-3be8d7da5a82"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,    Name = "Access Manager",               Code = "tilgangsstyrer",                Description = "Allows granting further accesses for the business that have been received", Urn = "urn:altinn:role:tilgangsstyrer" },
            new Role() { Id = Guid.Parse("ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,    Name = "Main Administrator",           Code = "hovedadministrator",            Description = "Allows managing all accesses for the business", Urn = "urn:altinn:role:hovedadministrator" },
            new Role() { Id = Guid.Parse("b3f5c1e8-4e3b-4d2a-8c3e-1f2b3d4e5f6a"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,    Name = "Maskinporten Administrator",   Code = "maskinporten-administrator",    Description = "Allows the user to manage access to Maskinporten scopes", Urn = "urn:altinn:role:maskinporten-administrator" },

            new Role() { Id = Guid.Parse("66ad5542-4f4a-4606-996f-18690129ce00"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Administrative Unit - Public Sector",                 Code = "administrativ-enhet-offentlig-sektor",  Description = "Administrative Unit - Public Sector", Urn = "urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor" },
            new Role() { Id = Guid.Parse("29a24eab-a25f-445d-b56d-e3b914844853"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Deputy Leader",                                       Code = "nestleder",                             Description = "Board member who acts as chair in the absence of the leader", Urn = "urn:altinn:external-role:ccr:nestleder" },
            new Role() { Id = Guid.Parse("8c1e91c2-a71c-4abf-a74e-a600a98be976"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Part of Office Community",                            Code = "kontorfelleskapmedlem",                 Description = "Participates in office community", Urn = "urn:altinn:external-role:ccr:kontorfelleskapmedlem" },
            new Role() { Id = Guid.Parse("cfc41a92-2061-4ff4-97dc-658ffba2c00e"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Organizational Unit in the Public Sector",            Code = "organisasjonsledd-offentlig-sektor",    Description = "Organizational Unit in the Public Sector", Urn = "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor" },
            new Role() { Id = Guid.Parse("2fec6d4b-cead-419a-adf3-1bf482a3c9dc"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Distinct Subunit",                                    Code = "saerskilt-oppdelt-enhet",               Description = "Distinct Subunit", Urn = "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet" },
            new Role() { Id = Guid.Parse("55bd7d4d-08dd-46ee-ac8e-3a44d800d752"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Managing Director",                                   Code = "daglig-leder",                          Description = "An individual or legal entity responsible for the daily operations of a business", Urn = "urn:altinn:external-role:ccr:daglig-leder" },
            new Role() { Id = Guid.Parse("18baa914-ac43-4663-9fa4-6f5760dc68eb"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Participant with Shared Responsibility",              Code = "deltaker-delt-ansvar",                  Description = "An individual or legal entity who has personal responsibility for parts of the company's obligations", Urn = "urn:altinn:external-role:ccr:deltaker-delt-ansvar" },
            new Role() { Id = Guid.Parse("2651ed07-f31b-4bc1-87bd-4d270742a19d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Owner",                                               Code = "innehaver",                             Description = "An individual who is the owner of a sole proprietorship", Urn = "urn:altinn:external-role:ccr:innehaver" },
            new Role() { Id = Guid.Parse("f1021b8c-9fbc-4296-bd17-a05d713037ef"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Participant with Full Responsibility",                Code = "deltaker-fullt-ansvar",                 Description = "An individual or legal entity who has unlimited personal responsibility for the company's obligations", Urn = "urn:altinn:external-role:ccr:deltaker-fullt-ansvar" },
            new Role() { Id = Guid.Parse("d41d67f2-15b0-4c82-95db-b8d5baaa14a4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Alternate Member",                                    Code = "varamedlem",                            Description = "An individual or legal entity who acts as a substitute for a board member", Urn = "urn:altinn:external-role:ccr:varamedlem" },
            new Role() { Id = Guid.Parse("1f8a2518-9494-468a-80a0-7405f0daf9e9"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Observer",                                            Code = "observator",                            Description = "An individual who participates in board meetings of a business, but without voting rights", Urn = "urn:altinn:external-role:ccr:observator" },
            new Role() { Id = Guid.Parse("f045ffda-dbdc-41da-b674-b9b276ad5b01"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Board Member",                                        Code = "styremedlem",                           Description = "An individual or legal entity who is a member of a board", Urn = "urn:altinn:external-role:ccr:styremedlem" },
            new Role() { Id = Guid.Parse("9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Chair of the Board",                                  Code = "styreleder",                            Description = "An individual or legal entity who is a board member and chairs the board", Urn = "urn:altinn:external-role:ccr:styreleder" },
            new Role() { Id = Guid.Parse("2e2fc06e-d9b7-4cd9-91bc-d5de766d20de"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Personal Bankruptcy",                                 Code = "personlige-konkurs",                    Description = "Personal Bankruptcy", Urn = "urn:altinn:external-role:ccr:personlige-konkurs" },
            new Role() { Id = Guid.Parse("e852d758-e8dd-41ec-a1e2-4632deb6857d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Norwegian Representative for a Foreign Entity",       Code = "norsk-representant",                    Description = "An individual or legal entity responsible for the daily operations in Norway", Urn = "urn:altinn:external-role:ccr:norsk-representant" },
            new Role() { Id = Guid.Parse("db013059-4a8a-442d-bf90-b03539fe5dda"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Contact Person",                                      Code = "kontaktperson",                         Description = "An individual who represents a business", Urn = "urn:altinn:external-role:ccr:kontaktperson" },
            new Role() { Id = Guid.Parse("69c4397a-9e34-4e73-9f69-534bc1bb74c8"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Contact Person NUF",                                  Code = "kontaktperson-nuf",                     Description = "An individual who represents a business - NUF", Urn = "urn:altinn:external-role:ccr:kontaktperson-nuf" },
            new Role() { Id = Guid.Parse("8f0cf433-954e-4680-a25d-a3cf9ffdf149"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Managing Shipowner",                                  Code = "bestyrende-reder",                      Description = "Managing Shipowner", Urn = "urn:altinn:external-role:ccr:bestyrende-reder" },
            new Role() { Id = Guid.Parse("9ce84a4d-4970-4ef2-8208-b8b8f4d45556"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Owning Municipality",                                 Code = "eierkommune",                           Description = "Owning Municipality", Urn = "urn:altinn:external-role:ccr:eierkommune" },
            new Role() { Id = Guid.Parse("2cacfb35-2346-4a8d-95f6-b6fa4206881c"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Estate Administrator",                                Code = "bostyrer",                              Description = "Administrator of a bankruptcy or probate estate under public administration", Urn = "urn:altinn:external-role:ccr:bostyrer" },
            new Role() { Id = Guid.Parse("e4674211-034a-45f3-99ac-b2356984968a"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Healthcare Institution",                              Code = "helseforetak",                          Description = "Healthcare Institution", Urn = "urn:altinn:external-role:ccr:helseforetak" },
            new Role() { Id = Guid.Parse("f76b997a-9bd8-4f7b-899f-fcd85d35669f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Auditor",                                             Code = "revisor",                               Description = "Auditor", Urn = "urn:altinn:external-role:ccr:revisor" },
            new Role() { Id = Guid.Parse("348b2f47-47ee-4084-abf8-68aa54c2b27f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Business Manager",                                    Code = "forretningsforer",                      Description = "Business Manager", Urn = "urn:altinn:external-role:ccr:forretningsforer" },
            new Role() { Id = Guid.Parse("cfcf75af-9902-41f7-ab47-b77ba60bcae5"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "General Partner",                                     Code = "komplementar",                          Description = "General Partner", Urn = "urn:altinn:external-role:ccr:komplementar" },
            new Role() { Id = Guid.Parse("50cc3f41-4dde-4417-8c04-eea428f169dd"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Bankrupt Debtor",                                     Code = "konkursdebitor",                        Description = "Bankrupt Debtor", Urn = "urn:altinn:external-role:ccr:konkursdebitor" },
            new Role() { Id = Guid.Parse("d78dd1d8-a3f3-4ae6-807e-ea5149f47035"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Part of a Church Council",                            Code = "kirkelig-fellesraad",                   Description = "Part of a Church Council", Urn = "urn:altinn:external-role:ccr:kirkelig-fellesraad" },
            new Role() { Id = Guid.Parse("185f623b-f614-4a83-839c-1788764bd253"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Information about the Company in the Home Country",   Code = "hovedforetak",                          Description = "Information about the company in the home country", Urn = "urn:altinn:external-role:ccr:hovedforetak" },
            new Role() { Id = Guid.Parse("46e27685-b3ba-423e-8b42-faab54de5817"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Accountant",                                          Code = "regnskapsforer",                        Description = "Accountant", Urn = "urn:altinn:external-role:ccr:regnskapsforer" },
            
            new Role() { Id = Guid.Parse("c497b499-7e98-423d-9fe7-ad5a6c3b71ad"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Primary industry and foodstuff",                    Code = "A0212",               Description = "Import, processing, production and/or sales of primary products and other foodstuff. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0212" },
            new Role() { Id = Guid.Parse("151955ec-d8aa-4c14-a435-ffa96b26a9fb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Mail/archive",                                      Code = "A0236",               Description = "Access to read correpondences. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0236" },
            new Role() { Id = Guid.Parse("c2884487-a634-4537-95b4-bafb917b62a8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Auditor in charge",                                 Code = "A0237",               Description = "Delegateble auditor role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0237" },
            new Role() { Id = Guid.Parse("10fcad57-7a91-4e02-a921-63e5751fbc24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Assistant auditor",                                 Code = "A0238",               Description = "Delegateble auditor role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0238" },
            new Role() { Id = Guid.Parse("ebed65a5-dd87-4180-b898-e1da249b128d"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Accountant with signing rights",                    Code = "A0239",               Description = "Delegateble accountant role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0239" },
            new Role() { Id = Guid.Parse("9407620b-21b6-4538-b4d8-2b4eb339c373"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Accountant without signing rights",                 Code = "A0240",               Description = "Delegateble accountant role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0240" },
            new Role() { Id = Guid.Parse("723a43ab-13d8-4585-81e2-e4c734b2d4fc"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Accountant salary",                                 Code = "A0241",               Description = "Delegateble accountant role with signing right to services related to salary. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0241" },
            new Role() { Id = Guid.Parse("6828080b-e846-4c51-b670-201af4917562"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Planning and construction",                         Code = "A0278",               Description = "The role is reserved for forms and services approved by Norwegian Building Authority (DiBK). In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0278" },
            new Role() { Id = Guid.Parse("f4df0522-3034-405b-a9e5-83f971737033"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Private tax affairs",                               Code = "A0282",               Description = "The permission applies to all information about your own or your sole proprietorship’s tax affairs. In case of changes to regulations or implementation of new digital services, the Tax Administration may change the permission.", Urn = "urn:altinn:rolecode:A0282" },
            new Role() { Id = Guid.Parse("92ea5544-ca64-4e03-9532-646b9f86ff65"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Confidential information",                          Code = "A0286",               Description = "This role provides access to confidential information from public agencies. In the event of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:A0286" },
            new Role() { Id = Guid.Parse("df34b69a-e0aa-4245-a840-3a850769b2bd"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Confidential - education",                          Code = "A0287",               Description = "This role provides access to confidential information from public agencies", Urn = "urn:altinn:rolecode:A0287" },
            new Role() { Id = Guid.Parse("5fda4732-dd10-416d-b876-9e1715bbf21c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Conficential - administration",                     Code = "A0288",               Description = "This role provides access to confidential information from public agencies", Urn = "urn:altinn:rolecode:A0288" },
            new Role() { Id = Guid.Parse("4652e98f-7a6b-4dc2-b061-fc8d6840e456"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Algea test data",                                   Code = "A0293",               Description = "Havforskningsinstituttet - registration of algea test data", Urn = "urn:altinn:rolecode:A0293" },
            new Role() { Id = Guid.Parse("c22c6add-dd5d-4735-87de-b75491018e50"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Transport permit guarantee",                        Code = "A0294",               Description = "The Norwegian Public Roads Administration - role that provides access to the app for transport permi", Urn = "urn:altinn:rolecode:A0294" },
            new Role() { Id = Guid.Parse("d8b9c47b-e5a7-4912-8aa8-1d2bab75e41c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Auditor certifier",                                 Code = "A0298",               Description = "The role gives the user access to certify services for the reportee as an auditor. In the event of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides..", Urn = "urn:altinn:rolecode:A0298" },
            new Role() { Id = Guid.Parse("48f9e5ec-efd5-4863-baba-9697b8971666"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Access manager",                                    Code = "ADMAI",               Description = "Administration of access", Urn = "urn:altinn:rolecode:ADMAI" },
            new Role() { Id = Guid.Parse("e078bb18-f55a-4a2d-8964-c599f41b29b5"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Application Programming Interface (API)",           Code = "APIADM",              Description = "Delegable role that provides access to manage access to APIs on behalf of the business.", Urn = "urn:altinn:rolecode:APIADM" },
            new Role() { Id = Guid.Parse("0ea4e5de-3fb4-499e-b013-1e1b4459af24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Application Programming Interface for NUF (API)",   Code = "APIADMNUF",           Description = "Delegable role that provides the representative for a Norwegian-registered foreign enterprise (NUF) access to manage access to the programming interface - API, on behalf of the business.", Urn = "urn:altinn:rolecode:APIADMNUF" },
            new Role() { Id = Guid.Parse("60abf944-cf8c-4845-b310-83bcb6c77198"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Auditor certifies validity of VAT compensation",    Code = "ATTST",               Description = "Certification by auditor of RF-0009", Urn = "urn:altinn:rolecode:ATTST" },
            new Role() { Id = Guid.Parse("0a76304e-345b-4f22-bb31-4837a630eb7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Bankruptcy administrator",                          Code = "BOADM",               Description = "Applies to lawyers and gives opportunity to manage access to bankruptcies", Urn = "urn:altinn:rolecode:BOADM" },
            new Role() { Id = Guid.Parse("7246639c-137b-4981-b172-6134c9fc1a7f"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Bankruptcy read",                                   Code = "BOBEL",               Description = "Reading rights for information in the service Konkursbehandling (bankruptcy proceedings)", Urn = "urn:altinn:rolecode:BOBEL" },
            new Role() { Id = Guid.Parse("5f73b031-8b5b-45d8-a682-e9a7e75a7691"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Bankruptcy write",                                  Code = "BOBES",               Description = "Writing rights for information in the service Konkursbehandling (bankruptcy proceedings)", Urn = "urn:altinn:rolecode:BOBES" },
            new Role() { Id = Guid.Parse("e0684f66-a46e-4706-a754-8889b532509c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "ECKEYROLE",                                         Code = "ECKEYROLE",           Description = "Key role for enterprise users", Urn = "urn:altinn:rolecode:ECKEYROLE" },
            new Role() { Id = Guid.Parse("1225bc46-4b03-4b63-b6e8-58926b29a97b"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Explicit service delegation",                       Code = "EKTJ",                Description = "Non-delegable role for services to be delegated as single rights", Urn = "urn:altinn:rolecode:EKTJ" },
            new Role() { Id = Guid.Parse("cde501eb-0d23-410b-b728-00ab9d68fb2e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Godkjenning av bedriftshelsetjeneste",              Code = "GKBHT",               Description = "Godkjenning av bedriftshelsetjeneste", Urn = "urn:altinn:rolecode:GKBHT" },
            new Role() { Id = Guid.Parse("d9e05d40-9849-4982-bf04-aa03b19e4a66"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Main Administrator",                                Code = "HADM",                Description = "This role allows you to delegate all roles and rights for an actor, including those you do not have yourself. The Main administrator role can only be delegated by General manager, Chairman of the board, Soul proprietor and Managing shipowner.", Urn = "urn:altinn:rolecode:HADM" },
            new Role() { Id = Guid.Parse("98bebcac-d6bb-4343-97b8-0fe8bc744d7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Økokrim reporting",                                 Code = "HVASK",               Description = "Access to services from The Norwegian National Authority for Investigation and Prosecution of Economic and Environmental Crime. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provide", Urn = "urn:altinn:rolecode:HVASK" },
            new Role() { Id = Guid.Parse("27e1ef41-df4d-439e-b948-df136c139e81"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Client administrator",                              Code = "KLADM",               Description = "Administration of access to client roles for accountants and auditors", Urn = "urn:altinn:rolecode:KLADM" },
            new Role() { Id = Guid.Parse("b8e6dd1c-ca10-4ce6-9c27-53cdb3c275b3"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Municipal services",                                Code = "KOMAB",               Description = "Role for municipal services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:KOMAB" },
            new Role() { Id = Guid.Parse("010b4c49-bf56-44e3-b73b-84be7b2a5eb6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Salaries and personnel employee",                   Code = "LOPER",               Description = "Access to services related to salaries and personnel", Urn = "urn:altinn:rolecode:LOPER" },
            new Role() { Id = Guid.Parse("0f276fc4-c201-4ff7-8e8a-caa3efe9c02a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Parallel signing",                                  Code = "PASIG",               Description = "Right to sign elements from other reportees", Urn = "urn:altinn:rolecode:PASIG" },
            new Role() { Id = Guid.Parse("23cade0a-287a-49e0-8957-22d5a14cb100"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Patents, trademarks and design",                    Code = "PAVAD",               Description = "Access to services related to patents, trademarks and design. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:PAVAD" },
            new Role() { Id = Guid.Parse("696478f4-c85b-4bda-ace0-caa058fe5def"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Limited rights for an individual",                  Code = "PRIUT",               Description = "Delegable rights to services for individuals. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:PRIUT" },
            new Role() { Id = Guid.Parse("633cde7d-3604-45b2-ba8c-e16161cf2cf8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Accounting employee",                               Code = "REGNA",               Description = "Access to accounting related forms and services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:REGNA" },
            new Role() { Id = Guid.Parse("1d71e23d-91b6-44ca-b171-c179028e7cdf"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Auditor's rights",                                  Code = "REVAI",               Description = "Delegable auditor's rights", Urn = "urn:altinn:rolecode:REVAI" },
            new Role() { Id = Guid.Parse("1a15b75c-2387-4278-ba3a-7eb1cffe1653"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Confidential correspondence from the municipality", Code = "SENS01",              Description = "This role provides access to services with confidential information from the municipality", Urn = "urn:altinn:rolecode:SENS01" },
            new Role() { Id = Guid.Parse("e427a9fb-4b6b-44b3-b873-689d174283b8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Signer of Coordinated register notification",       Code = "SIGNE",               Description = "Applies to singing on behalf of entities/businesses. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:SIGNE" },
            new Role() { Id = Guid.Parse("16857e39-441f-4dd4-8592-aed94e816c04"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Limited signing rights",                            Code = "SISKD",               Description = "Signing access for selected forms and services.In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:SISKD" },
            new Role() { Id = Guid.Parse("b1213d79-03fa-4837-9193-e4b9fe24eccb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Health-, social- and welfare services",             Code = "UIHTL",               Description = "Access to health-, social- and welfare related services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:UIHTL" },
            new Role() { Id = Guid.Parse("3c99647d-10b5-447e-9f0b-7bef1c7880f7"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Transport",                                         Code = "UILUF",               Description = "Access to services related to transport. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:UILUF" },
            new Role() { Id = Guid.Parse("dbaae9f8-107a-4222-9afd-d9f95cd5319c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Reporter/sender",                                   Code = "UTINN",               Description = "This role provides right to a wide selection of forms and services that do not have very strict requirements for authorization. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provide", Urn = "urn:altinn:rolecode:UTINN" },
            new Role() { Id = Guid.Parse("af338fd5-3f1d-4ab5-8326-9dfecad26f71"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Energy, environment and climate",                   Code = "UTOMR",               Description = "Access to services related to energy, environment and climate. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.", Urn = "urn:altinn:rolecode:UTOMR" },
            new Role() { Id = Guid.Parse("478f710a-4af1-412d-9c67-de976fd0b229"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Hovedrolle for sensitive tjeneste",                 Code = "SENS",                Description = "Hovedrolle for sensitive tjeneste", Urn = "urn:altinn:rolecode:SENS", IsKeyRole = false },
            
            new Role() { Id = Guid.Parse("1c6eeec1-fe70-4fc5-8b45-df4a2255dea6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Private person",                                    Code = "privatperson",        Description = "Private person", Urn = "urn:altinn:role:privatperson", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e16ab886-1e1e-4f45-8f79-46f06f720f3e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Self registered user",                              Code = "selvregistrert",      Description = "Self registered user", Urn = "urn:altinn:role:selvregistrert", IsKeyRole = false }

        };

        var rolesNno = new List<Role>()
        {
            new Role() { Id = Guid.Parse("42CAE370-2DC1-4FDC-9C67-C2F4B0F0F829"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId,  Name = "Rettshavar",                      Code = "rettighetshaver",               Description = "Gjev høve til å motta delegerte fullmakter for verksemda", Urn = "urn:altinn:role:rettighetshaver", IsKeyRole = false },
            new Role() { Id = Guid.Parse("FF4C33F5-03F7-4445-85ED-1E60B8AAFB30"), EntityTypeId = persEntityTypeId, ProviderId = a3ProviderId, Name = "Agent",                           Code = "agent",                         Description = "Gjev høve til å motta delegerte fullmakter for verksemda", Urn = "urn:altinn:role:agent" },
            new Role() { Id = Guid.Parse("6795081e-e69c-4efd-8d42-2bfccd346777"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Klientadministrator",              Code = "klientadministrator",           Description = "Gjev høve til å administrere tilgang til tenester vidare til tilsette på vegne av kundane deira", Urn = "urn:altinn:role:klientadministrator" },
            new Role() { Id = Guid.Parse("6c1fbcb9-609c-4ab8-a048-3be8d7da5a82"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Tilgangsstyrer",                   Code = "tilgangsstyrer",                Description = "Gjev høve til å vidareformidle tilgongar for verksemda som ein sjølv har motteke", Urn = "urn:altinn:role:tilgangsstyrer" },
            new Role() { Id = Guid.Parse("ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Hovudadministrator",               Code = "hovedadministrator",            Description = "Gjev høve til å administrere alle tilgongar for verksemda", Urn = "urn:altinn:role:hovedadministrator" },
            new Role() { Id = Guid.Parse("b3f5c1e8-4e3b-4d2a-8c3e-1f2b3d4e5f6a"), EntityTypeId = orgEntityTypeId, ProviderId = a3ProviderId, Name = "Maskinporten administrator",       Code = "maskinporten-administrator",    Description = "Gjev høve til å administrere tilgongar for Maskinporten scopes", Urn = "urn:altinn:role:maskinporten-administrator" },

            new Role() { Id = Guid.Parse("66ad5542-4f4a-4606-996f-18690129ce00"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Administrativ eining - offentleg sektor",             Code = "administrativ-enhet-offentlig-sektor",  Description = "Administrativ eining - offentleg sektor", Urn = "urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor" },
            new Role() { Id = Guid.Parse("29a24eab-a25f-445d-b56d-e3b914844853"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Nestleiar",                                           Code = "nestleder",                             Description = "Styremedlem som fungerer som styreleiar ved leiarens fråvær", Urn = "urn:altinn:external-role:ccr:nestleder" },
            new Role() { Id = Guid.Parse("8c1e91c2-a71c-4abf-a74e-a600a98be976"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i kontorfellesskap",                           Code = "kontorfelleskapmedlem",                 Description = "Inngår i kontorfellesskap", Urn = "urn:altinn:external-role:ccr:kontorfelleskapmedlem" },
            new Role() { Id = Guid.Parse("cfc41a92-2061-4ff4-97dc-658ffba2c00e"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Organisasjonsledd i offentleg sektor",                Code = "organisasjonsledd-offentlig-sektor",    Description = "Organisasjonsledd i offentleg sektor", Urn = "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor" },
            new Role() { Id = Guid.Parse("2fec6d4b-cead-419a-adf3-1bf482a3c9dc"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Særskild oppdelt eining",                             Code = "saerskilt-oppdelt-enhet",               Description = "Særskild oppdelt eining", Urn = "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet" },
            new Role() { Id = Guid.Parse("55bd7d4d-08dd-46ee-ac8e-3a44d800d752"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Dagleg leiar",                                        Code = "daglig-leder",                          Description = "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i ei verksemd", Urn = "urn:altinn:external-role:ccr:daglig-leder" },
            new Role() { Id = Guid.Parse("18baa914-ac43-4663-9fa4-6f5760dc68eb"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Deltakar delt ansvar",                                Code = "deltaker-delt-ansvar",                  Description = "Fysisk- eller juridisk person som har personleg ansvar for delar av selskapet sine forpliktingar", Urn = "urn:altinn:external-role:ccr:deltaker-delt-ansvar" },
            new Role() { Id = Guid.Parse("2651ed07-f31b-4bc1-87bd-4d270742a19d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Innehavar",                                           Code = "innehaver",                             Description = "Fysisk person som er eigar av eit enkeltpersonforetak", Urn = "urn:altinn:external-role:ccr:innehaver" },
            new Role() { Id = Guid.Parse("f1021b8c-9fbc-4296-bd17-a05d713037ef"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Deltakar fullt ansvar",                               Code = "deltaker-fullt-ansvar",                 Description = "Fysisk- eller juridisk person som har ubegrensa, personleg ansvar for selskapet sine forpliktingar", Urn = "urn:altinn:external-role:ccr:deltaker-fullt-ansvar" },
            new Role() { Id = Guid.Parse("d41d67f2-15b0-4c82-95db-b8d5baaa14a4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Varamedlem",                                          Code = "varamedlem",                            Description = "Fysisk- eller juridisk person som er staden for eit styremedlem", Urn = "urn:altinn:external-role:ccr:varamedlem" },
            new Role() { Id = Guid.Parse("1f8a2518-9494-468a-80a0-7405f0daf9e9"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Observatør",                                          Code = "observator",                            Description = "Fysisk person som deltek i styremøter i ei verksemd, men utan stemmerett", Urn = "urn:altinn:external-role:ccr:observator" },
            new Role() { Id = Guid.Parse("f045ffda-dbdc-41da-b674-b9b276ad5b01"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Styremedlem",                                         Code = "styremedlem",                           Description = "Fysisk- eller juridisk person som inngår i eit styre", Urn = "urn:altinn:external-role:ccr:styremedlem" },
            new Role() { Id = Guid.Parse("9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Styrets leiar",                                       Code = "styreleder",                            Description = "Fysisk- eller juridisk person som er styremedlem og leiar eit styre", Urn = "urn:altinn:external-role:ccr:styreleder" },
            new Role() { Id = Guid.Parse("2e2fc06e-d9b7-4cd9-91bc-d5de766d20de"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Den personlege konkursen angår",                      Code = "personlige-konkurs",                    Description = "Den personlege konkursen angår", Urn = "urn:altinn:external-role:ccr:personlige-konkurs" },
            new Role() { Id = Guid.Parse("e852d758-e8dd-41ec-a1e2-4632deb6857d"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Norsk representant for utanlandsk eining",            Code = "norsk-representant",                    Description = "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i Noreg", Urn = "urn:altinn:external-role:ccr:norsk-representant" },
            new Role() { Id = Guid.Parse("db013059-4a8a-442d-bf90-b03539fe5dda"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson",                                       Code = "kontaktperson",                         Description = "Fysisk person som representerer ei verksemd", Urn = "urn:altinn:external-role:ccr:kontaktperson" },
            new Role() { Id = Guid.Parse("69c4397a-9e34-4e73-9f69-534bc1bb74c8"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Kontaktperson NUF",                                   Code = "kontaktperson-nuf",                     Description = "Fysisk person som representerer ei verksemd - NUF", Urn = "urn:altinn:external-role:ccr:kontaktperson-nuf" },
            new Role() { Id = Guid.Parse("8f0cf433-954e-4680-a25d-a3cf9ffdf149"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Bestyrande reder",                                    Code = "bestyrende-reder",                      Description = "Bestyrande reder", Urn = "urn:altinn:external-role:ccr:bestyrende-reder" },
            new Role() { Id = Guid.Parse("9ce84a4d-4970-4ef2-8208-b8b8f4d45556"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Eigarkommune",                                        Code = "eierkommune",                           Description = "Eigarkommune", Urn = "urn:altinn:external-role:ccr:eierkommune" },
            new Role() { Id = Guid.Parse("2cacfb35-2346-4a8d-95f6-b6fa4206881c"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Bobestyrar",                                          Code = "bostyrer",                              Description = "Bestyrar av eit konkursbo eller dødsbo som er under offentleg skiftehandtering", Urn = "urn:altinn:external-role:ccr:bostyrer" },
            new Role() { Id = Guid.Parse("e4674211-034a-45f3-99ac-b2356984968a"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Helseforetak",                                        Code = "helseforetak",                          Description = "Helseforetak", Urn = "urn:altinn:external-role:ccr:helseforetak" },
            new Role() { Id = Guid.Parse("f76b997a-9bd8-4f7b-899f-fcd85d35669f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Revisor",                                             Code = "revisor",                               Description = "Revisor", Urn = "urn:altinn:external-role:ccr:revisor" },
            new Role() { Id = Guid.Parse("348b2f47-47ee-4084-abf8-68aa54c2b27f"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Forretningsførar",                                    Code = "forretningsforer",                      Description = "Forretningsførar", Urn = "urn:altinn:external-role:ccr:forretningsforer" },
            new Role() { Id = Guid.Parse("cfcf75af-9902-41f7-ab47-b77ba60bcae5"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Komplementar",                                        Code = "komplementar",                          Description = "Komplementar", Urn = "urn:altinn:external-role:ccr:komplementar" },
            new Role() { Id = Guid.Parse("50cc3f41-4dde-4417-8c04-eea428f169dd"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Konkursdebitor",                                      Code = "konkursdebitor",                        Description = "Konkursdebitor", Urn = "urn:altinn:external-role:ccr:konkursdebitor" },
            new Role() { Id = Guid.Parse("d78dd1d8-a3f3-4ae6-807e-ea5149f47035"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Inngår i kyrkjeleg fellesråd",                        Code = "kirkelig-fellesraad",                   Description = "Inngår i kyrkjeleg fellesråd", Urn = "urn:altinn:external-role:ccr:kirkelig-fellesraad" },
            new Role() { Id = Guid.Parse("185f623b-f614-4a83-839c-1788764bd253"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Opplysningar om foretaket i heimalandet",             Code = "hovedforetak",                          Description = "Opplysningar om foretaket i heimalandet", Urn = "urn:altinn:external-role:ccr:hovedforetak" },
            new Role() { Id = Guid.Parse("46e27685-b3ba-423e-8b42-faab54de5817"), EntityTypeId = orgEntityTypeId, ProviderId = ccrProviderId, Name = "Reknskapsførar",                                      Code = "regnskapsforer",                        Description = "Reknskapsførar", Urn = "urn:altinn:external-role:ccr:regnskapsforer" },
            
            new Role() { Id = Guid.Parse("c497b499-7e98-423d-9fe7-ad5a6c3b71ad"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Primærnæring og næringsmiddel",             Code = "A0212",               Description = "Import, foredling, produksjon og/eller sal av primærnæringsprodukter og andre næringsmiddel. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0212" },
            new Role() { Id = Guid.Parse("151955ec-d8aa-4c14-a435-ffa96b26a9fb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Post/arkiv",                                Code = "A0236",               Description = "Rolle som gjer rett til å lese meldingstenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0236" },
            new Role() { Id = Guid.Parse("c2884487-a634-4537-95b4-bafb917b62a8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Ansvarleg revisor",                         Code = "A0237",               Description = "Delegerbar revisorrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0237" },
            new Role() { Id = Guid.Parse("10fcad57-7a91-4e02-a921-63e5751fbc24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisormedarbeidar",                        Code = "A0238",               Description = "Delegerbar revisorrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0238" },
            new Role() { Id = Guid.Parse("ebed65a5-dd87-4180-b898-e1da249b128d"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Rekneskapsførar med signeringsrett",        Code = "A0239",               Description = "Delegerbar rekneskapsrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0239" },
            new Role() { Id = Guid.Parse("9407620b-21b6-4538-b4d8-2b4eb339c373"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Rekneskapsførar utan signeringsrett",       Code = "A0240",               Description = "Delegerbar rekneskapsrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0240" },
            new Role() { Id = Guid.Parse("723a43ab-13d8-4585-81e2-e4c734b2d4fc"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Rekneskapsførar løn",                       Code = "A0241",               Description = "Delegerbar rekneskapsrolle med signeringsrett for tenester knytta til lønsrapportering. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0241" },
            new Role() { Id = Guid.Parse("6828080b-e846-4c51-b670-201af4917562"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Plan- og byggesak",                         Code = "A0278",               Description = "Rollen er reservert skjema og tenester som er godkjend av DiBK. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:A0278" },
            new Role() { Id = Guid.Parse("f4df0522-3034-405b-a9e5-83f971737033"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Skatteforhold for privatpersonar",          Code = "A0282",               Description = "Løyvet gjeld alle opplysningar om skatteforholda dine og om skatteforholda for enkeltpersonføretaket ditt. Ved regelverksendringar eller innføring av nye digitale tenester kan Skatteetaten endre løyvet.", Urn = "urn:altinn:rolecode:A0282" },
            new Role() { Id = Guid.Parse("92ea5544-ca64-4e03-9532-646b9f86ff65"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetslagd post",                         Code = "A0286",               Description = "Gir tlgang til taushetslagd post frå det offentlige. Ved regelverksendringer eller innføring av nye digitale tenester kan det bli endringer i tilganger som rollen gir", Urn = "urn:altinn:rolecode:A0286" },
            new Role() { Id = Guid.Parse("df34b69a-e0aa-4245-a840-3a850769b2bd"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetslagd post - oppvekst og utdanning", Code = "A0287",               Description = "Gir tlgang til taushetslagd post frå det offentlige innan oppvekst og utdanning", Urn = "urn:altinn:rolecode:A0287" },
            new Role() { Id = Guid.Parse("5fda4732-dd10-416d-b876-9e1715bbf21c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetslagd post - administrasjon",        Code = "A0288",               Description = "Gir tlgang til taushetslagd post frå det offentlige innan administrasjon", Urn = "urn:altinn:rolecode:A0288" },
            new Role() { Id = Guid.Parse("4652e98f-7a6b-4dc2-b061-fc8d6840e456"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Algetestdata",                              Code = "A0293",               Description = "Havforskningsinstituttet - registrering av algetestdata", Urn = "urn:altinn:rolecode:A0293" },
            new Role() { Id = Guid.Parse("c22c6add-dd5d-4735-87de-b75491018e50"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Transportløyvegaranti",                     Code = "A0294",               Description = "Statens vegvesen - rolle som gjer tilgang til app for transportløuvegarantistar", Urn = "urn:altinn:rolecode:A0294" },
            new Role() { Id = Guid.Parse("d8b9c47b-e5a7-4912-8aa8-1d2bab75e41c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorattesterar",                         Code = "A0298",               Description = "Rollen gir bruker tilgang til å attestere tjenester for avgiver som revisor. Ved regelverksendringer eller innføring av nye digitale tenester kan det bli endringer i tilganger som rollen gir", Urn = "urn:altinn:rolecode:A0298" },
            new Role() { Id = Guid.Parse("48f9e5ec-efd5-4863-baba-9697b8971666"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Tilgangsstyring",                           Code = "ADMAI",               Description = "Administrasjon av tilgangar", Urn = "urn:altinn:rolecode:ADMAI" },
            new Role() { Id = Guid.Parse("e078bb18-f55a-4a2d-8964-c599f41b29b5"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Programmeringsgrensesnitt (API)",           Code = "APIADM",              Description = "Delegerbar rolle som gir tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av verksemden.", Urn = "urn:altinn:rolecode:APIADM" },
            new Role() { Id = Guid.Parse("0ea4e5de-3fb4-499e-b013-1e1b4459af24"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Programmeringsgrensesnitt for NUF (API)",   Code = "APIADMNUF",           Description = "Delegerbar rolle som gir kontaktperson for norskregistrert utanlandsk føretak (NUF) tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av verksemden", Urn = "urn:altinn:rolecode:APIADMNUF" },
            new Role() { Id = Guid.Parse("60abf944-cf8c-4845-b310-83bcb6c77198"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorattesterar - MVA kompensasjon",      Code = "ATTST",               Description = "Revisor si attestering av RF-0009", Urn = "urn:altinn:rolecode:ATTST" },
            new Role() { Id = Guid.Parse("0a76304e-345b-4f22-bb31-4837a630eb7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbu tilgangsstyring",                 Code = "BOADM",               Description = "Gjeld advokatar og gjev moglegheit for tilgangsstyring av konkursbu", Urn = "urn:altinn:rolecode:BOADM" },
            new Role() { Id = Guid.Parse("7246639c-137b-4981-b172-6134c9fc1a7f"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbu lesetilgang",                     Code = "BOBEL",               Description = "Tilgang til å lese informasjon i tenesta Konkursbehandling", Urn = "urn:altinn:rolecode:BOBEL" },
            new Role() { Id = Guid.Parse("5f73b031-8b5b-45d8-a682-e9a7e75a7691"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Konkursbu skrivetilgang",                   Code = "BOBES",               Description = "Tilgang til å skrive informasjon i tenesta Konkursbehandling", Urn = "urn:altinn:rolecode:BOBES" },
            new Role() { Id = Guid.Parse("e0684f66-a46e-4706-a754-8889b532509c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "ECKEYROLE",                                 Code = "ECKEYROLE",           Description = "Nøkkelrolle for virksomhetsertifikatbrukere", Urn = "urn:altinn:rolecode:ECKEYROLE" },
            new Role() { Id = Guid.Parse("1225bc46-4b03-4b63-b6e8-58926b29a97b"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Eksplisitt tenestedelegering",              Code = "EKTJ",                Description = "Ikkje-delegerbar rolle for tenester som kun skal delegerast enkeltvis", Urn = "urn:altinn:rolecode:EKTJ" },
            new Role() { Id = Guid.Parse("cde501eb-0d23-410b-b728-00ab9d68fb2e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Godkjenning av bedriftshelsetjeneste",      Code = "GKBHT",               Description = "Godkjenning av bedriftshelsetjeneste", Urn = "urn:altinn:rolecode:GKBHT" },
            new Role() { Id = Guid.Parse("d9e05d40-9849-4982-bf04-aa03b19e4a66"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Hovudadministrator",                        Code = "HADM",                Description = "Denne rolla gir høve til å delegere alle roller og rettar for ein aktør, også dei ein ikkje har sjøl", Urn = "urn:altinn:rolecode:HADM" },
            new Role() { Id = Guid.Parse("98bebcac-d6bb-4343-97b8-0fe8bc744d7a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Økokrim rapportering",                      Code = "HVASK",               Description = "Tilgang til tenester frå Økokrim. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:HVASK" },
            new Role() { Id = Guid.Parse("27e1ef41-df4d-439e-b948-df136c139e81"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Klientadministrator",                       Code = "KLADM",               Description = "Tilgang til å administrere klientroller for rekneskapsførarar og revisorar", Urn = "urn:altinn:rolecode:KLADM" },
            new Role() { Id = Guid.Parse("b8e6dd1c-ca10-4ce6-9c27-53cdb3c275b3"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Kommunale tenester",                        Code = "KOMAB",               Description = "Rolle for kommunale tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:KOMAB" },
            new Role() { Id = Guid.Parse("010b4c49-bf56-44e3-b73b-84be7b2a5eb6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Løn og personalmedarbeidar",                Code = "LOPER",               Description = "Tilgang til løns- og personalrelaterte tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:LOPER" },
            new Role() { Id = Guid.Parse("0f276fc4-c201-4ff7-8e8a-caa3efe9c02a"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Parallell signering",                       Code = "PASIG",               Description = "Rett til å signere elementer frå andre avgjevarar", Urn = "urn:altinn:rolecode:PASIG" },
            new Role() { Id = Guid.Parse("23cade0a-287a-49e0-8957-22d5a14cb100"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Patent, varemerke og design",               Code = "PAVAD",               Description = "Tilgang til tenester frå Patentstyret. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:PAVAD" },
            new Role() { Id = Guid.Parse("696478f4-c85b-4bda-ace0-caa058fe5def"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Privatperson avgrensa retter",              Code = "PRIUT",               Description = "Delegerbare retter for tenester knytt til privatperson. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:PRIUT" },
            new Role() { Id = Guid.Parse("633cde7d-3604-45b2-ba8c-e16161cf2cf8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Rekneskapsmedarbeidar",                     Code = "REGNA",               Description = "Tilgang til rekneskapsrelaterte skjema og tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:REGNA" },
            new Role() { Id = Guid.Parse("1d71e23d-91b6-44ca-b171-c179028e7cdf"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Revisorrett",                               Code = "REVAI",               Description = "Delegerbare revisorrettar", Urn = "urn:altinn:rolecode:REVAI" },
            new Role() { Id = Guid.Parse("1a15b75c-2387-4278-ba3a-7eb1cffe1653"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Taushetslagd post frå kommunen",            Code = "SENS01",              Description = "Rolla gir tilgang til tenester med taushetsalgd informasjon frå kommunen.", Urn = "urn:altinn:rolecode:SENS01" },
            new Role() { Id = Guid.Parse("e427a9fb-4b6b-44b3-b873-689d174283b8"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Signerar av Samordna registermelding",      Code = "SIGNE",               Description = "Gjeld for signering på vegne av einingar/føretak. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:SIGNE" },
            new Role() { Id = Guid.Parse("16857e39-441f-4dd4-8592-aed94e816c04"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Avgrensa signeringsrett",                   Code = "SISKD",               Description = "Tilgang til å signere utvalde skjema og tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:SISKD" },
            new Role() { Id = Guid.Parse("b1213d79-03fa-4837-9193-e4b9fe24eccb"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Helse-, sosial- og velferdstenester",       Code = "UIHTL",               Description = "Tilgang til helse-, sosial- og velferdsrelaterte tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:UIHTL" },
            new Role() { Id = Guid.Parse("3c99647d-10b5-447e-9f0b-7bef1c7880f7"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Samferdsel",                                Code = "UILUF",               Description = "Tilgang til tenester relatert til samferdsel. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:UILUF" },
            new Role() { Id = Guid.Parse("dbaae9f8-107a-4222-9afd-d9f95cd5319c"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Utfyllar/innsendar",                        Code = "UTINN",               Description = "Denne rolla gir rett til eit breitt utval skjema og tenester som ikkje har så strenge krav til autorisasjon. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:UTINN" },
            new Role() { Id = Guid.Parse("af338fd5-3f1d-4ab5-8326-9dfecad26f71"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Energi, miljø og klima",                    Code = "UTOMR",               Description = "Tilgang til tenester relatert til energi, miljø og klima. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir", Urn = "urn:altinn:rolecode:UTOMR" },            
            new Role() { Id = Guid.Parse("478f710a-4af1-412d-9c67-de976fd0b229"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Hovudrolle for sensitive tenester",         Code = "SENS",                Description = "Hovudrolle for sensitive teneste", Urn = "urn:altinn:rolecode:SENS", IsKeyRole = false },

            new Role() { Id = Guid.Parse("1c6eeec1-fe70-4fc5-8b45-df4a2255dea6"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Privatperson",                              Code = "privatperson",        Description = "Privatperson", Urn = "urn:altinn:role:privatperson", IsKeyRole = false },
            new Role() { Id = Guid.Parse("e16ab886-1e1e-4f45-8f79-46f06f720f3e"), EntityTypeId = null,  ProviderId = a2ProviderId, Name = "Sjølregistrert brukar",                     Code = "selvregistrert",      Description = "Sjølregistrert brukar", Urn = "urn:altinn:role:selvregistrert", IsKeyRole = false }
        };

        await ingestService.IngestAndMergeData(roles, options, cancellationToken: cancellationToken);

        foreach (var item in rolesEng)
        {
            await roleService.UpsertTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in rolesNno)
        {
            await roleService.UpsertTranslation(item.Id, item, "nno", options: options, cancellationToken: cancellationToken);
        }

        await RoleLookup(roles, options: options, cancellationToken: cancellationToken);
    }

    private async Task RoleLookup(List<Role> roles, ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var urn = new List<RoleLookup>
        {
            new RoleLookup() { RoleId = roles.First(t => t.Code == "agent").Id, Key = "Urn", Value = "urn:altinn:role:agent" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "klientadministrator").Id, Key = "Urn", Value = "urn:altinn:role:klientadministrator" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "tilgangsstyrer").Id, Key = "Urn", Value = "urn:altinn:role:tilgangsstyrer" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "hovedadministrator").Id, Key = "Urn", Value = "urn:altinn:role:hovedadministrator" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "maskinporten-administrator").Id, Key = "Urn", Value = "urn:altinn:role:maskinporten-administrator" },
            
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-ados").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontaktperson-ados" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "nestleder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:nestleder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontorfelleskapmedlem").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontorfelleskapmedlem" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "organisasjonsledd-offentlig-sektor").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "saerskilt-oppdelt-enhet").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "daglig-leder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:daglig-leder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "deltaker-delt-ansvar").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:deltaker-delt-ansvar" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "innehaver").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:innehaver" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "deltaker-fullt-ansvar").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:deltaker-fullt-ansvar" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "varamedlem").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:varamedlem" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "observator").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:observator" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "styremedlem").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:styremedlem" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "styreleder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:styreleder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "personlige-konkurs").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:personlige-konkurs" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "norsk-representant").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:norsk-representant" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontaktperson" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-nuf").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontaktperson-nuf" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "bestyrende-reder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:bestyrende-reder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "eierkommune").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:eierkommune" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "bostyrer").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:bostyrer" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "helseforetak").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:helseforetak" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "revisor").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:revisor" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "forretningsforer").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:forretningsforer" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "komplementar").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:komplementar" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konkursdebitor").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:konkursdebitor" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kirkelig-fellesraad").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kirkelig-fellesraad" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "hovedforetak").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:hovedforetak" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "regnskapsforer").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:regnskapsforer" },
            
            new RoleLookup() { RoleId = roles.First(t => t.Code == "regnskapsforeradressat").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:regnskapsforeradressat" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:signerer" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "fusjonsovertaker").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:fusjonsovertaker" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "fisjonsovertaker").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:fisjonsovertaker" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "hovedenhet").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:hovedenhet" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "ikke-naeringsdrivende-hovedenhet").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:ikke-naeringsdrivende-hovedenhet" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist-fellesskap").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:prokurist-fellesskap" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist-hver-for-seg").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:prokurist-hver-for-seg" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:prokurist" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "revisoradressat").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:revisoradressat" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "sameier").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:sameier" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer-fellesskap").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:signerer-fellesskap" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer-hver-for-seg").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:signerer-hver-for-seg" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-kommune").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontaktperson-kommune" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-leder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:parti-organ-leder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "elektronisk-signeringsrettig").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:elektronisk-signeringsrettig" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "elektronisk-signeringsrett-tildeler").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:elektronisk-signeringsrett-tildeler" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "foretaksgruppe-med").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:foretaksgruppe-med" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-datter").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:konsern-datter" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-grunnlag").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:konsern-grunnlag" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-mor").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:konsern-mor" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "forestaar-avvikling").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:forestaar-avvikling" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "felles-registrert-med").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:felles-registrert-med" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "utleiebygg").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:utleiebygg" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "virksomhet-fellesskap-drifter").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:virksomhet-fellesskap-drifter" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "mva-utfyller").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:mva-utfyller" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "mva-signerer").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:mva-signerer" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-revisor").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:kontaktperson-revisor" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "stifter").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:stifter" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-varamedlem").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:parti-organ-varamedlem" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-nestleder").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:parti-organ-nestleder" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-styremedlem").Id, Key = "Urn", Value = "urn:altinn:external-role:ccr:parti-organ-styremedlem" },
            
            new RoleLookup() { RoleId = roles.First(t => t.Code == "privatperson").Id, Key = "Urn", Value = "urn:altinn:role:privatperson" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "selvregistrert").Id, Key = "Urn", Value = "urn:altinn:role:selvregistrert" }
        };

        var legacyCodes = new List<RoleLookup>
        {
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-ados").Id, Key = "LegacyCode", Value = "ADOS" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "nestleder").Id, Key = "LegacyCode", Value = "NEST" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontorfelleskapmedlem").Id, Key = "LegacyCode", Value = "KTRF" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "organisasjonsledd-offentlig-sektor").Id, Key = "LegacyCode", Value = "ORGL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "saerskilt-oppdelt-enhet").Id, Key = "LegacyCode", Value = "OPMV" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "daglig-leder").Id, Key = "LegacyCode", Value = "DAGL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "deltaker-delt-ansvar").Id, Key = "LegacyCode", Value = "DTPR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "innehaver").Id, Key = "LegacyCode", Value = "INNH" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "deltaker-fullt-ansvar").Id, Key = "LegacyCode", Value = "DTSO" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "varamedlem").Id, Key = "LegacyCode", Value = "VARA" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "observator").Id, Key = "LegacyCode", Value = "OBS" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "styremedlem").Id, Key = "LegacyCode", Value = "MEDL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "styreleder").Id, Key = "LegacyCode", Value = "LEDE" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "personlige-konkurs").Id, Key = "LegacyCode", Value = "KENK" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "norsk-representant").Id, Key = "LegacyCode", Value = "REPR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson").Id, Key = "LegacyCode", Value = "KONT" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-nuf").Id, Key = "LegacyCode", Value = "KNUF" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "bestyrende-reder").Id, Key = "LegacyCode", Value = "BEST" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "eierkommune").Id, Key = "LegacyCode", Value = "EIKM" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "bostyrer").Id, Key = "LegacyCode", Value = "BOBE" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "helseforetak").Id, Key = "LegacyCode", Value = "HLSE" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "revisor").Id, Key = "LegacyCode", Value = "REVI" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "forretningsforer").Id, Key = "LegacyCode", Value = "FFØR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "komplementar").Id, Key = "LegacyCode", Value = "KOMP" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konkursdebitor").Id, Key = "LegacyCode", Value = "KDEB" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kirkelig-fellesraad").Id, Key = "LegacyCode", Value = "KIRK" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "hovedforetak").Id, Key = "LegacyCode", Value = "HFOR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "regnskapsforer").Id, Key = "LegacyCode", Value = "REGN" },

            new RoleLookup() { RoleId = roles.First(t => t.Code == "regnskapsforeradressat").Id, Key = "LegacyCode", Value = "RFAD" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer").Id, Key = "LegacyCode", Value = "SIGN" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "fusjonsovertaker").Id, Key = "LegacyCode", Value = "FUSJ" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "fisjonsovertaker").Id, Key = "LegacyCode", Value = "FISJ" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "hovedenhet").Id, Key = "LegacyCode", Value = "BEDR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "ikke-naeringsdrivende-hovedenhet").Id, Key = "LegacyCode", Value = "AAFY" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist-fellesskap").Id, Key = "LegacyCode", Value = "POFE" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist-hver-for-seg").Id, Key = "LegacyCode", Value = "POHV" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "prokurist").Id, Key = "LegacyCode", Value = "PROK" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "revisoradressat").Id, Key = "LegacyCode", Value = "READ" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "sameier").Id, Key = "LegacyCode", Value = "SAM" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer-fellesskap").Id, Key = "LegacyCode", Value = "SIFE" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "signerer-hver-for-seg").Id, Key = "LegacyCode", Value = "SIHV" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-kommune").Id, Key = "LegacyCode", Value = "KOMK" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-leder").Id, Key = "LegacyCode", Value = "HLED" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "elektronisk-signeringsrettig").Id, Key = "LegacyCode", Value = "ESGR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "elektronisk-signeringsrett-tildeler").Id, Key = "LegacyCode", Value = "ETDL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "foretaksgruppe-med").Id, Key = "LegacyCode", Value = "FGRP" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-datter").Id, Key = "LegacyCode", Value = "KDAT" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-grunnlag").Id, Key = "LegacyCode", Value = "KGRL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "konsern-mor").Id, Key = "LegacyCode", Value = "KMOR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "forestaar-avvikling").Id, Key = "LegacyCode", Value = "AVKL" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "felles-registrert-med").Id, Key = "LegacyCode", Value = "FEMV" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "utleiebygg").Id, Key = "LegacyCode", Value = "UTBG" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "virksomhet-fellesskap-drifter").Id, Key = "LegacyCode", Value = "VIFE" },
            
            new RoleLookup() { RoleId = roles.First(t => t.Code == "mva-utfyller").Id, Key = "LegacyCode", Value = "MVAU" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "mva-signerer").Id, Key = "LegacyCode", Value = "MVAG" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "kontaktperson-revisor").Id, Key = "LegacyCode", Value = "SREVA" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "stifter").Id, Key = "LegacyCode", Value = "STFT" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-varamedlem").Id, Key = "LegacyCode", Value = "HVAR" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-nestleder").Id, Key = "LegacyCode", Value = "HNST" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "parti-organ-styremedlem").Id, Key = "LegacyCode", Value = "HMDL" },
            
            new RoleLookup() { RoleId = roles.First(t => t.Code == "privatperson").Id, Key = "LegacyCode", Value = "PRIV" },
            new RoleLookup() { RoleId = roles.First(t => t.Code == "selvregistrert").Id, Key = "LegacyCode", Value = "SELN" }
        };

        var mergeFilter = new List<string>()
        {
            "RoleId", "Key"
        };

        await ingestService.IngestAndMergeData(legacyCodes, options: options, mergeFilter, cancellationToken);
        await ingestService.IngestAndMergeData(urn, options: options, mergeFilter, cancellationToken);
    }

    /// <summary>
    /// Ingest all static rolemap data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestRoleMap(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        /*DAGL*/ var roleDagl  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:daglig-leder")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "daglig-leder"));
        /*LEDE*/ var roleLede  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:styreleder")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "styreleder"));
        /*INNH*/ var roleInnh  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:innehaver")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "innehaver"));
        /*DTSO*/ var roleDtso  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:deltaker-fullt-ansvar")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "deltaker-fullt-ansvar"));
        /*DTPR*/ var roleDtpr  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:deltaker-delt-ansvar")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "deltaker-delt-ansvar"));
        /*KOMP*/ var roleKomp  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:komplementar")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "komplementar"));
        /*BEST*/ var roleBest  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:bestyrende-reder")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "bestyrende-reder"));
        /*BOBE*/ var roleBobe  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:bostyrer")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "bostyrer"));
        /*REGN*/ var roleRegn  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:regnskapsforer")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "regnskapsforer"));
        /*REVI*/ var roleRevi  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:revisor")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "revisor"));
        /*KNUF*/ var roleKnuf  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:kontaktperson-nuf")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "kontaktperson-nuf"));
        
        /*FFØR*/ var roleFfor  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:forretningsforer")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "forretningsforer"));
        /*KEMN*/ var roleKemn  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:kontaktperson-ados")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "kontaktperson-ados"));
        /*PRIV*/ var rolePriv  = (await roleService.Get(t => t.Urn, "urn:altinn:role:privatperson")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "privatperson"));
        /*KOMK*/ var roleKomk  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:kontaktperson-kommune")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "kontaktperson-kommune"));
        /*KONT*/ var roleKont  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:kontaktperson")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "kontaktperson"));
        /*MEDL*/ var roleMedl  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:styremedlem")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "styremedlem"));
        /*MVAG*/ var roleMvag  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:mva-signerer")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "mva-signerer"));
        /*MVAU*/ var roleMvau  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:mva-utfyller")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "mva-utfyller"));
        /*NEST*/ var roleNest  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:nestleder")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "nestleder"));
        /*REPR*/ var roleRepr  = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:norsk-representant")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "norsk-representant"));
        /*SAM*/  var roleSam   = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:sameier")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "sameier"));
        /*SENS*/ var roleSens  = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:SENS")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "Sensitive-tjenester"));
        /*SREVA*/var roleSreva = (await roleService.Get(t => t.Urn, "urn:altinn:external-role:ccr:kontaktperson-revisor")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "kontaktperson-revisor"));


        var roleKLA = (await roleService.Get(t => t.Urn, "urn:altinn:role:klientadministrator")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "klientadministrator"));
        var roleTS = (await roleService.Get(t => t.Urn, "urn:altinn:role:tilgangsstyrer")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "tilgangsstyrer"));
        var roleHA = (await roleService.Get(t => t.Urn, "urn:altinn:role:hovedadministrator")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "hovedadministrator"));
        var roleMPA = (await roleService.Get(t => t.Urn, "urn:altinn:role:maskinporten-administrator")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "maskinporten-administrator"));
        /*A0212*/    var roleA0212     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0212")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0212"));
        /*A0236*/    var roleA0236     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0236")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0236"));
        /*A0237*/    var roleA0237     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0237")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0237"));
        /*A0238*/    var roleA0238     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0238")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0238"));
        /*A0239*/    var roleA0239     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0239")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0239"));
        /*A0240*/    var roleA0240     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0240")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0240"));
        /*A0241*/    var roleA0241     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0241")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0241"));
        /*A0278*/    var roleA0278     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0278")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0278"));
        /*A0282*/    var roleA0282     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0282")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0282"));
        /*A0286*/    var roleA0286     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0286")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0286"));
        /*A0293*/    var roleA0293     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0293")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0293"));
        /*A0294*/    var roleA0294     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0294")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0294"));
        /*A0298*/    var roleA0298     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:A0298")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "A0298"));
        /*ADMAI*/    var roleADMAI     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:ADMAI")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "ADMAI"));
        /*APIADM*/   var roleAPIADM    = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:APIADM")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "APIADM"));
        /*APIADMNUF*/var roleAPIADMNUF = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:APIADMNUF")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "APIADMNUF"));
        /*ATTST*/    var roleATTST     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:ATTST")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "ATTST"));
        /*BOADM*/    var roleBOADM     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:BOADM")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "BOADM"));
        /*BOBEL*/    var roleBOBEL     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:BOBEL")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "BOBEL"));
        /*BOBES*/    var roleBOBES     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:BOBES")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "BOBES"));
        /*ECKEYROLE*/var roleECKEYROLE = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:ECKEYROLE")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "ECKEYROLE"));
        /*EKTJ*/     var roleEKTJ      = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:EKTJ")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "EKTJ"));
        /*HADM*/     var roleHADM      = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:HADM")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "HADM"));
        /*HVASK*/    var roleHVASK     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:HVASK")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "HVASK"));
        /*KLADM*/    var roleKLADM     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:KLADM")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "KLADM"));
        /*KOMAB*/    var roleKOMAB     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:KOMAB")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "KOMAB"));
        /*LOPER*/    var roleLOPER     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:LOPER")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "LOPER"));
        /*PASIG*/    var rolePASIG     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:PASIG")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "PASIG"));
        /*PAVAD*/    var rolePAVAD     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:PAVAD")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "PAVAD"));
        /*PRIUT*/    var rolePRIUT     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:PRIUT")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "PRIUT"));
        /*REGNA*/    var roleREGNA     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:REGNA")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "REGNA"));
        /*SIGNE*/    var roleSIGNE     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:SIGNE")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "SIGNE"));
        /*SISKD*/    var roleSISKD     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:SISKD")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "SISKD"));
        /*UIHTL*/    var roleUIHTL     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:UIHTL")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "UIHTL"));
        /*UILUF*/    var roleUILUF     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:UILUF")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "UILUF"));
        /*UTINN*/    var roleUTINN     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:UTINN")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "UTINN"));
        /*UTOMR*/    var roleUTOMR     = (await roleService.Get(t => t.Urn, "urn:altinn:rolecode:UTOMR")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Role not found '{0}'", "UTOMR"));
        
        var roleMaps = new List<RoleMap>()
        {
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleKLA },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleKLA },

            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleTS },
            new RoleMap() { HasRoleId = roleKnuf, GetRoleId = roleTS },

            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleHA },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleHA },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleHA },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleHA },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleHA },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleHA },

            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleMPA },
            new RoleMap() { HasRoleId = roleKnuf, GetRoleId = roleMPA },
            
            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0212 },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleA0212 },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0236 },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleA0236 },

            new RoleMap() { HasRoleId = roleRevi, GetRoleId = roleA0237 },

            new RoleMap() { HasRoleId = roleRevi, GetRoleId = roleA0238 },

            new RoleMap() { HasRoleId = roleRegn, GetRoleId = roleA0239 },

            new RoleMap() { HasRoleId = roleRegn, GetRoleId = roleA0240 },

            new RoleMap() { HasRoleId = roleRegn, GetRoleId = roleA0241 },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0278 },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleA0278 },

            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleA0282 },

            new RoleMap() { HasRoleId = roleSens, GetRoleId = roleA0286 },

            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0293 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0293 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0293 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0293 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0293 },

            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0294 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0294 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0294 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0294 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0294 },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleA0298 },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleA0298 },

            new RoleMap() { HasRoleId = roleBest,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleBobe,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleDagl,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleDtpr,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleDtso,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleFfor,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleInnh,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleKemn,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleKnuf,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleKomk,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleKomp,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleLede,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = rolePriv,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleRepr,  GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleSam,   GetRoleId = roleADMAI },
            new RoleMap() { HasRoleId = roleSreva, GetRoleId = roleADMAI },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleAPIADM },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleAPIADM },

            new RoleMap() { HasRoleId = roleKnuf, GetRoleId = roleAPIADMNUF },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleMvag, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleATTST },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleATTST },

            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleBOADM },

            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleBOBEL },

            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleBOBES },

            new RoleMap() { HasRoleId = roleBest,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleBobe,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleDagl,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleDtpr,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleDtso,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleInnh,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleKemn,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleKnuf,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleKomp,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleLede,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleRepr,  GetRoleId = roleECKEYROLE },
            new RoleMap() { HasRoleId = roleSreva, GetRoleId = roleECKEYROLE },

            new RoleMap() { HasRoleId = roleSens, GetRoleId = roleEKTJ },
            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleHADM },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleHADM },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleHADM },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleHADM },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleHADM },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleHADM },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleHVASK },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleHVASK },

            new RoleMap() { HasRoleId = roleBest,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleBobe,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleDagl,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleDtpr,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleDtso,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleInnh,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleKomp,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleLede,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleRepr,  GetRoleId = roleKLADM },
            new RoleMap() { HasRoleId = roleSreva, GetRoleId = roleKLADM },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleKOMAB },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleKOMAB },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleMvag, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleLOPER },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleLOPER },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleKnuf, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = rolePASIG },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = rolePASIG },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = rolePAVAD },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = rolePAVAD },

            new RoleMap() { HasRoleId = rolePriv, GetRoleId = rolePRIUT },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleMedl, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleMvag, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleMvau, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleREGNA },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleREGNA },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleSIGNE },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleSIGNE },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleMvag, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleSISKD },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleSISKD },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleUIHTL },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleUIHTL },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleUILUF },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleUILUF },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleKemn, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleKont, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleMedl, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleUTINN },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleUTINN },

            new RoleMap() { HasRoleId = roleBest, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleBobe, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleDagl, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleDtpr, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleDtso, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleFfor, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleInnh, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleKomk, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleKomp, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleLede, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleMedl, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleNest, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = rolePriv, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleRepr, GetRoleId = roleUTOMR },
            new RoleMap() { HasRoleId = roleSam,  GetRoleId = roleUTOMR }
        };

        await ingestService.IngestAndMergeData(roleMaps, options: options, GetRoleMapMergeMatchFilter, cancellationToken);
    }

    private static readonly IReadOnlyList<string> GetRoleMapMergeMatchFilter = new List<string>() { "hasroleid", "getroleid" }.AsReadOnly();

    /// <summary>
    /// Ingest all static areagroup data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestAreaGroup(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var orgEntityTypeId = (await entityTypeService.Get(t => t.Name, "Organisasjon")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found '{0}'", "Organisasjon"));

        var areaGroups = new List<AreaGroup>()
        {
            new AreaGroup() { Id = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145"), Name = "Allment", Description = "Standard gruppe", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4"), Name = "Bransje", Description = "For bransje grupper", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159"), Name = "Særskilt", Description = "For de sære tingene", EntityTypeId = orgEntityTypeId }
        };

        var areaGroupsEng = new List<AreaGroup>()
        {
            new AreaGroup() { Id = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145"), Name = "General", Description = "Standard group", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4"), Name = "Industry", Description = "For industry groups", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159"), Name = "Special", Description = "For the unique things", EntityTypeId = orgEntityTypeId }
        };

        var areaGroupsNno = new List<AreaGroup>()
        {
            new AreaGroup() { Id = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145"), Name = "Allment", Description = "Standard gruppe", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4"), Name = "Bransje", Description = "For bransjagrupper", EntityTypeId = orgEntityTypeId },
            new AreaGroup() { Id = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159"), Name = "Særskilt", Description = "For dei sære tinga", EntityTypeId = orgEntityTypeId }
        };

        foreach (var item in areaGroups)
        {
            await areaGroupService.Upsert(item, options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in areaGroupsEng)
        {
            await areaGroupService.UpsertTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in areaGroupsNno)
        {
            await areaGroupService.UpsertTranslation(item.Id, item, "nno", options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static area data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestArea(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var areas = new List<Area>()
        {
            new Area() { Id = Guid.Parse("7d32591d-34b7-4afc-8afa-013722f8c05d"), Urn = "accesspackage:area:skatt_avgift_regnskap_og_toll", Name = "Skatt, avgift, regnskap og toll", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til skatt, avgift, regnskap og toll.", IconUrl = $"{iconBaseUrl}Aksel_Money_SackKroner.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f7f3b02-8b5a-4823-9468-0f4646d3a790"), Urn = "accesspackage:area:personale", Name = "Personale", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til personale.", IconUrl = $"{iconBaseUrl}Aksel_People_PersonGroup.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("a8834a7c-ed89-4c73-b5d5-19a2347f3b13"), Urn = "accesspackage:area:miljo_ulykke_og_sikkerhet", Name = "Miljø, ulykke og sikkerhet", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til miljø, ulykke og sikkerhet.", IconUrl = $"{iconBaseUrl}Aksel_People_HandHeart.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f938de8-34f2-4bab-a0c6-3a3eb64aad3b"), Urn = "accesspackage:area:post_og_arkiv", Name = "Post og arkiv", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til post og arkiv.", IconUrl = $"{iconBaseUrl}Aksel_Interface_EnvelopeClosed.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("3f5df819-7aca-49e1-bf6f-3e8f120f20d1"), Urn = "accesspackage:area:forhold_ved_virksomheten", Name = "Forhold ved virksomheten", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til forhold ved virksomheten.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings3.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("892e98c6-1696-46e7-9bb1-59c08761ec64"), Urn = "accesspackage:area:integrasjoner", Name = "Integrasjoner", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til integrasjoner.", IconUrl = $"{iconBaseUrl}Aksel_Interface_RotateLeft.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("e4ae823f-41db-46ed-873f-8a5d1378fff8"), Urn = "accesspackage:area:administrere_tilganger", Name = "Administrere tilganger", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til administrere tilganger.", IconUrl = $"{iconBaseUrl}Altinn_Administrere-tilganger_PersonLock.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("fc93d25e-80bc-469a-aa43-a6cee80eb3e2"), Urn = "accesspackage:area:jordbruk_skogbruk_jakt_fiske_og_akvakultur", Name = "Jordbruk, skogbruk, jakt, fiske og akvakultur", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til jordbruk, skogbruk, jakt, fiske og akvakultur.", IconUrl = $"{iconBaseUrl}Aksel_Nature-and-animals-Plant.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("536b317c-ef85-45d4-9b48-6511578e1952"), Urn = "accesspackage:area:bygg_anlegg_og_eiendom", Name = "Bygg, anlegg og eiendom", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til bygg, anlegg og eiendom.", IconUrl = $"{iconBaseUrl}Altinn_Bygg-anlegg-og-eiendom_HandHouse.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6ff90072-566b-4acd-baac-ec477534e712"), Urn = "accesspackage:area:transport_og_lagring", Name = "Transport og lagring", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til transport og lagring.", IconUrl = $"{iconBaseUrl}Aksel_Transportation_Truck.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("eab59b26-833f-40ca-9e27-72107e8f1908"), Urn = "accesspackage:area:helse_pleie_omsorg_og_vern", Name = "Helse, pleie, omsorg og vern", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til helse, pleie, omsorg og vern.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_Hospital.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("7326614f-cf7c-492e-8e7f-d74e6e4a8970"), Urn = "accesspackage:area:oppvekst_og_utdanning", Name = "Oppvekst og utdanning", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til oppvekst og utdanning.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings2.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6e152c10-0f63-4060-9b14-66808e7ac320"), Urn = "accesspackage:area:energi_vann_avlop_og_avfall", Name = "Energi, vann, avløp og avfall", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til energi, vann, avløp og avfall.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_TapWater.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("10c2dd29-5ab3-4a26-900e-8e2326150353"), Urn = "accesspackage:area:industrier", Name = "Industrier", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til industrier.", IconUrl = $"{iconBaseUrl}Altinn_Industrier_Factory.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("5996ba37-6db0-4391-8918-b1b0bd4b394b"), Urn = "accesspackage:area:kultur_og_frivillighet", Name = "Kultur og frivillighet", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til kultur og frivillighet.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_HeadHeart.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("3797e9f0-dd83-404c-9897-e356c32ef600"), Urn = "accesspackage:area:handel_overnatting_og_servering", Name = "Handel, overnatting og servering", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til handel, overnatting og servering.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_TrayFood.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("e31169f6-d4c7-4e45-93c7-f90bc285b639"), Urn = "accesspackage:area:andre_tjenesteytende_naeringer", Name = "Andre tjenesteytende næringer", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til andre tjenesteytende næringer.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Reception.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("64cbcdc8-01c9-448c-b3d2-eb9582beb3c2"), Urn = "accesspackage:area:fullmakter_for_regnskapsforer", Name = "Fullmakter for regnskapsfører", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til fullmakter for regnskapsfører.", IconUrl = $"{iconBaseUrl}Aksel_Home_Calculator.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("7df15290-f43c-4831-a1b4-3edfa43e526d"), Urn = "accesspackage:area:fullmakter_for_revisor", Name = "Fullmakter for revisor", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til fullmakter for revisor.", IconUrl = $"{iconBaseUrl}Aksel_Files-and-application_FileSearch.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("f3daddb7-6e21-455e-b6d2-65a281375b6b"), Urn = "accesspackage:area:fullmakter_for_konkursbo", Name = "Fullmakter for konkursbo", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til fullmakter for konkursbo.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("0195efb8-7c80-76b3-bb86-ae9dfd74bca2"), Urn = "accesspackage:area:fullmakter_for_forretningsforer", Name = "Fullmakter for forretningsfører", Description = "Dette fullmaktsområdet omfatter tilgangspakker knyttet til fullmakter for forretningsfører.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
        };

        var areasEng = new List<Area>()
        {
            new Area() { Id = Guid.Parse("7d32591d-34b7-4afc-8afa-013722f8c05d"), Urn = "accesspackage:area:skatt_avgift_regnskap_og_toll", Name = "Taxes, Fees, Accounting and Customs", Description = "This authorization area includes access packages related to taxes, fees, accounting, and customs.", IconUrl = $"{iconBaseUrl}Aksel_Money_SackKroner.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f7f3b02-8b5a-4823-9468-0f4646d3a790"), Urn = "accesspackage:area:personale", Name = "Personnel", Description = "This authorization area includes access packages related to personnel.", IconUrl = $"{iconBaseUrl}Aksel_People_PersonGroup.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("a8834a7c-ed89-4c73-b5d5-19a2347f3b13"), Urn = "accesspackage:area:miljo_ulykke_og_sikkerhet", Name = "Environment, Accident and Safety", Description = "This authorization area includes access packages related to environment, accident, and safety.", IconUrl = $"{iconBaseUrl}Aksel_People_HandHeart.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f938de8-34f2-4bab-a0c6-3a3eb64aad3b"), Urn = "accesspackage:area:post_og_arkiv", Name = "Mail and Archive", Description = "This authorization area includes access packages related to mail and archive.", IconUrl = $"{iconBaseUrl}Aksel_Interface_EnvelopeClosed.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("3f5df819-7aca-49e1-bf6f-3e8f120f20d1"), Urn = "accesspackage:area:forhold_ved_virksomheten", Name = "Business Affairs", Description = "This authorization area includes access packages related to business affairs.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings3.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("892e98c6-1696-46e7-9bb1-59c08761ec64"), Urn = "accesspackage:area:integrasjoner", Name = "Integrations", Description = "This authorization area includes access packages related to integrations.", IconUrl = $"{iconBaseUrl}Aksel_Interface_RotateLeft.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("e4ae823f-41db-46ed-873f-8a5d1378fff8"), Urn = "accesspackage:area:administrere_tilganger", Name = "Manage Access", Description = "This authorization area includes access packages related to managing access.", IconUrl = $"{iconBaseUrl}Altinn_Administrere-tilganger_PersonLock.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("fc93d25e-80bc-469a-aa43-a6cee80eb3e2"), Urn = "accesspackage:area:jordbruk_skogbruk_jakt_fiske_og_akvakultur", Name = "Agriculture, Forestry, Hunting, Fishing and Aquaculture", Description = "This authorization area includes access packages related to agriculture, forestry, hunting, fishing and aquaculture.", IconUrl = $"{iconBaseUrl}Aksel_Nature-and-animals-Plant.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("536b317c-ef85-45d4-9b48-6511578e1952"), Urn = "accesspackage:area:bygg_anlegg_og_eiendom", Name = "Construction, Infrastructure and Real Estate", Description = "This authorization area includes access packages related to construction, infrastructure and real estate.", IconUrl = $"{iconBaseUrl}Altinn_Bygg-anlegg-og-eiendom_HandHouse.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6ff90072-566b-4acd-baac-ec477534e712"), Urn = "accesspackage:area:transport_og_lagring", Name = "Transport and Storage", Description = "This authorization area includes access packages related to transport and storage.", IconUrl = $"{iconBaseUrl}Aksel_Transportation_Truck.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("eab59b26-833f-40ca-9e27-72107e8f1908"), Urn = "accesspackage:area:helse_pleie_omsorg_og_vern", Name = "Health, Care and Protection", Description = "This authorization area includes access packages related to health, care and protection.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_Hospital.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("7326614f-cf7c-492e-8e7f-d74e6e4a8970"), Urn = "accesspackage:area:oppvekst_og_utdanning", Name = "Childhood and Education", Description = "This authorization area includes access packages related to childhood and education.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings2.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6e152c10-0f63-4060-9b14-66808e7ac320"), Urn = "accesspackage:area:energi_vann_avlop_og_avfall", Name = "Energy, Water, Sewage and Waste", Description = "This authorization area includes access packages related to energy, water, sewage and waste.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_TapWater.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("10c2dd29-5ab3-4a26-900e-8e2326150353"), Urn = "accesspackage:area:industrier", Name = "Industries", Description = "This authorization area includes access packages related to industries.", IconUrl = $"{iconBaseUrl}Altinn_Industrier_Factory.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("5996ba37-6db0-4391-8918-b1b0bd4b394b"), Urn = "accesspackage:area:kultur_og_frivillighet", Name = "Culture and Volunteering", Description = "This authorization area includes access packages related to culture and volunteering.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_HeadHeart.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("3797e9f0-dd83-404c-9897-e356c32ef600"), Urn = "accesspackage:area:handel_overnatting_og_servering", Name = "Commerce, Accommodation and Catering", Description = "This authorization area includes access packages related to commerce, accommodation and catering.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_TrayFood.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("e31169f6-d4c7-4e45-93c7-f90bc285b639"), Urn = "accesspackage:area:andre_tjenesteytende_naeringer", Name = "Other Service Industries", Description = "This authorization area includes access packages related to other service industries.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Reception.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("64cbcdc8-01c9-448c-b3d2-eb9582beb3c2"), Urn = "accesspackage:area:fullmakter_for_regnskapsforer", Name = "Authorizations for Accountants", Description = "This authorization area includes access packages related to authorizations for accountants.", IconUrl = $"{iconBaseUrl}Aksel_Home_Calculator.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("7df15290-f43c-4831-a1b4-3edfa43e526d"), Urn = "accesspackage:area:fullmakter_for_revisor", Name = "Authorizations for Auditors", Description = "This authorization area includes access packages related to authorizations for auditors.", IconUrl = $"{iconBaseUrl}Aksel_Files-and-application_FileSearch.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("f3daddb7-6e21-455e-b6d2-65a281375b6b"), Urn = "accesspackage:area:fullmakter_for_konkursbo", Name = "Authorizations for Bankruptcy Estates", Description = "This authorization area includes access packages related to authorizations for bankruptcy estates.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("0195efb8-7c80-76b3-bb86-ae9dfd74bca2"), Urn = "accesspackage:area:fullmakter_for_forretningsforer", Name = "Authorizations for Bussineses", Description = "This authorization area includes access packages related to authorizations for bussineses.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
        };

        var areasNno = new List<Area>()
        {
            new Area() { Id = Guid.Parse("7d32591d-34b7-4afc-8afa-013722f8c05d"), Urn = "accesspackage:area:skatt_avgift_regnskap_og_toll", Name = "Skatt, avgift, rekneskap og toll", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til skatt, avgift, rekneskap og toll.", IconUrl = $"{iconBaseUrl}Aksel_Money_SackKroner.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f7f3b02-8b5a-4823-9468-0f4646d3a790"), Urn = "accesspackage:area:personale", Name = "Personale", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til personalet.", IconUrl = $"{iconBaseUrl}Aksel_People_PersonGroup.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("a8834a7c-ed89-4c73-b5d5-19a2347f3b13"), Urn = "accesspackage:area:miljo_ulykke_og_sikkerhet", Name = "Miljø, ulykke og tryggleik", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til miljø, ulykke og tryggleik.", IconUrl = $"{iconBaseUrl}Aksel_People_HandHeart.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("6f938de8-34f2-4bab-a0c6-3a3eb64aad3b"), Urn = "accesspackage:area:post_og_arkiv", Name = "Post og arkiv", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til post og arkiv.", IconUrl = $"{iconBaseUrl}Aksel_Interface_EnvelopeClosed.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("3f5df819-7aca-49e1-bf6f-3e8f120f20d1"), Urn = "accesspackage:area:forhold_ved_virksomheten", Name = "Forhold ved verksemda", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til forhold ved verksemda.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings3.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("892e98c6-1696-46e7-9bb1-59c08761ec64"), Urn = "accesspackage:area:integrasjoner", Name = "Integrasjonar", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til integrasjonar.", IconUrl = $"{iconBaseUrl}Aksel_Interface_RotateLeft.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("e4ae823f-41db-46ed-873f-8a5d1378fff8"), Urn = "accesspackage:area:administrere_tilganger", Name = "Administrere tilgongar", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til administrering av tilgongar.", IconUrl = $"{iconBaseUrl}Altinn_Administrere-tilganger_PersonLock.svg", GroupId = Guid.Parse("7E2A3AF8-08CB-43A9-BDD7-7D5C7E377145") },
            new Area() { Id = Guid.Parse("fc93d25e-80bc-469a-aa43-a6cee80eb3e2"), Urn = "accesspackage:area:jordbruk_skogbruk_jakt_fiske_og_akvakultur", Name = "Jordbruk, skogbruk, jakt, fiske og akvakultur", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til jordbruk, skogbruk, jakt, fiske og akvakultur.", IconUrl = $"{iconBaseUrl}Aksel_Nature-and-animals-Plant.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("536b317c-ef85-45d4-9b48-6511578e1952"), Urn = "accesspackage:area:bygg_anlegg_og_eiendom", Name = "Bygg, anlegg og eigedom", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til bygg, anlegg og eigedom.", IconUrl = $"{iconBaseUrl}Altinn_Bygg-anlegg-og-eiendom_HandHouse.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6ff90072-566b-4acd-baac-ec477534e712"), Urn = "accesspackage:area:transport_og_lagring", Name = "Transport og lagring", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til transport og lagring.", IconUrl = $"{iconBaseUrl}Aksel_Transportation_Truck.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("eab59b26-833f-40ca-9e27-72107e8f1908"), Urn = "accesspackage:area:helse_pleie_omsorg_og_vern", Name = "Helse, pleie, omsorg og vern", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til helse, pleie, omsorg og vern.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_Hospital.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("7326614f-cf7c-492e-8e7f-d74e6e4a8970"), Urn = "accesspackage:area:oppvekst_og_utdanning", Name = "Oppvekst og utdanning", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til oppvekst og utdanning.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Buildings2.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("6e152c10-0f63-4060-9b14-66808e7ac320"), Urn = "accesspackage:area:energi_vann_avlop_og_avfall", Name = "Energi, vann, avløp og avfall", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til energi, vann, avløp og avfall.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_TapWater.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("10c2dd29-5ab3-4a26-900e-8e2326150353"), Urn = "accesspackage:area:industrier", Name = "Industriar", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til industriar.", IconUrl = $"{iconBaseUrl}Altinn_Industrier_Factory.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("5996ba37-6db0-4391-8918-b1b0bd4b394b"), Urn = "accesspackage:area:kultur_og_frivillighet", Name = "Kultur og frivillighet", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til kultur og frivillighet.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_HeadHeart.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("3797e9f0-dd83-404c-9897-e356c32ef600"), Urn = "accesspackage:area:handel_overnatting_og_servering", Name = "Handel, overnatting og servering", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til handel, overnatting og servering.", IconUrl = $"{iconBaseUrl}Aksel_Wellness_TrayFood.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("e31169f6-d4c7-4e45-93c7-f90bc285b639"), Urn = "accesspackage:area:andre_tjenesteytende_naeringer", Name = "Andre tenesteytande næringar", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til andre tenesteytande næringar.", IconUrl = $"{iconBaseUrl}Aksel_Workplace_Reception.svg", GroupId = Guid.Parse("3757643A-316D-4D0E-A52B-4DC7CDEBC0B4") },
            new Area() { Id = Guid.Parse("64cbcdc8-01c9-448c-b3d2-eb9582beb3c2"), Urn = "accesspackage:area:fullmakter_for_regnskapsforer", Name = "Fullmakter for rekneskapsførar", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til fullmakter for rekneskapsførar.", IconUrl = $"{iconBaseUrl}Aksel_Home_Calculator.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("7df15290-f43c-4831-a1b4-3edfa43e526d"), Urn = "accesspackage:area:fullmakter_for_revisor", Name = "Fullmakter for revisor", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til fullmakter for revisor.", IconUrl = $"{iconBaseUrl}Aksel_Files-and-application_FileSearch.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("f3daddb7-6e21-455e-b6d2-65a281375b6b"), Urn = "accesspackage:area:fullmakter_for_konkursbo", Name = "Fullmakter for konkursbo", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til fullmakter for konkursbo.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
            new Area() { Id = Guid.Parse("0195efb8-7c80-76b3-bb86-ae9dfd74bca2"), Urn = "accesspackage:area:fullmakter_for_forretningsforer", Name = "Fullmakter for forretningsfører", Description = "Dette fullmaktsområdet omfattar tilgangspakkar knytt til fullmakter for forretningsfører.", IconUrl = $"{iconBaseUrl}Aksel_Statistics-and-math_TrendDown.svg", GroupId = Guid.Parse("554F0321-53B8-4D97-BE12-6A585C507159") },
        };

        foreach (var item in areas)
        {
            await areaService.Upsert(item, options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in areasEng)
        {
            await areaService.UpsertTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }

        foreach (var item in areasNno)
        {
            await areaService.UpsertTranslation(item.Id, item, "eng", options: options, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Ingest all static package data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestPackage(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        //// TODO: Translate

        var provider = (await providerRepository.Get(t => t.Code, "sys-altinn3")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("Provider not found '{0}'", "Digitaliseringsdirektoratet"));
        var orgEntityType = (await entityTypeService.Get(t => t.Name, "Organisasjon")).FirstOrDefault()?.Id ?? throw new KeyNotFoundException(string.Format("EntityType not found '{0}'", "Organisasjon"));

        var areas = await areaService.Get();

        var area_oppvekst_og_utdanning = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:oppvekst_og_utdanning")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Oppvekst og utdanning")); /*7326614f-cf7c-492e-8e7f-d74e6e4a8970*/
        var area_skatt_avgift_regnskap_og_toll = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:skatt_avgift_regnskap_og_toll")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Skatt, avgift, regnskap og toll")); /*7d32591d-34b7-4afc-8afa-013722f8c05d*/
        var area_personale = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:personale")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Personale")); /*6f7f3b02-8b5a-4823-9468-0f4646d3a790*/
        var area_miljo_ulykke_og_sikkerhet = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:miljo_ulykke_og_sikkerhet")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Miljø, ulykke og sikkerhet")); /*a8834a7c-ed89-4c73-b5d5-19a2347f3b13*/
        var area_post_og_arkiv = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:post_og_arkiv")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Post og arkiv")); /*6f938de8-34f2-4bab-a0c6-3a3eb64aad3b*/
        var area_forhold_ved_virksomheten = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:forhold_ved_virksomheten")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Forhold ved virksomheten")); /*3f5df819-7aca-49e1-bf6f-3e8f120f20d1*/
        var area_integrasjoner = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:integrasjoner")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Integrasjoner")); /*892e98c6-1696-46e7-9bb1-59c08761ec64*/
        var area_administrere_tilganger = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:administrere_tilganger")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Administrere tilganger")); /*e4ae823f-41db-46ed-873f-8a5d1378fff8*/
        var area_jordbruk_skogbruk_jakt_fiske_og_akvakultur = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:jordbruk_skogbruk_jakt_fiske_og_akvakultur")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Jordbruk, skogbruk, jakt, fiske og akvakultur")); /*fc93d25e-80bc-469a-aa43-a6cee80eb3e2*/
        var area_bygg_anlegg_og_eiendom = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:bygg_anlegg_og_eiendom")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Bygg, anlegg og eiendom")); /*536b317c-ef85-45d4-9b48-6511578e1952*/
        var area_transport_og_lagring = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:transport_og_lagring")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Transport og lagring")); /*6ff90072-566b-4acd-baac-ec477534e712*/
        var area_helse_pleie_omsorg_og_vern = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:helse_pleie_omsorg_og_vern")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Helse, pleie, omsorg og vern")); /*eab59b26-833f-40ca-9e27-72107e8f1908*/
        var area_energi_vann_avlop_og_avfall = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:energi_vann_avlop_og_avfall")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Energi, vann,avløp og avfall")); /*6e152c10-0f63-4060-9b14-66808e7ac320*/
        var area_industrier = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:industrier")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Industrier")); /*10c2dd29-5ab3-4a26-900e-8e2326150353*/
        var area_kultur_og_frivillighet = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:kultur_og_frivillighet")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Kultur og frivillighet")); /*5996ba37-6db0-4391-8918-b1b0bd4b394b*/
        var area_handel_overnatting_og_servering = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:handel_overnatting_og_servering")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Handel, overnatting og servering")); /*3797e9f0-dd83-404c-9897-e356c32ef600*/
        var area_andre_tjenesteytende_naeringer = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:andre_tjenesteytende_naeringer")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Andre tjenesteytende næringer")); /*e31169f6-d4c7-4e45-93c7-f90bc285b639*/
        var area_fullmakter_for_regnskapsforer = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:fullmakter_for_regnskapsforer")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Fullmakter for regnskapsfører")); /*64cbcdc8-01c9-448c-b3d2-eb9582beb3c2*/
        var area_fullmakter_for_revisor = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:fullmakter_for_revisor")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Fullmakter for revisor")); /*7df15290-f43c-4831-a1b4-3edfa43e526d*/
        var area_fullmakter_for_konkursbo = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:fullmakter_for_konkursbo")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Fullmakter for konkursbo")); /*f3daddb7-6e21-455e-b6d2-65a281375b6b*/
        var area_fullmakter_for_forretningsforer = areas.FirstOrDefault(t => t.Urn == "accesspackage:area:fullmakter_for_forretningsforer")?.Id ?? throw new KeyNotFoundException(string.Format("Area not found '{0}'", "Fullmakter for forretningsforer")); /*0195efb8-7c80-76b3-bb86-ae9dfd74bca2*/

        var packages = new List<Package>()
        {
            new Package() { Id = Guid.Parse("1dba50d6-f604-48e9-bd41-82321b13e85c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:skatt-naering", Name = "Skatt næring", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til skatt for næringer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4c859601-9b2b-4662-af39-846f4117ad7a"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:skattegrunnlag", Name = "Skattegrunnlag", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til innhenting av skattegrunnlag. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("9a61b136-7810-4939-ab6d-84938e9a12c6"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:merverdiavgift", Name = "Merverdiavgift", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4f4d4567-2384-49fa-b34b-84b4b77139d8"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:motorvognavgift", Name = "Motorvognavgifter", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til motorvognavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("b4e69b54-895e-42c5-bf0d-861a571cd282"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:regnskap-okonomi-rapport", Name = "Regnskap og økonomirapportering", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til regnskap og øknomirapportering som ikke tilhører skatt og merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("a02fc602-872a-4671-b338-8b86a64b534a"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:krav-og-utlegg", Name = "Krav, betalinger og utlegg", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til krav og utlegg. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("1886712b-e077-445a-ab3f-8c8bdebccc67"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:reviorattesterer", Name = "Revisorattesterer", Description = "Denne fullmakten gir tilgang til alle tjenester som krever revisorattestering. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("74c1dcf6-a760-4065-83f0-8edc6dec5dba"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:saeravgifter", Name = "Særavgifter", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til særavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("eb9006a6-dbd5-4155-9174-91e182a56715"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:kreditt-og-oppgjoer", Name = "Kreditt- og oppgjørsordninger", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kreditt- og oppgjørsordninger. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("deb7f8ae-8427-469d-b824-937c146c0489"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_skatt_avgift_regnskap_og_toll, Urn = "urn:altinn:accesspackage:toll", Name = "Toll", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til toll og fortolling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("cf575d42-72be-4d14-b0d2-77352221df4f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:ansettelsesforhold", Name = "Ansettelsesforhold", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til ansettelsesforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("fb0aa257-e7dc-4b7b-9528-77dfb749461c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:lonn", Name = "Lønn", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til lønn og honorar. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4af2d44d-f9f8-4585-b679-7875bb1828ea"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:a-ordning", Name = "A-ordningen", Description = "Denne tilgangspakken gir fullmakter til tjenester som inngår i A-ordningen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.  OBS! Vær oppmerksompå at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("ae41bf08-c7ef-4bfe-b2b9-7bf6deb7798f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:pensjon", Name = "Pensjon", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til pensjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("08ec673d-9126-4ed0-ae75-7ceec8633c77"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:sykefravaer", Name = "Sykefravær", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykefravær. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("71a2bd47-e885-49c7-8a58-7ef4bd936f41"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_personale, Urn = "urn:altinn:accesspackage:permisjon", Name = "Permisjon", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til permisjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("5eb07bdc-5c3c-4c85-add3-5405b214b8a3"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:renovasjon", Name = "Renovasjon", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til renovasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("04c5a001-5249-4765-ae8e-58617c404223"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende", Name = "Miljørydding, miljørensing og lignende", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til miljørydding, miljørensing og lignende. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("bacc9294-56fd-457f-930e-59ee4a7a3894"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:baerekraft", Name = "Bærekraft", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til tiltak og rapportering på bærekraft. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("cfe074fa-0a66-4a4b-974a-5d1db8eb94e6"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:sikkerhet-og-internkontroll", Name = "Sikkerhet og internkontroll", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sikkerhet og internkontroll. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("a03af7d5-74b9-4f18-aead-5d47edc36be5"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:ulykke", Name = "Ulykke", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til ulykke. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("fa84bffc-ac17-40cd-af9c-61c89f92e44c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_miljo_ulykke_og_sikkerhet, Urn = "urn:altinn:accesspackage:yrkesskade", Name = "Yrkesskade", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til yrkesskade. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("91cf61ae-69ab-49d5-b51a-80591c91f255"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_post_og_arkiv, Urn = "urn:altinn:accesspackage:ordinaer-post-til-virksomheten", Name = "Ordinær post til virksomheten", Description = "Denne fullmakten gir tilgang til all mottatt post som ikke innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("bb0569a6-2268-49b5-9d38-8158b26124c3"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_post_og_arkiv, Urn = "urn:altinn:accesspackage:post-til-virksomheten-med-taushetsbelagt-innhold", Name = "Post til virksomheten med taushetsbelagt innhold", Description = "Denne fullmakten gir tilgang til all mottatt post som innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post som har taushetsbelagt innhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("06c13400-8683-4985-9802-cef13e247f24"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:generelle-helfotjenester", Name = "Generelle helfotjenester", Description = "Denne fullmakten gir tilgang til ordinære tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("2ee6e972-2423-4f3a-bd3c-d4871fcc9876"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:helfo-saerlig-kategori", Name = "Helfotjenester med personopplysninger av særlig kategori", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger av særlig kategori. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("956cfcf0-ab4c-44d8-98a2-d68c4d59321b"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet", Name = "Starte, endre og avvikle virksomhet", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å starte, endre og avvikle en virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("7dfc669b-70e4-4974-9e11-d6dca4803aaa"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:aksjer-og-eierforhold", Name = "Aksjer og eierforhold", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til aksjer og eierforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4717e6e0-1ec2-4354-b825-d9a9e2588fb1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:attester", Name = "Attester", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til attestering av virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("df13707e-5252-496a-bc00-daffeed1e4b2"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:dokumentbasert-tilsyn", Name = "Dokumentbasert tilsyn", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til dokumentbaserte tilsyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("75978efe-2437-421e-8c77-dd61925c7ba4"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:infrastruktur", Name = "Infrastruktur", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens infrastruktur. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("250568e6-ed9f-4bdc-b9e3-df08be7181ba"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:patent-varemerke-design", Name = "Patent, varemerke og design", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("d3ba7bba-195b-4b69-ae68-df20ecd57097"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:tilskudd-stotte-erstatning", Name = "Tilskudd, støtte og erstatning", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke tilskudd, støtte og erstatning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("600bf1be-61f6-423c-9a13-df93ee3214a5"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:mine-sider-kommune", Name = "Mine sider hos kommunen", Description = "Denne fullmakten gir generell tilgang til tjenester av typen “mine side” tjenester hos kommuner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("55e73960-09e5-473e-aa40-deaf97cf9bf2"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:politi-og-domstol", Name = "Politi og domstol", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog om juridiske forhold med politi og jusitsmyndigheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("ac54a5ca-16d2-4132-ae0d-e5aa8bd1ff6e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:rapportering-statistikk", Name = "Rapportering av statistikk", Description = "Denne fullmakten gir tilgang til alle pålagte rapportering av statistikk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("594a2c4b-47a1-48c6-a01c-ed44ec4c05a4"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:forskning", Name = "Forskning", Description = "Denne fullmakten gir tilgang til tjenester knyttet til forskning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c0eb20c1-2268-48f5-88c5-f26cb47a6b1f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:eksplisitt", Name = "Eksplisitt tjenestedelegering", Description = "Denne fullmakten er ikke delegerbar, og er ikke knyttet til noen roller i ENhetsregisteret. Tilgang til tjenester knyttet til denne pakken kan gis av Hovedadministrator gjennom enkeltrettighetsdelegering.", IsDelegable = false, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("5b51658e-f716-401c-9fc2-fe70bbe70f48"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_forhold_ved_virksomheten, Urn = "urn:altinn:accesspackage:folkeregister", Name = "Folkeregister", Description = "Denne tilgangspakken gir fullmakt til tjenester som en virksomhet kan ha mot folkeregister. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("27f488d7-d1f6-4aee-ae81-2bb42d62c446"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_integrasjoner, Urn = "urn:altinn:accesspackage:maskinporten-scopes", Name = "Delegerbare Maskinporten scopes", Description = "Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("66bd8101-fbd8-491b-b1f5-2f53a9476ffd"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_integrasjoner, Urn = "urn:altinn:accesspackage:maskinlesbare-hendelser", Name = "Maskinlesbare hendelser", Description = "Denne tilgangspakken gir fullmakter til å administrere tilgang til maskinlesbare hendelser. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("5dad616e-5538-4e3f-b15a-bae33f06c99f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_integrasjoner, Urn = "urn:altinn:accesspackage:maskinporten-scopes-nuf", Name = "Delegerbare Maskinporten scopes - NUF", Description = "Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring på vegne av norskregistrerte utenlandske foretak (NUF). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c5bbbc3f-605a-4dcb-a587-32124d7bb76d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:jordbruk", Name = "Jordbruk", Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til jordbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("7b8a3aaa-c8ed-4ac4-923a-335f4f9eb45a"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:dyrehold", Name = "Dyrehold", Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til dyrehold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("0bb5963e-df17-4f35-b913-3ce10a34b866"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:reindrift", Name = "Reindrift", Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til reindrift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("906aec0d-ad1f-496b-a0bb-40f81b3303cb"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:jakt-og-viltstell", Name = "Jakt og viltstell", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til jakt og viltstell. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("f7e02568-90b6-477d-8abb-44984ddeb1f9"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:skogbruk", Name = "Skogbruk", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til skogbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("9d2ec6e9-5148-4f47-9ae4-4536f6c9c1cb"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:fiske", Name = "Fiske", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til fiske. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("78c21107-7d2d-4e85-af82-47ea0e47ceca"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_jordbruk_skogbruk_jakt_fiske_og_akvakultur, Urn = "urn:altinn:accesspackage:akvakultur", Name = "Akvakultur", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til akvakultur og fiskeoppdrett. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("1e40697b-e178-4920-8fbd-af5164b4d147"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:byggesoknad", Name = "Byggesøknad", Description = "Denne tilgangspakken gir fullmakter til tjenester som ansvarlig søker/tiltakshaver trenger, for eksempel byggesøknader, direkte signerte erklæringer, nabovarsel og eiendomssak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("482eee1e-ec79-45bb-8bd0-af8459cbb9f0"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:plansak", Name = "Plansak", Description = "Denne tilgangspakken gir fullmakter til tjenester som forslagsstiller/ plankonsulent trenger, for eksempel varsel om planopppstart og høring og offentlig ettersyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("1e1e8bf9-096f-4de2-9db7-b176db55db09"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:motta-nabo-og-planvarsel", Name = "Motta nabo- og planvarsel", Description = "Denne tilgangspakken gir fullmakter til tjenester til å lese og svare på varsel om plan-/byggesak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("ac655a2f-be29-4888-9a6c-b21e524fa90e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:oppforing-bygg-anlegg", Name = "Oppføring av bygg og anlegg", Description = "Denne tilgangspakken gir fullmakter til tjenester relatert til oppføring av bygninger og annlegg unntatt plan og byggesaksbehandling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("b98ec05d-1ac5-4ced-8250-b6d75b83502b"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:kjop-og-salg-eiendom", Name = "Kjøp og salg av eiendom", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kjøp og salg av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("565a3110-3d51-4e72-ae4c-b89308e5c96e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:utleie-eiendom", Name = "Utleie av eiendom", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til utleie av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("79329c4a-d856-4491-965e-bcc4ed5a7453"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:eiendomsmegler", Name = "Eiendomsmegler", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til omsetning og drift av fast eiendom på oppdrag, som eiendomsmegling og eiendomsforvaltning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("36c245db-3207-449e-92b1-94cb0a0f3031"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:veitransport", Name = "Veitransport", Description = "Denne fullmakten gir tilgang til tjenester knyttet til person- og godstransport langs veinettet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("9cc8b401-b4cb-473d-a3fa-96171aeb1389"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:transport-i-ror", Name = "Transport i rør", Description = "Denne fullmakten gir tilgang til tjenester knyttet til transport i rør. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("9603dd83-be41-4190-b0a7-97490f4a601d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:sjofart", Name = "Sjøfart", Description = "Denne fullmakten gir tilgang til tjenester knyttet til skipsarbeidstakere og fartøy til sjøs. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("3b3bc323-207b-4d56-a21f-97b6875ccc28"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:lufttransport", Name = "Lufttransport", Description = "Denne fullmakten gir tilgang til tjenester knyttet til luftfartøy og romfartøy. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("dfcd3923-cb66-45f7-9535-999b6c0c496d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:jernbanetransport", Name = "Jernbanetransport", Description = "Denne fullmakten gir tilgang til tjenester knyttet til jernbane, inkludert trikk, T-bane og sporvogn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("86a8ff87-3a3d-4f32-a4ef-99c43494bc6e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_transport_og_lagring, Urn = "urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport", Name = "Lagring og andre tjenester tilknyttet transport", Description = "Denne fullmakten gir tilgang til tjenester knyttet til lagring og hjelpetjenester i forbindelse med transport, samt post- og kurervirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("03404270-aa2d-498f-9e6a-103043d41f1f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:kommuneoverlege", Name = "Kommuneoverlege", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er relevant for kommuneleger. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("5a642040-46cf-4466-b671-115a022e3048"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori", Name = "Helsetjenester med personopplysninger av særlig kategori", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er av særlig kategori. Denne fullmakten kan gi bruker tilgang til sensitive personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4fa0fbbc-3841-4405-9d94-12731a8fdb81"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:helsetjenester", Name = "Helsetjenester", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4d71e7b8-c6eb-4e33-9a64-1279a509a53b"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon", Name = "Pleie- og omsorgstjenester i institusjon", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til pleie og omsorgstilbud i institursjon. Dette er tjenester som tilbyr institusjonsopphold kombinert med sykepleie, tilsyn eller annen form for pleie alt etter hva som kreves av beboerne. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("70831546-6bfa-45a0-bdad-14e9db265847"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak", Name = "Sosiale omsorgstjenester uten botilbud og flyktningemottak", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sosiale omsorgstjeneser uten botilbud for eldre, funksjonshemmede og rusmisbrukere samt flykningemottak, og tjenester relatert til arbeidstrening og andre sosiale tjenester, f eks i regi av velferdsorganisasjoner. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("98c404f4-5350-42cd-86d0-15fd38f178c4"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:familievern", Name = "Familievern", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til familievern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("83ff0734-0de5-4c2a-939b-18a9452c00bc"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_helse_pleie_omsorg_og_vern, Urn = "urn:altinn:accesspackage:barnevern", Name = "Barnevern", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til barnevern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("8cf4ec08-90dc-47d0-93f8-64a50c9b38b0"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:godkjenning-av-personell", Name = "Godkjenning av personell", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning til enkeltpersoner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("b56d2614-3f9d-4d93-8a8e-64d80b654ad7"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet", Name = "Godkjenning av utdanningsvirksomhet", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning av utdanningsvirksomheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("93f37a3d-f799-47b3-bc8e-675b5813abf4"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning", Name = "Høyere utdanning og høyere yrkesfaglig utdanning", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til høyere utdanning og høyere yrkesfaglig utdanning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("219ea5f7-eb30-424f-a958-67d3c7e7a4c2"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:sfo-leder", Name = "SFO-leder", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til førskole og fritidsordning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("4cd69693-aff0-4e88-8b64-6b5620672468"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:ppt-leder", Name = "PPT-leder", Description = "Denne fullmakten gir tilgang til tjenester innen drift av Pedagogisk-psykologisk tjeneste (PPT) som PPT-leder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("40397a93-b047-4011-a6b8-6b8af16b6324"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:opplaeringskontorleder", Name = "Opplæringskontorleder", Description = "Denne fullmakten gir tilgang til tjenester innen drift av opplæringskontor som opplæringskontorleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("499d0e1c-18d7-4f21-b110-723e3d13003a"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:skoleleder", Name = "Skoleleder", Description = "Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("32bfd131-1570-4b0c-888b-733cbb72d0cb"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:skoleeier", Name = "Skoleeier", Description = "Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c1242df4-6af1-4e0b-b022-73c1099f5297"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:statsforvalter-skole-og-opplearing", Name = "Statsforvalter - skole og opplæring", Description = "Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til skole- og opplæringssektor, herunder fagopplæring og voksenopplæring.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("7e6d8dd4-35a3-49d6-a2c5-73cd35018646"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:statsforvalter-barnehage", Name = "Statsforvalter - barnehage", Description = "Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til barnehagesektor.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("0ecc481a-691c-496d-a3f2-748b8c450ed9"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:barnehagemyndighet", Name = "Barnehagemyndighet", Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehagemyndighet er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("dcb57f3e-0e5b-4ef7-a10c-74c53bc5a90d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:barnehageleder", Name = "Barnehageleder", Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("9a14f2c1-f0bc-4965-97c0-76a4e191bbe1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_oppvekst_og_utdanning, Urn = "urn:altinn:accesspackage:barnehageeier", Name = "Barnehageeier", Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("f56bdb15-686b-46f8-9343-bde5d7c17648"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere", Name = "Elektrisitet - produsere, overføre og distribuere", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til elektrisitet: produsere, overføre og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("27afe398-b25b-4287-b0fa-c1d03d6c9fa9"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:damp-varmtvann", Name = "Damp- og varmtvann", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til damp- og varmtvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("e7cda008-f265-4452-b03c-c21b8b51dfe1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:vann-kilde-rense-distrubere", Name = "Vann - ta ut fra kilde, rense og distribuere", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til vann: ta ut fra kilde, rense og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("fef4aac0-d227-4ef6-834b-cc2eb4b942ed"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:samle-behandle-avlopsvann", Name = "Samle opp og behandle avløpsvann", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til samle opp og behandle avløpsvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("6dcffdde-cc5c-4b57-a5a2-cc5ae6ad44fc"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:avfall-behandle-gjenvinne", Name = "Avfall - samle inn, behandle, bruke og gjenvinne", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til avfall: samle inn, behandle bruke og gjenvinne. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("ab656cf2-1e65-4b5a-a1e3-cd5bd4cb804c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:miljorydding-rensing", Name = "Miljørydding - rensing og lignende virksomhet", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til miljøryddng, -rensing og lignende virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("452bd15d-2cd2-4279-9470-cd97ba8ef1c7"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_energi_vann_avlop_og_avfall, Urn = "urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull", Name = "Utvinning av råolje, naturgass og kull", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til utvinning av råolje, naturgass og kull. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("65c7bbe7-aeb4-4f18-b0b0-1b1b83bd24d1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk", Name = "Næringsmidler, drikkevarer og tobakk", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med næringsmidler, drikkevarer og tobakk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c6231d60-5373-4179-b98b-1e7eb83da474"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:tekstiler-klaer-laervarer", Name = "Tekstiler, klær og lærvarer", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med tekstiler, klær og lærvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("deb1618f-395c-4a14-9a70-20e90e5f9a76"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:trelast-trevarer-papirvarer", Name = "Trelast, trevarer og papirvarer", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trelast, trevarer og papirvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("965f9aca-48f5-4a16-b5e3-228806ad4fa7"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:trykkerier-reproduksjon-opptak", Name = "Trykkerier og reproduksjon av innspilte opptak", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trykkerier og reproduksjon av innspilte opptak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("79203df9-d71b-460b-a828-22b0bf79f335"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri", Name = "Oljeraffinering, kjemisk og farmasøytisk industri", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med oljeraffinering, kjemisk og farmasøytisk industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("573be5d2-3ddd-4862-9711-2350147a1b25"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter", Name = "Gummi, plast og ikke-metallholdige mineralprodukter", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med gummi, plast og ikke-metallholdige mineralprodukter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("84e744a0-e93f-4caf-bb3f-24387f045d2d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:metaller-og-mineraler", Name = "Metaller og mineraler", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metaller og mineraler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("2857fd90-dad2-4cc3-9947-282c22f5d2dc"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner", Name = "Metallvarer elektrisk utstyr og maskiner", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metallvarer, elektrisk utstyr og maskiner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c37aabdf-a78a-4f59-999a-298a83e9e113"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:verft-og-andre-transportmidler", Name = "Verft og andre transportmidler", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med verft og andre transportmidler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("07bfd8a5-5b13-4937-84b9-2bd6ac726ea1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:mobler-og-annen-industri", Name = "Møbler og annen industri", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med møbler og annen industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("ec492675-3a48-4ad9-b864-2d6865020642"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr", Name = "Reparasjon og installasjon av maskiner og utstyr", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med reparasjon og installasjon av maskiner og utstyr. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("8c700369-8fff-40f1-b4ce-2f116416d804"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_industrier, Urn = "urn:altinn:accesspackage:bergverk", Name = "Bergverk", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med bergverk og tilhørende tjenester til bergverksdrift og utvinning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("75fa5863-3368-4ac6-9a4b-48f595e483ad"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:kunst-og-underholdning", Name = "Kunst og underholdning", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kunstnerisk og underholdningsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("73418c0c-d4db-4e26-8581-4ccf1384aad7"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur", Name = "Biblioteker, museer, arkiver og annen kultur", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til biblioteker, museer, arkiver, og annen kultur som botaniske og zoologiske hager, og drift av naturfenomener av historisk, kulturell eller undervisningsmessig interesse (f.eks. verdenskulturarv mv.). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("cc5ccdec-6c67-4462-ab51-4d5eaafd64c1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:lotteri-og-spill", Name = "Lotteri og spill", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til lotteri og spill, som f eks kasinoer, bingohaller og videospillhaller samt spillevirksomhet som f.eks. lotterier og veddemål som inngås utenfor banen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("d2d00311-cc33-47ad-b33b-4eb15cce8d1d"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:sport-og-fritid", Name = "Sport og fritid", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sports- og fritidsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("c82ab275-dad6-461a-bf0c-50be46b25ec9"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:fornoyelser", Name = "Fornøyelser", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til drift av fornøyelsesetablissementer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("f05153b5-4784-46f0-805a-525ed31fde3b"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_kultur_og_frivillighet, Urn = "urn:altinn:accesspackage:politikk", Name = "Politikk", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til aktiviteter i forbindelse med politisk arbeid. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("a6d704b7-d56b-4517-be79-0acd5b55b35e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_handel_overnatting_og_servering, Urn = "urn:altinn:accesspackage:varehandel", Name = "Varehandel", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til varehandel, inkludert engros- og detaljhandel, import og eksport, og salg og reparasjon av motorvogner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("77cdd23a-dddf-43e6-b5c2-0d8299b0888c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_handel_overnatting_og_servering, Urn = "urn:altinn:accesspackage:overnatting", Name = "Overnatting", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til overnattingsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("6710a7a0-78f9-47e3-bfa6-0d875869befd"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_handel_overnatting_og_servering, Urn = "urn:altinn:accesspackage:servering", Name = "Servering", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til serveringsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("a736c33b-c15a-43ac-85be-a684630e1e59"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_andre_tjenesteytende_naeringer, Urn = "urn:altinn:accesspackage:post-og-telekommunikasjon", Name = "Post- og telekommunikasjon", Description = "Denne fullmakten gir tilgang til tjenester knyttet til post og telekommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("413d79fb-a419-4e74-98f7-aa91389deb81"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_andre_tjenesteytende_naeringer, Urn = "urn:altinn:accesspackage:informasjon-og-kommunikasjon", Name = "Informasjon og kommunikasjon", Description = "Denne fullmakten gir tilgang til tjenester knyttet til informasjon og kommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("8f977b5f-a2f9-4712-88e2-ab1a51a6b26f"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_andre_tjenesteytende_naeringer, Urn = "urn:altinn:accesspackage:finansiering-og-forsikring", Name = "Finansiering og forsikring", Description = "Denne fullmakten gir tilgang til tjenester knyttet til finansiering og forsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("a8b18888-2216-4eaa-972d-ae96da04b1ac"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_andre_tjenesteytende_naeringer, Urn = "urn:altinn:accesspackage:annen-tjenesteyting", Name = "Annen tjenesteyting", Description = "Denne fullmakten gir tilgang til tjenester knyttet til annen tjenesteyting som f eks organisasjoner og foreninger, reparasjon av datamaskiner og varer til personlig bruk og husholdningsbruk og en rekke personlige tjenester som ikke er nevnt annet sted. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("955d5779-3e2b-4098-b11d-0431dc41ddbe"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_regnskapsforer, Urn = "urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet", Name = "Regnskapsfører med signeringsrettighet", Description = "Denne fullmakten gir tilgang til regnskapfører å kunne signere på vegne av kunden for alle tjenester som krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("a5f7f72a-9b89-445d-85bb-06f678a3d4d1"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_regnskapsforer, Urn = "urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet", Name = "Regnskapsfører uten signeringsrettighet", Description = "Denne fullmakten gir tilgang til å kunne utføre alle tjenester som ikke krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("43becc6a-8c6c-4e9e-bb2f-08fe588ada21"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_regnskapsforer, Urn = "urn:altinn:accesspackage:regnskapsforer-lonn", Name = "Regnskapsfører lønn", Description = "Denne fullmakten gir tilgang til regnskapsfører å rapportere lønn for sin kunde. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("2f176732-b1e9-449b-9918-090d1fa986f6"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_revisor, Urn = "urn:altinn:accesspackage:ansvarlig-revisor", Name = "Ansvarlig revisor", Description = "Denne fullmakten gir revisor tilgang til å opptre som ansvarlig revisor for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("96120c32-389d-46eb-8212-0a6540540c25"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_revisor, Urn = "urn:altinn:accesspackage:revisormedarbeider", Name = "Revisormedarbeider", Description = "Denne fullmakten gir revisor tilgang til å opptre som revisormedarbeider for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
            new Package() { Id = Guid.Parse("5ef836c7-69cc-4ea8-84d6-fb933cc4fc5c"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_konkursbo, Urn = "urn:altinn:accesspackage:konkursbo-lesetilgang", Name = "Konkursbo lesetilgang", Description = "Denne fullmakten delegeres til kreditorer og andre som skal ha lesetilgang til det enkelte konkursbo.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("0e219609-02c6-44e6-9c80-fe2c1997940e"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_konkursbo, Urn = "urn:altinn:accesspackage:konkursbo-skrivetilgang", Name = "Konkursbo skrivetilgang", Description = "Denne fullmakten gir bostyrers medhjelper tilgang til å jobbe på vegne av bostyrer. Bostyrer delegerer denne fullmakten sammen med Konkursbo lesetilgang til medhjelper for hvert konkursbo.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("0195efb8-7c80-7642-b9b8-c748ee4fecd4"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_bygg_anlegg_og_eiendom, Urn = "urn:altinn:accesspackage:tinglysing-eiendom", Name = "Tinglysing eiendom", Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til elektronisk tinglysing av rettigheter i eiendom.", IsDelegable = true, HasResources = true, IsAssignable = true },
            new Package() { Id = Guid.Parse("0195efb8-7c80-7cf2-bcc8-720a3fb39d44"), ProviderId = provider, EntityTypeId = orgEntityType, AreaId = area_fullmakter_for_forretningsforer, Urn = "urn:altinn:accesspackage:forretningsforer-eiendom", Name = "Forretningsforer eiendom", Description = "Denne fullmakten gir forretningsfører for Borettslag og Eierseksjonssameie tilgang til å opptre på vegne av kunde, og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en forretningsfører utfører på vegne av sin kunde. Fullmakt hos forretningsfører oppstår når Borettslaget eller Eierseksjonssameiet registrerer forretningsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.", IsDelegable = true, HasResources = true, IsAssignable = false },
        };

        await ingestService.IngestAndMergeData(packages, options: options, null, cancellationToken);
    }

    /// <summary>
    /// Ingest all static rolepackage data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestRolePackage(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var packages = new Dictionary<string, Guid>();
        foreach (var pack in await packageService.Get())
        {
            packages.Add(pack.Urn, pack.Id);
        }

        var roles = new Dictionary<string, Guid>();
        foreach (var role in await roleService.Get())
        {
            roles.Add(role.Urn, role.Id);
        }

        var variants = new Dictionary<string, Guid>();
        foreach (var variant in await entityVariantService.Get())
        {
            variants.Add(variant.Name, variant.Id);
        }

        var rolePackages = new List<RolePackage>()
        {
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"], PackageId = packages["urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"], PackageId = packages["urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"], PackageId = packages["urn:altinn:accesspackage:regnskapsforer-lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:revisor"], PackageId = packages["urn:altinn:accesspackage:ansvarlig-revisor"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:revisor"], PackageId = packages["urn:altinn:accesspackage:revisormedarbeider"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:konkursbo-lesetilgang"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:konkursbo-skrivetilgang"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:ordinaer-post-til-virksomheten"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:veitransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:transport-i-ror"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sjofart"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:lufttransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:jernbanetransport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:skatt-naering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:skattegrunnlag"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:merverdiavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:motorvognavgift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:saeravgifter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:krav-og-utlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:kreditt-og-oppgjoer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:regnskap-okonomi-rapport"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:reviorattesterer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:toll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:ansettelsesforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:lonn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:pensjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:permisjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sykefravaer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:a-ordning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:barnehageeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:barnehageleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:barnehagemyndighet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-barnehage"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:statsforvalter-skole-og-opplearing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:skoleeier"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:skoleleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:opplaeringskontorleder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:ppt-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sfo-leder"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-personell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:renovasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:baerekraft"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sikkerhet-og-internkontroll"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:ulykke"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:yrkesskade"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:kunst-og-underholdning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:lotteri-og-spill"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sport-og-fritid"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:fornoyelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:politikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:jordbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:dyrehold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:reindrift"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:jakt-og-viltstell"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:skogbruk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:fiske"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:akvakultur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:kontaktperson-nuf"], PackageId = packages["urn:altinn:accesspackage:maskinporten-scopes-nuf"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:maskinlesbare-hendelser"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:tekstiler-klaer-laervarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:trelast-trevarer-papirvarer"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:trykkerier-reproduksjon-opptak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:metaller-og-mineraler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:verft-og-andre-transportmidler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:mobler-og-annen-industri"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:bergverk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:kommuneoverlege"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:helsetjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:barnevern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:familievern"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:varehandel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:overnatting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:servering"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:generelle-helfotjenester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:helfo-saerlig-kategori"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:aksjer-og-eierforhold"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:attester"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:dokumentbasert-tilsyn"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:infrastruktur"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:patent-varemerke-design"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:tilskudd-stotte-erstatning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:mine-sider-kommune"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:politi-og-domstol"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:rapportering-statistikk"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:forskning"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:damp-varmtvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:vann-kilde-rense-distrubere"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:samle-behandle-avlopsvann"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:avfall-behandle-gjenvinne"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:miljorydding-rensing"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:post-og-telekommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:informasjon-og-kommunikasjon"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:finansiering-og-forsikring"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:annen-tjenesteyting"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:byggesoknad"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:plansak"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:motta-nabo-og-planvarsel"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:oppforing-bygg-anlegg"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:kjop-og-salg-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:utleie-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:eiendomsmegler"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bostyrer"], PackageId = packages["urn:altinn:accesspackage:folkeregister"], EntityVariantId = null, CanDelegate = true, HasAccess = true },

            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:styreleder"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:innehaver"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:komplementar"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"], PackageId = packages["urn:altinn:accesspackage:tinglysing-eiendom"], EntityVariantId = null, CanDelegate = true, HasAccess = true },

            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"], PackageId = packages["urn:altinn:accesspackage:forretningsforer-eiendom"], EntityVariantId = variants["ESEK"], CanDelegate = true, HasAccess = true },
            new RolePackage() { RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"], PackageId = packages["urn:altinn:accesspackage:forretningsforer-eiendom"], EntityVariantId = variants["BRL"], CanDelegate = true, HasAccess = true },

            new RolePackage() { RoleId = roles["urn:altinn:role:hovedadministrator"], PackageId = packages["urn:altinn:accesspackage:post-til-virksomheten-med-taushetsbelagt-innhold"], EntityVariantId = null, CanDelegate = true, HasAccess = false },
            new RolePackage() { RoleId = roles["urn:altinn:role:hovedadministrator"], PackageId = packages["urn:altinn:accesspackage:eksplisitt"], EntityVariantId = null, CanDelegate = true, HasAccess = false },
        };

        await ingestService.IngestAndMergeData(rolePackages, options: options, matchColumns: ["RoleId", "PackageId", "EntityVariantId"], cancellationToken);
    }

    /// <summary>
    /// Ingest all static variantrole data
    /// </summary>
    /// <param name="options">ChangeRequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestEntityVariantRole(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var roles = new Dictionary<string, Guid>();
        foreach (var role in await roleService.Get())
        {
            roles.Add(role.Urn, role.Id);
        }

        var variants = new Dictionary<string, Guid>();
        foreach (var variant in await entityVariantService.Get())
        {
            variants.Add(variant.Name, variant.Id);
        }

        Console.WriteLine("VariantRoles");

        var variantRoles = new List<EntityVariantRole>()
        {
            new EntityVariantRole() { Id = Guid.Parse("7f677a6d-295f-4b7a-bef9-070ddd6525d7"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1624a50c-5ca3-480b-9128-29703d2b2038"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("8314429c-b887-4e83-bccc-8a563d40fbb2"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("4d9f0774-fb2c-487a-ab28-aa85b56fd41e"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson-ados"] },
            new EntityVariantRole() { Id = Guid.Parse("a6b0a73d-d605-4839-b0e1-1a8fde14dc51"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("4cf85a10-696a-49e3-84c3-26e9d2068725"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("173f33ca-8f76-4592-ba21-c2e533ee175e"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("bdc4ccd6-02c8-46a5-adaa-ef9a15bd1899"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("c3d28a99-9c52-44a2-ac76-efae6d31a80e"), VariantId = variants["ADOS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("652c872e-a1de-4020-8834-8a0bbb4dd03d"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("3a6e064e-a4a9-41e5-afc1-fc93dda29f97"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("0a071f08-389b-445a-b7b7-3c1bc57ff4ff"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("8939e9bb-35f3-4fa0-bd48-6299831a4fb6"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("acdcaf38-cd81-4de8-b615-30ba751cb920"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("8afc00c9-2bff-483a-84a4-60f15d555c09"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("84ef38f9-6e12-45e8-91d3-41d43155b338"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("aa494bba-abfa-48d4-b247-c93232679470"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("94749862-e9e1-41c5-bf82-deb94f066b4f"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("124d6d13-7c08-466d-a246-b4ac581da775"), VariantId = variants["ANNA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("6fd44a23-5c62-47fe-9c39-ff6695aac005"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("84327709-7420-4e48-b228-2ba4123f2df5"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("9862e082-5b39-42d6-998b-072df283b219"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("72eacd4c-9eaf-47a8-8bcb-571495ba27b2"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("2d7cce8a-04aa-4287-859c-54adef097701"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("819cb300-90d7-45db-bacc-545349bf28b3"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("d0bb506c-58f4-4e78-84e3-ff1539c341cc"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("4bfd5fe7-33c5-451a-b8fa-dd7951614b3a"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:deltaker-fullt-ansvar"] },
            new EntityVariantRole() { Id = Guid.Parse("f98a63e7-53f2-4831-bd35-9c15b98482dd"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("380614a6-831c-4dec-a9a1-688e813d5902"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("52ba12cd-47d7-4340-8fcf-d8832187cd71"), VariantId = variants["ANS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("1d3cd64c-f060-4719-95fd-3a4a6d7fa3a1"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("95fe3ed8-13b6-4129-99f6-f950d0b68683"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("70324f68-e677-4d5e-abdb-87a2e51a58eb"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("9a372655-7ccd-4e2e-8c8c-af4cd564f9ac"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("766a8fff-dc73-4acd-9d44-a378b848dc9e"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("5bd8527b-8d7a-41b9-8ec6-de57f552c7b0"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("caacbd5a-acc8-48da-82a2-badd60f2eb60"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("12470c12-e190-4ae0-b9cc-f38153a80858"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("a637f1f2-bf6e-46a1-a8d1-f34edfb8e343"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("1c911d5b-b1de-4884-96d4-5c07d312bb45"), VariantId = variants["AS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("9ec86a6e-11fd-435f-a57b-7a4442b98acb"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("7be38437-0491-4733-9eeb-d1565463fdce"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("f6b188de-14cf-4e05-b171-654021eebf74"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("4239583e-0331-4bb6-9422-6cd5243688b6"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("8c3ad8f2-e064-42f3-b814-fc39e3ce50cc"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("52708ecf-40cd-4243-a531-db5163d9cefb"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("07a21de2-9667-4642-9b0e-756b6e411259"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("d5fb7b75-ba30-4beb-9359-35fca6c73486"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("ebd639ff-08f6-484f-90c5-5c99ea8dd10c"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("3726a2ae-4e1a-4a19-9d29-5c3af4329407"), VariantId = variants["ASA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("aaae639f-64fd-496e-b50d-7aecd8ffcf93"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("c8dc4c49-ee86-4c09-b123-4c3e891f56e3"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("43b64020-dfd9-4f7d-b26c-e0f24312ee4f"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("5af0d3b1-6d37-4566-ab0e-b2c5c07d57fc"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("472676e2-f56c-4d3a-8931-4a90ebb1c32c"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("56b8fb03-9bea-4b2d-bcdf-4b34da736917"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("5df3d170-ca31-4809-96d6-523973bbe23d"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("98f88e4c-ddd0-4a80-a377-bb4f84f4d2f4"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5874bf80-b5bb-4049-a788-012d665721d8"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("99221674-66a2-4bb6-a93d-8d9ddcb630c0"), VariantId = variants["BA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("75c8aa69-81df-4c38-bf52-24335db22293"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("c9c56ba5-7a37-4cc1-a1df-537a9d4a42d2"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("0cc812d3-a869-42a8-bd52-011f5eb8780a"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("ae49835a-6012-46c5-919e-70a233b8d7ce"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("6df34034-58b5-4235-a084-a4fcb1ce2321"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("62d8e7b5-8b6d-4867-af4d-97e7f9a5a4a7"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("2ad1f891-4846-4574-ab4d-cccbe5b31d9c"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("110dc50e-0c24-4769-b6b9-b0b4ead1eadc"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("9df80469-b333-430e-a501-838f9966439e"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("84caf74a-0646-4ffa-a170-dd295f270156"), VariantId = variants["BBL"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("7a4f593b-1002-4d33-b90a-95385ba1b815"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("a3338c5e-a65f-4b0f-9e67-12fecc405915"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("980fbabe-2518-43d7-ab47-e6f02673be97"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("eadc8e91-5083-406c-af9e-200035b66197"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("ec19eaad-6b51-463e-9f80-326ead70c26f"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("5f0f462a-986b-4a1a-b988-91c9751a87b1"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("4a9d73de-45f5-4ba5-a534-a7b0ef56286b"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("64523172-39fd-4651-b47e-560fdc2e9988"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("ae84a673-d0f3-4c84-9a11-a255dd8a4085"), VariantId = variants["BO"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("2eb828d8-f347-4e8b-b09c-13705c8c2e0e"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("1b22445f-1c10-47ea-bd3f-f662058a2b0e"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("8a3ce252-fe03-4077-b222-feaf3680cc67"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("141dfc6a-dff3-4f78-bb47-0ea275a09e61"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("f66fec3a-296c-43c2-ac6d-568d08d3b895"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("91d2f5ec-5792-4860-a7ba-24fa10464843"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("d898e8de-9d89-419a-8478-49ffa44ec888"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("10125ffb-3b9e-4df8-a2b7-7594c9767780"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("1b3fa2c2-4c85-4f75-acf2-868fdac47b61"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("81c782ea-ec2b-4efc-a76e-f9715c5f5af3"), VariantId = variants["BRL"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("aca50f25-4d2a-4cba-9f84-63df232d4bde"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("c2852f85-7053-4113-bcb0-21240999891c"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("a602b705-6824-44ed-a003-004ee7845fd3"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("9533af22-de5e-4070-a256-deec95da997f"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"] },
            new EntityVariantRole() { Id = Guid.Parse("bc3ab3ba-1d7c-42b9-a98f-a5f64b37adbb"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("6305f07b-a7d0-4593-88c7-a934d9ac73a8"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("17dec074-f0fe-4822-a5e7-e1221b316e63"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("23b98c90-4a96-4933-8a93-8f494b2cc6b4"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("ff51aae9-4a13-42c3-900c-95fd8b13c5c1"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("9183902d-8a1a-46ed-a02b-a81729cc146a"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("baf63be2-a091-4621-8adb-d69141c0e331"), VariantId = variants["DA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("cdecb843-9b8a-4d88-af52-fd0c76554213"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("e1286b53-740e-4303-8c9e-1e10c68aced3"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("d5677001-181a-4806-9a92-77273ba954fb"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:innehaver"] },
            new EntityVariantRole() { Id = Guid.Parse("14ef8844-c43a-480e-8959-704b898b824c"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("f5e09d28-f25a-4055-aba0-1a263dfe772b"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("2f813d2a-a203-40ac-88f5-5f488b12adf9"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("634db16e-3f8e-4845-ac41-e97bd62ed049"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("3480f2f6-a7c9-4bdd-8d0b-f3586b4c4773"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("8b478e3f-132e-4e3c-9f14-4164fd8ccd18"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("cafeea2b-9743-4907-aaee-f699b433a3e2"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("02c21949-d94f-4963-a684-992907dff320"), VariantId = variants["ENK"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("e90bf748-13eb-41fe-b8de-442d9dff0d5c"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("c2f67257-43eb-44dc-b3f8-40a1e3a372b2"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("f3d2352c-cb9e-46a0-b7f0-1417849d1311"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5a2cc156-aa89-4984-9843-6bdef16c5696"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("7a343a25-f454-4392-84fb-246f813178dc"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("e7cc1167-7984-48fc-8911-ed738db1a18a"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("d9d7ea20-8cb0-4af9-ba7f-d1083ea0188e"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("1f0a3f0c-c258-4fc8-8b16-6a91397c9c52"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("00ede93c-d586-4abc-b019-2150598d42c6"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("d7b6b26b-290a-4b58-8a23-241224eb1535"), VariantId = variants["EOFG"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("607aee39-c29f-4950-a772-df8ebb995c18"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("b5c0512e-5d71-493a-b09f-16c814856b31"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("bb1ae4f8-09aa-41a7-b1f0-e5dc9e1c1d4a"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("c921f4d9-d393-41ca-be21-1c9b761fc421"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("c958da45-3f25-4e2a-adca-c12e730183ae"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1106d839-5344-419c-8739-b5f7400ccc96"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("983f1257-5ed6-48ea-a4a0-50a46ca07228"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("c13119b7-e17f-4cac-92a8-66fe8dbcbb3f"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("3403dfa7-ad72-4b92-976b-897815a2bad3"), VariantId = variants["ESEK"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("e8f35322-84c7-48a9-a487-44761e467df3"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("d3f6a227-aa41-4e28-97ea-0eaca51b6e61"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("0a30b3f3-ad9e-4015-bb3c-a842abe6ff87"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("5c254060-311b-4e6d-9f77-f19456b3ac57"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("3b859a17-7582-458c-bb41-3ed81d7c716f"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("0d9ba2e7-472a-4b56-b736-9639c3b358f4"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("43cddf4e-509e-4c9f-be01-803e6b801a83"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("aac5f1ad-aa7e-44d0-90ad-84c76910fda5"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:eierkommune"] },
            new EntityVariantRole() { Id = Guid.Parse("456099ca-7ff5-47e4-b641-bc9d5174b7bd"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("fa21390e-2ee2-4ebd-9178-a10b576ce05f"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("ff0cf936-61ce-41f9-915a-2546f0245ed6"), VariantId = variants["FKF"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("94ec68e4-2b81-4ef8-9f82-85d3857f9a4b"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("94225083-9c0d-43f0-9484-0580956da009"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("df02d99e-a7d4-4ec2-b8cb-9eef2c6ca934"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("c4a1075b-531f-41c7-a47e-9a3072a9245c"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("9b130560-451d-4b19-8ffa-b9fd8b378f3e"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("1eb73cd2-4595-4d32-be2a-91a1dbbd2031"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("ece6ae97-2952-436d-988b-a2981fed6f80"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("9ed87363-113f-47ec-b287-04a00b5565c2"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("91a6286c-7ae5-42c2-b0cd-fc6b45ad4caf"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("59a1024c-bd02-42a3-bab9-3628620446b4"), VariantId = variants["FLI"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("40ab7bdc-47ee-4e53-a2a6-4a9c5c082aa7"), VariantId = variants["FYLK"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("e37b5c34-c4e2-47f6-844d-9232796f95e1"), VariantId = variants["FYLK"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("9072ac91-eb21-4b26-be54-29b69b232a9d"), VariantId = variants["FYLK"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1ed68071-a0ef-4a4f-848a-5c90ad18d1ba"), VariantId = variants["FYLK"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("62a184ed-95f3-4b29-8c5e-5feab8a0e2b9"), VariantId = variants["FYLK"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("6ff7668c-758b-44a3-953e-8ecd51698666"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("d851b5fc-43aa-4354-b909-afb9ffbe64eb"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("25f1b5e9-234c-421a-bba0-388147b93043"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("098d394e-2faf-4995-b27d-da91152084cd"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("48aa9279-e5d4-4662-9544-671584fd5723"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("83393c94-2dcd-4d51-b125-21b244053675"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("24ab35fe-0b59-4a7d-bb86-7ec1d861a12b"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("98a9b7e6-5b7c-4053-86d2-764f29d38bb9"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("7d0d9734-5815-420f-aaa0-7c4675190b04"), VariantId = variants["GFS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("b0059b9b-7d1f-43db-8e5a-8b4f9bda9d19"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("82c239fc-efd8-4167-99af-7c993309a4cc"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5fd06149-5e62-4965-96f8-3afc2508126e"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("e4b26c29-241e-488b-b4e4-7f29be48cfb4"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("5630a5a9-7545-420b-bf3f-2353e6971472"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:deltaker-delt-ansvar"] },
            new EntityVariantRole() { Id = Guid.Parse("452362ae-4c2c-4f57-a2fa-0c3cace5a3ac"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("d854d6cf-c0af-4048-b831-088b70d89c20"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("dc7e38f3-b2aa-4a2d-b8ba-cb896ece02c6"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("9b94d2df-2d86-4e0d-a5e5-b24940d3f08b"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("cf153441-443e-4622-bd14-3f6b36cceaa7"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("910aed15-6cc4-45cb-a23b-7a8e9c3c2df0"), VariantId = variants["IKS"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("342bf0d7-8e9d-409b-acb9-9742148a67da"), VariantId = variants["KBO"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("7d3bb310-5a6c-409e-8582-e9960858fc23"), VariantId = variants["KBO"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("ea5975c8-9c62-43a2-a23d-39e19a2d5877"), VariantId = variants["KBO"], RoleId = roles["urn:altinn:external-role:ccr:bostyrer"] },
            new EntityVariantRole() { Id = Guid.Parse("60c14788-6001-4a0f-95b6-9712cd817a88"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("32afb670-0487-4f5e-9816-0bd4b9dbe286"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("93213862-8607-474e-9820-1170477b98f7"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("a05e26d4-5c8c-44b5-a059-187f6584d078"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("d8b22a93-749a-4a74-8354-331f4b7d42ad"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:eierkommune"] },
            new EntityVariantRole() { Id = Guid.Parse("f014d8a4-5e80-4a7a-a508-351e98cd6b73"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("b441c5fe-0c7a-4c12-98c1-370db33a1a6d"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("f31b6012-d100-479c-b3fa-6bcb8f0beead"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("50ecf708-dc98-4515-b913-8e1d668b03ef"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("efd38f56-907c-4e76-9b3d-a59090d05acd"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("4ea69e8a-8d54-474b-9630-adda4b012862"), VariantId = variants["KF"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("bbb89d3d-b6ba-459c-8479-05cc1e842ea0"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("85ac1dd8-64e2-41d3-a192-8d625cffd589"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("22bedc49-8a2a-4ea4-adaf-3e4af232077d"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("551af393-175e-445d-853e-2709cdc22d9f"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("434f59c8-dea2-46ba-8bd4-cfa79c443b8d"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:kirkelig-fellesraad"] },
            new EntityVariantRole() { Id = Guid.Parse("0bd3e5ea-af73-4e4f-b898-f7158c612373"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("fd8c5446-d143-4241-814b-436a4ee30e37"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("965b9833-b73e-4f69-902e-648878c26b9a"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("aa1308d7-d17d-4012-b07a-97eb2eb2088f"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("b314abfd-b770-41f9-94aa-a4b23c147075"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("8b39c0ae-7179-44fa-b557-4ad7fc9470ac"), VariantId = variants["KIRK"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("8af0df9a-e07b-4750-9ef4-157c1df01402"), VariantId = variants["KOMM"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("cd0467eb-7d67-4c83-8d0d-6d6d32a3220a"), VariantId = variants["KOMM"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1f6b0652-cab1-4992-812f-2b7d2de99070"), VariantId = variants["KOMM"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("62bd7bb2-5637-4695-a95a-304389464641"), VariantId = variants["KOMM"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("904404f0-1517-46c8-95aa-581f2ecd68eb"), VariantId = variants["KOMM"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("4b169d67-7472-4120-be0c-57d2e233757a"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("ee0005ed-a654-4002-b4db-61da3067f7eb"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("4095f4d7-19ae-4b93-a2bd-5f84bab7f23b"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("65543295-6d0d-4b79-b845-f0483abb0dc3"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("66484ae9-fe50-4649-b227-be5561fc60d0"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("193bda2c-e40c-4d57-8e50-83f020bd3d67"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("59b45424-baee-4a2b-a20f-2817fa91fcbd"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("fe2a37aa-ea40-4344-8662-73139fd43f89"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("b88f1346-78d0-46e8-abc4-c55730254405"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("f9f00551-c25e-4f00-b2d5-00b19dc088cc"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("84c4f117-96ac-49dc-8656-7ad145d2917c"), VariantId = variants["KS"], RoleId = roles["urn:altinn:external-role:ccr:komplementar"] },
            new EntityVariantRole() { Id = Guid.Parse("12899373-01d3-4692-8496-5106641f25e1"), VariantId = variants["KTRF"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("1718ace4-1211-44ab-b6fc-8b8428cd7d6d"), VariantId = variants["KTRF"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("eba7e829-d459-4991-8e06-693928afb71e"), VariantId = variants["KTRF"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1d9c9949-b635-4060-aeea-fdba35a3fc3c"), VariantId = variants["KTRF"], RoleId = roles["urn:altinn:external-role:ccr:kontorfelleskapmedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("ee80c35d-92c6-4845-b492-c9215de78f17"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("0ec258f0-f5e2-4fa3-b841-6ef2453cd3e8"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:norsk-representant"] },
            new EntityVariantRole() { Id = Guid.Parse("15279523-371b-4ba3-b084-f9d7bc91ae9c"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("a73cf154-ad85-4425-b462-861fb8789af4"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("e650306f-4bfa-4d18-84d6-87069a3f0833"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("fca451e0-b859-4152-a8d0-1c0832334484"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("f941ec03-f76c-4f2b-823d-df47323aa2cd"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("e2ebd0fa-af89-46c4-babb-df301dca3909"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("bcd0da6b-a8a1-4322-af8c-3798c2fa41d2"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("2295a4ac-a3fe-4ab2-97d3-b92b70656f95"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("35a059db-5b4c-4ecf-801a-327eb301ada4"), VariantId = variants["NUF"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("5d7cb608-bba1-4bd2-9d0c-10ac498d745f"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("68f05fc2-3d47-4adf-aaf0-2424dd51d324"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("d79db533-a2ea-4c37-a69a-730ad3945b8a"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("f8296d6d-6488-4283-baa1-198015fe0081"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet"] },
            new EntityVariantRole() { Id = Guid.Parse("2569d806-a489-4d20-949a-56da26ffef73"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("6e6dea46-68c7-45ae-a4cf-02632e4e926a"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("634b9934-c7bf-4ed9-b7f8-6ba6366757ed"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("3f8f731f-a16c-4721-a92d-a9757043bfb6"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("fc803efd-80ac-4151-9413-7add63d2b0e5"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("693a7a06-0a73-4457-8270-09d9fedbfbdc"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5de175fd-3c65-4dee-99d6-38a883ca7d28"), VariantId = variants["OPMV"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("17cb5eb9-7cc1-4f89-80f7-17a3488b3f3e"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor"] },
            new EntityVariantRole() { Id = Guid.Parse("37de968b-77a6-44b6-ae03-55b90b67ae86"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("2631b13e-4c30-43d9-a661-4fe9153b33e8"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("a512c157-dac4-4010-b6e4-9e7369691465"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("85934293-4a32-47e5-a311-8c9ed1d19d05"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("837878c9-082c-4596-8fd1-e15a925cec9f"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("1919f3c0-5b33-4aea-b555-b477e5d5d898"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("a000fde8-4ea1-4feb-abb6-110cc66ea532"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("3555050c-1d40-4792-8c17-a632aa79cf68"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("42a7e8eb-02c7-4f88-8a1e-1cde836a4e4a"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("c077c4aa-ca93-4b02-a63d-435aa8b67657"), VariantId = variants["ORGL"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("2cc95e38-2c26-4c0d-b5a0-5e02e27a5694"), VariantId = variants["PERS"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("14720c1d-cc77-4b9a-af4c-a841111ced95"), VariantId = variants["PERS"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("6c71cdc7-ccb2-4ea9-bc70-ed359e6ab77d"), VariantId = variants["PERS"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("cefddda9-fc8a-4a5b-b281-b9b48c258274"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("5c992d20-618c-4873-ae06-45aa2d041ad4"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("e12c7a34-d0ba-492e-9849-726aa913ec4d"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("1448be40-44ca-4184-94c3-c4adc95cb88d"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("3eead88d-e5de-4d7a-a193-c4e10d8581dc"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("359977c3-5bed-45a7-8279-80ef4a90d914"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("7f58e6cd-32b9-4954-9ac1-d9141d2e0898"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("98c9a5d2-801e-4b25-b7c1-c14dbdc61a87"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("0d651f64-f4a6-4d49-87e7-9067da8dc00c"), VariantId = variants["PK"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("34e03820-ee94-4354-b616-469bdd42064a"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("bb871689-69f8-4402-a865-23d790126af9"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("e5d984a2-76eb-4655-833e-3192dadd8d40"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("693d0182-9875-46c5-b06d-8c553edff687"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("cf5f6b0a-3eea-498a-91fa-9250638eee5e"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("20d1b163-1788-4bf7-8cc7-3a338f684564"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("746e8dc8-748d-4563-b259-b1e31701b414"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("d8ba21ba-ef38-4c33-811c-abcaa1d31909"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5dc12550-d4af-4ed0-9082-15bb623fad83"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("38001503-9484-4e37-8137-9f5b5d43394d"), VariantId = variants["PRE"], RoleId = roles["urn:altinn:external-role:ccr:bestyrende-reder"] },
            new EntityVariantRole() { Id = Guid.Parse("f48cd7e1-3e84-4b5a-bb6b-251b4710609b"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("452c7e46-1ec1-4306-ad32-60dcb1893418"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("a3b38ac0-f0e9-40ef-bada-a6f5ac0a1053"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("43c82543-ae85-44b2-8303-b2b4f5a7d956"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("a1051a12-34f9-4fcd-acbf-2d2b57a09970"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("d9b4b3b8-abe5-4f7f-8d31-48efa3d630dd"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("98256531-4b49-4bb0-9313-9ae0eb308a59"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("24ce90eb-3bac-4141-ab47-59aef9ce362e"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("5f1fae53-880c-430c-9ab6-ce378972782e"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("58eb56ff-ed4e-4183-b197-12dced372988"), VariantId = variants["SA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("4ac5a409-be20-4b2a-9100-107584991c61"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("9214c70a-a7b4-418d-bd19-d7ba0f0ff811"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("dcfc1fbe-ae18-429d-b636-1a153870d929"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:helseforetak"] },
            new EntityVariantRole() { Id = Guid.Parse("5d1c511f-79ec-4e29-a4fa-adc5d18670cf"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("e1700447-fa5c-4c69-be9f-a994c69958c0"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("7e2f9fb7-f15e-49c7-b93a-d1377f5f5f8d"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("d3697561-5fed-409a-8353-f725b3728fda"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("40861223-50c4-4bb7-a094-32c2c0b2c117"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("26313d93-2534-4cc3-89af-ffb6e53d410e"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("7c6bcbaf-fb0f-438f-9784-2fb469d6b9e6"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("3103cde4-8416-4733-bc0d-1e55d7d31b5c"), VariantId = variants["SÆR"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("de000c99-5185-4830-a47b-985385c13c24"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5d168ead-8de0-40eb-bc63-69cc4c75ac62"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("60537b79-8b9d-4d5f-85d9-4f12c25158cb"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("75c71023-1da2-4aad-bc29-41757860ac7b"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("f087189d-04ea-4221-ba80-413dd4568ced"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("2b064f21-8230-494e-b87c-bec1b6600115"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("18e96a0a-f6e9-4e31-a1fd-2f615d82df4f"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("6327f44d-c77d-4f14-bcda-d18bfe75cd2c"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("ed2b0746-6f50-4e38-87b7-e939b27bb743"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("39933535-9108-47f0-877d-efe06aafd96e"), VariantId = variants["SAM"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("9d86bc2a-8bdf-46bf-87df-3630a05b553c"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("b13f3d8c-5720-4cd8-ab6a-eb94f68a13e6"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("6d8fc483-9382-40e7-a70f-86ff5109949a"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("9c8efece-de93-4285-8d2e-16b932ec14bd"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("0f2a7f9e-b399-42b1-8dfc-ce33f758f63e"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("291dcf6d-23d3-4e79-83a2-f02bb576438d"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("dd13ffca-516c-4f5a-a925-bff963dd63dd"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("b085c806-f191-472d-9ccd-e0820ceb4eed"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("10b386aa-d8ce-456a-9d41-fa468b0589f1"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("70d2e1ff-3234-46ca-b737-64586aa8b29b"), VariantId = variants["SE"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("fc92a941-965b-404a-b12d-1e75cfbad42f"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("27fe9114-b8cc-4411-8586-cf2c850dcdf7"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("e890697c-47d9-4747-a824-dd1d1351bc13"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("6fbd440a-b013-455e-8010-18ce0a0ba3a2"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("347e9e80-672b-48af-a3fa-11f9d8f4e58b"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("d9bf2965-adb7-485d-a82c-29f772ca7da5"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("6932ff2c-bfca-4853-9e6d-60668040808d"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("c6d8a6d0-8a22-4e65-b981-ad2ec25ade9c"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("87d6e70e-4def-4d66-b124-771e3b316ba3"), VariantId = variants["SF"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("de59e34d-8acd-422b-b6ba-835ff0f2f516"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("8fc346ed-ec68-472c-b532-33a7de7bd6aa"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("918e0ee4-e1eb-42fb-a0a9-2e3a0015983d"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("a04b63f1-641c-4a1e-aee6-6e4c73864847"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("e37dffbe-c738-4b4e-aae3-75eee6e8ee40"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("a0854d98-c95a-4d59-a68b-dd7f9ce3de0e"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("7f454cdd-1a58-45ff-87a7-345f2f11a84b"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("189f7b86-ed13-420c-831c-01d468e8f879"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("733c7132-7851-40dc-b441-0d20b01f40f1"), VariantId = variants["SPA"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("3b39d830-b0fc-4435-b2ed-2b4de0f5941d"), VariantId = variants["STAT"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("1859aac6-3517-4e8d-b46a-d7ea6c16d884"), VariantId = variants["STAT"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("6b12a751-d7b1-4987-bfa8-af6c1219f3b1"), VariantId = variants["STAT"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("21a1ec57-b04e-42f9-b14f-1523896fe8df"), VariantId = variants["STAT"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("db2b19fc-7b1c-4f73-8720-6d2f12d4e8f4"), VariantId = variants["STAT"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("10a17b0e-5639-4d3f-906e-7fdb51258c78"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("5c97b266-e081-4c50-b175-651728200f75"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("1729dc99-564d-4b46-b32f-742fee31d599"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("dc43840e-f8c8-4d99-b4fa-85d8e37e13f6"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("25929489-67e6-41a3-9475-dbc855f99b11"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("6aac69ef-9fc4-43b9-ae38-82412e99ae2d"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("ac08a007-97ba-427e-9b8e-a26f355c90d4"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("384df41a-d761-4dac-ba7a-ca409b170c29"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("23b0008a-041c-4e64-a043-b04ab0c5fc66"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("3ff09437-c00f-44c5-aaa8-923c6c666fd8"), VariantId = variants["STI"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("3dfb7268-00c5-416a-9eff-20836aebe7de"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:nestleder"] },
            new EntityVariantRole() { Id = Guid.Parse("676bc013-6e93-47aa-ab13-477e1d7c1714"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:varamedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("43c84fd1-99a1-4213-8861-ae6dc73e6aa4"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("f7556503-472e-4b2c-9a2c-1c51bf24f9cb"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:styremedlem"] },
            new EntityVariantRole() { Id = Guid.Parse("37168a3b-e7c5-4233-ae0c-de5a09ce658a"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:styreleder"] },
            new EntityVariantRole() { Id = Guid.Parse("236e4744-8a22-43a3-ad77-f8b4bb9332cb"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:innehaver"] },
            new EntityVariantRole() { Id = Guid.Parse("2522a5f2-e188-4bdf-b6ec-dabee7a5ed6d"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("8870d327-c57a-4f64-b61e-becab801cd64"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("5f03421b-df32-499e-af20-077cdfc9d4e2"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:observator"] },
            new EntityVariantRole() { Id = Guid.Parse("5bc5c51f-2e30-404b-a013-c36afe33f327"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("14fbe6fd-a948-4548-ac2e-16bcb3747a92"), VariantId = variants["TVAM"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("f6c5d815-c2ce-4187-818e-dd315ffe47d8"), VariantId = variants["UTLA"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("9579b6ed-5adc-4b33-9523-813044ee52fd"), VariantId = variants["UTLA"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("c4cecad8-f41c-4db2-8b8f-0ab6357e7d7a"), VariantId = variants["UTLA"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
            new EntityVariantRole() { Id = Guid.Parse("f19bf3c9-d1d2-4c5a-baec-4cc6cfbc9973"), VariantId = variants["VPFO"], RoleId = roles["urn:altinn:external-role:ccr:forretningsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("714a128c-737f-4cb0-aa8c-7e9d4a6c6f19"), VariantId = variants["VPFO"], RoleId = roles["urn:altinn:external-role:ccr:regnskapsforer"] },
            new EntityVariantRole() { Id = Guid.Parse("e1d2aedd-6f33-49a5-a0dd-aa22fb432902"), VariantId = variants["VPFO"], RoleId = roles["urn:altinn:external-role:ccr:kontaktperson"] },
            new EntityVariantRole() { Id = Guid.Parse("c1064173-8eba-43a8-8b62-06de54d11bb3"), VariantId = variants["VPFO"], RoleId = roles["urn:altinn:external-role:ccr:revisor"] },
            new EntityVariantRole() { Id = Guid.Parse("a3719e58-286d-4395-95b0-1a654f2eeafa"), VariantId = variants["VPFO"], RoleId = roles["urn:altinn:external-role:ccr:daglig-leder"] },
        };

        await ingestService.IngestAndMergeData(variantRoles, options: options, null, cancellationToken);
    }
    private async Task Cleanup(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var dataKey = "<cleanup-data>";
        if (migrationService.NeedMigration<RolePackage>(dataKey, 1))
        {
            await CleanupRolePackage(options, cancellationToken);
            await migrationService.LogMigration<RolePackage>(dataKey, string.Empty, 1);
        }
    }

    private async Task CleanupRolePackage(ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        var packages = new Dictionary<string, Guid>();
        foreach (var pack in await packageService.Get())
        {
            packages.Add(pack.Urn, pack.Id);
        }

        var roles = new Dictionary<string, Guid>();
        foreach (var role in await roleService.Get())
        {
            roles.Add(role.Urn, role.Id);
        }

        var variants = new Dictionary<string, Guid>();
        foreach (var variant in await entityVariantService.Get())
        {
            variants.Add(variant.Name, variant.Id);
        }

        try
        {
            var filter = rolePackageRepository.CreateFilterBuilder();
            filter.Equal(t => t.RoleId, roles["urn:altinn:external-role:ccr:forretningsforer"]);
            filter.Equal(t => t.PackageId, packages["urn:altinn:accesspackage:forretningsforer-eiendom"]);
            filter.Equal(t => t.EntityVariantId, variants["BBL"]);
            await rolePackageRepository.Delete(filter, options, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {

        }
    }
}
