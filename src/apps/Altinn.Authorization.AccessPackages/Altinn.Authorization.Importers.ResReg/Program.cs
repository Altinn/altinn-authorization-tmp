using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.Importers.ResReg;

var builder = Host.CreateApplicationBuilder(args);

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();
builder.AddDbAccessMigrations();

builder.Services.AddSingleton<Engine>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.UseDatabaseDefinitions();
await host.Services.UseDbAccessMigrations();

host.Run();
