using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddScoped<IAuditContextProvider, HttpContextAuditContextProvider>();
// builder.Services.AddScoped<AuditConnectionInterceptor>();

builder.Services.AddDbContext<BasicDbContext>((sp, options) =>
{
    //var interceptor = sp.GetRequiredService<AuditConnectionInterceptor>();
    options.UseNpgsql(builder.Configuration["Database:Postgres:AppConnectionString"]);
    //.AddInterceptors(interceptor);
});

builder.Services.AddDbContext<ExtendedDbContext>((sp, options) =>
{
    //var interceptor = sp.GetRequiredService<AuditConnectionInterceptor>();
    options.UseNpgsql(builder.Configuration["Database:Postgres:AppConnectionString"]);
    options.EnableSensitiveDataLogging(); // Viser verdier i parametre
    //options.LogTo(Console.WriteLine, LogLevel.Information);
    //options.AddInterceptors(interceptor);

});

builder.Services.AddDbContext<AuditDbContext>((sp, options) =>
{
    //var interceptor = sp.GetRequiredService<AuditConnectionInterceptor>();
    options.UseNpgsql(builder.Configuration["Database:Postgres:AppConnectionString"]);
    //.AddInterceptors(interceptor);
});

builder.Services.AddScoped<PackageService>();
builder.Services.AddScoped<AreaGroupService>();

var app = builder.Build();

app.MapGet("/test", async ([FromServices] PackageService service, Guid id) => { return await service.Get(id); });
app.MapGet("/test2", async ([FromServices] PackageService service, Guid id) => { return await service.GetExtended(id); });
app.MapGet("/test3", async ([FromServices] ExtendedDbContext db, Guid id) => { return await db.ExtendedPackages
    .Include(t => t.Area).ThenInclude(t => t.Group)
    .Include(t => t.Provider)
    .Include(t => t.EntityType).ThenInclude(t => t.Provider)
    .SingleOrDefaultAsync(t => t.Id == id); });

app.MapGet("/create", async ([FromServices] BasicDbContext db, [FromServices] AreaGroupService service) => 
{
    var org = await db.EntityTypes.SingleAsync(t => t.Name == "Organisasjon") ?? throw new Exception("EntityType not found");

    var areaGroup = new Altinn.AccessMgmt.Core.Models.AreaGroup() { Id = Guid.CreateVersion7(), Name = "TEST", Description = "Bare en test", Urn = "urn:areagroup:test", EntityTypeId = org.Id };

    await service.Create(areaGroup, audit: new AuditValues(ChangedBy: AuditDefaultsTemp.StaticDataIngest, ChangedBySystem: AuditDefaultsTemp.StaticDataIngest, OperationId: Guid.CreateVersion7().ToString()));
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

/// <summary>
/// Default values for Audit system entities
/// </summary>
public static class AuditDefaultsTemp
{
    /// <summary>
    /// StaticDataIngest
    /// </summary>
    public static readonly Guid StaticDataIngest = Guid.Parse("3296007F-F9EA-4BD0-B6A6-C8462D54633A");

    /// <summary>
    /// RegisterImportSystem
    /// </summary>
    public static readonly Guid RegisterImportSystem = Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B");

    /// <summary>
    /// EnduserApi
    /// </summary>
    public static readonly Guid EnduserApi = Guid.Parse(EnduserApiStr);

    /// <summary>
    /// EnduserApiStr
    /// </summary>
    public const string EnduserApiStr = "ED771364-42A8-4934-801E-B482ED20EC3E";
}
