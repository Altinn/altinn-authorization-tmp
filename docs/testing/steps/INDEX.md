# Testing Infrastructure Overhaul — Step Log

## Getting Started & Workflow

**New chat?** Read these docs **in order** to get full context:

1. **This file** (`docs/testing/steps/INDEX.md`) — step log, coverage results,
   recommended next steps, deferred work, and workflow rules.
2. **[TESTING_INFRASTRUCTURE_OVERHAUL.md](../TESTING_INFRASTRUCTURE_OVERHAUL.md)** —
   original audit, issue IDs (C1–C5, M1–M8, L1–L3), and the phase plan.
3. **The step doc for the work you're about to do** (linked in the table below or
   in the Recommended Next Steps section).

> **Handoff note (after Step 14):** Steps 1–14 complete! ✅ FluentAssertions v7.0.0 added to all test projects.
> **Package installed:** FluentAssertions is now available in all 11 test projects via global using directive.
> See [Add_FluentAssertions_Package.md](Add_FluentAssertions_Package.md) for implementation details.

**When completing a step:**

- **Create a step doc** (`docs/testing/steps/<Step_Name>.md`) describing the goal,
  what changed, verification results, and any deferred items. Add a row to the
  step log table below linking to the new doc.
- **Commit and push** at the end of each step.
- **Wait for explicit go-ahead** before proceeding to the next step.

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
| 7 | ✅ | Maximize code coverage (actionable items) | Phase 6 | [Maximize_Coverage.md](Maximize_Coverage.md) |
| 8 | ✅ | CI coverage threshold (6.6) | Phase 6 | [CI_Coverage_Threshold.md](CI_Coverage_Threshold.md) |
| 9 | ✅ | Shared fixture for Authorization.Tests | Phase 2.4 | [Shared_Fixture_Authorization.md](Shared_Fixture_Authorization.md) |
| 10 | ✅ | Dead code & suppressions cleanup (L1–L3) | Phase 4.5–4.6 | [Dead_Code_and_Suppressions_Cleanup.md](Dead_Code_and_Suppressions_Cleanup.md) |
| 11 | ✅ | Certificate consolidation — Authorization.Tests (M8) | Phase 3.5 | [Certificate_Consolidation.md](Certificate_Consolidation.md) |
| 12 | ✅ | AccessManagement coverage baseline with Podman (6.7a) | Phase 6 | [AccessManagement_Coverage_Baseline_Success.md](AccessManagement_Coverage_Baseline_Success.md) |
| 13 | ✅ | FluentAssertions evaluation | Phase 4.2 | [FluentAssertions_Evaluation.md](FluentAssertions_Evaluation.md) |
| 14 | ✅ | Add FluentAssertions package | Phase 4.2a | [Add_FluentAssertions_Package.md](Add_FluentAssertions_Package.md) |

### Final Coverage (measured)

| Assembly | Line% | Branch% | Threshold | Status |
|---|---|---|---|---|
| Altinn.Authorization | 70.91 | 70.93 | 60% | ✅ |
| Altinn.Authorization.ABAC | 63.41 | 63.83 | 60% | ✅ |
| Altinn.Authorization.PEP | 77.75 | 76.10 | 75% | ✅ |

**236 new tests** added across Phase 6 (184 Authorization + 52 PEP).

### Recommended Next Steps (priority order)

**🎯 Ready to Execute** (Podman Desktop working, all dependencies met)

1. **Phase 3.2–3.4 — Mock dedup implementation** (medium impact, straightforward)
   - Consolidate 8 duplicated AccessManagement mocks between `AccessMgmt.Tests/` and `TestUtils/`
   - **Status: Ready** — Was blocked by Testcontainers, now unblocked (Step 12)
   - Read [Mock_Deduplication_Audit.md](Mock_Deduplication_Audit.md) for strategy

2. **Phase 2.2–2.3 — AccessMgmt.Tests WAF consolidation** (complex, high impact)
   - Migrate AccessMgmt.Tests controller tests to `ApiFixture` pattern
   - **Status: Ready** — Podman + Testcontainers confirmed working (Step 12)
   - Read [Consolidate_WebApplicationFactory.md](Consolidate_WebApplicationFactory.md)

3. **Phase 4.2b — FluentAssertions guidelines** (quick documentation task)
   - Create usage guidelines and patterns documentation
   - **Status: Ready** — Package installed (Step 14), can document best practices
   - Read [Add_FluentAssertions_Package.md](Add_FluentAssertions_Package.md)

4. **Phase 6 coverage improvements** — Fill identified gaps (can use FluentAssertions!):
   - **6.7b:** AccessManagement.Api.ServiceOwner (0% coverage)
   - **6.7c:** AccessManagement.Api.Enduser (1.19% coverage)
   - **6.7d:** AccessMgmt persistence layers (8-45% coverage)

See [AccessManagement_Coverage_Baseline_Success.md](AccessManagement_Coverage_Baseline_Success.md) for detailed coverage metrics.

### Blocked Items

| Item | Blocker | Notes |
|---|---|---|
| Phase 6.5: Host.Lease tests | Azure Storage Emulator/Azurite required | See [TESTING_INFRASTRUCTURE_OVERHAUL.md](../TESTING_INFRASTRUCTURE_OVERHAUL.md) Phase 6.5 |
