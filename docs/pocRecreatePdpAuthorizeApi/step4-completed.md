# Step 4: Resource/Instance Delegation Resolution via ConnectionQuery — Completed

## Summary

Step 4 is **merged into Step 3**. The `ConnectionQuery` call in `AuthorizationContextService.GetConnections()` retrieves delegation information alongside roles and access packages in a single database round-trip.

## How Delegations Are Retrieved

The `ConnectionQueryFilter` configured in Step 3 includes:
- `IncludeDelegation = true` — connections originating from delegations are included
- `IncludeResources = true` — delegation records are enriched with resource details (resource ID, name, RefId)
- `IncludeInstances = true` — instance delegation details are included

## Connection Data for Delegations

Each `ConnectionQueryExtendedRecord` contains:
- `Reason` — indicates whether the connection is from `Assignment`, `Delegation`, `Hierarchy`, or `KeyRole`
- `DelegationId` — non-null for delegation-based connections
- `Resources` — list of `ConnectionQueryResource` with `Id`, `Name`, and `RefId` (the resource registry identifier)
- `Instances` — list of `ConnectionQueryInstance` for instance-level delegations

## Replacing the Old Delegation Flow

| Old Flow | New Flow |
|---|---|
| `IAccessManagementWrapper.GetAllDelegationChanges()` (HTTP call) | `ConnectionQuery.GetConnectionsFromOthersAsync()` with `IncludeDelegation = true` |
| Filter by `IsTypeApp` / `IsTypeResource` | Connection records already associated with specific resources via `Resources[]` |
| Filter by instance ID matching | Connection records filtered by `Instances[]` or `InstanceIds` filter |
| `IPolicyRetrievalPoint.GetPolicyVersionAsync()` for each delegation | **Kept as-is** — policies remain in blob storage |

## No Separate Service Needed

As noted in the updated plan, no separate `IDelegationResolutionService` was created. The unified `ConnectionQuery` approach handles roles, packages, and delegations in one call.

## Build Status

✅ No additional code changes — covered by Step 3 implementation.
