using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.Workers.BrReg;
using Altinn.Authorization.Workers.BrReg.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddCommandLine(args).AddEnvironmentVariables().AddJsonFile("appsettings.json");

builder.Services.Configure<IngestorConfig>(builder.Configuration.GetRequiredSection("IngestorConfig"));
builder.Services.Configure<ImporterConfig>(builder.Configuration.GetRequiredSection("ImporterConfig"));

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddSingleton<Ingestor>();
builder.Services.AddSingleton<Importer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.UseDatabaseDefinitions();

host.Run();
