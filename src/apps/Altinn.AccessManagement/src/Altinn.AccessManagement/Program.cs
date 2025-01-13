using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.AddDbAccessMigrations();
builder.AddJsonIngests();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
{
    var originalIdSelector = c.SchemaGeneratorOptions.SchemaIdSelector;
    c.SchemaGeneratorOptions.SchemaIdSelector = (Type t) =>
    {
        if (!t.IsNested)
        {
            return originalIdSelector(t);
        }

        var chain = new List<string>();
        do
        {
            chain.Add(originalIdSelector(t));
            t = t.DeclaringType;
        }
        while (t != null);

        chain.Reverse();
        return string.Join(".", chain);
    };
});

var app = builder.Build();

//WebApplication app = AccessManagementHost.Create(args);

app.AddDefaultAltinnMiddleware(errorHandlingPath: "/accessmanagement/api/v1/error");

if (app.Environment.IsDevelopment())
{
    // Enable higher level of detail in exceptions related to JWT validation
    IdentityModelEventSource.ShowPII = true;

    // Enable Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.Services.UseDatabaseDefinitions();
await app.Services.UseDbAccessMigrations();
await app.Services.UseJsonIngests();
////app.Services.UseDbAccessMigrations();

app.MapControllers();
app.UseHealthChecks("/health");
////app.MapDefaultAltinnEndpoints();

await app.RunAsync();
