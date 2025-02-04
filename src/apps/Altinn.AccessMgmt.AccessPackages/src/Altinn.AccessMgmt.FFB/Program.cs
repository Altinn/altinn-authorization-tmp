using System.Reflection;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
using Altinn.AccessMgmt.FFB.Components;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

var assembly = Assembly.Load(new AssemblyName("Altinn.AccessMgmt.AccessPackages.Repo"));
builder.Configuration.AddUserSecrets(assembly);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.ConfigureDb();
builder.AddDb();

//// builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

await app.UseDb();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
