using System.Diagnostics;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessMgmt.Core.Services.Legacy;

/// <inheritdoc/>
public class DelegationMetadataEF : IDelegationMetadataRepository
{
    private DelegationChange Convert(AssignmentResource assignmentResource)
    {
        return new DelegationChange()
        {
            FromUuid = assignmentResource.Assignment.FromId,
            ToUuid = assignmentResource.Assignment.ToId,
            PerformedByUuid = assignmentResource.Audit_ChangedBy.ToString(),
            Created = assignmentResource.Audit_ValidFrom.UtcDateTime,
            ResourceId = assignmentResource.ResourceId.ToString(),
            ResourceType = assignmentResource.Resource.Type.Name,
            //BlobStoragePolicyPath = assignmentResource.ddd,
            //BlobStorageVersionId = assignmentResource.ssss,
            //DelegationChangeId = assignmentResource.eeeee,
            //InstanceId = assignmentResource.qqqqq,
        };
    }

    //private DelegationChange Convert(AssignmentInstance assignmentInstance)
    //{
    //    return new DelegationChange()
    //    {
    //        FromUuid = assignmentInstance.Assignment.FromId,
    //        ToUuid = assignmentInstance.Assignment.ToId,
    //        PerformedByUuid = assignmentInstance.Audit_ChangedBy.ToString(),
    //        Created = assignmentInstance.Audit_ValidFrom.UtcDateTime,
    //        ResourceId = assignmentInstance.ResourceId.ToString(),
    //        ResourceType = assignmentInstance.Resource.Type.Name,
    //        BlobStoragePolicyPath = assignmentInstance.ddd,
    //        BlobStorageVersionId = assignmentInstance.ssss,
    //        DelegationChangeId = assignmentInstance.eeeee,
    //        InstanceId = assignmentInstance.qqqqq,
    //    };
    //}

    private async Task<DelegationChange> GetAssignmentResource(Guid id)
    {
        return Convert(await DbContext.AssignmentResources
            .Include(t => t.Assignment).ThenInclude(t => t.From)
            .Include(t => t.Assignment).ThenInclude(t => t.To)
            .Include(t => t.Resource).ThenInclude(t => t.Provider)
            .Where(t => t.Id == id)
            .SingleAsync(t => t.Id == id)
            );
    }

    private async Task<Resource> GetResource(string resourceIdentifier, CancellationToken cancellationToken = default)
    {
        return await DbContext.Resources.AsNoTracking().SingleAsync(t => t.RefId == resourceIdentifier, cancellationToken);
    }

    private async Task<List<Resource>> GetResource(List<string> resourceIdentifier, CancellationToken cancellationToken = default)
    {
        return await DbContext.Resources.AsNoTracking().Where(t => resourceIdentifier.Contains(t.RefId)).ToListAsync(cancellationToken);
    }

    private readonly NpgsqlDataSource _conn;
    private readonly string defaultAppColumns = "delegationChangeId, delegationChangeType, altinnAppId, offeredByPartyId, fromUuid, fromType, coveredByUserId, coveredByPartyId, toUuid, toType, performedByUserId, performedByUuid, performedByType, blobStoragePolicyPath, blobStorageVersionId, created";
    private const string FromUuid = "fromUuid";
    private const string FromType = "fromType";
    private const string ToUuid = "toUuid";
    private const string ToType = "toType";
    private const string PerformedByUuid = "performedByUuid";
    private const string PerformedByType = "performedByType";
    private const string InstanceId = "instanceId";
    private const string ResourceId = "resourceId";
    private const string DelegationChangeType = "delegationChangeType";

