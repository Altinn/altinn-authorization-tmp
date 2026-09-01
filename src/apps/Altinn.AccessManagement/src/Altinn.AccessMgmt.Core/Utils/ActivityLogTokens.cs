using System.Text.Json;
using Altinn.AccessMgmt.PersistenceEF.Queries;

namespace Altinn.AccessMgmt.Core.Utils;

/// <summary>
/// Encodes and decodes the opaque continuation token used by the activity log API,
/// wrapping the keyset cursor <see cref="ActivityLogQueryCursor"/>.
/// </summary>
public static class ActivityLogTokens
{
    /// <summary>
    /// Encodes a cursor as an opaque token, or returns <see langword="null"/> for a null cursor.
    /// </summary>
    public static string Encode(ActivityLogQueryCursor cursor)
        => cursor is null ? null : Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(cursor));

    /// <summary>
    /// Decodes an opaque token back to a cursor. Returns false when the token is not one
    /// produced by <see cref="Encode"/>.
    /// </summary>
    public static bool TryDecode(string token, out ActivityLogQueryCursor cursor)
    {
        cursor = null;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            cursor = JsonSerializer.Deserialize<ActivityLogQueryCursor>(Convert.FromBase64String(token));
            return cursor is not null;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
