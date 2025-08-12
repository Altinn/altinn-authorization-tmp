using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement;
using Altinn.AccessMgmt.Persistence.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

AppDomain domain = AppDomain.CurrentDomain;
domain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));

WebApplication app = AccessManagementHost.Create(args);

var appsettings = app.Services.GetRequiredService<IOptions<AccessManagementAppsettings>>().Value;
if (appsettings.Database.MigrateDb || appsettings.Database.MigrateDbAndTerminate)
{
    using var scope = app.Services.CreateScope();

    // Do Something like this when using EF
    // scope.ServiceProvider.GetRequiredService<AppDbContext>().MigrateAsync();

    // Current framework
    await app.UseAccessMgmtDb(true);
    
    if (appsettings.Database.MigrateDbAndTerminate)
    {
        return;
    }
}

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
app.UseDbAudit();

app.MapControllers();
app.MapDefaultAltinnEndpoints();

await app.RunAsync();

/// <summary>
/// Startup class.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed partial class Program { }
