using System.Net.Mime;
using Altinn.Authorization.Configuration.AppSettings;
using Altinn.Authorization.Hosting.Extensions;
using Altinn.Authorization.Index;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var app = IndexHost.Create(args, "Index");

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