using Altinn.AccessManagement;
using Microsoft.IdentityModel.Logging;

WebApplication app = AccessManagementHost.Create(args);

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

await app.UseDb();

app.MapControllers();
app.UseHealthChecks("/health");
////app.MapDefaultAltinnEndpoints();

await app.RunAsync();
