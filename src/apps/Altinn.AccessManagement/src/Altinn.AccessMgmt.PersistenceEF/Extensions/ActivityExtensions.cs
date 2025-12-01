using System.Diagnostics;
using System.Text.Json;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class ActivityExtensions
{
    /// <summary>
    /// Adds tags to the activity baggage with "db." prefix
    /// </summary>
    public static void AddDbJsonTags<T>(this Activity? activity, string key, T data)
    {
        if (activity is { } && data is { })
        {
            activity.AddBaggage($"db.{key}", JsonSerializer.Serialize(data));
        }
    }

    /// <summary>
    /// Adds tags to the activity baggage with "db." prefix
    /// </summary>
    public static void AddDbTags(this Activity? activity, string key, string data)
    {
        if (activity is { } && !string.IsNullOrEmpty(data))
        {
            activity.AddBaggage($"db.{key}", data);
        }
    }
}
