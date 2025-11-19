using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Altinn.Authorization.Scopes;

public abstract class ScopeSearchValues
    : IReadOnlyList<string>
{
    public static ScopeSearchValues Create(scoped ReadOnlySpan<string> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values cannot be empty", nameof(values));
        }

        return new RegexScopeSearchValues(ImmutableArray.Create(values));
    }

    private readonly ImmutableArray<string> _values;

    private ScopeSearchValues(ImmutableArray<string> values)
    {
        _values = values;
    }

    /// <inheritdoc/>
    public string this[int index] => _values[index];

    /// <inheritdoc/>
    public int Count => _values.Length;

    /// <inheritdoc cref="ImmutableArray{T}.GetEnumerator()"/>
    public ImmutableArray<string>.Enumerator GetEnumerator()
        => _values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)_values).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)_values).GetEnumerator();

    public abstract bool Check(ReadOnlySpan<char> scopeString);

    private sealed class RegexScopeSearchValues
        : ScopeSearchValues
    {
        private readonly Regex _regex;

        public RegexScopeSearchValues(ImmutableArray<string> values)
            : base(values)
        {
            // preceeded by either whitespace or start of string
            var pattern = new StringBuilder("(?<=^| )(");

            var first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    pattern.Append('|');
                }

                first = false;
                pattern.Append(Regex.Escape(value));
            }

            // succeeded by either whitespace or end of string
            pattern.Append(")(?=$| )");

            _regex = new Regex(pattern.ToString(), RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }

        public override bool Check(ReadOnlySpan<char> scopeString)
            => _regex.IsMatch(scopeString);
    }
}
