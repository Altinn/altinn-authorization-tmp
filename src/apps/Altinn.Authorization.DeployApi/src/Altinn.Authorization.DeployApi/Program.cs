using Altinn.Authorization.DeployApi.BootstrapDatabase;
using Altinn.Authorization.Hosting.Extensions;
using Altinn.Authorization.DeployApi.Pipelines;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddAltinnHostDefaults();

TokenCredential cred;
if (builder.Environment.IsDevelopment())
{
    cred = new DefaultAzureCredential();
}
else
{
    var clientId = builder.Configuration.GetValue<string>("ManagedIdentity:ClientId");
    if (string.IsNullOrEmpty(clientId))
    {
        throw new InvalidOperationException("ManagedIdentity:ClientId is required in production mode");
    }

    cred = new ManagedIdentityCredential(clientId);
}

builder.Services.AddSingleton(cred);
builder.Services.AddOptions<KestrelServerOptions>()
    .Configure(o => o.AllowSynchronousIO = true);

// Add services to the container.
builder.Services.AddAuthentication()
    .AddJwtBearer("github", "GitHub", options => { });

var app = builder.Build();

app.UseAltinnHostDefaults();
app.UseWebSockets();

app.MapTaskPipeline<BootstrapDatabasePipeline>("/deployapi/api/v1/database/bootstrap");

app.Run();
