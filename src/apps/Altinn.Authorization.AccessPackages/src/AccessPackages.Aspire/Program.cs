using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cli = builder.AddProject<Altinn_Authorization_AccessPackages_CLI>("cli");

//var brreg = builder.AddProject<Altinn_Authorization_Workers_BrReg>("workers-brreg").WaitForCompletion(cli);
//var resreg = builder.AddProject<Altinn_Authorization_Workers_ResReg>("workers-resreg").WaitForCompletion(cli);

var metaApi = builder.AddProject<AccessPackages_MetaApi>("meta-api"); //.WaitForCompletion(cli);
//var api = builder.AddProject<Altinn_Authorization_AccessPackages>("api"); //.WaitForCompletion(cli); // .WaitFor(brreg).WaitFor(resreg);
var web = builder.AddProject<Altinn_Authorization_FFB>("web");//.WaitFor(api);

builder.AddProject<Projects.AccessPackages_MetaWeb>("accesspackages-metaweb");
//var api = builder.AddProject<Altinn_Authorization_AccessPackages>("api").WaitForCompletion(cli); // .WaitFor(brreg).WaitFor(resreg);
//var web = builder.AddProject<Altinn_Authorization_FFB>("web").WaitFor(api);

builder.Build().Run();
