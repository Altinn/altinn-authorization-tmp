using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Altinn_Authorization_AccessPackages>("api");
builder.AddProject<Altinn_Authorization_FFB>("web");

/*
builder.AddProject<Altinn_Authorization_AccessPackages_CLI>("cli");
builder.AddProject<Altinn_Authorization_Importers_BRREG>("brreg");
*/

builder.Build().Run();
