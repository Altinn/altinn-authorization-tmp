using System.Diagnostics;

namespace Altinn.Authorization.FFB;
public static class Telemetry
{
    public static ActivitySource Source = new ActivitySource("Altinn.Authorization.FFB");

    public static Activity? StartActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var a = Source.StartActivity(name + $"<{typeof(T).Name}>", kind);
        a?.SetCustomProperty("Type", typeof(T));
        return a;
    }
}
