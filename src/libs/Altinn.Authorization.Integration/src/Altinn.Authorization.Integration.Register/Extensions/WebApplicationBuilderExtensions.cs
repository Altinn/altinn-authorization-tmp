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
    public static IHostApplicationBuilder AddAltinnRegister(this IHostApplicationBuilder builder, Action<AltinnRegisterOptions> configureOptions)
    {
        builder.Services.AddOptions<AltinnRegisterOptions>()
            .Validate(opts => opts.Endpoint != null)
            .Configure(configureOptions);

        builder.Services.AddHttpClient(RegisterClient.HttpClientName, (serviceProvider, httpClient) =>
        {
        });

        builder.Services.AddSingleton<IAltinnRegister, RegisterClient>();

        return builder;
    }
}
