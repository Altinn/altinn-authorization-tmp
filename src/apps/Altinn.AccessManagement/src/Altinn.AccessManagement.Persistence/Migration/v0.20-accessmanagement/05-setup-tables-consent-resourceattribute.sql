-- Enum: delegation.instanceType

CREATE TABLE IF NOT EXISTS consent.resourceattribute
(
    consentRightId uuid PRIMARY KEY NOT NULL,
    type text,
    value text  
);


