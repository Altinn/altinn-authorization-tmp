using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Altinn.AccessMgmt.PersistenceEF.Models.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

public abstract class ConstantBase<TType, TClass>
    where TType : IEntityId, IEntityName
    where TClass : ConstantBase<TType, TClass>
{
    private static readonly Dictionary<string, ConstantDefinition<TType>> _byName;
    private static readonly Dictionary<Guid, ConstantDefinition<TType>> _byId;

    static ConstantBase()
    {
        var constants = typeof(TClass)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(ConstantDefinition<TType>))
            .Select(p => (ConstantDefinition<TType>)p.GetValue(null)!)
            .ToList();

        _byName = constants.ToDictionary(
            cd => cd.Entity.Name,
            cd => cd,
            StringComparer.OrdinalIgnoreCase);

        _byId = constants.ToDictionary(
            cd => cd.Entity.Id,
            cd => cd);
    }

    /// <summary>
    /// Try to get entity by name.
    /// </summary>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out ConstantDefinition<TType>? result)
        => _byName.TryGetValue(name, out result);

    /// <summary>
    /// Try to get entity using Guid.
    /// </summary>
    public static bool TryGetById(Guid id, [NotNullWhen(true)] out ConstantDefinition<TType>? result)
        => _byId.TryGetValue(id, out result);

    /// <summary>
    /// Get all constants as a read-only collection.
    /// </summary>
    public static IReadOnlyCollection<ConstantDefinition<TType>> AllEntities() => _byId.Values;

    /// <summary>
    /// Get all translations as read-only collection.
    /// </summary>
    public static List<TranslationEntry> AllTranslations() => AllEntities()
        .SelectMany(t => (List<TranslationEntry>)t)
        .ToList();
}