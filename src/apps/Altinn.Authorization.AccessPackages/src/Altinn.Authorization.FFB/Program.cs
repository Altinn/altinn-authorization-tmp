using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.FFB.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

app.Services.UseDatabaseDefinitions();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

var providerService = app.Services.GetRequiredService<IProviderService>();
var res = await providerService.Get();
foreach (var item in res)
{
    Console.WriteLine(item.Name);
}

app.Run();
