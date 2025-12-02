namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public sealed class ReadOnlyHintService : IReadOnlyHintService
{
    private readonly AsyncLocal<HintContext?> current = new();

    public string? GetHint() => current.Value?.Hint;

    public IDisposable Use(string? hint = null)
    {
        var previous = current.Value;

        current.Value = new HintContext
        {
            Hint = hint,
            Parent = previous
        };

        return new HintScope(this, previous);
    }

    public void SetHint(string? hint)
    {
        current.Value = new HintContext
        {
            Hint = hint,
            Parent = null
        };
    }

    public void ClearHint()
    {
        current.Value = null;
    }

    private void Restore(HintContext? previous)
    {
        current.Value = previous;
    }

    private sealed class HintContext
    {
        public string? Hint { get; init; }

        public HintContext? Parent { get; init; }
    }

    private sealed class HintScope : IDisposable
    {
        private readonly ReadOnlyHintService owner;
        private readonly HintContext? previous;
        private bool disposed;

        public HintScope(ReadOnlyHintService owner, HintContext? previous)
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
