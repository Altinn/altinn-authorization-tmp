using Altinn.AccessMgmt.PersistenceEF.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Utils;

/// <inheritdoc />
public class TranslationService : ITranslationService
{
    private readonly DbContext _db;

    public TranslationService(DbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async ValueTask<T> TranslateAsync<T>(T source, string languageCode)
    {
        var type = typeof(T);
        var typeName = type.Name.ToLower();

        var idProp = type.GetProperty("Id");
        if (idProp == null)
        {
            return source;
        }

        var id = idProp.GetValue(source);
        if (id is not Guid entityId)
        {
            return source;
        }

        var transMap = await _db.Set<TranslationEntry>()
            .Where(t => t.Type == typeName &&
                        t.Id == entityId &&
                        t.LanguageCode == languageCode)
            .ToDictionaryAsync(t => t.FieldName, t => t.Value);

        foreach (var prop in type.GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            if (transMap.TryGetValue(prop.Name.ToLower(), out var val) && val is not null)
            {
                prop.SetValue(source, val);
            }
        }

        return source;
    }
}

/// <summary>
/// Translation service for EF model
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Translates the specified object to the target language asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to be translated. The type must support translation or serialization.</typeparam>
    /// <param name="source">The object to be translated. Cannot be <see langword="null"/>.</param>
    /// <param name="languageCode">The language code representing the target language for translation. Must be a valid ISO 639-1 code.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the asynchronous operation. The result contains the translated object
    /// of type <typeparamref name="T"/>.</returns>
    ValueTask<T> TranslateAsync<T>(T source, string languageCode);
}

/// <summary>
/// Translation entry
/// </summary>
public class TranslationEntry
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Field
    /// </summary>
    public string FieldName { get; set; } = default!;

    /// <summary>
    /// Language
    /// </summary>
    public string LanguageCode { get; set; } = default!;

    /// <summary>
    /// Translated value
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// Audit extension of Package
/// </summary>
public class TranslationEntryAudit : TranslationEntry, IAudit
{
    /// <inheritdoc />
    public DateTime ValidFrom { get; set; }

    /// <inheritdoc />
    public DateTime? ValidTo { get; set; }

    /// <inheritdoc />
    public Guid? ChangedBy { get; set; }

    /// <inheritdoc />
    public Guid? ChangedBySystem { get; set; }

    /// <inheritdoc />
    public string ChangeOperation { get; set; }

    /// <inheritdoc />
    public Guid? DeletedBy { get; set; }

    /// <inheritdoc />
    public Guid? DeletedBySystem { get; set; }

    /// <inheritdoc />
    public string DeleteOperation { get; set; }
}
