# Step 3: Connection-Based Role/AccessPackage/Delegation Resolution — Completed

## Summary

Created `IAuthorizationContextService` and `AuthorizationContextService` in `Altinn.AccessMgmt.Core` to replace the old `ContextHandler`, `DelegationContextHandler`, and all their external API dependencies with local Entity Framework lookups using `IEntityService` and `ConnectionQuery`.

## Files Created

| File | Description |
|---|---|
| `Services/Contracts/IAuthorizationContextService.cs` | Interface defining entity resolution and connection lookup methods |
| `Services/AuthorizationContextService.cs` | Implementation using `IEntityService` and `ConnectionQuery` |

## Interface: `IAuthorizationContextService`

### Entity Resolution Methods
These replace `IRegisterService`, `IProfile`, and `IParties` calls:

| Method | Replaces |
|---|---|
| `ResolveEntityByOrgNo(string orgNo)` | `IRegisterService.PartyLookup(orgNo, null)` |
| `ResolveEntityByPersonId(string personId)` | `IRegisterService.PartyLookup(null, ssn)` |
| `ResolveEntityByPartyId(int partyId)` | `IRegisterService.GetParty(partyId)` |
| `ResolveEntityByUserId(int userId)` | `IProfile.GetUserProfile(userId)` → party info |
| `ResolveEntityByUuid(Guid partyUuid)` | `IRegisterService.GetPartiesAsync(List<Guid>)` |

### Connection Lookup Method

```csharp
Task<List<ConnectionQueryExtendedRecord>> GetConnections(
    Guid resourcePartyUuid,          // From (grantor)
    IReadOnlyCollection<Guid> subjectPartyUuids,  // To (recipients, including keyrole parties)
    CancellationToken cancellationToken);
```

**Replaces** (in a single query):
- `IRoles.GetDecisionPointRolesForUser()` — roles come from `ConnectionQueryRecord.Role`
- `IAccessManagementWrapper.GetAccessPackages()` — packages come from `ConnectionQueryExtendedRecord.Packages`
- `IAccessManagementWrapper.GetAllDelegationChanges()` — delegations come from `ConnectionQueryExtendedRecord.Resources` / `Instances`

**Direction mapping:**
- `FromIds` = resource party UUID (the **grantor** of access)
- `ToIds` = subject party UUID(s) (the **recipient** of access)

**Filter configuration:**
- `IncludePackages = true` — retrieves access packages
- `IncludeResources = true` — retrieves resource delegations
- `IncludeInstances = true` — retrieves instance delegations
- `IncludeDelegation = true` — includes delegation-based connections
- `IncludeKeyRole = true` — includes keyrole-derived connections
- `ExcludeDeleted = true` — filters out soft-deleted entities

## Connection Data Mapping to XACML Attributes

The `ConnectionQueryExtendedRecord` data maps to XACML enrichment as follows:

| Connection Data | XACML Attribute | Old Source |
|---|---|---|
| `record.Role.Urn` / `record.Role.Code` | `urn:altinn:rolecode` | `IRoles` (SBL Bridge) |
| `record.Packages[].Urn` | `urn:altinn:accesspackage` | `IAccessManagementWrapper.GetAccessPackages()` |
| `record.Resources[]` | Delegation policy path for PDP eval | `IAccessManagementWrapper.GetAllDelegationChanges()` |
| `record.Instances[]` | Instance delegation policy path | `IAccessManagementWrapper.GetAllDelegationChanges()` |

## External Dependency Retained: OED Roles

`IOedRoleAssignmentWrapper` is **not** replaced by this service. OED role lookups remain as an external call, conditionally invoked only when the policy contains `urn:digitaltdodsbo:rolecode` attributes. This will be handled in the `AuthorizationDecisionService` (Step 5).

## Build Status

✅ Build successful
