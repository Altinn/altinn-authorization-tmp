using System.Diagnostics.CodeAnalysis;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

/// <summary>
/// Defines constant <see cref="Role"/> instances used across the system.
/// Each role represents a specific role in the access management domain,
/// with a fixed unique identifier (GUID), localized names, codes, descriptions, and associated provider.
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Try to get <see cref="Role"/> by name.
    /// </summary>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out ConstantDefinition<Role>? result)
        => ConstantLookup.TryGetByName(typeof(RoleConstants), name, out result);

    /// <summary>
    /// Try to get <see cref="Role"/> using Guid.
    /// </summary>
    public static bool TryGetById(Guid id, [NotNullWhen(true)] out ConstantDefinition<Role>? result)
        => ConstantLookup.TryGetById(typeof(RoleConstants), id, out result);
    
    /// <summary>
    /// Try to get <see cref="Role"/> by Urn.
    /// </summary>
    public static bool TryGetByUrn(string urn, [NotNullWhen(true)] out ConstantDefinition<Role>? result)
        => ConstantLookup.TryGetByUrn(typeof(RoleConstants), urn, out result);

    /// <summary>
    /// Get all constants as a read-only collection.
    /// </summary>
    public static IReadOnlyCollection<ConstantDefinition<Role>> AllEntities()
        => ConstantLookup.AllEntities<Role>(typeof(RoleConstants));

    /// <summary>
    /// Get all translations as read-only collection.
    /// </summary>
    public static IReadOnlyCollection<TranslationEntry> AllTranslations()
        => ConstantLookup.AllTranslations<Role>(typeof(RoleConstants));

    #region Altinn 3 Roles

    /// <summary>
    /// Represents the Rightholder role ("Rettighetshaver").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 42cae370-2dc1-4fdc-9c67-c2f4b0f0f829
    /// - <c>Name:</c> "Rettighetshaver"
    /// - <c>Code:</c> "rettighetshaver"
    /// - <c>Description:</c> "Gir mulighet til å motta delegerte fullmakter for virksomheten"
    /// - <c>Urn:</c> "urn:altinn:role:rettighetshaver"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> true
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn3"/>
    /// - <c>Translations:</c>
    ///   - EN: "Rightholder" - "Allows receiving delegated authorizations for the business"
    ///   - NN: "Rettshavar" - "Gjev høve til å motta delegerte fullmakter for verksemda"
    /// </remarks>
    public static ConstantDefinition<Role> Rightholder { get; } = new ConstantDefinition<Role>("42cae370-2dc1-4fdc-9c67-c2f4b0f0f829")
    {
        Entity = new()
        {
            Name = "Rettighetshaver",
            Code = "rettighetshaver",
            Description = "Gir mulighet til å motta delegerte fullmakter for virksomheten",
            Urn = "urn:altinn:role:rettighetshaver",
            IsKeyRole = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rightholder"),
            KeyValuePair.Create("Description", "Allows receiving delegated authorizations for the business")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rettshavar"),
            KeyValuePair.Create("Description", "Gjev høve til å motta delegerte fullmakter for verksemda")
        ),
    };

    /// <summary>
    /// Represents the Agent role ("Agent").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ff4c33f5-03f7-4445-85ed-1e60b8aafb30
    /// - <c>Name:</c> "Agent"
    /// - <c>Code:</c> "agent"
    /// - <c>Description:</c> "Gir mulighet til å motta delegerte fullmakter for virksomheten"
    /// - <c>Urn:</c> "urn:altinn:role:agent"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> true
    /// - <c>EntityTypeId:</c> References person entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn3"/>
    /// - <c>Translations:</c>
    ///   - EN: "Agent" - "Allows receiving delegated authorizations for the business"
    ///   - NN: "Agent" - "Gjev høve til å motta delegerte fullmakter for verksemda"
    /// </remarks>
    public static ConstantDefinition<Role> Agent { get; } = new ConstantDefinition<Role>("ff4c33f5-03f7-4445-85ed-1e60b8aafb30")
    {
        Entity = new()
        {
            Name = "Agent",
            Code = "agent",
            Description = "Gir mulighet til å motta delegerte fullmakter for virksomheten",
            Urn = "urn:altinn:role:agent",
            IsKeyRole = false,
            IsAssignable = true,
            EntityTypeId = EntityTypeConstants.Person.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Agent"),
            KeyValuePair.Create("Description", "Allows receiving delegated authorizations for the business")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Agent"),
            KeyValuePair.Create("Description", "Gjev høve til å motta delegerte fullmakter for verksemda")
        ),
    };

    /// <summary>
    /// Represents the Main Administrator role ("Hovedadministrator").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7
    /// - <c>Name:</c> "Hovedadministrator"
    /// - <c>Code:</c> "hovedadministrator"
    /// - <c>Description:</c> "Intern rolle for å samle alle delegerbare fullmakter en hovedadministrator kan utføre for virksomheten"
    /// - <c>Urn:</c> "urn:altinn:role:hovedadministrator"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn3"/>
    /// - <c>Translations:</c>
    ///   - EN: "Main Administrator" - "Internal role to collect all delegable access a main administrator can perform for the business"
    ///   - NN: "Hovudadministrator" - "Intern rolle for å samla alle delegerbare fullmakter ein hovudadministrator kan utføra for verksemda"
    /// </remarks>
    public static ConstantDefinition<Role> MainAdministrator { get; } = new ConstantDefinition<Role>("ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7")
    {
        Entity = new()
        {
            Name = "Hovedadministrator",
            Code = "hovedadministrator",
            Description = "Intern rolle for å samle alle delegerbare fullmakter en hovedadministrator kan utføre for virksomheten",
            Urn = "urn:altinn:role:hovedadministrator",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn3.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Main Administrator"),
            KeyValuePair.Create("Description", "Internal role to collect all delegable access a main administrator can perform for the business")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Hovudadministrator"),
            KeyValuePair.Create("Description", "Intern rolle for å samla alle delegerbare fullmakter ein hovudadministrator kan utføra for verksemda")
        ),
    };

    #endregion

    #region CCR External Roles

    /// <summary>
    /// Represents the Administrative Unit - Public Sector role ("Administrativ enhet - offentlig sektor").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 66ad5542-4f4a-4606-996f-18690129ce00
    /// - <c>Name:</c> "Administrativ enhet - offentlig sektor"
    /// - <c>Code:</c> "administrativ-enhet-offentlig-sektor"
    /// - <c>Description:</c> "Administrativ enhet - offentlig sektor"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Administrative Unit - Public Sector" - "Administrative Unit - Public Sector"
    ///   - NN: "Administrativ eining - offentleg sektor" - "Administrativ eining - offentleg sektor"
    /// </remarks>
    public static ConstantDefinition<Role> AdministrativeUnitPublicSector { get; } = new ConstantDefinition<Role>("66ad5542-4f4a-4606-996f-18690129ce00")
    {
        Entity = new()
        {
            Name = "Administrativ enhet - offentlig sektor",
            Code = "administrativ-enhet-offentlig-sektor",
            Description = "Administrativ enhet - offentlig sektor",
            Urn = "urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Administrative Unit - Public Sector"),
            KeyValuePair.Create("Description", "Administrative Unit - Public Sector")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Administrativ eining - offentleg sektor"),
            KeyValuePair.Create("Description", "Administrativ eining - offentleg sektor")
        ),
    };

    /// <summary>
    /// Represents the Deputy Leader role ("Nestleder").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 29a24eab-a25f-445d-b56d-e3b914844853
    /// - <c>Name:</c> "Nestleder"
    /// - <c>Code:</c> "nestleder"
    /// - <c>Description:</c> "Styremedlem som opptrer som styreleder ved leders fravær"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:nestleder"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Deputy Leader" - "Board member who acts as chair in the absence of the leader"
    ///   - NN: "Nestleiar" - "Styremedlem som fungerer som styreleiar ved leiarens fråvær"
    /// </remarks>
    public static ConstantDefinition<Role> DeputyLeader { get; } = new ConstantDefinition<Role>("29a24eab-a25f-445d-b56d-e3b914844853")
    {
        Entity = new()
        {
            Name = "Nestleder",
            Code = "nestleder",
            Description = "Styremedlem som opptrer som styreleder ved leders fravær",
            Urn = "urn:altinn:external-role:ccr:nestleder",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Deputy Leader"),
            KeyValuePair.Create("Description", "Board member who acts as chair in the absence of the leader")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Nestleiar"),
            KeyValuePair.Create("Description", "Styremedlem som fungerer som styreleiar ved leiarens fråvær")
        ),
    };

    /// <summary>
    /// Represents the Office Community Member role ("Inngår i kontorfellesskap").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 8c1e91c2-a71c-4abf-a74e-a600a98be976
    /// - <c>Name:</c> "Inngår i kontorfellesskap"
    /// - <c>Code:</c> "kontorfelleskapmedlem"
    /// - <c>Description:</c> "Inngår i kontorfellesskap"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:kontorfelleskapmedlem"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Part of Office Community" - "Participates in office community"
    ///   - NN: "Inngår i kontorfellesskap" - "Inngår i kontorfellesskap"
    /// </remarks>
    public static ConstantDefinition<Role> OfficeCommunityMember { get; } = new ConstantDefinition<Role>("8c1e91c2-a71c-4abf-a74e-a600a98be976")
    {
        Entity = new()
        {
            Name = "Inngår i kontorfellesskap",
            Code = "kontorfelleskapmedlem",
            Description = "Inngår i kontorfellesskap",
            Urn = "urn:altinn:external-role:ccr:kontorfelleskapmedlem",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Part of Office Community"),
            KeyValuePair.Create("Description", "Participates in office community")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Inngår i kontorfellesskap"),
            KeyValuePair.Create("Description", "Inngår i kontorfellesskap")
        ),
    };

    /// <summary>
    /// Represents the Organizational Unit in the Public Sector role ("Organisasjonsledd i offentlig sektor").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> cfc41a92-2061-4ff4-97dc-658ffba2c00e
    /// - <c>Name:</c> "Organisasjonsledd i offentlig sektor"
    /// - <c>Code:</c> "organisasjonsledd-offentlig-sektor"
    /// - <c>Description:</c> "Organisasjonsledd i offentlig sektor"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Organizational Unit in the Public Sector" - "Organizational Unit in the Public Sector"
    ///   - NN: "Organisasjonsledd i offentleg sektor" - "Organisasjonsledd i offentleg sektor"
    /// </remarks>
    public static ConstantDefinition<Role> OrganizationalUnitPublicSector { get; } = new ConstantDefinition<Role>("cfc41a92-2061-4ff4-97dc-658ffba2c00e")
    {
        Entity = new()
        {
            Name = "Organisasjonsledd i offentlig sektor",
            Code = "organisasjonsledd-offentlig-sektor",
            Description = "Organisasjonsledd i offentlig sektor",
            Urn = "urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Organizational Unit in the Public Sector"),
            KeyValuePair.Create("Description", "Organizational Unit in the Public Sector")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Organisasjonsledd i offentleg sektor"),
            KeyValuePair.Create("Description", "Organisasjonsledd i offentleg sektor")
        ),
    };

    /// <summary>
    /// Represents the Distinct Subunit role ("Særskilt oppdelt enhet").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2fec6d4b-cead-419a-adf3-1bf482a3c9dc
    /// - <c>Name:</c> "Særskilt oppdelt enhet"
    /// - <c>Code:</c> "saerskilt-oppdelt-enhet"
    /// - <c>Description:</c> "Særskilt oppdelt enhet"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Distinct Subunit" - "Distinct Subunit"
    ///   - NN: "Særskild oppdelt eining" - "Særskild oppdelt eining"
    /// </remarks>
    public static ConstantDefinition<Role> DistinctSubunit { get; } = new ConstantDefinition<Role>("2fec6d4b-cead-419a-adf3-1bf482a3c9dc")
    {
        Entity = new()
        {
            Name = "Særskilt oppdelt enhet",
            Code = "saerskilt-oppdelt-enhet",
            Description = "Særskilt oppdelt enhet",
            Urn = "urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Distinct Subunit"),
            KeyValuePair.Create("Description", "Distinct Subunit")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Særskild oppdelt eining"),
            KeyValuePair.Create("Description", "Særskild oppdelt eining")
        ),
    };

    /// <summary>
    /// Represents the Managing Director role ("Daglig leder").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 55bd7d4d-08dd-46ee-ac8e-3a44d800d752
    /// - <c>Name:</c> "Daglig leder"
    /// - <c>Code:</c> "daglig-leder"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som har ansvaret for den daglige driften i en virksomhet"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:daglig-leder"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Managing Director" - "An individual or legal entity responsible for the daily operations of a business"
    ///   - NN: "Dagleg leiar" - "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i ei verksemd"
    /// </remarks>
    public static ConstantDefinition<Role> ManagingDirector { get; } = new ConstantDefinition<Role>("55bd7d4d-08dd-46ee-ac8e-3a44d800d752")
    {
        Entity = new()
        {
            Name = "Daglig leder",
            Code = "daglig-leder",
            Description = "Fysisk- eller juridisk person som har ansvaret for den daglige driften i en virksomhet",
            Urn = "urn:altinn:external-role:ccr:daglig-leder",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Managing Director"),
            KeyValuePair.Create("Description", "An individual or legal entity responsible for the daily operations of a business")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Dagleg leiar"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i ei verksemd")
        ),
    };

    /// <summary>
    /// Represents the Participant with Shared Responsibility role ("Deltaker delt ansvar").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 18baa914-ac43-4663-9fa4-6f5760dc68eb
    /// - <c>Name:</c> "Deltaker delt ansvar"
    /// - <c>Code:</c> "deltaker-delt-ansvar"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som har personlig ansvar for deler av selskapets forpliktelser"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:deltaker-delt-ansvar"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Participant with Shared Responsibility" - "An individual or legal entity who has personal responsibility for parts of the company's obligations"
    ///   - NN: "Deltakar delt ansvar" - "Fysisk- eller juridisk person som har personleg ansvar for delar av selskapet sine forpliktingar"
    /// </remarks>
    public static ConstantDefinition<Role> ParticipantSharedResponsibility { get; } = new ConstantDefinition<Role>("18baa914-ac43-4663-9fa4-6f5760dc68eb")
    {
        Entity = new()
        {
            Name = "Deltaker delt ansvar",
            Code = "deltaker-delt-ansvar",
            Description = "Fysisk- eller juridisk person som har personlig ansvar for deler av selskapets forpliktelser",
            Urn = "urn:altinn:external-role:ccr:deltaker-delt-ansvar",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Participant with Shared Responsibility"),
            KeyValuePair.Create("Description", "An individual or legal entity who has personal responsibility for parts of the company's obligations")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Deltakar delt ansvar"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som har personleg ansvar for delar av selskapet sine forpliktingar")
        ),
    };

    /// <summary>
    /// Represents the Owner role ("Innehaver").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2651ed07-f31b-4bc1-87bd-4d270742a19d
    /// - <c>Name:</c> "Innehaver"
    /// - <c>Code:</c> "innehaver"
    /// - <c>Description:</c> "Fysisk person som er eier av et enkeltpersonforetak"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:innehaver"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Owner" - "An individual who is the owner of a sole proprietorship"
    ///   - NN: "Innehavar" - "Fysisk person som er eigar av eit enkeltpersonforetak"
    /// </remarks>
    public static ConstantDefinition<Role> Owner { get; } = new ConstantDefinition<Role>("2651ed07-f31b-4bc1-87bd-4d270742a19d")
    {
        Entity = new()
        {
            Name = "Innehaver",
            Code = "innehaver",
            Description = "Fysisk person som er eier av et enkeltpersonforetak",
            Urn = "urn:altinn:external-role:ccr:innehaver",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Owner"),
            KeyValuePair.Create("Description", "An individual who is the owner of a sole proprietorship")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Innehavar"),
            KeyValuePair.Create("Description", "Fysisk person som er eigar av eit enkeltpersonforetak")
        ),
    };

    /// <summary>
    /// Represents the Participant with Full Responsibility role ("Deltaker fullt ansvar").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f1021b8c-9fbc-4296-bd17-a05d713037ef
    /// - <c>Name:</c> "Deltaker fullt ansvar"
    /// - <c>Code:</c> "deltaker-fullt-ansvar"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som har ubegrenset, personlig ansvar for selskapets forpliktelser"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:deltaker-fullt-ansvar"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Participant with Full Responsibility" - "An individual or legal entity who has unlimited personal responsibility for the company's obligations"
    ///   - NN: "Deltakar fullt ansvar" - "Fysisk- eller juridisk person som har ubegrensa, personleg ansvar for selskapet sine forpliktingar"
    /// </remarks>
    public static ConstantDefinition<Role> ParticipantFullResponsibility { get; } = new ConstantDefinition<Role>("f1021b8c-9fbc-4296-bd17-a05d713037ef")
    {
        Entity = new()
        {
            Name = "Deltaker fullt ansvar",
            Code = "deltaker-fullt-ansvar",
            Description = "Fysisk- eller juridisk person som har ubegrenset, personlig ansvar for selskapets forpliktelser",
            Urn = "urn:altinn:external-role:ccr:deltaker-fullt-ansvar",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Participant with Full Responsibility"),
            KeyValuePair.Create("Description", "An individual or legal entity who has unlimited personal responsibility for the company's obligations")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Deltakar fullt ansvar"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som har ubegrensa, personleg ansvar for selskapet sine forpliktingar")
        ),
    };

    /// <summary>
    /// Represents the Alternate Member role ("Varamedlem").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d41d67f2-15b0-4c82-95db-b8d5baaa14a4
    /// - <c>Name:</c> "Varamedlem"
    /// - <c>Code:</c> "varamedlem"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som er stedfortreder for et styremedlem"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:varamedlem"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Alternate Member" - "An individual or legal entity who acts as a substitute for a board member"
    ///   - NN: "Varamedlem" - "Fysisk- eller juridisk person som er staden for eit styremedlem"
    /// </remarks>
    public static ConstantDefinition<Role> AlternateMember { get; } = new ConstantDefinition<Role>("d41d67f2-15b0-4c82-95db-b8d5baaa14a4")
    {
        Entity = new()
        {
            Name = "Varamedlem",
            Code = "varamedlem",
            Description = "Fysisk- eller juridisk person som er stedfortreder for et styremedlem",
            Urn = "urn:altinn:external-role:ccr:varamedlem",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Alternate Member"),
            KeyValuePair.Create("Description", "An individual or legal entity who acts as a substitute for a board member")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Varamedlem"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som er staden for eit styremedlem")
        ),
    };

    /// <summary>
    /// Represents the Observer role ("Observatør").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1f8a2518-9494-468a-80a0-7405f0daf9e9
    /// - <c>Name:</c> "Observatør"
    /// - <c>Code:</c> "observator"
    /// - <c>Description:</c> "Fysisk person som deltar i styremøter i en virksomhet, men uten stemmerett"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:observator"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Observer" - "An individual who participates in board meetings of a business, but without voting rights"
    ///   - NN: "Observatør" - "Fysisk person som deltek i styremøter i ei verksemd, men utan stemmerett"
    /// </remarks>
    public static ConstantDefinition<Role> Observer { get; } = new ConstantDefinition<Role>("1f8a2518-9494-468a-80a0-7405f0daf9e9")
    {
        Entity = new()
        {
            Name = "Observatør",
            Code = "observator",
            Description = "Fysisk person som deltar i styremøter i en virksomhet, men uten stemmerett",
            Urn = "urn:altinn:external-role:ccr:observator",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Observer"),
            KeyValuePair.Create("Description", "An individual who participates in board meetings of a business, but without voting rights")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Observatør"),
            KeyValuePair.Create("Description", "Fysisk person som deltek i styremøter i ei verksemd, men utan stemmerett")
        ),
    };

    /// <summary>
    /// Represents the Board Member role ("Styremedlem").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f045ffda-dbdc-41da-b674-b9b276ad5b01
    /// - <c>Name:</c> "Styremedlem"
    /// - <c>Code:</c> "styremedlem"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som inngår i et styre"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:styremedlem"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Board Member" - "An individual or legal entity who is a member of a board"
    ///   - NN: "Styremedlem" - "Fysisk- eller juridisk person som inngår i eit styre"
    /// </remarks>
    public static ConstantDefinition<Role> BoardMember { get; } = new ConstantDefinition<Role>("f045ffda-dbdc-41da-b674-b9b276ad5b01")
    {
        Entity = new()
        {
            Name = "Styremedlem",
            Code = "styremedlem",
            Description = "Fysisk- eller juridisk person som inngår i et styre",
            Urn = "urn:altinn:external-role:ccr:styremedlem",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Board Member"),
            KeyValuePair.Create("Description", "An individual or legal entity who is a member of a board")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Styremedlem"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som inngår i eit styre")
        ),
    };

    /// <summary>
    /// Represents the Chair of the Board role ("Styrets leder").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4
    /// - <c>Name:</c> "Styrets leder"
    /// - <c>Code:</c> "styreleder"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som er styremedlem og leder et styre"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:styreleder"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Chair of the Board" - "An individual or legal entity who is a board member and chairs the board"
    ///   - NN: "Styrets leiar" - "Fysisk- eller juridisk person som er styremedlem og leiar eit styre"
    /// </remarks>
    public static ConstantDefinition<Role> ChairOfTheBoard { get; } = new ConstantDefinition<Role>("9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4")
    {
        Entity = new()
        {
            Name = "Styrets leder",
            Code = "styreleder",
            Description = "Fysisk- eller juridisk person som er styremedlem og leder et styre",
            Urn = "urn:altinn:external-role:ccr:styreleder",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Chair of the Board"),
            KeyValuePair.Create("Description", "An individual or legal entity who is a board member and chairs the board")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Styrets leiar"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som er styremedlem og leiar eit styre")
        ),
    };

    /// <summary>
    /// Represents the Personal Bankruptcy role ("Den personlige konkursen angår").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2e2fc06e-d9b7-4cd9-91bc-d5de766d20de
    /// - <c>Name:</c> "Den personlige konkursen angår"
    /// - <c>Code:</c> "personlige-konkurs"
    /// - <c>Description:</c> "Den personlige konkursen angår"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:personlige-konkurs"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Personal Bankruptcy" - "Personal Bankruptcy"
    ///   - NN: "Den personlege konkursen angår" - "Den personlege konkursen angår"
    /// </remarks>
    public static ConstantDefinition<Role> PersonalBankruptcy { get; } = new ConstantDefinition<Role>("2e2fc06e-d9b7-4cd9-91bc-d5de766d20de")
    {
        Entity = new()
        {
            Name = "Den personlige konkursen angår",
            Code = "personlige-konkurs",
            Description = "Den personlige konkursen angår",
            Urn = "urn:altinn:external-role:ccr:personlige-konkurs",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Personal Bankruptcy"),
            KeyValuePair.Create("Description", "Personal Bankruptcy")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Den personlege konkursen angår"),
            KeyValuePair.Create("Description", "Den personlege konkursen angår")
        ),
    };

    /// <summary>
    /// Represents the Norwegian Representative for a Foreign Entity role ("Norsk representant for utenlandsk enhet").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e852d758-e8dd-41ec-a1e2-4632deb6857d
    /// - <c>Name:</c> "Norsk representant for utenlandsk enhet"
    /// - <c>Code:</c> "norsk-representant"
    /// - <c>Description:</c> "Fysisk- eller juridisk person som har ansvaret for den daglige driften i Norge"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:norsk-representant"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Norwegian Representative for a Foreign Entity" - "An individual or legal entity responsible for the daily operations in Norway"
    ///   - NN: "Norsk representant for utanlandsk eining" - "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i Noreg"
    /// </remarks>
    public static ConstantDefinition<Role> NorwegianRepresentativeForeignEntity { get; } = new ConstantDefinition<Role>("e852d758-e8dd-41ec-a1e2-4632deb6857d")
    {
        Entity = new()
        {
            Name = "Norsk representant for utenlandsk enhet",
            Code = "norsk-representant",
            Description = "Fysisk- eller juridisk person som har ansvaret for den daglige driften i Norge",
            Urn = "urn:altinn:external-role:ccr:norsk-representant",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Norwegian Representative for a Foreign Entity"),
            KeyValuePair.Create("Description", "An individual or legal entity responsible for the daily operations in Norway")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Norsk representant for utanlandsk eining"),
            KeyValuePair.Create("Description", "Fysisk- eller juridisk person som har ansvaret for den daglege drifta i Noreg")
        ),
    };

    /// <summary>
    /// Represents the Contact Person role ("Kontaktperson").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> db013059-4a8a-442d-bf90-b03539fe5dda
    /// - <c>Name:</c> "Kontaktperson"
    /// - <c>Code:</c> "kontaktperson"
    /// - <c>Description:</c> "Fysisk person som representerer en virksomhet"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:kontaktperson"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Contact Person" - "An individual who represents a business"
    ///   - NN: "Kontaktperson" - "Fysisk person som representerer ei verksemd"
    /// </remarks>
    public static ConstantDefinition<Role> ContactPerson { get; } = new ConstantDefinition<Role>("db013059-4a8a-442d-bf90-b03539fe5dda")
    {
        Entity = new()
        {
            Name = "Kontaktperson",
            Code = "kontaktperson",
            Description = "Fysisk person som representerer en virksomhet",
            Urn = "urn:altinn:external-role:ccr:kontaktperson",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Contact Person"),
            KeyValuePair.Create("Description", "An individual who represents a business")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Kontaktperson"),
            KeyValuePair.Create("Description", "Fysisk person som representerer ei verksemd")
        ),
    };

    /// <summary>
    /// Represents the Contact Person NUF role ("Kontaktperson NUF").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 69c4397a-9e34-4e73-9f69-534bc1bb74c8
    /// - <c>Name:</c> "Kontaktperson NUF"
    /// - <c>Code:</c> "kontaktperson-nuf"
    /// - <c>Description:</c> "Fysisk person som representerer en virksomhet - NUF"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:kontaktperson-nuf"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Contact Person NUF" - "An individual who represents a business - NUF"
    ///   - NN: "Kontaktperson NUF" - "Fysisk person som representerer ei verksemd - NUF"
    /// </remarks>
    public static ConstantDefinition<Role> ContactPersonNUF { get; } = new ConstantDefinition<Role>("69c4397a-9e34-4e73-9f69-534bc1bb74c8")
    {
        Entity = new()
        {
            Name = "Kontaktperson NUF",
            Code = "kontaktperson-nuf",
            Description = "Fysisk person som representerer en virksomhet - NUF",
            Urn = "urn:altinn:external-role:ccr:kontaktperson-nuf",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Contact Person NUF"),
            KeyValuePair.Create("Description", "An individual who represents a business - NUF")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Kontaktperson NUF"),
            KeyValuePair.Create("Description", "Fysisk person som representerer ei verksemd - NUF")
        ),
    };

    /// <summary>
    /// Represents the Managing Shipowner role ("Bestyrende reder").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 8f0cf433-954e-4680-a25d-a3cf9ffdf149
    /// - <c>Name:</c> "Bestyrende reder"
    /// - <c>Code:</c> "bestyrende-reder"
    /// - <c>Description:</c> "Bestyrende reder"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:bestyrende-reder"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Managing Shipowner" - "Managing Shipowner"
    ///   - NN: "Bestyrande reder" - "Bestyrande reder"
    /// </remarks>
    public static ConstantDefinition<Role> ManagingShipowner { get; } = new ConstantDefinition<Role>("8f0cf433-954e-4680-a25d-a3cf9ffdf149")
    {
        Entity = new()
        {
            Name = "Bestyrende reder",
            Code = "bestyrende-reder",
            Description = "Bestyrende reder",
            Urn = "urn:altinn:external-role:ccr:bestyrende-reder",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Managing Shipowner"),
            KeyValuePair.Create("Description", "Managing Shipowner")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bestyrande reder"),
            KeyValuePair.Create("Description", "Bestyrande reder")
        ),
    };

    /// <summary>
    /// Represents the Owning Municipality role ("Eierkommune").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9ce84a4d-4970-4ef2-8208-b8b8f4d45556
    /// - <c>Name:</c> "Eierkommune"
    /// - <c>Code:</c> "eierkommune"
    /// - <c>Description:</c> "Eierkommune"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:eierkommune"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Owning Municipality" - "Owning Municipality"
    ///   - NN: "Eigarkommune" - "Eigarkommune"
    /// </remarks>
    public static ConstantDefinition<Role> OwningMunicipality { get; } = new ConstantDefinition<Role>("9ce84a4d-4970-4ef2-8208-b8b8f4d45556")
    {
        Entity = new()
        {
            Name = "Eierkommune",
            Code = "eierkommune",
            Description = "Eierkommune",
            Urn = "urn:altinn:external-role:ccr:eierkommune",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Owning Municipality"),
            KeyValuePair.Create("Description", "Owning Municipality")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Eigarkommune"),
            KeyValuePair.Create("Description", "Eigarkommune")
        ),
    };

    /// <summary>
    /// Represents the Estate Administrator role ("Bobestyrer").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2cacfb35-2346-4a8d-95f6-b6fa4206881c
    /// - <c>Name:</c> "Bobestyrer"
    /// - <c>Code:</c> "bostyrer"
    /// - <c>Description:</c> "Bestyrer av et konkursbo eller dødsbo som er under offentlig skiftebehandling"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:bostyrer"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Estate Administrator" - "Administrator of a bankruptcy or probate estate under public administration"
    ///   - NN: "Bobestyrar" - "Bestyrar av eit konkursbo eller dødsbo som er under offentleg skiftehandtering"
    /// </remarks>
    public static ConstantDefinition<Role> EstateAdministrator { get; } = new ConstantDefinition<Role>("2cacfb35-2346-4a8d-95f6-b6fa4206881c")
    {
        Entity = new()
        {
            Name = "Bobestyrer",
            Code = "bostyrer",
            Description = "Bestyrer av et konkursbo eller dødsbo som er under offentlig skiftebehandling",
            Urn = "urn:altinn:external-role:ccr:bostyrer",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Estate Administrator"),
            KeyValuePair.Create("Description", "Administrator of a bankruptcy or probate estate under public administration")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bobestyrar"),
            KeyValuePair.Create("Description", "Bestyrar av eit konkursbo eller dødsbo som er under offentleg skiftehandtering")
        ),
    };

    /// <summary>
    /// Represents the Healthcare Institution role ("Helseforetak").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e4674211-034a-45f3-99ac-b2356984968a
    /// - <c>Name:</c> "Helseforetak"
    /// - <c>Code:</c> "helseforetak"
    /// - <c>Description:</c> "Helseforetak"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:helseforetak"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Healthcare Institution" - "Healthcare Institution"
    ///   - NN: "Helseforetak" - "Helseforetak"
    /// </remarks>
    public static ConstantDefinition<Role> HealthcareInstitution { get; } = new ConstantDefinition<Role>("e4674211-034a-45f3-99ac-b2356984968a")
    {
        Entity = new()
        {
            Name = "Helseforetak",
            Code = "helseforetak",
            Description = "Helseforetak",
            Urn = "urn:altinn:external-role:ccr:helseforetak",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Healthcare Institution"),
            KeyValuePair.Create("Description", "Healthcare Institution")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Helseforetak"),
            KeyValuePair.Create("Description", "Helseforetak")
        ),
    };

    /// <summary>
    /// Represents the Auditor role ("Revisor").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f76b997a-9bd8-4f7b-899f-fcd85d35669f
    /// - <c>Name:</c> "Revisor"
    /// - <c>Code:</c> "revisor"
    /// - <c>Description:</c> "Revisor"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:revisor"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Auditor" - "Auditor"
    ///   - NN: "Revisor" - "Revisor"
    /// </remarks>
    public static ConstantDefinition<Role> Auditor { get; } = new ConstantDefinition<Role>("f76b997a-9bd8-4f7b-899f-fcd85d35669f")
    {
        Entity = new()
        {
            Name = "Revisor",
            Code = "revisor",
            Description = "Revisor",
            Urn = "urn:altinn:external-role:ccr:revisor",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Auditor"),
            KeyValuePair.Create("Description", "Auditor")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Revisor"),
            KeyValuePair.Create("Description", "Revisor")
        ),
    };

    /// <summary>
    /// Represents the Business Manager role ("Forretningsfører").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 348b2f47-47ee-4084-abf8-68aa54c2b27f
    /// - <c>Name:</c> "Forretningsfører"
    /// - <c>Code:</c> "forretningsforer"
    /// - <c>Description:</c> "Forretningsfører"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:forretningsforer"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Business Manager" - "Business Manager"
    ///   - NN: "Forretningsførar" - "Forretningsførar"
    /// </remarks>
    public static ConstantDefinition<Role> BusinessManager { get; } = new ConstantDefinition<Role>("348b2f47-47ee-4084-abf8-68aa54c2b27f")
    {
        Entity = new()
        {
            Name = "Forretningsfører",
            Code = "forretningsforer",
            Description = "Forretningsfører",
            Urn = "urn:altinn:external-role:ccr:forretningsforer",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Business Manager"),
            KeyValuePair.Create("Description", "Business Manager")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Forretningsførar"),
            KeyValuePair.Create("Description", "Forretningsførar")
        ),
    };

    /// <summary>
    /// Represents the General Partner role ("Komplementar").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> cfcf75af-9902-41f7-ab47-b77ba60bcae5
    /// - <c>Name:</c> "Komplementar"
    /// - <c>Code:</c> "komplementar"
    /// - <c>Description:</c> "Komplementar"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:komplementar"
    /// - <c>IsKeyRole:</c> true
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "General Partner" - "General Partner"
    ///   - NN: "Komplementar" - "Komplementar"
    /// </remarks>
    public static ConstantDefinition<Role> GeneralPartner { get; } = new ConstantDefinition<Role>("cfcf75af-9902-41f7-ab47-b77ba60bcae5")
    {
        Entity = new()
        {
            Name = "Komplementar",
            Code = "komplementar",
            Description = "Komplementar",
            Urn = "urn:altinn:external-role:ccr:komplementar",
            IsKeyRole = true,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "General Partner"),
            KeyValuePair.Create("Description", "General Partner")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Komplementar"),
            KeyValuePair.Create("Description", "Komplementar")
        ),
    };

    /// <summary>
    /// Represents the Bankrupt Debtor role ("Konkursdebitor").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 50cc3f41-4dde-4417-8c04-eea428f169dd
    /// - <c>Name:</c> "Konkursdebitor"
    /// - <c>Code:</c> "konkursdebitor"
    /// - <c>Description:</c> "Konkursdebitor"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:konkursdebitor"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Bankrupt Debtor" - "Bankrupt Debtor"
    ///   - NN: "Konkursdebitor" - "Konkursdebitor"
    /// </remarks>
    public static ConstantDefinition<Role> BankruptDebtor { get; } = new ConstantDefinition<Role>("50cc3f41-4dde-4417-8c04-eea428f169dd")
    {
        Entity = new()
        {
            Name = "Konkursdebitor",
            Code = "konkursdebitor",
            Description = "Konkursdebitor",
            Urn = "urn:altinn:external-role:ccr:konkursdebitor",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bankrupt Debtor"),
            KeyValuePair.Create("Description", "Bankrupt Debtor")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Konkursdebitor"),
            KeyValuePair.Create("Description", "Konkursdebitor")
        ),
    };

    /// <summary>
    /// Represents the Part of a Church Council role ("Inngår i kirkelig fellesråd").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d78dd1d8-a3f3-4ae6-807e-ea5149f47035
    /// - <c>Name:</c> "Inngår i kirkelig fellesråd"
    /// - <c>Code:</c> "kirkelig-fellesraad"
    /// - <c>Description:</c> "Inngår i kirkelig fellesråd"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:kirkelig-fellesraad"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Part of a Church Council" - "Part of a Church Council"
    ///   - NN: "Inngår i kyrkjeleg fellesråd" - "Inngår i kyrkjeleg fellesråd"
    /// </remarks>
    public static ConstantDefinition<Role> PartOfChurchCouncil { get; } = new ConstantDefinition<Role>("d78dd1d8-a3f3-4ae6-807e-ea5149f47035")
    {
        Entity = new()
        {
            Name = "Inngår i kirkelig fellesråd",
            Code = "kirkelig-fellesraad",
            Description = "Inngår i kirkelig fellesråd",
            Urn = "urn:altinn:external-role:ccr:kirkelig-fellesraad",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Part of a Church Council"),
            KeyValuePair.Create("Description", "Part of a Church Council")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Inngår i kyrkjeleg fellesråd"),
            KeyValuePair.Create("Description", "Inngår i kyrkjeleg fellesråd")
        ),
    };

    /// <summary>
    /// Represents the Information about the Company in the Home Country role ("Opplysninger om foretaket i hjemlandet").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 185f623b-f614-4a83-839c-1788764bd253
    /// - <c>Name:</c> "Opplysninger om foretaket i hjemlandet"
    /// - <c>Code:</c> "hovedforetak"
    /// - <c>Description:</c> "Opplysninger om foretaket i hjemlandet"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:hovedforetak"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Information about the Company in the Home Country" - "Information about the company in the home country"
    ///   - NN: "Opplysningar om foretaket i heimalandet" - "Opplysningar om foretaket i heimalandet"
    /// </remarks>
    public static ConstantDefinition<Role> InformationCompanyHomeCountry { get; } = new ConstantDefinition<Role>("185f623b-f614-4a83-839c-1788764bd253")
    {
        Entity = new()
        {
            Name = "Opplysninger om foretaket i hjemlandet",
            Code = "hovedforetak",
            Description = "Opplysninger om foretaket i hjemlandet",
            Urn = "urn:altinn:external-role:ccr:hovedforetak",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Information about the Company in the Home Country"),
            KeyValuePair.Create("Description", "Information about the company in the home country")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Opplysningar om foretaket i heimalandet"),
            KeyValuePair.Create("Description", "Opplysningar om foretaket i heimalandet")
        ),
    };

    /// <summary>
    /// Represents the Accountant role ("Regnskapsfører").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 46e27685-b3ba-423e-8b42-faab54de5817
    /// - <c>Name:</c> "Regnskapsfører"
    /// - <c>Code:</c> "regnskapsforer"
    /// - <c>Description:</c> "Regnskapsfører"
    /// - <c>Urn:</c> "urn:altinn:external-role:ccr:regnskapsforer"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.CentralCoordinatingRegister"/>
    /// - <c>Translations:</c>
    ///   - EN: "Accountant" - "Accountant"
    ///   - NN: "Reknskapsførar" - "Reknskapsførar"
    /// </remarks>
    public static ConstantDefinition<Role> Accountant { get; } = new ConstantDefinition<Role>("46e27685-b3ba-423e-8b42-faab54de5817")
    {
        Entity = new()
        {
            Name = "Regnskapsfører",
            Code = "regnskapsforer",
            Description = "Regnskapsfører",
            Urn = "urn:altinn:external-role:ccr:regnskapsforer",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.CentralCoordinatingRegister.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accountant"),
            KeyValuePair.Create("Description", "Accountant")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Reknskapsførar"),
            KeyValuePair.Create("Description", "Reknskapsførar")
        ),
    };

    #endregion

    #region Altinn 2 Role Codes

    /// <summary>
    /// Represents the Primary Industry and Foodstuff role ("Primærnæring og næringsmiddel").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> c497b499-7e98-423d-9fe7-ad5a6c3b71ad
    /// - <c>Name:</c> "Primærnæring og næringsmiddel"
    /// - <c>Code:</c> "A0212"
    /// - <c>Description:</c> "Denne rollen gir rettighet til tjenester innen import, foredling, produksjon og/eller salg av primærnæringsprodukter og andre næringsmiddel, samt dyrehold, akvakultur, planter og kosmetikk. Ved regelverksendringer eller innføring av nye digitale tjenester"
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0212"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Primary industry and foodstuff" - "Import, processing, production and/or sales of primary products and other foodstuff. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Primærnæring og næringsmiddel" - "Import, foredling, produksjon og/eller sal av primærnæringsprodukter og andre næringsmiddel. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> PrimaryIndustryAndFoodstuff { get; } = new ConstantDefinition<Role>("c497b499-7e98-423d-9fe7-ad5a6c3b71ad")
    {
        Entity = new()
        {
            Name = "Primærnæring og næringsmiddel",
            Code = "A0212",
            Description = "Denne rollen gir rettighet til tjenester innen import, foredling, produksjon og/eller salg av primærnæringsprodukter og andre næringsmiddel, samt dyrehold, akvakultur, planter og kosmetikk. Ved regelverksendringer eller innføring av nye digitale tjenester",
            Urn = "urn:altinn:rolecode:A0212",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Primary industry and foodstuff"),
            KeyValuePair.Create("Description", "Import, processing, production and/or sales of primary products and other foodstuff. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Primærnæring og næringsmiddel"),
            KeyValuePair.Create("Description", "Import, foredling, produksjon og/eller sal av primærnæringsprodukter og andre næringsmiddel. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Mail/Archive role ("Post/arkiv").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 151955ec-d8aa-4c14-a435-ffa96b26a9fb
    /// - <c>Name:</c> "Post/arkiv"
    /// - <c>Code:</c> "A0236"
    /// - <c>Description:</c> "Denne rollen gir rettighet til å lese meldinger som blir sendt til brukerens meldingsboks. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0236"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Mail/archive" - "Access to read correpondences. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Post/arkiv" - "Rolle som gjer rett til å lese meldingstenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> MailArchive { get; } = new ConstantDefinition<Role>("151955ec-d8aa-4c14-a435-ffa96b26a9fb")
    {
        Entity = new()
        {
            Name = "Post/arkiv",
            Code = "A0236",
            Description = "Denne rollen gir rettighet til å lese meldinger som blir sendt til brukerens meldingsboks. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0236",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Mail/archive"),
            KeyValuePair.Create("Description", "Access to read correpondences. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Post/arkiv"),
            KeyValuePair.Create("Description", "Rolle som gjer rett til å lese meldingstenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Auditor in Charge role ("Ansvarlig revisor").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> c2884487-a634-4537-95b4-bafb917b62a8
    /// - <c>Name:</c> "Ansvarlig revisor"
    /// - <c>Code:</c> "A0237"
    /// - <c>Description:</c> "Delegerbar revisorrolle med signeringsrettighet.Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0237"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Auditor in charge" - "Delegateble auditor role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Ansvarleg revisor" - "Delegerbar revisorrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> AuditorInCharge { get; } = new ConstantDefinition<Role>("c2884487-a634-4537-95b4-bafb917b62a8")
    {
        Entity = new()
        {
            Name = "Ansvarlig revisor",
            Code = "A0237",
            Description = "Delegerbar revisorrolle med signeringsrettighet.Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0237",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Auditor in charge"),
            KeyValuePair.Create("Description", "Delegateble auditor role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Ansvarleg revisor"),
            KeyValuePair.Create("Description", "Delegerbar revisorrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Assistant Auditor role ("Revisormedarbeider").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 10fcad57-7a91-4e02-a921-63e5751fbc24
    /// - <c>Name:</c> "Revisormedarbeider"
    /// - <c>Code:</c> "A0238"
    /// - <c>Description:</c> "Denne rollen gir revisor rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0238"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Assistant auditor" - "Delegateble auditor role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Revisormedarbeidar" - "Delegerbar revisorrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> AssistantAuditor { get; } = new ConstantDefinition<Role>("10fcad57-7a91-4e02-a921-63e5751fbc24")
    {
        Entity = new()
        {
            Name = "Revisormedarbeider",
            Code = "A0238",
            Description = "Denne rollen gir revisor rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0238",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Assistant auditor"),
            KeyValuePair.Create("Description", "Delegateble auditor role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Revisormedarbeidar"),
            KeyValuePair.Create("Description", "Delegerbar revisorrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Accountant with Signing Rights role ("Regnskapsfører med signeringsrettighet").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ebed65a5-dd87-4180-b898-e1da249b128d
    /// - <c>Name:</c> "Regnskapsfører med signeringsrettighet"
    /// - <c>Code:</c> "A0239"
    /// - <c>Description:</c> "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester, samt signeringsrettighet for tjenestene. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0239"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Accountant with signing rights" - "Delegateble accountant role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Rekneskapsførar med signeringsrett" - "Delegerbar rekneskapsrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> AccountantWithSigningRights { get; } = new ConstantDefinition<Role>("ebed65a5-dd87-4180-b898-e1da249b128d")
    {
        Entity = new()
        {
            Name = "Regnskapsfører med signeringsrettighet",
            Code = "A0239",
            Description = "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester, samt signeringsrettighet for tjenestene. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0239",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accountant with signing rights"),
            KeyValuePair.Create("Description", "Delegateble accountant role with signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rekneskapsførar med signeringsrett"),
            KeyValuePair.Create("Description", "Delegerbar rekneskapsrolle med signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Accountant without Signing Rights role ("Regnskapsfører uten signeringsrettighet").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9407620b-21b6-4538-b4d8-2b4eb339c373
    /// - <c>Name:</c> "Regnskapsfører uten signeringsrettighet"
    /// - <c>Code:</c> "A0240"
    /// - <c>Description:</c> "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0240"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Accountant without signing rights" - "Delegateble accountant role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Rekneskapsførar utan signeringsrett" - "Delegerbar rekneskapsrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> AccountantWithoutSigningRights { get; } = new ConstantDefinition<Role>("9407620b-21b6-4538-b4d8-2b4eb339c373")
    {
        Entity = new()
        {
            Name = "Regnskapsfører uten signeringsrettighet",
            Code = "A0240",
            Description = "Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0240",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accountant without signing rights"),
            KeyValuePair.Create("Description", "Delegateble accountant role without signing right. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rekneskapsførar utan signeringsrett"),
            KeyValuePair.Create("Description", "Delegerbar rekneskapsrolle utan signeringsrett. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Accountant Salary role ("Regnskapsfører lønn").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 723a43ab-13d8-4585-81e2-e4c734b2d4fc
    /// - <c>Name:</c> "Regnskapsfører lønn"
    /// - <c>Code:</c> "A0241"
    /// - <c>Description:</c> "Denne rollen gir regnskapsfører rettighet til lønnsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir."
    /// - <c>Urn:</c> "urn:altinn:rolecode:A0241"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References organization entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Accountant salary" - "Delegateble accountant role with signing right to services related to salary. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides."
    ///   - NN: "Rekneskapsførar løn" - "Delegerbar rekneskapsrolle med signeringsrett for tenester knytta til lønsrapportering. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir"
    /// </remarks>
    public static ConstantDefinition<Role> AccountantSalary { get; } = new ConstantDefinition<Role>("723a43ab-13d8-4585-81e2-e4c734b2d4fc")
    {
        Entity = new()
        {
            Name = "Regnskapsfører lønn",
            Code = "A0241",
            Description = "Denne rollen gir regnskapsfører rettighet til lønnsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0241",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accountant salary"),
            KeyValuePair.Create("Description", "Delegateble accountant role with signing right to services related to salary. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rekneskapsførar løn"),
            KeyValuePair.Create("Description", "Delegerbar rekneskapsrolle med signeringsrett for tenester knytta til lønsrapportering. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Private Person role ("Privatperson").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1c6eeec1-fe70-4fc5-8b45-df4a2255dea6
    /// - <c>Name:</c> "Privatperson"
    /// - <c>Code:</c> "privatperson"
    /// - <c>Description:</c> "Denne rollen er hentet fra Folkeregisteret og gir rettighet til flere tjenester."
    /// - <c>Urn:</c> "urn:altinn:role:privatperson"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References person entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Private person" - "Private person"
    ///   - NN: "Privatperson" - "Privatperson"
    /// </remarks>
    public static ConstantDefinition<Role> PrivatePerson { get; } = new ConstantDefinition<Role>("1c6eeec1-fe70-4fc5-8b45-df4a2255dea6")
    {
        Entity = new()
        {
            Name = "Privatperson",
            Code = "privatperson",
            Description = "Denne rollen er hentet fra Folkeregisteret og gir rettighet til flere tjenester.",
            Urn = "urn:altinn:role:privatperson",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Person.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Private person"),
            KeyValuePair.Create("Description", "Private person")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Privatperson"),
            KeyValuePair.Create("Description", "Privatperson")
        ),
    };

    /// <summary>
    /// Represents the Self Registered User role ("Selvregistrert bruker").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e16ab886-1e1e-4f45-8f79-46f06f720f3e
    /// - <c>Name:</c> "Selvregistrert bruker"
    /// - <c>Code:</c> "selvregistrert"
    /// - <c>Description:</c> "Selvregistrert bruker"
    /// - <c>Urn:</c> "urn:altinn:role:selvregistrert"
    /// - <c>IsKeyRole:</c> false
    /// - <c>IsAssignable:</c> false
    /// - <c>EntityTypeId:</c> References person entity type
    /// - <c>ProviderId:</c> References <see cref="ProviderConstants.Altinn2"/>
    /// - <c>Translations:</c>
    ///   - EN: "Self registered user" - "Self registered user"
    ///   - NN: "Sjølregistrert brukar" - "Sjølregistrert brukar"
    /// </remarks>
    public static ConstantDefinition<Role> SelfRegisteredUser { get; } = new ConstantDefinition<Role>("e16ab886-1e1e-4f45-8f79-46f06f720f3e")
    {
        Entity = new()
        {
            Name = "Selvregistrert bruker",
            Code = "selvregistrert",
            Description = "Selvregistrert bruker",
            Urn = "urn:altinn:role:selvregistrert",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Person.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Self registered user"),
            KeyValuePair.Create("Description", "Self registered user")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Sjølregistrert brukar"),
            KeyValuePair.Create("Description", "Sjølregistrert brukar")
        ),
    };

    /// <summary>
    /// Represents the Planning and Construction role ("Plan- og byggesak").
    /// </summary>
    public static ConstantDefinition<Role> PlanningAndConstruction { get; } = new ConstantDefinition<Role>("6828080b-e846-4c51-b670-201af4917562")
    {
        Entity = new()
        {
            Name = "Plan- og byggesak",
            Code = "A0278",
            Description = "Rollen er forbeholdt skjemaer og tjenester som er godkjent av Direktoratet for byggkvalitet (DiBK). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0278",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Planning and construction"),
            KeyValuePair.Create("Description", "The role is reserved for forms and services approved by Norwegian Building Authority (DiBK). In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Plan- og byggesak"),
            KeyValuePair.Create("Description", "Rollen er reservert skjema og tenester som er godkjend av DiBK. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the Access Manager role ("Tilgangsstyring").
    /// </summary>
    public static ConstantDefinition<Role> AccessManager { get; } = new ConstantDefinition<Role>("48f9e5ec-efd5-4863-baba-9697b8971666")
    {
        Entity = new()
        {
            Name = "Tilgangsstyring",
            Code = "ADMAI",
            Description = "Denne rollen gir administratortilgang til å gi videre rettigheter til andre.",
            Urn = "urn:altinn:rolecode:ADMAI",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Access manager"),
            KeyValuePair.Create("Description", "Administration of access")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Tilgangsstyring"),
            KeyValuePair.Create("Description", "Administrasjon av tilgangar")
        ),
    };

    /// <summary>
    /// Represents the Application Programming Interface (API) role ("Programmeringsgrensesnitt (API)").
    /// </summary>
    public static ConstantDefinition<Role> ApplicationProgrammingInterface { get; } = new ConstantDefinition<Role>("e078bb18-f55a-4a2d-8964-c599f41b29b5")
    {
        Entity = new()
        {
            Name = "Programmeringsgrensesnitt (API)",
            Code = "APIADM",
            Description = "Delegerbar rolle som gir tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.",
            Urn = "urn:altinn:rolecode:APIADM",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation.Id,
            ProviderId = ProviderConstants.Altinn2.Id,
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Application Programming Interface (API)"),
            KeyValuePair.Create("Description", "Delegable role that provides access to manage access to APIs on behalf of the business.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Programmeringsgrensesnitt (API)"),
            KeyValuePair.Create("Description", "Delegerbar rolle som gir tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av verksemden.")
        ),
    };

    #endregion
}
