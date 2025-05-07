namespace Altinn.AccessManagement.Api.Enduser;

internal static class StringExtensions
{
    /// <summary>
    /// Add support for me
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    internal static Guid TryConvertToUuid(this string value, Guid fallback)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            return Guid.Empty;
        }

        if (value.Equals("me", StringComparison.InvariantCultureIgnoreCase))
        {
            return fallback;
        }

        return Guid.Parse(value);
    }
}
