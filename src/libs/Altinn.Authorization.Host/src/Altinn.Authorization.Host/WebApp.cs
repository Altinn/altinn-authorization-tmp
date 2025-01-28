using Altinn.Authorization.Host.AppSettings;
using Altinn.Authorization.ServiceDefaults;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Host;

/// <summary>
/// WebApp
/// </summary>
public static class WebApp
{
    /// <summary>
    /// CreateServiceDefaults
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="args">args</param>
    public static WebApplicationBuilder CreateServiceDefaults(string name, string[] args)
    {
        return AltinnHost.CreateWebApplicationBuilder(name, args);
    }

    /// <summary>
    /// ConfigureAppConfigurationDefeaults
    /// </summary>
    /// <param name="builder">builder</param>
    /// <param name="configureOptions">configureOptions</param>
    public static IHostApplicationBuilder ConfigureAppSettingDefaults(this WebApplicationBuilder builder, Action<AppSettingsOptions> configureOptions = null)
    {
        var options = new AppSettingsOptions();
        configureOptions?.Invoke(options);
        builder.Services.Configure<AltinnAppSettings>(builder.Configuration);

        builder.Configuration.AddAzureAppConfiguration(cfg =>
        {
            var localSettings = new AltinnAppSettings(builder.Configuration);
            cfg.Connect(localSettings.AppConfiguration.Endpoint, DefaultTokenCredential.Instance);
            cfg.ConfigureStartupOptions(startup =>
            {
                startup.Timeout = TimeSpan.FromSeconds(3);
            });

            cfg.ConfigureKeyVault(kv =>
            {
                kv.SetCredential(DefaultTokenCredential.Instance);
            });

            cfg.ConfigureRefresh(refresh =>
            {
                refresh.Register("Sentinel", refreshAll: true);
            });

            foreach (var label in options.AppConfigurationLabels)
            {
                cfg.Select("*", label);
            }

            cfg.UseFeatureFlags(flags =>
            {
                foreach (var label in options.AppConfigurationLabels)
                {
                    flags.Select("*", label);
                }
            });
        });

        return builder;
    }
}
