-- Enum: delegation.instanceType

CREATE TABLE IF NOT EXISTS consent.consentright
(
    consentRightId uuid PRIMARY KEY NOT NULL,
    consentRequestId uuid,
    action text[]
)




