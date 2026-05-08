using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

/// <summary>
/// Startup class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed partial class Program { }
