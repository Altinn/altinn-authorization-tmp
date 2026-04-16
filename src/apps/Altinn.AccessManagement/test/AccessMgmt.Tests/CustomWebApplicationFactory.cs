using Altinn.AccessMgmt.PersistenceEF.Audit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Tests
{
    /// <summary>
    /// CustomWebApplicationFactory for integration tests
    /// </summary>
    /// <typeparam name="TStartup">Entrypoint</typeparam>
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
       where TStartup : class
    {
        /// <summary>
        /// ConfigureWebHost for setup of configuration and test services
        /// </summary>
        /// <param name="builder">IWebHostBuilder</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var appsettings = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Logging:LogLevel:*"] = "Warning",
                ["FeatureManagement:AccessManagement.InstanceDelegation.EF"] = "true",
                ["FeatureManagement:AccessManagement.ResourceDelegation.EF"] = "true",
            });

            builder.UseConfiguration(appsettings.Build());

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IAuditAccessor, AuditAccessor>();
            });
        }
    }
}
