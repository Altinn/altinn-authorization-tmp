using System.Diagnostics.CodeAnalysis;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

public static class RequestStatusConstants
{
    /// <summary>
    /// Try to get <see cref="RequestStatus"/> by name.
    /// </summary>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out ConstantDefinition<RequestStatus>? result)
        => ConstantLookup.TryGetByName(typeof(RequestStatusConstants), name, out result);

    /// <summary>
    /// Try to get <see cref="RequestStatus"/> using Guid.
    /// </summary>
    public static bool TryGetById(Guid id, [NotNullWhen(true)] out ConstantDefinition<RequestStatus>? result)
        => ConstantLookup.TryGetById(typeof(RequestStatusConstants), id, out result);

    /// <summary>
    /// Get all constants as a read-only collection.
    /// </summary>
    public static IReadOnlyCollection<ConstantDefinition<RequestStatus>> AllEntities()
        => ConstantLookup.AllEntities<RequestStatus>(typeof(RequestStatusConstants));

    /// <summary>
    /// Get all translations as read-only collection.
    /// </summary>
    public static IReadOnlyCollection<TranslationEntry> AllTranslations()
        => ConstantLookup.AllTranslations<RequestStatus>(typeof(RequestStatusConstants));

    /// <summary>
    /// Represents the Accepted request status.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7c6d-aec6-5eafa8154ca1
    /// - <c>Name:</c> "Godkjent"
    /// - <c>Description:</c> "Forespørselen er godkjent"
    /// </remarks>
    public static ConstantDefinition<RequestStatus> Accepted { get; } = new ConstantDefinition<RequestStatus>("0195efb8-7c80-7c6d-aec6-5eafa8154ca1")
    {
        Entity = new()
        {
            Name = "Godkjent",
            Description = "Forespørselen er godkjent"
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Accepted"),
            KeyValuePair.Create("Description", "Request is accepted")),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Godkjent"),
            KeyValuePair.Create("Description", "Forespørselen er godkjent")),
    };

    /// <summary>
    /// Represents the Rejected request status.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-761b-b950-e709c703b6b1
    /// - <c>Name:</c> "Godkjent"
    /// - <c>Description:</c> "Forespørselen er avslått"
    /// </remarks>
    public static ConstantDefinition<RequestStatus> Rejected { get; } = new ConstantDefinition<RequestStatus>("0195efb8-7c80-761b-b950-e709c703b6b1")
    {
        Entity = new()
        {
            Name = "Avlsått",
            Description = "Forespørselen er avslått"
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Rejected"),
            KeyValuePair.Create("Description", "Request is rejected")),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Avlsått"),
            KeyValuePair.Create("Description", "Forespørselen er avslått")),
    };

    /// <summary>
    /// Represents the Open request status.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7239-8ee5-7156872b53d1
    /// - <c>Name:</c> "Åpen"
    /// - <c>Description:</c> "Forespørselen er åpen"
    /// </remarks>
    public static ConstantDefinition<RequestStatus> Open { get; } = new ConstantDefinition<RequestStatus>("0195efb8-7c80-7239-8ee5-7156872b53d1")
    {
        Entity = new()
        {
            Name = "Åpen",
            Description = "Forespørselen er åpen"
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Open"),
            KeyValuePair.Create("Description", "Request is open")),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Åpen"),
            KeyValuePair.Create("Description", "Forespørselen er åpen")),
    };

    /// <summary>
    /// Represents the Closed request status.
    /// </summary>
    /// <remarks>
    /// - <c>Id:</c> 0195efb8-7c80-7731-82a3-1f6b659ec848
    /// - <c>Name:</c> "Lukket"
    /// - <c>Description:</c> "Forespørselen er lukket"
    /// </remarks>
    public static ConstantDefinition<RequestStatus> Closed { get; } = new ConstantDefinition<RequestStatus>("0195efb8-7c80-7731-82a3-1f6b659ec848")
    {
        Entity = new()
        {
            Name = "Lukket",
            Description = "Forespørselen er lukket"
        },
        EN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Closed"),
            KeyValuePair.Create("Description", "Request is closed")),
        NN = TranslationEntryList.Create(
            KeyValuePair.Create("Name", "Lukket"),
            KeyValuePair.Create("Description", "Forespørselen er lukket")),
    };
}
