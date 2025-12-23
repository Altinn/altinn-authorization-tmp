using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Clients;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Models.EventLog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Altinn.Authorization.Tests
{
    public class EventsQueueClientTests
    {
        private EventsQueueClient CreateClient()
        {
            var settings = Options.Create(new QueueStorageSettings
            {
                EventLogConnectionString = "UseDevelopmentStorage=true",
                AuthorizationEventQueueName = "testqueue",
                TimeToLive = 1
            });
            var logger = Mock.Of<ILogger<EventsQueueClient>>();
            return new EventsQueueClient(settings, logger);
        }

        private byte[] InvokeCompress(EventsQueueClient client, AuthorizationEvent evt)
        {
            using var stream = new MemoryStream();
            var method = typeof(EventsQueueClient).GetMethod("CompressAuthorizationEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(client, new object[] { stream, evt });
            return stream.ToArray();
        }

        [Fact]
        public void CompressAuthorizationEvent_SmallJson_CompressesAndRetainsData()
        {
            // Arrange
            var client = CreateClient();
            var evt = new AuthorizationEvent
            {
                Operation = "read",
                ContextRequestJson = JsonSerializer.Deserialize<JsonElement>("{\"foo\":\"bar\"}")
            };

            // Act
            var result = InvokeCompress(client, evt);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 2);

            // Decompress and verify
            using var ms = new MemoryStream(result, 2, result.Length - 2);
            using var brotli = new BrotliStream(ms, CompressionMode.Decompress);
            var decompressed = JsonSerializer.Deserialize<AuthorizationEvent>(brotli, JsonSerializerOptions.Web);
            Assert.Equal(evt.Operation, decompressed.Operation);
            Assert.Equal(evt.ContextRequestJson.GetRawText(), decompressed.ContextRequestJson.GetRawText());
        }

        [Fact]
        public void CompressAuthorizationEvent_LargeJson_CompressesAndHandlesFallback()
        {
            // Arrange
            const int LENGTH = 64 * 1024;
            var values = new List<int>(LENGTH);
            var random = new Random();
            for (var i = 0; i < LENGTH; i++)
            {
                values.Add(random.Next());
            }

            var largeJson = JsonSerializer.Serialize(values);
            var client = CreateClient();
            var evt = new AuthorizationEvent
            {
                Operation = "write",
                ContextRequestJson = JsonSerializer.Deserialize<JsonElement>(largeJson)
            };

            // Act
            // Get the compressed data
            var result = InvokeCompress(client, evt);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 2, "Compressed data should be longer than version header.");
            Console.WriteLine(BitConverter.ToString(result.Take(10).ToArray()));
            Assert.True(result.Length >= 64 * 1024); // Should be within size limit after fallback

            // Decompress and verify fallback, Skip the first two bytes (version header) when decompressing
            using var ms = new MemoryStream(result, 2, result.Length - 2);
            using var brotli = new BrotliStream(ms, CompressionMode.Decompress);
            var decompressed = JsonSerializer.Deserialize<AuthorizationEvent>(brotli, JsonSerializerOptions.Web);
            Assert.Equal(evt.Operation, decompressed.Operation);
            Assert.Contains(
                largeJson.ToString(),
                decompressed.ContextRequestJson.ToString()
            );
        }
    }
}
