using Altinn.Authorization.AccessPackages.Extensions;
using Altinn.Authorization.AccessPackages.Repo.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddDatabaseDefinitions();
builder.AddDbAccessData();

var app = builder.Build();

app.Services.UseDatabaseDefinitions();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/icon/{type}/{category}/{name}", (string type, string category, string name) =>
{
    return Results.File(@$"resources/{type}/{category}/{name}.svg", contentType: "image/svg+xml");
}).WithOpenApi().WithTags("Icon").WithSummary("Gets icons");

app.MapDbAccessEndpoints();

/*
app.MapDefaultsExt<IEntityService, Entity, ExtEntity>(mapIngest: true, mapSearch: true);
app.MapDefaultsExt<IEntityTypeService, EntityType, ExtEntityType>();
app.MapDefaultsExt<IEntityVariantService, EntityVariant, ExtEntityVariant>();
app.MapCrossDefaults<EntityVariant, IEntityVariantRoleService, EntityVariantRole, Role>("variants", "roles");
app.MapDefaultsExt<IPackageService, Package, ExtPackage>(mapSearch: true);
app.MapCrossDefaults<Package, IPackageTagService, PackageTag, Tag>("packages", "tags");
app.MapDefaults<IProviderService, Provider>();
app.MapDefaults<IRoleService, Role>();
app.MapDefaultsExt<IRolePackageService, RolePackage, ExtRolePackage>();
app.MapDefaultsExt<IRoleAssignmentService, RoleAssignment, ExtRoleAssignment>();
app.MapDefaultsExt<ITagService, Tag, ExtTag>();
app.MapDefaults<ITagGroupService, TagGroup>();
*/

app.Run();
