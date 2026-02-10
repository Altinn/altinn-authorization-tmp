using System.Diagnostics.CodeAnalysis;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;

/// <summary>
/// Defines a set of constant <see cref="Reason"/> instances used across the system.
/// Each constant represents a specific provider, such as system providers or service owners,
/// with a fixed unique identifier (GUID), code, name, and associated provider type.
/// </summary>
public static class ReasonConstants
{
    /// <summary>
    /// Try to get <see cref="Reason"/> by any identifier: Name or Guid.
    /// </summary>
    public static bool TryGetByAll(string value, [NotNullWhen(true)] out ConstantDefinition<Reason>? result)
    {
        if (TryGetByName(value, out result))
        {
            return true;
        }

        if (Guid.TryParse(value, out var providerGuid) && TryGetById(providerGuid, out result))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Try to get <see cref="Reason"/> by name.
    /// </summary>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out ConstantDefinition<Reason>? result)
        => ConstantLookup.TryGetByName(typeof(ReasonConstants), name, out result);

    /// <summary>
    /// Try to get <see cref="Reason"/> using Guid.
    /// </summary>
    public static bool TryGetById(Guid id, [NotNullWhen(true)] out ConstantDefinition<Reason>? result)
        => ConstantLookup.TryGetById(typeof(ReasonConstants), id, out result);

    /// <summary>
    /// Get all constants as a read-only collection.
    /// </summary>
    public static IReadOnlyCollection<ConstantDefinition<Reason>> AllEntities()
        => ConstantLookup.AllEntities<Reason>(typeof(ReasonConstants));

    /// <summary>
    /// Get all translations as read-only collection.
    /// </summary>
    public static IReadOnlyCollection<TranslationEntry> AllTranslations()
        => ConstantLookup.AllTranslations<Reason>(typeof(ReasonConstants));

    /// <summary>
    /// Assignment - Connection originates from an Assignment
    /// </summary>
    public static ConstantDefinition<Reason> Assignment { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-786d-89a9-100c117fc2ff")
    {
        Entity = new()
        {
            Name = "Assignment",
            Description = "Connection originates from an Assignment"
        },
    };


    /// <summary>
    /// Delegation - Connection originates from a Delegation
    /// </summary>
    public static ConstantDefinition<Reason> Delegation { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-76e8-8c8a-25a6e23a4379")
    {
        Entity = new()
        {
            Name = "Delegation",
            Description = "Connection originates from a Delegation"
        },
    };

    /// <summary>
    /// Hierarchy - Connection originates from a parent/child hierarchy
    /// </summary>
    public static ConstantDefinition<Reason> Hierarchy { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-75d5-b825-8e0ce046d30d")
    {
        Entity = new()
        {
            Name = "Hierarchy",
            Description = "Connection originates from a parent/child hierarchy"
        },
    };

    /// <summary>
    /// Mapped - Connection originates from a RoleMap
    /// </summary>
    public static ConstantDefinition<Reason> Mapped { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-7d0e-82fd-c3cba387b9a8")
    {
        Entity = new()
        {
            Name = "Mapped",
            Description = "Connection originates from a RoleMap"
        },
    };

    /// <summary>
    /// KeyRole - Connection originates from a key role with isKeyRole flag
    /// </summary>
    public static ConstantDefinition<Reason> KeyRole { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-7aaa-9a1a-57c5660b0938")
    {
        Entity = new()
        {
            Name = "KeyRole",
            Description = "Connection originates from a role with isKeyRole flag"
        },
    };

    /// <summary>
    /// HierarchyKeyRole - Connection originates from a key role with isKeyRole flag and parent/child hierarchy
    /// </summary>
    public static ConstantDefinition<Reason> HierarchyKeyRole { get; } = new ConstantDefinition<Reason>("0195efb8-7c80-7641-8634-930d83b883bc")
    {
        Entity = new()
        {
            Name = "HierarchyKeyRole",
            Description = "Connection originates from a key role with isKeyRole flag and parent/child hierarchy"
        },
    };
}

/*
0195efb8-7c80-761f-941a-bd6dcd13225d
0195efb8-7c80-7c83-b15c-e906094127ec
0195efb8-7c80-70ee-b99b-3a141b21e2c1
0195efb8-7c80-7e28-b770-9fb0f181971a
0195efb8-7c80-794e-910e-12969bf5a42f 
0195efb8-7c80-7b06-86ab-d573afc4a795
0195efb8-7c80-7a20-82a2-66b6cb99eba7
0195efb8-7c80-7a29-ace5-c22a6538aab3
0195efb8-7c80-72dc-b61d-eba8616906a2
0195efb8-7c80-7bcc-8a67-18e8d93075bd
0195efb8-7c80-7e76-bb5b-663a903630ed
0195efb8-7c80-7b7e-b491-20529aa5059a
0195efb8-7c80-767b-9fc9-081811366d59
0195efb8-7c80-78d6-8600-18749f5f9898
0195efb8-7c80-72a7-b337-4ba737f3d93a
0195efb8-7c80-7bb4-999f-cdabfb60a54a
0195efb8-7c80-790c-bf3e-204cc367a23e
0195efb8-7c80-7b53-b12e-cef3dca9b1b6
0195efb8-7c80-7715-afab-a7a3340c68f6
0195efb8-7c80-759c-a1b4-5b46020f8103
0195efb8-7c80-7606-b36f-9b3995c3d18b
0195efb8-7c80-7506-a7b7-a13ccb814fe4
0195efb8-7c80-77b7-8787-79af141568df
0195efb8-7c80-7ad1-b482-fdee63f9f9fa
0195efb8-7c80-72b8-b37f-f2a34b23e1d9
0195efb8-7c80-762c-8318-201dcbc5c911
0195efb8-7c80-76e7-91d2-3df78a35857b
0195efb8-7c80-7c7c-9150-e87a46a90840
0195efb8-7c80-7002-b268-128d775b5941
0195efb8-7c80-78c3-a4c4-850e8b7665d3
*/
