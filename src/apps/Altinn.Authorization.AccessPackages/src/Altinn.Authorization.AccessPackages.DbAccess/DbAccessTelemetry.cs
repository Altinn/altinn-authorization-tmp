using System.Diagnostics;

namespace Altinn.Authorization.AccessPackages.DbAccess;
public static class DbAccessTelemetry
{
    public static ActivitySource DbAccessSource = new ActivitySource("Altinn.Authorization.AccessPackages.DbAccess");

    public static Activity? StartActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var a = DbAccessSource.StartActivity(name + $"<{typeof(T).Name}>", kind);
        a?.SetCustomProperty("Type", typeof(T));
        return a;
    }
}
