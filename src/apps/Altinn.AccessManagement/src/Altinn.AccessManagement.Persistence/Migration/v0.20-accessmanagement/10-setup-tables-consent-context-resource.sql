-- Table: consent.resourcecontext

CREATE TABLE IF NOT EXISTS consent.resourcecontext
(
  id uuid PRIMARY KEY NOT NULL,
  contextId uuid NOT NULL,
  resourceId text NOT NULL,
  language text NOT NULL,
  context text NOT NULL
);



