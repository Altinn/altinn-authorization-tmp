using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Audit;

public class AuditMiddleware(AppDbContext db) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var auditContextAccessor = context.RequestServices.GetRequiredService<IAuditAccessor>();
        if (context.GetEndpoint() is var endpoint && endpoint != null)
        {
            if (endpoint.Metadata.GetMetadata<AuditJWTClaimToDbAttribute>() is var jwtClaimToDb && jwtClaimToDb != null)
            {
                var claim = context.User?.Claims?
                    .FirstOrDefault(c => c.Type.Equals(jwtClaimToDb.Claim, StringComparison.OrdinalIgnoreCase));

                if (claim == null && jwtClaimToDb.AllowSystemUser)
                {
                    claim = GetSystemUserClaim(context.User?.Claims);
                }

                if (claim != null && Guid.TryParse(claim.Value, out var uuid))
                {
                    auditContextAccessor.AuditValues = new(uuid, Guid.Parse(jwtClaimToDb.System), TraceId(context), DateTimeOffset.UtcNow);
                }
            }
            else if (endpoint.Metadata.GetMetadata<AuditStaticDbAttribute>() is var staticDb && staticDb != null)
            {
                if (staticDb.ChangedBy != null && staticDb.System != null)
                {
                    auditContextAccessor.AuditValues = new(Guid.Parse(staticDb.ChangedBy), Guid.Parse(staticDb.System), TraceId(context), DateTimeOffset.UtcNow);
                }
                else if (staticDb.System != null)
                {
                    auditContextAccessor.AuditValues = new(Guid.Parse(staticDb.System), Guid.Parse(staticDb.System), TraceId(context), DateTimeOffset.UtcNow);
                }
            }
            else if (endpoint.Metadata.GetMetadata<AuditServiceOwnerConsumerAttribute>() is var serviceOwnerConsumer && serviceOwnerConsumer is { })
            {
                var party = OrgUtil.GetAuthenticatedParty(context.User);
                if (party is { })
                {
                    var entity = await GetEntityFromConsumerClaim(context, party);
                    if (entity is { })
                    {
                        if (serviceOwnerConsumer.System is { })
                        {
                            auditContextAccessor.AuditValues = new(entity.Id, Guid.Parse(serviceOwnerConsumer.System), TraceId(context));
                        }
                        else
                        {
                            auditContextAccessor.AuditValues = new(entity.Id, entity.Id, TraceId(context));
                        }
                    }
                }
            }
        }

        await next(context);
    }

    private static string TraceId(HttpContext context)
    {
        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

    private async Task<Entity> GetEntityFromConsumerClaim(HttpContext context, AccessManagement.Core.Models.Consent.ConsentPartyUrn party)
    {
        if (party.IsOrganizationId(out var organizationIdentifier))
        {
            return await db.Entities.FirstOrDefaultAsync(e => e.OrganizationIdentifier == organizationIdentifier, context.RequestAborted);
        }
        else if (party.IsPartyId(out var partyId))
        {
            return await db.Entities.FirstOrDefaultAsync(e => e.PartyId == partyId, context.RequestAborted);
        }
        else if (party.IsPartyUuid(out var partyUuid))
        {
            return await db.Entities.FirstOrDefaultAsync(e => e.Id == partyUuid, context.RequestAborted);
        }
        else if (party.IsPersonId(out var personId))
        {
            return await db.Entities.FirstOrDefaultAsync(e => e.PersonIdentifier == personId, context.RequestAborted);
        }

        return null;
    }

    /// <summary>
    /// Find the special system user claim and return it as standard claim if available
    /// </summary>
    private Claim? GetSystemUserClaim(IEnumerable<Claim>? claims)
    {
        Claim? authorizationDetails = claims?.FirstOrDefault(c => c.Type.Equals("authorization_details"));

        if (authorizationDetails != null)
        {
            JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
            SystemUserClaim systemUserClaimCore = JsonSerializer.Deserialize<SystemUserClaim>(authorizationDetails.Value, jsonOptions);

            if (systemUserClaimCore?.Systemuser_id != null && systemUserClaimCore.Systemuser_id.Count > 0)
            {
                return new Claim("urn:altinn:party:uuid", systemUserClaimCore.Systemuser_id[0]);
            }
        }

        return null;
    }
}
