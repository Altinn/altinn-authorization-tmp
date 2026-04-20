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

## Remaining Sub-steps

| Sub-step | Status | Notes |
|---|---|---|
| 6.1 Full baseline across all 11 projects | ⬜ | Needs Docker for AccessMgmt projects |
| 6.2 Triage uncovered business logic | ⬜ | |
| **6.3 PEP coverage sprint** | **✅** | 60% → 79% |
| 6.4 Authorization.Tests edge cases (target 75%+) | ⬜ | |
| 6.5 Host.Lease tests | ⬜ | Blocked by storage account dependency |
| 6.6 CI threshold | ⬜ | |
| 6.7 AccessManagement coverage | ⬜ | |