    public AppDbContext DbContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegationMetadataEF"/> class
    /// </summary>
    /// <param name="dbContext">AppDbContext</param>
    public DelegationMetadataEF(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(
        List<int> offeredByPartyIds, 
        List<string> altinnAppIds, 
        List<int> coveredByPartyIds = null, 
        List<int> coveredByUserIds = null, 
        CancellationToken cancellationToken = default
        )
    {
        if (offeredByPartyIds?.Count < 1)
        {
            throw new ArgumentNullException(nameof(offeredByPartyIds));
        }

        if (altinnAppIds?.Count < 1)
        {
            throw new ArgumentNullException(nameof(altinnAppIds));
        }

        if (coveredByPartyIds == null && coveredByUserIds == null)
        {
            throw new ArgumentException($"Both params: {nameof(coveredByUserIds)}, {nameof(coveredByPartyIds)} cannot be null.");
        }

        var result = await DbContext.AssignmentResources.AsNoTracking()
            .Include(t => t.Assignment).ThenInclude(t => t.From)
            .Include(t => t.Assignment).ThenInclude(t => t.To)
            .Include(t => t.Resource)
            .Where(t => altinnAppIds.Contains(t.Resource.RefId))
            .Where(t => t.Assignment.From.PartyId.HasValue && offeredByPartyIds.Contains(t.Assignment.From.PartyId.Value))
            .WhereIf(coveredByPartyIds != null && coveredByPartyIds.Any(), t => t.Assignment.To.PartyId.HasValue && coveredByPartyIds.Contains(t.Assignment.To.PartyId.Value))
            .WhereIf(coveredByUserIds != null && coveredByUserIds.Any(), t => t.Assignment.To.UserId.HasValue && coveredByUserIds.Contains(t.Assignment.To.UserId.Value))
            .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(
        List<string> altinnAppIds, 
        List<int> fromPartyIds, 
        UuidType toUuidType, 
        Guid toUuid, 
        CancellationToken cancellationToken = default
        )
    {
        if (altinnAppIds?.Count < 1)
        {
            throw new ArgumentNullException(nameof(altinnAppIds));
        }

        if (fromPartyIds?.Count < 1)
        {
            throw new ArgumentNullException(nameof(fromPartyIds));
        }

        if (toUuidType == UuidType.NotSpecified)
        {
            throw new ArgumentException($"Param: {nameof(toUuidType)} must be specified.");
        }

        if (toUuid == Guid.Empty)
        {
            throw new ArgumentException($"Param: {nameof(toUuid)} must be specified.");
        }

        var resourceUuids = altinnAppIds.Select(t => Guid.Parse(t));

        var result = await DbContext.AssignmentResources.AsNoTracking()
            .Include(t => t.Assignment).ThenInclude(t => t.From)
            .Include(t => t.Assignment).ThenInclude(t => t.To)
            .Where(t => resourceUuids.Contains(t.ResourceId))
            .Where(t => t.Assignment.From.PartyId.HasValue && fromPartyIds.Contains(t.Assignment.From.PartyId.Value))
            .Where(t => t.Assignment.ToId == toUuid)
            .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<DelegationChange> GetCurrentDelegationChange(ResourceAttributeMatchType resourceMatchType, string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            throw new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace.");
        }

        if (offeredByPartyId == 0)
        {
            throw new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero.");
        }

        if (coveredByPartyId == null && coveredByUserId == null && toUuidType != UuidType.SystemUser)
        {
            throw new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null.");
        }

        var resourceUuid = Guid.Parse(resourceId);

        var from = await DbContext.Entities.AsNoTracking().SingleAsync(t => t.PartyId == offeredByPartyId, cancellationToken);

        var result = await DbContext.AssignmentResources.AsNoTracking()
            .Include(t => t.Assignment).ThenInclude(t => t.To)
            .Where(t => t.ResourceId == resourceUuid)
            .Where(t => t.Assignment.FromId == from.Id)
            .WhereIf(coveredByPartyId != null, t => t.Assignment.To.PartyId == coveredByPartyId)
            .WhereIf(coveredByPartyId != null, t => t.Assignment.To.UserId == coveredByUserId)
            .WhereIf(toUuid.HasValue, t => t.Assignment.ToId == toUuid.Value)
            .SingleAsync(cancellationToken);

        return Convert(result);
    }

    /// <inheritdoc/>
    public async Task<DelegationChange> InsertDelegation(ResourceAttributeMatchType resourceMatchType, DelegationChange delegationChange, CancellationToken cancellationToken = default)
    {
        var role = RoleConstants.Rightholder;
        var from = await DbContext.Entities.AsNoTracking().SingleAsync(t => t.PartyId == delegationChange.OfferedByPartyId, cancellationToken);
        var to = await DbContext.Entities.AsNoTracking().SingleAsync(t => t.PartyId == delegationChange.CoveredByPartyId, cancellationToken);
        var resource = await DbContext.Resources.AsNoTracking().SingleAsync(t => t.RefId == delegationChange.ResourceId, cancellationToken);

        var assignment = await DbContext.Assignments.FirstOrDefaultAsync(t => t.FromId == from.Id && t.ToId == to.Id && t.RoleId == role.Id, cancellationToken);
        if (assignment == null)
        {
            assignment = new Assignment()
            {
                Id = Guid.CreateVersion7(),
                FromId = from.Id,
                ToId = to.Id,
                RoleId = role.Id
            };
            DbContext.Assignments.Add(assignment);
        }

        var assignmentResource = await DbContext.AssignmentResources.FirstOrDefaultAsync(t => t.AssignmentId == assignment.Id && t.ResourceId == resource.Id, cancellationToken);
        if (assignmentResource == null)
        {
            assignmentResource = new AssignmentResource()
            {
                Id = Guid.CreateVersion7(),
                AssignmentId = assignment.Id,
                ResourceId = resource.Id,
                // Policy...
            };
            DbContext.AssignmentResources.Add(assignmentResource);
        }

        await DbContext.SaveChangesAsync();

        return await GetAssignmentResource(assignmentResource.Id);
    }

    /// <summary>
    ///  Fetches all instance delegated to given param
    /// </summary>
    /// <param name="toUuid">list of parties that has received an instance delegation</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public async Task<List<InstanceDelegationChange>> GetAllCurrentReceivedInstanceDelegations(List<Guid> toUuid, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

        var query = /* strpsql */ @"
                WITH latestChanges AS (
                    SELECT
                        MAX(instancedelegationchangeid) as latestId
                    FROM
                        delegation.instancedelegationchanges
                    WHERE
                        touuid = ANY(@toUuid)
                    GROUP BY
                        touuid,
                        fromuuid,
                        resourceid,
                        instanceid
                )
                SELECT
                    instancedelegationchangeid,
                    delegationchangetype,
                    instanceDelegationMode,
                    resourceid,
                    instanceid,
                    fromuuid,
                    fromtype,
                    touuid,
                    totype,
                    performedby,
                    performedbytype,
                    blobstoragepolicypath,
                    blobstorageversionid,
                    created
                FROM
                    delegation.instancedelegationchanges
                INNER JOIN latestChanges
                    ON instancedelegationchangeid = latestChanges.latestId
                WHERE
                    delegationchangetype != 'revoke_last'
            ";

        try
        {
            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.AddWithValue("toUuid", NpgsqlDbType.Array | NpgsqlDbType.Uuid, toUuid);
            return await cmd.ExecuteEnumerableAsync(cancellationToken)
                .SelectAwait(GetInstanceDelegationChange)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<InstanceDelegationChange> GetLastInstanceDelegationChange(InstanceDelegationChangeRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

        string query = /*strpsql*/@"
            SELECT
                instancedelegationchangeid
                ,delegationchangetype
                ,instanceDelegationMode
                ,resourceId
                ,instanceId
                ,fromUuid
                ,fromType
                ,toUuid
                ,toType
                ,performedBy
                ,performedByType
                ,blobStoragePolicyPath
                ,blobStorageVersionId
                ,created
            FROM
                delegation.instancedelegationchanges
            WHERE
                resourceId = @resourceId
                AND instanceId = @instanceId
                AND instanceDelegationMode = @instanceDelegationMode
                AND fromUuid = @fromUuid
                AND fromType = @fromType
                AND toUuid = @toUuid
                AND toType = @toType
            ORDER BY
                instancedelegationchangeid DESC LIMIT 1;
            ";

        try
        {
            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, request.Resource);
            cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, request.Instance);
            cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationMode>("instancedelegationmode", request.InstanceDelegationMode));
            cmd.Parameters.AddWithValue(FromUuid, NpgsqlDbType.Uuid, request.FromUuid);
            cmd.Parameters.Add(new NpgsqlParameter<UuidType>(FromType, request.FromType));
            cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, request.ToUuid);
            cmd.Parameters.Add(new NpgsqlParameter<UuidType>(ToType, request.ToType));

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return await GetInstanceDelegationChange(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<InstanceDelegationChange> InsertInstanceDelegation(InstanceDelegationChange instanceDelegationChange, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

        string query = /*strpsql*/@"
            INSERT INTO delegation.instancedelegationchanges(
                delegationchangetype
                ,instanceDelegationMode
                ,resourceId
                ,instanceid
                ,fromUuid
                ,fromType
                ,toUuid
                ,toType
                ,performedBy
                ,performedByType
                ,blobStoragePolicyPath
                ,blobStorageVersionId)
            VALUES (
                @delegationchangetype
                ,@instanceDelegationMode
                ,@resourceId
                ,@instanceid
                ,@fromUuid
                ,@fromType
                ,@toUuid
                ,@toType
                ,@performedBy
                ,@performedByType
                ,@blobStoragePolicyPath
                ,@blobStorageVersionId)
            RETURNING *;
            ";

        try
        {
            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.Add(new NpgsqlParameter<DelegationChangeType>(DelegationChangeType, instanceDelegationChange.DelegationChangeType));
            cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationMode>("instancedelegationmode", instanceDelegationChange.InstanceDelegationMode));
            cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, instanceDelegationChange.ResourceId);
            cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, instanceDelegationChange.InstanceId);
            cmd.Parameters.AddWithValue(FromUuid, NpgsqlDbType.Uuid, instanceDelegationChange.FromUuid);
            cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(FromType, instanceDelegationChange.FromUuidType != UuidType.NotSpecified ? instanceDelegationChange.FromUuidType : null));
            cmd.Parameters.AddWithValue(ToUuid, NpgsqlDbType.Uuid, instanceDelegationChange.ToUuid);
            cmd.Parameters.Add(new NpgsqlParameter<UuidType?>(ToType, instanceDelegationChange.ToUuidType != UuidType.NotSpecified ? instanceDelegationChange.ToUuidType : null));
            cmd.Parameters.AddWithValue("performedBy", NpgsqlDbType.Text, instanceDelegationChange.PerformedBy);
            cmd.Parameters.Add(new NpgsqlParameter<UuidType?>("performedByType", instanceDelegationChange.PerformedByType != UuidType.NotSpecified ? instanceDelegationChange.PerformedByType : null));
            cmd.Parameters.AddWithValue("blobStoragePolicyPath", NpgsqlDbType.Text, instanceDelegationChange.BlobStoragePolicyPath);
            cmd.Parameters.AddWithValue("blobStorageVersionId", NpgsqlDbType.Text, instanceDelegationChange.BlobStorageVersionId);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return await GetInstanceDelegationChange(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> InsertMultipleInstanceDelegations(List<PolicyWriteOutput> policyWriteOutputs, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
        NpgsqlConnection connection = null;

        try
        {
            connection = await _conn.OpenConnectionAsync(cancellationToken);

            using (var writer = await connection.BeginBinaryImportAsync("copy delegation.instancedelegationchanges (delegationchangetype, instancedelegationmode, resourceid, instanceid, fromuuid, fromtype, touuid, totype, performedby, performedbytype, blobstoragepolicypath, blobstorageversionid, instancedelegationsource) from STDIN (FORMAT BINARY)", cancellationToken))
            {
                foreach (var record in policyWriteOutputs)
                {
                    await writer.StartRowAsync(cancellationToken);
                    await writer.WriteAsync(record.ChangeType, cancellationToken);
                    await writer.WriteAsync(record.Rules.InstanceDelegationMode, cancellationToken);
                    await writer.WriteAsync(record.Rules.ResourceId, NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(record.Rules.InstanceId, NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(record.Rules.FromUuid, NpgsqlDbType.Uuid, cancellationToken);
                    await writer.WriteAsync(record.Rules.FromType, cancellationToken);
                    await writer.WriteAsync(record.Rules.ToUuid, NpgsqlDbType.Uuid, cancellationToken);
                    await writer.WriteAsync(record.Rules.ToType, cancellationToken);
                    await writer.WriteAsync(record.Rules.PerformedBy, NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(record.Rules.PerformedByType, cancellationToken);
                    await writer.WriteAsync(record.PolicyPath, NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(record.VersionId, NpgsqlDbType.Text, cancellationToken);
                    await writer.WriteAsync(record.Rules.InstanceDelegationSource, cancellationToken);
                }

                await writer.CompleteAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            return false;
        }
        finally
        {
            if (connection != null)
            {
                await connection.CloseAsync();
            }
        }
    }

    /// <inheritdoc />
    public async Task<List<InstanceDelegationChange>> GetAllLatestInstanceDelegationChanges(InstanceDelegationSource source, string resourceID, string instanceID, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

        string query = /*strpsql*/@"
            WITH LatestChanges AS(
		    SELECT 
			    MAX(instancedelegationchangeid) instancedelegationchangeid
		    FROM
			    delegation.instancedelegationchanges
		    WHERE
			    instancedelegationsource = @source
                AND resourceid = @resourceId
			    AND instanceid = @instanceId
		    GROUP BY
			    instancedelegationmode
			    ,resourceid
			    ,instanceid
			    ,fromuuid
			    ,fromtype
			    ,touuid
			    ,totype)
            SELECT
	            dc.instancedelegationchangeid
	            ,delegationchangetype
	            ,instancedelegationmode
	            ,resourceid
	            ,instanceid
	            ,fromuuid
	            ,fromtype
	            ,touuid
	            ,totype
	            ,performedby
	            ,performedbytype
	            ,blobstoragepolicypath
	            ,blobstorageversionid
	            ,created
            FROM
	            LatestChanges lc
	            JOIN delegation.instancedelegationchanges dc ON lc.instancedelegationchangeid = dc.instancedelegationchangeid
            WHERE
                delegationchangetype != 'revoke_last';";

        try
        {
            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.Add(new NpgsqlParameter<InstanceDelegationSource>("source", source));
            cmd.Parameters.AddWithValue(ResourceId, NpgsqlDbType.Text, resourceID);
            cmd.Parameters.AddWithValue(InstanceId, NpgsqlDbType.Text, instanceID);

            return await cmd.ExecuteEnumerableAsync(cancellationToken)
                .SelectAwait(GetInstanceDelegationChange)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InstanceDelegationChange>> GetActiveInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);

        string query = /*strpsql*/@"
            WITH LatestChanges AS(
		    SELECT 
			    MAX(instancedelegationchangeid) instancedelegationchangeid
		    FROM
			    delegation.instancedelegationchanges
		    WHERE
                resourceid = ANY(@resourceIds)
			    AND fromuuid = @fromUuid
                AND touuid = ANY(@toUuids)
		    GROUP BY
			    instancedelegationmode
			    ,resourceid
			    ,instanceid
			    ,fromuuid
			    ,fromtype
			    ,touuid
			    ,totype)
            SELECT
	            dc.instancedelegationchangeid
	            ,delegationchangetype
	            ,instancedelegationmode
	            ,resourceid
	            ,instanceid
	            ,fromuuid
	            ,fromtype
	            ,touuid
	            ,totype
	            ,performedby
	            ,performedbytype
	            ,blobstoragepolicypath
	            ,blobstorageversionid
	            ,created
            FROM
	            LatestChanges lc
	            JOIN delegation.instancedelegationchanges dc ON lc.instancedelegationchangeid = dc.instancedelegationchangeid
            WHERE delegationchangetype != 'revoke_last';";

        try
        {
            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.AddWithValue("resourceIds", NpgsqlDbType.Array | NpgsqlDbType.Text, resourceIds);
            cmd.Parameters.AddWithValue("fromUuid", NpgsqlDbType.Uuid, from);
            cmd.Parameters.AddWithValue("toUuids", NpgsqlDbType.Array | NpgsqlDbType.Uuid, to);

            return await cmd.ExecuteEnumerableAsync(cancellationToken)
                .SelectAwait(GetInstanceDelegationChange)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.StopWithError(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default)
    {
        if (offeredByPartyIds?.Count == 0)
        {
            throw new ArgumentNullException(nameof(offeredByPartyIds));
        }

        List<DelegationChange> delegationChanges = new List<DelegationChange>();

        if (coveredByPartyIds?.Count > 0)
        {
            delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyIds, offeredByPartyIds, resourceRegistryIds, cancellationToken: cancellationToken));
        }

        if (coveredByUserId.HasValue)
        {
            delegationChanges.AddRange(await GetReceivedResourceRegistryDelegationsForCoveredByUser(coveredByUserId.Value, offeredByPartyIds, resourceRegistryIds, cancellationToken: cancellationToken));
        }

        return delegationChanges;
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(List<string> resourceRegistryIds, List<int> fromPartyIds, UuidType toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
    {
        return await GetAllCurrentAppDelegationChanges(
            altinnAppIds: resourceRegistryIds,
            fromPartyIds: fromPartyIds,
            toUuidType: toUuidType,
            toUuid: toUuid,
            cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
           .Include(t => t.Assignment).ThenInclude(t => t.From)
           .Include(t => t.Assignment).ThenInclude(t => t.To)
           .Include(t => t.Resource)
           .Where(t => t.Assignment.From.PartyId.HasValue && t.Assignment.From.PartyId.Value == offeredByPartyId)
           .WhereIf(resourceRegistryIds != null && resourceRegistryIds.Any(), t => resourceRegistryIds.Contains(t.Resource.RefId))
           .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
           .Include(t => t.Assignment).ThenInclude(t => t.From)
           .Include(t => t.Assignment).ThenInclude(t => t.To)
           .Include(t => t.Resource)
           .Where(t => t.Assignment.To.PartyId.HasValue && coveredByPartyIds.Contains(t.Assignment.To.PartyId.Value))
           .WhereIf(resourceRegistryIds != null && resourceRegistryIds.Any(), t => resourceRegistryIds.Contains(t.Resource.RefId))
           .WhereIf(offeredByPartyIds != null && offeredByPartyIds.Any(), t => t.Assignment.From.PartyId.HasValue && offeredByPartyIds.Contains(t.Assignment.From.PartyId.Value))
           .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
           .Include(t => t.Assignment).ThenInclude(t => t.From)
           .Include(t => t.Assignment).ThenInclude(t => t.To)
           .Include(t => t.Resource)
           .Where(t => t.Assignment.To.UserId.HasValue && t.Assignment.To.UserId.Value == coveredByUserId)
           .Where(t => t.Assignment.From.PartyId.HasValue && offeredByPartyIds.Contains(t.Assignment.From.PartyId.Value))
           .WhereIf(resourceRegistryIds != null && resourceRegistryIds.Any(), t => resourceRegistryIds.Contains(t.Resource.RefId))
           .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
           .Include(t => t.Assignment).ThenInclude(t => t.From)
           .Include(t => t.Assignment).ThenInclude(t => t.To)
           .Include(t => t.Resource)
           .Where(t => t.Assignment.From.PartyId.HasValue && t.Assignment.From.PartyId.Value == offeredByPartyId)
           .Where(t => t.Assignment.To.PartyId.HasValue && t.Assignment.To.PartyId.Value == coveredByPartyId)
           .Where(t => resourceIds.Contains(t.Resource.RefId))
           .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
           .Include(t => t.Assignment).ThenInclude(t => t.From)
           .Include(t => t.Assignment).ThenInclude(t => t.To)
           .Include(t => t.Resource)
           .Where(t => t.Assignment.From.PartyId.HasValue && offeredByPartyIds.Contains(t.Assignment.From.PartyId.Value))
           .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
    {
        /*
        Intended?
        var simpleResult = await DbContext.AssignmentResources.AsNoTracking()
          .Include(t => t.Assignment).ThenInclude(t => t.From)
          .Include(t => t.Assignment).ThenInclude(t => t.To)
          .Include(t => t.Resource)
          .WhereIf(coveredByPartyIds != null && coveredByPartyIds.Any(), t => t.Assignment.To.PartyId.HasValue && coveredByPartyIds.Contains(t.Assignment.To.PartyId.Value))
          .WhereIf(coveredByUserIds != null && coveredByUserIds.Any(), t => t.Assignment.To.UserId.HasValue && coveredByUserIds.Contains(t.Assignment.To.UserId.Value))
          .ToListAsync(cancellationToken);
        */

        var partyChanges = DbContext.AssignmentResources.AsNoTracking()
          .Include(t => t.Assignment).ThenInclude(t => t.From)
          .Include(t => t.Assignment).ThenInclude(t => t.To)
          .Include(t => t.Resource)
          .Where(t => t.Assignment.To.PartyId.HasValue && coveredByPartyIds.Contains(t.Assignment.To.PartyId.Value));

        var userChanges = DbContext.AssignmentResources.AsNoTracking()
          .Include(t => t.Assignment).ThenInclude(t => t.From)
          .Include(t => t.Assignment).ThenInclude(t => t.To)
          .Include(t => t.Resource)
          .Where(t => t.Assignment.To.UserId.HasValue && coveredByUserIds.Contains(t.Assignment.To.UserId.Value));

        var result = await partyChanges.Union(userChanges).ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(List<Guid> toPartyUuids, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AssignmentResources.AsNoTracking()
          .Include(t => t.Assignment).ThenInclude(t => t.From)
          .Include(t => t.Assignment).ThenInclude(t => t.To)
          .Include(t => t.Resource)
          .Where(t => toPartyUuids.Contains(t.Assignment.ToId))
          .ToListAsync(cancellationToken);

        return result.Select(Convert).ToList();
    }
}
