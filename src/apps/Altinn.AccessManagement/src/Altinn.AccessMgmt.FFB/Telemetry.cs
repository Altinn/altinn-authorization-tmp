using System.Diagnostics;

namespace Altinn.AccessMgmt.FFB;

public static class Telemetry
{
    public static ActivitySource Source = new ActivitySource("Altinn.AccessMgmt.FFB");

    public static Activity? StartActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var a = Source.StartActivity(name + $"<{typeof(T).Name}>", kind);
        a?.SetCustomProperty("Type", typeof(T));
        return a;
    }
}
