using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.Importers.ResReg;
using Altinn.Authorization.Importers.ResReg.Services;
using Altinn.Authorization.Workers.ResReg.Models;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args).AddEnvironmentVariables().AddJsonFile("appsettings.json");

builder.Services.Configure<ResourceRegisterImportConfig>(builder.Configuration.GetRequiredSection("ResRegConfig"));

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddSingleton<Engine>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Services.UseDatabaseDefinitions();

host.Run();
