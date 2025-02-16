using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Repo.Data.Services;
using Altinn.AccessMgmt.Repo.Ingest;
using Altinn.AccessMgmt.Repo.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//// using Altinn.Authorization.Host.Lease;

namespace Altinn.AccessMgmt.Repo.Extensions;

public static class DbAccessHostExtensions
{
    public static IHostApplicationBuilder ConfigureDb(this IHostApplicationBuilder builder)
    {
        //builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));

        //builder.AddAltinnLease(opt =>
        //{
        //    opt.Type = AltinnLeaseType.InMemory;
        //    //opt.Type = AltinnLeaseType.AzureStorageAccount;
        //    //opt.StorageAccount.Endpoint = new Uri("https://standreastest.blob.core.windows.net/");
        //});

        return builder;
    }

    public static IHostApplicationBuilder AddDb(this IHostApplicationBuilder builder)
    {
        DefinitionStore.RegisterAllDefinitions("Altinn.AccessMgmt.Repo");
        builder.Services.AddSingleton<IDbConverter, DbConverter>();

        RegisterDbServices(builder.Services);
        builder.Services.AddSingleton<MigrationService>();
        builder.Services.AddSingleton<IngestService>();
        builder.Services.AddSingleton<MockupService>();

        return builder;
    }

    public static async Task<IHost> UseDb(this IHost host)
    {
        var migration = host.Services.GetRequiredService<MigrationService>();
        migration.Generate("Altinn.AccessMgmt.Models");
        await migration.Migrate();

        var dbIngest = host.Services.GetRequiredService<IngestService>();
        if (dbIngest != null)
        {
            await dbIngest.IngestAll();
        }

        var mockService = host.Services.GetService<MockupService>();
        if (mockService != null)
        {
            await mockService.Run();
        }

        return host;
    }

    private static void RegisterDbServices(IServiceCollection services)
    {
        #region Register Services
        services.AddSingleton<IPackageResourceService, PackageResourceDataService>();
        services.AddSingleton<IResourceService, ResourceDataService>();
        services.AddSingleton<IResourceGroupService, ResourceGroupDataService>();
        services.AddSingleton<IResourceTypeService, ResourceTypeDataService>();
        services.AddSingleton<IElementTypeService, ElementTypeDataService>();
        services.AddSingleton<IElementService, ElementDataService>();
        services.AddSingleton<IPolicyService, PolicyDataService>();
        services.AddSingleton<IPolicyElementService, PolicyElementDataService>();
        services.AddSingleton<IAreaService, AreaDataService>();
        services.AddSingleton<IAreaGroupService, AreaGroupDataService>();
        services.AddSingleton<IWorkerConfigService, WorkerConfigDataService>();
        services.AddSingleton<IEntityTypeService, EntityTypeDataService>();
        services.AddSingleton<IEntityVariantService, EntityVariantDataService>();
        services.AddSingleton<IPackageService, PackageDataService>();
        services.AddSingleton<IProviderService, ProviderDataService>();
        services.AddSingleton<IRoleService, RoleDataService>();
        services.AddSingleton<IRolePackageService, RolePackageDataService>();
        services.AddSingleton<IRoleResourceService, RoleResourceDataService>();
        services.AddSingleton<ITagGroupService, TagGroupDataService>();
        services.AddSingleton<IPackageTagService, PackageTagDataService>();
        services.AddSingleton<ITagService, TagDataService>();
        services.AddSingleton<IEntityService, EntityDataService>();
        services.AddSingleton<IEntityLookupService, EntityLookupDataService>();
        services.AddSingleton<IEntityVariantRoleService, EntityVariantRoleDataService>();
        services.AddSingleton<IRoleMapService, RoleMapDataService>();
        services.AddSingleton<IAssignmentService, AssignmentDataService>();
        services.AddSingleton<IAssignmentPackageService, AssignmentPackageDataService>();
        services.AddSingleton<IAssignmentResourceService, AssignmentResourceDataService>();
        services.AddSingleton<IGroupService, GroupDataService>();
        services.AddSingleton<IGroupMemberService, GroupMemberDataService>();
        services.AddSingleton<IGroupAdminService, GroupAdminDataService>();
        services.AddSingleton<IGroupDelegationService, GroupDelegationDataService>();
        services.AddSingleton<IDelegationService, DelegationDataService>();
        #endregion
    }

}
