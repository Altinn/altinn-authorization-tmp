using Altinn.Authorization.AccessPackages.CLI;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddUserSecrets("2163e793-201c-46c9-9d8f-a586a3aaf7b5");

var config = new CLIConfig()
{
    EnableMigrations = true,
    EnableJsonIngest = true,
    RunTests = true
};

builder.Services.AddSingleton<Mockups>();

//// builder.AddDbAccessTelemetry();
builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

if (config.EnableMigrations)
{
    builder.AddDbAccessMigrations();
}

if (config.EnableJsonIngest)
{
    builder.AddJsonIngests();
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

var mockService = host.Services.GetRequiredService<Mockups>();
await mockService.KlientDelegeringMock();

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
