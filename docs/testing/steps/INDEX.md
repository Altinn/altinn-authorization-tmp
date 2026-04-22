# Testing Infrastructure Overhaul — Step Log

## Getting Started & Workflow

**New chat?** Read these docs **in order** to get full context:

1. **This file** (`docs/testing/steps/INDEX.md`) — step log, coverage results,
   recommended next steps, deferred work, and workflow rules.
2. **[TESTING_INFRASTRUCTURE_OVERHAUL.md](../TESTING_INFRASTRUCTURE_OVERHAUL.md)** —
   original audit, issue IDs (C1–C5, M1–M8, L1–L3), and the phase plan.
3. **The step doc for the work you're about to do** (linked in the table below or
   in the Recommended Next Steps section).

**When completing a step:**

- **Create a step doc** (`docs/testing/steps/<Step_Name>.md`) describing the goal,
  what changed, verification results, and any deferred items. Add a row to the
  step log table below linking to the new doc.
- **Run all tests that were changed or impacted by the step** and record the
  results in the step doc. Update the [Final Coverage (measured)](#final-coverage-measured)
  table at the bottom of this file if the step affected coverage of any listed
  assembly (or a new assembly that should be tracked).
- **Re-check the [Blocked Items](#blocked-items) section** to see if anything is
  now unblocked by the completed step. If so, move it into
  `### Recommended Next Steps (priority order)` at an appropriate priority and
  remove it from the Blocked Items table.
- **Sweep `docs/testing/steps/` for obsoleted docs.** Review every file under
  `docs/testing/steps/` and check whether any have been superseded or
  invalidated by the completed step (e.g. plans that are now fully executed,
  audits whose findings have all been addressed, POCs whose follow-up work is
  done). For each obsolete doc, either delete it and update links, or add a
  banner at the top pointing to the step that replaced it.
- **Commit and push** at the end of each step.
- **Recommend whether a new chat should be started** for the next step, based on complexity and context.
- **If a new chat is recommended, provide this ready-to-copy prompt** to hand off
  cleanly (don't rewrite it — `INDEX.md` already carries all the context):

  > Continue the testing infrastructure overhaul on branch
  > `feature/2842_Optimize_Test_Infrastructure_and_Performance`.
  >
  > Start by reading `docs/testing/steps/INDEX.md` — it's the entry point and tells
  > you exactly what to read next, how to pick the next step, and the workflow
  > rules for completing one.
  >
  > Then execute the highest-priority item from
  > `### Recommended Next Steps (priority order)` in that file.
- **Wait for explicit go-ahead** before proceeding to the next step.

**Picking the next step (when the list below is thinning):**

1. If `### Recommended Next Steps (priority order)` still has actionable
   items, take the highest-priority one.
2. If that list is empty or only contains blocked/unactionable items, consult
   [TESTING_INFRASTRUCTURE_OVERHAUL.md](../TESTING_INFRASTRUCTURE_OVERHAUL.md)
   for the next actionable item from the phase plan, and add it back to the
   list below before starting.
3. If `TESTING_INFRASTRUCTURE_OVERHAUL.md` is also exhausted of actionable
   work, the next step should itself be **a fresh audit of the current
   testing infrastructure** to identify the next most valuable improvements
   — produce an updated audit doc and a refreshed recommended-next-steps
   list, then resume the cycle.

---

Steps are listed in the order they were **actually completed**, not by the
original phase numbers in the [overhaul plan](../TESTING_INFRASTRUCTURE_OVERHAUL.md).

| # | Completed | Topic | Plan Phase | Doc |
|---|-----------|-------|------------|-----|
| 1 | ✅ | Create overhaul plan & audit | Phase 0 | [1_Create_Overhaul_Plan.md](1_Create_Overhaul_Plan.md) |
| 2 | ✅ | Unify xUnit v3 & net9.0 TFM | Phase 1 | [2_Unify_xUnit_and_TFM.md](2_Unify_xUnit_and_TFM.md) |
| 3 | ✅ | Consolidate WebApplicationFactory (Authorization.Tests) | Phase 2 | [3_Consolidate_WebApplicationFactory.md](3_Consolidate_WebApplicationFactory.md) |
| 4 | ✅ | Mock deduplication audit | Phase 3 | [4_Mock_Deduplication_Audit.md](4_Mock_Deduplication_Audit.md) |
| 5 | ✅ | Coverage infrastructure (`dotnet-coverage`, `run-coverage.ps1`) | Phase 5 | [5_Coverage_Infrastructure.md](5_Coverage_Infrastructure.md) |
| 6 | ✅ | Test patterns, naming convention & csproj cleanup | Phase 4 | [6_Test_Patterns_and_Naming.md](6_Test_Patterns_and_Naming.md) |
| 7 | ✅ | Maximize code coverage (actionable items) | Phase 6.1–6.5 | [7_Maximize_Coverage.md](7_Maximize_Coverage.md) |
| 8 | ✅ | CI coverage threshold (6.6) | Phase 6.6 | [8_CI_Coverage_Threshold.md](8_CI_Coverage_Threshold.md) |
| 9 | ✅ | Shared fixture for Authorization.Tests | Phase 2.4 | [9_Shared_Fixture_Authorization.md](9_Shared_Fixture_Authorization.md) |
| 10 | ✅ | Dead code & suppressions cleanup (L1–L3) | Phase 4.5–4.6 | [10_Dead_Code_and_Suppressions_Cleanup.md](10_Dead_Code_and_Suppressions_Cleanup.md) |
| 11 | ✅ | Certificate consolidation — Authorization.Tests (M8) | Phase 3.5 | [11_Certificate_Consolidation.md](11_Certificate_Consolidation.md) |
| 12 | ✅ | AccessManagement coverage baseline with Podman (6.7a) | Phase 6.7a | [12_AccessManagement_Coverage_Baseline_Success.md](12_AccessManagement_Coverage_Baseline_Success.md) |
| 13 | ✅ | FluentAssertions evaluation | Phase 4.2 | [13_FluentAssertions_Evaluation.md](13_FluentAssertions_Evaluation.md) |
| 14 | ✅ | Add FluentAssertions package | Phase 4.2a | [14_Add_FluentAssertions_Package.md](14_Add_FluentAssertions_Package.md) |
| 15 | ✅ | Mock deduplication implementation | Phase 3.2–3.4 | [15_Mock_Deduplication_Implementation.md](15_Mock_Deduplication_Implementation.md) |
| 16 | ✅ | AccessMgmt.Tests WAF consolidation — plan + `ResourceControllerTest` POC | Phase 2.2 | [16_AccessMgmt_WAF_Consolidation_Plan_and_POC.md](16_AccessMgmt_WAF_Consolidation_Plan_and_POC.md) |
| 17 | ✅ | Sub-step 16.1 — Group A easy wins (`PolicyInformationPointControllerTest`, `DelegationsControllerTest`) | Phase 2.2 | [17_AccessMgmt_WAF_Group_A_Easy_Wins.md](17_AccessMgmt_WAF_Group_A_Easy_Wins.md) |
| 18 | ✅ | Sub-step 16.2a — Group A single-configuration migrations (`Altinn2RightsControllerTest`, `AppsInstanceDelegationControllerTest`) | Phase 2.2 | [18_AccessMgmt_WAF_Group_A_Single_Config.md](18_AccessMgmt_WAF_Group_A_Single_Config.md) |
| 19 | ✅ | Sub-step 16.2b — Group A nested-class splits (`MaskinportenSchemaControllerTest`, `RightsInternalControllerTest`); `CustomWebApplicationFactory` deleted | Phase 2.2 | [19_AccessMgmt_WAF_Group_A_Nested_Splits.md](19_AccessMgmt_WAF_Group_A_Nested_Splits.md) |
| 20 | ✅ | Sub-step 16.3 — Group B simple (`HealthCheckTests`, `PartyControllerTests`) | Phase 2.2 | [20_AccessMgmt_WAF_Group_B_Simple.md](20_AccessMgmt_WAF_Group_B_Simple.md) |
| 21 | ✅ | Sub-step 16.4 investigation — Group B scenario-based consumers blocked on Yuniql schema provisioning in `ApiFixture` | Phase 2.2 | [21_AccessMgmt_WAF_Group_B_Scenarios_16_4_Investigation.md](21_AccessMgmt_WAF_Group_B_Scenarios_16_4_Investigation.md) |
| 22 | ✅ | Sub-step 16.4-prep — `LegacyApiFixture` plumbing (Yuniql + EF schema) | Phase 2.2 | [22_AccessMgmt_WAF_Group_B_16_4_Prep_LegacyApiFixture.md](22_AccessMgmt_WAF_Group_B_16_4_Prep_LegacyApiFixture.md) |
| 23 | ✅ | Sub-step 16.4a — Migrate `V2ResourceControllerTest`, `ConsentControllerTestEnterprise`, `MaskinPorten.ConsentControllerTest` to `LegacyApiFixture` | Phase 2.2 | [23_AccessMgmt_WAF_Group_B_16_4a_Consent_Migrations.md](23_AccessMgmt_WAF_Group_B_16_4a_Consent_Migrations.md) |
| 24 | ⚠️ Partial | Sub-step 16.4b — Delete two 100%-`[Skip]`ped WAF consumers; `ConsentControllerTestBFF` migration blocked on per-test DB isolation gap | Phase 2.2 | [24_AccessMgmt_WAF_Group_B_16_4b_Final_Consumers.md](24_AccessMgmt_WAF_Group_B_16_4b_Final_Consumers.md) |
| 25 | ✅ | Sub-step 16.4b-continued — `ConsentControllerTestBFF` migrated to per-test `LegacyApiFixture` via `IAsyncLifetime`; `WebApplicationFixture` has no remaining consumers | Phase 2.2 | [25_AccessMgmt_WAF_16_4b_Continued_BFF_Migration.md](25_AccessMgmt_WAF_16_4b_Continued_BFF_Migration.md) |
| 26 | ✅ | Sub-step 16.5 — Retired `WebApplicationFixture`, `AcceptanceCriteriaComposer`, `Scenarios/*`, `ControllerTestTemplate`; `PostgresServer` retained (still used by `PostgresFixture`) | Phase 2.2 | [26_AccessMgmt_WAF_16_5_Retire_Legacy_Harness.md](26_AccessMgmt_WAF_16_5_Retire_Legacy_Harness.md) |
| 27 | ✅ | FluentAssertions usage guidelines (`docs/testing/FLUENT_ASSERTIONS_GUIDELINES.md`) | Phase 4.2b | [27_FluentAssertions_Guidelines.md](27_FluentAssertions_Guidelines.md) |
| 28 | ✅ | CI coverage thresholds for AccessManagement (4 enforced + 1 warn-only) | Phase 5.1b | [28_CI_Coverage_Thresholds_AccessManagement.md](28_CI_Coverage_Thresholds_AccessManagement.md) |
| 29 | ✅ | Coverage: AccessManagement.Api.ServiceOwner — closed the three untested `RequestController` endpoints; 54.35% → 71.74% line | Phase 6.7b | [29_Coverage_ServiceOwner_Api.md](29_Coverage_ServiceOwner_Api.md) |
| 30 | ✅ | Coverage: AccessManagement.Api.Enduser — closed five untested `RequestController` endpoints (`GetRequest`, `GetSentRequestsCount`, `GetReceivedRequestsCount`, `ApprovePackageRequest`, `ApproveResourceRequest`); 45.57% → 49.93% line | Phase 6.7c | [30_Coverage_Enduser_Api.md](30_Coverage_Enduser_Api.md) |
| 31 | ✅ | Coverage: AccessManagement.Api.Enduser Validation layer — `ConnectionValidation` + `ConnectionCombinationRules` direct unit tests via `InternalsVisibleTo`; 49.93% → 62.76% line | Phase 6.7c | [31_Coverage_Enduser_Api_Validation.md](31_Coverage_Enduser_Api_Validation.md) |
| 32 | ✅ | Coverage: AccessManagement.Api.Enduser `ParameterValidation` — 44 direct unit tests for atomic per-parameter rules; 62.76% → 65.94% line | Phase 6.7c | [32_Coverage_Enduser_Api_ParameterValidation.md](32_Coverage_Enduser_Api_ParameterValidation.md) |
| 33 | ✅ | Coverage: AccessManagement.Api.Enduser `Utils.ToUuidResolver` — 13 direct unit tests (Moq) for both resolve branches; 65.94% → 68.32% line | Phase 6.7c | [33_Coverage_Enduser_Api_ToUuidResolver.md](33_Coverage_Enduser_Api_ToUuidResolver.md) |
| 34 | ✅ | CI fix — scope coverage threshold enforcement to the owning vertical (unblocks `app: Authorization`, `lib: Integration`, `pkg: PEP`) | Phase 6.6 follow-up | [34_CI_Coverage_Threshold_Scoping.md](34_CI_Coverage_Threshold_Scoping.md) |
| 35 | ✅ | CI fix — route `dotnet test` to Microsoft Testing Platform so xUnit v3 tests are actually discovered (fixes "No test is available" across all verticals) | Phase 6.6 follow-up | [35_CI_Tests_MTP_Discovery.md](35_CI_Tests_MTP_Discovery.md) |
| 36 | ✅ | CI fix — post‑MTP hardening: make `FluentAssertions` available to test-helper libraries (fixes TestUtils CS0400) and detect xUnit v3 MTP executables cross‑platform in `run-coverage.ps1` (fixes Linux "No coverage files generated") | Phase 6.6 follow-up | [36_CI_Post_MTP_Hardening.md](36_CI_Post_MTP_Hardening.md) |
| 37 | ✅ | CI fix — restore MTP routing by adding `<TargetFramework></TargetFramework>` inline to the 9 apps/libs test csprojs that regressed in commit `20ae747b` (singular inherited from `src/Directory.Build.props` silently forced `dotnet test` back to VSTest → "No test is available") | Phase 6.6 follow-up | [37_CI_MTP_Routing_TargetFramework_Clear.md](37_CI_MTP_Routing_TargetFramework_Clear.md) |
| 38 | ✅ | CI fix — MTP follow-ups: forward `--results-directory`, `--report-xunit-trx`, `--ignore-exit-code 8` after `--` to the Sonar `analyze` job's inner `dotnet test` (restores Sonar test-result reporting + unblocks Host.Lease all-skipped vertical); document that `run-coverage.ps1`'s `dotnet-coverage collect -- dotnet <dll>` path uses xUnit v3's native runner (not MTP) and needs no exit-code handling | Phase 6.6 follow-up | [38_CI_MTP_Followups_Sonar_And_Coverage.md](38_CI_MTP_Followups_Sonar_And_Coverage.md) |
| 39 | ✅ | Housekeeping — relocate build tooling out of `docs/`: `git mv` `run-coverage.ps1`, `run-accessmanagement-coverage.ps1`, `coverage-thresholds.json` from `docs/testing/` to `eng/testing/`; update 2 path strings + 1 comment in `tpl-vertical-ci.yml`. Scripts' internal `$PSScriptRoot` / `$repoRoot` paths unchanged (still 2 levels up) | Phase 6.6 follow-up | [39_CI_Relocate_Scripts_to_Eng.md](39_CI_Relocate_Scripts_to_Eng.md) |
| 40 | ✅ | First green CI run follow-ups: Docker outage guard in `PostgresFixture`/`EFPostgresFactory` → `Assert.Skip`; upload MTP `*.log`/`*.trx` on failure; fix 3 Linux-specific test failures (Azurite-absent 5xx in `RequestController`, `'<'` → `'\0'` in `StringExtensionsTest` ×2); trim CI artifacts (drop `*.cobertura.xml`, `if: failure()`, retention 7→3 days) and redirect coverage-step per-project test stdout to `TestResults/<Project>.coverage.log` with tail echo on failure; PR review fixes in `TestCertificates`/`AuthorizationApiFixture`/`PolicyControllerTest` + stray file removal; new `Report failed tests` workflow step that parses MTP logs and emits per-failure `::group::` + `::error title::` annotations; follow-up fix removing the default `Accept: application/xml` header from `PolicyControllerTest`'s shared `_client` (introduced in Step 3) that was breaking the 8 JSON-parsing tests with `JsonReaderException: Unexpected character encountered while parsing value: <` — content negotiation returned XML which was fed into `JsonConvert.DeserializeObject`; Sonar fix on `Program.cs` (S1118 `protected Program()` + replace unresolved `<see cref="WebApplicationFactory{TEntryPoint}"/>` with `<c>` code span) | Phase 6.6 follow-up | [40_CI_First_Green_Run_Hardening.md](40_CI_First_Green_Run_Hardening.md) |
| 41 | ✅ | CI perf — eliminate duplicate test execution: replace "Test" + "Coverage threshold check" steps in `tpl-vertical-ci.yml` with hybrid design. Step A = `dotnet-coverage collect -- dotnet test -- --ignore-exit-code 8` (single run, emits TRX + `TestResults/coverage.cobertura.xml`). Step B = parse-only `eng/testing/check-coverage-thresholds.ps1` (seconds). New shared script splits pretty-print + per-assembly threshold enforcement out of `run-coverage.ps1` so local-dev and CI stay in sync. `run-coverage.ps1` simplified (213 → 148 lines) to delegate to the shared script while keeping its parallel per-project `dotnet-coverage collect` loop for workstation use. Preserves step-level failure signal (tests-fail vs coverage-fail distinguishable) and removes the ~4m32s serial coverage re-run that made Coverage 2x slower than Test | Phase 6.6 follow-up | [41_CI_Coverage_Single_Run.md](41_CI_Coverage_Single_Run.md) |
| 42 | ✅ | Coverage 6.7d Part 1 — 65 new pure-logic unit tests (no container required): `FuzzySearch`+`SearchPropertyBuilder<T>` for both `AccessMgmt.Persistence.Core` and `AccessMgmt.Core`; `GenericFilterBuilder<T>` for `Persistence.Core`; `PackagesController` (Search/GetHierarchy/GetGroups/GetGroup) and `TypesController.GetOrganizationSubTypes` for `Api.Metadata` | Phase 6.7d | [42_Coverage_Phase_6_7d_Part1.md](42_Coverage_Phase_6_7d_Part1.md) |
| 43 | ✅ | Coverage 6.7d Part 2 — 46 new pure-logic unit tests: `ValidationComposer` (All/Any/Validate), `OrgUtil` (GetMaskinportenScopes/GetAuthenticatedParty/GetSupplierParty), `DbHelperMethods` (GetPostgresType all .NET types + nullable + PropertyInfo overload), `PostgresQueryBuilder` (BuildInsertQuery/BuildUpdateQuery/BuildSingleNullUpdateQuery/GetTableName/BuildBasicSelectQuery) | Phase 6.7d | [43_Coverage_Phase_6_7d_Part2.md](43_Coverage_Phase_6_7d_Part2.md) |
| 44 | ✅ | Coverage 6.7d Part 3 — 40 new pure-logic unit tests: `InternalsVisibleTo` added to `AccessMgmt.Core`; `ValidationRuleClassesTest` (28 tests: `EntityValidation`, `EntityTypeValidation`, `RoleValidation`, `AssignmentPackageValidation`, `DelegationValidation`, `PackageValidation`); `DbConverterTest` (10 tests: flat mapping, case-insensitivity, nulls, nullable Guid, `_rownumber` page info, unknown columns, `PreloadCache`) | Phase 6.7d | [44_Coverage_Phase_6_7d_Part3.md](44_Coverage_Phase_6_7d_Part3.md) |
| 45 | ✅ | Coverage 6.7d Part 4 — 56 new pure-logic unit tests: `DtoMapperTest` covering all static/instance methods across the 11 `DtoMapper` partial-class files (`DtoMapper.cs`, `.Simplified`, `AssignmentDto`, `AssignmentPackageDto`, `PermissionDto`, `DelegationDto`, `CreateDelegationResponse`, `RolePackage`, `AccessPackageDto`, `AuthorizedPartyDto`, `RequestMapper`) | Phase 6.7d | [45_Coverage_Phase_6_7d_Part4.md](45_Coverage_Phase_6_7d_Part4.md) |
| 46 | ✅ | Coverage 6.7d Part 5 — 27 new pure-logic unit tests: all 5 `ConsentRequest*Extensions` + `UserUtil` + `PagingInput` (round-trip token) for `AccessManagement.Api.Internal`; `EventMapperService.MapToDelegationChangeEventList` for `AccessManagement.Integration`; `InternalsVisibleTo("Altinn.AccessManagement.Api.Tests")` added to `Api.Internal.csproj` | Phase 6.7d | [46_Coverage_Phase_6_7d_Part5.md](46_Coverage_Phase_6_7d_Part5.md) |

| 47 | ✅ | Fix Phase 6.7e latent production bugs — `ConstantLookup.GetByName` duplicate-key crash; `PackageConstants.TryGetByUrn` Case-2 off-by-one; `RequestController` `/resource` + `/package` query-param overloads now return 202 instead of 400; all 7 `PackageConstantsTest` skips removed | Phase 6.7e | [47_Coverage_ServiceOwner_Api_6_7e.md](47_Coverage_ServiceOwner_Api_6_7e.md) |
| 48 | ✅ | Fix 6.7f — `ApprovePackageRequest` production bug: replaced `connectionService.AddPackage` (which ran an unauthorized-delegation check and returned 400) with `assignmentService.ImportAssignmentPackages`; fixed `ImportAssignmentPackages` null-`AuditValues` crash (500) by using the `CancellationToken`-only `SaveChangesAsync` overload when no explicit audit is supplied; `Sender_GetSentRequests_ContainsSeededRequest` was already passing; both `TODO (6.7f)` comments removed; 306 tests, 0 failed | Phase 6.7f | [48_Fix_6_7f_ApprovePackageRequest.md](48_Fix_6_7f_ApprovePackageRequest.md) |
| 49 | ✅ | Coverage 6.7d Part 6 — 34 direct Moq-based unit tests for `Api.Internal` controllers: `InternalConnectionsController` (17 tests, 6 actions × 2 branches), `SystemUserClientDelegationController` (13 tests, 5 actions × multi-branch), `PartyController` (6 tests, JWT app-claim + service paths); resolved `PersistenceEF.Models` vs `Persistence.Models` type ambiguity; `Result<T>` constructor usage documented | Phase 6.7d | [49_Coverage_Api_Internal_Controllers.md](49_Coverage_Api_Internal_Controllers.md) |
| 50 | ✅ | Coverage: `AccessManagement.Integration` `DelegationRequestProxy` — 5 pure-unit tests via `FakeHttpMessageHandler` covering 200 OK deserialization, non-OK → null, query-string param inclusion/omission, multi-item response | Phase 6.7d | [50_Coverage_Integration_DelegationRequestProxy.md](50_Coverage_Integration_DelegationRequestProxy.md) |
| 51 | ✅ | Fix 6.7f — `ResourceRegistryMock.GetMembershipsForResourceForParty` cache-hit bug: on cache hit the method fell through to `return Enumerable.Empty<>()` instead of returning cached memberships; `DenyActionFilterNotMatching` primed the cache for `ttd-accesslist-resource-with-actionfilter`, `PermitWithActionFilterMatch` got empty on the subsequent cache hit → Deny; fixed with `return memberships ?? Enumerable.Empty<>()` after the block; `[Skip]` removed; all 21 `ResourceRegistry_DecisionTests` pass deterministically | Phase 6.7f | [51_Fix_6_7f_AccessListAuthorizationMockCacheBug.md](51_Fix_6_7f_AccessListAuthorizationMockCacheBug.md) |

| 51 | ✅ | Fix 6.7f — `ResourceRegistryMock.GetMembershipsForResourceForParty` cache-hit bug: on cache hit, method fell through to `return Enumerable.Empty<>()` instead of returning cached memberships; `DenyActionFilterNotMatching` primed the cache, `PermitWithActionFilterMatch` got empty on cache hit → Deny; fixed with `return memberships ?? Enumerable.Empty<>()` after the block; `[Skip]` removed; all 21 `ResourceRegistry_DecisionTests` pass deterministically | Phase 6.7f | [51_Fix_6_7f_AccessListAuthorizationMockCacheBug.md](51_Fix_6_7f_AccessListAuthorizationMockCacheBug.md) |
| 52 | ✅ | Coverage: `AccessManagement.Api.Metadata` `RolesController` — 14 direct unit tests covering all 6 endpoints (GetAll, GetId, GetPackages/GetResources by code, GetPackages/GetResources by id); pass-through `ITranslationService` mock; no containers required | Phase 6.7d | [52_Coverage_Api_Metadata_RolesController.md](52_Coverage_Api_Metadata_RolesController.md) |
| 53 | ✅ | Coverage 6.7d Part 7 — 54 new pure-logic unit tests (no container required): `ResourceValidation` (18: all 8 internal factory methods); `DelegationCheckDtoMapper.Convert` (4: grouping + any-true semantics); `QueryWrapper.WrapQueryResponse` (3: non-empty, empty, single); `DelegationCheckHelper.IsAccessListModeEnabledAndApplicable` (5: enabled/disabled × org/person/empty); `SearchCache<T>` (6: null before set, round-trip, defensive copy, overwrite); `DbDefinitionBuilder<T>` (18: all fluent methods + chaining) | Phase 6.7d | [53_Coverage_6_7d_Part7.md](53_Coverage_6_7d_Part7.md) |

All items below are actionable and have no container-runtime dependency.

1. **Phase 6 coverage improvements** — Fill identified gaps (can use FluentAssertions!):
   - **6.7d (continued):** AccessMgmt persistence/core layers — Parts 1–7 added FuzzySearch/GenericFilterBuilder/Metadata controllers, ValidationComposer/OrgUtil/DbHelperMethods/PostgresQueryBuilder, all internal validation-rule classes + DbConverter, all DtoMapper partial-class methods, all Api.Internal consent extensions + EventMapperService, Api.Internal controllers (Step 49), `DelegationRequestProxy` (Step 50), and `ResourceValidation`/`DelegationCheckDtoMapper`/`QueryWrapper`/`DelegationCheckHelper`/`SearchCache`/`DbDefinitionBuilder` (Step 53). Remaining targets: `AccessMgmt.Persistence` (32.51%), `AccessManagement.Persistence` (44.94%) — both dominated by Npgsql repository code that needs a live DB. Also: `DtoMapper.Extract*`/`ConvertToOthers`/`ConvertFromOthers` (deferred; complex `Connection` graph setup); `DelegationCheckHelper.GetFirstAccessorValuesFromPolicy`/`DecomposePolicy`/`BuildDelegationRuleTarget` (deferred; require XACML object graph setup).
   - **6.7c (last follow-up):** `MaskinportenConsumersController` / `MaskinportenSuppliersController` — requires PDP stubbing or seeding of `altinn_maskinporten_scope_delegation` resource (controller-level integration test, distinct from the unit-test direction taken in Steps 31–33). See [33_Coverage_Enduser_Api_ToUuidResolver.md](33_Coverage_Enduser_Api_ToUuidResolver.md).
   - **6.7f (remaining):** ~~`PDP_Decision_ResourceRegistry_AccessListAuthorization_Json_PermitWithActionFilterMatch` flaky~~ — **fixed in Step 51** (`ResourceRegistryMock` cache-hit bug). `Sender_ConfirmsDraftRequest_ReturnsPending` remains `[Skip]`ped — separate environmental investigation needed.

See [12_AccessManagement_Coverage_Baseline_Success.md](12_AccessManagement_Coverage_Baseline_Success.md) for detailed coverage metrics.

### Blocked Items

| Item | Blocker | Notes |
|---|---|---|
| Phase 6.5: Host.Lease tests | Azure Storage Emulator/Azurite required | See [TESTING_INFRASTRUCTURE_OVERHAUL.md](../TESTING_INFRASTRUCTURE_OVERHAUL.md) Phase 6.5 |

### Final Coverage (measured)

**Altinn.Authorization app** (Phase 6 — CI-enforced):

| Assembly | Line% | Branch% | Threshold | Status |
|---|---|---|---|---|
| Altinn.Authorization | 70.91 | 70.93 | 60% | ✅ |
| Altinn.Authorization.ABAC | 63.41 | 63.83 | 60% | ✅ |
| Altinn.Authorization.PEP | 77.75 | 76.10 | 75% | ✅ |

**236 new tests** added across Phase 6 (184 Authorization + 52 PEP).

**AccessManagement app** — Step 12 baseline; four assemblies are now
**CI-enforced** as of Step 28 (see [28_CI_Coverage_Thresholds_AccessManagement.md](28_CI_Coverage_Thresholds_AccessManagement.md)).
The main app is a warning-only ratchet until it crosses 60%. Other assemblies
are tracked under priority 1 in [Recommended Next Steps](#recommended-next-steps-priority-order)
and will be enforced as their coverage improves. Source:
[12_AccessManagement_Coverage_Baseline_Success.md](12_AccessManagement_Coverage_Baseline_Success.md).

| Assembly | Line% | Branch% | Threshold | Status |
|---|---|---|---|---|
| Altinn.AccessMgmt.PersistenceEF | 98.59 | 90.78 | 90% (enforced) | ✅ |
| AccessManagement.Api.Maskinporten | 80.36 | 80.00 | 75% (enforced) | ✅ |
| AccessManagement.Api.Enterprise | 66.39 | 56.52 | 60% (enforced) | ✅ |
| AccessManagement.Core | 63.43 | 61.49 | 60% (enforced) | ✅ |
| AccessManagement (main app) | 58.19 | 60.93 | 60% (⚠ warn-only) | ⚠️ Near |
| AccessManagement.Integration | 47.57→↑ | 43.75→↑ | — | ⏫ Step 50 |
| AccessManagement.Api.Internal | 46.74→↑ | 46.20→↑ | — | ⏫ Step 49 |
| AccessManagement.Persistence | 44.94 | 30.23 | — | ❌ Gap |
| AccessMgmt.Persistence | 32.51 | 9.42 | — | ❌ Gap |
| AccessMgmt.Core | 17.31→↑ | 12.00→↑ | — | ⏫ Step 53 |
| AccessManagement.Api.Metadata | 16.59→↑ | 13.33→↑ | — | ⏫ Step 52/53 |
| AccessMgmt.Persistence.Core | 8.78→↑ | 3.21→↑ | — | ⏫ Step 53 |
| AccessManagement.Api.Enduser | 68.32 | 58.90 | — | ⏫ Step 33 |
| AccessManagement.Api.ServiceOwner | 71.74→↑ | 60.00→↑ | — | ⏫ Step 47 |
