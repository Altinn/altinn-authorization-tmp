namespace Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class UseHintAttribute : Attribute
{
    public string Name { get; }

    public UseHintAttribute(string name)
    {
        Name = name;
    }
}
