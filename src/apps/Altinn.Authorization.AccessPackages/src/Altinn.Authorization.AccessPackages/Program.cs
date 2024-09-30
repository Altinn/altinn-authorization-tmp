using Altinn.Authorization.AccessPackages;
using Altinn.Authorization.Hosting.Extensions;

var app = AccessPackagesHost.Create(args, "AccessPackages");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAltinnHostDefaults();
app.MapControllers();

await app.RunAsync();

/// <summary>
/// Program
/// </summary>
public partial class Program { }