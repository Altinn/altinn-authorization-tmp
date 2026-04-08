-- Index for consent

-- consent.consentrequest: party + status lookup for GetRequestsForParty() and status filtering
CREATE INDEX IF NOT EXISTS idx_consentrequest_frompartyuuid_status
    ON consent.consentrequest (fromPartyUuid, status);

-- consent.consentrequest: status-only lookup for getting consent requests by status. Not used by any existing query, but low cost and may be useful for future queries for statistics.
CREATE INDEX IF NOT EXISTS idx_consentrequest_status
    ON consent.consentrequest (status);

-- consent.consentright: join/lookup column used by multiple queries
CREATE INDEX IF NOT EXISTS idx_consentright_consentrequestid
    ON consent.consentright (consentRequestId);

-- consent.metadata: JOIN column (no existing index)
CREATE INDEX IF NOT EXISTS idx_metadata_consentrightid
    ON consent.metadata (consentRightId);

-- consent.consentevent: filter + sort for GetEvents()
CREATE INDEX IF NOT EXISTS idx_consentevent_consentrequestid_created
    ON consent.consentevent (consentRequestId, created);

-- consent.context: lookup column for GetConsentContext()
CREATE INDEX IF NOT EXISTS idx_context_consentrequestid
    ON consent.context (consentRequestId);
