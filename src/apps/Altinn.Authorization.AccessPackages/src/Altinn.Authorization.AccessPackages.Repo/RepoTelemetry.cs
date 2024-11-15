using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.AccessPackages.Repo;

//public static class RepoTelemetry
//{
//    public static ActivitySource DbAccessSource = new ActivitySource("Altinn.Authorization.Repo");
//    public static Activity? StartDbAccessActivity<T>(string name, ActivityKind kind = ActivityKind.Internal)
//    {
//        var a = DbAccessSource.StartDbAccessActivity(name + $"<{typeof(T).Name}>", kind);
//        a?.SetCustomProperty("Type", typeof(T));
//        return a;
//    }
//}

public class JsonIngestMeters
{
    public Meter Meter { get; set; }

    public Dictionary<string, Gauge<int>> Counters { get; set; } = new Dictionary<string, Gauge<int>>();

    public Gauge<int> Test { get; set; }


    public JsonIngestMeters(IMeterFactory meterFactory)
    {
        Meter = meterFactory.Create("ingest-metric", "1.0.0");
        Test = Meter.CreateGauge<int>("init", "item");
        Test.Record(1);
    }

    private void CreateMeter(string key)
    {
        Counters.Add(key, Meter.CreateGauge<int>(key, "Item"));
    }

    public void SetIngestValues(string key, int json, int db)
    {
        Test.Record(1);
        Console.WriteLine($"Setting metric {key}:{json}:{db}");
        var jsonCounter = $"{key.ToLower()}-json";
        var dbCounter = $"{key.ToLower()}-db";
        if (!Counters.ContainsKey(jsonCounter))
        {
            Console.WriteLine("Create counter " + jsonCounter);
            CreateMeter(jsonCounter);
        }

        if (!Counters.ContainsKey(dbCounter))
        {
            Console.WriteLine("Create counter " + dbCounter);
            CreateMeter(dbCounter);
        }

        Counters[jsonCounter].Record(json);
        Counters[dbCounter].Record(db);
    }
}
