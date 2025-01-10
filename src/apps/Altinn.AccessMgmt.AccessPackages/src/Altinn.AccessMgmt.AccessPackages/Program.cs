using Altinn.AccessMgmt.AccessPackages.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

var app = builder.Build();

app.Services.UseDatabaseDefinitions();

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
