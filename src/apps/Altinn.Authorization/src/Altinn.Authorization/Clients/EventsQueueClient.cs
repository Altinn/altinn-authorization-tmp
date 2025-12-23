using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using Altinn.Platform.Authorization.Clients.Interfaces;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.EventLog;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace Altinn.Platform.Authorization.Clients
{
    /// <summary>
    /// Implementation of the <see ref="IEventsQueueClient"/> using Azure Storage Queues.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class EventsQueueClient : IEventsQueueClient
    {
        private static readonly JsonElement FallbackContextRequestJson = JsonSerializer.SerializeToElement(new
        {
            message = "ContextRequestJson is not available due to size limitations.",
            info = "See the following link for more details https://github.com/Altinn/altinn-authorization-tmp/issues/1858",
        });

        private readonly QueueStorageSettings _settings;

        private QueueClient _authenticationEventQueueClient;
        private readonly ILogger<EventsQueueClient> _logger;
        private static readonly RecyclableMemoryStreamManager _manager = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsQueueClient"/> class.
        /// </summary>
        /// <param name="settings">The queue storage settings</param>
        /// <param name="logger">the logger handler</param>
        public EventsQueueClient(
            IOptions<QueueStorageSettings> settings,
            ILogger<EventsQueueClient> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<QueuePostReceipt> EnqueueAuthorizationEvent(AuthorizationEvent authorizationEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                QueueClient client = await GetAuthorizationEventQueueClient();
                TimeSpan timeToLive = TimeSpan.FromDays(_settings.TimeToLive);

                using var stream = _manager.GetStream();
                CompressAuthorizationEvent(stream, authorizationEvent);

                if (stream.Length > 64 * 1024)
                {
                    Log.AuthorizationEventTooLarge(_logger, stream.Length);
                    authorizationEvent.ContextRequestJson = FallbackContextRequestJson;

                    stream.SetLength(0);
                    CompressAuthorizationEvent(stream, authorizationEvent);
                }

                var buffer = stream.GetBuffer();
                var data = BinaryData.FromBytes(buffer.AsMemory(checked((int)stream.Length)));
                
                await client.SendMessageAsync(data, null, timeToLive, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                Log.OperationCanceled(_logger, ex);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(_logger, ex);
                return new QueuePostReceipt { Success = false, Exception = ex };
            }

            return new QueuePostReceipt { Success = true };
        }

        private async Task<QueueClient> GetAuthorizationEventQueueClient()
        {
            if (_authenticationEventQueueClient == null)
            {
                _authenticationEventQueueClient = new QueueClient(_settings.EventLogConnectionString, _settings.AuthorizationEventQueueName);
                await _authenticationEventQueueClient.CreateIfNotExistsAsync();
            }

            return _authenticationEventQueueClient;
        }

        private void CompressAuthorizationEvent(Stream stream, AuthorizationEvent authorizationEvent)
        {
            stream.Write("01"u8 /* version header */);

            using (var compressor = new BrotliStream(stream, CompressionLevel.Fastest, leaveOpen: true))
            {
                JsonSerializer.Serialize(compressor, authorizationEvent, JsonSerializerOptions.Web);
            }

            stream.Position = 0;
        }

        private static partial class Log
        {
            [LoggerMessage(1, LogLevel.Warning, "Authorization event size {size} bytes exceeds maximum allowed size for queue messages.")]
            public static partial void AuthorizationEventTooLarge(ILogger logger, long size);

            [LoggerMessage(2, LogLevel.Warning, "Operation was canceled.")]
            public static partial void OperationCanceled(ILogger logger, Exception exception);

            [LoggerMessage(3, LogLevel.Error, "An error occurred while enqueuing authorization event.")]
            public static partial void Error(ILogger logger, Exception exception);
        }
    }
}
