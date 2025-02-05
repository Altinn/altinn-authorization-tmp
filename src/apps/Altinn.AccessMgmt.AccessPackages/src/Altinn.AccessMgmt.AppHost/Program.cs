using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Altinn_AccessMgmt_AccessPackages>("api");
var web = builder.AddProject<Altinn_AccessMgmt_FFB>("web").WaitFor(api);

//// var er = builder.AddProject<Altinn_AccessMgmt_Worker_ER>("workers-er").WaitFor(web);
//// var rr = builder.AddProject<Altinn_AccessMgmt_Worker_RR>("workers-rr").WaitFor(web);

builder.Build().Run();
