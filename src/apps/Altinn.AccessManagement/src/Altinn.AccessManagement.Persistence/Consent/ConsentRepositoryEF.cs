using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using EfConsent = Altinn.AccessMgmt.PersistenceEF.Models.Consent;

namespace Altinn.AccessManagement.Persistence.Consent
{
    /// <summary>
    /// EF Core implementation of <see cref="IConsentRepository"/>, reading and writing the
    /// <c>consent</c> schema through <see cref="AppDbContext"/>.
    /// </summary>
    public class ConsentRepositoryEF(AppDbContext db, IAuditAccessor auditAccessor, IOptions<PostgreSQLSettings> config, TimeProvider timeProvider) : IConsentRepository
    {
        private readonly AppDbContext _db = db;
        private readonly IAuditAccessor _auditAccessor = auditAccessor;
        private readonly TimeSpan _consentRequestSafetyLag = TimeSpan.FromSeconds(config.Value.ConsentEventsSafetyLag);
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <summary>
        /// The consent tables carry no audit columns and have no history triggers, so the
        /// session audit context is unused for these writes — but <see cref="AppDbContext"/>
        /// requires audit values on every save. Use the request's values when the endpoint
        /// pipeline provided them; otherwise attribute the write to the consent API system,
        /// preserving the behaviour of the raw implementation, which required no audit context.
        /// </summary>
        private AuditValues AuditValues => _auditAccessor.AuditValues ?? new AuditValues(SystemEntityConstants.ConsentApi);

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedByParty, CancellationToken cancellationToken = default)
        {
            DateTimeOffset createdTime = DateTime.UtcNow;

            if (!consentRequest.From.IsPartyUuid(out Guid fromPartyUuid))
            {
                throw new InvalidDataException("Invalid fromPartyUuid");
            }

            if (!consentRequest.To.IsPartyUuid(out Guid toPartyUuid))
            {
                throw new InvalidDataException("Invalid toPartyUuid");
            }

            Guid? handledByPartyUuid = consentRequest.HandledBy != null && consentRequest.HandledBy.IsPartyUuid(out Guid handledBy) ? handledBy : null;
            Guid? requiredDelegatorUuid = consentRequest.RequiredDelegator != null && consentRequest.RequiredDelegator.IsPartyUuid(out Guid requiredDelegator) ? requiredDelegator : null;

            EfConsent.ConsentRequest request = new()
            {
                ConsentRequestId = consentRequest.Id,
                FromPartyUuid = fromPartyUuid,
                ToPartyUuid = toPartyUuid,
                HandledByPartyUuid = handledByPartyUuid,
                RequiredDelegatorUuid = requiredDelegatorUuid,
                ValidTo = consentRequest.ValidTo.ToOffset(TimeSpan.Zero),
                Consented = consentRequest.Consented?.ToOffset(TimeSpan.Zero),
                RequestMessage = consentRequest.RequestMessage,
                RedirectUrl = consentRequest.RedirectUrl,
                TemplateId = consentRequest.TemplateId,
                TemplateVersion = consentRequest.TemplateVersion,
                Status = ToEfStatus(consentRequest.ConsentRequestStatus),
                PortalViewMode = ToEfViewMode(consentRequest.PortalViewMode),
            };

            if (consentRequest.CreatedTime is { } explicitCreated)
            {
                // Only Altinn 2 consents carry their own creation time; otherwise the
                // property stays unset so the database default (CURRENT_TIMESTAMP) applies.
                request.Created = explicitCreated.ToOffset(TimeSpan.Zero);
            }

            _db.ConsentRequests.Add(request);

            for (int rightIndex = 0; rightIndex < consentRequest.ConsentRights.Count; rightIndex++)
            {
                ConsentRight consentRight = consentRequest.ConsentRights[rightIndex];

                // consentright has no ordinal column; reads return the rights ordered by id,
                // so the ids get strictly increasing uuid7 timestamps to preserve the
                // caller's right order (uuid7 values within one millisecond do not sort).
                Guid consentRightId = Guid.CreateVersion7(createdTime.AddMilliseconds(rightIndex));

                _db.ConsentRights.Add(new EfConsent.ConsentRight
                {
                    ConsentRightId = consentRightId,
                    ConsentRequestId = consentRequest.Id,
                    Action = consentRight.Action?.ToArray(),
                });

                foreach (ConsentResourceAttribute attribute in consentRight.Resource)
                {
                    _db.ConsentResourceAttributes.Add(new EfConsent.ConsentResourceAttribute
                    {
                        ConsentRightId = consentRightId,
                        Type = attribute.Type,
                        Value = attribute.Value,
                        Version = attribute.Version,
                    });
                }

                if (consentRight.Metadata is { Count: > 0 })
                {
                    foreach (KeyValuePair<string, string> metadata in consentRight.Metadata)
                    {
                        _db.ConsentMetadata.Add(new EfConsent.ConsentMetadata
                        {
                            ConsentRightId = consentRightId,
                            Id = metadata.Key,
                            Value = metadata.Value,
                        });
                    }
                }
            }

            if (consentRequest.ConsentRequestEvents is { Count: > 0 })
            {
                foreach (ConsentRequestEvent consentEvent in consentRequest.ConsentRequestEvents)
                {
                    if (!consentEvent.PerformedBy.IsPartyUuid(out Guid eventPerformedBy))
                    {
                        throw new InvalidDataException("Invalid performedBy party");
                    }

                    _db.ConsentEvents.Add(new EfConsent.ConsentEvent
                    {
                        ConsentEventId = Guid.CreateVersion7(),
                        ConsentRequestId = consentRequest.Id,
                        EventType = ToEfEventType(consentEvent.EventType),
                        Created = consentEvent.Created.ToOffset(TimeSpan.Zero),
                        PerformedByParty = eventPerformedBy,
                    });
                }
            }
            else
            {
                if (!performedByParty.IsPartyUuid(out Guid createPerformedBy))
                {
                    throw new InvalidDataException("Invalid performedBy party");
                }

                _db.ConsentEvents.Add(new EfConsent.ConsentEvent
                {
                    ConsentEventId = Guid.CreateVersion7(),
                    ConsentRequestId = consentRequest.Id,
                    EventType = EfConsent.ConsentRequestEventType.Created,
                    Created = createdTime,
                    PerformedByParty = createPerformedBy,
                });
            }

            try
            {
                await _db.SaveChangesAsync(AuditValues, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return null;
            }

            return await GetRequest(consentRequest.Id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AcceptConsentRequest(Guid consentRequestId, Guid performedByParty, ConsentContext context, CancellationToken cancellationToken = default)
        {
            DateTimeOffset consentedTime = _timeProvider.GetUtcNow();

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            int rowsAffected = await _db.ConsentRequests
                .Where(r => r.ConsentRequestId == consentRequestId && r.Status == EfConsent.ConsentRequestStatusType.Created)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(r => r.Status, EfConsent.ConsentRequestStatusType.Accepted)
                        .SetProperty(r => r.Consented, consentedTime),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            _db.ConsentEvents.Add(new EfConsent.ConsentEvent
            {
                ConsentEventId = Guid.CreateVersion7(),
                ConsentRequestId = consentRequestId,
                EventType = EfConsent.ConsentRequestEventType.Accepted,
                Created = consentedTime,
                PerformedByParty = performedByParty,
            });

            _db.ConsentContexts.Add(new EfConsent.ConsentContext
            {
                ContextId = Guid.CreateVersion7(),
                ConsentRequestId = consentRequestId,
                Language = context.Language,
            });

            await _db.SaveChangesAsync(AuditValues, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RejectConsentRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            DateTimeOffset rejectedTime = DateTime.UtcNow;

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            int rowsAffected = await _db.ConsentRequests
                .Where(r => r.ConsentRequestId == consentRequestId && r.Status == EfConsent.ConsentRequestStatusType.Created)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(r => r.Status, EfConsent.ConsentRequestStatusType.Rejected)
                        .SetProperty(r => r.Rejected, rejectedTime),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            _db.ConsentEvents.Add(new EfConsent.ConsentEvent
            {
                ConsentEventId = Guid.CreateVersion7(),
                ConsentRequestId = consentRequestId,
                EventType = EfConsent.ConsentRequestEventType.Rejected,
                Created = rejectedTime,
                PerformedByParty = performedByParty,
            });

            await _db.SaveChangesAsync(AuditValues, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task Revoke(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            DateTimeOffset revokedTime = DateTime.UtcNow;

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            int rowsAffected = await _db.ConsentRequests
                .Where(r => r.ConsentRequestId == consentRequestId && r.Status == EfConsent.ConsentRequestStatusType.Accepted)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(r => r.Status, EfConsent.ConsentRequestStatusType.Revoked)
                        .SetProperty(r => r.Revoked, revokedTime),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            _db.ConsentEvents.Add(new EfConsent.ConsentEvent
            {
                ConsentEventId = Guid.CreateVersion7(),
                ConsentRequestId = consentRequestId,
                EventType = EfConsent.ConsentRequestEventType.Revoked,
                Created = revokedTime,
                PerformedByParty = performedByParty,
            });

            await _db.SaveChangesAsync(AuditValues, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task DeleteRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<Core.Models.Consent.Consent>> GetAllConsents(Guid partyUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> GetRequest(Guid consentRequestId, CancellationToken cancellationToken = default)
        {
            EfConsent.ConsentRequest row = await _db.ConsentRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ConsentRequestId == consentRequestId, cancellationToken);

            if (row == null)
            {
                return null;
            }

            List<ConsentRight> consentRights = await GetConsentRights(consentRequestId, cancellationToken);
            List<ConsentRequestEvent> consentRequestEvents = await GetEvents(consentRequestId, cancellationToken);
            return MapRequestDetails(row, consentRights, consentRequestEvents);
        }

        /// <inheritdoc/>
        public async Task<Result<List<ConsentRequestDetails>>> GetRequestsForParty(Guid fromParty, CancellationToken cancellationToken)
        {
            List<EfConsent.ConsentRequest> rows = await _db.ConsentRequests.AsNoTracking()
                .Where(r => r.FromPartyUuid == fromParty)
                .ToListAsync(cancellationToken);

            List<ConsentRequestDetails> results = [];
            foreach (EfConsent.ConsentRequest row in rows)
            {
                List<ConsentRight> consentRights = await GetConsentRights(row.ConsentRequestId, cancellationToken);
                List<ConsentRequestEvent> consentRequestEvents = await GetEvents(row.ConsentRequestId, cancellationToken);
                results.Add(MapRequestDetails(row, consentRights, consentRequestEvents));
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<int> GetConsentRequestCountForParty(Guid fromPartyUuid, ConsentRequestStatusType status, CancellationToken cancellationToken)
        {
            EfConsent.ConsentRequestStatusType statusFilter = ToEfStatus(status);

            return await _db.ConsentRequests.AsNoTracking()
                .CountAsync(
                    r => r.FromPartyUuid == fromPartyUuid
                        && r.Status == statusFilter
                        && r.PortalViewMode == EfConsent.ConsentPortalViewMode.Show,
                    cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ConsentContext> GetConsentContext(Guid consentRequestId, CancellationToken cancellationToken = default)
        {
            List<EfConsent.ConsentContext> contexts = await _db.ConsentContexts.AsNoTracking()
                .Where(c => c.ConsentRequestId == consentRequestId)
                .ToListAsync(cancellationToken);

            EfConsent.ConsentContext row = contexts.LastOrDefault();
            if (row == null)
            {
                return null;
            }

            return new ConsentContext
            {
                Language = row.Language,
                ContextId = row.ContextId,
            };
        }

        /// <inheritdoc/>
        public async Task<Result<List<ConsentStatusChange>>> GetConsentEventsForParty(Guid partyUuid, ConsentEventsQuery consentEventsQuery, int pageSize, CancellationToken cancellationToken)
        {
            Guid uuid7SafetyBound = Guid.CreateVersion7(_timeProvider.GetUtcNow() - _consentRequestSafetyLag);
            EfConsent.ConsentRequestEventType[] eventTypes = consentEventsQuery.EventTypes?.Select(ParseEventTypeLabel).ToArray();

            IQueryable<EfConsent.ConsentEvent> events = _db.ConsentEvents.AsNoTracking()
                .Where(ce => _db.ConsentRequests.Any(r => r.ConsentRequestId == ce.ConsentRequestId && r.ToPartyUuid == partyUuid))
                .Where(ce => ce.ConsentEventId.CompareTo(uuid7SafetyBound) < 0);

            if (consentEventsQuery.ConsentRequestId is { } consentRequestId)
            {
                events = events.Where(ce => ce.ConsentRequestId == consentRequestId);
            }

            if (eventTypes is not null)
            {
                events = events.Where(ce => eventTypes.Contains(ce.EventType));
            }

            if (consentEventsQuery.CreatedAfter is { } createdAfter)
            {
                DateTimeOffset after = createdAfter.ToUniversalTime();
                events = events.Where(ce => ce.Created >= after);
            }

            if (consentEventsQuery.CreatedBefore is { } createdBefore)
            {
                DateTimeOffset before = createdBefore.ToUniversalTime();
                events = events.Where(ce => ce.Created < before);
            }

            if (consentEventsQuery.ContinueFrom is { } continueFrom)
            {
                events = events.Where(ce => ce.ConsentEventId.CompareTo(continueFrom) > 0);
            }

            List<EfConsent.ConsentEvent> page = await events
                .OrderBy(ce => ce.ConsentEventId)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return page.ConvertAll(ce => new ConsentStatusChange
            {
                ConsentRequestId = ce.ConsentRequestId,
                EventType = FromEfEventType(ce.EventType),
                ChangedDate = ce.Created,
                ConsentEventId = ce.ConsentEventId,
            });
        }

        /// <summary>
        /// Returns the consent rights for a consent request, with resource attributes and
        /// metadata grouped per right.
        /// </summary>
        private async Task<List<ConsentRight>> GetConsentRights(Guid consentRequestId, CancellationToken cancellationToken)
        {
            List<EfConsent.ConsentResourceAttribute> attributeRows = await (
                from attribute in _db.ConsentResourceAttributes.AsNoTracking()
                join right in _db.ConsentRights on attribute.ConsentRightId equals right.ConsentRightId
                where right.ConsentRequestId == consentRequestId
                select attribute).ToListAsync(cancellationToken);

            Dictionary<Guid, List<ConsentResourceAttribute>> attributesByRight = attributeRows
                .GroupBy(a => a.ConsentRightId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(a => new ConsentResourceAttribute
                    {
                        Type = a.Type,
                        Value = a.Value,
                        Version = a.Version,
                    }).ToList());

            List<EfConsent.ConsentMetadata> metadataRows = await (
                from metadata in _db.ConsentMetadata.AsNoTracking()
                join right in _db.ConsentRights on metadata.ConsentRightId equals right.ConsentRightId
                where right.ConsentRequestId == consentRequestId
                select metadata).ToListAsync(cancellationToken);

            Dictionary<Guid, Dictionary<string, string>> metadataByRight = metadataRows
                .GroupBy(m => m.ConsentRightId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(m => m.Id, m => m.Value));

            List<EfConsent.ConsentRight> rightRows = await _db.ConsentRights.AsNoTracking()
                .Where(r => r.ConsentRequestId == consentRequestId)
                .OrderBy(r => r.ConsentRightId)
                .ToListAsync(cancellationToken);

            List<ConsentRight> consentRights = [];
            foreach (EfConsent.ConsentRight rightRow in rightRows)
            {
                ConsentRight consentRight = new()
                {
                    Action = rightRow.Action?.ToList(),
                    Resource = attributesByRight.TryGetValue(rightRow.ConsentRightId, out List<ConsentResourceAttribute> attributes) ? attributes : [],
                };

                consentRight.AddMetadataValues(metadataByRight.TryGetValue(rightRow.ConsentRightId, out Dictionary<string, string> metadata) ? metadata : null);
                consentRights.Add(consentRight);
            }

            return consentRights;
        }

        private async Task<List<ConsentRequestEvent>> GetEvents(Guid consentRequestId, CancellationToken cancellationToken)
        {
            List<EfConsent.ConsentEvent> rows = await _db.ConsentEvents.AsNoTracking()
                .Where(e => e.ConsentRequestId == consentRequestId)
                .OrderBy(e => e.Created)
                .ToListAsync(cancellationToken);

            return rows.ConvertAll(e => new ConsentRequestEvent
            {
                ConsentEventID = e.ConsentEventId,
                ConsentRequestID = e.ConsentRequestId,
                EventType = FromEfEventType(e.EventType),
                Created = e.Created,
                PerformedBy = ConsentPartyUrn.PartyUuid.Create(e.PerformedByParty),
            });
        }

        private static ConsentRequestDetails MapRequestDetails(EfConsent.ConsentRequest row, List<ConsentRight> consentRights, List<ConsentRequestEvent> consentRequestEvents)
        {
            if (row.FromPartyUuid is not { } fromPartyUuid || row.ToPartyUuid is not { } toPartyUuid)
            {
                throw new InvalidDataException("Invalid party URN");
            }

            return new ConsentRequestDetails
            {
                Id = row.ConsentRequestId,
                From = ConsentPartyUrn.PartyUuid.Create(fromPartyUuid),
                To = ConsentPartyUrn.PartyUuid.Create(toPartyUuid),
                HandledBy = row.HandledByPartyUuid is { } handledBy ? ConsentPartyUrn.PartyUuid.Create(handledBy) : null,
                RequiredDelegator = row.RequiredDelegatorUuid is { } requiredDelegator ? ConsentPartyUrn.PartyUuid.Create(requiredDelegator) : null,
                ValidTo = row.ValidTo,
                ConsentRights = consentRights,
                RequestMessage = row.RequestMessage,
                ConsentRequestStatus = FromEfStatus(row.Status),
                Consented = row.Consented,
                RedirectUrl = row.RedirectUrl,
                ConsentRequestEvents = consentRequestEvents,
                TemplateId = row.TemplateId,
                TemplateVersion = row.TemplateVersion,
                PortalViewMode = FromEfViewMode(row.PortalViewMode),
            };
        }

        private static EfConsent.ConsentRequestStatusType ToEfStatus(ConsentRequestStatusType status) => status switch
        {
            ConsentRequestStatusType.Created => EfConsent.ConsentRequestStatusType.Created,
            ConsentRequestStatusType.Rejected => EfConsent.ConsentRequestStatusType.Rejected,
            ConsentRequestStatusType.Accepted => EfConsent.ConsentRequestStatusType.Accepted,
            ConsentRequestStatusType.Revoked => EfConsent.ConsentRequestStatusType.Revoked,
            ConsentRequestStatusType.Deleted => EfConsent.ConsentRequestStatusType.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "No consent.status_type label exists for the value."),
        };

        private static ConsentRequestStatusType FromEfStatus(EfConsent.ConsentRequestStatusType status) => status switch
        {
            EfConsent.ConsentRequestStatusType.Created => ConsentRequestStatusType.Created,
            EfConsent.ConsentRequestStatusType.Rejected => ConsentRequestStatusType.Rejected,
            EfConsent.ConsentRequestStatusType.Accepted => ConsentRequestStatusType.Accepted,
            EfConsent.ConsentRequestStatusType.Revoked => ConsentRequestStatusType.Revoked,
            EfConsent.ConsentRequestStatusType.Deleted => ConsentRequestStatusType.Deleted,
            _ => throw new InvalidDataException($"Consent request status '{status}' has no equivalent in {nameof(ConsentRequestStatusType)}."),
        };

        private static EfConsent.ConsentRequestEventType ToEfEventType(ConsentRequestEventType eventType) => eventType switch
        {
            ConsentRequestEventType.Created => EfConsent.ConsentRequestEventType.Created,
            ConsentRequestEventType.Rejected => EfConsent.ConsentRequestEventType.Rejected,
            ConsentRequestEventType.Accepted => EfConsent.ConsentRequestEventType.Accepted,
            ConsentRequestEventType.Revoked => EfConsent.ConsentRequestEventType.Revoked,
            ConsentRequestEventType.Deleted => EfConsent.ConsentRequestEventType.Deleted,
            ConsentRequestEventType.Used => EfConsent.ConsentRequestEventType.Used,
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "No consent.event_type label exists for the value."),
        };

        private static ConsentRequestEventType FromEfEventType(EfConsent.ConsentRequestEventType eventType) => eventType switch
        {
            EfConsent.ConsentRequestEventType.Created => ConsentRequestEventType.Created,
            EfConsent.ConsentRequestEventType.Rejected => ConsentRequestEventType.Rejected,
            EfConsent.ConsentRequestEventType.Accepted => ConsentRequestEventType.Accepted,
            EfConsent.ConsentRequestEventType.Revoked => ConsentRequestEventType.Revoked,
            EfConsent.ConsentRequestEventType.Deleted => ConsentRequestEventType.Deleted,
            EfConsent.ConsentRequestEventType.Used => ConsentRequestEventType.Used,
            _ => throw new InvalidDataException($"Consent event type '{eventType}' has no equivalent in {nameof(ConsentRequestEventType)}."),
        };

        private static EfConsent.ConsentRequestEventType ParseEventTypeLabel(string label) => label switch
        {
            "accepted" => EfConsent.ConsentRequestEventType.Accepted,
            "rejected" => EfConsent.ConsentRequestEventType.Rejected,
            "deleted" => EfConsent.ConsentRequestEventType.Deleted,
            "created" => EfConsent.ConsentRequestEventType.Created,
            "revoked" => EfConsent.ConsentRequestEventType.Revoked,
            "used" => EfConsent.ConsentRequestEventType.Used,
            _ => throw new ArgumentOutOfRangeException(nameof(label), label, "Unknown consent.event_type label."),
        };

        private static EfConsent.ConsentPortalViewMode ToEfViewMode(ConsentPortalViewMode viewMode) => viewMode switch
        {
            ConsentPortalViewMode.Hide => EfConsent.ConsentPortalViewMode.Hide,
            ConsentPortalViewMode.Show => EfConsent.ConsentPortalViewMode.Show,
            _ => throw new ArgumentOutOfRangeException(nameof(viewMode), viewMode, "No consent.portal_view_mode label exists for the value."),
        };

        private static ConsentPortalViewMode FromEfViewMode(EfConsent.ConsentPortalViewMode viewMode) => viewMode switch
        {
            EfConsent.ConsentPortalViewMode.Hide => ConsentPortalViewMode.Hide,
            EfConsent.ConsentPortalViewMode.Show => ConsentPortalViewMode.Show,
            _ => throw new InvalidDataException($"Consent portal view mode '{viewMode}' has no equivalent in {nameof(ConsentPortalViewMode)}."),
        };
    }
}
