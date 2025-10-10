using System.Collections.Concurrent;
using System.Reflection;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Host.Pipeline.Builders;
using Altinn.Authorization.Host.Pipeline.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Altinn.Authorization.Host.Pipeline.HostedServices;

/// <summary>
/// Hosted background service responsible for managing and executing registered data pipelines.
///
/// <para>
/// The <see cref="PipelineHostedService"/> continuously monitors and executes pipeline groups defined in the
/// <see cref="IPipelineRegistry"/>. Each group can contain one or more pipelines (defined by <see cref="IPipelineDescriptor"/>)
/// and may be scheduled to run periodically or on-demand. This service supports:
/// </para>
///
/// <list type="bullet">
/// <item><description>Feature flag control via <see cref="FeatureManager"/> (enabling or disabling entire pipeline groups).</description></item>
/// <item><description>Distributed lease coordination using <see cref="ILeaseService"/> to avoid concurrent runs.</description></item>
/// <item><description>Dynamic runtime construction of pipelines using <see cref="PipelineSourceJob"/>, <see cref="PipelineSegmentJob"/>, and <see cref="PipelineSinkJob"/>.</description></item>
/// <item><description>Graceful cancellation and resource cleanup during application shutdown.</description></item>
/// </list>
///
/// <para>
/// Each pipeline consists of a <em>source</em> (data producer), zero or more <em>segments</em> (transformers),
/// and a <em>sink</em> (consumer). The service builds and runs each pipeline dynamically using reflection
/// and generics, based on its registered definition.
/// </para>
/// </summary>
/// <remarks>
/// This service is registered as a hosted background service and starts automatically with the application.
/// It continues executing configured pipelines until cancellation or application shutdown.
/// </remarks>
internal partial class PipelineHostedService(
    ILogger<PipelineHostedService> logger,
    ILeaseService leaseService,
    FeatureManager featureManager,
    PipelineSourceJob pipelineSourceJob,
    PipelineSegmentJob pipelineSegmentJob,
    PipelineSinkJob pipelineSinkJob,
    IPipelineRegistry registry
) : IHostedService
{
    internal List<Task> DispatchedPipelineGroups { get; set; } = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.HostedServiceStarting(logger);

        if (registry.Groups.Count == 0)
        {
            Log.NoPipelinesRegistered(logger);
            return Task.CompletedTask;
        }

        foreach (var group in registry.Groups)
        {
            try
            {
                Log.PipelineGroupRegistered(logger, group.GroupName, group.Builders.Count, group.Recurring?.ToString() ?? "none");
                DispatchedPipelineGroups.Add(DispatchPipelines(group, cancellationToken));
            }
            catch (Exception ex)
            {
                Log.UnhandledPipelineGroupError(logger, group.GroupName, ex);
            }
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.HostedServiceStopping(logger);

        try
        {
            await Task.WhenAll(DispatchedPipelineGroups);
        }
        catch (Exception ex)
        {
            Log.FailedToStopPipelines(logger, ex);
        }
    }

    private async Task DispatchPipelines(PipelineGroup group, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var descriptor in group.Builders)
            {
                if (!await IsPipelineEnabled(group, cancellationToken))
                {
                    Log.PipelineDisabled(logger, group.GroupName, descriptor.Name ?? "(unnamed)");
                    continue;
                }

                var state = new PipelineState();

                if (!string.IsNullOrEmpty(descriptor.LeaseName))
                {
                    var lease = await leaseService.TryAcquireNonBlocking(descriptor.LeaseName, cancellationToken);
                    if (lease is null)
                    {
                        Log.LeaseUnavailable(logger, descriptor.LeaseName);
                        continue;
                    }

                    state.Lease = lease;
                    Log.LeaseAcquired(logger, descriptor.LeaseName);
                }

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    Log.PipelineStarting(logger, group.GroupName, descriptor.Name ?? "(unnamed)");
                    var pipelineTasks = BuildPipeline([], descriptor, state, descriptor.Source, null, cts);
                    var jobTasks = pipelineTasks.Select(taskFactory => taskFactory()).ToList();

                    await Task.WhenAll(jobTasks);
                    Log.PipelineCompleted(logger, group.GroupName, descriptor.Name ?? "(unnamed)");
                }
                catch (InvalidOperationException ex)
                {
                    Log.FailedToBuildPipeline(logger, ex);
                }
                catch (OperationCanceledException)
                {
                    Log.PipelineCanceled(logger, group.GroupName, descriptor.Name ?? "(unnamed)");
                }
                catch (Exception ex)
                {
                    Log.PipelineFailed(logger, group.GroupName, descriptor.Name ?? "(unnamed)", ex);
                }
                finally
                {
                    cts.Cancel();
                    if (state.Lease is not null)
                    {
                        await state.Lease.DisposeAsync();
                        Log.LeaseReleased(logger, descriptor.LeaseName ?? "(none)");
                    }
                }
            }

            // If this group should not recur, exit the loop
            if (group.Recurring is null)
            {
                return;
            }

            Log.WaitingForNextRun(logger, group.GroupName, group.Recurring.Value);
            await Task.Delay(group.Recurring.Value, cancellationToken);
        }
    }

    private async Task<bool> IsPipelineEnabled(PipelineGroup group, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(group.FeatureFlag))
        {
            bool enabled = await featureManager.IsEnabledAsync(group.FeatureFlag, cancellationToken);
            return enabled;
        }

        return true;
    }

    public List<Func<Task>> BuildPipeline(
        List<Func<Task>> jobs,
        IPipelineDescriptor descriptor,
        PipelineState state,
        object? stage,
        object? inbound,
        CancellationTokenSource cts)
    {
        if (stage is PipelineSourceBuilder sourceBuilder)
        {
            var funcType = sourceBuilder.Func.GetType();
            var genericArgs = funcType.GetGenericArguments();
            var outboundType = genericArgs[0];

            var method = GetPipelineSourceRunMethod(sourceBuilder.Func, outboundType);
            var outbound = GetPipelineOutbound(outboundType);

            jobs.Add(() => (Task)method.Invoke(pipelineSourceJob, [descriptor, state, sourceBuilder.Func, outbound, cts])!);
            return BuildPipeline(jobs, descriptor, state, sourceBuilder.Segment, outbound, cts);
        }

        if (IsTypePipelineSegmentBuilder(stage))
        {
            var (func, segment, sink) = GetSegmentBuilderProperties(stage);

            if (segment is not null)
            {
                var funcType = func.GetType();
                var genericArgs = funcType.GetGenericArguments();
                var inboundType = genericArgs[0];
                var outboundType = genericArgs[1];

                var method = GetPipelineSegmentRunMethod(func);
                var outbound = GetPipelineOutbound(outboundType);

                jobs.Add(() => (Task)method.Invoke(pipelineSegmentJob, [descriptor, state, func, inbound, outbound])!);
                return BuildPipeline(jobs, descriptor, state, segment, outbound, cts);
            }
            else if (sink is not null)
            {
                var funcType = func.GetType();
                var genericArgs = funcType.GetGenericArguments();
                var inboundType = genericArgs[0];

                var method = GetPipelineSinkRunMethod(inboundType);
                jobs.Add(() => (Task)method.Invoke(pipelineSinkJob, [descriptor, state, inbound, func])!);
            }
            else
            {
                throw new InvalidOperationException("Pipeline builder has both Segment and Sink set to null.");
            }
        }

        return jobs;
    }

    private static MethodInfo GetPipelineSourceRunMethod(object func, Type type) =>
        typeof(PipelineSourceJob)
            .GetMethod(nameof(PipelineSourceJob.Run))!
            .MakeGenericMethod(type);

    private static MethodInfo GetPipelineSegmentRunMethod(object func)
    {
        var funcType = func.GetType();
        var genericArgs = funcType.GetGenericArguments();
        var inboundType = genericArgs[0];
        var outboundType = genericArgs[1];

        return typeof(PipelineSegmentJob)
            .GetMethod(nameof(PipelineSegmentJob.Run))!
            .MakeGenericMethod(inboundType, outboundType);
    }

    private static MethodInfo GetPipelineSinkRunMethod(Type inboundType) =>
        typeof(PipelineSinkJob)
            .GetMethod(nameof(PipelineSinkJob.Run))!
            .MakeGenericMethod(inboundType);

    private static object GetPipelineOutbound(Type type)
    {
        var messageType = typeof(PipelineSingleMessage<>).MakeGenericType(type);
        var collectionType = typeof(BlockingCollection<>).MakeGenericType(messageType);
        return Activator.CreateInstance(collectionType)!;
    }

    private static bool IsTypePipelineSegmentBuilder(object? currentStage) =>
        currentStage is not null &&
        currentStage.GetType().IsGenericType &&
        currentStage.GetType().GetGenericTypeDefinition() == typeof(PipelineSegmentBuilder<>);

    private static (object? Func, object? Segment, object? Sink) GetSegmentBuilderProperties(object stage)
    {
        var type = stage.GetType();
        var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        var funcProp = type.GetProperty(nameof(PipelineSegmentBuilder<object>.Func), flags);
        var segmentProp = type.GetProperty(nameof(PipelineSegmentBuilder<object>.Segment), flags);
        var sinkProp = type.GetProperty(nameof(PipelineSegmentBuilder<object>.Sink), flags);

        return (
            funcProp?.GetValue(stage),
            segmentProp?.GetValue(stage),
            sinkProp?.GetValue(stage)
        );
    }

    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Starting pipeline hosted service...")]
        internal static partial void HostedServiceStarting(ILogger logger);

        [LoggerMessage(1, LogLevel.Information, "No pipelines registered.")]
        internal static partial void NoPipelinesRegistered(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Pipeline group '{PipelineGroup}' registered with {PipelineCount} pipelines (recurring: {Recurring}).")]
        internal static partial void PipelineGroupRegistered(ILogger logger, string PipelineGroup, int PipelineCount, string Recurring);

        [LoggerMessage(3, LogLevel.Information, "Stopping pipeline hosted service...")]
        internal static partial void HostedServiceStopping(ILogger logger);

        [LoggerMessage(4, LogLevel.Information, "Lease is present.")]
        internal static partial void LeaseIsPresent(ILogger logger);

        [LoggerMessage(5, LogLevel.Error, "Failed to build pipeline.")]
        internal static partial void FailedToBuildPipeline(ILogger logger, Exception ex);

        [LoggerMessage(6, LogLevel.Error, "Pipeline '{PipelineName}' in group '{PipelineGroup}' failed.")]
        internal static partial void PipelineFailed(ILogger logger, string PipelineGroup, string PipelineName, Exception ex);

        [LoggerMessage(7, LogLevel.Warning, "Pipeline '{PipelineName}' in group '{PipelineGroup}' was canceled.")]
        internal static partial void PipelineCanceled(ILogger logger, string PipelineGroup, string PipelineName);

        [LoggerMessage(8, LogLevel.Information, "Pipeline '{PipelineName}' in group '{PipelineGroup}' completed successfully.")]
        internal static partial void PipelineCompleted(ILogger logger, string PipelineGroup, string PipelineName);

        [LoggerMessage(9, LogLevel.Information, "Pipeline '{PipelineName}' in group '{PipelineGroup}' starting...")]
        internal static partial void PipelineStarting(ILogger logger, string PipelineGroup, string PipelineName);

        [LoggerMessage(10, LogLevel.Warning, "Feature flag disabled pipeline group '{PipelineGroup}' (pipeline '{PipelineName}').")]
        internal static partial void PipelineDisabled(ILogger logger, string PipelineGroup, string PipelineName);

        [LoggerMessage(11, LogLevel.Warning, "Lease unavailable for '{LeaseName}', skipping run.")]
        internal static partial void LeaseUnavailable(ILogger logger, string LeaseName);

        [LoggerMessage(12, LogLevel.Information, "Lease '{LeaseName}' acquired.")]
        internal static partial void LeaseAcquired(ILogger logger, string LeaseName);

        [LoggerMessage(13, LogLevel.Information, "Lease '{LeaseName}' released.")]
        internal static partial void LeaseReleased(ILogger logger, string LeaseName);

        [LoggerMessage(14, LogLevel.Information, "Waiting {Delay} before next pipeline group '{PipelineGroup}' run...")]
        internal static partial void WaitingForNextRun(ILogger logger, string PipelineGroup, TimeSpan Delay);

        [LoggerMessage(15, LogLevel.Error, "Unhandled exception while processing pipeline group '{PipelineGroup}'.")]
        internal static partial void UnhandledPipelineGroupError(ILogger logger, string PipelineGroup, Exception ex);

        [LoggerMessage(16, LogLevel.Error, "Failed to stop running pipelines cleanly.")]
        internal static partial void FailedToStopPipelines(ILogger logger, Exception ex);
    }
}
