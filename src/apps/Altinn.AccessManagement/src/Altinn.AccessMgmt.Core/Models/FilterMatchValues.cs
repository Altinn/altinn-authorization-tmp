#nullable enable

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// The query parameter values that map onto <see cref="FilterMatch"/>.
/// </summary>
public static class FilterMatchValues
{
    /// <summary>
    /// Query value for <see cref="FilterMatch.Any"/>.
    /// </summary>
    public const string Any = "any";

    /// <summary>
    /// Query value for <see cref="FilterMatch.All"/>.
    /// </summary>
    public const string All = "all";

    /// <summary>
    /// Gets the accepted query values.
    /// </summary>
    public static IReadOnlyList<string> Valid { get; } = [Any, All];

    /// <summary>
    /// Parses a raw query value into a <see cref="FilterMatch"/>.
    /// </summary>
    /// <param name="value">The raw query value. An absent value yields <paramref name="fallback"/>.</param>
    /// <param name="fallback">The value used when nothing is provided.</param>
    /// <param name="filterMatch">The parsed value.</param>
    /// <returns>True when the value is absent or recognized, false otherwise.</returns>
    public static bool TryParse(string? value, FilterMatch fallback, out FilterMatch filterMatch)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            filterMatch = fallback;
            return true;
        }

        if (string.Equals(value, Any, StringComparison.OrdinalIgnoreCase))
        {
            filterMatch = FilterMatch.Any;
            return true;
        }

        if (string.Equals(value, All, StringComparison.OrdinalIgnoreCase))
        {
            filterMatch = FilterMatch.All;
            return true;
        }

        filterMatch = fallback;
        return false;
    }
}
