using Altinn.AccessMgmt.Core.Models.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

public sealed class ConstantDefinition<T>(Guid id)
    where T : IEntityId
{
    public ConstantDefinition(string id)
        : this(Guid.Parse(id)) { }

    private readonly Guid _id = id;

    private T _entity = default!;

    private TranslationEntryList? _en;

    private TranslationEntryList? _nn;

    public Guid Id => _id;

    public required T Entity
    {
        get => _entity;
        init
        {
            _entity = value;
            _entity.Id = _id;
        }
    }

    public TranslationEntryList? EN
    {
        get => _en;
        init
        {
            _en = value;
            if (_en is not null)
            {
                _en.Id = _id;
                _en.LanguageCode = "eng";
                _en.Type = typeof(T).Name;
            }
        }
    }

    public TranslationEntryList? NN
    {
        get => _nn;
        init
        {
            _nn = value;
            if (_nn is not null)
            {
                _nn.Id = _id;
                _nn.LanguageCode = "nno";
                _nn.Type = typeof(T).Name;
            }
        }
    }

    public static implicit operator Guid(ConstantDefinition<T> def)
        => def._id;

    public static implicit operator T(ConstantDefinition<T> def)
        => def._entity;

    public static implicit operator List<TranslationEntry>(ConstantDefinition<T> def)
    {
        var result = new List<TranslationEntry>();
        if (def._en is { })
        {
            result.AddRange(def._en.SingleEntries());
        }

        if (def._nn is { })
        {
            result.AddRange(def._nn.SingleEntries());
        }

        return result;
    }
}
