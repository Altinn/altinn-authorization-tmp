-- Function: select_delegationchanges_by_id_range
CREATE OR REPLACE FUNCTION delegation.select_delegationchanges_by_id_range(
	_startid bigint,
	_endid bigint DEFAULT '9223372036854775807'::bigint)
    RETURNS SETOF delegation.delegationchanges 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$

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
    delegationChangeId BETWEEN _startId AND _endId
$BODY$;