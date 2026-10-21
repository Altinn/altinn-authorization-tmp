using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Altinn.Platform.Authentication.Clients;
using Altinn.Platform.Authentication.Configuration;
using Altinn.Platform.Authentication.Model;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.Platform.Authentication.Tests.Clients
{
    public class EventsQueueClientTests
    {
        // Azurite's published development account, pointed at a loopback port nothing listens on.
        private const string UnreachableQueue =
            "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;" +
            "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
            "QueueEndpoint=http://127.0.0.1:1/devstoreaccount1";

        [Fact]
        public async Task EnqueueAuthenticationEvent_QueueUnreachable_FailsFastAndRetriesOnNextEvent()
        {
            // Arrange
            EventsQueueClient client = new(Options.Create(new QueueStorageSettings
            {
                EventLogConnectionString = UnreachableQueue,
                AuthenticationEventQueueName = "authenticationeventqueue",
            }));
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Act
            QueuePostReceipt first = await client.EnqueueAuthenticationEvent("{}");
            QueuePostReceipt second = await client.EnqueueAuthenticationEvent("{}");
            stopwatch.Stop();

            // Assert: both fail with the cause, and the bounded retry policy keeps it to seconds,
            // where the SDK default holds each call for minutes.
            Assert.False(first.Success);
            Assert.NotNull(first.Exception);
            Assert.False(second.Success);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(30), $"Two failed enqueues took {stopwatch.Elapsed}.");
        }
    }
}
