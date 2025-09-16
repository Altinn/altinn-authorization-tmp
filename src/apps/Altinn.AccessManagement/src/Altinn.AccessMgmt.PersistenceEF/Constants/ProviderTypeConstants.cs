using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

/// <summary>
/// Defines constant <see cref="ProviderType"/> instances used in the system.
/// Each constant represents a type of provider (e.g., system provider or service owner)
/// with a fixed unique identifier (GUID), name, and multilingual translations.
/// </summary>
public static class ProviderTypeConstants
{
    /// <summary>
    /// Represents a system provider type.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> Unique identifier for system provider type.
    /// - <c>Name:</c> "System"
    /// - <c>Translations:</c>
    ///   - EN: "System"
    ///   - NN: "System"
    /// </remarks>
    public static ConstantDefinition<ProviderType> System { get; } = new ConstantDefinition<ProviderType>("0195efb8-7c80-7bb5-a35c-11d58ea36695")
    {
        Entity = new() { Name = "System" },
        EN = TranslationEntryList.Create(KeyValuePair.Create("Name", "System")),
        NN = TranslationEntryList.Create(KeyValuePair.Create("Name", "System")),
    };

    /// <summary>
    /// Represents a service owner provider type ("Tjenesteeier").
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> Unique identifier for service owner provider type.
    /// - <c>Name:</c> "Tjenesteeier"
    /// - <c>Translations:</c>
    ///   - EN: "ServiceOwner"
    ///   - NN: "Tenesteeigar"
    /// </remarks>
    public static ConstantDefinition<ProviderType> ServiceOwner { get; } = new ConstantDefinition<ProviderType>("0195efb8-7c80-713e-ad96-a9896d12f444")
    {
        Entity = new() { Name = "Tjenesteeier" },
        EN = TranslationEntryList.Create(KeyValuePair.Create("Name", "ServiceOwner")),
        NN = TranslationEntryList.Create(KeyValuePair.Create("Name", "Tenesteeigar")),
    };
}
