var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Altinn_AccessManagement_Api_Enduser>("altinn-accessmanagement-api-enduser");
builder.AddProject<Projects.Altinn_AccessManagement>("altinn-accessmanagement");

builder.Build().Run();
