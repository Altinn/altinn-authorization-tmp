using System.Diagnostics.CodeAnalysis;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

/// <summary>
/// Defines constant <see cref="Package"/> instances used across the system.
/// Each package represents a specific access package in the access management domain,
/// with a fixed unique identifier (GUID), localized names, descriptions, area references, and provider information.
/// </summary>
public static class PackageConstants
{
    /// <summary>
    /// Try to get <see cref="Package"/> by name.
    /// </summary>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out ConstantDefinition<Package>? result)
        => ConstantLookup.TryGetByName(typeof(PackageConstants), name, out result);

    /// <summary>
    /// Try to get <see cref="Package"/> using Guid.
    /// </summary>
    public static bool TryGetById(Guid id, [NotNullWhen(true)] out ConstantDefinition<Package>? result)
        => ConstantLookup.TryGetById(typeof(PackageConstants), id, out result);

    /// <summary>
    /// Try to get <see cref="Role"/> by Urn.
    /// </summary>
    public static bool TryGetByUrn(string urn, [NotNullWhen(true)] out ConstantDefinition<Package>? result)
        => ConstantLookup.TryGetByUrn(typeof(PackageConstants), urn, out result);

    /// <summary>
    /// Get all constants as a read-only collection.
    /// </summary>
    public static IReadOnlyCollection<ConstantDefinition<Package>> AllEntities()
        => ConstantLookup.AllEntities<Package>(typeof(PackageConstants));

    /// <summary>
    /// Get all translations as read-only collection.
    /// </summary>
    public static IReadOnlyCollection<TranslationEntry> AllTranslations()
        => ConstantLookup.AllTranslations<Package>(typeof(PackageConstants));

    #region Accesment Packages

