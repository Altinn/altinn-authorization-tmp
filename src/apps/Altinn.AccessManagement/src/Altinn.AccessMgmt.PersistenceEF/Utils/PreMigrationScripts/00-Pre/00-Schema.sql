alter schema ingest owner TO platform_authorization_admin;
GRANT USAGE ON SCHEMA ingest TO platform_authorization;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA ingest TO platform_authorization;
