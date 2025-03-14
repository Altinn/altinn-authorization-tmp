using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Core.Services;

public class IngestBatchMerger<T>
{
    private readonly IIngestService ingestService;

    public List<T> DataTrack1 { get; set; }

    public List<T> DataTrack2 { get; set; }

    public bool Track1Full { get; set; }

    public bool Track2Full { get; set; }

    public Guid Track1Id { get; set; }

    public Guid Track2Id { get; set; }

    public int Limit { get; set; } = 100;

    public IReadOnlyList<GenericParameter> MergeMatchFilter { get; set; }

    public IngestBatchMerger(IIngestService ingestService, IReadOnlyList<GenericParameter> matchFilter = null, int limit = 100)
    {
        this.ingestService = ingestService;
        MergeMatchFilter = matchFilter ?? new List<GenericParameter>() { new GenericParameter("id", "id") }.AsReadOnly();
        Limit = limit;
        ResetTrack1();
        ResetTrack2();
    }

    public void ResetTrack1()
    {
        Track1Id = Guid.NewGuid();
        DataTrack1 = new List<T>();
        Track1Full = false;
    }

    public void ResetTrack2()
    {
        Track2Id = Guid.NewGuid();
        DataTrack2 = new List<T>();
        Track2Full = false;
    }

    public bool AddData(T data)
    {
        if (!Track1Full)
        {
            DataTrack1.Add(data);
            if (DataTrack1.Count >= Limit)
            {
                Track1Full = true;
            }

            return true;
        }

        if (!Track2Full)
        {
            DataTrack2.Add(data);
            if (DataTrack2.Count >= Limit)
            {
                Track2Full = true;
            }

            return true;
        }

        return false;
    }

    public async Task WriteData()
    {
        if (Track1Full)
        {
            Console.WriteLine("Write batch to db");
            await ingestService.IngestTempData(DataTrack1, Track1Id);

            Console.WriteLine("Merge batch to db");
            await ingestService.MergeTempData<T>(Track1Id, MergeMatchFilter);

            ResetTrack1();
        }

        if (Track2Full)
        {
            Console.WriteLine("Write batch to db");
            await ingestService.IngestTempData(DataTrack2, Track2Id);

            Console.WriteLine("Merge batch to db");
            await ingestService.MergeTempData<T>(Track2Id, MergeMatchFilter);

            ResetTrack2();
        }
    }
}
