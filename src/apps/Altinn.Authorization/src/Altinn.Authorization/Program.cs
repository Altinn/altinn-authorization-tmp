using Altinn.Authorization;
using Microsoft.IdentityModel.Logging;

var app = AuthorizationHost.Create(args);

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();

    // Enable higher level of detail in exceptions related to JWT validation
    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseExceptionHandler("/authorization/api/v1/error");
}

app.UseSwagger(o => o.RouteTemplate = "authorization/swagger/{documentName}/swagger.json");

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/authorization/swagger/v1/swagger.json", "Altinn Platform Authorization API");
    c.RoutePrefix = "authorization/swagger";
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
