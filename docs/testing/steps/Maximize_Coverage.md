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
| **6.6 CI threshold** | **✅** | Per-assembly thresholds in CI via `coverage-thresholds.json` |
| 6.7 AccessManagement coverage | ⬜ | |

## Sub-step 6.7a: Authorization Infrastructure & Utility Coverage Sprint

### New Test Files

| File | Tests | Covers |
|---|---|---|
| `ExtensionTests/HttpClientExtensionTest.cs` | 9 | `PostAsync` (5) — no tokens, auth token, platform token, both tokens, POST method; `GetAsync` (4) — no tokens, auth token, platform token, GET method |
| `PlatformHttpExceptionTest.cs` | 4 | `CreateAsync` message formatting, `CreateAsync` stores response, constructor, `IsException` inheritance |
| `ServiceResourceHelperTest.cs` | 11 | `ResourceIdentifierRegex` — valid (5: alpha, alphanumeric, hyphen, underscore, mixed), invalid (6: too short, empty, uppercase, space, dot, special char) |
| `XacmlRequestApiModelBinderTest.cs` | 3 | `BindModelAsync` — null context throws, valid body, empty body |
| `XacmlRequestApiModelBinderProviderTest.cs` | 3 | `GetBinder` — null context throws, matching type returns binder, non-matching type returns null |

**Total new tests: 30** (284 → 314)

### Approach

- Targeted **previously completely untested** classes: `HttpClientExtension`, `PlatformHttpException`,
  `ServiceResourceHelper`, `XacmlRequestApiModelBinder`, `XacmlRequestApiModelBinderProvider`.
- All tests are pure unit tests — no WAF, Docker, or external dependencies needed.
- Used `RecordingHandler` (test double for `HttpMessageHandler`) to verify HTTP extension behavior.
- Used `DefaultHttpContext` and `DefaultModelBindingContext` for model binder tests.

### Verification

- [x] Build passes (0 errors)
- [x] All 314 Authorization.Tests pass (284 existing + 30 new)

---

## Remaining Sub-steps

| Sub-step | Status | Notes |
|---|---|---|
| 6.1 Full baseline across all 11 projects | ⬜ | Needs Docker for AccessMgmt projects |
| 6.2 Triage uncovered business logic | ⬜ | |
| **6.3 PEP coverage sprint** | **✅** | 60% → 79% |
| **6.4 Authorization.Tests edge cases** | **✅** | +66 tests (218 → 284) |
| 6.5 Host.Lease tests | ⬜ | Blocked by storage account dependency |
| **6.6 CI threshold** | **✅** | Per-assembly thresholds in CI via `coverage-thresholds.json` |
| **6.7a Authorization infra/utility coverage** | **✅** | +30 tests (284 → 314) |
| **6.7b Authorization models & services coverage** | **✅** | +23 tests (314 → 337) |
| 6.7c AccessManagement coverage | ⬜ | Needs Docker |

## Sub-step 6.7b: Authorization Models & Services Coverage Sprint

### New / Updated Test Files

| File | Tests | Covers |
|---|---|---|
| `OrganizationNumberTest.cs` (new) | 18 | `Parse` (4: string valid/invalid, span valid/invalid), `TryParse` (6: valid, too short, too long, letters, null, span), `CreateUnchecked` (1), `ToString` overloads (2), `TryFormat` (2: sufficient/insufficient buffer), JSON round-trip (1), JSON invalid (1), `GetExamples` (1) |
| `EventLogServiceTest.cs` (new) | 2 | `CreateAuthorizationEvent` — audit log enabled enqueues event, audit log disabled does not enqueue |
| `EventLogHelperTest.cs` (updated) | 3 new | `GetClientIpAddress` with forwarded-for header, no header; `MapAuthorizationEventFromContextRequest` full field mapping |

**Total new tests: 23** (314 → 337)

### Approach

- `OrganizationNumber` — previously completely untested model with substantial parsing, formatting,
  and JSON serialization logic (~140 lines). Tests cover all public API surface.
- `EventLogService` — previously untested service. Tested with mock `IFeatureManager` and
  `IEventsQueueClient` to verify audit-log feature flag gating.
- `MapAuthorizationEventFromContextRequest` — the main orchestrating method in `EventLogHelper`
  that was missing from the existing test suite.

### Verification

