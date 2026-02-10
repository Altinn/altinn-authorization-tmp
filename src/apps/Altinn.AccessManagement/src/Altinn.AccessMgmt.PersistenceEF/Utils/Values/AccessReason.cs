namespace Altinn.AccessMgmt.PersistenceEF.Utils.Values;

/// <summary>
/// Reasons for access
/// </summary>
public sealed class AccessReason
{
    private readonly List<HashSet<AccessReasonKey>> groups;

    private AccessReason(List<HashSet<AccessReasonKey>> groups)
        => this.groups = groups;

    public static AccessReason Set(params AccessReasonKey[] keys)
        => new(new List<HashSet<AccessReasonKey>>
        {
            new(keys)
        });

    public static AccessReason Set(params AccessReason[] reasons)
        => new(reasons.SelectMany(r => r.groups).ToList());

    public AccessReason And(AccessReasonKey key)
    {
        var copy = Clone();
        copy.groups[^1].Add(key);
        return copy;
    }

    public AccessReason Or(params AccessReasonKey[] keys)
    {
        var copy = Clone();
        copy.groups.Add(new HashSet<AccessReasonKey>(keys));
        return copy;
    }

    public AccessReason Or(AccessReason other)
    {
        var copy = Clone();
        copy.groups.AddRange(other.groups);
        return copy;
    }

    public bool Matches(AccessReason actual)
    {
        foreach (var required in groups)
        {
            if (actual.groups.Any(actualGroup =>
                required.All(actualGroup.Contains)))
            {
                return true;
            }
        }

        return false;
    }

    public static AccessReason Assignment => Set(AccessReasonKeys.Assignment);

    public static AccessReason Delegation => Set(AccessReasonKeys.Delegation);

    public static AccessReason KeyRole => Set(AccessReasonKeys.KeyRole);

    public static AccessReason RoleMap => Set(AccessReasonKeys.RoleMap);

    public static AccessReason Hierarchy => Set(AccessReasonKeys.Hierarchy);

    public IReadOnlyList<IReadOnlyCollection<AccessReasonKey>> All =>
        groups.Select(g => (IReadOnlyCollection<AccessReasonKey>)g).ToList();

    public IReadOnlyList<IReadOnlyCollection<string>> Names =>
        groups.Select(g => g.Select(k => k.Name).ToList()).ToList();

    private AccessReason Clone()
        => new(
            groups
                .Select(g => new HashSet<AccessReasonKey>(g))
                .ToList()
        );
}

public sealed record AccessReasonKey(string Name, string Description)
{
    public override string ToString() => Name;
}

public static class AccessReasonKeys
{
    public static readonly AccessReasonKey Assignment =
        new("assignment", "Access granted directly with roleassignment");

    public static readonly AccessReasonKey Delegation =
        new("delegation", "Access granted via delegation");

    public static readonly AccessReasonKey KeyRole =
        new("keyrole", "Access granted through a key role");

    public static readonly AccessReasonKey RoleMap =
        new("rolemap", "Access granted through rolemapping");

    public static readonly AccessReasonKey Hierarchy =
        new("hierarchy", "Access granted through parent/child relation");
}
