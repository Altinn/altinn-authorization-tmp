using Altinn.Authorization.AccessPackages.Repo.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Services.UseDatabaseDefinitions();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
