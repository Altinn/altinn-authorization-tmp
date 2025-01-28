using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Integration.Register.Extensions;

public static class WebApplicationBuilderExtensions

{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    public static IHostApplicationBuilder UseAltinnRegister(this IHostApplicationBuilder builder, Action<AltinnRegisterOptions> configureOptions)
    {
        builder.Services.AddHttpClient(RegisterClient.HttpClientName, cfg =>
        {
            builder.Services.Configure(configureOptions);
            var options = new AltinnRegisterOptions(configureOptions);
            cfg.BaseAddress = new Uri(options.Endpoint);
        });

        return builder;
    }
}
