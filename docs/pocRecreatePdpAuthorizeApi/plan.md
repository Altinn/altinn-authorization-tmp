# Plan: Recreate Authorization Authorize Endpoint in AccessManagement

## Background

The existing `POST authorization/api/v1/authorize` endpoint lives in the **Altinn.Authorization** project's `DecisionController`. It accepts an external XACML JSON authorization request, enriches it with context (roles, access packages, party info, delegations), evaluates it against XACML policies using a Policy Decision Point (PDP), and returns an authorization decision.

The current implementation has several issues:
- **Heavy controller**: Most business logic (context enrichment, delegation lookup, PDP evaluation, access list checks, metric recording) lives directly in the controller or tightly coupled handler classes.
- **External API dependencies**: Party lookups go to the Register API, roles come from SBL Bridge (`IRoles`), access packages come from the AccessManagement PIP API (`IAccessManagementWrapper`), user profiles from the Profile API (`IProfile`), and delegations from the AccessManagement API.
- **Legacy Altinn 2 integrations**: `IRoles` (SBL Bridge), `IParties` (SBL Bridge) are Altinn 2 dependencies that need replacement.
- **External dependency to keep**: `IOedRoleAssignmentWrapper` is an external (non-Altinn 2) dependency for OED role lookups that must be retained. The existing pattern of only calling it when the policy contains OED role attributes should be preserved.

The new implementation will live in **Altinn.AccessManagement.Api.ServiceOwner** with business logic in **Altinn.AccessMgmt.Core**, replacing external API calls with local Entity Framework database lookups.

---

## Step-by-Step Plan

### Step 1: Define the Authorization Request/Response Models

**Project:** `Altinn.Authorization.Api.Contracts`

Create clean request and response DTO models in the shared contracts library under a new `Authorization` folder/namespace (`Altinn.Authorization.Api.Contracts.Authorization`). These should be functionally equivalent to `XacmlJsonRequestRootExternal` / `XacmlJsonResponseExternal` from the old implementation but defined as shared contracts consumable by both the API host and any client.

**Files to create (in `src/libs/Altinn.Authorization.Api.Contracts/src/Altinn.Authorization.Api.Contracts/Authorization/`):**
- `AuthorizeRequest.cs` — The incoming authorization request DTO
- `AuthorizeResponse.cs` — The authorization response DTO
- `AuthorizeResult.cs` — Individual result per request (decision, status, obligations)
- `AuthorizeRequestCategory.cs` — Subject/Resource/Action category DTO
- `AuthorizeRequestAttribute.cs` — Single attribute (id + value) DTO

**Design decisions:**
- Keep XACML JSON profile format for backward compatibility with existing consumers.
- Internally, the service layer converts these DTOs to/from the ABAC library types (`XacmlJsonRequest`, `XacmlJsonResponse`) for policy evaluation.
- The contracts project already references `Altinn.Urn` which can be used for typed identifiers in the DTOs.

---

### Step 2: Create the Entity Resolution Service

**Project:** `Altinn.AccessMgmt.Core`

Replace all calls to `IRegisterService` (Register API) with local Entity lookups using `IEntityService` and `AppDbContext`.

**Capabilities needed (already exist in `EntityService`):**
- `GetByOrgNo(string orgNo)` — replaces `_registerService.PartyLookup(orgNo, null)`
- `GetByPersNo(string persNo)` — replaces `_registerService.PartyLookup(null, ssn)`
- `GetByPartyId(int partyId)` — replaces `_registerService.GetParty(partyId)`
- `GetByUserId(int userId)` — replaces `_profileWrapper.GetUserProfile(userId)` for getting party/uuid info
- `GetEntity(Guid id)` — replaces `_registerService.GetPartiesAsync(List<Guid>)`

**Gap analysis:** The existing `IEntityService` already covers most of the party/entity resolution needs. The `Entity` model has `PartyId`, `OrganizationIdentifier`, `PersonIdentifier`, `UserId`, `ParentId` (for hierarchy/keyrole), and `Id` (which is the party UUID). No new service needed — `IEntityService` is sufficient.

