using Altinn.AccessMgmt.Persistence;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Extensions;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// move to : C:\r\altinn-authorization\src\apps\Altinn.AccessManagement\src

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(NpgsqlDataSource.Create("Database=newtests;Host=localhost;Username=wigg;Password=jw8s0F4;Include Error Detail=true"));

var config = builder.Configuration.Get<DbAccessConfig>();
//// builder.ConfigureDb();

//builder.AddDb(opts =>
//{
//    opts.DbType = MgmtDbType.Postgres;
//    opts.Enabled = true;
//});

var app = builder.Build();

//await app.UseDb();

//app.MapAllDefinitionEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
