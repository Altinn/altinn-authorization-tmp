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
| 1 | ✅ | Create overhaul plan & audit | Phase 0 | [Create_Overhaul_Plan.md](Create_Overhaul_Plan.md) |
| 2 | ✅ | Unify xUnit v3 & net9.0 TFM | Phase 1 | [Unify_xUnit_and_TFM.md](Unify_xUnit_and_TFM.md) |
| 3 | ✅ | Consolidate WebApplicationFactory (Authorization.Tests) | Phase 2 | [Consolidate_WebApplicationFactory.md](Consolidate_WebApplicationFactory.md) |
| 4 | ✅ | Mock deduplication audit | Phase 3 | [Mock_Deduplication_Audit.md](Mock_Deduplication_Audit.md) |
| 5 | ✅ | Coverage infrastructure (`dotnet-coverage`, `run-coverage.ps1`) | Phase 5 | [Coverage_Infrastructure.md](Coverage_Infrastructure.md) |
| 6 | ✅ | Test patterns, naming convention & csproj cleanup | Phase 4 | [Test_Patterns_and_Naming.md](Test_Patterns_and_Naming.md) |
| 7 | ✅ | Maximize code coverage (actionable items) | Phase 6.1–6.5 | [Maximize_Coverage.md](Maximize_Coverage.md) |
| 8 | ✅ | CI coverage threshold (6.6) | Phase 6.6 | [CI_Coverage_Threshold.md](CI_Coverage_Threshold.md) |
| 9 | ✅ | Shared fixture for Authorization.Tests | Phase 2.4 | [Shared_Fixture_Authorization.md](Shared_Fixture_Authorization.md) |
| 10 | ✅ | Dead code & suppressions cleanup (L1–L3) | Phase 4.5–4.6 | [Dead_Code_and_Suppressions_Cleanup.md](Dead_Code_and_Suppressions_Cleanup.md) |
| 11 | ✅ | Certificate consolidation — Authorization.Tests (M8) | Phase 3.5 | [Certificate_Consolidation.md](Certificate_Consolidation.md) |
| 12 | ✅ | AccessManagement coverage baseline with Podman (6.7a) | Phase 6.7a | [AccessManagement_Coverage_Baseline_Success.md](AccessManagement_Coverage_Baseline_Success.md) |
| 13 | ✅ | FluentAssertions evaluation | Phase 4.2 | [FluentAssertions_Evaluation.md](FluentAssertions_Evaluation.md) |
| 14 | ✅ | Add FluentAssertions package | Phase 4.2a | [Add_FluentAssertions_Package.md](Add_FluentAssertions_Package.md) |
| 15 | ✅ | Mock deduplication implementation | Phase 3.2–3.4 | [Mock_Deduplication_Implementation.md](Mock_Deduplication_Implementation.md) |
| 16 | ✅ | AccessMgmt.Tests WAF consolidation — plan + `ResourceControllerTest` POC | Phase 2.2 | [AccessMgmt_WAF_Consolidation_Plan_and_POC.md](AccessMgmt_WAF_Consolidation_Plan_and_POC.md) |
| 17 | ✅ | Sub-step 16.1 — Group A easy wins (`PolicyInformationPointControllerTest`, `DelegationsControllerTest`) | Phase 2.2 | [AccessMgmt_WAF_Group_A_Easy_Wins.md](AccessMgmt_WAF_Group_A_Easy_Wins.md) |
| 18 | ✅ | Sub-step 16.2a — Group A single-configuration migrations (`Altinn2RightsControllerTest`, `AppsInstanceDelegationControllerTest`) | Phase 2.2 | [AccessMgmt_WAF_Group_A_Single_Config.md](AccessMgmt_WAF_Group_A_Single_Config.md) |
| 19 | ✅ | Sub-step 16.2b — Group A nested-class splits (`MaskinportenSchemaControllerTest`, `RightsInternalControllerTest`); `CustomWebApplicationFactory` deleted | Phase 2.2 | [AccessMgmt_WAF_Group_A_Nested_Splits.md](AccessMgmt_WAF_Group_A_Nested_Splits.md) |
| 20 | ✅ | Sub-step 16.3 — Group B simple (`HealthCheckTests`, `PartyControllerTests`) | Phase 2.2 | [AccessMgmt_WAF_Group_B_Simple.md](AccessMgmt_WAF_Group_B_Simple.md) |
| 21 | ✅ | Sub-step 16.4 investigation — Group B scenario-based consumers blocked on Yuniql schema provisioning in `ApiFixture` | Phase 2.2 | [AccessMgmt_WAF_Group_B_Scenarios_16_4_Investigation.md](AccessMgmt_WAF_Group_B_Scenarios_16_4_Investigation.md) |
| 22 | ✅ | Sub-step 16.4-prep — `LegacyApiFixture` plumbing (Yuniql + EF schema) | Phase 2.2 | [AccessMgmt_WAF_Group_B_16_4_Prep_LegacyApiFixture.md](AccessMgmt_WAF_Group_B_16_4_Prep_LegacyApiFixture.md) |
| 23 | ✅ | Sub-step 16.4a — Migrate `V2ResourceControllerTest`, `ConsentControllerTestEnterprise`, `MaskinPorten.ConsentControllerTest` to `LegacyApiFixture` | Phase 2.2 | [AccessMgmt_WAF_Group_B_16_4a_Consent_Migrations.md](AccessMgmt_WAF_Group_B_16_4a_Consent_Migrations.md) |
| 24 | ⚠️ Partial | Sub-step 16.4b — Delete two 100%-`[Skip]`ped WAF consumers; `ConsentControllerTestBFF` migration blocked on per-test DB isolation gap | Phase 2.2 | [AccessMgmt_WAF_Group_B_16_4b_Final_Consumers.md](AccessMgmt_WAF_Group_B_16_4b_Final_Consumers.md) |
| 25 | ✅ | Sub-step 16.4b-continued — `ConsentControllerTestBFF` migrated to per-test `LegacyApiFixture` via `IAsyncLifetime`; `WebApplicationFixture` has no remaining consumers | Phase 2.2 | [AccessMgmt_WAF_16_4b_Continued_BFF_Migration.md](AccessMgmt_WAF_16_4b_Continued_BFF_Migration.md) |
| 26 | ✅ | Sub-step 16.5 — Retired `WebApplicationFixture`, `AcceptanceCriteriaComposer`, `Scenarios/*`, `ControllerTestTemplate`; `PostgresServer` retained (still used by `PostgresFixture`) | Phase 2.2 | [AccessMgmt_WAF_16_5_Retire_Legacy_Harness.md](AccessMgmt_WAF_16_5_Retire_Legacy_Harness.md) |

