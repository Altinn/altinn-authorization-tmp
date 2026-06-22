-- Baseline DDL for the legacy delegation, accessmanagement and consent schemas.
-- Generated from 'pg_dump --schema-only' of the established database, so it
-- reproduces those schemas exactly. Two lines are added on top of the dump body:
-- the hstore extension (used by the consent tables; pg_dump --schema does not emit
-- extensions) and check_function_bodies=false (so functions may forward-reference
-- tables created later in the script, as pg_dump itself does). The latter is SET
-- LOCAL so it is scoped to the migration transaction and does not leak onto the
-- pooled connection.
SET LOCAL check_function_bodies = false;
CREATE EXTENSION IF NOT EXISTS hstore;


--
-- Name: accessmanagement; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA accessmanagement;


--
-- Name: consent; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA consent;


--
-- Name: delegation; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA delegation;


--
-- Name: event_type; Type: TYPE; Schema: consent; Owner: -
--

CREATE TYPE consent.event_type AS ENUM (
    'accepted',
    'rejected',
    'deleted',
    'created',
    'revoked',
    'used'
);


--
-- Name: portal_view_mode; Type: TYPE; Schema: consent; Owner: -
--

CREATE TYPE consent.portal_view_mode AS ENUM (
    'hide',
    'show'
);


--
-- Name: status_type; Type: TYPE; Schema: consent; Owner: -
--

CREATE TYPE consent.status_type AS ENUM (
    'unopened',
    'opened',
    'accepted',
    'rejected',
    'deleted',
    'created',
    'revoked'
);


--
-- Name: delegationchangetype; Type: TYPE; Schema: delegation; Owner: -
--

CREATE TYPE delegation.delegationchangetype AS ENUM (
    'grant',
    'revoke',
    'revoke_last'
);


--
-- Name: instancedelegationmode; Type: TYPE; Schema: delegation; Owner: -
--

CREATE TYPE delegation.instancedelegationmode AS ENUM (
    'parallelsigning',
    'normal'
);


--
-- Name: instancedelegationsource; Type: TYPE; Schema: delegation; Owner: -
--

CREATE TYPE delegation.instancedelegationsource AS ENUM (
    'user',
    'app'
);


--
-- Name: uuidtype; Type: TYPE; Schema: delegation; Owner: -
--

CREATE TYPE delegation.uuidtype AS ENUM (
    'urn:altinn:person:uuid',
    'urn:altinn:organization:uuid',
    'urn:altinn:systemuser:uuid',
    'urn:altinn:enterpriseuser:uuid',
    'urn:altinn:resource',
    'urn:altinn:party:uuid'
);


--
-- Name: upsert_resourceregistryresource(text, text); Type: FUNCTION; Schema: accessmanagement; Owner: -
--

CREATE FUNCTION accessmanagement.upsert_resourceregistryresource(_resourceregistryid text, _resourcetype text) RETURNS TABLE(resourceid bigint, resourceregistryid text, resourcetype text, created timestamp with time zone, modified timestamp with time zone)
    LANGUAGE sql ROWS 1
    AS $$
  	
	INSERT INTO 
		accessmanagement.resource (resourceregistryid, resourcetype, created, modified)
	VALUES
		(_resourceregistryid, _resourcetype, now(), now()) 
	ON CONFLICT (resourceregistryid)
	DO
		UPDATE SET resourcetype = _resourcetype, modified = now();	
	
	SELECT	    
		r.resourceid,
		r.resourceregistryid,
		r.resourcetype,
		r.created,
		r.modified
	FROM
		accessmanagement.resource r
	WHERE
		r.resourceregistryid = _resourceregistryid
	
$$;


--
-- Name: get_all_changes(character varying, integer, integer, integer); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.get_all_changes(_altinnappid character varying, _offeredbypartyid integer, _coveredbyuserid integer, _coveredbypartyid integer) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
  SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
  FROM delegation.delegationChanges
  WHERE
    altinnAppId = _altinnAppId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
$$;


