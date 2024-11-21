using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cli = builder.AddProject<Altinn_Authorization_AccessPackages_CLI>("cli");
builder.AddProject<Altinn_Authorization_AccessPackages>("api").WaitForCompletion(cli);
builder.AddProject<Altinn_Authorization_FFB>("web").WaitForCompletion(cli);

/*
builder.AddProject<Altinn_Authorization_Importers_ResReg>("altinn-authorization-importers-resreg").WaitForCompletion(cli);
builder.AddProject<Altinn_Authorization_Importers_BRREG>("brreg").WaitForCompletion(cli);
*/

builder.Build().Run();
