using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authentication.Integration.Configuration;
using Altinn.Platform.Authentication.Core.Models.ResourceRegistry;
using Altinn.Platform.Authentication.Integration.ResourceRegister;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Platform.Authentication.Tests.Clients
{
    /// <summary>
    /// Tests for <see cref="ResourceRegistryClient"/>, covering the short in-memory cache in front of
    /// <see cref="ResourceRegistryClient.GetResource"/> (issue Altinn/altinn-auth#4571).
    /// </summary>
    public class ResourceRegistryClientTests
    {
        private const string ResourceBody = "{\"identifier\":\"ske-krav-og-betalinger\",\"title\":{\"nb\":\"Krav og betalinger\"},\"delegable\":true}";

        [Fact]
        public async Task GetResource_SameResourceTwice_FetchesOnceAndServesSecondFromCache()
        {
            // Arrange
            int calls = 0;
            ResourceRegistryClient client = CreateClient(CreateHttpClient(HttpStatusCode.OK, ResourceBody, _ => calls++));

            // Act
            ServiceResource? first = await client.GetResource("ske-krav-og-betalinger");
            ServiceResource? second = await client.GetResource("ske-krav-og-betalinger");

            // Assert
            Assert.NotNull(first);
            Assert.Equal("ske-krav-og-betalinger", first.Identifier);
            Assert.True(first.Delegable);
            Assert.Same(first, second);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task GetResource_DifferentResources_FetchesEach()
        {
            // Arrange
            int calls = 0;
            ResourceRegistryClient client = CreateClient(CreateHttpClient(HttpStatusCode.OK, ResourceBody, _ => calls++));

            // Act
            await client.GetResource("ske-krav-og-betalinger");
            await client.GetResource("ske-krav-og-betalinger-2");

            // Assert
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task GetResource_NotFound_ReturnsNullAndIsNotCached()
        {
            // Arrange: a resource that does not exist yet must be looked up again on the next call
            int calls = 0;
            ResourceRegistryClient client = CreateClient(CreateHttpClient(HttpStatusCode.NotFound, string.Empty, _ => calls++));

            // Act
            ServiceResource? first = await client.GetResource("missing-resource");
            ServiceResource? second = await client.GetResource("missing-resource");

            // Assert
            Assert.Null(first);
            Assert.Null(second);
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task GetResource_CacheExpired_FetchesAgain()
        {
            // Arrange: a cache whose clock can be moved past the cache duration
            FakeSystemClock clock = new();
            MemoryCache memoryCache = new(new MemoryCacheOptions { Clock = clock });
            int calls = 0;
            ResourceRegistryClient client = CreateClient(CreateHttpClient(HttpStatusCode.OK, ResourceBody, _ => calls++), memoryCache);

            // Act
            await client.GetResource("ske-krav-og-betalinger");
            clock.Advance(ResourceRegistryClient.ResourceCacheDuration + TimeSpan.FromSeconds(1));
            await client.GetResource("ske-krav-og-betalinger");

            // Assert
            Assert.Equal(2, calls);
        }

        private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string responseBody, Action<HttpRequestMessage>? capture = null)
        {
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capture?.Invoke(request))
                .ReturnsAsync(() => new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
                });

            return new HttpClient(handlerMock.Object);
        }

        private static ResourceRegistryClient CreateClient(HttpClient httpClient, IMemoryCache? memoryCache = null)
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            IOptions<PlatformSettings> platformSettings = Options.Create(new PlatformSettings
            {
                ApiResourceRegistryEndpoint = "http://localhost:5100/resourceregistry/api/v1/"
            });

            return new ResourceRegistryClient(
                httpClient,
                Mock.Of<ILogger<ResourceRegistryClient>>(),
                httpContextAccessorMock.Object,
                platformSettings,
                memoryCache ?? new MemoryCache(new MemoryCacheOptions()));
        }

        /// <summary>
        /// A clock the test moves forward by hand, so cache expiry can be tested without waiting.
        /// </summary>
        private sealed class FakeSystemClock : ISystemClock
        {
            private DateTimeOffset _now = new(2026, 10, 16, 12, 0, 0, TimeSpan.Zero);

            public DateTimeOffset UtcNow => _now;

            public void Advance(TimeSpan by) => _now = _now.Add(by);
        }
    }
}
