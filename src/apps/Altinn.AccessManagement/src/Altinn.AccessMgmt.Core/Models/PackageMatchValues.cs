#nullable enable

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// The query parameter values that map onto <see cref="PackageMatch"/>.
/// </summary>
public static class PackageMatchValues
{
    /// <summary>
    /// Query value for <see cref="PackageMatch.Any"/>.
    /// </summary>
    public const string Any = "any";

    /// <summary>
    /// Query value for <see cref="PackageMatch.All"/>.
    /// </summary>
    public const string All = "all";

    /// <summary>
    /// Gets the accepted query values.
    /// </summary>
    public static IReadOnlyList<string> Valid { get; } = [Any, All];

    /// <summary>
    /// Parses a raw query value into a <see cref="PackageMatch"/>.
    /// </summary>
    /// <param name="value">The raw query value. An absent value yields <paramref name="fallback"/>.</param>
    /// <param name="fallback">The value used when nothing is provided.</param>
    /// <param name="packageMatch">The parsed value.</param>
    /// <returns>True when the value is absent or recognized, false otherwise.</returns>
    public static bool TryParse(string? value, PackageMatch fallback, out PackageMatch packageMatch)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            packageMatch = fallback;
            return true;
        }

        if (string.Equals(value, Any, StringComparison.OrdinalIgnoreCase))
        {
            packageMatch = PackageMatch.Any;
            return true;
        }

        if (string.Equals(value, All, StringComparison.OrdinalIgnoreCase))
        {
            packageMatch = PackageMatch.All;
            return true;
        }

        packageMatch = fallback;
        return false;
    }
}