---

### Step 3: Create the Connection-Based Role/AccessPackage Resolution

**Project:** `Altinn.AccessMgmt.Core`

Replace lookups of roles (`IRoles` / SBL Bridge) and access packages (`IAccessManagementWrapper`) with `ConnectionQuery`.

**Current flow in `ContextHandler.EnrichSubjectAttributes`:**
1. Get roles for subject user → resource party via `IRoles.GetDecisionPointRolesForUser(userId, partyId)`
2. Get access packages for subject → resource party via `IAccessManagementWrapper.GetAccessPackages(subjectUuid, partyUuid)`

**New flow using `ConnectionQuery`:**
1. Use `ConnectionQuery.GetConnectionsFromOthersAsync()` with a `ConnectionQueryFilter` that:
   - Sets `ToIds` = [subject party UUID] (and keyrole party UUIDs) — the subject is the *recipient* of access
   - Sets `FromIds` = [resource party UUID] — the resource party is the *grantor* of access
   - Sets `IncludePackages = true` for access package enrichment
   - Retrieves both role-based assignments and package-based connections

2. From the connection results, extract:
   - **Roles**: From assignment records that have role information
   - **Access Packages**: From the `Packages` property on connection records

3. **OED Roles** (external lookup — kept as-is):
   - If the policy contains OED role attributes (`AltinnXacmlConstants.MatchAttributeIdentifiers.OedRoleAttribute`), call `IOedRoleAssignmentWrapper` to get OED role assignments between subject SSN and resource party SSN.
   - This is an external dependency that is NOT an Altinn 2 integration and must be retained.
   - Same conditional pattern as current `ContextHandler.EnrichSubjectAttributes`: only invoke if the policy requires it and both subject SSN and resource party SSN are available.

**Files to create:**
- `Services/Contracts/IAuthorizationContextService.cs` — Interface for context enrichment
- `Services/AuthorizationContextService.cs` — Implementation replacing `ContextHandler` + `DelegationContextHandler`

**Key methods:**
- `ResolveSubjectEntity(request)` — Resolve subject from userId/ssn/orgNo/partyUuid to Entity
- `ResolveResourcePartyEntity(request)` — Resolve resource party from partyId/orgNo/ssn/partyUuid to Entity
- `GetRolesForSubject(subjectEntity, resourcePartyEntity)` — Get roles via ConnectionQuery
- `GetAccessPackagesForSubject(subjectEntity, resourcePartyEntity)` — Get access packages via ConnectionQuery
- `GetKeyRoleParties(subjectEntity)` — Get keyrole parties via Entity parent/assignment relationships

---

### Step 4: Resolve Resource/Instance Delegations via ConnectionQuery

**Project:** `Altinn.AccessMgmt.Core`

Replace the delegation lookup that currently goes through `IAccessManagementWrapper.GetAllDelegationChanges()` (an HTTP API call to AccessManagement itself) with `ConnectionQuery`, unifying all access lookups (roles, access packages, and delegations including resource/instance delegations) into a single query mechanism.

**Current flow:**
1. `DecisionController.AuthorizeUsingDelegations()` calls `_accessManagement.GetAllDelegationChanges()` (HTTP to AM PIP)
2. For each delegation, retrieves the delegation policy from blob storage via `_prp.GetPolicyVersionAsync()`
3. Evaluates each delegation policy with the PDP

**New flow using `ConnectionQuery`:**
- Use `ConnectionQuery.GetConnectionsFromOthersAsync()` with a `ConnectionQueryFilter` configured to include delegations:
  - `ToIds` = [subject party UUID(s) including keyrole parties] — subject is the recipient of access
  - `FromIds` = [resource party UUID] — resource party is the grantor of access
  - `IncludeDelegation = true` — includes resource and instance delegations
  - `IncludeResources = true` — enriches delegation records with resource details
  - `IncludeInstances = true` — enriches with instance delegation details
  - `ResourceIds` = [resource UUID] — filters to only relevant resource delegations
  - `InstanceIds` = [instance ID] — filters to relevant instance delegations (if applicable)

