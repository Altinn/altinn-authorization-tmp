using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.PersistenceEF.Audit;

public static class ServiceProviderExtensions
{
    public static IServiceScope CreateEFScope(this IServiceProvider provider, AuditValues auditValues)
    {
        var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IAuditAccessor>();
        accessor.Current = auditValues;
        return scope;
    }

}