### Recommended Next Steps (priority order)

All items below are actionable and have no container-runtime dependency.

1. **Phase 4.2b — FluentAssertions guidelines**
   - Create usage guidelines and patterns documentation
   - **Status: Ready** — Package installed (Step 14), can document best practices
   - Read [Add_FluentAssertions_Package.md](Add_FluentAssertions_Package.md)

2. **Phase 5.1b — CI coverage thresholds for AccessManagement** (lock in the ratchet)
   - Extend the CI coverage gate (Step 8) to the 4 AccessManagement assemblies already above 60%:
     - `Altinn.AccessMgmt.PersistenceEF` → 90% (currently 98.59)
     - `AccessManagement.Api.Maskinporten` → 75% (currently 80.36)
     - `AccessManagement.Api.Enterprise` → 60% (currently 66.39)
     - `AccessManagement.Core` → 60% (currently 63.43)
   - Leave `AccessManagement` (main app, 58.19%) as a **warning-only** ratchet until it crosses 60%.
   - Do **not** enforce thresholds on the low-coverage assemblies — those are covered by 6.7b–6.7d below.
   - Maps to overhaul plan Phase 5.1 / 5.4. Prevents regression on assemblies we've already invested in, without blocking CI on known gaps.
   - Read [CI_Coverage_Threshold.md](CI_Coverage_Threshold.md) for the existing Authorization-app threshold pattern.

3. **Phase 6 coverage improvements** — Fill identified gaps (can use FluentAssertions!):
   - **6.7b:** AccessManagement.Api.ServiceOwner (0% coverage)
   - **6.7c:** AccessManagement.Api.Enduser (1.19% coverage)
   - **6.7d:** AccessMgmt persistence layers (8-45% coverage)

See [AccessManagement_Coverage_Baseline_Success.md](AccessManagement_Coverage_Baseline_Success.md) for detailed coverage metrics.

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

**AccessManagement app** — Step 12 baseline, **not yet CI-enforced**. Numbers are a
point-in-time measurement from Phase 6.7a; the `Target` column is the aspirational
threshold we'd enforce in CI once reached (see priority 3 in
[Recommended Next Steps](#recommended-next-steps-priority-order)). Source:
[AccessManagement_Coverage_Baseline_Success.md](AccessManagement_Coverage_Baseline_Success.md).

| Assembly | Line% | Branch% | Target | Status |
|---|---|---|---|---|
| Altinn.AccessMgmt.PersistenceEF | 98.59 | 90.78 | 60% | ✅ |
| AccessManagement.Api.Maskinporten | 80.36 | 80.00 | 60% | ✅ |
| AccessManagement.Api.Enterprise | 66.39 | 56.52 | 60% | ✅ |
| AccessManagement.Core | 63.43 | 61.49 | 60% | ✅ |
| AccessManagement (main app) | 58.19 | 60.93 | 60% | ⚠️ Near |
| AccessManagement.Integration | 47.57 | 43.75 | 60% | ❌ Gap |
| AccessManagement.Api.Internal | 46.74 | 46.20 | 60% | ❌ Gap |
| AccessManagement.Persistence | 44.94 | 30.23 | 60% | ❌ Gap |
| AccessMgmt.Persistence | 32.51 | 9.42 | 60% | ❌ Gap |
| AccessMgmt.Core | 17.31 | 12.00 | 60% | ❌ Gap |
| AccessManagement.Api.Metadata | 16.59 | 13.33 | 60% | ❌ Gap |
| AccessMgmt.Persistence.Core | 8.78 | 3.21 | 60% | ❌ Gap |
| AccessManagement.Api.Enduser | 1.19 | 0.15 | 60% | ❌ Gap |
| AccessManagement.Api.ServiceOwner | 0.00 | 0.00 | 60% | ❌ Gap |