- This means **all access information** (roles via assignments, access packages, and delegations) can be retrieved in a single `ConnectionQuery` call in Step 3, eliminating the need for a separate delegation lookup step.

**Merged approach:** Steps 3 and 4 can share the same `ConnectionQuery` call. The `ConnectionQueryFilter` supports `IncludePackages`, `IncludeResources`, and `IncludeInstances` simultaneously, enabling a single database round-trip to retrieve:
  - Role-based assignments (for XACML role attribute enrichment)
  - Access packages (for access package attribute enrichment)
  - Resource delegations (replacing `GetAllDelegationChanges`)
  - Instance delegations (replacing instance-specific delegation filtering)

**Policy retrieval:** For delegations that require policy evaluation, `IPolicyRetrievalPoint` remains as-is since policies are stored in blob storage. The delegation records from ConnectionQuery provide the blob storage path and version needed for `GetPolicyVersionAsync()`.

**Files to create/extend:**
- Extend `Services/AuthorizationContextService.cs` from Step 3 to also handle delegation resolution
- No separate `IDelegationResolutionService` needed — the unified ConnectionQuery approach handles it

---

### Step 5: Create the Authorization Decision Service

**Project:** `Altinn.AccessMgmt.Core`

This is the main orchestrator service that replaces all the logic currently in `DecisionController.Authorize()`.

**Files to create:**
- `Services/Contracts/IAuthorizationDecisionService.cs`
- `Services/AuthorizationDecisionService.cs`

**Responsibilities:**
1. Parse and validate the incoming authorization request
2. Enrich the request context using `IAuthorizationContextService` (Step 3)
3. Retrieve the resource policy via `IPolicyRetrievalPoint`
4. Evaluate the request against the policy using `PolicyDecisionPoint` (ABAC library)
5. If role-based decision is "NotApplicable", evaluate delegations using the delegation data already retrieved from `ConnectionQuery` (Step 3/4)
6. Check access list authorization if applicable (using local data or existing `IAccessListAuthorization`)
7. Return the final authorization decision

**Key method:**
```csharp
Task<XacmlJsonResponse> AuthorizeAsync(XacmlJsonRequest request, CancellationToken ct);
```

**Flow:**
```
Request → Enrich Context → Get Policy → PDP (roles) 
  → if NotApplicable → PDP (delegations) 
  → if Permit → Check Access Lists 
  → Return Decision
```

---

### Step 6: Create the DecisionController in ServiceOwner API

**Project:** `Altinn.AccessManagement.Api.ServiceOwner`

A thin controller that delegates to `IAuthorizationDecisionService`.

**File to create:**
- `Controllers/DecisionController.cs`

**Route:** `accessmanagement/api/v1/serviceowner/authorize`

**Implementation:**
```csharp
[ApiController]
[Route("accessmanagement/api/v1/serviceowner")]
public class DecisionController(IAuthorizationDecisionService authorizationDecisionService) : ControllerBase
{
    [HttpPost("authorize")]
    [Authorize(Policy = "AuthorizeScopeAccess")]  // same scope or new scope
    public async Task<ActionResult<AuthorizeResponse>> Authorize(
        [FromBody] AuthorizeRequest request, 
        CancellationToken ct)
    {
        var result = await authorizationDecisionService.AuthorizeAsync(request, ct);
        return Ok(result);
    }
}
```

---

### Step 7: Register Services in DI

**Project:** `Altinn.AccessMgmt.Core`

Add service registration for:
- `IAuthorizationContextService` → `AuthorizationContextService`
- `IAuthorizationDecisionService` → `AuthorizationDecisionService`

Ensure `ConnectionQuery`, `IEntityService`, and the delegation metadata repository are already registered (they should be from existing DI setup).

