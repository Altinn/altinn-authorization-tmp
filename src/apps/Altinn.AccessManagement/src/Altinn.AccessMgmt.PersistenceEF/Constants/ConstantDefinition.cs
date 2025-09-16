using Altinn.AccessMgmt.Core.Models.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.PersistenceEF.Constants;

public sealed class ConstantDefinition<T>
    where T : IEntityId
{
    private readonly Guid _id;

    private T? _entity;

    private TranslationEntryList? _en;

    private TranslationEntryList? _nn;

    public Guid Id => _id;

    public ConstantDefinition(string id)
    {
        _id = Guid.Parse(id);
    }

    public T? Entity
    {
        get => _entity;
        init
        {
            _entity = value;
            if (_entity is { })
            {
                _entity.Id = _id;
            }
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
                _en.Id = _id;
                _en.LanguageCode = "nno";
                _en.Type = typeof(T).Name;
            }
        }
    }
}