- [x] Build passes (0 errors)
- [x] All 337 Authorization.Tests pass (314 existing + 23 new)

---

## Next Step

Sub-step 6.7b is **complete** (Authorization models & services coverage sprint, +23 tests).

Next candidates:
- **6.1** (full baseline, needs Docker)
- **6.5** (Host.Lease, needs storage account)
- **Additional Authorization service-layer coverage**: `ContextHandler`, `DelegationContextHandler`,
  `PolicyInformationPoint`, `AccessListAuthorization` (require more complex mocking but no Docker)
- Return to deferred work (Phase 2.2–2.3 AccessMgmt WAF consolidation, Phase 3.2–3.4 mock dedup)

## Sub-step 6.7c: Authorization Service-Layer Coverage Sprint

### New Test Files

| File | Tests | Covers |
|---|---|---|
| `PolicyInformationPointTest.cs` | 7 | `GetRulesAsync` — no delegations returns empty, RevokeLast skipped, Grant extracts rules from XACML policy, Deny rules excluded, null-target rules excluded, mixed delegation types, multiple permit rules in single policy |

**Total new tests: 7** (337 → 344)

### Approach

- Targeted `PolicyInformationPoint` — previously completely untested service with non-trivial
  rule extraction logic (filters delegation changes by type, parses XACML policy rules,
  maps action/subject/resource attribute matches to `Rule` model).
- All tests are pure unit tests using Moq for `IPolicyRetrievalPoint` and `IDelegationMetadataRepository`.

### Verification

- [x] Build passes (0 errors)
- [x] All 344 Authorization.Tests pass (337 existing + 7 new)

---

## Remaining Sub-steps

| Sub-step | Status | Notes |
|---|---|---|
| 6.1 Full baseline across all 11 projects | ⬜ | Needs Docker for AccessMgmt projects |
| 6.2 Triage uncovered business logic | ⬜ | |
| **6.3 PEP coverage sprint** | **✅** | 60% → 79% |
| **6.4 Authorization.Tests edge cases** | **✅** | +66 tests (218 → 284) |
| 6.5 Host.Lease tests | ⬜ | Blocked by storage account dependency |
| **6.6 CI threshold** | **✅** | Per-assembly thresholds in CI via `coverage-thresholds.json` |
| **6.7a Authorization infra/utility coverage** | **✅** | +30 tests (284 → 314) |
| **6.7b Authorization models & services coverage** | **✅** | +23 tests (314 → 337) |
| **6.7c Authorization service-layer coverage** | **✅** | +7 tests (337 → 344) |
| **6.7d AccessListAuthorization coverage** | **✅** | +5 tests (344 → 349) |
| 6.7e AccessManagement coverage | ⬜ | Needs Docker |

## Sub-step 6.7d: AccessListAuthorization Coverage Sprint

### New Test Files

| File | Tests | Covers |
|---|---|---|
| `AccessListAuthorizationTest.cs` | 5 | `Authorize` — null memberships → NotAuthorized, empty memberships → NotAuthorized, null ActionFilters → Authorized (wildcard), matching action filter → Authorized, non-matching action filter → NotAuthorized |

**Total new tests: 5** (344 → 349)

### Approach

- `AccessListAuthorization` — previously completely untested service with 3 distinct authorization
  branches based on access list membership and action filter matching.
- Uses JSON deserialization to construct `AccessListAuthorizationRequest` (complex URN types
  `UrnJsonTypeValue<PartyUrn>`, `UrnJsonTypeValue<ResourceIdUrn>`, `UrnJsonTypeValue<ActionUrn>`).
- Single mock dependency (`IResourceRegistry`), all tests are pure unit tests.

### Verification

- [x] Build passes (0 errors)
- [x] All 349 Authorization.Tests pass (344 existing + 5 new)

---

## Next Step

Sub-step 6.7d is **complete** (AccessListAuthorization coverage sprint, +5 tests).

Next candidates:
- **Additional Authorization service-layer coverage**: `DelegationContextHandler`,
  `ContextHandler` gap analysis (require more complex mocking but no Docker)
- **6.1** (full baseline, needs Docker)
- **6.5** (Host.Lease, needs storage account)
- Return to deferred work (Phase 2.2–2.3 AccessMgmt WAF consolidation, Phase 3.2–3.4 mock dedup)
