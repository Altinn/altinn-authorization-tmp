using System.Buffers;
using System.Text.RegularExpressions;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Utilities;

/// <summary>
/// Compilation of different validator checks
/// </summary>
public static partial class IdentifierValidationUtil
{
    private static readonly SearchValues<char> NUMBERS = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);

    [GeneratedRegex(@"(?!^\d+$)^[a-zA-Z0-9._@\-]{6,64}$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex UsernameGeneratedRegex();

    /// <summary>
    /// Check for Orgnumber
    /// Should be 9 numbers
    /// </summary>
    /// <param name="orgnumber">orgnumer to check</param>
    /// <returns>result of regex match</returns>
    public static bool CheckOrgNumber(ReadOnlySpan<char> orgnumber)
    {
        if (orgnumber.Length != 9)
        {
            return false;
        }

        if (orgnumber.ContainsAnyExcept(NUMBERS))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if it is a valid username
    /// Should be between 6-64, can contain number, letters, ., _, @, -
    /// Can not only be numbers
    /// </summary>
    /// <param name="username">the username</param>
    /// <returns>the result of the check</returns>
    public static bool CheckUsername(string username)
    {
        return username.Length <= 64 && UsernameGeneratedRegex().IsMatch(username);
    }

    /// <summary>
    /// Checks if it is a valid ssn
    /// Should be 9 chars long, can only be numbers
    /// </summary>
    /// <param name="ssn">the ssn</param>
    /// <returns>the result of the check</returns>
    public static bool CheckSSN(ReadOnlySpan<char> ssn)
    {
        if (ssn.Length != 11)
        {
            return false;
        }

        if (ssn.ContainsAnyExcept(NUMBERS))
        {
            return false;
        }

        return true;
    }
}
