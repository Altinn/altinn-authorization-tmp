# Testing Infrastructure Overhaul — Step Log

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
| 7 | ⬜ | Maximize code coverage | Phase 6 | _not started_ |

### Next Step

**Step 7 — Maximize Code Coverage (Phase 6)**. Detailed execution plan, baseline
numbers, and prioritized sub-steps are in
[Test_Patterns_and_Naming.md → "Next Phase" section](Test_Patterns_and_Naming.md#next-phase-phase-6--maximize-code-coverage).
The full Phase 6 checklist (6.1–6.6) is in the
[overhaul plan](../TESTING_INFRASTRUCTURE_OVERHAUL.md#phase-6-maximize-code-coverage).

Start by reading `docs/testing/steps/INDEX.md` and the linked sections above.

### Deferred Work

| Item | Reason | Tracked In |
|---|---|---|
| Phase 2.2–2.3: AccessMgmt.Tests WAF consolidation | Complex; different mock/data strategy | [Consolidate_WebApplicationFactory.md](Consolidate_WebApplicationFactory.md) |
| Phase 3.2–3.4: Mock dedup implementation | Blocked until AccessMgmt.Tests migrates to ApiFixture | [Mock_Deduplication_Audit.md](Mock_Deduplication_Audit.md) |

### Workflow

- **Commit and push** at the end of each step.
- **Wait for explicit go-ahead** before proceeding to the next step.
