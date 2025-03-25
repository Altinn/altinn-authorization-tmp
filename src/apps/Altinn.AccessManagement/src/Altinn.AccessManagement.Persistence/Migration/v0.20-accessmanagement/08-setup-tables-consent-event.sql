-- Table: consent.metadata

CREATE TABLE IF NOT EXISTS consent.consentevents
(
  consentEventId uuid NOT NULL,
  eventtype consent.event_type NOT NULL,
  created timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  performedByPartyUuid UUID
);



