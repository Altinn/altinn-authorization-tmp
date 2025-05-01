-- Table: consent.consentevent

CREATE TABLE IF NOT EXISTS consent.context
(
  contextId uuid NOT NULL,
  consentRequestId uuid NOT NULL,
  language text NOT NULL,
  context text NOT NULL
);



