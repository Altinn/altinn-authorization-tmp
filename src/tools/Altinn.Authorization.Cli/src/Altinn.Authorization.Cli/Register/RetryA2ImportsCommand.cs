using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Channels;
using Altinn.Authorization.Cli.Database;
using Altinn.Authorization.Cli.Register.Messages;
using Altinn.Authorization.Cli.ServiceBus.MassTransit;
using Altinn.Authorization.Cli.ServiceBus.Utils;
using Altinn.Authorization.Cli.Utils;
using Azure.Messaging.ServiceBus;
using Npgsql;
using NpgsqlTypes;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.Cli.Register;

/// <summary>
/// Command for retrying failed messages.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RetryA2ImportsCommand(CancellationToken ct)
    : BaseCommand<RetryA2ImportsCommand.Settings>(ct)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    // Note: order of queues is important
    private static readonly ImmutableArray<ErrorQueueHandler> ErrorQueues = [
        new A2ImportErrorQueueHandler(),
        new ImportValidationQueueHandler(),
        new ImportBatchErrorQueueHandler(),
        new A2ExternalRoleResolverErrorQueueHandler(),
    ];

    /// <inheritdoc/>
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Database))
        {
            AnsiConsole.MarkupLine("[red]Database connection string is required.[/]");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceBus))
        {
            AnsiConsole.MarkupLine("[red]Service bus connection string is required.[/]");
            return 1;
        }

        await using var db = await DbHelper.Create(settings.Database, cancellationToken);
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
                HashSet<string> queueNames;
                {
                    using var task = ctx.AddTask("Getting queues", autoStart: false).Run(setValueMax: true);
                    queueNames = await sb.AdministrationClient.GetQueuesRuntimePropertiesAsync(cancellationToken)
                        .Select(static queue => queue.Name)
                        .ToHashSetAsync(cancellationToken)
                        .LogOnFailure("Failed to get queues");
                }

                await using var sender = sb.Client.CreateSender("register-a2-party-import");
                var context = new Context(sender, db);

                foreach (var queue in ErrorQueues)
                {
                    if (queueNames.Contains(queue.Name) || queueNames.Contains(queue.ErrorQueueName))
                    {
                        await Process(sb, context, queue, queueNames, ctx, cancellationToken);
                    }
                }
            });

        return 0;
    }

    private async Task Process(
        ServiceBusHandle sb,
        Context context,
        ErrorQueueHandler queue,
        HashSet<string> queueNames,
        ProgressContext ctx,
        CancellationToken cancellationToken)
    {
        if (queueNames.Contains(queue.Name))
        {
            await RetryDeadLetters(sb, context, queue, queue.Name, ctx, cancellationToken);
        }

        if (queueNames.Contains(queue.ErrorQueueName))
        {
            await DeadLetterMessages(sb, queue, queue.ErrorQueueName, ctx, cancellationToken);
            await RetryDeadLetters(sb, context, queue, queue.ErrorQueueName, ctx, cancellationToken);
        }
    }

    private async Task DeadLetterMessages(
        ServiceBusHandle sb,
        ErrorQueueHandler queue,
        string queueName,
        ProgressContext ctx,
        CancellationToken cancellationToken)
    {
        var task = ctx.AddTask($"Dead-letter {queueName} messages", autoStart: true, maxValue: 0);

        await sb.DeadLetterMessages(queueName, task, cancellationToken);
    }

    private async Task RetryDeadLetters(
        ServiceBusHandle sb,
        Context context,
        ErrorQueueHandler queue,
        string queueName,
        ProgressContext ctx,
        CancellationToken cancellationToken)
    {
        var task = ctx.AddTask($"Retry {queueName} messages", autoStart: true, maxValue: 0);

        await sb.ProcessDeadLetterMessages(
            queueName,
            task,
            async (msg, cancellationToken) =>
            {
                if (queue.GetHandler(msg, out var handler)
                    && await handler.Handle(msg, context, cancellationToken))
                {
                    return true;
                }

                return false;
            },
            cancellationToken);
    }

    private abstract class ErrorQueueMessageHandler(Utf8String messageUrn)
    {
        public Utf8String MessageUrn { get; } = messageUrn;

        public abstract ValueTask<bool> Handle(ServiceBusReceivedMessage message, Context sender, CancellationToken cancellationToken);
    }

    private abstract class ErrorQueueMessageHandler<T>()
        : ErrorQueueMessageHandler(T.MessageUrn)
        where T : class, IFakeMassTransitMessage<T>
    {
        protected abstract ValueTask<bool> Handle(T message, Context sender, CancellationToken cancellationToken);

        public sealed override ValueTask<bool> Handle(ServiceBusReceivedMessage message, Context sender, CancellationToken cancellationToken)
        {
            // we've already validated the message urn, so we can skip that
            var reader = new Utf8JsonReader(message.Body.ToMemory().Span);
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                return ValueTask.FromResult(false);
            }

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    return ValueTask.FromResult(false);
                }

                if (!reader.ValueTextEquals("message"u8))
                {
                    reader.Skip();
                    continue;
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                {
                    return ValueTask.FromResult(false);
                }

                break;
            }

            // found message object
            T? body = null;
            try
            {
                body = JsonSerializer.Deserialize<T>(ref reader, Options);
            }
            catch (JsonException)
            {
                return ValueTask.FromResult(false);
            }

            if (body is null)
            {
                return ValueTask.FromResult(false);
            }

            return Handle(body, sender, cancellationToken);
        }
    }

    private abstract class ErrorQueueHandler(
        string queueName,
        ImmutableArray<ErrorQueueMessageHandler> handlers)
    {
        private readonly ImmutableArray<ErrorQueueMessageHandler> _handlers = handlers;

        public string Name { get; } = queueName;

        public string ErrorQueueName { get; } = $"{queueName}_error";

        public bool Handles(ServiceBusReceivedMessage message)
            => GetHandler(message, out _);

        public virtual bool GetHandler(ServiceBusReceivedMessage message, [NotNullWhen(true)] out ErrorQueueMessageHandler? handler)
        {
            handler = null;

            if (message.ContentType != "application/vnd.masstransit+json")
            {
                return false;
            }

            var reader = new Utf8JsonReader(message.Body.ToMemory().Span);
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                return false;
            }

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    return false;
                }

                if (!reader.ValueTextEquals("messageType"u8))
                {
                    reader.Skip();
                    continue;
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
                {
                    return false;
                }

                break;
            }

            // found messageType array
            while (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                foreach (var h in _handlers)
                {
                    if (reader.ValueTextEquals(h.MessageUrn))
                    {
                        handler = h;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    private sealed class A2ExternalRoleResolverErrorQueueHandler()
        : ErrorQueueHandler(
            "register-a2-external-role-resolver",
            [
                new ResolveAndUpsertA2CCRRoleAssignmentsCommandHandler(),
            ])
    {
        private sealed class ResolveAndUpsertA2CCRRoleAssignmentsCommandHandler
            : ErrorQueueMessageHandler<ResolveAndUpsertA2CCRRoleAssignmentsCommand>
        {
            protected override async ValueTask<bool> Handle(ResolveAndUpsertA2CCRRoleAssignmentsCommand message, Context sender, CancellationToken cancellationToken)
            {
                var partyId = message.FromPartyId;
                if (partyId == 0)
                {
                    partyId = await sender.GetPartyIdFromUuid(message.FromPartyUuid, cancellationToken);
                }

                var command = new ImportA2CCRRolesCommand
                {
                    PartyId = partyId,
                    PartyUuid = message.FromPartyUuid,
                    ChangeId = message.Tracking.Progress,
                    ChangedTime = DateTimeOffset.UtcNow,
                };

                await sender.Send(command, cancellationToken);
                return true;
            }
        }
    }

    private sealed class ImportBatchErrorQueueHandler()
        : ErrorQueueHandler(
            "register-party-import-batch",
            [
                new UpsertValidatedPartyCommandHandler(),
                new UpsertExternalRoleAssignmentsCommandHandler(),
            ])
    {
        private sealed class UpsertValidatedPartyCommandHandler
            : ErrorQueueMessageHandler<UpsertValidatedPartyCommand>
        {
            protected override ValueTask<bool> Handle(UpsertValidatedPartyCommand message, Context sender, CancellationToken cancellationToken)
            {
                // we only handle imports from A2
                if (message.Tracking.JobName != "a2-party-import:party")
                {
                    return ValueTask.FromResult(false);
                }

                var command = new ImportA2PartyCommand
                {
                    PartyUuid = message.Party.PartyUuid,
                    ChangeId = message.Tracking.Progress,
                    ChangedTime = DateTimeOffset.UtcNow,
                };

                return SendAndReturn(command, sender, cancellationToken);
            }

            private async ValueTask<bool> SendAndReturn(ImportA2PartyCommand command, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(command, cancellationToken);
                return true;
            }
        }

        private sealed class UpsertExternalRoleAssignmentsCommandHandler
            : ErrorQueueMessageHandler<UpsertExternalRoleAssignmentsCommand>
        {
            protected override ValueTask<bool> Handle(UpsertExternalRoleAssignmentsCommand message, Context sender, CancellationToken cancellationToken)
            {
                // we only handle imports from A2
                if (message.Tracking.JobName != "a2-party-import:ccr-roles")
                {
                    return ValueTask.FromResult(false);
                }

                var command = new ImportA2CCRRolesCommand
                {
                    PartyId = message.FromPartyId,
                    PartyUuid = message.FromPartyUuid,
                    ChangeId = message.Tracking.Progress,
                    ChangedTime = DateTimeOffset.UtcNow,
                };

                return SendAndReturn(command, sender, cancellationToken);
            }

            private async ValueTask<bool> SendAndReturn(ImportA2CCRRolesCommand command, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(command, cancellationToken);
                return true;
            }
        }
    }

    private sealed class ImportValidationQueueHandler()
        : ErrorQueueHandler(
            "register-party-import-validation",
            [
                new UpsertPartyCommandHandler()
            ])
    {
        private sealed class UpsertPartyCommandHandler
            : ErrorQueueMessageHandler<UpsertPartyComand>
        {
            protected override ValueTask<bool> Handle(UpsertPartyComand message, Context sender, CancellationToken cancellationToken)
            {
                // we only handle imports from A2
                if (message.Tracking.JobName != "a2-party-import:party")
                {
                    return ValueTask.FromResult(false);
                }

                var command = new ImportA2PartyCommand
                {
                    PartyUuid = message.Party.PartyUuid,
                    ChangeId = message.Tracking.Progress,
                    ChangedTime = DateTimeOffset.UtcNow,
                };

                return SendAndReturn(command, sender, cancellationToken);
            }

            private async ValueTask<bool> SendAndReturn(ImportA2PartyCommand command, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(command, cancellationToken);
                return true;
            }
        }
    }

    private sealed class A2ImportErrorQueueHandler()
        : ErrorQueueHandler(
            "register-a2-party-import",
            [
                new ImportA2PartyCommandHandler(),
                new ImportA2CCRRolesCommandHandler(),
            ])
    {
        public override bool GetHandler(ServiceBusReceivedMessage message, [NotNullWhen(true)] out ErrorQueueMessageHandler? handler)
        {
            // TODO: remove after cleaning junk
            if (string.IsNullOrEmpty(message.ContentType))
            {
                handler = JunkHandler.Instance;
                return true;
            }

            return base.GetHandler(message, out handler);
        }

        private sealed class ImportA2PartyCommandHandler
            : ErrorQueueMessageHandler<ImportA2PartyCommand>
        {
            protected override async ValueTask<bool> Handle(ImportA2PartyCommand message, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(message, cancellationToken);
                return true;
            }
        }

        private sealed class ImportA2CCRRolesCommandHandler
            : ErrorQueueMessageHandler<ImportA2CCRRolesCommand>
        {
            protected override async ValueTask<bool> Handle(ImportA2CCRRolesCommand message, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(message, cancellationToken);
                return true;
            }
        }

        private sealed class JunkHandler()
            : ErrorQueueMessageHandler("junk"u8)
        {
            public static JunkHandler Instance { get; } = new();

            public override ValueTask<bool> Handle(ServiceBusReceivedMessage message, Context sender, CancellationToken cancellationToken)
            {
                try
                {
                    var command = message.Body.ToObjectFromJson<ImportA2PartyCommand>(Options);
                    if (command is null)
                    {
                        return ValueTask.FromResult(false);
                    }

                    return SendAndReturn(command, sender, cancellationToken);
                }
                catch (JsonException)
                {
                    return ValueTask.FromResult(false);
                }
            }

            private async ValueTask<bool> SendAndReturn(ImportA2PartyCommand command, Context sender, CancellationToken cancellationToken)
            {
                await sender.Send(command, cancellationToken);
                return true;
            }
        }
    }

    private sealed class Context(ServiceBusSender sender, DbHelper db)
    {
        public async Task Send(ImportA2PartyCommand command, CancellationToken cancellationToken)
        {
            var data = new BinaryData(FakeMassTransitEnvelope.Create(command), Options, typeof(FakeMassTransitEnvelope<ImportA2PartyCommand>));
            var message = new ServiceBusMessage(data);
            message.ContentType = "application/vnd.masstransit+json";
            await sender.SendMessageAsync(message, cancellationToken);
        }

        public async Task Send(ImportA2CCRRolesCommand command, CancellationToken cancellationToken)
        {
            var data = new BinaryData(FakeMassTransitEnvelope.Create(command), Options, typeof(FakeMassTransitEnvelope<ImportA2CCRRolesCommand>));
            var message = new ServiceBusMessage(data);
            message.ContentType = "application/vnd.masstransit+json";
            await sender.SendMessageAsync(message, cancellationToken);
        }

        public async Task<int> GetPartyIdFromUuid(Guid partyUuid, CancellationToken cancellationToken)
        {
            const string QUERY =
                /*strpsql*/"""
                SELECT p.id
                FROM register.party p
                WHERE p.uuid = @partyUuid
                """;

            await using var conn = await db.Source.OpenConnectionAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = QUERY;

            var parameter = new NpgsqlParameter<Guid>("partyUuid", NpgsqlDbType.Uuid);
            parameter.TypedValue = partyUuid;
            cmd.Parameters.Add(parameter);

            await cmd.PrepareAsync(cancellationToken);
            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("Party not found");
            }

            return reader.GetInt32(0);
        }
    }

    /// <summary>
    /// Settings for the retry command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Settings
        : BaseCommandSettings
    {
        /// <summary>
        /// Gets the connection string to the service-bus.
        /// </summary>
        [Description("The connection string to the service-bus.")]
        [CommandArgument(0, "<CONNECTION_STRING>")]
        [ExpandEnvironmentVariables]
        public string? ServiceBus { get; init; }

        /// <summary>
        /// Gets the connection string to the database.
        /// </summary>
        [Description("The connection string to the database.")]
        [CommandArgument(1, "<CONNECTION_STRING>")]
        [ExpandEnvironmentVariables]
        public string? Database { get; init; }
    }
}
