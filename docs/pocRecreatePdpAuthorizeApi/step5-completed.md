# Step 5: Authorization Decision Service — Completed

## Summary

Created `AuthorizationDecisionService` in `Altinn.AccessMgmt.Core` — the main orchestrator that replaces all business logic previously in the old `DecisionController`.

## Files Created

| File | Description |
|---|---|
| `Services/Contracts/IAuthorizationDecisionService.cs` | Interface with single `AuthorizeAsync` method |
| `Services/Contracts/IOedRoleAssignmentService.cs` | Interface for OED role lookups (external dependency, kept as-is) |
| `Services/AuthorizationDecisionService.cs` | Full implementation of the authorization decision flow |

## Authorization Flow

```
AuthorizationRequestDto (contract DTO)
  → MapToInternal (convert to ABAC XacmlJsonRequest)
  → Handle single vs multi-request
  → For each single request:
    1. Extract resource & subject attributes from XACML context
    2. Resolve resource party Entity (via IAuthorizationContextService)
    3. Resolve subject Entity (via IAuthorizationContextService)
    4. Enrich request with resolved party ID
    5. Get policy via IPolicyRetrievalPoint
    6. Enrich subject with roles & access packages from ConnectionQuery
    7. Enrich with OED roles if policy requires (external IOedRoleAssignmentService)
    8. Evaluate against policy using PolicyDecisionPoint (ABAC library)
    9. If NotApplicable → attempt delegation-based authorization
    10. Return final decision
  → MapToResponse (convert back to contract DTO)
```

## Key Design Decisions

### Mapping Layer
- Contract DTOs (`AuthorizationRequestDto` etc.) are mapped to/from internal ABAC types (`XacmlJsonRequest`, `XacmlJsonResponse`) within the service
- No AutoMapper dependency — pure manual mapping methods

### Connection-Based Enrichment
- Single `ConnectionQuery` call retrieves roles, access packages, and delegations
- Roles are extracted from `ConnectionQueryRecord.Role.Code` and added as `urn:altinn:rolecode` attributes
- Access packages are extracted from `ConnectionQueryPackage.Urn` and decomposed into XACML attribute id + value

### Policy-Driven Enrichment
- Role attributes are only added if the policy contains `urn:altinn:rolecode` subjects
- Access package attributes are only added if the policy contains `urn:altinn:accesspackage` subjects
- OED roles are only looked up if the policy contains `urn:digitaltdodsbo:rolecode` subjects

### OED Roles (External Dependency)
- `IOedRoleAssignmentService` interface defined in `Altinn.AccessMgmt.Core`
- Only called when policy requires it AND both subject and resource party SSNs are available
- Implementation will be provided separately (wrapping the existing external HTTP client)

### Delegation Authorization
- Delegation connections are retrieved from the same `ConnectionQuery` call
- TODO: Full delegation policy evaluation from blob storage pending — requires delegation records to include blob storage path/version

## Dependencies

| Dependency | Purpose |
|---|---|
| `IAuthorizationContextService` | Entity resolution + ConnectionQuery |
| `IPolicyRetrievalPoint` | Policy retrieval from blob storage |
| `IOedRoleAssignmentService` | External OED role lookup |
| `PolicyDecisionPoint` (ABAC) | XACML policy evaluation |
| `ILogger` | Logging |

## Build Status

✅ Build successful
