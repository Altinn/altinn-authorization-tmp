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
    /// Try to get <see cref="Package"/> by Urn.
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

    #region Access Management Packages

    /// <summary>
    /// Represents the Client Administrator package ("Klientadministrator").
    /// </summary>
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
    /// Represents the Access Manager package ("Tilgangsstyrer").
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
    /// Represents the Main Administrator package ("Hovedadministrator").
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
    /// Represents the Maskinporten Administrator package ("Maskinporten administrator").
    /// </summary>
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

    #endregion

    #region Tax and Accounting Packages

    /// <summary>
    /// Represents the Business Tax package ("Skatt næring").
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
    /// Represents the Tax Base package ("Skattegrunnlag").
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
    /// Represents the Value Added Tax package ("Merverdiavgift").
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
    /// Represents the Accounting and Economic Reporting package ("Regnskap og økonomirapportering").
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

    #endregion

    #region Personnel Packages

    /// <summary>
    /// Represents the Employment Relations package ("Ansettelsesforhold").
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
    /// Represents the Salary package ("Lønn").
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
    /// Represents the A-ordningen package ("A-ordningen").
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

    #endregion
}