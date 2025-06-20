using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Services;
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

var connStr = builder.Configuration["Database:Postgres:AppConnectionString"];

builder.Services.AddScoped<PackageService>();

var app = builder.Build();

app.MapGet("/test", async ([FromServices] PackageService service, Guid id) => { return await service.Get(id); });
app.MapGet("/test2", async ([FromServices] PackageService service, Guid id) => { return await service.GetExtended(id); });
app.MapGet("/test3", async ([FromServices] ExtendedDbContext db, Guid id) => { return await db.ExtendedPackages
    .Include(t => t.Area).ThenInclude(t => t.Group)
    .Include(t => t.Provider)
    .Include(t => t.EntityType).ThenInclude(t => t.Provider)
    .SingleOrDefaultAsync(t => t.Id == id); });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
