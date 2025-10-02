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
    /// Try to get <see cref="Role"/> by Code.
    /// </summary>
    public static bool TryGetByCode(string code, [NotNullWhen(true)] out ConstantDefinition<Role>? result)
        => ConstantLookup.TryGetByCode(typeof(RoleConstants), code, out result);

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
    /// Represents the 'Rettighetshaver' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 42cae370-2dc1-4fdc-9c67-c2f4b0f0f829
    /// - <c>URN:</c> urn:altinn:role:rettighetshaver
    /// - <c>Provider:</c> Altinn3
    /// - <c>Code:</c> rettighetshaver
    /// - <c>Description:</c> Gir mulighet til å motta delegerte fullmakter for virksomheten
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
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
    /// Represents the 'Agent' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ff4c33f5-03f7-4445-85ed-1e60b8aafb30
    /// - <c>URN:</c> urn:altinn:role:agent
    /// - <c>Provider:</c> Altinn3
    /// - <c>Code:</c> agent
    /// - <c>Description:</c> Gir mulighet til å motta delegerte fullmakter for virksomheten
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
            EntityTypeId = EntityTypeConstants.Person,
            ProviderId = ProviderConstants.Altinn3,
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
    /// Represents the 'Hovedadministrator' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ba1c261c-20ec-44e2-9e0b-4e7cfe9f36e7
    /// - <c>URN:</c> urn:altinn:role:hovedadministrator
    /// - <c>Provider:</c> Altinn3
    /// - <c>Code:</c> hovedadministrator
    /// - <c>Description:</c> Intern rolle for å samle alle delegerbare fullmakter en hovedadministrator kan utføre for virksomheten
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn3,
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
    /// Represents the 'Administrativ enhet - offentlig sektor' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 66ad5542-4f4a-4606-996f-18690129ce00
    /// - <c>URN:</c> urn:altinn:external-role:ccr:administrativ-enhet-offentlig-sektor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> administrativ-enhet-offentlig-sektor
    /// - <c>Description:</c> Administrativ enhet - offentlig sektor
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Nestleder' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 29a24eab-a25f-445d-b56d-e3b914844853
    /// - <c>URN:</c> urn:altinn:external-role:ccr:nestleder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> nestleder
    /// - <c>Description:</c> Styremedlem som opptrer som styreleder ved leders fravær
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Inngår i kontorfellesskap' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 8c1e91c2-a71c-4abf-a74e-a600a98be976
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontorfelleskapmedlem
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontorfelleskapmedlem
    /// - <c>Description:</c> Inngår i kontorfellesskap
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Organisasjonsledd i offentlig sektor' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> cfc41a92-2061-4ff4-97dc-658ffba2c00e
    /// - <c>URN:</c> urn:altinn:external-role:ccr:organisasjonsledd-offentlig-sektor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> organisasjonsledd-offentlig-sektor
    /// - <c>Description:</c> Organisasjonsledd i offentlig sektor
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Særskilt oppdelt enhet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2fec6d4b-cead-419a-adf3-1bf482a3c9dc
    /// - <c>URN:</c> urn:altinn:external-role:ccr:saerskilt-oppdelt-enhet
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> saerskilt-oppdelt-enhet
    /// - <c>Description:</c> Særskilt oppdelt enhet
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Daglig leder' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 55bd7d4d-08dd-46ee-ac8e-3a44d800d752
    /// - <c>URN:</c> urn:altinn:external-role:ccr:daglig-leder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> daglig-leder
    /// - <c>Description:</c> Fysisk- eller juridisk person som har ansvaret for den daglige driften i en virksomhet
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Deltaker delt ansvar' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 18baa914-ac43-4663-9fa4-6f5760dc68eb
    /// - <c>URN:</c> urn:altinn:external-role:ccr:deltaker-delt-ansvar
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> deltaker-delt-ansvar
    /// - <c>Description:</c> Fysisk- eller juridisk person som har personlig ansvar for deler av selskapets forpliktelser
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Innehaver' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2651ed07-f31b-4bc1-87bd-4d270742a19d
    /// - <c>URN:</c> urn:altinn:external-role:ccr:innehaver
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> innehaver
    /// - <c>Description:</c> Fysisk person som er eier av et enkeltpersonforetak
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Deltaker fullt ansvar' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f1021b8c-9fbc-4296-bd17-a05d713037ef
    /// - <c>URN:</c> urn:altinn:external-role:ccr:deltaker-fullt-ansvar
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> deltaker-fullt-ansvar
    /// - <c>Description:</c> Fysisk- eller juridisk person som har ubegrenset, personlig ansvar for selskapets forpliktelser
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Varamedlem' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d41d67f2-15b0-4c82-95db-b8d5baaa14a4
    /// - <c>URN:</c> urn:altinn:external-role:ccr:varamedlem
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> varamedlem
    /// - <c>Description:</c> Fysisk- eller juridisk person som er stedfortreder for et styremedlem
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Observatør' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1f8a2518-9494-468a-80a0-7405f0daf9e9
    /// - <c>URN:</c> urn:altinn:external-role:ccr:observator
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> observator
    /// - <c>Description:</c> Fysisk person som deltar i styremøter i en virksomhet, men uten stemmerett
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Styremedlem' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f045ffda-dbdc-41da-b674-b9b276ad5b01
    /// - <c>URN:</c> urn:altinn:external-role:ccr:styremedlem
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> styremedlem
    /// - <c>Description:</c> Fysisk- eller juridisk person som inngår i et styre
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Styrets leder' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9e5d3acf-cef7-4bbe-b101-8e9ab7b8b3e4
    /// - <c>URN:</c> urn:altinn:external-role:ccr:styreleder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> styreleder
    /// - <c>Description:</c> Fysisk- eller juridisk person som er styremedlem og leder et styre
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Den personlige konkursen angår' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2e2fc06e-d9b7-4cd9-91bc-d5de766d20de
    /// - <c>URN:</c> urn:altinn:external-role:ccr:personlige-konkurs
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> personlige-konkurs
    /// - <c>Description:</c> Den personlige konkursen angår
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Norsk representant for utenlandsk enhet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e852d758-e8dd-41ec-a1e2-4632deb6857d
    /// - <c>URN:</c> urn:altinn:external-role:ccr:norsk-representant
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> norsk-representant
    /// - <c>Description:</c> Fysisk- eller juridisk person som har ansvaret for den daglige driften i Norge
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Kontaktperson' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> db013059-4a8a-442d-bf90-b03539fe5dda
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontaktperson
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontaktperson
    /// - <c>Description:</c> Fysisk person som representerer en virksomhet
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Kontaktperson NUF' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 69c4397a-9e34-4e73-9f69-534bc1bb74c8
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontaktperson-nuf
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontaktperson-nuf
    /// - <c>Description:</c> Fysisk person som representerer en virksomhet - NUF
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Bestyrende reder' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 8f0cf433-954e-4680-a25d-a3cf9ffdf149
    /// - <c>URN:</c> urn:altinn:external-role:ccr:bestyrende-reder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> bestyrende-reder
    /// - <c>Description:</c> Bestyrende reder
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Eierkommune' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9ce84a4d-4970-4ef2-8208-b8b8f4d45556
    /// - <c>URN:</c> urn:altinn:external-role:ccr:eierkommune
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> eierkommune
    /// - <c>Description:</c> Eierkommune
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Bobestyrer' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2cacfb35-2346-4a8d-95f6-b6fa4206881c
    /// - <c>URN:</c> urn:altinn:external-role:ccr:bostyrer
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> bostyrer
    /// - <c>Description:</c> Bestyrer av et konkursbo eller dødsbo som er under offentlig skiftebehandling
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Helseforetak' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e4674211-034a-45f3-99ac-b2356984968a
    /// - <c>URN:</c> urn:altinn:external-role:ccr:helseforetak
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> helseforetak
    /// - <c>Description:</c> Helseforetak
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Revisor' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f76b997a-9bd8-4f7b-899f-fcd85d35669f
    /// - <c>URN:</c> urn:altinn:external-role:ccr:revisor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> revisor
    /// - <c>Description:</c> Revisor
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Forretningsfører' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 348b2f47-47ee-4084-abf8-68aa54c2b27f
    /// - <c>URN:</c> urn:altinn:external-role:ccr:forretningsforer
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> forretningsforer
    /// - <c>Description:</c> Forretningsfører
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Komplementar' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> cfcf75af-9902-41f7-ab47-b77ba60bcae5
    /// - <c>URN:</c> urn:altinn:external-role:ccr:komplementar
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> komplementar
    /// - <c>Description:</c> Komplementar
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Konkursdebitor' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 50cc3f41-4dde-4417-8c04-eea428f169dd
    /// - <c>URN:</c> urn:altinn:external-role:ccr:konkursdebitor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> konkursdebitor
    /// - <c>Description:</c> Konkursdebitor
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Inngår i kirkelig fellesråd' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d78dd1d8-a3f3-4ae6-807e-ea5149f47035
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kirkelig-fellesraad
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kirkelig-fellesraad
    /// - <c>Description:</c> Inngår i kirkelig fellesråd
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Opplysninger om foretaket i hjemlandet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 185f623b-f614-4a83-839c-1788764bd253
    /// - <c>URN:</c> urn:altinn:external-role:ccr:hovedforetak
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> hovedforetak
    /// - <c>Description:</c> Opplysninger om foretaket i hjemlandet
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Regnskapsfører' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 46e27685-b3ba-423e-8b42-faab54de5817
    /// - <c>URN:</c> urn:altinn:external-role:ccr:regnskapsforer
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> regnskapsforer
    /// - <c>Description:</c> Regnskapsfører
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
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
    /// Represents the 'Primærnæring og næringsmiddel' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> c497b499-7e98-423d-9fe7-ad5a6c3b71ad
    /// - <c>URN:</c> urn:altinn:rolecode:A0212
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0212
    /// - <c>Description:</c> Denne rollen gir rettighet til tjenester innen import, foredling, produksjon og/eller salg av primærnæringsprodukter og andre næringsmiddel, samt dyrehold, akvakultur, planter og kosmetikk. Ved regelverksendringer eller innføring av nye digitale tjenester
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Post/arkiv' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 151955ec-d8aa-4c14-a435-ffa96b26a9fb
    /// - <c>URN:</c> urn:altinn:rolecode:A0236
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0236
    /// - <c>Description:</c> Denne rollen gir rettighet til å lese meldinger som blir sendt til brukerens meldingsboks. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Ansvarlig revisor' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> c2884487-a634-4537-95b4-bafb917b62a8
    /// - <c>URN:</c> urn:altinn:rolecode:A0237
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0237
    /// - <c>Description:</c> Delegerbar revisorrolle med signeringsrettighet.Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> A0237 { get; } = new ConstantDefinition<Role>("c2884487-a634-4537-95b4-bafb917b62a8")
    {
        Entity = new()
        {
            Name = "Ansvarlig revisor",
            Code = "A0237",
            Description = "Delegerbar revisorrolle med signeringsrettighet.Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0237",
            IsKeyRole = false,
            IsAssignable = false,
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Revisormedarbeider' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 10fcad57-7a91-4e02-a921-63e5751fbc24
    /// - <c>URN:</c> urn:altinn:rolecode:A0238
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0238
    /// - <c>Description:</c> Denne rollen gir revisor rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Regnskapsfører med signeringsrettighet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ebed65a5-dd87-4180-b898-e1da249b128d
    /// - <c>URN:</c> urn:altinn:rolecode:A0239
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0239
    /// - <c>Description:</c> Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester, samt signeringsrettighet for tjenestene. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Regnskapsfører uten signeringsrettighet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9407620b-21b6-4538-b4d8-2b4eb339c373
    /// - <c>URN:</c> urn:altinn:rolecode:A0240
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0240
    /// - <c>Description:</c> Denne rollen gir regnskapsfører rettighet til aktuelle skjema og tjenester. Denne gir ikke rettighet til å signere. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Regnskapsfører lønn' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 723a43ab-13d8-4585-81e2-e4c734b2d4fc
    /// - <c>URN:</c> urn:altinn:rolecode:A0241
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0241
    /// - <c>Description:</c> Denne rollen gir regnskapsfører rettighet til lønnsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Privatperson' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1c6eeec1-fe70-4fc5-8b45-df4a2255dea6
    /// - <c>URN:</c> urn:altinn:role:privatperson
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> privatperson
    /// - <c>Description:</c> Denne rollen er hentet fra Folkeregisteret og gir rettighet til flere tjenester.
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
            EntityTypeId = EntityTypeConstants.Person,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Selvregistrert bruker' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e16ab886-1e1e-4f45-8f79-46f06f720f3e
    /// - <c>URN:</c> urn:altinn:role:selvregistrert
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> selvregistrert
    /// - <c>Description:</c> Selvregistrert bruker
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
            EntityTypeId = EntityTypeConstants.Person,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Plan- og byggesak' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 6828080b-e846-4c51-b670-201af4917562
    /// - <c>URN:</c> urn:altinn:rolecode:A0278
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0278
    /// - <c>Description:</c> Rollen er forbeholdt skjemaer og tjenester som er godkjent av Direktoratet for byggkvalitet (DiBK). Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Tilgangsstyring' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 48f9e5ec-efd5-4863-baba-9697b8971666
    /// - <c>URN:</c> urn:altinn:rolecode:ADMAI
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> ADMAI
    /// - <c>Description:</c> Denne rollen gir administratortilgang til å gi videre rettigheter til andre.
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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
    /// Represents the 'Programmeringsgrensesnitt (API)' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e078bb18-f55a-4a2d-8964-c599f41b29b5
    /// - <c>URN:</c> urn:altinn:rolecode:APIADM
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> APIADM
    /// - <c>Description:</c> Delegerbar rolle som gir tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.
    /// </remarks>
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
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
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

    /// <summary>
    /// Represents the 'Er regnskapsforeradresse for' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 17cb6a9e-5d27-4a8e-9647-f3a53c7a09c6
    /// - <c>URN:</c> urn:altinn:external-role:ccr:regnskapsforeradressat
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> regnskapsforeradressat
    /// - <c>Description:</c> Er regnskapsforeradresse for
    /// </remarks>
    public static ConstantDefinition<Role> IsAccountingAddressFor { get; } = new ConstantDefinition<Role>("17cb6a9e-5d27-4a8e-9647-f3a53c7a09c6")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Er regnskapsforeradresse for",
            Code = "regnskapsforeradressat",
            Description = "Er regnskapsforeradresse for",
            Urn = "urn:altinn:external-role:ccr:regnskapsforeradressat",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Signatur' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> ea8f1038-9717-472d-a579-f32960f0eecb
    /// - <c>URN:</c> urn:altinn:external-role:ccr:signerer
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> signerer
    /// - <c>Description:</c> Signatur
    /// </remarks>
    public static ConstantDefinition<Role> Signatory { get; } = new ConstantDefinition<Role>("ea8f1038-9717-472d-a579-f32960f0eecb")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Signatur",
            Code = "signerer",
            Description = "Signatur",
            Urn = "urn:altinn:external-role:ccr:signerer",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Skal fusjoneres med' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 9822b632-3822-4a9e-b768-8411c046bb75
    /// - <c>URN:</c> urn:altinn:external-role:ccr:fusjonsovertaker
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> fusjonsovertaker
    /// - <c>Description:</c> Skal fusjoneres med
    /// </remarks>
    public static ConstantDefinition<Role> MergerTakeover { get; } = new ConstantDefinition<Role>("9822b632-3822-4a9e-b768-8411c046bb75")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Skal fusjoneres med",
            Code = "fusjonsovertaker",
            Description = "Skal fusjoneres med",
            Urn = "urn:altinn:external-role:ccr:fusjonsovertaker",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Skal fisjoneres med' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e9292053-92ee-42e0-a30c-011667ee8db8
    /// - <c>URN:</c> urn:altinn:external-role:ccr:fisjonsovertaker
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> fisjonsovertaker
    /// - <c>Description:</c> Skal fisjoneres med
    /// </remarks>
    public static ConstantDefinition<Role> DivisionTakeover { get; } = new ConstantDefinition<Role>("e9292053-92ee-42e0-a30c-011667ee8db8")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Skal fisjoneres med",
            Code = "fisjonsovertaker",
            Description = "Skal fisjoneres med",
            Urn = "urn:altinn:external-role:ccr:fisjonsovertaker",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Har som registreringsenhet BEDR' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 5f868b06-7531-448c-a275-a2dfa100f840
    /// - <c>URN:</c> urn:altinn:external-role:ccr:hovedenhet
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> hovedenhet
    /// - <c>Description:</c> Har som registreringsenhet
    /// </remarks>
    public static ConstantDefinition<Role> HasAsRegistrationUnitBEDR { get; } = new ConstantDefinition<Role>("5f868b06-7531-448c-a275-a2dfa100f840")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Har som registreringsenhet BEDR",
            Code = "hovedenhet",
            Description = "Har som registreringsenhet",
            Urn = "urn:altinn:external-role:ccr:hovedenhet",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Har som registreringsenhet AAFY' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> a53c833b-6dc1-4ceb-b56c-00d333c211c0
    /// - <c>URN:</c> urn:altinn:external-role:ccr:ikke-naeringsdrivende-hovedenhet
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> ikke-naeringsdrivende-hovedenhet
    /// - <c>Description:</c> Har som registreringsenhet
    /// </remarks>
    public static ConstantDefinition<Role> HasAsRegistrationUnitAAFY { get; } = new ConstantDefinition<Role>("a53c833b-6dc1-4ceb-b56c-00d333c211c0")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Har som registreringsenhet AAFY",
            Code = "ikke-naeringsdrivende-hovedenhet",
            Description = "Har som registreringsenhet",
            Urn = "urn:altinn:external-role:ccr:ikke-naeringsdrivende-hovedenhet",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Prokura i fellesskap' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f7c13f9b-8246-4a16-8b93-33e945b8cf5b
    /// - <c>URN:</c> urn:altinn:external-role:ccr:prokurist-fellesskap
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> prokurist-fellesskap
    /// - <c>Description:</c> Prokura i fellesskap
    /// </remarks>
    public static ConstantDefinition<Role> JointProcuration { get; } = new ConstantDefinition<Role>("f7c13f9b-8246-4a16-8b93-33e945b8cf5b")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Prokura i fellesskap",
            Code = "prokurist-fellesskap",
            Description = "Prokura i fellesskap",
            Urn = "urn:altinn:external-role:ccr:prokurist-fellesskap",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Prokura hver for seg' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e39b6f89-6e42-4ca4-8e21-913a632e9c95
    /// - <c>URN:</c> urn:altinn:external-role:ccr:prokurist-hver-for-seg
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> prokurist-hver-for-seg
    /// - <c>Description:</c> Prokura hver for seg
    /// </remarks>
    public static ConstantDefinition<Role> IndividualProcuration { get; } = new ConstantDefinition<Role>("e39b6f89-6e42-4ca4-8e21-913a632e9c95")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Prokura hver for seg",
            Code = "prokurist-hver-for-seg",
            Description = "Prokura hver for seg",
            Urn = "urn:altinn:external-role:ccr:prokurist-hver-for-seg",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Prokura' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 6aa99128-c901-4ab4-86cd-b5d92aeb0b80
    /// - <c>URN:</c> urn:altinn:external-role:ccr:prokurist
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> prokurist
    /// - <c>Description:</c> Prokura
    /// </remarks>
    public static ConstantDefinition<Role> Procuration { get; } = new ConstantDefinition<Role>("6aa99128-c901-4ab4-86cd-b5d92aeb0b80")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Prokura",
            Code = "prokurist",
            Description = "Prokura",
            Urn = "urn:altinn:external-role:ccr:prokurist",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Er revisoradresse for' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 2c812df3-cbb8-46cf-9071-f5fbb6c28ad2
    /// - <c>URN:</c> urn:altinn:external-role:ccr:revisoradressat
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> revisoradressat
    /// - <c>Description:</c> Er revisoradresse for
    /// </remarks>
    public static ConstantDefinition<Role> IsAuditorAddressFor { get; } = new ConstantDefinition<Role>("2c812df3-cbb8-46cf-9071-f5fbb6c28ad2")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Er revisoradresse for",
            Code = "revisoradressat",
            Description = "Er revisoradresse for",
            Urn = "urn:altinn:external-role:ccr:revisoradressat",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Sameiere' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 94df9e5c-7d52-43a2-91af-a50cf81fca2d
    /// - <c>URN:</c> urn:altinn:external-role:ccr:sameier
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> sameier
    /// - <c>Description:</c> Ekstern rolle
    /// </remarks>
    public static ConstantDefinition<Role> CoOwners { get; } = new ConstantDefinition<Role>("94df9e5c-7d52-43a2-91af-a50cf81fca2d")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Sameiere",
            Code = "sameier",
            Description = "Ekstern rolle",
            Urn = "urn:altinn:external-role:ccr:sameier",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Signatur i fellesskap' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 917dcbb9-8cb9-4d2d-984c-8f877b510747
    /// - <c>URN:</c> urn:altinn:external-role:ccr:signerer-fellesskap
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> signerer-fellesskap
    /// - <c>Description:</c> Signatur i fellesskap
    /// </remarks>
    public static ConstantDefinition<Role> JointSignature { get; } = new ConstantDefinition<Role>("917dcbb9-8cb9-4d2d-984c-8f877b510747")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Signatur i fellesskap",
            Code = "signerer-fellesskap",
            Description = "Signatur i fellesskap",
            Urn = "urn:altinn:external-role:ccr:signerer-fellesskap",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Signatur hver for seg' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> a6a94254-7459-4096-b889-411793febbee
    /// - <c>URN:</c> urn:altinn:external-role:ccr:signerer-hver-for-seg
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> signerer-hver-for-seg
    /// - <c>Description:</c> Signatur hver for seg
    /// </remarks>
    public static ConstantDefinition<Role> IndividualSignature { get; } = new ConstantDefinition<Role>("a6a94254-7459-4096-b889-411793febbee")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Signatur hver for seg",
            Code = "signerer-hver-for-seg",
            Description = "Signatur hver for seg",
            Urn = "urn:altinn:external-role:ccr:signerer-hver-for-seg",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Kontaktperson i kommune' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0fc0fc0b-d3e1-4360-982e-b1d0a798f374
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontaktperson-kommune
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontaktperson-kommune
    /// - <c>Description:</c> Ekstern rolle
    /// </remarks>
    public static ConstantDefinition<Role> ContactPersonInMunicipality { get; } = new ConstantDefinition<Role>("0fc0fc0b-d3e1-4360-982e-b1d0a798f374")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Kontaktperson i kommune",
            Code = "kontaktperson-kommune",
            Description = "Ekstern rolle",
            Urn = "urn:altinn:external-role:ccr:kontaktperson-kommune",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Kontaktperson i Ad' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 7f6c14f6-7809-4867-83ab-30c426b53d57
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontaktperson-ados
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontaktperson-ados
    /// - <c>Description:</c> enhet - offentlig sektor
    /// </remarks>
    public static ConstantDefinition<Role> ContactPersonInAdministrativeUnit { get; } = new ConstantDefinition<Role>("7f6c14f6-7809-4867-83ab-30c426b53d57")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Kontaktperson i Ad",
            Code = "kontaktperson-ados",
            Description = "enhet - offentlig sektor",
            Urn = "urn:altinn:external-role:ccr:kontaktperson-ados",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Leder i partiets utovende organ' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:parti-organ-leder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> parti-organ-leder
    /// - <c>Description:</c> Leder i partiets utovende organ
    /// </remarks>
    public static ConstantDefinition<Role> PartyExecutiveOrganLeader { get; } = new ConstantDefinition<Role>("E9E25AEC-66AB-4C02-8737-21B79A5D9EB5")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Leder i partiets utovende organ",
            Code = "parti-organ-leder",
            Description = "Leder i partiets utovende organ",
            Urn = "urn:altinn:external-role:ccr:parti-organ-leder",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Elektronisk signeringsrett' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0
    /// - <c>URN:</c> urn:altinn:external-role:ccr:elektronisk-signeringsrettig
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> elektronisk-signeringsrettig
    /// - <c>Description:</c> Elektronisk signeringsrett
    /// </remarks>
    public static ConstantDefinition<Role> ElektroniskSigneringsrett { get; } = new ConstantDefinition<Role>("0BE0982C-6650-49F2-9A1E-364AD879472C")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Elektronisk signeringsrett",                  /*"ESGR"*/
            Code = "elektronisk-signeringsrettig",
            Description = "Elektronisk signeringsrett",
            Urn = "urn:altinn:external-role:ccr:elektronisk-signeringsrettig",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Tildeler av elektronisk signeringsrett' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:elektronisk-signeringsrett-tildeler
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> elektronisk-signeringsrett-tildeler
    /// - <c>Description:</c> Tildeler av elektronisk signeringsrett
    /// </remarks>
    public static ConstantDefinition<Role> ElectronicSigningRightGranter { get; } = new ConstantDefinition<Role>("EE453078-9A2A-4997-969E-40F6663379AB")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Tildeler av elektronisk signeringsrett",      /*"ETDL"*/
            Code = "elektronisk-signeringsrett-tildeler",
            Description = "Tildeler av elektronisk signeringsrett",
            Urn = "urn:altinn:external-role:ccr:elektronisk-signeringsrett-tildeler",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Inngår i foretaksgruppe med' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 156
    /// - <c>URN:</c> urn:altinn:external-role:ccr:foretaksgruppe-med
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> foretaksgruppe-med
    /// - <c>Description:</c> Inngår i foretaksgruppe med
    /// </remarks>
    public static ConstantDefinition<Role> CompanyGroupMember { get; } = new ConstantDefinition<Role>("156AE2E3-D9E8-4DAA-BB3C-5859A31BE8C9")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Inngår i foretaksgruppe med",                 /*"FGRP"*/
            Code = "foretaksgruppe-med",
            Description = "Inngår i foretaksgruppe med",
            Urn = "urn:altinn:external-role:ccr:foretaksgruppe-med",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Har som datter i konsern' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:konsern-datter
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> konsern-datter
    /// - <c>Description:</c> Har som datter i konsern
    /// </remarks>
    public static ConstantDefinition<Role> CorporateSubsidiary { get; } = new ConstantDefinition<Role>("A14D5CDD-A8C9-4E7B-AC90-5A008C0C6129")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Har som datter i konsern",                    /*"KDAT"*/
            Code = "konsern-datter",
            Description = "Har som datter i konsern",
            Urn = "urn:altinn:external-role:ccr:konsern-datter",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Har som grunnlag for konsern' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:konsern-grunnlag
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> konsern-grunnlag
    /// - <c>Description:</c> Har som grunnlag for konsern
    /// </remarks>
    public static ConstantDefinition<Role> CorporateBasis { get; } = new ConstantDefinition<Role>("ACD90AC5-4A9D-4AB1-A5D9-5D33D1684A45")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Har som grunnlag for konsern",                /*"KGRL"*/
            Code = "konsern-grunnlag",
            Description = "Har som grunnlag for konsern",
            Urn = "urn:altinn:external-role:ccr:konsern-grunnlag",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Har som mor i konsern' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:konsern-mor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> konsern-mor
    /// - <c>Description:</c> Har som mor i konsern
    /// </remarks>
    public static ConstantDefinition<Role> CorporateParent { get; } = new ConstantDefinition<Role>("BFA050A6-25BB-4AF8-8DE3-651D0C6FDDC2")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Har som mor i konsern",                       /*"KMOR"*/
            Code = "konsern-mor",
            Description = "Har som mor i konsern",
            Urn = "urn:altinn:external-role:ccr:konsern-mor",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Forestår avvikling' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:forestaar-avvikling
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> forestaar-avvikling
    /// - <c>Description:</c> Forestår avvikling
    /// </remarks>
    public static ConstantDefinition<Role> PerformsLiquidation { get; } = new ConstantDefinition<Role>("E4A1253C-31C0-4E11-85BA-6E2E63627FB5")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Forestår avvikling",                          /*"AVKL"*/
            Code = "forestaar-avvikling",
            Description = "Forestår avvikling",
            Urn = "urn:altinn:external-role:ccr:forestaar-avvikling",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Inngår i felles- registrering' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 177
    /// - <c>URN:</c> urn:altinn:external-role:ccr:felles-registrert-med
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> felles-registrert-med
    /// - <c>Description:</c> Inngår i felles- registrering
    /// </remarks>
    public static ConstantDefinition<Role> JointRegistrationMember { get; } = new ConstantDefinition<Role>("177B7290-DAEA-4368-9A7A-71DBE1EB3B1B")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Inngår i felles- registrering",               /*"FEMV"*/
            Code = "felles-registrert-med",
            Description = "Inngår i felles- registrering",
            Urn = "urn:altinn:external-role:ccr:felles-registrert-med",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Er frivillig registrert utleiebygg for' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 920
    /// - <c>URN:</c> urn:altinn:external-role:ccr:utleiebygg
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> utleiebygg
    /// - <c>Description:</c> Er frivillig registrert utleiebygg for
    /// </remarks>
    public static ConstantDefinition<Role> VoluntaryRegisteredRentalBuilding { get; } = new ConstantDefinition<Role>("920F602D-B82B-40EE-BFD2-856A1C6A26F2")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Er frivillig registrert utleiebygg for",      /*"UTBG"*/
            Code = "utleiebygg",
            Description = "Er frivillig registrert utleiebygg for",
            Urn = "urn:altinn:external-role:ccr:utleiebygg",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Er virksomhet drevet i fellesskap av' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 3
    /// - <c>URN:</c> urn:altinn:external-role:ccr:virksomhet-fellesskap-drifter
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> virksomhet-fellesskap-drifter
    /// - <c>Description:</c> Er virksomhet drevet i fellesskap av
    /// </remarks>
    public static ConstantDefinition<Role> JointlyOperatedBusiness { get; } = new ConstantDefinition<Role>("3A9E145D-3CE6-4DF4-85D4-8901AFFAF347")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Er virksomhet drevet i fellesskap av",        /*"VIFE"*/
            Code = "virksomhet-fellesskap-drifter",
            Description = "Er virksomhet drevet i fellesskap av",
            Urn = "urn:altinn:external-role:ccr:virksomhet-fellesskap-drifter",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Utfyller MVA-oppgaver' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 92651683-36
    /// - <c>URN:</c> urn:altinn:external-role:ccr:mva-utfyller
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> mva-utfyller
    /// - <c>Description:</c> Utfyller MVA-oppgaver
    /// </remarks>
    public static ConstantDefinition<Role> VatFormCompleter { get; } = new ConstantDefinition<Role>("92651683-36B2-4604-9CE9-B5B688F68696")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Utfyller MVA-oppgaver",                       /*"MVAU"*/
            Code = "mva-utfyller",
            Description = "Utfyller MVA-oppgaver",
            Urn = "urn:altinn:external-role:ccr:mva-utfyller",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Signerer MVA-oppgaver' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:mva-signerer
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> mva-signerer
    /// - <c>Description:</c> Signerer MVA-oppgaver
    /// </remarks>
    public static ConstantDefinition<Role> VatFormSigner { get; } = new ConstantDefinition<Role>("B5136A2C-F48C-40A7-8276-B74E121AB4EB")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Signerer MVA-oppgaver",                       /*"MVAG"*/
            Code = "mva-signerer",
            Description = "Signerer MVA-oppgaver",
            Urn = "urn:altinn:external-role:ccr:mva-signerer",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Revisor registrert i revisorregisteret' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 4
    /// - <c>URN:</c> urn:altinn:external-role:ccr:kontaktperson-revisor
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> kontaktperson-revisor
    /// - <c>Description:</c> Rettigheter for revisjonsselskap
    /// </remarks>
    public static ConstantDefinition<Role> RegisteredAuditor { get; } = new ConstantDefinition<Role>("4B3AE668-5CAE-4416-9121-C20E81597B12")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Revisor registrert i revisorregisteret",      /*"SREVA"*/
            Code = "kontaktperson-revisor",
            Description = "Rettigheter for revisjonsselskap",
            Urn = "urn:altinn:external-role:ccr:kontaktperson-revisor",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Stifter' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:stifter
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> stifter
    /// - <c>Description:</c> Stifter
    /// </remarks>
    public static ConstantDefinition<Role> Founder { get; } = new ConstantDefinition<Role>("CDD312F9-8A6E-4184-9374-D4AE4BAABE3E")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Stifter",                                     /*"STFT"*/
            Code = "stifter",
            Description = "Stifter",
            Urn = "urn:altinn:external-role:ccr:stifter",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Varamedlem i partiets utovende organ' role.
    /// </summary>
    /// <remarks>
    /// - <c>URN:</c> urn:altinn:external-role:ccr:parti-organ-varamedlem
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> parti-organ-varamedlem
    /// - <c>Description:</c> Varamedlem i partiets utovende organ
    /// </remarks>
    public static ConstantDefinition<Role> PartyExecutiveOrganAlternateMember { get; } = new ConstantDefinition<Role>("F23B832A-CE0E-42F0-B314-E1B0751506F2")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Varamedlem i partiets utovende organ",        /*"HVAR"*/
            Code = "parti-organ-varamedlem",
            Description = "Varamedlem i partiets utovende organ",
            Urn = "urn:altinn:external-role:ccr:parti-organ-varamedlem",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Nestleder i partiets utovende organ' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 355
    /// - <c>URN:</c> urn:altinn:external-role:ccr:parti-organ-nestleder
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> parti-organ-nestleder
    /// - <c>Description:</c> Nestleder i partiets utovende organ
    /// </remarks>
    public static ConstantDefinition<Role> PartyExecutiveOrganDeputyLeader { get; } = new ConstantDefinition<Role>("355BC5D6-C346-4B6B-BDB4-ED2CBDEE8318")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Nestleder i partiets utovende organ",         /*"HNST"*/
            Code = "parti-organ-nestleder",
            Description = "Nestleder i partiets utovende organ",
            Urn = "urn:altinn:external-role:ccr:parti-organ-nestleder",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Styremedlem i partiets utovende organ' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 4
    /// - <c>URN:</c> urn:altinn:external-role:ccr:parti-organ-styremedlem
    /// - <c>Provider:</c> CentralCoordinatingRegister
    /// - <c>Code:</c> parti-organ-styremedlem
    /// - <c>Description:</c> Styremedlem i partiets utovende organ
    /// </remarks>
    public static ConstantDefinition<Role> PartyExecutiveOrganBoardMember { get; } = new ConstantDefinition<Role>("4A596F51-199E-4586-8292-F9F84B079769")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.CentralCoordinatingRegister,
            Name = "Styremedlem i partiets utovende organ",       /*"HMDL"*/
            Code = "parti-organ-styremedlem",
            Description = "Styremedlem i partiets utovende organ",
            Urn = "urn:altinn:external-role:ccr:parti-organ-styremedlem",
            IsKeyRole = false,
            IsAssignable = false
        },
    };

    /// <summary>
    /// Represents the 'Skatteforhold for privatpersoner' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> f4df0522-3034-405b-a9e5-83f971737033
    /// - <c>URN:</c> urn:altinn:rolecode:A0282
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0282
    /// - <c>Description:</c> Tillatelsen gjelder alle opplysninger vedrørende dine eller ditt enkeltpersonsforetaks skatteforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan Skatteetaten endre i tillatelsen.
    /// </remarks>
    public static ConstantDefinition<Role> A0282 { get; } = new ConstantDefinition<Role>("f4df0522-3034-405b-a9e5-83f971737033")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Skatteforhold for privatpersoner",
            Code = "A0282",
            Description = "Tillatelsen gjelder alle opplysninger vedrørende dine eller ditt enkeltpersonsforetaks skatteforhold. Ved regelverksendringer eller innføring av nye digitale tjenester kan Skatteetaten endre i tillatelsen.",
            Urn = "urn:altinn:rolecode:A0282",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Private tax affairs"),
            KeyValuePair.Create("Description", "The permission applies to all information about your own or your sole proprietorship’s tax affairs. In case of changes to regulations or implementation of new digital services, the Tax Administration may change the permission.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Skatteforhold for privatpersonar"),
            KeyValuePair.Create("Description", "Løyvet gjeld alle opplysningar om skatteforholda dine og om skatteforholda for enkeltpersonføretaket ditt. Ved regelverksendringar eller innføring av nye digitale tenester kan Skatteetaten endre løyvet.")
        )
    };

    /// <summary>
    /// Represents the 'Taushetsbelagt post' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 92ea5544-ca64-4e03-9532-646b9f86ff65
    /// - <c>URN:</c> urn:altinn:rolecode:A0286
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0286
    /// - <c>Description:</c> Denne rollen gir tilgang til taushetsbelagt post fra stat og kommune. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> A0286 { get; } = new ConstantDefinition<Role>("92ea5544-ca64-4e03-9532-646b9f86ff65")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Taushetsbelagt post ",
            Code = "A0286",
            Description = "Denne rollen gir tilgang til taushetsbelagt post fra stat og kommune. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0286",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Confidential information"),
            KeyValuePair.Create("Description", "This role provides access to confidential information from public agencies. In the event of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Taushetslagd post"),
            KeyValuePair.Create("Description", "Gir tlgang til taushetslagd post frå det offentlige. Ved regelverksendringer eller innføring av nye digitale tenester kan det bli endringer i tilganger som rollen gir")
        )
    };

    public static ConstantDefinition<Role> A0287 { get; } = new ConstantDefinition<Role>("df34b69a-e0aa-4245-a840-3a850769b2bd")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Taushetsbelagt post - oppvekst og utdanning",
            Code = "A0287",
            Description = "Gir tilgang til taushetsbelagt post fra det offentlige innen oppvekst og utdanning",
            Urn = "urn:altinn:rolecode:A0287",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Confidential - education"),
            KeyValuePair.Create("Description", "This role provides access to confidential information from public agencies")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Taushetslagd post - oppvekst og utdanning"),
            KeyValuePair.Create("Description", "Gir tlgang til taushetslagd post frå det offentlige innan oppvekst og utdanning")
        )
    };

    /// <summary>
    /// Represents the 'Taushetsbelagt post - administrasjon' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 5fda4732-dd10-416d-b876-9e1715bbf21c
    /// - <c>URN:</c> urn:altinn:rolecode:A0288
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0288
    /// - <c>Description:</c> Gir tilgang til taushetsbelagt post fra det offentlige innen administrasjon
    /// </remarks>
    public static ConstantDefinition<Role> A0288 { get; } = new ConstantDefinition<Role>("5fda4732-dd10-416d-b876-9e1715bbf21c")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Taushetsbelagt post - administrasjon",
            Code = "A0288",
            Description = "Gir tilgang til taushetsbelagt post fra det offentlige innen administrasjon",
            Urn = "urn:altinn:rolecode:A0288",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Conficential - administration"),
            KeyValuePair.Create("Description", "This role provides access to confidential information from public agencies")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Taushetslagd post - administrasjon"),
            KeyValuePair.Create("Description", "Gir tlgang til taushetslagd post frå det offentlige innan administrasjon")
        )
    };

    /// <summary>
    /// Represents the 'Algetestdata' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 4652e98f-7a6b-4dc2-b061-fc8d6840e456
    /// - <c>URN:</c> urn:altinn:rolecode:A0293
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0293
    /// - <c>Description:</c> Havforskningsinstituttet - registrering av algetestdata
    /// </remarks>
    public static ConstantDefinition<Role> A0293 { get; } = new ConstantDefinition<Role>("4652e98f-7a6b-4dc2-b061-fc8d6840e456")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Algetestdata",
            Code = "A0293",
            Description = "Havforskningsinstituttet - registrering av algetestdata",
            Urn = "urn:altinn:rolecode:A0293",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Algea test data"),
            KeyValuePair.Create("Description", "Havforskningsinstituttet - registration of algea test data")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Algetestdata"),
            KeyValuePair.Create("Description", "Havforskningsinstituttet - registrering av algetestdata")
        )
    };

    /// <summary>
    /// Represents the 'Transportløyvegaranti' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> c22c6add-dd5d-4735-87de-b75491018e50
    /// - <c>URN:</c> urn:altinn:rolecode:A0294
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0294
    /// - <c>Description:</c> Statens vegvesen - rolle som gir tilgang til app for transportløyvegarantister
    /// </remarks>
    public static ConstantDefinition<Role> A0294 { get; } = new ConstantDefinition<Role>("c22c6add-dd5d-4735-87de-b75491018e50")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Transportløyvegaranti",
            Code = "A0294",
            Description = "Statens vegvesen - rolle som gir tilgang til app for transportløyvegarantister",
            Urn = "urn:altinn:rolecode:A0294",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Transport permit guarantee"),
            KeyValuePair.Create("Description", "The Norwegian Public Roads Administration - role that provides access to the app for transport permi")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Transportløyvegaranti"),
            KeyValuePair.Create("Description", "Statens vegvesen - rolle som gjer tilgang til app for transportløuvegarantistar")
        )
    };

    /// <summary>
    /// Represents the 'Revisorattesterer' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d8b9c47b-e5a7-4912-8aa8-1d2bab75e41c
    /// - <c>URN:</c> urn:altinn:rolecode:A0298
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> A0298
    /// - <c>Description:</c> Rollen gir bruker tilgang til å attestere tjenester for avgiver som revisor. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> A0298 { get; } = new ConstantDefinition<Role>("d8b9c47b-e5a7-4912-8aa8-1d2bab75e41c")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Revisorattesterer",
            Code = "A0298",
            Description = "Rollen gir bruker tilgang til å attestere tjenester for avgiver som revisor. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:A0298",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Auditor certifier"),
            KeyValuePair.Create("Description", "The role gives the user access to certify services for the reportee as an auditor. In the event of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides..")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Revisorattesterar"),
            KeyValuePair.Create("Description", "Rollen gir bruker tilgang til å attestere tjenester for avgiver som revisor. Ved regelverksendringer eller innføring av nye digitale tenester kan det bli endringer i tilganger som rollen gir")
        )
    };

    /// <summary>
    /// Represents the 'Programmeringsgrensesnitt for NUF (API)' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0ea4e5de-3fb4-499e-b013-1e1b4459af24
    /// - <c>URN:</c> urn:altinn:rolecode:APIADMNUF
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> APIADMNUF
    /// - <c>Description:</c> Delegerbar rolle som gir kontaktperson for norskregistrert utenlandsk foretak (NUF) tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.
    /// </remarks>
    public static ConstantDefinition<Role> Apidfmnuf { get; } = new ConstantDefinition<Role>("0ea4e5de-3fb4-499e-b013-1e1b4459af24")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Programmeringsgrensesnitt for NUF (API)",
            Code = "APIADMNUF",
            Description = "Delegerbar rolle som gir kontaktperson for norskregistrert utenlandsk foretak (NUF) tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av virksomheten.",
            Urn = "urn:altinn:rolecode:APIADMNUF",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Application Programming Interface for NUF (API)"),
            KeyValuePair.Create("Description", "Delegable role that provides the representative for a Norwegian-registered foreign enterprise (NUF) access to manage access to the programming interface - API, on behalf of the business.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Programmeringsgrensesnitt for NUF (API)"),
            KeyValuePair.Create("Description", "Delegerbar rolle som gir kontaktperson for norskregistrert utanlandsk føretak (NUF) tilgang til å administrere tilgang til programmeringsgrensesnitt - API, på vegne av verksemden")
        )
    };

    /// <summary>
    /// Represents the 'Revisorattesterer - MVA kompensasjon' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 60abf944-cf8c-4845-b310-83bcb6c77198
    /// - <c>URN:</c> urn:altinn:rolecode:ATTST
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> ATTST
    /// - <c>Description:</c> Denne rollen gir revisor rettighet til å attestere tjenesten Merverdiavgift - søknad om kompensasjon (RF-0009).
    /// </remarks>
    public static ConstantDefinition<Role> Attst { get; } = new ConstantDefinition<Role>("60abf944-cf8c-4845-b310-83bcb6c77198")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Revisorattesterer - MVA kompensasjon",
            Code = "ATTST",
            Description = "Denne rollen gir revisor rettighet til å attestere tjenesten Merverdiavgift - søknad om kompensasjon (RF-0009).",
            Urn = "urn:altinn:rolecode:ATTST",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Auditor certifies validity of VAT compensation"),
            KeyValuePair.Create("Description", "Certification by auditor of RF-0009")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Revisorattesterar - MVA kompensasjon"),
            KeyValuePair.Create("Description", "Revisor si attestering av RF-0009")
        )
    };

    /// <summary>
    /// Represents the 'Konkursbo tilgangsstyring' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0a76304e-345b-4f22-bb31-4837a630eb7a
    /// - <c>URN:</c> urn:altinn:rolecode:BOADM
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> BOADM
    /// - <c>Description:</c> Denne rollen gir advokater mulighet til å styre hvem som har rettigheter til konkursbo.
    /// </remarks>
    public static ConstantDefinition<Role> Boadm { get; } = new ConstantDefinition<Role>("0a76304e-345b-4f22-bb31-4837a630eb7a")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Konkursbo tilgangsstyring",
            Code = "BOADM",
            Description = "Denne rollen gir advokater mulighet til å styre hvem som har rettigheter til konkursbo.",
            Urn = "urn:altinn:rolecode:BOADM",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bankruptcy administrator"),
            KeyValuePair.Create("Description", "Applies to lawyers and gives opportunity to manage access to bankruptcies")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Konkursbu tilgangsstyring"),
            KeyValuePair.Create("Description", "Gjeld advokatar og gjev moglegheit for tilgangsstyring av konkursbu")
        )
    };

    public static ConstantDefinition<Role> Bobel { get; } = new ConstantDefinition<Role>("7246639c-137b-4981-b172-6134c9fc1a7f")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Konkursbo lesetilgang",
            Code = "BOBEL",
            Description = "Tilgang til å lese informasjon i tjenesten Konkursbehandling",
            Urn = "urn:altinn:rolecode:BOBEL",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bankruptcy read"),
            KeyValuePair.Create("Description", "Reading rights for information in the service Konkursbehandling (bankruptcy proceedings)")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Konkursbu lesetilgang"),
            KeyValuePair.Create("Description", "Tilgang til å lese informasjon i tenesta Konkursbehandling")
        )
    };

    /// <summary>
    /// Represents the 'Konkursbo skrivetilgang' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 5f73b031-8b5b-45d8-a682-e9a7e75a7691
    /// - <c>URN:</c> urn:altinn:rolecode:BOBES
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> BOBES
    /// - <c>Description:</c> Utvidet lesetilgang og innsendingsrett for tjenesten Konkursbehandling
    /// </remarks>
    public static ConstantDefinition<Role> Bobes { get; } = new ConstantDefinition<Role>("5f73b031-8b5b-45d8-a682-e9a7e75a7691")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Konkursbo skrivetilgang",
            Code = "BOBES",
            Description = "Utvidet lesetilgang og innsendingsrett for tjenesten Konkursbehandling",
            Urn = "urn:altinn:rolecode:BOBES",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Bankruptcy write"),
            KeyValuePair.Create("Description", "Writing rights for information in the service Konkursbehandling (bankruptcy proceedings)")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Konkursbu skrivetilgang"),
            KeyValuePair.Create("Description", "Tilgang til å skrive informasjon i tenesta Konkursbehandling")
        )
    };

    /// <summary>
    /// Represents the 'ECKEYROLE' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e0684f66-a46e-4706-a754-8889b532509c
    /// - <c>URN:</c> urn:altinn:rolecode:ECKEYROLE
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> ECKEYROLE
    /// - <c>Description:</c> Nøkkelrolle for virksomhetsertifikatbrukere
    /// </remarks>
    public static ConstantDefinition<Role> Eckeyrole { get; } = new ConstantDefinition<Role>("e0684f66-a46e-4706-a754-8889b532509c")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "ECKEYROLE",
            Code = "ECKEYROLE",
            Description = "Nøkkelrolle for virksomhetsertifikatbrukere",
            Urn = "urn:altinn:rolecode:ECKEYROLE",
            IsKeyRole = true
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "ECKEYROLE"),
            KeyValuePair.Create("Description", "Key role for enterprise users")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "ECKEYROLE"),
            KeyValuePair.Create("Description", "Nøkkelrolle for virksomhetsertifikatbrukere")
        )
    };

    /// <summary>
    /// Represents the 'Eksplisitt tjenestedelegering' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1225bc46-4b03-4b63-b6e8-58926b29a97b
    /// - <c>URN:</c> urn:altinn:rolecode:EKTJ
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> EKTJ
    /// - <c>Description:</c> Ikke-delegerbar roller for tjenester som kun skal delegeres enkeltvis
    /// </remarks>
    public static ConstantDefinition<Role> Ektj { get; } = new ConstantDefinition<Role>("1225bc46-4b03-4b63-b6e8-58926b29a97b")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Eksplisitt tjenestedelegering",
            Code = "EKTJ",
            Description = "Ikke-delegerbar roller for tjenester som kun skal delegeres enkeltvis",
            Urn = "urn:altinn:rolecode:EKTJ",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Explicit service delegation"),
            KeyValuePair.Create("Description", "Non-delegable role for services to be delegated as single rights")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Eksplisitt tenestedelegering"),
            KeyValuePair.Create("Description", "Ikkje-delegerbar rolle for tenester som kun skal delegerast enkeltvis")
        )
    };

    /// <summary>
    /// Represents the 'Godkjenning av bedriftshelsetjeneste' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> cde501eb-0d23-410b-b728-00ab9d68fb2e
    /// - <c>URN:</c> urn:altinn:rolecode:GKBHT
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> GKBHT
    /// - <c>Description:</c> Godkjenning av bedriftshelsetjeneste
    /// </remarks>
    public static ConstantDefinition<Role> Gkbht { get; } = new ConstantDefinition<Role>("cde501eb-0d23-410b-b728-00ab9d68fb2e")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Godkjenning av bedriftshelsetjeneste",
            Code = "GKBHT",
            Description = "Godkjenning av bedriftshelsetjeneste",
            Urn = "urn:altinn:rolecode:GKBHT",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Godkjenning av bedriftshelsetjeneste"),
            KeyValuePair.Create("Description", "Godkjenning av bedriftshelsetjeneste")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Godkjenning av bedriftshelsetjeneste"),
            KeyValuePair.Create("Description", "Godkjenning av bedriftshelsetjeneste")
        )
    };

    /// <summary>
    /// Represents the 'Hovedadministrator' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> d9e05d40-9849-4982-bf04-aa03b19e4a66
    /// - <c>URN:</c> urn:altinn:rolecode:HADM
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> HADM
    /// - <c>Description:</c> Denne rollen gir mulighet for å delegere alle roller og rettigheter for en aktør, også de man ikke har selv. Hovedadministrator-rollen kan bare delegeres av daglig leder, styrets leder, innehaver og bestyrende reder.
    /// </remarks>
    public static ConstantDefinition<Role> Hadm { get; } = new ConstantDefinition<Role>("d9e05d40-9849-4982-bf04-aa03b19e4a66")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Hovedadministrator",
            Code = "HADM",
            Description = "Denne rollen gir mulighet for å delegere alle roller og rettigheter for en aktør, også de man ikke har selv. Hovedadministrator-rollen kan bare delegeres av daglig leder, styrets leder, innehaver og bestyrende reder.",
            Urn = "urn:altinn:rolecode:HADM",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Main Administrator"),
            KeyValuePair.Create("Description", "This role allows you to delegate all roles and rights for an actor, including those you do not have yourself. The Main administrator role can only be delegated by General manager, Chairman of the board, Soul proprietor and Managing shipowner.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Hovudadministrator"),
            KeyValuePair.Create("Description", "Denne rolla gir høve til å delegere alle roller og rettar for ein aktør, også dei ein ikkje har sjøl")
        )
    };

    /// <summary>
    /// Represents the 'Økokrim rapportering' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 98bebcac-d6bb-4343-97b8-0fe8bc744d7a
    /// - <c>URN:</c> urn:altinn:rolecode:HVASK
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> HVASK
    /// - <c>Description:</c> Tilgang til tjenester fra Økokrim. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Hvask { get; } = new ConstantDefinition<Role>("98bebcac-d6bb-4343-97b8-0fe8bc744d7a")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Økokrim rapportering",
            Code = "HVASK",
            Description = "Tilgang til tjenester fra Økokrim. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:HVASK",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Økokrim reporting"),
            KeyValuePair.Create("Description", "Access to services from The Norwegian National Authority for Investigation and Prosecution of Economic and Environmental Crime. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provide")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Økokrim rapportering"),
            KeyValuePair.Create("Description", "Tilgang til tenester frå Økokrim. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Klientadministrator' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 27e1ef41-df4d-439e-b948-df136c139e81
    /// - <c>URN:</c> urn:altinn:rolecode:KLADM
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> KLADM
    /// - <c>Description:</c> Tilgang til å administrere klientroller for regnskapsførere og revisorer
    /// </remarks>
    public static ConstantDefinition<Role> Kladm { get; } = new ConstantDefinition<Role>("27e1ef41-df4d-439e-b948-df136c139e81")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Klientadministrator",
            Code = "KLADM",
            Description = "Tilgang til å administrere klientroller for regnskapsførere og revisorer",
            Urn = "urn:altinn:rolecode:KLADM",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Client administrator"),
            KeyValuePair.Create("Description", "Administration of access to client roles for accountants and auditors")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Klientadministrator"),
            KeyValuePair.Create("Description", "Tilgang til å administrere klientroller for rekneskapsførarar og revisorar")
        )
    };

    /// <summary>
    /// Represents the 'Kommunale tjenester' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> b8e6dd1c-ca10-4ce6-9c27-53cdb3c275b3
    /// - <c>URN:</c> urn:altinn:rolecode:KOMAB
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> KOMAB
    /// - <c>Description:</c> Rollen gir tilgang til kommunale tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Komab { get; } = new ConstantDefinition<Role>("b8e6dd1c-ca10-4ce6-9c27-53cdb3c275b3")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Kommunale tjenester",
            Code = "KOMAB",
            Description = "Rollen gir tilgang til kommunale tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:KOMAB",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Municipal services"),
            KeyValuePair.Create("Description", "Role for municipal services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Kommunale tenester"),
            KeyValuePair.Create("Description", "Rolle for kommunale tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Lønn og personalmedarbeider' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 010b4c49-bf56-44e3-b73b-84be7b2a5eb6
    /// - <c>URN:</c> urn:altinn:rolecode:LOPER
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> LOPER
    /// - <c>Description:</c> Denne rollen gir rettighet til lønns- og personalrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Loper { get; } = new ConstantDefinition<Role>("010b4c49-bf56-44e3-b73b-84be7b2a5eb6")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Lønn og personalmedarbeider",
            Code = "LOPER",
            Description = "Denne rollen gir rettighet til lønns- og personalrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:LOPER",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Salaries and personnel employee"),
            KeyValuePair.Create("Description", "Access to services related to salaries and personnel")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Løn og personalmedarbeidar"),
            KeyValuePair.Create("Description", "Tilgang til løns- og personalrelaterte tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    public static ConstantDefinition<Role> Pasig { get; } = new ConstantDefinition<Role>("0f276fc4-c201-4ff7-8e8a-caa3efe9c02a")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Parallell signering",
            Code = "PASIG",
            Description = "Denne rollen gir rettighet til å signere elementer fra andre avgivere.",
            Urn = "urn:altinn:rolecode:PASIG",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Parallel signing"),
            KeyValuePair.Create("Description", "Right to sign elements from other reportees")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Parallell signering"),
            KeyValuePair.Create("Description", "Rett til å signere elementer frå andre avgjevarar")
        )
    };

    /// <summary>
    /// Represents the 'Patent, varemerke og design' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 23cade0a-287a-49e0-8957-22d5a14cb100
    /// - <c>URN:</c> urn:altinn:rolecode:PAVAD
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> PAVAD
    /// - <c>Description:</c> Denne rollen gir rettighet til tjenester relatert til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Pavad { get; } = new ConstantDefinition<Role>("23cade0a-287a-49e0-8957-22d5a14cb100")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Patent, varemerke og design",
            Code = "PAVAD",
            Description = "Denne rollen gir rettighet til tjenester relatert til patent, varemerke og design. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:PAVAD",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Patents, trademarks and design"),
            KeyValuePair.Create("Description", "Access to services related to patents, trademarks and design. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Patent, varemerke og design"),
            KeyValuePair.Create("Description", "Tilgang til tenester frå Patentstyret. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Privatperson begrensede rettigheter' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 696478f4-c85b-4bda-ace0-caa058fe5def
    /// - <c>URN:</c> urn:altinn:rolecode:PRIUT
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> PRIUT
    /// - <c>Description:</c> Denne rollen gir mulighet til å benytte tjenester på vegne av en annen privatperson. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Priut { get; } = new ConstantDefinition<Role>("696478f4-c85b-4bda-ace0-caa058fe5def")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Privatperson begrensede rettigheter",
            Code = "PRIUT",
            Description = "Denne rollen gir mulighet til å benytte tjenester på vegne av en annen privatperson. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:PRIUT",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Limited rights for an individual"),
            KeyValuePair.Create("Description", "Delegable rights to services for individuals. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Privatperson avgrensa retter"),
            KeyValuePair.Create("Description", "Delegerbare retter for tenester knytt til privatperson. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Regnskapsmedarbeider' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 633cde7d-3604-45b2-ba8c-e16161cf2cf8
    /// - <c>URN:</c> urn:altinn:rolecode:REGNA
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> REGNA
    /// - <c>Description:</c> Denne rollen gir rettighet til regnskapsrelaterte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Regna { get; } = new ConstantDefinition<Role>("633cde7d-3604-45b2-ba8c-e16161cf2cf8")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Regnskapsmedarbeider",
            Code = "REGNA",
            Description = "Denne rollen gir rettighet til regnskapsrelaterte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:REGNA",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accounting employee"),
            KeyValuePair.Create("Description", "Access to accounting related forms and services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rekneskapsmedarbeidar"),
            KeyValuePair.Create("Description", "Tilgang til rekneskapsrelaterte skjema og tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Revisorrettighet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1d71e23d-91b6-44ca-b171-c179028e7cdf
    /// - <c>URN:</c> urn:altinn:rolecode:REVAI
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> REVAI
    /// - <c>Description:</c> Denne rollen gir revisor rettighet til aktuelle skjema og tjenester
    /// </remarks>
    public static ConstantDefinition<Role> Revai { get; } = new ConstantDefinition<Role>("1d71e23d-91b6-44ca-b171-c179028e7cdf")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Revisorrettighet",
            Code = "REVAI",
            Description = "Denne rollen gir revisor rettighet til aktuelle skjema og tjenester",
            Urn = "urn:altinn:rolecode:REVAI",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Auditor's rights"),
            KeyValuePair.Create("Description", "Delegable auditor's rights")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Revisorrett"),
            KeyValuePair.Create("Description", "Delegerbare revisorrettar")
        )
    };

    /// <summary>
    /// Represents the 'Taushetsbelagt post fra kommunen' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 1a15b75c-2387-4278-ba3a-7eb1cffe1653
    /// - <c>URN:</c> urn:altinn:rolecode:SENS01
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> SENS01
    /// - <c>Description:</c> Rollen gir tilgang til tjenester med taushetsbelagt informasjon fra kommunen, og bør ikke delegeres i stort omfang
    /// </remarks>
    public static ConstantDefinition<Role> Sens01 { get; } = new ConstantDefinition<Role>("1a15b75c-2387-4278-ba3a-7eb1cffe1653")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Taushetsbelagt post fra kommunen",
            Code = "SENS01",
            Description = "Rollen gir tilgang til tjenester med taushetsbelagt informasjon fra kommunen, og bør ikke delegeres i stort omfang",
            Urn = "urn:altinn:rolecode:SENS01",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Confidential correspondence from the municipality"),
            KeyValuePair.Create("Description", "This role provides access to services with confidential information from the municipality")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Taushetslagd post frå kommunen"),
            KeyValuePair.Create("Description", "Rolla gir tilgang til tenester med taushetsalgd informasjon frå kommunen.")
        )
    };

    /// <summary>
    /// Represents the 'Signerer av Samordnet registermelding' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> e427a9fb-4b6b-44b3-b873-689d174283b8
    /// - <c>URN:</c> urn:altinn:rolecode:SIGNE
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> SIGNE
    /// - <c>Description:</c> Denne rollen gir rettighet til tjenester på vegne av enheter/foretak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Signe { get; } = new ConstantDefinition<Role>("e427a9fb-4b6b-44b3-b873-689d174283b8")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Signerer av Samordnet registermelding",
            Code = "SIGNE",
            Description = "Denne rollen gir rettighet til tjenester på vegne av enheter/foretak. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:SIGNE",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Signer of Coordinated register notification"),
            KeyValuePair.Create("Description", "Applies to singing on behalf of entities/businesses. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Signerar av Samordna registermelding"),
            KeyValuePair.Create("Description", "Gjeld for signering på vegne av einingar/føretak. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Begrenset signeringsrettighet' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 16857e39-441f-4dd4-8592-aed94e816c04
    /// - <c>URN:</c> urn:altinn:rolecode:SISKD
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> SISKD
    /// - <c>Description:</c> Tilgang til å signere utvalgte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Siskd { get; } = new ConstantDefinition<Role>("16857e39-441f-4dd4-8592-aed94e816c04")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Begrenset signeringsrettighet",
            Code = "SISKD",
            Description = "Tilgang til å signere utvalgte skjema og tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:SISKD",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Limited signing rights"),
            KeyValuePair.Create("Description", "Signing access for selected forms and services.In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Avgrensa signeringsrett"),
            KeyValuePair.Create("Description", "Tilgang til å signere utvalde skjema og tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Helse-, sosial- og velferdstjenester' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> b1213d79-03fa-4837-9193-e4b9fe24eccb
    /// - <c>URN:</c> urn:altinn:rolecode:UIHTL
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> UIHTL
    /// - <c>Description:</c> Tilgang til helse-, sosial- og velferdsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Uihtl { get; } = new ConstantDefinition<Role>("b1213d79-03fa-4837-9193-e4b9fe24eccb")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Helse-, sosial- og velferdstjenester",
            Code = "UIHTL",
            Description = "Tilgang til helse-, sosial- og velferdsrelaterte tjenester. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:UIHTL",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Health-, social- and welfare services"),
            KeyValuePair.Create("Description", "Access to health-, social- and welfare related services. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Helse-, sosial- og velferdstenester"),
            KeyValuePair.Create("Description", "Tilgang til helse-, sosial- og velferdsrelaterte tenester. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Samferdsel' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 3c99647d-10b5-447e-9f0b-7bef1c7880f7
    /// - <c>URN:</c> urn:altinn:rolecode:UILUF
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> UILUF
    /// - <c>Description:</c> Rollen gir rettighet til tjenester relatert til samferdsel. For eksempel tjenester fra Statens Vegvesen, Sjøfartsdirektoratet og Luftfartstilsynet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rolen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Uiluf { get; } = new ConstantDefinition<Role>("3c99647d-10b5-447e-9f0b-7bef1c7880f7")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Samferdsel",
            Code = "UILUF",
            Description = "Rollen gir rettighet til tjenester relatert til samferdsel. For eksempel tjenester fra Statens Vegvesen, Sjøfartsdirektoratet og Luftfartstilsynet. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rolen gir.",
            Urn = "urn:altinn:rolecode:UILUF",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Transport"),
            KeyValuePair.Create("Description", "Access to services related to transport. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Samferdsel"),
            KeyValuePair.Create("Description", "Tilgang til tenester relatert til samferdsel. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        )
    };

    /// <summary>
    /// Represents the 'Utfyller/Innsender' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> dbaae9f8-107a-4222-9afd-d9f95cd5319c
    /// - <c>URN:</c> urn:altinn:rolecode:UTINN
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> UTINN
    /// - <c>Description:</c> Denne rollen gir rettighet til et bredt utvalg skjema og tjenester som ikke har så strenge krav til autorisasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Utinn { get; } = new ConstantDefinition<Role>("dbaae9f8-107a-4222-9afd-d9f95cd5319c")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Utfyller/Innsender",
            Code = "UTINN",
            Description = "Denne rollen gir rettighet til et bredt utvalg skjema og tjenester som ikke har så strenge krav til autorisasjon. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:UTINN",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Reporter/sender"),
            KeyValuePair.Create("Description", "This role provides right to a wide selection of forms and services that do not have very strict requirements for authorization. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provide")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Utfyllar/innsendar"),
            KeyValuePair.Create("Description", "Denne rolla gir rett til eit breitt utval skjema og tenester som ikkje har så strenge krav til autorisasjon. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the 'Energi, miljø og klima' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> af338fd5-3f1d-4ab5-8326-9dfecad26f71
    /// - <c>URN:</c> urn:altinn:rolecode:UTOMR
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> UTOMR
    /// - <c>Description:</c> Tilgang til tjenester relatert til energi, miljø og klima. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.
    /// </remarks>
    public static ConstantDefinition<Role> Utomr { get; } = new ConstantDefinition<Role>("af338fd5-3f1d-4ab5-8326-9dfecad26f71")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Energi, miljø og klima",
            Code = "UTOMR",
            Description = "Tilgang til tjenester relatert til energi, miljø og klima. Ved regelverksendringer eller innføring av nye digitale tjenester kan det bli endringer i tilganger som rollen gir.",
            Urn = "urn:altinn:rolecode:UTOMR",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Energy, environment and climate"),
            KeyValuePair.Create("Description", "Access to services related to energy, environment and climate. In case of regulatory changes or the introduction of new digital services, there may be changes in access that the role provides.")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Energi, miljø og klima"),
            KeyValuePair.Create("Description", "Tilgang til tenester relatert til energi, miljø og klima. Ved regelverksendringar eller innføring av nye digitale tenester kan det bli endringar i tilgangar som rolla gir")
        ),
    };

    /// <summary>
    /// Represents the 'Hovedrolle for sensitive tjeneste' role.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 478f710a-4af1-412d-9c67-de976fd0b229
    /// - <c>URN:</c> urn:altinn:rolecode:SENS
    /// - <c>Provider:</c> Altinn2
    /// - <c>Code:</c> SENS
    /// - <c>Description:</c> Hovedrolle for sensitive tjeneste
    /// </remarks>
    public static ConstantDefinition<Role> Sens { get; } = new ConstantDefinition<Role>("478f710a-4af1-412d-9c67-de976fd0b229")
    {
        Entity = new()
        {
            EntityTypeId = EntityTypeConstants.Organisation,
            ProviderId = ProviderConstants.Altinn2,
            Name = "Hovedrolle for sensitive tjeneste",
            Code = "SENS",
            Description = "Hovedrolle for sensitive tjeneste",
            Urn = "urn:altinn:rolecode:SENS",
            IsKeyRole = false
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Hovedrolle for sensitive tjeneste"),
            KeyValuePair.Create("Description", "Hovedrolle for sensitive tjeneste")
        ),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Hovudrolle for sensitive tenester"),
            KeyValuePair.Create("Description", "Hovudrolle for sensitive teneste")
        ),
    };
}
