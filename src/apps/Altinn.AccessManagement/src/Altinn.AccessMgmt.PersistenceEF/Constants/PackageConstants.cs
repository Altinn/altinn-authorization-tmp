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

    #region Jordbruk, skogbruk, jakt, fiske og akvakultur

    /// <summary>
    /// Represents the 'Jordbruk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c5bbbc3f-605a-4dcb-a587-32124d7bb76d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:jordbruk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir tilgang til tjenester knyttet til jordbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Agriculture { get; } = new ConstantDefinition<Package>("c5bbbc3f-605a-4dcb-a587-32124d7bb76d")
    {
        Entity = new()
        {
            Name = "Jordbruk",
            Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til jordbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:jordbruk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Dyrehold' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 7b8a3aaa-c8ed-4ac4-923a-335f4f9eb45a</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:dyrehold</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir tilgang til tjenester knyttet til dyrehold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AnimalHusbandry { get; } = new ConstantDefinition<Package>("7b8a3aaa-c8ed-4ac4-923a-335f4f9eb45a")
    {
        Entity = new()
        {
            Name = "Dyrehold",
            Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til dyrehold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:dyrehold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Akvakultur' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 78c21107-7d2d-4e85-af82-47ea0e47ceca</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:akvakultur</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til akvakultur og fiskeoppdrett. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Aquaculture { get; } = new ConstantDefinition<Package>("78c21107-7d2d-4e85-af82-47ea0e47ceca")
    {
        Entity = new()
        {
            Name = "Akvakultur",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til akvakultur og fiskeoppdrett. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:akvakultur",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Fiske' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 9d2ec6e9-5148-4f47-9ae4-4536f6c9c1cb</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:fiske</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til fiske. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Fishing { get; } = new ConstantDefinition<Package>("9d2ec6e9-5148-4f47-9ae4-4536f6c9c1cb")
    {
        Entity = new()
        {
            Name = "Fiske",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til fiske. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:fiske",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Skogbruk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> f7e02568-90b6-477d-8abb-44984ddeb1f9</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:skogbruk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til skogbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Forestry { get; } = new ConstantDefinition<Package>("f7e02568-90b6-477d-8abb-44984ddeb1f9")
    {
        Entity = new()
        {
            Name = "Skogbruk",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til skogbruk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skogbruk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Jakt og viltstell' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 906aec0d-ad1f-496b-a0bb-40f81b3303cb</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:jakt-og-viltstell</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til jakt og viltstell. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> HuntingAndWildlifeManagement { get; } = new ConstantDefinition<Package>("906aec0d-ad1f-496b-a0bb-40f81b3303cb")
    {
        Entity = new()
        {
            Name = "Jakt og viltstell",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til jakt og viltstell. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:jakt-og-viltstell",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    /// <summary>
    /// Represents the 'Reindrift' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0bb5963e-df17-4f35-b913-3ce10a34b866</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:reindrift</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir tilgang til tjenester knyttet til reindrift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ReindeerHerding { get; } = new ConstantDefinition<Package>("0bb5963e-df17-4f35-b913-3ce10a34b866")
    {
        Entity = new()
        {
            Name = "Reindrift",
            Description = "Denne tilgangspakken gir tilgang til tjenester knyttet til reindrift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:reindrift",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AgricultureForestryHuntingFishingAndAquaculture,
        },
    };

    #endregion

    #region Fullmakter for regnskapsfører

    /// <summary>
    /// Represents the 'Regnskapsfører med signeringsrettighet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 955d5779-3e2b-4098-b11d-0431dc41ddbe</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til regnskapfører å kunne signere på vegne av kunden for alle tjenester som krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AuditorEmployee { get; } = new ConstantDefinition<Package>("955d5779-3e2b-4098-b11d-0431dc41ddbe")
    {
        Entity = new()
        {
            Name = "Regnskapsfører med signeringsrettighet",
            Description = "Denne fullmakten gir tilgang til regnskapfører å kunne signere på vegne av kunden for alle tjenester som krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:regnskapsforer-med-signeringsrettighet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForAccountants,
        },
    };

    /// <summary>
    /// Represents the 'Regnskapsfører lønn' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 43becc6a-8c6c-4e9e-bb2f-08fe588ada21</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:regnskapsforer-lonn</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til regnskapsfører å rapportere lønn for sin kunde. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> CentralCoordinationRegister { get; } = new ConstantDefinition<Package>("43becc6a-8c6c-4e9e-bb2f-08fe588ada21")
    {
        Entity = new()
        {
            Name = "Regnskapsfører lønn",
            Description = "Denne fullmakten gir tilgang til regnskapsfører å rapportere lønn for sin kunde. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:regnskapsforer-lonn",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForAccountants,
        },
    };

    /// <summary>
    /// Represents the 'Regnskapsfører uten signeringsrettighet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a5f7f72a-9b89-445d-85bb-06f678a3d4d1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til å kunne utføre alle tjenester som ikke krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PopulationRegistry { get; } = new ConstantDefinition<Package>("a5f7f72a-9b89-445d-85bb-06f678a3d4d1")
    {
        Entity = new()
        {
            Name = "Regnskapsfører uten signeringsrettighet",
            Description = "Denne fullmakten gir tilgang til å kunne utføre alle tjenester som ikke krever signeringsrett. Dette er tjenester som man har vurdert det som naturlig at en regnskapsfører utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte regnskapsførere. Fullmakt hos regnskapfører oppstår når kunden registrerer regnskapsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:regnskapsforer-uten-signeringsrettighet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForAccountants,
        },
    };

    #endregion

    #region Fullmakter for revisor

    /// <summary>
    /// Represents the 'Revisormedarbeider' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 96120c32-389d-46eb-8212-0a6540540c25</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:revisormedarbeider</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir revisor tilgang til å opptre som revisormedarbeider for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> EnforcementOfficer { get; } = new ConstantDefinition<Package>("96120c32-389d-46eb-8212-0a6540540c25")
    {
        Entity = new()
        {
            Urn = "urn:altinn:accesspackage:revisormedarbeider",
            Name = "Revisormedarbeider",
            Description = "Denne fullmakten gir revisor tilgang til å opptre som revisormedarbeider for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForAuditors
        },
    };

    /// <summary>
    /// Represents the 'Ansvarlig revisor' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 2f176732-b1e9-449b-9918-090d1fa986f6</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:ansvarlig-revisor</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir revisor tilgang til å opptre som ansvarlig revisor for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PersonalIdentityRegistry { get; } = new ConstantDefinition<Package>("2f176732-b1e9-449b-9918-090d1fa986f6")
    {
        Entity = new()
        {
            Name = "Ansvarlig revisor",
            Description = "Denne fullmakten gir revisor tilgang til å opptre som ansvarlig revisor for en kunde og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en revisor utfører på vegne av sin kunde. Fullmakten gis kun til autoriserte revisorer. Fullmakt hos revisor oppstår når kunden registrerer revisor i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:ansvarlig-revisor",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForAuditors,
        },
    };

    #endregion

    #region Fullmakter for konkursbo

    /// <summary>
    /// Represents the 'Konkursbo skrivetilgang' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0e219609-02c6-44e6-9c80-fe2c1997940e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:konkursbo-skrivetilgang</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir bostyrers medhjelper tilgang til å jobbe på vegne av bostyrer. Bostyrer delegerer denne fullmakten sammen med Konkursbo lesetilgang til medhjelper for hvert konkursbo.</para>
    /// </remarks>
    public static ConstantDefinition<Package> BankruptcyEstateWriteAccess { get; } = new ConstantDefinition<Package>("0e219609-02c6-44e6-9c80-fe2c1997940e")
    {
        Entity = new()
        {
            Name = "Konkursbo skrivetilgang",
            Description = "Denne fullmakten gir bostyrers medhjelper tilgang til å jobbe på vegne av bostyrer. Bostyrer delegerer denne fullmakten sammen med Konkursbo lesetilgang til medhjelper for hvert konkursbo.",
            Urn = "urn:altinn:accesspackage:konkursbo-skrivetilgang",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForBankruptcyEstates,
        },
    };

    /// <summary>
    /// Represents the 'Konkursbo lesetilgang' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 5ef836c7-69cc-4ea8-84d6-fb933cc4fc5c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:konkursbo-lesetilgang</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten delegeres til kreditorer og andre som skal ha lesetilgang til det enkelte konkursbo.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PrivateEnforcementOfficer { get; } = new ConstantDefinition<Package>("5ef836c7-69cc-4ea8-84d6-fb933cc4fc5c")
    {
        Entity = new()
        {
            Urn = "urn:altinn:accesspackage:konkursbo-lesetilgang",
            Name = "Konkursbo lesetilgang",
            Description = "Denne fullmakten delegeres til kreditorer og andre som skal ha lesetilgang til det enkelte konkursbo.",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForBankruptcyEstates,
        }
    };

    #endregion

    #region Fullmakter for forretningsfører

    /// <summary>
    /// Represents the 'Forretningsforer eiendom' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7cf2-bcc8-720a3fb39d44</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:forretningsforer-eiendom</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir forretningsfører for Borettslag og Eierseksjonssameie tilgang til å opptre på vegne av kunde, og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en forretningsfører utfører på vegne av sin kunde. Fullmakt hos forretningsfører oppstår når Borettslaget eller Eierseksjonssameiet registrerer forretningsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> BusinessManagerRealEstate { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7cf2-bcc8-720a3fb39d44")
    {
        Entity = new()
        {
            Urn = "urn:altinn:accesspackage:forretningsforer-eiendom",
            Name = "Forretningsforer eiendom",
            Description = "Denne fullmakten gir forretningsfører for Borettslag og Eierseksjonssameie tilgang til å opptre på vegne av kunde, og utføre alle tjenester som krever denne fullmakten. Dette er tjenester som tjenestetilbyder har vurdert det som naturlig at en forretningsfører utfører på vegne av sin kunde. Fullmakt hos forretningsfører oppstår når Borettslaget eller Eierseksjonssameiet registrerer forretningsfører i Enhetsregisteret. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.AuthorizationsForBusinesses,
        },
    };

    #endregion

    #region Forhold ved virksomheten

    /// <summary>
    /// Represents the 'Attester' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4717e6e0-1ec2-4354-b825-d9a9e2588fb1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:attester</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til attestering av virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Dokumentbasert tilsyn' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> df13707e-5252-496a-bc00-daffeed1e4b2</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:dokumentbasert-tilsyn</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til dokumentbaserte tilsyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Eksplisitt tjenestedelegering' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c0eb20c1-2268-48f5-88c5-f26cb47a6b1f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:eksplisitt</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten er ikke delegerbar, og er ikke knyttet til noen roller i ENhetsregisteret. Tilgang til tjenester knyttet til denne pakken kan gis av Hovedadministrator gjennom enkeltrettighetsdelegering.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ExplicitServiceDelegation { get; } = new ConstantDefinition<Package>("c0eb20c1-2268-48f5-88c5-f26cb47a6b1f")
    {
        Entity = new()
        {
            Name = "Eksplisitt tjenestedelegering",
            Description = "Denne fullmakten er ikke delegerbar, og er ikke knyttet til noen roller i ENhetsregisteret. Tilgang til tjenester knyttet til denne pakken kan gis av Hovedadministrator gjennom enkeltrettighetsdelegering.",
            Urn = "urn:altinn:accesspackage:eksplisitt",
            IsDelegable = false,
            HasResources = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Generelle helfotjenester' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 06c13400-8683-4985-9802-cef13e247f24</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:generelle-helfotjenester</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til ordinære tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Tilskudd, støtte og erstatning' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> d3ba7bba-195b-4b69-ae68-df20ecd57097</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:tilskudd-stotte-erstatning</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke tilskudd, støtte og erstatning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Helfotjenester med personopplysninger av særlig kategori' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 2ee6e972-2423-4f3a-bd3c-d4871fcc9876</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:helfo-saerlig-kategori</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog med Helfo der bruker kan få tilgang til personopplysninger av særlig kategori. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Infrastruktur' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 75978efe-2437-421e-8c77-dd61925c7ba4</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:infrastruktur</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens infrastruktur. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Mine sider hos kommunen' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 600bf1be-61f6-423c-9a13-df93ee3214a5</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:mine-sider-kommune</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir generell tilgang til tjenester av typen “mine side” tjenester hos kommuner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MunicipalServices { get; } = new ConstantDefinition<Package>("600bf1be-61f6-423c-9a13-df93ee3214a5")
    {
        Entity = new()
        {
            Name = "Mine sider hos kommunen",
            Description = "Denne fullmakten gir generell tilgang til tjenester av typen “mine side” tjenester hos kommuner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:mine-sider-kommune",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Patent, varemerke og design' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 250568e6-ed9f-4bdc-b9e3-df08be7181ba</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:patent-varemerke-design</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Politi og domstol' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 55e73960-09e5-473e-aa40-deaf97cf9bf2</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:politi-og-domstol</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til virksomhetens dialog om juridiske forhold med politi og jusitsmyndigheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Folkeregister' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 5b51658e-f716-401c-9fc2-fe70bbe70f48</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:folkeregister</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakt til tjenester som en virksomhet kan ha mot folkeregister. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PopulationRegistryBusiness { get; } = new ConstantDefinition<Package>("5b51658e-f716-401c-9fc2-fe70bbe70f48")
    {
        Entity = new()
        {
            Name = "Folkeregister",
            Description = "Denne tilgangspakken gir fullmakt til tjenester som en virksomhet kan ha mot folkeregister. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:folkeregister",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Forskning' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 594a2c4b-47a1-48c6-a01c-ed44ec4c05a4</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:forskning</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til forskning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Research { get; } = new ConstantDefinition<Package>("594a2c4b-47a1-48c6-a01c-ed44ec4c05a4")
    {
        Entity = new()
        {
            Name = "Forskning",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til forskning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:forskning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Aksjer og eierforhold' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 7dfc669b-70e4-4974-9e11-d6dca4803aaa</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:aksjer-og-eierforhold</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til aksjer og eierforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Starte, endre og avvikle virksomhet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 956cfcf0-ab4c-44d8-98a2-d68c4d59321b</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:starte-drive-endre-avikle-virksomhet</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til å starte, endre og avvikle en virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    /// <summary>
    /// Represents the 'Rapportering av statistikk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> ac54a5ca-16d2-4132-ae0d-e5aa8bd1ff6e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:rapportering-statistikk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til alle pålagte rapportering av statistikk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> StatisticsReporting { get; } = new ConstantDefinition<Package>("ac54a5ca-16d2-4132-ae0d-e5aa8bd1ff6e")
    {
        Entity = new()
        {
            Name = "Rapportering av statistikk",
            Description = "Denne fullmakten gir tilgang til alle pålagte rapportering av statistikk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:rapportering-statistikk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.BusinessAffairs,
        },
    };

    #endregion

    #region Oppvekst og utdanning

    /// <summary>
    /// Represents the 'SFO-leder' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 219ea5f7-eb30-424f-a958-67d3c7e7a4c2</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sfo-leder</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til førskole og fritidsordning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AfterSchoolProgramLeader { get; } = new ConstantDefinition<Package>("219ea5f7-eb30-424f-a958-67d3c7e7a4c2")
    {
        Entity = new()
        {
            Name = "SFO-leder",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til førskole og fritidsordning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sfo-leder",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Godkjenning av utdanningsvirksomhet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> b56d2614-3f9d-4d93-8a8e-64d80b654ad7</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning av utdanningsvirksomheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> EducationalInstitutionApproval { get; } = new ConstantDefinition<Package>("b56d2614-3f9d-4d93-8a8e-64d80b654ad7")
    {
        Entity = new()
        {
            Name = "Godkjenning av utdanningsvirksomhet",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning av utdanningsvirksomheter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:godkjenning-av-utdanningsvirksomhet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Statsforvalter - barnehage' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 7e6d8dd4-35a3-49d6-a2c5-73cd35018646</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:statsforvalter-barnehage</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til barnehagesektor.</para>
    /// </remarks>
    public static ConstantDefinition<Package> GovernorKindergartenAuthority { get; } = new ConstantDefinition<Package>("7e6d8dd4-35a3-49d6-a2c5-73cd35018646")
    {
        Entity = new()
        {
            Name = "Statsforvalter - barnehage",
            Description = "Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til barnehagesektor.",
            Urn = "urn:altinn:accesspackage:statsforvalter-barnehage",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Statsforvalter - skole og opplæring' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c1242df4-6af1-4e0b-b022-73c1099f5297</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:statsforvalter-skole-og-opplearing</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til skole- og opplæringssektor, herunder fagopplæring og voksenopplæring.</para>
    /// </remarks>
    public static ConstantDefinition<Package> GovernorSchoolEducation { get; } = new ConstantDefinition<Package>("c1242df4-6af1-4e0b-b022-73c1099f5297")
    {
        Entity = new()
        {
            Name = "Statsforvalter - skole og opplæring",
            Description = "Denne fullmakten gir tilgang til tjenester for statsforvalter knyttet til skole- og opplæringssektor, herunder fagopplæring og voksenopplæring.",
            Urn = "urn:altinn:accesspackage:statsforvalter-skole-og-opplearing",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Høyere utdanning og høyere yrkesfaglig utdanning' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 93f37a3d-f799-47b3-bc8e-675b5813abf4</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til høyere utdanning og høyere yrkesfaglig utdanning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> HigherEducation { get; } = new ConstantDefinition<Package>("93f37a3d-f799-47b3-bc8e-675b5813abf4")
    {
        Entity = new()
        {
            Name = "Høyere utdanning og høyere yrkesfaglig utdanning",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til høyere utdanning og høyere yrkesfaglig utdanning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:hoyere-utdanning-og-hoyere-yrkesfaglig-utdanning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Barnehagemyndighet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0ecc481a-691c-496d-a3f2-748b8c450ed9</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:barnehagemyndighet</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehagemyndighet er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> KindergartenAuthority { get; } = new ConstantDefinition<Package>("0ecc481a-691c-496d-a3f2-748b8c450ed9")
    {
        Entity = new()
        {
            Name = "Barnehagemyndighet",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehagemyndighet er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:barnehagemyndighet",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Barnehageleder' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> dcb57f3e-0e5b-4ef7-a10c-74c53bc5a90d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:barnehageleder</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> KindergartenLeader { get; } = new ConstantDefinition<Package>("dcb57f3e-0e5b-4ef7-a10c-74c53bc5a90d")
    {
        Entity = new()
        {
            Name = "Barnehageleder",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:barnehageleder",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Barnehageeier' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 9a14f2c1-f0bc-4965-97c0-76a4e191bbe1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:barnehageeier</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> KindergartenOwner { get; } = new ConstantDefinition<Package>("9a14f2c1-f0bc-4965-97c0-76a4e191bbe1")
    {
        Entity = new()
        {
            Name = "Barnehageeier",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av barnehage som barnehageeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:barnehageeier",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Opplæringskontorleder' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 40397a93-b047-4011-a6b8-6b8af16b6324</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:opplaeringskontorleder</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av opplæringskontor som opplæringskontorleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> OfficeLeader { get; } = new ConstantDefinition<Package>("40397a93-b047-4011-a6b8-6b8af16b6324")
    {
        Entity = new()
        {
            Urn = "urn:altinn:accesspackage:opplaeringskontorleder",
            Name = "Opplæringskontorleder",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av opplæringskontor som opplæringskontorleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        }
    };

    /// <summary>
    /// Represents the 'PPT-leder' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4cd69693-aff0-4e88-8b64-6b5620672468</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:ppt-leder</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av Pedagogisk-psykologisk tjeneste (PPT) som PPT-leder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PPTLeader { get; } = new ConstantDefinition<Package>("4cd69693-aff0-4e88-8b64-6b5620672468")
    {
        Entity = new()
        {
            Urn = "urn:altinn:accesspackage:ppt-leder",
            Name = "PPT-leder",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av Pedagogisk-psykologisk tjeneste (PPT) som PPT-leder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Godkjenning av personell' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 8cf4ec08-90dc-47d0-93f8-64a50c9b38b0</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:godkjenning-av-personell</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning til enkeltpersoner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PersonnelApproval { get; } = new ConstantDefinition<Package>("8cf4ec08-90dc-47d0-93f8-64a50c9b38b0")
    {
        Entity = new()
        {
            Name = "Godkjenning av personell",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til å søke om godkjenning til enkeltpersoner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:godkjenning-av-personell",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Skoleleder' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 499d0e1c-18d7-4f21-b110-723e3d13003a</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:skoleleder</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SchoolLeader { get; } = new ConstantDefinition<Package>("499d0e1c-18d7-4f21-b110-723e3d13003a")
    {
        Entity = new()
        {
            Name = "Skoleleder",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleleder er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skoleleder",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    /// <summary>
    /// Represents the 'Skoleeier' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 32bfd131-1570-4b0c-888b-733cbb72d0cb</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:skoleeier</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SchoolOwner { get; } = new ConstantDefinition<Package>("32bfd131-1570-4b0c-888b-733cbb72d0cb")
    {
        Entity = new()
        {
            Name = "Skoleeier",
            Description = "Denne fullmakten gir tilgang til tjenester innen drift av skole som skoleeier er ansvarlig for. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:skoleeier",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ChildhoodAndEducation,
        },
    };

    #endregion

    #region Handel, overnatting og servering

    /// <summary>
    /// Represents the 'Overnatting' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 77cdd23a-dddf-43e6-b5c2-0d8299b0888c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:overnatting</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til overnattingsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Accommodation { get; } = new ConstantDefinition<Package>("77cdd23a-dddf-43e6-b5c2-0d8299b0888c")
    {
        Entity = new()
        {
            Name = "Overnatting",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til overnattingsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:overnatting",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CommerceAccommodationAndCatering,
        },
    };

    /// <summary>
    /// Represents the 'Servering' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 6710a7a0-78f9-47e3-bfa6-0d875869befd</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:servering</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til serveringsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Catering { get; } = new ConstantDefinition<Package>("6710a7a0-78f9-47e3-bfa6-0d875869befd")
    {
        Entity = new()
        {
            Name = "Servering",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til serveringsvirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:servering",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CommerceAccommodationAndCatering,
        },
    };

    /// <summary>
    /// Represents the 'Varehandel' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a6d704b7-d56b-4517-be79-0acd5b55b35e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:varehandel</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til varehandel, inkludert engros- og detaljhandel, import og eksport, og salg og reparasjon av motorvogner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RetailTrade { get; } = new ConstantDefinition<Package>("a6d704b7-d56b-4517-be79-0acd5b55b35e")
    {
        Entity = new()
        {
            Name = "Varehandel",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til varehandel, inkludert engros- og detaljhandel, import og eksport, og salg og reparasjon av motorvogner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:varehandel",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CommerceAccommodationAndCatering,
        },
    };

    #endregion

    #region Bygg, anlegg og eiendom

    /// <summary>
    /// Represents the 'Byggesøknad' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 1e40697b-e178-4920-8fbd-af5164b4d147</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:byggesoknad</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester som ansvarlig søker/tiltakshaver trenger, for eksempel byggesøknader, direkte signerte erklæringer, nabovarsel og eiendomssak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> BuildingApplication { get; } = new ConstantDefinition<Package>("1e40697b-e178-4920-8fbd-af5164b4d147")
    {
        Entity = new()
        {
            Name = "Byggesøknad",
            Description = "Denne tilgangspakken gir fullmakter til tjenester som ansvarlig søker/tiltakshaver trenger, for eksempel byggesøknader, direkte signerte erklæringer, nabovarsel og eiendomssak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:byggesoknad",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Kjøp og salg av eiendom' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> b98ec05d-1ac5-4ced-8250-b6d75b83502b</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:kjop-og-salg-eiendom</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til kjøp og salg av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> BuyingSellungRealEstate { get; } = new ConstantDefinition<Package>("b98ec05d-1ac5-4ced-8250-b6d75b83502b")
    {
        Entity = new()
        {
            Name = "Kjøp og salg av eiendom",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kjøp og salg av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:kjop-og-salg-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Oppføring av bygg og anlegg' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> ac655a2f-be29-4888-9a6c-b21e524fa90e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:oppforing-bygg-anlegg</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester relatert til oppføring av bygninger og annlegg unntatt plan og byggesaksbehandling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ConstructionBuildingsFacilities { get; } = new ConstantDefinition<Package>("ac655a2f-be29-4888-9a6c-b21e524fa90e")
    {
        Entity = new()
        {
            Name = "Oppføring av bygg og anlegg",
            Description = "Denne tilgangspakken gir fullmakter til tjenester relatert til oppføring av bygninger og annlegg unntatt plan og byggesaksbehandling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:oppforing-bygg-anlegg",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Plansak' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 482eee1e-ec79-45bb-8bd0-af8459cbb9f0</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:plansak</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester som forslagsstiller/ plankonsulent trenger, for eksempel varsel om planopppstart og høring og offentlig ettersyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PlanningCase { get; } = new ConstantDefinition<Package>("482eee1e-ec79-45bb-8bd0-af8459cbb9f0")
    {
        Entity = new()
        {
            Name = "Plansak",
            Description = "Denne tilgangspakken gir fullmakter til tjenester som forslagsstiller/ plankonsulent trenger, for eksempel varsel om planopppstart og høring og offentlig ettersyn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:plansak",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Tinglysing eiendom' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7642-b9b8-c748ee4fecd4</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:tinglysing-eiendom</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til elektronisk tinglysing av rettigheter i eiendom.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PropertyRegistration { get; } = new ConstantDefinition<Package>("0195efb8-7c80-7642-b9b8-c748ee4fecd4")
    {
        Entity = new()
        {
            Name = "Tinglysing eiendom",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til elektronisk tinglysing av rettigheter i eiendom.",
            Urn = "urn:altinn:accesspackage:tinglysing-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Eiendomsmegler' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 79329c4a-d856-4491-965e-bcc4ed5a7453</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:eiendomsmegler</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til omsetning og drift av fast eiendom på oppdrag, som eiendomsmegling og eiendomsforvaltning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RealEstateAgent { get; } = new ConstantDefinition<Package>("79329c4a-d856-4491-965e-bcc4ed5a7453")
    {
        Entity = new()
        {
            Name = "Eiendomsmegler",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til omsetning og drift av fast eiendom på oppdrag, som eiendomsmegling og eiendomsforvaltning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:eiendomsmegler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Utleie av eiendom' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 565a3110-3d51-4e72-ae4c-b89308e5c96e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:utleie-eiendom</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til utleie av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RealEstateRental { get; } = new ConstantDefinition<Package>("565a3110-3d51-4e72-ae4c-b89308e5c96e")
    {
        Entity = new()
        {
            Name = "Utleie av eiendom",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til utleie av eiendom. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:utleie-eiendom",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    /// <summary>
    /// Represents the 'Motta nabo- og planvarsel' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 1e1e8bf9-096f-4de2-9db7-b176db55db09</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:motta-nabo-og-planvarsel</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester til å lese og svare på varsel om plan-/byggesak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ReceiveNeighborPlanNotifications { get; } = new ConstantDefinition<Package>("1e1e8bf9-096f-4de2-9db7-b176db55db09")
    {
        Entity = new()
        {
            Name = "Motta nabo- og planvarsel",
            Description = "Denne tilgangspakken gir fullmakter til tjenester til å lese og svare på varsel om plan-/byggesak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:motta-nabo-og-planvarsel",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ConstructionInfrastructureAndRealEstate,
        },
    };

    #endregion

    #region Kultur og frivillighet

    /// <summary>
    /// Represents the 'Kunst og underholdning' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 75fa5863-3368-4ac6-9a4b-48f595e483ad</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:kunst-og-underholdning</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til kunstnerisk og underholdningsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ArtsAndEntertainment { get; } = new ConstantDefinition<Package>("75fa5863-3368-4ac6-9a4b-48f595e483ad")
    {
        Entity = new()
        {
            Name = "Kunst og underholdning",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til kunstnerisk og underholdningsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:kunst-og-underholdning",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    /// <summary>
    /// Represents the 'Fornøyelser' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c82ab275-dad6-461a-bf0c-50be46b25ec9</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:fornoyelser</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til drift av fornøyelsesetablissementer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Entertainment { get; } = new ConstantDefinition<Package>("c82ab275-dad6-461a-bf0c-50be46b25ec9")
    {
        Entity = new()
        {
            Name = "Fornøyelser",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til drift av fornøyelsesetablissementer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:fornoyelser",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    /// <summary>
    /// Represents the 'Biblioteker, museer, arkiver og annen kultur' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 73418c0c-d4db-4e26-8581-4ccf1384aad7</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til biblioteker, museer, arkiver, og annen kultur som botaniske og zoologiske hager, og drift av naturfenomener av historisk, kulturell eller undervisningsmessig interesse (f.eks. verdenskulturarv mv.). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> LibrariesMuseumsArchivesOtherCulture { get; } = new ConstantDefinition<Package>("73418c0c-d4db-4e26-8581-4ccf1384aad7")
    {
        Entity = new()
        {
            Name = "Biblioteker, museer, arkiver og annen kultur",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til biblioteker, museer, arkiver, og annen kultur som botaniske og zoologiske hager, og drift av naturfenomener av historisk, kulturell eller undervisningsmessig interesse (f.eks. verdenskulturarv mv.). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:biblioteker-museer-arkiver-og-annen-kultur",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    /// <summary>
    /// Represents the 'Lotteri og spill' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> cc5ccdec-6c67-4462-ab51-4d5eaafd64c1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:lotteri-og-spill</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til lotteri og spill, som f eks kasinoer, bingohaller og videospillhaller samt spillevirksomhet som f.eks. lotterier og veddemål som inngås utenfor banen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> LotteryAndGames { get; } = new ConstantDefinition<Package>("cc5ccdec-6c67-4462-ab51-4d5eaafd64c1")
    {
        Entity = new()
        {
            Name = "Lotteri og spill",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til lotteri og spill, som f eks kasinoer, bingohaller og videospillhaller samt spillevirksomhet som f.eks. lotterier og veddemål som inngås utenfor banen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:lotteri-og-spill",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    /// <summary>
    /// Represents the 'Politikk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> f05153b5-4784-46f0-805a-525ed31fde3b</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:politikk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til aktiviteter i forbindelse med politisk arbeid. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Politics { get; } = new ConstantDefinition<Package>("f05153b5-4784-46f0-805a-525ed31fde3b")
    {
        Entity = new()
        {
            Name = "Politikk",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til aktiviteter i forbindelse med politisk arbeid. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:politikk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    /// <summary>
    /// Represents the 'Sport og fritid' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> d2d00311-cc33-47ad-b33b-4eb15cce8d1d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sport-og-fritid</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sports- og fritidsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SportsAndRecreation { get; } = new ConstantDefinition<Package>("d2d00311-cc33-47ad-b33b-4eb15cce8d1d")
    {
        Entity = new()
        {
            Name = "Sport og fritid",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sports- og fritidsaktiviteter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sport-og-fritid",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.CultureAndVolunteering,
        },
    };

    #endregion

    #region Energi, vann, avløp og avfall

    /// <summary>
    /// Represents the 'Miljørydding - rensing og lignende virksomhet' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> ab656cf2-1e65-4b5a-a1e3-cd5bd4cb804c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:miljorydding-rensing</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til miljøryddng, -rensing og lignende virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AccountantPayroll { get; } = new ConstantDefinition<Package>("ab656cf2-1e65-4b5a-a1e3-cd5bd4cb804c")
    {
        Entity = new()
        {
            Name = "Miljørydding - rensing og lignende virksomhet",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til miljøryddng, -rensing og lignende virksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:miljorydding-rensing",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Elektrisitet - produsere, overføre og distribuere' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> f56bdb15-686b-46f8-9343-bde5d7c17648</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til elektrisitet: produsere, overføre og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ElectricityProductionTransmissionDistribution { get; } = new ConstantDefinition<Package>("f56bdb15-686b-46f8-9343-bde5d7c17648")
    {
        Entity = new()
        {
            Name = "Elektrisitet - produsere, overføre og distribuere",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til elektrisitet: produsere, overføre og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:elektrisitet-produsere-overfore-distrubere",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Utvinning av råolje, naturgass og kull' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 452bd15d-2cd2-4279-9470-cd97ba8ef1c7</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til utvinning av råolje, naturgass og kull. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> OilNaturalGasCoalExtraction { get; } = new ConstantDefinition<Package>("452bd15d-2cd2-4279-9470-cd97ba8ef1c7")
    {
        Entity = new()
        {
            Name = "Utvinning av råolje, naturgass og kull",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til utvinning av råolje, naturgass og kull. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:utvinning-raaolje-naturgass-kull",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Samle opp og behandle avløpsvann' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> fef4aac0-d227-4ef6-834b-cc2eb4b942ed</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:samle-behandle-avlopsvann</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til samle opp og behandle avløpsvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SewageCollectionTreatment { get; } = new ConstantDefinition<Package>("fef4aac0-d227-4ef6-834b-cc2eb4b942ed")
    {
        Entity = new()
        {
            Name = "Samle opp og behandle avløpsvann",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til samle opp og behandle avløpsvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:samle-behandle-avlopsvann",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Damp- og varmtvann' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 27afe398-b25b-4287-b0fa-c1d03d6c9fa9</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:damp-varmtvann</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til damp- og varmtvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SteamAndHotWater { get; } = new ConstantDefinition<Package>("27afe398-b25b-4287-b0fa-c1d03d6c9fa9")
    {
        Entity = new()
        {
            Name = "Damp- og varmtvann",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til damp- og varmtvann. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:damp-varmtvann",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Avfall - samle inn, behandle, bruke og gjenvinne' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 6dcffdde-cc5c-4b57-a5a2-cc5ae6ad44fc</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:avfall-behandle-gjenvinne</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til avfall: samle inn, behandle bruke og gjenvinne. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> WasteCollectionTreatmentRecycling { get; } = new ConstantDefinition<Package>("6dcffdde-cc5c-4b57-a5a2-cc5ae6ad44fc")
    {
        Entity = new()
        {
            Name = "Avfall - samle inn, behandle, bruke og gjenvinne",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til avfall: samle inn, behandle bruke og gjenvinne. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:avfall-behandle-gjenvinne",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    /// <summary>
    /// Represents the 'Vann - ta ut fra kilde, rense og distribuere' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> e7cda008-f265-4452-b03c-c21b8b51dfe1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:vann-kilde-rense-distrubere</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til vann: ta ut fra kilde, rense og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> WaterSourceTreatmentDistribution { get; } = new ConstantDefinition<Package>("e7cda008-f265-4452-b03c-c21b8b51dfe1")
    {
        Entity = new()
        {
            Name = "Vann - ta ut fra kilde, rense og distribuere",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til vann: ta ut fra kilde, rense og distribuere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:vann-kilde-rense-distrubere",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnergyWaterSewageAndWaste,
        },
    };

    #endregion

    #region Miljø, ulykke og sikkerhet

    /// <summary>
    /// Represents the 'Ulykke' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a03af7d5-74b9-4f18-aead-5d47edc36be5</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:ulykke</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til ulykke. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Accident { get; } = new ConstantDefinition<Package>("a03af7d5-74b9-4f18-aead-5d47edc36be5")
    {
        Entity = new()
        {
            Name = "Ulykke",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til ulykke. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:ulykke",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    /// <summary>
    /// Represents the 'Miljørydding, miljørensing og lignende' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 04c5a001-5249-4765-ae8e-58617c404223</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til miljørydding, miljørensing og lignende. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> EnvironmentalCleanup { get; } = new ConstantDefinition<Package>("04c5a001-5249-4765-ae8e-58617c404223")
    {
        Entity = new()
        {
            Name = "Miljørydding, miljørensing og lignende",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til miljørydding, miljørensing og lignende. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:miljorydding-miljorensing-og-lignende",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    /// <summary>
    /// Represents the 'Yrkesskade' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> fa84bffc-ac17-40cd-af9c-61c89f92e44c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:yrkesskade</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til yrkesskade. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> OccupationalInjury { get; } = new ConstantDefinition<Package>("fa84bffc-ac17-40cd-af9c-61c89f92e44c")
    {
        Entity = new()
        {
            Name = "Yrkesskade",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til yrkesskade. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:yrkesskade",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    /// <summary>
    /// Represents the 'Sikkerhet og internkontroll' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> cfe074fa-0a66-4a4b-974a-5d1db8eb94e6</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sikkerhet-og-internkontroll</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sikkerhet og internkontroll. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SecurityAndInternalControl { get; } = new ConstantDefinition<Package>("cfe074fa-0a66-4a4b-974a-5d1db8eb94e6")
    {
        Entity = new()
        {
            Name = "Sikkerhet og internkontroll",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sikkerhet og internkontroll. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sikkerhet-og-internkontroll",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    /// <summary>
    /// Represents the 'Bærekraft' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> bacc9294-56fd-457f-930e-59ee4a7a3894</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:baerekraft</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til tiltak og rapportering på bærekraft. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Sustainability { get; } = new ConstantDefinition<Package>("bacc9294-56fd-457f-930e-59ee4a7a3894")
    {
        Entity = new()
        {
            Name = "Bærekraft",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til tiltak og rapportering på bærekraft. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:baerekraft",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    /// <summary>
    /// Represents the 'Renovasjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 5eb07bdc-5c3c-4c85-add3-5405b214b8a3</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:renovasjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til renovasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> WasteManagement { get; } = new ConstantDefinition<Package>("5eb07bdc-5c3c-4c85-add3-5405b214b8a3")
    {
        Entity = new()
        {
            Name = "Renovasjon",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til renovasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:renovasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.EnvironmentAccidentAndSafety,
        },
    };

    #endregion

    #region Helse, pleie, omsorg og vern

    /// <summary>
    /// Represents the 'Pleie- og omsorgstjenester i institusjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4d71e7b8-c6eb-4e33-9a64-1279a509a53b</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til pleie og omsorgstilbud i institursjon. Dette er tjenester som tilbyr institusjonsopphold kombinert med sykepleie, tilsyn eller annen form for pleie alt etter hva som kreves av beboerne. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> CareServicesInstitution { get; } = new ConstantDefinition<Package>("4d71e7b8-c6eb-4e33-9a64-1279a509a53b")
    {
        Entity = new()
        {
            Name = "Pleie- og omsorgstjenester i institusjon",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til pleie og omsorgstilbud i institursjon. Dette er tjenester som tilbyr institusjonsopphold kombinert med sykepleie, tilsyn eller annen form for pleie alt etter hva som kreves av beboerne. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:pleie-omsorgstjenester-i-institusjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Barnevern' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 83ff0734-0de5-4c2a-939b-18a9452c00bc</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:barnevern</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til barnevern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ChildProtection { get; } = new ConstantDefinition<Package>("83ff0734-0de5-4c2a-939b-18a9452c00bc")
    {
        Entity = new()
        {
            Name = "Barnevern",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til barnevern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.",
            Urn = "urn:altinn:accesspackage:barnevern",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Familievern' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 98c404f4-5350-42cd-86d0-15fd38f178c4</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:familievern</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til familievern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.</para>
    /// </remarks>
    public static ConstantDefinition<Package> FamilyCounseling { get; } = new ConstantDefinition<Package>("98c404f4-5350-42cd-86d0-15fd38f178c4")
    {
        Entity = new()
        {
            Name = "Familievern",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til familievern. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir. OBS! Vær oppmerksom på at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.",
            Urn = "urn:altinn:accesspackage:familievern",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Helsetjenester' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4fa0fbbc-3841-4405-9d94-12731a8fdb81</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:helsetjenester</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> HealthServices { get; } = new ConstantDefinition<Package>("4fa0fbbc-3841-4405-9d94-12731a8fdb81")
    {
        Entity = new()
        {
            Name = "Helsetjenester",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:helsetjenester",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Helsetjenester med personopplysninger av særlig kategori' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 5a642040-46cf-4466-b671-115a022e3048</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er av særlig kategori. Denne fullmakten kan gi bruker tilgang til sensitive personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> HealthServicesSpecialCategory { get; } = new ConstantDefinition<Package>("5a642040-46cf-4466-b671-115a022e3048")
    {
        Entity = new()
        {
            Name = "Helsetjenester med personopplysninger av særlig kategori",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er av særlig kategori. Denne fullmakten kan gi bruker tilgang til sensitive personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:helsetjenester-personopplysinger-saerlig-kategori",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Kommuneoverlege' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 03404270-aa2d-498f-9e6a-103043d41f1f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:kommuneoverlege</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er relevant for kommuneleger. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MunicipalDoctor { get; } = new ConstantDefinition<Package>("03404270-aa2d-498f-9e6a-103043d41f1f")
    {
        Entity = new()
        {
            Name = "Kommuneoverlege",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sykehus, lege, tannlege og hjemmesykepleie,fysioterapi, ambulanse og lignende, som er relevant for kommuneleger. Denne fullmakten kan gi bruker tilgang til personopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:kommuneoverlege",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    /// <summary>
    /// Represents the 'Sosiale omsorgstjenester uten botilbud og flyktningemottak' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 70831546-6bfa-45a0-bdad-14e9db265847</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sosiale omsorgstjeneser uten botilbud for eldre, funksjonshemmede og rusmisbrukere samt flykningemottak, og tjenester relatert til arbeidstrening og andre sosiale tjenester, f eks i regi av velferdsorganisasjoner. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> SocialCareServicesWithoutHousing { get; } = new ConstantDefinition<Package>("70831546-6bfa-45a0-bdad-14e9db265847")
    {
        Entity = new()
        {
            Name = "Sosiale omsorgstjenester uten botilbud og flyktningemottak",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til sosiale omsorgstjeneser uten botilbud for eldre, funksjonshemmede og rusmisbrukere samt flykningemottak, og tjenester relatert til arbeidstrening og andre sosiale tjenester, f eks i regi av velferdsorganisasjoner. Denne fullmakten kan gi bruker tilgang til helseopplysninger om personer det rapporteres om. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sosiale-omsorgstjenester-uten-botilbud-og-flyktningemottak",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.HealthCareAndProtection,
        },
    };

    #endregion

    #region Industrier

    /// <summary>
    /// Represents the 'Næringsmidler, drikkevarer og tobakk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 65c7bbe7-aeb4-4f18-b0b0-1b1b83bd24d1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med næringsmidler, drikkevarer og tobakk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> FoodBeveragesTobacco { get; } = new ConstantDefinition<Package>("65c7bbe7-aeb4-4f18-b0b0-1b1b83bd24d1")
    {
        Entity = new()
        {
            Name = "Næringsmidler, drikkevarer og tobakk",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med næringsmidler, drikkevarer og tobakk. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:naeringsmidler-drikkevarer-og-tobakk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Møbler og annen industri' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 07bfd8a5-5b13-4937-84b9-2bd6ac726ea1</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:mobler-og-annen-industri</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med møbler og annen industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> FurnitureOtherManufacturing { get; } = new ConstantDefinition<Package>("07bfd8a5-5b13-4937-84b9-2bd6ac726ea1")
    {
        Entity = new()
        {
            Name = "Møbler og annen industri",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med møbler og annen industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:mobler-og-annen-industri",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Trelast, trevarer og papirvarer' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> deb1618f-395c-4a14-9a70-20e90e5f9a76</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:trelast-trevarer-papirvarer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trelast, trevarer og papirvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> LumberWoodProductsPaperGoods { get; } = new ConstantDefinition<Package>("deb1618f-395c-4a14-9a70-20e90e5f9a76")
    {
        Entity = new()
        {
            Name = "Trelast, trevarer og papirvarer",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trelast, trevarer og papirvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:trelast-trevarer-papirvarer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Metallvarer elektrisk utstyr og maskiner' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 2857fd90-dad2-4cc3-9947-282c22f5d2dc</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metallvarer, elektrisk utstyr og maskiner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MetalProductsElectricalEquipmentMachinery { get; } = new ConstantDefinition<Package>("2857fd90-dad2-4cc3-9947-282c22f5d2dc")
    {
        Entity = new()
        {
            Name = "Metallvarer elektrisk utstyr og maskiner",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metallvarer, elektrisk utstyr og maskiner. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:metallvarer-elektrisk-utstyr-og-maskiner",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Metaller og mineraler' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 84e744a0-e93f-4caf-bb3f-24387f045d2d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:metaller-og-mineraler</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metaller og mineraler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MetalsAndMinerals { get; } = new ConstantDefinition<Package>("84e744a0-e93f-4caf-bb3f-24387f045d2d")
    {
        Entity = new()
        {
            Name = "Metaller og mineraler",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med metaller og mineraler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:metaller-og-mineraler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Bergverk' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 8c700369-8fff-40f1-b4ce-2f116416d804</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:bergverk</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med bergverk og tilhørende tjenester til bergverksdrift og utvinning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> Mining { get; } = new ConstantDefinition<Package>("8c700369-8fff-40f1-b4ce-2f116416d804")
    {
        Entity = new()
        {
            Name = "Bergverk",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med bergverk og tilhørende tjenester til bergverksdrift og utvinning. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:bergverk",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Oljeraffinering, kjemisk og farmasøytisk industri' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 79203df9-d71b-460b-a828-22b0bf79f335</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med oljeraffinering, kjemisk og farmasøytisk industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> OilRefiningChemicalPharmaceuticalIndustry { get; } = new ConstantDefinition<Package>("79203df9-d71b-460b-a828-22b0bf79f335")
    {
        Entity = new()
        {
            Name = "Oljeraffinering, kjemisk og farmasøytisk industri",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med oljeraffinering, kjemisk og farmasøytisk industri. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:oljeraffinering-kjemisk-farmasoytisk-industri",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Trykkerier og reproduksjon av innspilte opptak' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 965f9aca-48f5-4a16-b5e3-228806ad4fa7</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:trykkerier-reproduksjon-opptak</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trykkerier og reproduksjon av innspilte opptak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PrintingRecordedMediaReproduction { get; } = new ConstantDefinition<Package>("965f9aca-48f5-4a16-b5e3-228806ad4fa7")
    {
        Entity = new()
        {
            Name = "Trykkerier og reproduksjon av innspilte opptak",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med trykkerier og reproduksjon av innspilte opptak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:trykkerier-reproduksjon-opptak",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Reparasjon og installasjon av maskiner og utstyr' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> ec492675-3a48-4ad9-b864-2d6865020642</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med reparasjon og installasjon av maskiner og utstyr. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ResponsibleAuditor { get; } = new ConstantDefinition<Package>("ec492675-3a48-4ad9-b864-2d6865020642")
    {
        Entity = new()
        {
            Name = "Reparasjon og installasjon av maskiner og utstyr",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med reparasjon og installasjon av maskiner og utstyr. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:reparasjon-og-installasjon-av-maskiner-og-utstyr",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Gummi, plast og ikke-metallholdige mineralprodukter' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 573be5d2-3ddd-4862-9711-2350147a1b25</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med gummi, plast og ikke-metallholdige mineralprodukter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RubberPlasticNonMetallicMineralProducts { get; } = new ConstantDefinition<Package>("573be5d2-3ddd-4862-9711-2350147a1b25")
    {
        Entity = new()
        {
            Name = "Gummi, plast og ikke-metallholdige mineralprodukter",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med gummi, plast og ikke-metallholdige mineralprodukter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:gummi-plast-og-ikkemetallholdige-mineralprodukter",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Verft og andre transportmidler' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c37aabdf-a78a-4f59-999a-298a83e9e113</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:verft-og-andre-transportmidler</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med verft og andre transportmidler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ShipyardsOtherTransportVehicles { get; } = new ConstantDefinition<Package>("c37aabdf-a78a-4f59-999a-298a83e9e113")
    {
        Entity = new()
        {
            Name = "Verft og andre transportmidler",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med verft og andre transportmidler. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:verft-og-andre-transportmidler",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    /// <summary>
    /// Represents the 'Tekstiler, klær og lærvarer' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> c6231d60-5373-4179-b98b-1e7eb83da474</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:tekstiler-klaer-laervarer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med tekstiler, klær og lærvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> TextilesClothingLeatherGoods { get; } = new ConstantDefinition<Package>("c6231d60-5373-4179-b98b-1e7eb83da474")
    {
        Entity = new()
        {
            Name = "Tekstiler, klær og lærvarer",
            Description = "Denne tilgangspakken gir fullmakter til tjenester knyttet til industri i forbindelse med tekstiler, klær og lærvarer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:tekstiler-klaer-laervarer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Industries,
        },
    };

    #endregion

    #region Integrasjoner

    /// <summary>
    /// Represents the 'Delegerbare Maskinporten scopes' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 27f488d7-d1f6-4aee-ae81-2bb42d62c446</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:maskinporten-scopes</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> DelegableMaskinportenScopes { get; } = new ConstantDefinition<Package>("27f488d7-d1f6-4aee-ae81-2bb42d62c446")
    {
        Entity = new()
        {
            Name = "Delegerbare Maskinporten scopes",
            Description = "Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:maskinporten-scopes",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Integrations,
        },
    };

    /// <summary>
    /// Represents the 'Delegerbare Maskinporten scopes - NUF' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 5dad616e-5538-4e3f-b15a-bae33f06c99f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:maskinporten-scopes-nuf</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring på vegne av norskregistrerte utenlandske foretak (NUF). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> DelegableMaskinportenScopesNUF { get; } = new ConstantDefinition<Package>("5dad616e-5538-4e3f-b15a-bae33f06c99f")
    {
        Entity = new()
        {
            Name = "Delegerbare Maskinporten scopes - NUF",
            Description = "Denne tilgangspakken gir fullmakter til data og programmeringsgrensenitt (API) som benytter Maskinporten eller tilsvarende løsninger for APIsikring på vegne av norskregistrerte utenlandske foretak (NUF). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:maskinporten-scopes-nuf",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Integrations,
        },
    };

    /// <summary>
    /// Represents the 'Maskinlesbare hendelser' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 66bd8101-fbd8-491b-b1f5-2f53a9476ffd</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:maskinlesbare-hendelser</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til å administrere tilgang til maskinlesbare hendelser. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MachineReadableEventsIntegration { get; } = new ConstantDefinition<Package>("66bd8101-fbd8-491b-b1f5-2f53a9476ffd")
    {
        Entity = new()
        {
            Name = "Maskinlesbare hendelser",
            Description = "Denne tilgangspakken gir fullmakter til å administrere tilgang til maskinlesbare hendelser. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:maskinlesbare-hendelser",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Integrations,
        },
    };

    #endregion

    #region Post og arkiv

    /// <summary>
    /// Represents the 'Post til virksomheten med taushetsbelagt innhold' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> bb0569a6-2268-49b5-9d38-8158b26124c3</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:post-til-virksomheten-med-taushetsbelagt-innhold</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til all mottatt post som innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post som har taushetsbelagt innhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> ConfidentialMailToBusiness { get; } = new ConstantDefinition<Package>("bb0569a6-2268-49b5-9d38-8158b26124c3")
    {
        Entity = new()
        {
            Name = "Post til virksomheten med taushetsbelagt innhold",
            Description = "Denne fullmakten gir tilgang til all mottatt post som innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post som har taushetsbelagt innhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:post-til-virksomheten-med-taushetsbelagt-innhold",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.MailAndArchive,
        },
    };

    /// <summary>
    /// Represents the 'Ordinær post til virksomheten' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 91cf61ae-69ab-49d5-b51a-80591c91f255</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:ordinaer-post-til-virksomheten</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til all mottatt post som ikke innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RegularMailToBusiness { get; } = new ConstantDefinition<Package>("91cf61ae-69ab-49d5-b51a-80591c91f255")
    {
        Entity = new()
        {
            Name = "Ordinær post til virksomheten",
            Description = "Denne fullmakten gir tilgang til all mottatt post som ikke innholder sensitiv/taushetsbelagt informasjon som sendes virksomheten. Fullmakten gis normalt til de i virksomheten som håndterer mottak av post. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:ordinaer-post-til-virksomheten",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.MailAndArchive,
        },
    };

    #endregion

    #region Administrere tilganger

    /// <summary>
    /// Represents the 'Tilgangsstyrer' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7a95-ad36-900c3d8ad300</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:tilgangsstyrer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Gir mulighet til å gi videre tilganger for virksomheten som man selv har mottatt</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ManageAccess,
        },
    };

    /// <summary>
    /// Represents the 'Klientadministrator' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7e82-9b4f-7d63e773bbca</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:klientadministrator</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Gir mulighet til å administrere tilgang til tjenester videre til ansatte på vegne av deres kunder</para>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ManageAccess,
        },
    };

    /// <summary>
    /// Represents the 'Konkursbo administrator' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7e9c-95c1-48937e23960a</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:konkursbo-tilgangsstyrer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Gir bruker mulighet til å administrere konkursbo</para>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ManageAccess,
        },
    };

    /// <summary>
    /// Represents the 'Hovedadministrator' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7e16-ab0c-36dc8ab1a29d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:hovedadministrator</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Gir mulighet til å administrere alle tilganger for virksomheten</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ManageAccess,
        },
    };

    /// <summary>
    /// Represents the 'Maskinporten administrator' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 0195efb8-7c80-7b30-a84d-f37fed9fb89c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:maskinporten-administrator</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Gir bruker mulighet til å administrere tilgang til maskinporten scopes</para>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.ManageAccess,
        },
    };

    #endregion

    #region Andre tjenesteytende næringer

    /// <summary>
    /// Represents the 'Finansiering og forsikring' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 8f977b5f-a2f9-4712-88e2-ab1a51a6b26f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:finansiering-og-forsikring</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til finansiering og forsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> FinancingAndInsurance { get; } = new ConstantDefinition<Package>("8f977b5f-a2f9-4712-88e2-ab1a51a6b26f")
    {
        Entity = new()
        {
            Name = "Finansiering og forsikring",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til finansiering og forsikring. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:finansiering-og-forsikring",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.OtherServiceIndustries,
        },
    };

    /// <summary>
    /// Represents the 'Informasjon og kommunikasjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 413d79fb-a419-4e74-98f7-aa91389deb81</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:informasjon-og-kommunikasjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til informasjon og kommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> InformationAndCommunication { get; } = new ConstantDefinition<Package>("413d79fb-a419-4e74-98f7-aa91389deb81")
    {
        Entity = new()
        {
            Name = "Informasjon og kommunikasjon",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til informasjon og kommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:informasjon-og-kommunikasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.OtherServiceIndustries,
        },
    };

    /// <summary>
    /// Represents the 'Annen tjenesteyting' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a8b18888-2216-4eaa-972d-ae96da04b1ac</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:annen-tjenesteyting</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til annen tjenesteyting som f eks organisasjoner og foreninger, reparasjon av datamaskiner og varer til personlig bruk og husholdningsbruk og en rekke personlige tjenester som ikke er nevnt annet sted. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> OtherServices { get; } = new ConstantDefinition<Package>("a8b18888-2216-4eaa-972d-ae96da04b1ac")
    {
        Entity = new()
        {
            Name = "Annen tjenesteyting",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til annen tjenesteyting som f eks organisasjoner og foreninger, reparasjon av datamaskiner og varer til personlig bruk og husholdningsbruk og en rekke personlige tjenester som ikke er nevnt annet sted. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:annen-tjenesteyting",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.OtherServiceIndustries,
        },
    };

    /// <summary>
    /// Represents the 'Post- og telekommunikasjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a736c33b-c15a-43ac-85be-a684630e1e59</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:post-og-telekommunikasjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til post og telekommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PostalAndTelecommunications { get; } = new ConstantDefinition<Package>("a736c33b-c15a-43ac-85be-a684630e1e59")
    {
        Entity = new()
        {
            Name = "Post- og telekommunikasjon",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til post og telekommunikasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:post-og-telekommunikasjon",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.OtherServiceIndustries,
        },
    };

    #endregion

    #region Personale

    /// <summary>
    /// Represents the 'A-ordningen' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4af2d44d-f9f8-4585-b679-7875bb1828ea</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:a-ordning</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester som inngår i A-ordningen. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.  OBS! Vær oppmerksompå at tilgangspakken inneholder fullmakter som kan ha sensitiv karakter. Vurder om fullmaktene skal gis som enkeltrettigheter.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    /// <summary>
    /// Represents the 'Ansettelsesforhold' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> cf575d42-72be-4d14-b0d2-77352221df4f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:ansettelsesforhold</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til ansettelsesforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    /// <summary>
    /// Represents the 'Permisjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 71a2bd47-e885-49c7-8a58-7ef4bd936f41</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:permisjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til permisjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    /// <summary>
    /// Represents the 'Pensjon' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> ae41bf08-c7ef-4bfe-b2b9-7bf6deb7798f</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:pensjon</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til pensjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    /// <summary>
    /// Represents the 'Lønn' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> fb0aa257-e7dc-4b7b-9528-77dfb749461c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:lonn</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til lønn og honorar. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    /// <summary>
    /// Represents the 'Sykefravær' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 08ec673d-9126-4ed0-ae75-7ceec8633c77</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sykefravaer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til sykefravær. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.Personnel,
        },
    };

    #endregion

    #region Skatt, avgift, regnskap og toll

    /// <summary>
    /// Represents the 'Regnskap og økonomirapportering' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> b4e69b54-895e-42c5-bf0d-861a571cd282</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:regnskap-okonomi-rapport</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til regnskap og øknomirapportering som ikke tilhører skatt og merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Revisorattesterer' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 1886712b-e077-445a-ab3f-8c8bdebccc67</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:revisorattesterer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til alle tjenester som krever revisorattestering. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AuditorAttestation { get; } = new ConstantDefinition<Package>("1886712b-e077-445a-ab3f-8c8bdebccc67")
    {
        Entity = new()
        {
            Name = "Revisorattesterer",
            Description = "Denne fullmakten gir tilgang til alle tjenester som krever revisorattestering. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:revisorattesterer",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Skatt næring' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 1dba50d6-f604-48e9-bd41-82321b13e85c</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:skatt-naering</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til skatt for næringer. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Krav, betalinger og utlegg' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> a02fc602-872a-4671-b338-8b86a64b534a</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:krav-og-utlegg</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til krav og utlegg. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Kreditt- og oppgjørsordninger' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> eb9006a6-dbd5-4155-9174-91e182a56715</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:kreditt-og-oppgjoer</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til kreditt- og oppgjørsordninger. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Toll' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> deb7f8ae-8427-469d-b824-937c146c0489</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:toll</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til toll og fortolling. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Motorvognavgifter' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4f4d4567-2384-49fa-b34b-84b4b77139d8</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:motorvognavgift</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til motorvognavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Særavgifter' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 74c1dcf6-a760-4065-83f0-8edc6dec5dba</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:saeravgifter</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til særavgifter. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Skattegrunnlag' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 4c859601-9b2b-4662-af39-846f4117ad7a</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:skattegrunnlag</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til innhenting av skattegrunnlag. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    /// <summary>
    /// Represents the 'Merverdiavgift' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 9a61b136-7810-4939-ab6d-84938e9a12c6</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:merverdiavgift</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne tilgangspakken gir fullmakter til tjenester knyttet til merverdiavgift. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TaxFeesAccountingAndCustoms,
        },
    };

    #endregion

    #region Transport og lagring

    /// <summary>
    /// Represents the 'Lufttransport' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 3b3bc323-207b-4d56-a21f-97b6875ccc28</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:lufttransport</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til luftfartøy og romfartøy. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> AirTransport { get; } = new ConstantDefinition<Package>("3b3bc323-207b-4d56-a21f-97b6875ccc28")
    {
        Entity = new()
        {
            Name = "Lufttransport",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til luftfartøy og romfartøy. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:lufttransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    /// <summary>
    /// Represents the 'Sjøfart' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 9603dd83-be41-4190-b0a7-97490f4a601d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:sjofart</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til skipsarbeidstakere og fartøy til sjøs. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> MaritimeTransport { get; } = new ConstantDefinition<Package>("9603dd83-be41-4190-b0a7-97490f4a601d")
    {
        Entity = new()
        {
            Name = "Sjøfart",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til skipsarbeidstakere og fartøy til sjøs. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:sjofart",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    /// <summary>
    /// Represents the 'Transport i rør' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 9cc8b401-b4cb-473d-a3fa-96171aeb1389</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:transport-i-ror</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til transport i rør. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> PipelineTransport { get; } = new ConstantDefinition<Package>("9cc8b401-b4cb-473d-a3fa-96171aeb1389")
    {
        Entity = new()
        {
            Name = "Transport i rør",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til transport i rør. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:transport-i-ror",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    /// <summary>
    /// Represents the 'Jernbanetransport' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> dfcd3923-cb66-45f7-9535-999b6c0c496d</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:jernbanetransport</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til jernbane, inkludert trikk, T-bane og sporvogn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RailwayTransport { get; } = new ConstantDefinition<Package>("dfcd3923-cb66-45f7-9535-999b6c0c496d")
    {
        Entity = new()
        {
            Name = "Jernbanetransport",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til jernbane, inkludert trikk, T-bane og sporvogn. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:jernbanetransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    /// <summary>
    /// Represents the 'Veitransport' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 36c245db-3207-449e-92b1-94cb0a0f3031</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:veitransport</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til person- og godstransport langs veinettet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> RoadTransport { get; } = new ConstantDefinition<Package>("36c245db-3207-449e-92b1-94cb0a0f3031")
    {
        Entity = new()
        {
            Name = "Veitransport",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til person- og godstransport langs veinettet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:veitransport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    /// <summary>
    /// Represents the 'Lagring og andre tjenester tilknyttet transport' access package.
    /// </summary>
    /// <remarks>
    /// <para><strong>ID:</strong> 86a8ff87-3a3d-4f32-a4ef-99c43494bc6e</para>
    /// <para><strong>URN:</strong> urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport</para>
    /// <para><strong>Provider:</strong> Altinn3</para>
    /// <para><strong>Description:</strong> Denne fullmakten gir tilgang til tjenester knyttet til lagring og hjelpetjenester i forbindelse med transport, samt post- og kurervirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.</para>
    /// </remarks>
    public static ConstantDefinition<Package> StorageTransportServices { get; } = new ConstantDefinition<Package>("86a8ff87-3a3d-4f32-a4ef-99c43494bc6e")
    {
        Entity = new()
        {
            Name = "Lagring og andre tjenester tilknyttet transport",
            Description = "Denne fullmakten gir tilgang til tjenester knyttet til lagring og hjelpetjenester i forbindelse med transport, samt post- og kurervirksomhet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som fullmakten gir.",
            Urn = "urn:altinn:accesspackage:lagring-og-andre-tjenester-tilknyttet-transport",
            IsDelegable = true,
            HasResources = true,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
            AreaId = AreaConstants.TransportAndStorage,
        },
    };

    #endregion

}
