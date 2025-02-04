using Altinn.AccessMgmt.AccessPackages.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo.Mock;

var builder = WebApplication.CreateBuilder(args);

bool useMock = true;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.AddDbAccessMigrations();
builder.AddJsonIngests();

if (useMock)
{
    builder.Services.AddSingleton<Mockups>();
}

var app = builder.Build();

app.Services.UseDatabaseDefinitions();
await app.Services.UseDbAccessMigrations();
await app.Services.UseJsonIngests();

if (useMock)
{
    var mock = app.Services.GetRequiredService<Mockups>();
    await mock.SystemResourcesMock();
    //// await mock.BasicMock();
}

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
