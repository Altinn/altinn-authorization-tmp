using Altinn.AccessMgmt.Persistence.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Altinn.AccessMgmt.Persistence.Services;

public class AuditMiddleware(IDbAuditService auditService) : IMiddleware
{
    public IDbAuditService AuditService { get; } = auditService;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint() is var endpoint && endpoint != null)
        {
            if (endpoint.Metadata.GetMetadata<DbAuditAttribute>() is var attr && attr != null)
            {
                var claim = context.User?.Claims?
                    .FirstOrDefault(c => c.Type.Equals(attr.Claim, StringComparison.OrdinalIgnoreCase));

                var uuid = Guid.Parse("066148fe-7077-4484-b7ea-44b5ede0014e");

                // if (claim != null && Guid.TryParse(claim.Value, out var _))
                // {
                AuditService.Set(new()
                {
                    ChangedBy = uuid,
                    ChangedBySystem = Guid.Parse(attr.System),
                    ChangeOperationId = context.TraceIdentifier,
                });
                // }
            }
        }

        await next(context);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DbAuditAttribute : Attribute
{
    public string Claim { get; set; }

    public string System { get; set; }
}

/// <inheritdoc/>
public class AuditFactory(IHttpContextAccessor accessor) : IDbAudit, IDbAuditService
{
    public IHttpContextAccessor Accessor { get; } = accessor;

    public ChangeRequestOptions Value =>
        Accessor.HttpContext.Items.TryGetValue(nameof(AuditFactory), out var result) ?
            result as ChangeRequestOptions :
            throw new InvalidOperationException("");

    public void Set(ChangeRequestOptions options) =>
        Accessor.HttpContext.Items.Add(nameof(AuditFactory), options);
}

public interface IDbAudit
{
    public ChangeRequestOptions Value { get; }
}

public interface IDbAuditService
{
    void Set(ChangeRequestOptions options);
}
