using Altinn.AccessMgmt.AccessPackages.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo.Mock;
using Altinn.AccessMgmt.DbAccess;

var builder = WebApplication.CreateBuilder(args);

bool useMock = true;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureDb();
builder.AddDb();

var app = builder.Build();
var config = app.Configuration.Get<DbAccessConfig>();

await app.UseDb();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/icon/{type}/{category}/{name}", (string type, string category, string name) =>
{
    return Results.File(@$"resources/{type}/{category}/{name}.svg", contentType: "image/svg+xml");
}
).WithOpenApi().WithTags("Icon").WithSummary("Gets icons");

app.MapDbAccessEndpoints();

app.Run();