--
-- Name: get_all_current_changes_coveredbypartyids(character varying[], integer[], integer[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.get_all_current_changes_coveredbypartyids(_altinnappids character varying[], _offeredbypartyids integer[], _coveredbypartyids integer[]) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
  SELECT
    delegationchangeid,
    delegationChangeType,
    altinnappid,
    offeredbypartyid,
    coveredbypartyid,
    coveredbyuserid,
    performedbyuserid,
    blobstoragepolicypath,
    blobstorageversionid,
    created
  FROM delegation.delegationchanges
    INNER JOIN
    (
	  SELECT MAX(delegationChangeId) AS maxChange
	  FROM delegation.delegationchanges
	  WHERE
	    (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
	    AND (offeredByPartyId = ANY (_offeredByPartyIds))
	    AND coveredByPartyId = ANY (_coveredByPartyIds)
      GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId
    ) AS selectMaxChange
    ON delegationChangeId = selectMaxChange.maxChange
$$;


--
-- Name: get_all_current_changes_coveredbyuserids(character varying[], integer[], integer[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.get_all_current_changes_coveredbyuserids(_altinnappids character varying[], _offeredbypartyids integer[], _coveredbyuserids integer[]) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
  SELECT
    delegationchangeid,
    delegationChangeType,
    altinnappid,
    offeredbypartyid,
    coveredbypartyid,
    coveredbyuserid,
    performedbyuserid,
    blobstoragepolicypath,
    blobstorageversionid,
    created
  FROM delegation.delegationchanges
    INNER JOIN
    (
	  SELECT MAX(delegationChangeId) AS maxChange
	  FROM delegation.delegationchanges
	  WHERE
        (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
        AND (offeredByPartyId = ANY (_offeredByPartyIds))
        AND coveredByUserId = ANY (_coveredByUserIds)
	  GROUP BY altinnAppId, offeredByPartyId, coveredByUserId
    ) AS selectMaxChange
    ON delegationChangeId = selectMaxChange.maxChange
$$;


--
-- Name: get_all_current_changes_offeredbypartyid_only(character varying[], integer[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.get_all_current_changes_offeredbypartyid_only(_altinnappids character varying[], _offeredbypartyids integer[]) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
  SELECT
    delegationchangeid,
    delegationChangeType,
    altinnappid,
    offeredbypartyid,
    coveredbypartyid,
    coveredbyuserid,
    performedbyuserid,
    blobstoragepolicypath,
    blobstorageversionid,
    created
  FROM delegation.delegationchanges
	INNER JOIN
	(
		SELECT MAX(delegationChangeId) AS maxChange
	 	FROM delegation.delegationchanges
		WHERE
		  (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
		  AND (offeredByPartyId = ANY (_offeredByPartyIds))
		GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId, coveredByUserId
	) AS selectMaxChange
	ON delegationChangeId = selectMaxChange.maxChange
$$;


--
-- Name: get_current_change(character varying, integer, integer, integer); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.get_current_change(_altinnappid character varying, _offeredbypartyid integer, _coveredbyuserid integer, _coveredbypartyid integer) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbypartyid integer, coveredbyuserid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE ROWS 1 PARALLEL SAFE
    AS $$
  SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByPartyId,
    coveredByUserId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
  FROM delegation.delegationChanges
  WHERE
    altinnAppId = _altinnAppId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY delegationChangeId DESC LIMIT 1
$$;


--
-- Name: insert_delegationchange(delegation.delegationchangetype, character varying, integer, integer, integer, integer, character varying, character varying); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.insert_delegationchange(_delegationchangetype delegation.delegationchangetype, _altinnappid character varying, _offeredbypartyid integer, _coveredbyuserid integer, _coveredbypartyid integer, _performedbyuserid integer, _blobstoragepolicypath character varying, _blobstorageversionid character varying) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql ROWS 1
    AS $$
  INSERT INTO delegation.delegationChanges(
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId
  )
  VALUES (
    _delegationChangeType,
    _altinnAppId,
    _offeredByPartyId,
    _coveredByUserId,
    _coveredByPartyId,
    _performedByUserId,
    _blobStoragePolicyPath,
    _blobStorageVersionId
  )
  RETURNING
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
$$;


--
-- Name: insert_resourceregistrydelegationchange(delegation.delegationchangetype, text, integer, integer, integer, integer, integer, text, text, timestamp with time zone); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.insert_resourceregistrydelegationchange(_delegationchangetype delegation.delegationchangetype, _resourceregistryid text, _offeredbypartyid integer, _coveredbyuserid integer, _coveredbypartyid integer, _performedbyuserid integer, _performedbypartyid integer, _blobstoragepolicypath text, _blobstorageversionid text, _delegatedtime timestamp with time zone DEFAULT CURRENT_TIMESTAMP) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql ROWS 1
    AS $$
  WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE resourceRegistryId = _resourceregistryid
	),
	insertedDelegation AS (
	INSERT INTO delegation.ResourceRegistryDelegationChanges(
		delegationChangeType,
		resourceId_fk,
		offeredByPartyId,
		coveredByUserId,
		coveredByPartyId,
		performedByUserId,
		performedByPartyId,
		blobStoragePolicyPath,
		blobStorageVersionId,	
		created
	  )
	  SELECT _delegationChangeType,
		res.resourceId,
		_offeredByPartyId,
		_coveredByUserId,
		_coveredByPartyId,
		_performedByUserId,
		_performedbypartyid,
		_blobStoragePolicyPath,
		_blobStorageVersionId,
		_delegatedTime
	  FROM res
	  RETURNING 
	  	resourceRegistryDelegationChangeId,
		delegationChangeType,
		resourceId_fk,
		offeredByPartyId,
		coveredByUserId,
		coveredByPartyId,
		performedByUserId,
		performedByPartyId,
		blobStoragePolicyPath,
		blobStorageVersionId,	
		created
  )
  SELECT
  	ins.resourceRegistryDelegationChangeId,
	ins.delegationChangeType,
  	res.resourceRegistryId,
	res.resourceType,
	ins.offeredByPartyId,
	ins.coveredByUserId,
	ins.coveredByPartyId,
	ins.performedByUserId,
	ins.performedByPartyId,
	ins.blobStoragePolicyPath,
	ins.blobStorageVersionId,	
	ins.created
  FROM insertedDelegation AS ins
  JOIN res ON ins.resourceId_fk = res.resourceid;
$$;


--
-- Name: select_active_resourceregistrydelegationchanges(integer[], integer[], text[], text[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_active_resourceregistrydelegationchanges(_coveredbypartyids integer[], _offeredbypartyids integer[], _resourceregistryids text[] DEFAULT NULL::text[], _resourcetypes text[] DEFAULT NULL::text[]) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$

	WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE (_resourceRegistryIds IS NULL OR resourceRegistryId = ANY (_resourceRegistryIds))
		AND (_resourceTypes IS NULL OR resourceType = ANY (_resourceTypes))
	)
	SELECT
		rrDelegationChange.resourceRegistryDelegationChangeId,
		rrDelegationChange.delegationChangeType,
		res.resourceRegistryId,
		res.resourceType,
		rrDelegationChange.offeredByPartyId,
		rrDelegationChange.coveredByUserId,
		rrDelegationChange.coveredByPartyId,
		rrDelegationChange.performedByUserId,
		rrDelegationChange.performedByPartyId,
		rrDelegationChange.blobStoragePolicyPath,
		rrDelegationChange.blobStorageVersionId,	
		rrDelegationChange.created
	FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
		INNER JOIN res ON rrDelegationChange.resourceId_fk = res.resourceid
		INNER JOIN
		(
			SELECT MAX(resourceRegistryDelegationChangeId) AS maxChange
			FROM delegation.ResourceRegistryDelegationChanges AS rrdc
				INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
			WHERE
				(_offeredByPartyIds IS NULL OR offeredByPartyId = ANY (_offeredByPartyIds))
				AND (_coveredbypartyids IS NULL OR coveredByPartyId = ANY (_coveredbypartyids))
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$$;


--
-- Name: select_active_resourceregistrydelegationchanges_coveredbypartys(integer[], integer[], text[], text[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_active_resourceregistrydelegationchanges_coveredbypartys(_coveredbypartyids integer[], _offeredbypartyids integer[] DEFAULT NULL::integer[], _resourceregistryids text[] DEFAULT NULL::text[], _resourcetypes text[] DEFAULT NULL::text[]) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
	WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE (_resourceRegistryIds IS NULL OR resourceRegistryId = ANY (_resourceRegistryIds))
		AND (_resourceTypes IS NULL OR resourceType = ANY (_resourceTypes))
	)
	SELECT
		rrDelegationChange.resourceRegistryDelegationChangeId,
		rrDelegationChange.delegationChangeType,
		res.resourceRegistryId,
		res.resourceType,
		rrDelegationChange.offeredByPartyId,
		rrDelegationChange.coveredByUserId,
		rrDelegationChange.coveredByPartyId,
		rrDelegationChange.performedByUserId,
		rrDelegationChange.performedByPartyId,
		rrDelegationChange.blobStoragePolicyPath,
		rrDelegationChange.blobStorageVersionId,	
		rrDelegationChange.created
	FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
		INNER JOIN res ON rrDelegationChange.resourceId_fk = res.resourceid
		INNER JOIN
		(
			SELECT MAX(resourceRegistryDelegationChangeId) AS maxChange
			FROM delegation.ResourceRegistryDelegationChanges AS rrdc
				INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
			WHERE
				(_offeredByPartyIds IS NULL OR offeredByPartyId = ANY (_offeredByPartyIds))
				AND coveredByPartyId = ANY (_coveredbypartyids)
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$$;


--
-- Name: select_active_resourceregistrydelegationchanges_coveredbyuser(integer, integer[], text[], text[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_active_resourceregistrydelegationchanges_coveredbyuser(_coveredbyuserid integer, _offeredbypartyids integer[] DEFAULT NULL::integer[], _resourceregistryids text[] DEFAULT NULL::text[], _resourcetypes text[] DEFAULT NULL::text[]) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
	WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE (_resourceRegistryIds IS NULL OR resourceRegistryId = ANY (_resourceRegistryIds))
		AND (_resourceTypes IS NULL OR resourceType = ANY (_resourceTypes))
	)
	SELECT
		rrDelegationChange.resourceRegistryDelegationChangeId,
		rrDelegationChange.delegationChangeType,
		res.resourceRegistryId,
		res.resourceType,
		rrDelegationChange.offeredByPartyId,
		rrDelegationChange.coveredByUserId,
		rrDelegationChange.coveredByPartyId,
		rrDelegationChange.performedByUserId,
		rrDelegationChange.performedByPartyId,
		rrDelegationChange.blobStoragePolicyPath,
		rrDelegationChange.blobStorageVersionId,	
		rrDelegationChange.created
	FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
		INNER JOIN res ON rrDelegationChange.resourceId_fk = res.resourceid
		INNER JOIN
		(
			SELECT MAX(resourceRegistryDelegationChangeId) AS maxChange
			FROM delegation.ResourceRegistryDelegationChanges AS rrdc
				INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
			WHERE
				coveredByUserId = _coveredbyuserid
				AND (_offeredByPartyIds IS NULL OR offeredByPartyId = ANY (_offeredByPartyIds))
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$$;


--
-- Name: select_active_resourceregistrydelegationchanges_offeredby(integer, text[], text[]); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_active_resourceregistrydelegationchanges_offeredby(_offeredbypartyid integer, _resourceregistryids text[] DEFAULT NULL::text[], _resourcetypes text[] DEFAULT NULL::text[]) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$
	WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE (_resourceRegistryIds IS NULL OR resourceRegistryId = ANY (_resourceRegistryIds))
		AND (_resourceTypes IS NULL OR resourceType = ANY (_resourceTypes))
	)
	SELECT
		rrDelegationChange.resourceRegistryDelegationChangeId,
		rrDelegationChange.delegationChangeType,
		res.resourceRegistryId,
		res.resourceType,
		rrDelegationChange.offeredByPartyId,
		rrDelegationChange.coveredByUserId,
		rrDelegationChange.coveredByPartyId,
		rrDelegationChange.performedByUserId,
		rrDelegationChange.performedByPartyId,
		rrDelegationChange.blobStoragePolicyPath,
		rrDelegationChange.blobStorageVersionId,	
		rrDelegationChange.created
	FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
		INNER JOIN res ON rrDelegationChange.resourceId_fk = res.resourceid
		INNER JOIN
		(
			SELECT MAX(resourceRegistryDelegationChangeId) AS maxChange
			FROM delegation.ResourceRegistryDelegationChanges AS rrdc
				INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
			WHERE
				offeredByPartyId = _offeredByPartyId
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$$;


--
-- Name: select_current_resourceregistrydelegationchange(text, integer, integer, integer); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_current_resourceregistrydelegationchange(_resourceregistryid text, _offeredbypartyid integer, _coveredbyuserid integer, _coveredbypartyid integer) RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE ROWS 1 PARALLEL SAFE
    AS $$
  SELECT
  	rrDelegationChange.resourceRegistryDelegationChangeId,
	rrDelegationChange.delegationChangeType,
  	res.resourceRegistryId,
	res.resourceType,
	rrDelegationChange.offeredByPartyId,
	rrDelegationChange.coveredByUserId,
	rrDelegationChange.coveredByPartyId,
	rrDelegationChange.performedByUserId,
	rrDelegationChange.performedByPartyId,
	rrDelegationChange.blobStoragePolicyPath,
	rrDelegationChange.blobStorageVersionId,	
	rrDelegationChange.created
  FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
	JOIN accessmanagement.Resource AS res ON rrDelegationChange.resourceId_fk = res.resourceid
  WHERE
    res.resourceRegistryId = _resourceRegistryId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1
$$;


--
-- Name: select_delegationchanges_by_id_range(bigint, bigint); Type: FUNCTION; Schema: delegation; Owner: -
--

CREATE FUNCTION delegation.select_delegationchanges_by_id_range(_startid bigint, _endid bigint DEFAULT '9223372036854775807'::bigint) RETURNS TABLE(delegationchangeid integer, delegationchangetype delegation.delegationchangetype, altinnappid text, offeredbypartyid integer, coveredbypartyid integer, coveredbyuserid integer, performedbyuserid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone)
    LANGUAGE sql STABLE PARALLEL SAFE
    AS $$

  SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByPartyId,
    coveredByUserId,    
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
  FROM delegation.delegationChanges
  WHERE
    delegationChangeId BETWEEN _startId AND _endId
$$;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: resource; Type: TABLE; Schema: accessmanagement; Owner: -
--

CREATE TABLE accessmanagement.resource (
    resourceid bigint NOT NULL,
    resourceregistryid text NOT NULL,
    resourcetype text NOT NULL,
    created timestamp with time zone NOT NULL,
    modified timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: resource_resourceid_seq; Type: SEQUENCE; Schema: accessmanagement; Owner: -
--

ALTER TABLE accessmanagement.resource ALTER COLUMN resourceid ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME accessmanagement.resource_resourceid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: consentevent; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.consentevent (
    consenteventid uuid NOT NULL,
    consentrequestid uuid NOT NULL,
    eventtype consent.event_type NOT NULL,
    created timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    performedbyparty uuid NOT NULL
);


--
-- Name: consentrequest; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.consentrequest (
    consentrequestid uuid NOT NULL,
    frompartyuuid uuid,
    requireddelegatoruuid uuid,
    topartyuuid uuid,
    handledbypartyuuid uuid,
    requestmessage public.hstore,
    redirecturl text,
    isdeleted boolean DEFAULT false,
    created timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    templateid text NOT NULL,
    templateversion integer,
    status consent.status_type DEFAULT 'created'::consent.status_type NOT NULL,
    validto timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    consented timestamp with time zone,
    revoked timestamp with time zone,
    rejected timestamp with time zone,
    portalviewmode consent.portal_view_mode DEFAULT 'hide'::consent.portal_view_mode NOT NULL
);


--
-- Name: consentright; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.consentright (
    consentrightid uuid NOT NULL,
    consentrequestid uuid NOT NULL,
    action text[]
);


--
-- Name: context; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.context (
    contextid uuid NOT NULL,
    consentrequestid uuid NOT NULL,
    language text NOT NULL
);


--
-- Name: metadata; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.metadata (
    consentrightid uuid NOT NULL,
    id text,
    value text
);


--
-- Name: resourceattribute; Type: TABLE; Schema: consent; Owner: -
--

CREATE TABLE consent.resourceattribute (
    consentrightid uuid NOT NULL,
    type text,
    value text,
    version text
);


--
-- Name: delegationchanges; Type: TABLE; Schema: delegation; Owner: -
--

CREATE TABLE delegation.delegationchanges (
    delegationchangeid bigint NOT NULL,
    delegationchangetype delegation.delegationchangetype NOT NULL,
    altinnappid character varying NOT NULL,
    offeredbypartyid integer NOT NULL,
    coveredbypartyid integer,
    coveredbyuserid integer,
    performedbyuserid integer NOT NULL,
    blobstoragepolicypath character varying NOT NULL,
    blobstorageversionid character varying,
    created timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    fromuuid uuid,
    fromtype delegation.uuidtype,
    touuid uuid,
    totype delegation.uuidtype,
    performedbyuuid uuid,
    performedbytype delegation.uuidtype
);


--
-- Name: delegationchanges_delegationchangeid_seq; Type: SEQUENCE; Schema: delegation; Owner: -
--

ALTER TABLE delegation.delegationchanges ALTER COLUMN delegationchangeid ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME delegation.delegationchanges_delegationchangeid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: instancedelegationchanges; Type: TABLE; Schema: delegation; Owner: -
--

CREATE TABLE delegation.instancedelegationchanges (
    instancedelegationchangeid bigint NOT NULL,
    delegationchangetype delegation.delegationchangetype NOT NULL,
    instancedelegationmode delegation.instancedelegationmode NOT NULL,
    resourceid text NOT NULL,
    instanceid text NOT NULL,
    fromuuid uuid NOT NULL,
    fromtype delegation.uuidtype NOT NULL,
    touuid uuid NOT NULL,
    totype delegation.uuidtype NOT NULL,
    performedby text,
    performedbytype delegation.uuidtype,
    blobstoragepolicypath text NOT NULL,
    blobstorageversionid text NOT NULL,
    created timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    instancedelegationsource delegation.instancedelegationsource DEFAULT 'app'::delegation.instancedelegationsource NOT NULL
);


--
-- Name: instancedelegationchanges_instancedelegationchangeid_seq; Type: SEQUENCE; Schema: delegation; Owner: -
--

ALTER TABLE delegation.instancedelegationchanges ALTER COLUMN instancedelegationchangeid ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME delegation.instancedelegationchanges_instancedelegationchangeid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: resourceregistrydelegationchanges; Type: TABLE; Schema: delegation; Owner: -
--

CREATE TABLE delegation.resourceregistrydelegationchanges (
    resourceregistrydelegationchangeid bigint NOT NULL,
    delegationchangetype delegation.delegationchangetype NOT NULL,
    resourceid_fk integer NOT NULL,
    offeredbypartyid integer NOT NULL,
    coveredbypartyid integer,
    coveredbyuserid integer,
    performedbyuserid integer,
    performedbypartyid integer,
    blobstoragepolicypath text NOT NULL,
    blobstorageversionid text NOT NULL,
    created timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    fromuuid uuid,
    fromtype delegation.uuidtype,
    touuid uuid,
    totype delegation.uuidtype,
    performedbyuuid uuid,
    performedbytype delegation.uuidtype
);


--
-- Name: resourceregistrydelegationcha_resourceregistrydelegationcha_seq; Type: SEQUENCE; Schema: delegation; Owner: -
--

ALTER TABLE delegation.resourceregistrydelegationchanges ALTER COLUMN resourceregistrydelegationchangeid ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME delegation.resourceregistrydelegationcha_resourceregistrydelegationcha_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: resource resource_pkey; Type: CONSTRAINT; Schema: accessmanagement; Owner: -
--

ALTER TABLE ONLY accessmanagement.resource
    ADD CONSTRAINT resource_pkey PRIMARY KEY (resourceid);


--
-- Name: resource unique_resourceregisterid; Type: CONSTRAINT; Schema: accessmanagement; Owner: -
--

ALTER TABLE ONLY accessmanagement.resource
    ADD CONSTRAINT unique_resourceregisterid UNIQUE (resourceregistryid);


--
-- Name: consentevent consentevent_pkey; Type: CONSTRAINT; Schema: consent; Owner: -
--

ALTER TABLE ONLY consent.consentevent
    ADD CONSTRAINT consentevent_pkey PRIMARY KEY (consenteventid);


--
-- Name: consentrequest consentrequest_pkey; Type: CONSTRAINT; Schema: consent; Owner: -
--

ALTER TABLE ONLY consent.consentrequest
    ADD CONSTRAINT consentrequest_pkey PRIMARY KEY (consentrequestid);


--
-- Name: consentright consentright_pkey; Type: CONSTRAINT; Schema: consent; Owner: -
--

ALTER TABLE ONLY consent.consentright
    ADD CONSTRAINT consentright_pkey PRIMARY KEY (consentrightid);


--
-- Name: context context_pkey; Type: CONSTRAINT; Schema: consent; Owner: -
--

ALTER TABLE ONLY consent.context
    ADD CONSTRAINT context_pkey PRIMARY KEY (contextid);


--
-- Name: resourceattribute resourceattribute_pkey; Type: CONSTRAINT; Schema: consent; Owner: -
--

ALTER TABLE ONLY consent.resourceattribute
    ADD CONSTRAINT resourceattribute_pkey PRIMARY KEY (consentrightid);


--
-- Name: delegationchanges delegationchanges_pkey; Type: CONSTRAINT; Schema: delegation; Owner: -
--

ALTER TABLE ONLY delegation.delegationchanges
    ADD CONSTRAINT delegationchanges_pkey PRIMARY KEY (delegationchangeid);


--
-- Name: resourceregistrydelegationchanges resourceregistrydelegationchanges_pkey; Type: CONSTRAINT; Schema: delegation; Owner: -
--

ALTER TABLE ONLY delegation.resourceregistrydelegationchanges
    ADD CONSTRAINT resourceregistrydelegationchanges_pkey PRIMARY KEY (resourceregistrydelegationchangeid);


--
-- Name: idx_resource_resourcereferenceid; Type: INDEX; Schema: accessmanagement; Owner: -
--

CREATE INDEX idx_resource_resourcereferenceid ON accessmanagement.resource USING btree (resourceregistryid) INCLUDE (resourcetype);


--
-- Name: idx_consentevent_consentrequestid_created; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_consentevent_consentrequestid_created ON consent.consentevent USING btree (consentrequestid, created);


--
-- Name: idx_consentrequest_frompartyuuid; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_consentrequest_frompartyuuid ON consent.consentrequest USING btree (frompartyuuid);


--
-- Name: idx_consentrequest_frompartyuuid_status_portal_show; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_consentrequest_frompartyuuid_status_portal_show ON consent.consentrequest USING btree (frompartyuuid, status) WHERE (portalviewmode = 'show'::consent.portal_view_mode);


--
-- Name: idx_consentrequest_status; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_consentrequest_status ON consent.consentrequest USING btree (status);


--
-- Name: idx_consentright_consentrequestid; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_consentright_consentrequestid ON consent.consentright USING btree (consentrequestid);


--
-- Name: idx_context_consentrequestid; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_context_consentrequestid ON consent.context USING btree (consentrequestid);


--
-- Name: idx_metadata_consentrightid; Type: INDEX; Schema: consent; Owner: -
--

CREATE INDEX idx_metadata_consentrightid ON consent.metadata USING btree (consentrightid);


--
-- Name: idx_altinnappid; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_altinnappid ON delegation.delegationchanges USING btree (altinnappid);


--
-- Name: idx_coveredbyparty; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_coveredbyparty ON delegation.delegationchanges USING btree (coveredbypartyid);


--
-- Name: idx_coveredbyuser; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_coveredbyuser ON delegation.delegationchanges USING btree (coveredbyuserid);


--
-- Name: idx_delegation_from; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_delegation_from ON delegation.delegationchanges USING btree (fromuuid, fromtype);


--
-- Name: idx_delegation_to; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_delegation_to ON delegation.delegationchanges USING btree (touuid, totype);


--
-- Name: idx_instancedelegation_from; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_instancedelegation_from ON delegation.instancedelegationchanges USING btree (fromuuid, fromtype);


--
-- Name: idx_instancedelegation_to; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_instancedelegation_to ON delegation.instancedelegationchanges USING btree (touuid, totype);


--
-- Name: idx_offeredby; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_offeredby ON delegation.delegationchanges USING btree (offeredbypartyid);


--
-- Name: idx_rrdelegation_coveredbyparty; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_rrdelegation_coveredbyparty ON delegation.resourceregistrydelegationchanges USING btree (coveredbypartyid);


--
-- Name: idx_rrdelegation_coveredbyuser; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_rrdelegation_coveredbyuser ON delegation.resourceregistrydelegationchanges USING btree (coveredbyuserid);


--
-- Name: idx_rrdelegation_from; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_rrdelegation_from ON delegation.resourceregistrydelegationchanges USING btree (fromuuid, fromtype);


--
-- Name: idx_rrdelegation_offeredby; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_rrdelegation_offeredby ON delegation.resourceregistrydelegationchanges USING btree (offeredbypartyid);


--
-- Name: idx_rrdelegation_to; Type: INDEX; Schema: delegation; Owner: -
--

CREATE INDEX idx_rrdelegation_to ON delegation.resourceregistrydelegationchanges USING btree (touuid, totype);


--
-- Name: resourceregistrydelegationchanges resourceregistrydelegationchanges_resourceid_fk; Type: FK CONSTRAINT; Schema: delegation; Owner: -
--

ALTER TABLE ONLY delegation.resourceregistrydelegationchanges
    ADD CONSTRAINT resourceregistrydelegationchanges_resourceid_fk FOREIGN KEY (resourceid_fk) REFERENCES accessmanagement.resource(resourceid);


--
-- Name: SCHEMA accessmanagement; Type: ACL; Schema: -; Owner: -
--

GRANT USAGE ON SCHEMA accessmanagement TO platform_authorization;


--
-- Name: SCHEMA consent; Type: ACL; Schema: -; Owner: -
--

GRANT USAGE ON SCHEMA consent TO platform_authorization;


--
-- Name: SCHEMA delegation; Type: ACL; Schema: -; Owner: -
--

GRANT USAGE ON SCHEMA delegation TO platform_authorization;


--
-- Name: TABLE resource; Type: ACL; Schema: accessmanagement; Owner: -
--

GRANT ALL ON TABLE accessmanagement.resource TO platform_authorization;


--
-- Name: SEQUENCE resource_resourceid_seq; Type: ACL; Schema: accessmanagement; Owner: -
--

GRANT ALL ON SEQUENCE accessmanagement.resource_resourceid_seq TO platform_authorization;


--
-- Name: TABLE consentevent; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.consentevent TO platform_authorization;


--
-- Name: TABLE consentrequest; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.consentrequest TO platform_authorization;


--
-- Name: TABLE consentright; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.consentright TO platform_authorization;


--
-- Name: TABLE context; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.context TO platform_authorization;


--
-- Name: TABLE metadata; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.metadata TO platform_authorization;


--
-- Name: TABLE resourceattribute; Type: ACL; Schema: consent; Owner: -
--

GRANT ALL ON TABLE consent.resourceattribute TO platform_authorization;


--
-- Name: TABLE delegationchanges; Type: ACL; Schema: delegation; Owner: -
--

GRANT ALL ON TABLE delegation.delegationchanges TO platform_authorization;


--
-- Name: SEQUENCE delegationchanges_delegationchangeid_seq; Type: ACL; Schema: delegation; Owner: -
--

GRANT ALL ON SEQUENCE delegation.delegationchanges_delegationchangeid_seq TO platform_authorization;


--
-- Name: TABLE instancedelegationchanges; Type: ACL; Schema: delegation; Owner: -
--

GRANT ALL ON TABLE delegation.instancedelegationchanges TO platform_authorization;


--
-- Name: TABLE resourceregistrydelegationchanges; Type: ACL; Schema: delegation; Owner: -
--

GRANT ALL ON TABLE delegation.resourceregistrydelegationchanges TO platform_authorization;


--
-- Name: SEQUENCE resourceregistrydelegationcha_resourceregistrydelegationcha_seq; Type: ACL; Schema: delegation; Owner: -
--

GRANT ALL ON SEQUENCE delegation.resourceregistrydelegationcha_resourceregistrydelegationcha_seq TO platform_authorization;


--
