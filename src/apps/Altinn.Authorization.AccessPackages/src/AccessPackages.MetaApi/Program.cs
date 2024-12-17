using AccessPackages.MetaApi.JsonRepo;
using Altinn.Authorization.AccessPackages.Repo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddSingleton<LocalRepo>();
builder.Services.AddSingleton<LocalRepo2>();

var app = builder.Build();

app.Services.UseDatabaseDefinitions();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/accesspackage/meta/all", (LocalRepo repo) => { return repo.GetAllHier(); });
app.MapGet("/accesspackage/meta/all/flat", (LocalRepo repo) => { return repo.GetAllFlat(); });

app.MapGet("/accesspackage/groups", (LocalRepo2 repo) => { return repo.GetGroups(); });
app.MapGet("/accesspackage/group/{groupId}/areas", (LocalRepo2 repo, Guid groupId) => { return repo.GetAreas(groupId); });
app.MapGet("/accesspackage/areas", (LocalRepo2 repo) => { return repo.GetAreas(); });
app.MapGet("/accesspackage/area/{areaId}/packages", (LocalRepo2 repo, Guid areaId) => { return repo.GetPackages(areaId); });
app.MapGet("/accesspackage/packages", (LocalRepo2 repo) => { return repo.GetAllPackages(); });

app.Run();
