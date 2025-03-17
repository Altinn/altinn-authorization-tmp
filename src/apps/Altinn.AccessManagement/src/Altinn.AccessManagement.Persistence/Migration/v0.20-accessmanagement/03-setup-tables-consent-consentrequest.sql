-- Table: consent.consentrequest

CREATE TABLE IF NOT EXISTS consent.consentrequest
(
    consentRequestId uuid PRIMARY KEY NOT NULL,
    fromPartyUuid UUID,
    toPartyUuid UUID,
    requestMessage hstore,
    isDeleted bool default False,
    created timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status consent.status_type NOT NULL DEFAULT 'created'::consent.status_type,
    validto  timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    consented timestamp with time zone NULL,
    revoked timestamp with time zone NULL
);


