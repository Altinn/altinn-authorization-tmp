using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Altinn.AccessMgmt.PersistenceEF.Audit;

public class AuditMiddleware(IAuditAccessor auditContextAccessor) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint() is var endpoint && endpoint != null)
        {
            if (endpoint.Metadata.GetMetadata<AuditJWTClaimToDbAttribute>() is var attr && attr != null)
            {
                var claim = context.User?.Claims?
                    .FirstOrDefault(c => c.Type.Equals(attr.Claim, StringComparison.OrdinalIgnoreCase));

                if (claim != null && Guid.TryParse(claim.Value, out var uuid))
                {
                    auditContextAccessor.Current = new(uuid, Guid.Parse(attr.System), Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);
                }
            }
        }

        await next(context);
    }
}
