using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Pipeline.Services;
using Altinn.Authorization.Integration.Platform.ResourceRegistry;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Pipelines;

internal static class ResourceRegistryPipelines
{
    internal static class ServiceOwners
    {
        internal const string LeaseName = "resource_registry_pipeline_service_owners";

        internal static async IAsyncEnumerable<IDictionary<string, ServiceOwner>> Stream(PipelineSourceContext context, CancellationToken cancellationToken)
        {
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var result = await resourceRegistry.GetServiceOwners(cancellationToken);
            if (result.IsProblem)
            {
                throw new InvalidOperationException(result.ProblemDetails.Detail);
            }

            yield return result.Content.Orgs;
            yield break;
        }

        internal static Task<List<Provider>> Transform(PipelineSegmentContext<IDictionary<string, ServiceOwner>> context)
        {
            var result = new List<Provider>();
            foreach (var serviceOwner in context.Data)
            {
                result.Add(new()
                {
                    Id = serviceOwner.Value.Id,
                    LogoUrl = serviceOwner.Value.Logo,
                    Name = serviceOwner.Value.Name.Nb,
                    RefId = serviceOwner.Value.Orgnr,
                    Code = serviceOwner.Key,
                    TypeId = ProviderTypeConstants.ServiceOwner,
                });
            }

            return Task.FromResult(result);
        }

        internal static async Task Load(PipelineSinkContext<List<Provider>> context)
        {
            var ingest = context.Services.ServiceProvider.GetRequiredService<IIngestService>();
            var audit = context.Services.ServiceProvider.GetRequiredService<IAuditAccessor>().AuditValues;

            await ingest.IngestAndMergeData(context.Data, audit, ["Id"], CancellationToken.None);
        }
    }

    internal static class Resources
    {
        internal const string LeaseName = "resource_registry_pipeline_resources";

        public static async IAsyncEnumerable<(string NextLink, IEnumerable<ResourceUpdatedModel> Data)> Stream(PipelineSourceContext context, CancellationToken cancellationToken)
        {
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var lease = await context.Lease.Get<Lease>(cancellationToken);

            await foreach (var page in await resourceRegistry.StreamResources(lease.Since, lease.ResourceNextPageLink, cancellationToken))
            {
                if (page.IsProblem)
                {
                    throw new InvalidOperationException(page.ProblemDetails.Detail);
                }

                yield return (page.Content.Links.Next, page.Content.Data);
            }

            yield break;
        }

        public static Task<bool> Transform(PipelineSegmentContext<PipelineSegmentContext<(string NextLink, IEnumerable<ResourceUpdatedModel> Data)>> context)
        {
            var add = new List<>
            var remove = new List<string>(); 
            return Task.FromResult(false);
        }

        public static Task Load(PipelineSinkContext<bool> context)
        {
            return Task.CompletedTask;
        }

        internal class Lease
        {
            /// <summary>
            /// Latest element that was updated
            /// </summary>
            public DateTime Since { get; set; } = default;

            /// <summary>
            /// The URL of the next page of Resource data.
            /// </summary>
            public string ResourceNextPageLink { get; set; }
        }
    }
}
