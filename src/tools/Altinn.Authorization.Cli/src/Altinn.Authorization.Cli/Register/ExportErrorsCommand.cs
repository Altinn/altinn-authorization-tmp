using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Altinn.Authorization.Cli.Register.Messages;
using Altinn.Authorization.Cli.ServiceBus.MassTransit;
using Altinn.Authorization.Cli.ServiceBus.Utils;
using Altinn.Authorization.Cli.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Altinn.Authorization.Cli.Register;

/// <summary>
/// Command for exporting errors from the service bus.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ExportErrorsCommand(CancellationToken ct)
    : BaseCommand<ExportErrorsCommand.Settings>(ct)
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorMessage>();

        var sb = ServiceBusHandle.Create(settings.ServiceBus);
        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SkippedColumn(),
                new ElapsedTimeColumn(),
            ])
            .StartAsync(async (ctx) =>
            {
                IReadOnlyList<QueueModel> queues = null!;
                {
                    using var task = ctx.AddTask("Getting queues", autoStart: false, maxValue: 0).Run(setValueMax: true);
                    queues = await sb.GetQueueModels(cancellationToken)
                        .LogOnFailure("Failed to get queues");
                }

                foreach (var queue in queues)
                {
                    await sb.MoveErrorsToErrorDLQ(queue, ctx, cancellationToken)
                        .LogOnFailure($"Failed to move errors from queue {queue.Name} to error DLQ");

                    var task = ctx.AddTask($"Exporting errors from {queue.Name}", autoStart: false, maxValue: queue.ErrorCount);
                    using var taskProgress = task.Run(setValueMax: true);
                    await ProcessErrors(sb, queue, task, errors, cancellationToken);
                }
            });

        foreach (var errorQueue in errors.GroupBy(e => e.Queue))
        {
            await using var fs = File.CreateText($"C:\\temp\\errors-{errorQueue.Key}.csv");
            await fs.WriteLineAsync("PartyUuid;Message");

            foreach (var error in errorQueue.OrderBy(m => m.Message))
            {
                var msg = error.Message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace(";", string.Empty);
                await fs.WriteLineAsync($"{error.PartyUuid};{msg}");
            }
        }

        return 0;
    }

    private static async Task ProcessErrors(ServiceBusHandle sb, QueueModel queue, ProgressTask task, List<ErrorMessage> errors, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<ErrorMessage>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
        });

        var reader = channel.Reader;
        var writer = channel.Writer;

        var readerTask = Task.Run(
            async () =>
            {
                await foreach (var error in reader.ReadAllAsync(cancellationToken))
                {
                    errors.Add(error);
                    task.Increment(1);
                }
            }, 
            cancellationToken);

        var writerTask = Parallel.ForEachAsync(
            sb.PeekDeadLetterMessages(queue.ErrorQueueName, cancellationToken),
            new ParallelOptions { CancellationToken = cancellationToken },
            async (msg, ct) =>
            {
                if (msg.ContentType != "application/vnd.masstransit+json")
                {
                    throw new InvalidOperationException($"Invalid content type: {msg.ContentType}");
                }

                var errorMessage = msg.ApplicationProperties.TryGetValue("MT-Fault-Message", out var faultMessage)
                    ? faultMessage.ToString()
                    : string.Empty;

                var partyUuid = GetPartyUuid(msg.Body.ToMemory().Span);

                await writer.WriteAsync(
                    new ErrorMessage
                    {
                        Queue = queue.Name,
                        PartyUuid = partyUuid,
                        Message = errorMessage,
                    },
                    cancellationToken);
            })
            .ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    writer.Complete(task.Exception!);
                }
                else
                {
                    writer.Complete();
                }
            });

        await Task.WhenAll(readerTask, writerTask);
    }

    private static Guid GetPartyUuid(ReadOnlySpan<byte> msgData)
    {
        var reader = new Utf8JsonReader(msgData);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new InvalidOperationException("Invalid JSON data");
        }

        if (FindMessageAndType(ref reader, out var messageTypeReader, out var messageReader))
        {
            while (messageTypeReader.Read() && messageTypeReader.TokenType == JsonTokenType.String)
            {
                foreach (var resolver in GetPartyUuidResolvers)
                {
                    if (messageTypeReader.ValueTextEquals(resolver.MessageUrn))
                    {
                        if (resolver.TryRead(in messageReader, out var result))
                        {
                            return result;
                        }
                    }
                }
            }
        }

        return Guid.Empty;

        static bool FindMessageAndType(ref Utf8JsonReader reader, out Utf8JsonReader messageType, out Utf8JsonReader messageReader)
        {
            messageType = default;
            messageReader = default;

            bool hasMessageType = false, hasMessage = false;

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    return false;
                }

                if (reader.ValueTextEquals("messageType"u8))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        return false;
                    }

                    messageType = reader;
                    hasMessageType = true;
                    reader.Skip();
                }
                else if (reader.ValueTextEquals("message"u8))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        return false;
                    }

                    messageReader = reader;
                    hasMessage = true;
                    reader.Skip();
                }
                else
                {
                    reader.Skip();
                    continue;
                }

                if (hasMessage && hasMessageType)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private sealed record ErrorMessage
    {
        public required Guid PartyUuid { get; init; }

        public required string Message { get; init; }

        public required string Queue { get; init; }
    }

    private abstract class Handler<T>
    {
        public abstract Utf8String MessageUrn { get; }

        public abstract bool TryRead(in Utf8JsonReader reader, [NotNullWhen(true)] out T? result);
    }

    private abstract class MessageHandler<TMessage, TResult>
        : Handler<TResult>
        where TMessage : IFakeMassTransitMessage<TMessage>
    {
        public override Utf8String MessageUrn => TMessage.MessageUrn;

        protected abstract bool TryGet(TMessage message, [NotNullWhen(true)] out TResult? result);

        public override sealed bool TryRead(in Utf8JsonReader reader, [NotNullWhen(true)] out TResult? result)
        {
            var readerCopy = reader;
            var message = JsonSerializer.Deserialize<TMessage>(ref readerCopy, JsonSerializerOptions.Web);

            return TryGet(message, out result);
        }
    }

    private sealed class DelegateMessageHandler<TMessage, TResult>(Func<TMessage, TResult> getResult)
        : MessageHandler<TMessage, TResult>
        where TMessage : IFakeMassTransitMessage<TMessage>
    {
        protected override bool TryGet(TMessage message, [NotNullWhen(true)] out TResult? result)
        {
            result = getResult(message);
            return true;
        }
    }

    private static readonly ImmutableArray<Handler<Guid>> GetPartyUuidResolvers = [
        new DelegateMessageHandler<ImportA2CCRRolesCommand, Guid>(m => m.PartyUuid),
        new DelegateMessageHandler<ImportA2PartyCommand, Guid>(m => m.PartyUuid),
        new DelegateMessageHandler<ResolveAndUpsertA2CCRRoleAssignmentsCommand, Guid>(m => m.FromPartyUuid),
        new DelegateMessageHandler<UpsertExternalRoleAssignmentsCommand, Guid>(m => m.FromPartyUuid),
        new DelegateMessageHandler<UpsertPartyComand, Guid>(m => m.Party.PartyUuid),
        new DelegateMessageHandler<UpsertValidatedPartyCommand, Guid>(m => m.Party.PartyUuid),
    ];

    /// <summary>
    /// Settings for the export errors command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Settings
        : BaseCommandSettings
    {
        /// <summary>
        /// Gets the connection string to the source database.
        /// </summary>
        [Description("The connection string to the service-bus.")]
        [CommandArgument(0, "<CONNECTION_STRING>")]
        [ExpandEnvironmentVariables]
        public string? ServiceBus { get; init; }

        ////[Description("The connection string to the database to insert errors into.")]
        ////[CommandArgument(1, "<CONNECTION_STRING>")]
        ////[ExpandEnvironmentVariables]
        ////public string? Database { get; init; }
    }
}
