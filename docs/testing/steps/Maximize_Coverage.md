# Step 7 — Maximize Code Coverage (Phase 6)

## Goal

Increase test coverage across all source projects, starting with the
highest-impact, lowest-effort gaps identified in the Phase 5 baseline.

## Sub-step 6.3: PEP Coverage Sprint

### Baseline → After

| Assembly | Line% Before | Line% After | Branch% Before | Branch% After |
|---|---|---|---|---|
| Altinn.Authorization.PEP (own tests) | 60.20 | **78.99** | 56.25 | **78.68** |

### New Test Files

| File | Tests | Covers |
|---|---|---|
| `ClaimAccessHandlerTest.cs` | 6 | `ClaimAccessHandler.HandleRequirementAsync` — matching claim, no match, no claims, wrong value, multiple claims, null user claims |
| `IDFormatDeterminatorTest.cs` | 18 | `DetermineIDFormat`, `IsValidOrganizationNumber`, `IsValidSSN`, `IsValidUserName` — null, empty, valid, invalid, edge cases |
| `PDPAppSITest.cs` | 4 | `IPDP.GetDecisionForRequest` and `GetDecisionForUnvalidateRequest` — valid response, null response, permit, exception |
| `DecisionHelperTest.cs` (additions) | 24 | `ValidatePdpDecisionWithoutObligationCheck`, `ValidatePdpDecisionDetailed`, org-level obligations in `ValidateDecisionResult`, `CreateDecisionRequestForResourceRegistryResource`, consumer claims, system user claims, sid/jti claims, person UUID, party ID, organization number attribute override, `CreateActionCategory` with includeResult, `CreateXacmlJsonAttribute` |

**Total new tests: 52** (40 → 92)

### Approach

- Focused on untested classes first (`ClaimAccessHandler`, `IDFormatDeterminator`)
- Then expanded `DecisionHelper` coverage for uncovered claim-type branches and
  validation methods (`ValidatePdpDecisionWithoutObligationCheck`, detailed results)
- `PDPAppSI` tested via `IPDP` mock (the class depends on `AuthorizationApiClient`
  which has no interface — full integration test deferred)
- `AuthorizationApiClient` coverage deferred (requires HTTP test server setup)

## Verification

- [x] Build passes (0 errors)
- [x] All 92 PEP tests pass
- [x] PEP line coverage ≥ 75% target

## Full Baseline (Phase 5 + current)

| Assembly | Line% | Branch% | Source |
|---|---|---|---|
| Altinn.Authorization | 62.77 | 63.50 | Authorization.Tests |
| Altinn.Authorization.ABAC | 62.91 | 61.29 | Authorization.Tests |
| **Altinn.Authorization.PEP** | **78.99** | **78.68** | PEP.Tests |
| Altinn.Authorization.Host.Lease | 0 | 0 | Host.Lease.Tests (2 skipped — need storage account) |

> AccessManagement projects require Docker (Testcontainers) — baseline deferred.

## Sub-step 6.4: Authorization.Tests Edge-Case Coverage Sprint

### Baseline → After

| Assembly | Line% Before | Line% After | Branch% Before | Branch% After |
|---|---|---|---|---|
| Altinn.Authorization | 62.77 | _TBD (run coverage)_ | 63.50 | _TBD_ |
| Altinn.Authorization.ABAC | 62.91 | _TBD_ | 61.29 | _TBD_ |

> Coverage numbers pending `run-coverage.ps1` execution (no Docker needed for these projects).

### New Test Files

| File | Tests | Covers |
|---|---|---|
| `ExtensionTests/StringExtensionsTest.cs` | 8 | `StringExtensions.AsFileName` — null, empty, whitespace, valid chars, invalid chars throw vs replace, mixed chars |
| `ExtensionTests/ClaimsPrincipalExtensionsTest.cs` | 16 | `GetUserOrOrgId` (4), `GetOrg` (2), `GetOrgNumber` (3), `GetUserIdAsInt` (3), `GetAuthenticationLevel` (3) |
| `EventLogHelperTest.cs` | 10 | `GetResourceAttributes` (3), `GetSubjectInformation` (3), `GetActionInformation` (3), `GetClientIpAddress` (1) |
| `PolicyHelperTest.cs` | 16 | `GetAltinnAppsPolicyPath` (3), `GetAltinnAppDelegationPolicyPath` (5), `GetPolicyResourceType` (4), `GetPolicyPath`, `GetRolesWithAccess` (2), `BuildDelegationPolicy`, `ParsePolicy`, `GetMinimumAuthenticationLevelFromXacmlPolicy` |
| `DelegationHelperAdditionalTest.cs` | 16 | `TryGetCoveredByPartyIdFromMatch` (5), `TryGetCoveredByUserIdFromMatch` (2), `GetCoveredByFromMatch` (3), `TryGetResourceFromAttributeMatch` (2), `GetAttributeMatchKey`, `GetPolicyCount` (2), `GetRulesCountToDeleteFromRequestToDelete`, `TryGetDelegationParamsFromRule` (2), `SetRuleType` (4) |

**Total new tests: 66** (218 → 284)

### Approach

- Targeted previously **completely untested** classes first: `EventLogHelper`, `ClaimsPrincipalExtensions`, `StringExtensions`, `PolicyHelper`.
- Extended `DelegationHelper` coverage for methods not reached by existing `DelegationHelperTest` (10 existing tests cover `SortRulesByDelegationPolicyPath` and `PolicyContainsMatchingRule` only).
- All tests are pure unit tests — no WAF or Testcontainers needed.

### Verification

- [x] Build passes (0 errors)
- [x] All 284 Authorization.Tests pass (218 existing + 66 new)

---

## Remaining Sub-steps

| Sub-step | Status | Notes |
|---|---|---|
| 6.1 Full baseline across all 11 projects | ⬜ | Needs Docker for AccessMgmt projects |
| 6.2 Triage uncovered business logic | ⬜ | |
| **6.3 PEP coverage sprint** | **✅** | 60% → 79% |
| **6.4 Authorization.Tests edge cases** | **✅** | +66 tests (218 → 284) |
| 6.5 Host.Lease tests | ⬜ | Blocked by storage account dependency |
| 6.6 CI threshold | ⬜ | |
| 6.7 AccessManagement coverage | ⬜ | |

---

## Next Step

The next highest-impact actionable item is **sub-step 6.6 — CI coverage threshold**.
This is a quick infrastructure win that prevents coverage regressions:

1. Add minimum coverage thresholds to `run-coverage.ps1` or CI pipeline config.
2. Configure PR gates to fail if coverage drops below the established baselines.
3. Optionally publish Cobertura reports to a dashboard.

Alternatively, run `run-coverage.ps1` first to capture updated baselines for
Altinn.Authorization and Altinn.Authorization.PEP after the 6.3 + 6.4 sprints,
then use those numbers as the enforcement floor.

Start by reading `docs/testing/steps/INDEX.md` and this file.