---

### Step 8: Add Authorization Policy for the New Endpoint

**Project:** `Altinn.AccessManagement.Api.ServiceOwner` (or shared configuration)

Add the `AuthorizeScopeAccess` (or equivalent) authorization policy that validates the `altinn:authorization/authorize` scope on the Maskinporten token. Check if this policy already exists in the AccessManagement auth configuration; if not, add it.

---

## Dependency Replacement Summary

| Old Dependency | Used For | New Replacement |
|---|---|---|
| `IRegisterService` (Register API) | Party lookups (by partyId, orgNo, ssn, uuid) | `IEntityService` (local EF) |
| `IRoles` (SBL Bridge / Altinn 2) | Get roles for user↔party | `ConnectionQuery` (local EF) |
| `IAccessManagementWrapper` | Get access packages, delegation changes | `ConnectionQuery` (local EF — single query for roles, packages, and delegations) |
| `IProfile` (Profile API) | Get user profile (userId → party) | `IEntityService.GetByUserId()` (local EF) |
| `IParties` (SBL Bridge / Altinn 2) | Get keyrole parties for user | `ConnectionQuery` / Entity parent relationships (local EF) |
| `IOedRoleAssignmentWrapper` | OED role assignments (external, non-Altinn 2) | **Keep as-is** — still needed as external lookup when policy contains OED role attributes |
| `IPolicyRetrievalPoint` | Retrieve XACML policies from blob storage | **Keep as-is** (policies are in blob storage, not external API) |
| `PolicyDecisionPoint` (ABAC library) | Evaluate XACML policy | **Keep as-is** (pure library, no external deps) |
| `IAccessListAuthorization` | Access list authorization check | **Keep as-is** (or replace with local if data available) |

## File Structure

```
Altinn.Authorization.Api.Contracts/
├── Authorization/
│   ├── AuthorizeRequest.cs
│   ├── AuthorizeResponse.cs
│   ├── AuthorizeResult.cs
│   ├── AuthorizeRequestCategory.cs
│   └── AuthorizeRequestAttribute.cs

Altinn.AccessMgmt.Core/
├── Services/
│   ├── Contracts/
│   │   ├── IAuthorizationDecisionService.cs
│   │   └── IAuthorizationContextService.cs
│   ├── AuthorizationDecisionService.cs
│   └── AuthorizationContextService.cs

Altinn.AccessManagement.Api.ServiceOwner/
├── Controllers/
│   └── DecisionController.cs
```

## Implementation Order

1. **Step 1** — Models (no dependencies)
2. **Step 2** — Entity resolution (verify `IEntityService` coverage)
3. **Step 3** — Context enrichment service (depends on Step 2)
4. **Step 4** — Delegation resolution service (independent of Step 3)
5. **Step 5** — Authorization decision service (depends on Steps 3 & 4)
6. **Step 6** — Controller (depends on Step 5)
7. **Step 7** — DI registration
8. **Step 8** — Auth policy configuration

## Risks & Considerations

- **XACML compatibility**: The ABAC library (`PolicyDecisionPoint`) expects `XacmlContextRequest` format. The context enrichment must produce the same enriched request structure the PDP expects.
- **Connection data completeness**: The Entity table and ConnectionQuery must have complete data to replace all Register/SBL Bridge lookups. Verify data sync is in place.
- **Access list authorization**: This currently calls Resource Registry. Evaluate if this should remain as an external call or be replaced.
- **Feature flags**: The old implementation uses feature flags (`UserAccessPackageAuthorization`, `SystemUserAccessPackageAuthorization`). Decide if these carry over or if the new implementation always enables these features.
- **Blob storage policies**: The `IPolicyRetrievalPoint` dependency remains. Ensure the AccessManagement project has access to the same policy blob storage.
- **Telemetry**: The old `DecisionTelemetry` metric recording should be carried forward or replaced with equivalent instrumentation.
