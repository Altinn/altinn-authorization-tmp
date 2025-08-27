namespace Altinn.Authorization.Host.Job;

public class JobResult
{
    internal JobResult() { }

    public JobStatus JobStatus { get; set; }

    public object? Data { get; set; }

    public static JobResult Success(object? data = null) => new()
    {
        Data = data,
        JobStatus = JobStatus.Success,
    };

    public static JobResult LostLease(object data = null) => new()
    {
        JobStatus = JobStatus.LostLease,
        Data = data ?? "Lost lease during runtime",
    };

    public static JobResult Failure(object? data = null) => new()
    {
        JobStatus = JobStatus.Failure,
        Data = data,
    };

    public static JobResult CouldNotRun(object? data = null) => new()
    {
        JobStatus = JobStatus.CouldNotRun,
        Data = data,
    };

    public static JobResult FeatureFlagProhibited(object? data = null) => new()
    {
        JobStatus = JobStatus.Cancelled,
        Data = data,
    };

    public static JobResult Cancelled(object? data = null, OperationCanceledException ex = null) => new()
    {
        JobStatus = JobStatus.Cancelled,
        Data = data,
    };
}

public enum JobStatus
{
    Success = 0,

    CouldNotRun = 1,

    LostLease = 2,

    FeatureFlagProhibited = 3,

    Cancelled = 4,

    Failure = 5,
}
