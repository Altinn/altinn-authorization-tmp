namespace Altinn.AccessManagement.Api.Enduser;

internal static class StringExtensions
{
    /// <summary>
    /// Add support for me
    /// </summary>
    /// <param name="value"></param>
    /// <param name="party"></param>
    /// <returns></returns>
    internal static Guid ConvertToUuid(this string value, Guid party)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            return Guid.Empty;
        }

        if (value.Equals("me", StringComparison.InvariantCultureIgnoreCase))
        {
            return party;
        }

        return Guid.Parse(value);
    }
}
