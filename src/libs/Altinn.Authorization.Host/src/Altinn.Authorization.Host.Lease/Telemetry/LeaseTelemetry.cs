using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Lease.Telemetry;

/// <summary>
/// Config to be used for Telemetry in Altinn.AccessManagement.Persistence
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class LeaseTelemetry
{
    private const string StatusSuccess = "Success";

    private const string StatusFailed = "Failed";

    private const string StatusLeaseLost = "LeaseLost";

    private const string StatusLeasePresent = "LeasePresent";

    /// <summary>
    /// Used as source for the current project.
    /// </summary>
    internal static readonly ActivitySource ActivitySource = new("Altinn.Authorization.Host.Lease");

    /// <summary>
    /// Meters for lease implementation.
    /// </summary>
    internal static readonly Meter Meter = new("Altinn.Authorization.Host.Lease");

    internal static void RecordLeaseDuration(string lease, TimeSpan duration)
    {
        Meters.LeaseDuration.Record(duration.TotalSeconds, new KeyValuePair<string, object?>("lease", lease));
    }

    internal static async Task<T> RecordLeaseAcquire<T>(ILogger logger, string lease, Func<Task<T>> leaseFunc)
    {
        var activity = Activity.Current;

        var status = StatusSuccess;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await leaseFunc();
            Success(logger, activity, lease);
            return result;
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode.Equals(BlobErrorCode.LeaseAlreadyPresent))
            {
                status = StatusLeasePresent;
                LeasePresent(logger, activity, lease);
            }
            else
            {
                status = StatusFailed;
                Failed(logger, activity, lease, ex.ErrorCode);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            Meters.Acquire.Latency.Record(
                stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("lease", lease),
                new KeyValuePair<string, object?>("status", status)
            );
        }

        static void LeasePresent(ILogger logger, Activity? activity, string lease)
        {
            activity?.AddEvent(new ActivityEvent(
                "Lease already present",
                tags: new ActivityTagsCollection([new("lease", lease)])));

            Meters.Acquire.Present.Increment(lease);
            Log.Acquire.Present(logger, lease);
        }

        static void Failed(ILogger logger, Activity? activity, string lease, string errorCode)
        {
            activity?.AddEvent(new ActivityEvent(
                "Failed to acquire lease",
                tags: new ActivityTagsCollection([new("lease", lease), new("error_code", errorCode)])));

            Meters.Acquire.Failed.Increment(lease);
            Log.Acquire.Failed(logger, lease, errorCode);
        }

        static void Success(ILogger logger, Activity? activity, string lease)
        {
            activity?.AddEvent(new ActivityEvent(
                "Successfully acquired lease",
                tags: new ActivityTagsCollection([new("lease", lease)])));

            Meters.Acquire.Success.Increment(lease);
            Log.Acquire.Success(logger, lease);
        }
    }

    internal static async Task<T> RecordLeasePut<T>(ILogger logger, string leaseName, Func<Task<T>> leaseFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        var status = StatusSuccess;
        var activity = Activity.Current;

        try
        {
            var result = await leaseFunc();

            Success(logger, activity, leaseName);
            return result;
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
                status = StatusLeaseLost;
                LeaseLost(logger, activity, leaseName, ex.ErrorCode);
            }
            else
            {
                status = StatusFailed;
                Failed(logger, activity, leaseName, ex.ErrorCode);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            Meters.Put.Latency.Record(
                stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("lease", leaseName),
                new KeyValuePair<string, object?>("status", status)
            );
        }

        static void Success(ILogger logger, Activity? activity, string lease)
        {
            activity?.AddEvent(new ActivityEvent(
                "Successfully updated lease content",
                tags: new ActivityTagsCollection([new("lease", lease)])));

            Meters.Put.Success.Increment(lease);
            Log.Put.Success(logger, lease);
        }

        static void LeaseLost(ILogger logger, Activity? activity, string lease, string errorCode)
        {
            activity?.AddEvent(new ActivityEvent(
                "Lease lost while updating content",
                tags: new ActivityTagsCollection([new("lease", lease), new("error_code", errorCode)])));

            Meters.Put.LeaseLost.Increment(lease);
            Log.Put.LeaseLost(logger, lease, errorCode);
        }

        static void Failed(ILogger logger, Activity? activity, string lease, string error)
        {
            activity?.AddEvent(new ActivityEvent(
                "Failed to update lease content",
                tags: new ActivityTagsCollection([new("lease", lease), new("error", error)])));

            Meters.Put.Failed.Increment(lease);
            Log.Put.Failed(logger, lease, error);
        }
    }

    internal static async Task<T> RecordReleaseLease<T>(ILogger logger, string leaseName, Func<Task<T>> leaseFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        var status = StatusSuccess;
        var activity = Activity.Current;

        try
        {
            var result = await leaseFunc();

            Success(logger, activity, leaseName);
            return result;
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
                status = StatusLeaseLost;
                LeaseLost(logger, activity, leaseName, ex.ErrorCode);
            }
            else
            {
                status = StatusFailed;
                Failed(logger, activity, leaseName, ex.ErrorCode);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            Meters.Release.Latency.Record(
                stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("lease", leaseName),
                new KeyValuePair<string, object?>("status", status)
            );
        }

        static void Success(ILogger logger, Activity? activity, string lease)
        {
            activity?.AddEvent(new ActivityEvent(
                "Successfully released lease",
                tags: new ActivityTagsCollection([new("lease", lease)])));

            Meters.Release.Success.Increment(lease);
            Log.Release.Success(logger, lease);
        }

        static void LeaseLost(ILogger logger, Activity? activity, string lease, string errorCode)
        {
            activity?.AddEvent(new ActivityEvent(
                "Lease lost while releasing",
                tags: new ActivityTagsCollection([new("lease", lease), new("error_code", errorCode)])));

            Meters.Release.LeaseLost.Increment(lease);
            Log.Release.LeaseLost(logger, lease, errorCode);
        }

        static void Failed(ILogger logger, Activity? activity, string lease, string error)
        {
            activity?.AddEvent(new ActivityEvent(
                "Failed to release lease",
                tags: new ActivityTagsCollection([new("lease", lease), new("error", error)])));

            Meters.Release.Failed.Increment(lease);
            Log.Release.Failed(logger, lease, error);
        }
    }

    internal static async Task<T> RecordRefreshLease<T>(ILogger logger, string leaseName, Func<Task<T>> leaseFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        var status = StatusSuccess;
        var activity = Activity.Current;

        try
        {
            var result = await leaseFunc();

            Success(logger, activity, leaseName);
            return result;
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
                status = StatusLeaseLost;
                LeaseLost(logger, activity, leaseName, ex.ErrorCode);
            }
            else
            {
                status = StatusFailed;
                Failed(logger, activity, leaseName, ex.ErrorCode);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
            Meters.Refresh.Latency.Record(
                stopwatch.Elapsed.TotalSeconds,
                new KeyValuePair<string, object?>("lease", leaseName),
                new KeyValuePair<string, object?>("status", status)
            );
        }

        static void Success(ILogger logger, Activity? activity, string lease)
        {
            activity?.AddEvent(new ActivityEvent(
                "Successfully refreshed lease",
                tags: new ActivityTagsCollection([new("lease", lease)])));

            Meters.Refresh.Success.Increment(lease);
            Log.Refresh.Success(logger, lease);
        }

        static void LeaseLost(ILogger logger, Activity? activity, string lease, string errorCode)
        {
            activity?.AddEvent(new ActivityEvent(
                "Lease lost while refreshing",
                tags: new ActivityTagsCollection([new("lease", lease), new("error_code", errorCode)])));

            Meters.Refresh.LeaseLost.Increment(lease);
            Log.Refresh.LeaseLost(logger, lease, errorCode);
        }

        static void Failed(ILogger logger, Activity? activity, string lease, string error)
        {
            activity?.AddEvent(new ActivityEvent(
                "Failed to refresh lease",
                tags: new ActivityTagsCollection([new("lease", lease), new("error", error)])));

            Meters.Refresh.Failed.Increment(lease);
            Log.Refresh.Failed(logger, lease, error);
        }
    }

    internal static void Increment(this Counter<int> counter, string leaseName, params KeyValuePair<string, object?>[] tags)
    {
        if (counter is { })
        {
            counter.Add(1, [new("lease", leaseName), .. tags]);
        }
    }

    static partial class Meters
    {
        internal static class Acquire
        {
            private const string Namespace = "lease.acquire";

            internal static readonly Counter<int> Success =
                Meter.CreateCounter<int>($"{Namespace}.success");

            internal static readonly Counter<int> Failed =
                Meter.CreateCounter<int>($"{Namespace}.failed");

            internal static readonly Counter<int> Present =
                Meter.CreateCounter<int>($"{Namespace}.present");

            internal static readonly Histogram<double> Latency =
                Meter.CreateHistogram<double>($"{Namespace}.latency");
        }

        internal static class Put
        {
            private const string Namespace = "lease.put";

            internal static readonly Counter<int> Success =
                Meter.CreateCounter<int>($"{Namespace}.success");

            internal static readonly Counter<int> Failed =
                Meter.CreateCounter<int>($"{Namespace}.failed");

            internal static readonly Counter<int> LeaseLost =
                Meter.CreateCounter<int>($"{Namespace}.lease_lost");

            internal static readonly Histogram<double> Latency =
                Meter.CreateHistogram<double>($"{Namespace}.latency");
        }

        internal static class Refresh
        {
            private const string Namespace = "lease.refresh";

            internal static readonly Counter<int> Success =
                Meter.CreateCounter<int>($"{Namespace}.success");

            internal static readonly Counter<int> Failed =
                Meter.CreateCounter<int>($"{Namespace}.failed");

            internal static readonly Counter<int> LeaseLost =
                Meter.CreateCounter<int>($"{Namespace}.lease_lost");

            internal static readonly Histogram<double> Latency =
                Meter.CreateHistogram<double>($"{Namespace}.latency");
        }

        internal static class Release
        {
            private const string Namespace = "lease.release";

            internal static readonly Counter<int> Success =
                Meter.CreateCounter<int>($"{Namespace}.success");

            internal static readonly Counter<int> Failed =
                Meter.CreateCounter<int>($"{Namespace}.failed");

            internal static readonly Counter<int> LeaseLost =
                Meter.CreateCounter<int>($"{Namespace}.lease_lost");

            internal static readonly Histogram<double> Latency =
                Meter.CreateHistogram<double>($"{Namespace}.latency");
        }

        // Lease duration
        internal static readonly Histogram<double> LeaseDuration =
            Meter.CreateHistogram<double>("lease.duration");
    }

    static partial class Log
    {
        internal static partial class Acquire
        {
            [LoggerMessage(EventId = 100, Level = LogLevel.Debug, Message = "Attempting to acquire lease '{lease}'.")]
            internal static partial void Attempt(ILogger logger, string lease);

            [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Successfully acquired lease '{lease}'.")]
            internal static partial void Success(ILogger logger, string lease);

            [LoggerMessage(EventId = 103, Level = LogLevel.Debug, Message = "Lease '{lease}' is already present and cannot be acquired.")]
            internal static partial void Present(ILogger logger, string lease);

            [LoggerMessage(EventId = 104, Level = LogLevel.Error, Message = "Failed to acquire lease '{lease}'. ErrorCode: {errorCode}.")]
            internal static partial void Failed(ILogger logger, string lease, string errorCode);
        }

        internal static partial class Put
        {
            [LoggerMessage(EventId = 200, Level = LogLevel.Information, Message = "Successfully updated content for lease '{lease}'.")]
            internal static partial void Success(ILogger logger, string lease);

            [LoggerMessage(EventId = 201, Level = LogLevel.Warning, Message = "Lease '{lease}' was lost while updating content. ErrorCode: {errorCode}.")]
            internal static partial void LeaseLost(ILogger logger, string lease, string errorCode);

            [LoggerMessage(EventId = 202, Level = LogLevel.Error, Message = "Failed to update content for lease '{lease}'. Error: {error}.")]
            internal static partial void Failed(ILogger logger, string lease, string error);
        }

        internal static partial class Refresh
        {
            [LoggerMessage(EventId = 300, Level = LogLevel.Information, Message = "Successfully refreshed lease '{lease}'.")]
            internal static partial void Success(ILogger logger, string lease);

            [LoggerMessage(EventId = 301, Level = LogLevel.Error, Message = "Failed to refresh lease '{lease}'. Error: {error}.")]
            internal static partial void Failed(ILogger logger, string lease, string error);

            [LoggerMessage(EventId = 302, Level = LogLevel.Warning, Message = "Lease '{lease}' was lost while attempting to refresh. ErrorCode: {errorCode}.")]
            internal static partial void LeaseLost(ILogger logger, string lease, string errorCode);
        }

        internal static partial class Release
        {
            [LoggerMessage(EventId = 400, Level = LogLevel.Information, Message = "Successfully released lease '{lease}'.")]
            internal static partial void Success(ILogger logger, string lease);

            [LoggerMessage(EventId = 401, Level = LogLevel.Error, Message = "Failed to release lease '{lease}'. ErrorCode: {errorCode}.")]
            internal static partial void Failed(ILogger logger, string lease, string errorCode);

            [LoggerMessage(EventId = 402, Level = LogLevel.Warning, Message = "Lease '{lease}' was already lost before release. ErrorCode: {errorCode}.")]
            internal static partial void LeaseLost(ILogger logger, string lease, string errorCode);
        }
    }
}
