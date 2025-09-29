CREATE SCHEMA ingest;
CREATE ROLE ingest_admins NOLOGIN;
GRANT ingest_admins TO platform_authorization_admin, platform_authorization;
ALTER SCHEMA ingest OWNER TO ingest_admins;
