using System.Diagnostics;
using System.Text.Json;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class ActivityExtensions
{
    /// <summary>
    /// Creates an event in current traces with serialized json data.
    /// </summary>
    public static void AddJsonParamsEvent<T>(this Activity? activity, string queryName, string paramName, T data)
        where T : class
    {
        if (data is { })
        {
            AddParamsTags(activity, queryName, paramName, JsonSerializer.Serialize(data));
        }
    }

    /// <summary>
    /// Creates an event in current traces with provided raw data.
    /// </summary>
    public static void AddParamsTags(this Activity? activity, string queryName, string paramName, object? data)
    {
        if (activity is { } && data is { })
        {
            var activityEvent = new ActivityEvent(queryName, default, new ActivityTagsCollection()
            {
                { "db.parameter.name", paramName },
                { "db.parameter.value",  data },
            });

            activity.AddEvent(activityEvent);
        }
    }
}
