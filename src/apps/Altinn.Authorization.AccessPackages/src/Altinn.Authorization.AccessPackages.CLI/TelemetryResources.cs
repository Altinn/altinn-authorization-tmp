using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.AccessPackages.CLI;

/// <summary>
/// RepoTelemetry
/// </summary>
public static class TelemetryResources
{
    /// <summary>
    /// RepoTelemetry DbAccessSource
    /// </summary>
    public static ActivitySource Source = new ActivitySource("Altinn.Authorization.AccessPackages.CLI", "1.0.0");
}

public class JsonIngestMetersTest
{
    public Meter Meter { get; set; }

    public Dictionary<string, Gauge<int>> Counters { get; set; } = new Dictionary<string, Gauge<int>>();

    public JsonIngestMetersTest(IMeterFactory meterFactory)
    {
        Meter = meterFactory.Create("ingest");
    }

    private void CreateMeter(string key)
    {
        Counters.Add(key, Meter.CreateGauge<int>(key, "Item"));
    }

    public void SetIngestValues(string key, int json, int db)
    {
        var jsonCounter = $"{key.ToLower()}-json";
        var dbCounter = $"{key.ToLower()}-db";
        if (!Counters.ContainsKey(jsonCounter))
        {
            CreateMeter(jsonCounter);
        }

        if (!Counters.ContainsKey(dbCounter))
        {
            CreateMeter(dbCounter);
        }

        Counters[jsonCounter].Record(json);
        Counters[dbCounter].Record(db);
    }
}
