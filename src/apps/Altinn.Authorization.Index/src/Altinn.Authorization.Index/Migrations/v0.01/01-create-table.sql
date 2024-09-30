-- Table: delegation.delegationChanges
CREATE TABLE garfield.delegationchanges
(
  id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  age integer,
  coveredByUserId integer,
  blobStoragePolicyPath character
)