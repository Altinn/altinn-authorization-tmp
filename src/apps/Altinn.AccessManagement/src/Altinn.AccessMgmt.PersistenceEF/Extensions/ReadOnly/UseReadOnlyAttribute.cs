namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class UseReadOnlyAttribute : Attribute
{
    public string Name { get; }

    public UseReadOnlyAttribute(string name)
    {
        Name = name;
    }
}
