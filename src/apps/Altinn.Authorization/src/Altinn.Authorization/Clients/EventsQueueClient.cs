using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Clients.Interfaces;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.EventLog;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace Altinn.Platform.Authorization.Clients
{
    /// <summary>
    /// Implementation of the <see ref="IEventsQueueClient"/> using Azure Storage Queues.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EventsQueueClient : IEventsQueueClient
    {
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

                byte[] data = CompressAuthorizationEvent(authorizationEvent, cancellationToken);

                if (data.Length > 64 * 1024)
                {
                    var fallbackJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = "ContextRequestJson is not available due to size limitations.",
                        info = "See the following link for more details https://github.com/Altinn/altinn-authorization-tmp/issues/1858",
                    });

                    authorizationEvent.ContextRequestJson = fallbackJson;
                }

                data = CompressAuthorizationEvent(authorizationEvent, cancellationToken);
                await client.SendMessageAsync(BinaryData.FromBytes(data), null, timeToLive, cancellationToken);      
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
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

        private byte[] CompressAuthorizationEvent(AuthorizationEvent authorizationEvent, CancellationToken cancellationToken)
        {
            using var stream = _manager.GetStream();
            stream.Write("01"u8 /* version header */);
            {
                using var compressor = new BrotliStream(stream, CompressionLevel.Fastest, leaveOpen: true);
                JsonSerializer.Serialize(compressor, authorizationEvent, JsonSerializerOptions.Web);
            }

            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            return data;
        }
    }
}