    /// <summary>
    /// Represents the Client Administrator package.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7e82-9b4f-7d63e773bbca
    /// - <c>Name:</c> "Klientadministrator"
    /// - <c>Description:</c> "Gir mulighet til å administrere tilgang til tjenester videre til ansatte på vegne av deres kunder"
    /// - <c>Urn:</c> "urn:altinn:accesspackage:klientadministrator"
    /// - <c>IsDelegable:</c> false
    /// - <c>HasResources:</c> false
    /// - <c>IsAssignable:</c> true
    /// - <c>AreaId:</c> ManageAccess (Administrere tilganger)
    /// </remarks>
    public static ConstantDefinition<Package> ClientAdministrator { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7e82-9b4f-7d63e773bbca")
    {
        Entity = new()
        {
            Name = "Klientadministrator",
            Description = "Gir mulighet til å administrere tilgang til tjenester videre til ansatte på vegne av deres kunder",
            Urn = "urn:altinn:accesspackage:klientadministrator",
            IsDelegable = false,
            HasResources = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ManageAccess.Id,
        },
    };

    /// <summary>
    /// Represents the Access Manager package.
    /// </summary>
    public static ConstantDefinition<Package> AccessManager { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7a95-ad36-900c3d8ad300")
    {
        Entity = new()
        {
            Name = "Tilgangsstyrer",
            Description = "Gir mulighet til å gi videre tilganger for virksomheten som man selv har mottatt",
            Urn = "urn:altinn:accesspackage:tilgangsstyrer",
            IsDelegable = false,
            HasResources = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ManageAccess.Id,
        },
    };

    /// <summary>
    /// Represents the Main Administrator package.
    /// </summary>
    public static ConstantDefinition<Package> MainAdministrator { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7e16-ab0c-36dc8ab1a29d")
    {
        Entity = new()
        {
            Name = "Hovedadministrator",
            Description = "Gir mulighet til å administrere alle tilganger for virksomheten",
            Urn = "urn:altinn:accesspackage:hovedadministrator",
            IsDelegable = false,
            HasResources = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ManageAccess.Id,
        },
    };

    /// <summary>
    /// Represents the Maskinporten Administrator package.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7b30-a84d-f37fed9fb89c
    /// - <c>Name:</c> "Maskinporten administrator"
    /// - <c>Description:</c> "Gir bruker mulighet til å administrere tilgang til maskinporten scopes"
    /// - <c>Urn:</c> "urn:altinn:accesspackage:maskinporten-administrator"
    /// - <c>IsDelegable:</c> false
    /// - <c>HasResources:</c> false
    /// - <c>IsAssignable:</c> true
    /// - <c>AreaId:</c> ManageAccess (Administrere tilganger)
    /// </remarks>
    public static ConstantDefinition<Package> MaskinportenAdministrator { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7b30-a84d-f37fed9fb89c")
    {
        Entity = new()
        {
            Name = "Maskinporten administrator",
            Description = "Gir bruker mulighet til å administrere tilgang til maskinporten scopes",
            Urn = "urn:altinn:accesspackage:maskinporten-administrator",
            IsDelegable = false,
            HasResources = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ManageAccess.Id,
        },
    };

    /// <summary>
    /// Represents the Konkursbo Administrator package.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7e9c-95c1-48937e23960a
    /// - <c>Name:</c> "Konkursbo administrator"
    /// - <c>Description:</c> "Gir bruker mulighet til å administrere konkursbo"
    /// - <c>Urn:</c> "urn:altinn:accesspackage:konkursbo-tilgangsstyrer"
    /// - <c>IsDelegable:</c> false
    /// - <c>HasResources:</c> false
    /// - <c>IsAssignable:</c> true
    /// - <c>AreaId:</c> ManageAccess (Administrere tilganger)
    /// </remarks>
    public static ConstantDefinition<Package> KonkursboAdministrator { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7e9c-95c1-48937e23960a")
    {
        Entity = new()
        {
            Name = "Konkursbo administrator",
            Description = "Gir bruker mulighet til å administrere konkursbo",
            Urn = "urn:altinn:accesspackage:konkursbo-tilgangsstyrer",
            IsDelegable = false,
            HasResources = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ManageAccess.Id,
        },
    };

    #endregion

    #region Tax and Accounting Packages

    /// <summary>
    /// Represents the Business Tax package.
    /// </summary>
    public static ConstantDefinition<Package> BusinessTax { get; } = new ConstantDefinition<Package>("1dba50d6-f604-48e9-bd41-82321b13e85c")
    {
        Entity = new()
        {
            Name = "Skatt næring",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til skatt for næringer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skatt-naering",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Tax Base package.
    /// </summary>
    public static ConstantDefinition<Package> TaxBase { get; } = new ConstantDefinition<Package>("4c859601-9b2b-4662-af39-846f4117ad7a")
    {
        Entity = new()
        {
            Name = "Skattegrunnlag",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til innhenting av skattegrunnlag. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skattegrunnlag",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Value Added Tax package.
    /// </summary>
    public static ConstantDefinition<Package> ValueAddedTax { get; } = new ConstantDefinition<Package>("9a61b136-7810-4939-ab6d-84938e9a12c6")
    {
        Entity = new()
        {
            Name = "Merverdiavgift",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:merverdiavgift",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Motor Vehicle Taxes package.
    /// </summary>
    public static ConstantDefinition<Package> MotorVehicleTaxes { get; } = new ConstantDefinition<Package>("4f4d4567-2384-49fa-b34b-84b4b77139d8")
    {
        Entity = new()
        {
            Name = "Motorvognavgifter",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til motorvognavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:motorvognavgift",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Accounting and Economic Reporting package.
    /// </summary>
    public static ConstantDefinition<Package> AccountingAndEconomicReporting { get; } = new ConstantDefinition<Package>("b4e69b54-895e-42c5-bf0d-861a571cd282")
    {
        Entity = new()
        {
            Name = "Regnskap og økonomirapportering",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til regnskap og øknomirapportering som ikke tilhører skatt og merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:regnskap-okonomi-rapport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Claims, Payments and Enforcement package.
    /// </summary>
    public static ConstantDefinition<Package> ClaimsPaymentsAndEnforcement { get; } = new ConstantDefinition<Package>("a02fc602-872a-4671-b338-8b86a64b534a")
    {
        Entity = new()
        {
            Name = "Krav, betalinger og utlegg",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til krav og utlegg. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:krav-og-utlegg",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Auditor Attestation package.
    /// </summary>
    public static ConstantDefinition<Package> AuditorAttestation { get; } = new ConstantDefinition<Package>("1886712b-e077-445a-ab3f-8c8bdebccc67")
    {
        Entity = new()
        {
            Name = "Revisorattesterer",
            Description = "Denne fullmakten gir tilgang til alle tjenester som krever revisorattestering. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:reviorattesterer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Special Taxes package.
    /// </summary>
    public static ConstantDefinition<Package> SpecialTaxes { get; } = new ConstantDefinition<Package>("74c1dcf6-a760-4065-83f0-8edc6dec5dba")
    {
        Entity = new()
        {
            Name = "Særavgifter",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til særavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:saeravgifter",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Credit and Settlement Arrangements package.
    /// </summary>
    public static ConstantDefinition<Package> CreditAndSettlementArrangements { get; } = new ConstantDefinition<Package>("eb9006a6-dbd5-4155-9174-91e182a56715")
    {
        Entity = new()
        {
            Name = "Kreditt- og oppgjørsordninger",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kreditt- og oppgjørsordninger. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:kreditt-og-oppgjoer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    /// <summary>
    /// Represents the Customs package.
    /// </summary>
    public static ConstantDefinition<Package> Customs { get; } = new ConstantDefinition<Package>("deb7f8ae-8427-469d-b824-937c146c0489")
    {
        Entity = new()
        {
            Name = "Toll",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til toll og fortolling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:toll",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms.Id,
        },
    };

    #endregion

    #region Personnel Packages

    /// <summary>
    /// Represents the Employment Relations package.
    /// </summary>
    public static ConstantDefinition<Package> EmploymentRelations { get; } = new ConstantDefinition<Package>("cf575d42-72be-4d14-b0d2-77352221df4f")
    {
        Entity = new()
        {
            Name = "Ansettelsesforhold",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til ansettelsesforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:ansettelsesforhold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    /// <summary>
    /// Represents the Salary package.
    /// </summary>
    public static ConstantDefinition<Package> Salary { get; } = new ConstantDefinition<Package>("fb0aa257-e7dc-4b7b-9528-77dfb749461c")
    {
        Entity = new()
        {
            Name = "Lønn",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til lønn og honorar. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:lonn",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    /// <summary>
    /// Represents the A-ordningen package.
    /// </summary>
    public static ConstantDefinition<Package> AOrderSystem { get; } = new ConstantDefinition<Package>("4af2d44d-f9f8-4585-b679-7875bb1828ea")
    {
        Entity = new()
        {
            Name = "A-ordningen",
            Description = "Denne tilgangspakken gir fullmakter til tjenester som inngår i A-ordningen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.  OBS! Vær oppmerksompå at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.",
            Urn = "urn:altinn:accesspackage:a-ordning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    /// <summary>
    /// Represents the Pension package.
    /// </summary>
    public static ConstantDefinition<Package> Pension { get; } = new ConstantDefinition<Package>("ae41bf08-c7ef-4bfe-b2b9-7bf6deb7798f")
    {
        Entity = new()
        {
            Name = "Pensjon",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til pensjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:pensjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    /// <summary>
    /// Represents the Sick Leave package.
    /// </summary>
    public static ConstantDefinition<Package> SickLeave { get; } = new ConstantDefinition<Package>("08ec673d-9126-4ed0-ae75-7ceec8633c77")
    {
        Entity = new()
        {
            Name = "Sykefravær",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykefravær. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sykefravaer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    /// <summary>
    /// Represents the Leave package.
    /// </summary>
    public static ConstantDefinition<Package> Leave { get; } = new ConstantDefinition<Package>("71a2bd47-e885-49c7-8a58-7ef4bd936f41")
    {
        Entity = new()
        {
            Name = "Permisjon",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til permisjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:permisjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Personnel.Id,
        },
    };

    #endregion

    #region Environment, Accident and Safety Packages

    /// <summary>
    /// Represents the Waste Management package.
    /// </summary>
    public static ConstantDefinition<Package> WasteManagement { get; } = new ConstantDefinition<Package>("5eb07bdc-5c3c-4c85-add3-5405b214b8a3")
    {
        Entity = new()
        {
            Name = "Renovasjon",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till renovasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:renovasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    /// <summary>
    /// Represents the Environmental Cleanup package.
    /// </summary>
    public static ConstantDefinition<Package> EnvironmentalCleanup { get; } = new ConstantDefinition<Package>("04c5a001-5249-4765-ae8e-58617c404223")
    {
        Entity = new()
        {
            Name = "Miljørydding, miljørensing og lignende",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till miljørydding, miljørensing og lignende. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    /// <summary>
    /// Represents the Sustainability package.
    /// </summary>
    public static ConstantDefinition<Package> Sustainability { get; } = new ConstantDefinition<Package>("bacc9294-56fd-457f-930e-59ee4a7a3894")
    {
        Entity = new()
        {
            Name = "Bærekraft",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till tiltak og rapportering på bærekraft. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:baerekraft",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    /// <summary>
    /// Represents the Security and Internal Control package.
    /// </summary>
    public static ConstantDefinition<Package> SecurityAndInternalControl { get; } = new ConstantDefinition<Package>("cfe074fa-0a66-4a4b-974a-5d1db8eb94e6")
    {
        Entity = new()
        {
            Name = "Sikkerhet og internkontroll",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till sikkerhet og internkontroll. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sikkerhet-og-internkontroll",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    /// <summary>
    /// Represents the Accident package.
    /// </summary>
    public static ConstantDefinition<Package> Accident { get; } = new ConstantDefinition<Package>("a03af7d5-74b9-4f18-aead-5d47edc36be5")
    {
        Entity = new()
        {
            Name = "Ulykke",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till ulykke. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:ulykke",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    /// <summary>
    /// Represents the Occupational Injury package.
    /// </summary>
    public static ConstantDefinition<Package> OccupationalInjury { get; } = new ConstantDefinition<Package>("fa84bffc-ac17-40cd-af9c-61c89f92e44c")
    {
        Entity = new()
        {
            Name = "Yrkesskade",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till yrkesskade. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:yrkesskade",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety.Id,
        },
    };

    #endregion

    #region Postal and Archive Packages

    /// <summary>
    /// Represents the Regular Mail to Business package.
    /// </summary>
    public static ConstantDefinition<Package> RegularMailToBusiness { get; } = new ConstantDefinition<Package>("91cf61ae-69ab-49d5-b51a-80591c91f255")
    {
        Entity = new()
        {
            Name = "Ordinær post til virksomheten",
            Description = "Denna fullmaktan gir tilgang till all mottatt post som ikke innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmaktan gis normalt till de i virksomheten som håndterer mottak av post. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:ordinaer-post-til-virksomheten",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.MailAndArchive.Id,
        },
    };

    /// <summary>
    /// Represents the Confidential Mail to Business package.
    /// </summary>
    public static ConstantDefinition<Package> ConfidentialMailToBusiness { get; } = new ConstantDefinition<Package>("bb0569a6-2268-49b5-9d38-8158b26124c3")
    {
        Entity = new()
        {
            Name = "Post til virksomheten med taushetsbelagt innhold",
            Description = "Denna fullmaktan gir tilgang till all mottatt post som innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmaktan gis normalt till de i virksomheten som håndterer mottak av post som har taushetsbelagt innhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:post-til-virksomheten-med-taushetsbelagt-innhold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.MailAndArchive.Id,
        },
    };

    #endregion

    #region Business Affairs Packages

    /// <summary>
    /// Represents the General Helfo Services package.
    /// </summary>
    public static ConstantDefinition<Package> GeneralHelfoServices { get; } = new ConstantDefinition<Package>("06c13400-8683-4985-9802-cef13e247f24")
    {
        Entity = new()
        {
            Name = "Generelle helfotjenester",
            Description = "Denne fullmakten gir tilgang til ordinære tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:generelle-helfotjenester",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Helfo Services with Special Category Personal Data package.
    /// </summary>
    public static ConstantDefinition<Package> HelfoServicesSpecialCategory { get; } = new ConstantDefinition<Package>("2ee6e972-2423-4f3a-bd3c-d4871fcc9876")
    {
        Entity = new()
        {
            Name = "Helfotjenester med personopplysninger av særlig kategori",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger av særlig kategori. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir",
            Urn = "urn:altinn:accesspackage:helfo-saerlig-kategori",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Start, Change and Dissolve Business package.
    /// </summary>
    public static ConstantDefinition<Package> StartChangeDissolveEBusiness { get; } = new ConstantDefinition<Package>("956cfcf0-ab4c-44d8-98a2-d68c4d59321b")
    {
        Entity = new()
        {
            Name = "Starte, endre og avvikle virksomhet",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å starte, endre og avvikle en virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Shares and Ownership package.
    /// </summary>
    public static ConstantDefinition<Package> SharesAndOwnership { get; } = new ConstantDefinition<Package>("7dfc669b-70e4-4974-9e11-d6dca4803aaa")
    {
        Entity = new()
        {
            Name = "Aksjer og eierforhold",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til aksjer og eierforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:aksjer-og-eierforhold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Certificates package.
    /// </summary>
    public static ConstantDefinition<Package> Certificates { get; } = new ConstantDefinition<Package>("4717e6e0-1ec2-4354-b825-d9a9e2588fb1")
    {
        Entity = new()
        {
            Name = "Attester",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til attestering av virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:attester",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Document-based Supervision package.
    /// </summary>
    public static ConstantDefinition<Package> DocumentBasedSupervision { get; } = new ConstantDefinition<Package>("df13707e-5252-496a-bc00-daffeed1e4b2")
    {
        Entity = new()
        {
            Name = "Dokumentbasert tilsyn",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til dokumentbaserte tilsyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:dokumentbasert-tilsyn",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Infrastructure package.
    /// </summary>
    public static ConstantDefinition<Package> Infrastructure { get; } = new ConstantDefinition<Package>("75978efe-2437-421e-8c77-dd61925c7ba4")
    {
        Entity = new()
        {
            Name = "Infrastruktur",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens infrastruktur. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:infrastruktur",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Patent, Trademark and Design package.
    /// </summary>
    public static ConstantDefinition<Package> PatentTrademarkDesign { get; } = new ConstantDefinition<Package>("250568e6-ed9f-4bdc-b9e3-df08be7181ba")
    {
        Entity = new()
        {
            Name = "Patent, varemerke og design",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:patent-varemerke-design",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    #endregion

    #region Industry-Specific Packages

    /// <summary>
    /// Represents the Agriculture package.
    /// </summary>
    public static ConstantDefinition<Package> Agriculture { get; } = new ConstantDefinition<Package>("c5bbbc3f-605a-4dcb-a587-32124d7bb76d")
    {
        Entity = new()
        {
            Name = "Jordbruk",
            Description = "Denna tilgangspakken gir tilgang till tjenester knyttet till jordbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:jordbruk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Animal Husbandry package.
    /// </summary>
    public static ConstantDefinition<Package> AnimalHusbandry { get; } = new ConstantDefinition<Package>("7b8a3aaa-c8ed-4ac4-923a-335f4f9eb45a")
    {
        Entity = new()
        {
            Name = "Dyrehold",
            Description = "Denna tilgangspakken gir tilgang till tjenester knyttet till dyrehold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:dyrehold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Reindeer Herding package.
    /// </summary>
    public static ConstantDefinition<Package> ReindeerHerding { get; } = new ConstantDefinition<Package>("0bb5963e-df17-4f35-b913-3ce10a34b866")
    {
        Entity = new()
        {
            Name = "Reindrift",
            Description = "Denna tilgangspakken gir tilgang till tjenester knyttet till reindrift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:reindrift",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Hunting and Wildlife Management package.
    /// </summary>
    public static ConstantDefinition<Package> HuntingAndWildlifeManagement { get; } = new ConstantDefinition<Package>("906aec0d-ad1f-496b-a0bb-40f81b3303cb")
    {
        Entity = new()
        {
            Name = "Jakt og viltstell",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till jakt og viltstell. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:jakt-og-viltstell",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Forestry package.
    /// </summary>
    public static ConstantDefinition<Package> Forestry { get; } = new ConstantDefinition<Package>("f7e02568-90b6-477d-8abb-44984ddeb1f9")
    {
        Entity = new()
        {
            Name = "Skogbruk",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till skogbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skogbruk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Fishing package.
    /// </summary>
    public static ConstantDefinition<Package> Fishing { get; } = new ConstantDefinition<Package>("9d2ec6e9-5148-4f47-9ae4-4536f6c9c1cb")
    {
        Entity = new()
        {
            Name = "Fiske",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till fiske. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:fiske",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    /// <summary>
    /// Represents the Aquaculture package.
    /// </summary>
    public static ConstantDefinition<Package> Aquaculture { get; } = new ConstantDefinition<Package>("78c21107-7d2d-4e85-af82-47ea0e47ceca")
    {
        Entity = new()
        {
            Name = "Akvakultur",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till akvakultur og fiskeoppdrett. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:akvakultur",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture.Id,
        },
    };

    #endregion

    #region Construction, Infrastructure and Real Estate Packages

    /// <summary>
    /// Represents the Building Application package.
    /// </summary>
    public static ConstantDefinition<Package> BuildingApplication { get; } = new ConstantDefinition<Package>("1e40697b-e178-4920-8fbd-af5164b4d147")
    {
        Entity = new()
        {
            Name = "Byggesøknad",
            Description = "Denna tilgangspakken gir fullmakter till tjenester som ansvarlig søker/tiltakshaver trenger, for eksempel byggesøknader, direkte signerte erklæringer, nabovarsel og eiendomssak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:byggesoknad",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Planning Case package.
    /// </summary>
    public static ConstantDefinition<Package> PlanningCase { get; } = new ConstantDefinition<Package>("482eee1e-ec79-45bb-8bd0-af8459cbb9f0")
    {
        Entity = new()
        {
            Name = "Plansak",
            Description = "Denna tilgangspakken gir fullmakter till tjenester som forslagsstiller/ plankonsulent trenger, for eksempel varsel om planopppstart og høring og offentlig ettersyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:plansak",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Receive Neighbor and Planning Notifications package.
    /// </summary>
    public static ConstantDefinition<Package> ReceiveNeighborPlanNotifications { get; } = new ConstantDefinition<Package>("1e1e8bf9-096f-4de2-9db7-b176db55db09")
    {
        Entity = new()
        {
            Name = "Motta nabo- og planvarsel",
            Description = "Denna tilgangspakken gir fullmakter till tjenester till å lese og svare på varsel om plan-/byggesak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:motta-nabo-og-planvarsel",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Construction of Buildings and Facilities package.
    /// </summary>
    public static ConstantDefinition<Package> ConstructionBuildingsFacilities { get; } = new ConstantDefinition<Package>("ac655a2f-be29-4888-9a6c-b21e524fa90e")
    {
        Entity = new()
        {
            Name = "Oppføring av bygg og anlegg",
            Description = "Denna tilgangspakken gir fullmakter till tjenester relatert till oppføring av bygninger og annlegg unntatt plan og byggesaksbehandling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:oppforing-bygg-anlegg",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Buying and Selling Real Estate package.
    /// </summary>
    public static ConstantDefinition<Package> BuyingSellungRealEstate { get; } = new ConstantDefinition<Package>("b98ec05d-1ac5-4ced-8250-b6d75b83502b")
    {
        Entity = new()
        {
            Name = "Kjøp og salg av eiendom",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till kjøp og salg av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:kjop-og-salg-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Real Estate Rental package.
    /// </summary>
    public static ConstantDefinition<Package> RealEstateRental { get; } = new ConstantDefinition<Package>("565a3110-3d51-4e72-ae4c-b89308e5c96e")
    {
        Entity = new()
        {
            Name = "Utleie av eiendom",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till utleie av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:utleie-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Real Estate Agent package.
    /// </summary>
    public static ConstantDefinition<Package> RealEstateAgent { get; } = new ConstantDefinition<Package>("79329c4a-d856-4491-965e-bcc4ed5a7453")
    {
        Entity = new()
        {
            Name = "Eiendomsmegler",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till omsetning og drift av fast eiendom på oppdrag, som eiendomsmegling og eiendomsforvaltning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:eiendomsmegler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    /// <summary>
    /// Represents the Property Registration package.
    /// </summary>
    public static ConstantDefinition<Package> PropertyRegistration { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7642-b9b8-c748ee4fecd4")
    {
        Entity = new()
        {
            Name = "Tinglysing eiendom",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till elektronisk tinglysing av rettigheter i eiendom.",
            Urn = "urn:altinn:accesspackage:tinglysing-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate.Id,
        },
    };

    #endregion

    #region Transport and Storage Packages

    /// <summary>
    /// Represents the Road Transport package.
    /// </summary>
    public static ConstantDefinition<Package> RoadTransport { get; } = new ConstantDefinition<Package>("36c245db-3207-449e-92b1-94cb0a0f3031")
    {
        Entity = new()
        {
            Name = "Veitransport",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till person- og godstransport langs veinettet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:veitransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Pipeline Transport package.
    /// </summary>
    public static ConstantDefinition<Package> PipelineTransport { get; } = new ConstantDefinition<Package>("9cc8b401-b4cb-473d-a3fa-96171aeb1389")
    {
        Entity = new()
        {
            Name = "Transport i rør",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till transport i rør. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:transport-i-ror",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Maritime Transport package.
    /// </summary>
    public static ConstantDefinition<Package> MaritimeTransport { get; } = new ConstantDefinition<Package>("9603dd83-be41-4190-b0a7-97490f4a601d")
    {
        Entity = new()
        {
            Name = "Sjøfart",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till skipsarbeidstakere og fartøy till sjøs. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:sjofart",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Air Transport package.
    /// </summary>
    public static ConstantDefinition<Package> AirTransport { get; } = new ConstantDefinition<Package>("3b3bc323-207b-4d56-a21f-97b6875ccc28")
    {
        Entity = new()
        {
            Name = "Lufttransport",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till luftfartøy og romfartøy. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:lufttransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Railway Transport package.
    /// </summary>
    public static ConstantDefinition<Package> RailwayTransport { get; } = new ConstantDefinition<Package>("dfcd3923-cb66-45f7-9535-999b6c0c496d")
    {
        Entity = new()
        {
            Name = "Jernbanetransport",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till jernbane, inkludert trikk, T-bane og sporvogn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:jernbanetransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Storage and Transport-Related Services package.
    /// </summary>
    public static ConstantDefinition<Package> StorageTransportServices { get; } = new ConstantDefinition<Package>("86a8ff87-3a3d-4f32-a4ef-99c43494bc6e")
    {
        Entity = new()
        {
            Name = "Lagring og andre tjenester tilknyttet transport",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till lagring og hjelpetjenester i forbindelse med transport, samt post- og kurervirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    #endregion

    #region Healthcare and Protection Packages

    /// <summary>
    /// Represents the Municipal Doctor package.
    /// </summary>
    public static ConstantDefinition<Package> MunicipalDoctor { get; } = new ConstantDefinition<Package>("03404270-aa2d-498f-9e6a-103043d41f1f")
    {
        Entity = new()
        {
            Name = "Kommuneoverlege",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er relevant for kommuneleger. Denna fullmaktan kan gi bruker tilgang till personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:kommuneoverlege",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.HealthCareAndProtection.Id,
        },
    };

    /// <summary>
    /// Represents the Health Services with Special Category Personal Data package.
    /// </summary>
    public static ConstantDefinition<Package> HealthServicesSpecialCategory { get; } = new ConstantDefinition<Package>("5a642040-46cf-4466-b671-115a022e3048")
    {
        Entity = new()
        {
            Name = "Helsetjenester med personopplysninger av særlig kategori",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er av særlig kategori. Denna fullmaktan kan gi bruker tilgang till sensitive personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.HealthCareAndProtection.Id,
        },
    };

    /// <summary>
    /// Represents the Health Services package.
    /// </summary>
    public static ConstantDefinition<Package> HealthServices { get; } = new ConstantDefinition<Package>("4fa0fbbc-3841-4405-9d94-12731a8fdb81")
    {
        Entity = new()
        {
            Name = "Helsetjenester",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende. Denna fullmaktan kan gi bruker tilgang till personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:helsetjenester",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.HealthCareAndProtection.Id,
        },
    };

    #endregion

    #region Education and Childhood Packages

    /// <summary>
    /// Represents the Child Protection package.
    /// </summary>
    public static ConstantDefinition<Package> ChildProtection { get; } = new ConstantDefinition<Package>("83ff0734-0de5-4c2a-939b-18a9452c00bc")
    {
        Entity = new()
        {
            Name = "Barnevern",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till barnevern. Denna fullmaktan kan gi bruker tilgang till helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.",
            Urn = "urn:altinn:accesspackage:barnevern",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Personnel Approval package.
    /// </summary>
    public static ConstantDefinition<Package> PersonnelApproval { get; } = new ConstantDefinition<Package>("8cf4ec08-90dc-47d0-93f8-64a50c9b38b0")
    {
        Entity = new()
        {
            Name = "Godkjenning av personell",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till å søke om godkjenning till enkeltpersoner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:godkjenning-av-personell",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Educational Institution Approval package.
    /// </summary>
    public static ConstantDefinition<Package> EducationalInstitutionApproval { get; } = new ConstantDefinition<Package>("b56d2614-3f9d-4d93-8a8e-64d80b654ad7")
    {
        Entity = new()
        {
            Name = "Godkjenning av utdanningsvirksomhet",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till å søke om godkjenning av utdanningsvirksomheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Higher Education package.
    /// </summary>
    public static ConstantDefinition<Package> HigherEducation { get; } = new ConstantDefinition<Package>("93f37a3d-f799-47b3-bc8e-675b5813abf4")
    {
        Entity = new()
        {
            Name = "Høyere utdanning og høyere yrkesfaglig utdanning",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till høyere utdanning og høyere yrkesfaglig utdanning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the After-School Program Leader package.
    /// </summary>
    public static ConstantDefinition<Package> AfterSchoolProgramLeader { get; } = new ConstantDefinition<Package>("219ea5f7-eb30-424f-a958-67d3c7e7a4c2")
    {
        Entity = new()
        {
            Name = "SFO-leder",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till førskole og fritidsordning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:sfo-leder",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    #endregion

    #region Integration Packages

    /// <summary>
    /// Represents the Delegable Maskinporten Scopes package.
    /// </summary>
    public static ConstantDefinition<Package> DelegableMaskinportenScopes { get; } = new ConstantDefinition<Package>("27f488d7-d1f6-4aee-ae81-2bb42d62c446")
    {
        Entity = new()
        {
            Name = "Delegerbare Maskinporten scopes",
            Description = "Denna tilgangspakken gir fullmakter till data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:maskinporten-scopes",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Integrations.Id,
        },
    };

    /// <summary>
    /// Represents the Delegable Maskinporten Scopes for Norwegian Foreign Enterprises package.
    /// </summary>
    public static ConstantDefinition<Package> DelegableMaskinportenScopesNUF { get; } = new ConstantDefinition<Package>("5dad616e-5538-4e3f-b15a-bae33f06c99f")
    {
        Entity = new()
        {
            Name = "Delegerbare Maskinporten scopes - NUF",
            Description = "Denna tilgangspakken gir fullmakter till data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring på vegne av norskregistrerte utenlandske foretak (NUF). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:maskinporten-scopes-nuf",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Integrations.Id,
        },
    };

    #endregion

    #region Professional Authorization Packages

    /// <summary>
    /// Represents the Accountant Authorizations package.
    /// </summary>
    public static ConstantDefinition<Package> AccountantAuthorizations { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7e0e-9d6a-5a6c50c9bb8a")
    {
        Entity = new()
        {
            Name = "Fullmakter for regnskapsfører",
            Description = "Denna fullmaktan gir regnskapsfører tilgang till å opptre på vegne av kunde, og utføre alle tjenester som krever denna fullmaktan. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakt hos regnskapsfører oppstår når virksomheten registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:regnskapsfoerer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AuthorizationsForAccountants.Id,
        },
    };

    /// <summary>
    /// Represents the Auditor Authorizations package.
    /// </summary>
    public static ConstantDefinition<Package> AuditorAuthorizations { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7d16-a3c4-6b5e8c40b1d5")
    {
        Entity = new()
        {
            Name = "Fullmakter for revisor",
            Description = "Denna fullmaktan gir revisor tilgang till å opptre på vegne av kunde, og utføre alle tjenester som krever denna fullmaktan. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakt hos revisor oppstår når virksomheten registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:revisor",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AuthorizationsForAuditors.Id,
        },
    };

    #endregion

    #region Energy, Water, Sewage and Waste Packages

    /// <summary>
    /// Represents the Electricity Production, Transmission and Distribution package.
    /// </summary>
    public static ConstantDefinition<Package> ElectricityProductionTransmissionDistribution { get; } = new ConstantDefinition<Package>("f56bdb15-686b-46f8-9343-bde5d7c17648")
    {
        Entity = new()
        {
            Name = "Elektrisitet - produsere, overføre og distribuere",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till elektrisitet: produsere, overføre og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    /// <summary>
    /// Represents the Steam and Hot Water package.
    /// </summary>
    public static ConstantDefinition<Package> SteamAndHotWater { get; } = new ConstantDefinition<Package>("27afe398-b25b-4287-b0fa-c1d03d6c9fa9")
    {
        Entity = new()
        {
            Name = "Damp- og varmtvann",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till damp- og varmtvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:damp-varmtvann",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    /// <summary>
    /// Represents the Water Source, Treatment and Distribution package.
    /// </summary>
    public static ConstantDefinition<Package> WaterSourceTreatmentDistribution { get; } = new ConstantDefinition<Package>("e7cda008-f265-4452-b03c-c21b8b51dfe1")
    {
        Entity = new()
        {
            Name = "Vann - ta ut fra kilde, rense og distribuere",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till vann: ta ut fra kilde, rense og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:vann-kilde-rense-distrubere",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    /// <summary>
    /// Represents the Sewage Collection and Treatment package.
    /// </summary>
    public static ConstantDefinition<Package> SewageCollectionTreatment { get; } = new ConstantDefinition<Package>("fef4aac0-d227-4ef6-834b-cc2eb4b942ed")
    {
        Entity = new()
        {
            Name = "Samle opp og behandle avløpsvann",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till samle opp og behandle avløpsvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:samle-behandle-avlopsvann",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    /// <summary>
    /// Represents the Waste Collection, Treatment and Recycling package.
    /// </summary>
    public static ConstantDefinition<Package> WasteCollectionTreatmentRecycling { get; } = new ConstantDefinition<Package>("6dcffdde-cc5c-4b57-a5a2-cc5ae6ad44fc")
    {
        Entity = new()
        {
            Name = "Avfall - samle inn, behandle, bruke og gjenvinne",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till avfall: samle inn, behandle bruke og gjenvinne. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:avfall-behandle-gjenvinne",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    #endregion

    #region Additional Education Packages

    /// <summary>
    /// Represents the Kindergarten Owner package.
    /// </summary>
    public static ConstantDefinition<Package> KindergartenOwner { get; } = new ConstantDefinition<Package>("9a14f2c1-f0bc-4965-97c0-76a4e191bbe1")
    {
        Entity = new()
        {
            Name = "Barnehageeier",
            Description = "Denna fullmaktan gir tilgang till tjenester innen drift av barnehage som barnehageeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:barnehageeier",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the School Leader package.
    /// </summary>
    public static ConstantDefinition<Package> SchoolLeader { get; } = new ConstantDefinition<Package>("499d0e1c-18d7-4f21-b110-723e3d13003a")
    {
        Entity = new()
        {
            Name = "Skoleleder",
            Description = "Full tilgang til alle tjenester for skoleleder",
            Urn = "urn:altinn:accesspackage:schoolleader",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the School Owner package.
    /// </summary>
    public static ConstantDefinition<Package> SchoolOwner { get; } = new ConstantDefinition<Package>("32bfd131-1570-4b0c-888b-733cbb72d0cb")
    {
        Entity = new()
        {
            Name = "Skoleeier",
            Description = "Full tilgang til alle tjenester for skoleeier",
            Urn = "urn:altinn:accesspackage:schoolowner",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Governor School Education package.
    /// </summary>
    public static ConstantDefinition<Package> GovernorSchoolEducation { get; } = new ConstantDefinition<Package>("c1242df4-6af1-4e0b-b022-73c1099f5297")
    {
        Entity = new()
        {
            Name = "Fylkesmann - utdanning",
            Description = "Full tilgang til alle tjenester for fylkesmann innen utdanning",
            Urn = "urn:altinn:accesspackage:governorschooleducation",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Governor Kindergarten Authority package.
    /// </summary>
    public static ConstantDefinition<Package> GovernorKindergartenAuthority { get; } = new ConstantDefinition<Package>("7e6d8dd4-35a3-49d6-a2c5-73cd35018646")
    {
        Entity = new()
        {
            Name = "Fylkesmann - barnehager",
            Description = "Full tilgang til alle tjenester for fylkesmann innen barnehager",
            Urn = "urn:altinn:accesspackage:governorkindergartenauthority",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Kindergarten Authority package.
    /// </summary>
    public static ConstantDefinition<Package> KindergartenAuthority { get; } = new ConstantDefinition<Package>("0ecc481a-691c-496d-a3f2-748b8c450ed9")
    {
        Entity = new()
        {
            Name = "Barnehager",
            Description = "Full tilgang til alle tjenester for barnehager",
            Urn = "urn:altinn:accesspackage:kindergartenauthority",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    /// <summary>
    /// Represents the Kindergarten Leader package.
    /// </summary>
    public static ConstantDefinition<Package> KindergartenLeader { get; } = new ConstantDefinition<Package>("dcb57f3e-0e5b-4ef7-a10c-74c53bc5a90d")
    {
        Entity = new()
        {
            Name = "Barnehageleder",
            Description = "Full tilgang til alle tjenester for barnehageleder",
            Urn = "urn:altinn:accesspackage:kindergartenleader",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.ChildhoodAndEducation.Id,
        },
    };

    #endregion

    #region Professional Authorization Packages

    /// <summary>
    /// Represents the Accountant Payroll package.
    /// </summary>
    public static ConstantDefinition<Package> AccountantPayroll { get; } = new ConstantDefinition<Package>("ab656cf2-1e65-4b5a-a1e3-cd5bd4cb804c")
    {
        Entity = new()
        {
            Name = "Regnskapsfører lønn",
            Description = "Regnskapsfører lønn",
            Urn = "urn:altinn:accesspackage:accountantpayroll",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Responsible Auditor package.
    /// </summary>
    public static ConstantDefinition<Package> ResponsibleAuditor { get; } = new ConstantDefinition<Package>("ec492675-3a48-4ad9-b864-2d6865020642")
    {
        Entity = new()
        {
            Name = "Ansvarlig revisor",
            Description = "Ansvarlig revisor",
            Urn = "urn:altinn:accesspackage:responsibleauditor",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Auditor Employee package.
    /// </summary>
    public static ConstantDefinition<Package> AuditorEmployee { get; } = new ConstantDefinition<Package>("955d5779-3e2b-4098-b11d-0431dc41ddbe")
    {
        Entity = new()
        {
            Name = "Revisormedarbeider",
            Description = "Revisormedarbeider",
            Urn = "urn:altinn:accesspackage:auditoremployee",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    #endregion

    #region Additional Business Affairs Packages

    /// <summary>
    /// Represents the Population Registry package.
    /// </summary>
    public static ConstantDefinition<Package> PopulationRegistry { get; } = new ConstantDefinition<Package>("a5f7f72a-9b89-445d-85bb-06f678a3d4d1")
    {
        Entity = new()
        {
            Name = "Folkeregister",
            Description = "Folkeregister",
            Urn = "urn:altinn:accesspackage:populationregistry",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Central Coordination Register package.
    /// </summary>
    public static ConstantDefinition<Package> CentralCoordinationRegister { get; } = new ConstantDefinition<Package>("43becc6a-8c6c-4e9e-bb2f-08fe588ada21")
    {
        Entity = new()
        {
            Name = "Sentralt koordineringsregister",
            Description = "Sentralt koordineringsregister",
            Urn = "urn:altinn:accesspackage:centralcoordinationregister",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Personal Identity Registry package.
    /// </summary>
    public static ConstantDefinition<Package> PersonalIdentityRegistry { get; } = new ConstantDefinition<Package>("2f176732-b1e9-449b-9918-090d1fa986f6")
    {
        Entity = new()
        {
            Name = "Personnummer og identitetsforvaltning",
            Description = "Personnummer og identitetsforvaltning",
            Urn = "urn:altinn:accesspackage:personalidentityregistry",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Enforcement Officer package.
    /// </summary>
    public static ConstantDefinition<Package> EnforcementOfficer { get; } = new ConstantDefinition<Package>("96120c32-389d-46eb-8212-0a6540540c25")
    {
        Entity = new()
        {
            Name = "Namsmann",
            Description = "Namsmann",
            Urn = "urn:altinn:accesspackage:enforcementofficer",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Private Enforcement Officer package.
    /// </summary>
    public static ConstantDefinition<Package> PrivateEnforcementOfficer { get; } = new ConstantDefinition<Package>("5ef836c7-69cc-4ea8-84d6-fb933cc4fc5c")
    {
        Entity = new()
        {
            Name = "Privat namsmann",
            Description = "Privat namsmann",
            Urn = "urn:altinn:accesspackage:privateenforcementofficer",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    #endregion

    #region Integration Packages

    /// <summary>
    /// Represents the Machine Readable Events package.
    /// </summary>
    public static ConstantDefinition<Package> MachineReadableEvents { get; } = new ConstantDefinition<Package>("4cd69693-aff0-4e88-8b64-6b5620672468")
    {
        Entity = new()
        {
            Name = "Maskinlesbare hendelser",
            Description = "Maskinlesbare hendelser",
            Urn = "urn:altinn:accesspackage:machinereadableevents",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Altinn Events package.
    /// </summary>
    public static ConstantDefinition<Package> AltinnEvents { get; } = new ConstantDefinition<Package>("40397a93-b047-4011-a6b8-6b8af16b6324")
    {
        Entity = new()
        {
            Name = "Altinn Hendelser",
            Description = "Altinn Hendelser",
            Urn = "urn:altinn:accesspackage:altinnevents",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    #endregion

    #region Transportation and Infrastructure Packages

    /// <summary>
    /// Represents the Vessel Management Registry package.
    /// </summary>
    public static ConstantDefinition<Package> VesselManagementRegistry { get; } = new ConstantDefinition<Package>("70831546-6bfa-45a0-bdad-14e9db265847")
    {
        Entity = new()
        {
            Name = "Fartøyregistrering",
            Description = "Fartøyregistrering",
            Urn = "urn:altinn:accesspackage:vesselmanagementregistry",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    /// <summary>
    /// Represents the Shipping Company package.
    /// </summary>
    public static ConstantDefinition<Package> ShippingCompany { get; } = new ConstantDefinition<Package>("98c404f4-5350-42cd-86d0-15fd38f178c4")
    {
        Entity = new()
        {
            Name = "Skipsfartsselskap",
            Description = "Skipsfartsselskap",
            Urn = "urn:altinn:accesspackage:shippingcompany",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.TransportAndStorage.Id,
        },
    };

    #endregion

    #region Bankruptcy Estate Packages

    /// <summary>
    /// Represents the Bankruptcy Estate Read Access package.
    /// </summary>
    public static ConstantDefinition<Package> BankruptcyEstateReadAccess { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7e8c-8c8a-bd7c13a4f32e")
    {
        Entity = new()
        {
            Name = "Konkursbo lesetilgang",
            Description = "Denna fullmaktan gir bostyrer tilgang till å lese alt innhold som er rapportert til det offentlige for et konkursbo. Fullmaktan gis till bostyrer automatisk når oppstarten av konkursbo registreres i Konkursregisteret. Fullmaktan er personlig, og kan ikke delegeres videre.",
            Urn = "urn:altinn:accesspackage:konkursbo-lesetilgang",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AuthorizationsForBankruptcyEstates.Id,
        },
    };

    /// <summary>
    /// Represents the Bankruptcy Estate Write Access package.
    /// </summary>
    public static ConstantDefinition<Package> BankruptcyEstateWriteAccess { get; } = new ConstantDefinition<Package>("0e219609-02c6-44e6-9c80-fe2c1997940e")
    {
        Entity = new()
        {
            Name = "Konkursbo skrivetilgang",
            Description = "Denna fullmaktan gir bostyrers medhjelper tilgang till å jobbe på vegne av bostyrer. Bostyrer delegerer denna fullmaktan sammen med Konkursbo lesetilgang till medhjelper for hvert konkursbo.",
            Urn = "urn:altinn:accesspackage:konkursbo-skrivetilgang",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AuthorizationsForBankruptcyEstates.Id,
        },
    };

    #endregion

    #region Business Manager Authorizations

    /// <summary>
    /// Represents the Business Manager Real Estate package.
    /// </summary>
    public static ConstantDefinition<Package> BusinessManagerRealEstate { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7cf2-bcc8-720a3fb39d44")
    {
        Entity = new()
        {
            Name = "Forretningsfører eiendom",
            Description = "Denna fullmaktan gir forretningsfører for Borettslag og Eierseksjonssameie tilgang till å opptre på vegne av kunde, og utføre alla tjenester som krever denna fullmaktan. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en forretningsfører utfører på vegne av sin kunde. Fullmakt hos forretningsfører oppstår når Borettslaget eller Eierseksjonssameiet registrerer forretningsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:forretningsforer-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.AuthorizationsForBusinesses.Id,
        },
    };

    #endregion

    #region Additional Energy Packages

    /// <summary>
    /// Represents the Oil, Natural Gas and Coal Extraction package.
    /// </summary>
    public static ConstantDefinition<Package> OilNaturalGasCoalExtraction { get; } = new ConstantDefinition<Package>("452bd15d-2cd2-4279-9470-cd97ba8ef1c7")
    {
        Entity = new()
        {
            Name = "Utvinning av råolje, naturgass og kull",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till utvinning av råolje, naturgass og kull. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste.Id,
        },
    };

    #endregion

    #region Industry Packages

    /// <summary>
    /// Represents the Food, Beverages and Tobacco package.
    /// </summary>
    public static ConstantDefinition<Package> FoodBeveragesTobacco { get; } = new ConstantDefinition<Package>("65c7bbe7-aeb4-4f18-b0b0-1b1b83bd24d1")
    {
        Entity = new()
        {
            Name = "Næringsmidler, drikkevarer og tobakk",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med næringsmidler, drikkevarer og tobakk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Textiles, Clothing and Leather Goods package.
    /// </summary>
    public static ConstantDefinition<Package> TextilesClothingLeatherGoods { get; } = new ConstantDefinition<Package>("c6231d60-5373-4179-b98b-1e7eb83da474")
    {
        Entity = new()
        {
            Name = "Tekstiler, klær og lærvarer",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med tekstiler, klær og lærvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:tekstiler-klaer-laervarer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Lumber, Wood Products and Paper Goods package.
    /// </summary>
    public static ConstantDefinition<Package> LumberWoodProductsPaperGoods { get; } = new ConstantDefinition<Package>("deb1618f-395c-4a14-9a70-20e90e5f9a76")
    {
        Entity = new()
        {
            Name = "Trelast, trevarer og papirvarer",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med trelast, trevarer og papirvarer. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:trelast-trevarer-papirvarer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Printing and Recorded Media Reproduction package.
    /// </summary>
    public static ConstantDefinition<Package> PrintingRecordedMediaReproduction { get; } = new ConstantDefinition<Package>("965f9aca-48f5-4a16-b5e3-228806ad4fa7")
    {
        Entity = new()
        {
            Name = "Trykkerier og reproduksjon av innspilte opptak",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med trykkerier og reproduksjon av innspilte opptak. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:trykkerier-reproduksjon-opptak",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Oil Refining, Chemical and Pharmaceutical Industry package.
    /// </summary>
    public static ConstantDefinition<Package> OilRefiningChemicalPharmaceuticalIndustry { get; } = new ConstantDefinition<Package>("79203df9-d71b-460b-a828-22b0bf79f335")
    {
        Entity = new()
        {
            Name = "Oljeraffinering, kjemisk og farmasøytisk industri",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med oljeraffinering, kjemisk og farmasøytisk industri. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Rubber, Plastic and Non-metallic Mineral Products package.
    /// </summary>
    public static ConstantDefinition<Package> RubberPlasticNonMetallicMineralProducts { get; } = new ConstantDefinition<Package>("573be5d2-3ddd-4862-9711-2350147a1b25")
    {
        Entity = new()
        {
            Name = "Gummi, plast og ikke-metallholdige mineralprodukter",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med gummi, plast og ikke-metallholdige mineralprodukter. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Metals and Minerals package.
    /// </summary>
    public static ConstantDefinition<Package> MetalsAndMinerals { get; } = new ConstantDefinition<Package>("84e744a0-e93f-4caf-bb3f-24387f045d2d")
    {
        Entity = new()
        {
            Name = "Metaller og mineraler",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med metaller og mineraler. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:metaller-og-mineraler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Metal Products, Electrical Equipment and Machinery package.
    /// </summary>
    public static ConstantDefinition<Package> MetalProductsElectricalEquipmentMachinery { get; } = new ConstantDefinition<Package>("2857fd90-dad2-4cc3-9947-282c22f5d2dc")
    {
        Entity = new()
        {
            Name = "Metallvarer elektrisk utstyr og maskiner",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med metallvarer, elektrisk utstyr og maskiner. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Shipyards and Other Transport Vehicles package.
    /// </summary>
    public static ConstantDefinition<Package> ShipyardsOtherTransportVehicles { get; } = new ConstantDefinition<Package>("c37aabdf-a78a-4f59-999a-298a83e9e113")
    {
        Entity = new()
        {
            Name = "Verft og andre transportmidler",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med verft og andre transportmidler. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:verft-og-andre-transportmidler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Furniture and Other Manufacturing package.
    /// </summary>
    public static ConstantDefinition<Package> FurnitureOtherManufacturing { get; } = new ConstantDefinition<Package>("07bfd8a5-5b13-4937-84b9-2bd6ac726ea1")
    {
        Entity = new()
        {
            Name = "Møbler og annen industri",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med møbler og annen industri. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:mobler-og-annen-industri",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    /// <summary>
    /// Represents the Mining package.
    /// </summary>
    public static ConstantDefinition<Package> Mining { get; } = new ConstantDefinition<Package>("8c700369-8fff-40f1-b4ce-2f116416d804")
    {
        Entity = new()
        {
            Name = "Bergverk",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till industri i forbindelse med bergverk og tilhörande tjenester till bergverksdrift og utvinning. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:bergverk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.Industries.Id,
        },
    };

    #endregion

    #region Culture and Volunteering Packages

    /// <summary>
    /// Represents the Arts and Entertainment package.
    /// </summary>
    public static ConstantDefinition<Package> ArtsAndEntertainment { get; } = new ConstantDefinition<Package>("75fa5863-3368-4ac6-9a4b-48f595e483ad")
    {
        Entity = new()
        {
            Name = "Kunst og underholdning",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till kunstnerisk og underholdningsaktiviteter. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:kunst-og-underholdning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    /// <summary>
    /// Represents the Libraries, Museums, Archives and Other Culture package.
    /// </summary>
    public static ConstantDefinition<Package> LibrariesMuseumsArchivesOtherCulture { get; } = new ConstantDefinition<Package>("73418c0c-d4db-4e26-8581-4ccf1384aad7")
    {
        Entity = new()
        {
            Name = "Biblioteker, museer, arkiver og annen kultur",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till biblioteker, museer, arkiver, og annan kultur som botaniske og zoologiske hager, og drift av naturfenomener av historisk, kulturell eller undervisningsmessig interesse (f.eks. verdenskulturarv mv.). Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    /// <summary>
    /// Represents the Lottery and Games package.
    /// </summary>
    public static ConstantDefinition<Package> LotteryAndGames { get; } = new ConstantDefinition<Package>("cc5ccdec-6c67-4462-ab51-4d5eaafd64c1")
    {
        Entity = new()
        {
            Name = "Lotteri og spill",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till lotteri og spill, som f eks kasinoer, bingohaller og videospillhaller samt spillevirksomhet som f.eks. lotterier och veddemål som inngås utenfor banen. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:lotteri-og-spill",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    /// <summary>
    /// Represents the Sports and Recreation package.
    /// </summary>
    public static ConstantDefinition<Package> SportsAndRecreation { get; } = new ConstantDefinition<Package>("d2d00311-cc33-47ad-b33b-4eb15cce8d1d")
    {
        Entity = new()
        {
            Name = "Sport og fritid",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till sports- og fritidsaktiviteter. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:sport-og-fritid",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    /// <summary>
    /// Represents the Entertainment package.
    /// </summary>
    public static ConstantDefinition<Package> Entertainment { get; } = new ConstantDefinition<Package>("c82ab275-dad6-461a-bf0c-50be46b25ec9")
    {
        Entity = new()
        {
            Name = "Fornøyelser",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till drift av fornöyelsesetablissementer. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:fornoyelser",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    /// <summary>
    /// Represents the Politics package.
    /// </summary>
    public static ConstantDefinition<Package> Politics { get; } = new ConstantDefinition<Package>("f05153b5-4784-46f0-805a-525ed31fde3b")
    {
        Entity = new()
        {
            Name = "Politikk",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till aktiviteter i forbindelse med politisk arbeid. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:politikk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CultureAndVolunteering.Id,
        },
    };

    #endregion

    #region Commerce, Accommodation and Catering Packages

    /// <summary>
    /// Represents the Retail Trade package.
    /// </summary>
    public static ConstantDefinition<Package> RetailTrade { get; } = new ConstantDefinition<Package>("a6d704b7-d56b-4517-be79-0acd5b55b35e")
    {
        Entity = new()
        {
            Name = "Varehandel",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till varehandel, inkludert engros- og detaljhandel, import og eksport, og salg og reparasjon av motorvogner. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:varehandel",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CommerceAccommodationAndCatering.Id,
        },
    };

    /// <summary>
    /// Represents the Accommodation package.
    /// </summary>
    public static ConstantDefinition<Package> Accommodation { get; } = new ConstantDefinition<Package>("77cdd23a-dddf-43e6-b5c2-0d8299b0888c")
    {
        Entity = new()
        {
            Name = "Overnatting",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till overnattingsvirksomhet. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:overnatting",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CommerceAccommodationAndCatering.Id,
        },
    };

    /// <summary>
    /// Represents the Catering package.
    /// </summary>
    public static ConstantDefinition<Package> Catering { get; } = new ConstantDefinition<Package>("6710a7a0-78f9-47e3-bfa6-0d875869befd")
    {
        Entity = new()
        {
            Name = "Servering",
            Description = "Denna tilgangspakken gir fullmakter till tjenester knyttet till serveringsvirksomhet. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:servering",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.CommerceAccommodationAndCatering.Id,
        },
    };

    #endregion

    #region Other Service Industries Packages

    /// <summary>
    /// Represents the Postal and Telecommunications package.
    /// </summary>
    public static ConstantDefinition<Package> PostalAndTelecommunications { get; } = new ConstantDefinition<Package>("a736c33b-c15a-43ac-85be-a684630e1e59")
    {
        Entity = new()
        {
            Name = "Post- og telekommunikasjon",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till post og telekommunikasjon. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:post-og-telekommunikasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.OtherServiceIndustries.Id,
        },
    };

    /// <summary>
    /// Represents the Information and Communication package.
    /// </summary>
    public static ConstantDefinition<Package> InformationAndCommunication { get; } = new ConstantDefinition<Package>("413d79fb-a419-4e74-98f7-aa91389deb81")
    {
        Entity = new()
        {
            Name = "Informasjon og kommunikasjon",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till informasjon og kommunikasjon. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:informasjon-og-kommunikasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.OtherServiceIndustries.Id,
        },
    };

    /// <summary>
    /// Represents the Financing and Insurance package.
    /// </summary>
    public static ConstantDefinition<Package> FinancingAndInsurance { get; } = new ConstantDefinition<Package>("8f977b5f-a2f9-4712-88e2-ab1a51a6b26f")
    {
        Entity = new()
        {
            Name = "Finansiering og forsikring",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till finansiering og forsikring. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:finansiering-og-forsikring",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.OtherServiceIndustries.Id,
        },
    };

    /// <summary>
    /// Represents the Other Services package.
    /// </summary>
    public static ConstantDefinition<Package> OtherServices { get; } = new ConstantDefinition<Package>("a8b18888-2216-4eaa-972d-ae96da04b1ac")
    {
        Entity = new()
        {
            Name = "Annen tjenesteyting",
            Description = "Denna fullmaktan gir tilgang till tjenester knyttet till annan tjenesteyting som f eks organisasjoner og foreninger, reparasjon av datamaskiner och varer till personlig bruk och husholdningsbruk och en rekke personlige tjenester som ikke er nevnt annet sted. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:annen-tjenesteyting",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.OtherServiceIndustries.Id,
        },
    };

    #endregion

    #region Additional Business Affairs Packages

    /// <summary>
    /// Represents the Grant Support Compensation package.
    /// </summary>
    public static ConstantDefinition<Package> GrantSupportCompensation { get; } = new ConstantDefinition<Package>("d3ba7bba-195b-4b69-ae68-df20ecd57097")
    {
        Entity = new()
        {
            Name = "Tilskudd, støtte og erstatning",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke tilskudd, støtte og erstatning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:tilskudd-stotte-erstatning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Municipal Services package.
    /// </summary>
    public static ConstantDefinition<Package> MunicipalServices { get; } = new ConstantDefinition<Package>("600bf1be-61f6-423c-9a13-df93ee3214a5")
    {
        Entity = new()
        {
            Name = "Mine sider hos kommunen",
            Description = "Denne fullmaktan gir generell tilgang til tjenester av typen \"mine side\" tjenester hos kommuner. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:mine-sider-kommune",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Police and Court package.
    /// </summary>
    public static ConstantDefinition<Package> PoliceAndCourt { get; } = new ConstantDefinition<Package>("55e73960-09e5-473e-aa40-deaf97cf9bf2")
    {
        Entity = new()
        {
            Name = "Politi og domstol",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog om juridiske forhold med politi og jusitsmyndigheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:politi-og-domstol",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Statistics Reporting package.
    /// </summary>
    public static ConstantDefinition<Package> StatisticsReporting { get; } = new ConstantDefinition<Package>("ac54a5ca-16d2-4132-ae0d-e5aa8bd1ff6e")
    {
        Entity = new()
        {
            Name = "Rapportering av statistikk",
            Description = "Denna fullmaktan gir tilgang til alle pålagte rapportering av statistikk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:rapportering-statistikk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Research package.
    /// </summary>
    public static ConstantDefinition<Package> Research { get; } = new ConstantDefinition<Package>("594a2c4b-47a1-48c6-a01c-ed44ec4c05a4")
    {
        Entity = new()
        {
            Name = "Forskning",
            Description = "Denna fullmaktan gir tilgang til tjenester knyttet til forskning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:forskning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Explicit Service Delegation package.
    /// </summary>
    public static ConstantDefinition<Package> ExplicitServiceDelegation { get; } = new ConstantDefinition<Package>("c0eb20c1-2268-48f5-88c5-f26cb47a6b1f")
    {
        Entity = new()
        {
            Name = "Eksplisitt tjenestedelegering",
            Description = "Denna fullmaktan er ikke delegerbar, og er ikke knyttet til noen roller i ENhetsregisteret. Tilgang til tjenester knyttet til denna pakken kan gis av Huvudadministrator gjennom enkeltrettighetsdelegering.",
            Urn = "urn:altinn:accesspackage:eksplisitt",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Population Registry Business package.
    /// </summary>
    public static ConstantDefinition<Package> PopulationRegistryBusiness { get; } = new ConstantDefinition<Package>("5b51658e-f716-401c-9fc2-fe70bbe70f48")
    {
        Entity = new()
        {
            Name = "Folkeregister",
            Description = "Denna tilgangspakken gir fullmakt til tjenester som en virksomhet kan ha mot folkeregister. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:folkeregister",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Machine Readable Events Integration package.
    /// </summary>
    public static ConstantDefinition<Package> MachineReadableEventsIntegration { get; } = new ConstantDefinition<Package>("66bd8101-fbd8-491b-b1f5-2f53a9476ffd")
    {
        Entity = new()
        {
            Name = "Maskinlesbare hendelser",
            Description = "Denna tilgangspakken gir fullmakter til å administrere tilgang til maskinlesbare hendelser. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:maskinlesbare-hendelser",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.BusinessAffairs.Id,
        },
    };

    /// <summary>
    /// Represents the Care Services Institution package.
    /// </summary>
    public static ConstantDefinition<Package> CareServicesInstitution { get; } = new ConstantDefinition<Package>("4d71e7b8-c6eb-4e33-9a64-1279a509a53b")
    {
        Entity = new()
        {
            Name = "Pleie- og omsorgstjenester i institusjon",
            Description = "Denna tilgangspakken gir fullmakter til tjenester knyttet til pleie og omsorgstilbud i institusjon. Dette er tjenester som tilbyr institusjonsopphold kombinert med sykepleie, tilsyn eller annen form for pleie alt etter hva som kreves av beboerne. Denna fullmaktan kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innföring av nye digitale tjenester kan det bli endringer i tilganger som fullmaktan gir.",
            Urn = "urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
            AreaId = AreaConstants.HealthCareAndProtection.Id,
        },
    };

    #endregion
}
