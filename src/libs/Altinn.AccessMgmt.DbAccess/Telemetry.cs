using System.Diagnostics;

namespace Altinn.AccessMgmt.DbAccess;

/// <summary>
/// Telemetry
/// </summary>
public static class Telemetry
{
    private static readonly ActivitySource ActivitySource = new ActivitySource("Altinn.AccessMgmt.DbAccess", "1.0.0");

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="name">The operation name of the Activity</param>
    /// <returns></returns>
    public static Activity? StartActivity(string name = "")
    {
        return ActivitySource.StartActivity(name: name);
    }
}
