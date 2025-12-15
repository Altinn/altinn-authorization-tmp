namespace Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;

public sealed class HintService : IHintService
{
    private readonly AsyncLocal<HintContext> current = new();

    public HintContext GetHint() => current.Value;

    public IDisposable Use(string hint = null)
    {
        return Use(HintKind.Default, hint);
    }

    public IDisposable Use(HintKind kind, string hint = null)
    {
        var previous = current.Value;

        current.Value = new HintContext
        {
            Value = hint,
            Kind = kind,
            Parent = previous
        };

        return new HintScope(this, previous);
    }

    public void SetHint(string hint)
    {
        SetHint(HintKind.Default, hint);
    }

    public void SetHint(HintKind kind, string hint)
    {
        current.Value = new HintContext
        {
            Value = hint,
            Kind = kind,
            Parent = null
        };
    }

    public void ClearHint()
    {
        current.Value = null;
    }

    private void Restore(HintContext previous)
    {
        current.Value = previous;
    }

    public sealed class HintContext
    {
        public string Value { get; init; }

        public HintKind Kind { get; set; }

        public HintContext Parent { get; init; }
    }

    private sealed class HintScope : IDisposable
    {
        private readonly HintService owner;
        private readonly HintContext previous;
        private bool disposed;

        public HintScope(HintService owner, HintContext previous)
        {
            this.owner = owner;
            this.previous = previous;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            owner.Restore(previous);
        }
    }
}

public static class HintServiceExtensions
{
    public static IDisposable UseReadOnly(this IHintService hints, string? replicaKey = null)
    {
        return hints.Use(HintKind.ConnectionReadOnly, replicaKey);
    }

    public static void SetReadOnly(this IHintService hints, string? replicaKey = null)
    {
        hints.SetHint(HintKind.ConnectionReadOnly, replicaKey);
    }
}

public enum HintKind 
{ 
    Default, 
    ConnectionReadOnly 
}
