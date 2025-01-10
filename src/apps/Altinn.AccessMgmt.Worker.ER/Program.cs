using System.Reflection;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
using Altinn.AccessMgmt.Worker.ER;
using Altinn.AccessMgmt.Worker.ER.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args).AddEnvironmentVariables().AddJsonFile("appsettings.json");

var assembly = Assembly.Load(new AssemblyName("Altinn.AccessMgmt.AccessPackages.Repo"));
builder.Configuration.AddUserSecrets(assembly);

builder.Services.Configure<BrRegConfig>(builder.Configuration.GetRequiredSection("BrRegConfig"));

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddSingleton<Ingestor>();
builder.Services.AddSingleton<Importer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.UseDatabaseDefinitions();

host.Run();
