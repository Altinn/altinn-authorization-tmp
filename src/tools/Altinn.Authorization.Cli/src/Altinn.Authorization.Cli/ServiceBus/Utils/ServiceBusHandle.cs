using Altinn.Authorization.Cli.ServiceBus.MassTransit;
using Altinn.Authorization.Cli.Utils;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;

namespace Altinn.Authorization.Cli.ServiceBus.Utils;

internal class ServiceBusHandle
{
    public static ServiceBusHandle Create(string connectionString)
    {
        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            && uri.Scheme == "sb")
        {
            // Connection string is a endpoint URI
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = false,
            };

            var query = uri.Query.Length > 1 ? HttpUtility.ParseQueryString(uri.Query.Substring(1)) : [];
            if (query.Get("tenantId") is { Length: > 0 } tenantId)
            {
                options.TenantId = tenantId;
            }

            var credential = new DefaultAzureCredential(options);
            var client = new ServiceBusClient(uri.Host, credential);
            var administrationClient = new ServiceBusAdministrationClient(uri.Host, credential);

            return new ServiceBusHandle(client, administrationClient);
        }

        return new ServiceBusHandle(new(connectionString), new(connectionString));
    }

    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _administrationClient;

    private ServiceBusHandle(ServiceBusClient client, ServiceBusAdministrationClient administrationClient)
    {
        _client = client;
        _administrationClient = administrationClient;
    }

    public ServiceBusClient Client => _client;

    public ServiceBusAdministrationClient AdministrationClient => _administrationClient;

    public async Task<IReadOnlyList<QueueModel>> GetQueueModels(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, QueueModel>();

        await foreach (var queue in AdministrationClient.GetQueuesRuntimePropertiesAsync(cancellationToken))
        {
            var queueName = queue.Name;
            var isErrorQueue = queueName.EndsWith("_error", StringComparison.OrdinalIgnoreCase);

            if (isErrorQueue)
            {
                queueName = queueName[..^6];
            }

            ref var queueModel = ref CollectionsMarshal.GetValueRefOrAddDefault(result, queueName, out var exists);
            if (!exists)
            {
                queueModel = new QueueModel(queueName);
            }

            if (isErrorQueue)
            {
                queueModel.ErrorQueueRuntimeProperties = queue;
            }
            else
            {
                queueModel.QueueRuntimeProperties = queue;
            }
        }

        return [.. result.Values];
    }

    public async Task RefreshStats(QueueModel queue, CancellationToken cancellationToken)
    {
        try
        {
            queue.QueueRuntimeProperties = await AdministrationClient.GetQueueRuntimePropertiesAsync(queue.Name, cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            // Ignore not found
        }

        try
        {
            queue.ErrorQueueRuntimeProperties = await AdministrationClient.GetQueueRuntimePropertiesAsync(queue.ErrorQueueName, cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            // Ignore not found
        }
    }

    public async Task MoveErrorsToErrorDLQ(QueueModel queue, ProgressContext context, CancellationToken cancellationToken)
    {
        if (queue.QueueRuntimeProperties is { DeadLetterMessageCount: > 0 })
        {
            var task = context.AddTask($"Move {queue.Name}/$DLQ to error", autoStart: false);

            await using var sender = Client.CreateSender(queue.ErrorQueueName);
            await ProcessDeadLetterMessages(
                queue.Name,
                task,
                async (msg, ct) => 
                {
                    var toSend = new ServiceBusMessage(msg);
                    await sender.SendMessageAsync(toSend, ct);
                    return true;
                },
                cancellationToken);

            await RefreshStats(queue, cancellationToken);
        }

        if (queue.ErrorQueueRuntimeProperties is { ActiveMessageCount: > 0 })
        {
            var task = context.AddTask($"Move {queue.Name}_error to $DLQ", autoStart: false);
            await DeadLetterMessages(queue.ErrorQueueName, task, cancellationToken);
        }
    }

    public async Task DeadLetterMessages(string queueName, ProgressTask task, CancellationToken cancellationToken)
    {
        var queueStats = await AdministrationClient.GetQueueRuntimePropertiesAsync(queueName, cancellationToken);

        if (queueStats is null || queueStats.Value is null)
        {
            throw new InvalidOperationException($"Queue {queueName} not found");
        }

        var expected = (int)queueStats.Value.ActiveMessageCount;
        task.MaxValue = expected;
        using var taskDisposable = task.Run();

        if (expected == 0)
        {
            task.MaxValue = 1;
            task.Value = 1;
            return;
        }

        await using var receiver = Client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = Math.Clamp((int)expected, 100, 10_000),
            SubQueue = SubQueue.None,
        });

        await ProcessMessages(
            receiver,
            async (msg, cancellationToken) =>
            {
                await receiver.DeadLetterMessageAsync(msg, cancellationToken: cancellationToken);
                task.Increment(1);
            },
            cancellationToken);
    }

    public async Task ProcessDeadLetterMessages(string queueName, ProgressTask task, Func<ServiceBusReceivedMessage, CancellationToken, ValueTask<bool>> process, CancellationToken cancellationToken)
    {
        var queueStats = await AdministrationClient.GetQueueRuntimePropertiesAsync(queueName, cancellationToken);

        if (queueStats is null || queueStats.Value is null)
        {
            throw new InvalidOperationException($"Queue {queueName} not found");
        }

        var expected = (int)queueStats.Value.DeadLetterMessageCount;
        task.MaxValue = expected;
        using var taskDisposable = task.Run();

        if (expected == 0)
        {
            task.MaxValue = 1;
            task.Value = 1;
            return;
        }

        await using var receiver = Client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = Math.Clamp((int)expected, 100, 10_000),
            SubQueue = SubQueue.DeadLetter,
        });

        await ProcessMessages(
            receiver,
            async (msg, cancellationToken) =>
            {
                var completed = await process(msg, cancellationToken);
                task.Increment(1);

                if (completed)
                {
                    await receiver.CompleteMessageAsync(msg, cancellationToken: cancellationToken);
                }
                else
                {
                    task.State.Update<double>("task.skipped", static s => s + 1);
                }
            },
            cancellationToken);
    }

    public async IAsyncEnumerable<ServiceBusReceivedMessage> PeekDeadLetterMessages(string queueName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Azure.Response<QueueRuntimeProperties> queueStats;
        try
        {
            queueStats = await AdministrationClient.GetQueueRuntimePropertiesAsync(queueName, cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            // Ignore not found
            yield break;
        }

        if (queueStats is null || queueStats.Value is null)
        {
            throw new InvalidOperationException($"Queue {queueName} not found");
        }

        var expected = (int)queueStats.Value.DeadLetterMessageCount;
        var prefetchCount = Math.Clamp(expected, 100, 10_000);
        await using var receiver = Client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = prefetchCount,
            SubQueue = SubQueue.DeadLetter,
        });

        long from = 0;
        while (true)
        {
            var messages = await receiver.PeekMessagesAsync(maxMessages: prefetchCount, fromSequenceNumber: from, cancellationToken: cancellationToken);
            if (messages.Count == 0)
            {
                break;
            }

            foreach (var message in messages)
            {
                yield return message;
            }

            from = messages[^1].SequenceNumber + 1;
        }
    }

    private async Task ProcessMessages(ServiceBusReceiver receiver, Func<ServiceBusReceivedMessage, CancellationToken, ValueTask> process, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationToken = cts.Token;

        var channel = Channel.CreateBounded<ServiceBusReceivedMessage>(100);

        var workersTask = Parallel.ForEachAsync(
            channel.Reader.ReadAllAsync(cancellationToken),
            cancellationToken,
            process);

        var readerTask = Task.Run(
            async () =>
            {
                try
                {
                    var unqueued = new Queue<ServiceBusReceivedMessage>(receiver.PrefetchCount);
                    while (true)
                    {
                        if (unqueued.Count == 0)
                        {
                            var newMessages = await receiver.ReceiveMessagesAsync(receiver.PrefetchCount, maxWaitTime: TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);
                            if (newMessages.Count == 0)
                            {
                                break;
                            }

                            foreach (var message in newMessages)
                            {
                                unqueued.Enqueue(message);
                            }
                        }

                        await WriteAll(receiver, unqueued, channel.Writer, cancellationToken);
                    }
                }
                finally
                {
                    channel.Writer.Complete();
                }
            },
            cancellationToken);

        try
        {
            await Task.WhenAll(workersTask, readerTask);
        }
        catch
        {
            await cts.CancelAsync();
            throw;
        }

        static async Task WriteAll(ServiceBusReceiver receiver, Queue<ServiceBusReceivedMessage> queue, ChannelWriter<ServiceBusReceivedMessage> writer, CancellationToken cancellationToken)
        {
            while (queue.TryDequeue(out var msg))
            {
                var lockExpiry = msg.LockedUntil;
                var renewAt = lockExpiry - TimeSpan.FromMinutes(1);
                var duration = renewAt - DateTimeOffset.UtcNow;

                if (duration <= TimeSpan.Zero)
                {
                    await RenewLocks(receiver, queue, cancellationToken);
                }

                await writer.WriteAsync(msg, cancellationToken);
            }
        }

        static Task RenewLocks(ServiceBusReceiver receiver, Queue<ServiceBusReceivedMessage> messages, CancellationToken cancellationToken)
        {
            return Parallel.ForEachAsync(messages, cancellationToken, (msg, ct) => new(receiver.RenewMessageLockAsync(msg, ct)));
        }
    }
}
