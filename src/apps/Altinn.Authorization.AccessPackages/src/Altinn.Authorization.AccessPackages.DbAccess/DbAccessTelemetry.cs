using System.Diagnostics;

namespace Altinn.Authorization.AccessPackages.DbAccess;
public static class DbAccessTelemetry
{
    public static ActivitySource DbAccessSource = new ActivitySource("Altinn.Authorization.AccessPackages.DbAccess");
    public static ActivitySource RepoSource = new ActivitySource("Altinn.Authorization.AccessPackages.Repo");

    public static Activity? StartDbAccessActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var a = DbAccessSource.StartActivity(name + $"<{typeof(T).Name}>", kind);
        a?.SetCustomProperty("Type", typeof(T));
        return a;
    }

    public static Activity? StartRepoActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var a = RepoSource.StartActivity(name + $"<{typeof(T).Name}>", kind);
        a?.SetCustomProperty("Type", typeof(T));
        return a;
    }
}
