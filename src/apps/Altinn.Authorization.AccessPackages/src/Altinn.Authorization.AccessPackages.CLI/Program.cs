using Altinn.Authorization.AccessPackages.CLI;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.Importers.BRREG.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var config = new CLIConfig()
{
    EnableMigrations = true,
    EnableJsonIngest = true,
    EnableBrregIngest = true,
    EnableBrregImport = false,
    RunTests = true
};

//// builder.AddDbAccessTelemetry();
builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

if (config.EnableMigrations)
{
    builder.AddDbAccessMigrations(opt =>
    {
        opt.ConnectionString = "ddd";
    });
}

if (config.EnableJsonIngest)
{
    builder.AddJsonIngests();
}

if (config.EnableBrregIngest)
{
    builder.AddBrregIngestor();
}

if (config.EnableBrregImport)
{
    builder.AddBrregImporter();
}

var host = builder.Build();

host.Services.UseDatabaseDefinitions();

if (config.EnableMigrations)
{
    await host.Services.UseDbAccessMigrations();
}

if (config.EnableJsonIngest)
{
    await host.Services.UseJsonIngests();
}

if (config.EnableBrregIngest)
{
    await host.Services.UseBrregIngestor();
}

if (config.EnableBrregImport)
{
    await host.Services.UseBrregImporter();
}

if (config.RunTests)
{
    /*
    //// Test Provider
    var providerService = host.Services.GetRequiredService<IProviderService>();
    var res = await providerService.Get();
    foreach (var item in res)
    {
        Console.WriteLine(item.Name);
    }
    */

    var areaService = host.Services.GetRequiredService<IAreaService>();
    var areaRes = await areaService.GetExtended();
    foreach (var item in areaRes)
    {
        Console.WriteLine(item.Name + ":" + item.Packages.Count());
    }

    /*
    //// Test Variant
    var TagService = host.Services.GetRequiredService<ITagService>();
    var TagResult = await TagService.GetExtended();
    Console.WriteLine(JsonSerializer.Serialize(TagResult));
    foreach (var item in TagResult)
    {
        Console.WriteLine($"{item.Id}:{item.Name}");
    }
    */

    /*
    //// Test Package
    var packageService = host.Services.GetRequiredService<IPackageService>();
    var packageResult = await packageService.GetExtended();
    foreach (var item in packageResult)
    {
        Console.WriteLine($"{item.Id}:{item.Name}");
    }
    */
}
