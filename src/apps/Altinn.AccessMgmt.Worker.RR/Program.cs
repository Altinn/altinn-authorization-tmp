using System.Reflection;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
using Altinn.AccessMgmt.Worker.RR;
using Altinn.AccessMgmt.Worker.RR.Models;
using Altinn.AccessMgmt.Worker.RR.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args).AddEnvironmentVariables().AddJsonFile("appsettings.json");

var assembly = Assembly.Load(new AssemblyName("Altinn.AccessMgmt.AccessPackages.Repo"));
builder.Configuration.AddUserSecrets(assembly);

builder.Services.Configure<ResourceRegisterImportConfig>(builder.Configuration.GetRequiredSection("ResRegConfig"));

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddSingleton<Engine>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Services.UseDatabaseDefinitions();

host.Run();
