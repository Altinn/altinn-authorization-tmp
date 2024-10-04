using Altinn.Authorization.DeployApi.BootstrapDatabase;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

TokenCredential cred;
if (builder.Environment.IsDevelopment())
{
    cred = new DefaultAzureCredential();
}
else
{
    cred = new ManagedIdentityCredential();
}

builder.Services.AddSingleton(cred);
builder.Services.AddOptions<KestrelServerOptions>()
    .Configure(o => o.AllowSynchronousIO = true);

// Add services to the container.
builder.Services.AddAuthentication()
    .AddJwtBearer("github", "GitHub", options => { });

var app = builder.Build();

app.MapPost("/api/v1/database/bootstrap", (BootstrapDatabasePipeline pipeline, HttpContext context) => pipeline.Run(context));

app.Run();
