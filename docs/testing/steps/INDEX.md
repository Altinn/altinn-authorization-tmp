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
| 7 | ✅ | Maximize code coverage (actionable items) | Phase 6 | [Maximize_Coverage.md](Maximize_Coverage.md) |
| 8 | ✅ | CI coverage threshold (6.6) | Phase 6 | [CI_Coverage_Threshold.md](CI_Coverage_Threshold.md) |

### Final Coverage (measured)

| Assembly | Line% | Branch% | Threshold | Status |
|---|---|---|---|---|
| Altinn.Authorization | 70.91 | 70.93 | 60% | ✅ |
| Altinn.Authorization.ABAC | 63.41 | 63.83 | 60% | ✅ |
| Altinn.Authorization.PEP | 77.75 | 76.10 | 75% | ✅ |

**236 new tests** added across Phase 6 (184 Authorization + 52 PEP).

### Recommended Next Steps (priority order)

1. **Phase 2.4 — Shared fixture for Authorization.Tests** (high impact, no Docker)
   - Fixes audit issue M5: factory rebuilt per-test. Create `AuthorizationFixture`.
2. **Phase 3.2–3.4 — Mock dedup implementation** (medium impact)
   - Consolidate 9+ duplicated mock interfaces into shared library.
3. **Phase 4 cleanup — dead code & suppressions** (low effort)
   - `GlobalSuppressions.cs`, `Compile Remove`, empty `Folder` includes.
4. **Blocked items** (when Docker/storage available)
   - 6.1 + 6.7g: AccessManagement coverage, 6.5: Host.Lease tests.
   - Phase 2.2–2.3: AccessMgmt.Tests WAF consolidation.

See [Maximize_Coverage.md → Recommended Next Steps](Maximize_Coverage.md#recommended-next-steps-priority-order) for details.

Start by reading `docs/testing/steps/INDEX.md` and `docs/testing/steps/Maximize_Coverage.md`.

### Deferred Work

| Item | Reason | Tracked In |
|---|---|---|
| Phase 2.2–2.3: AccessMgmt.Tests WAF consolidation | Complex; needs Docker | [Consolidate_WebApplicationFactory.md](Consolidate_WebApplicationFactory.md) |
| Phase 3.2–3.4: Mock dedup implementation | Ready to start (no longer blocked) | [Mock_Deduplication_Audit.md](Mock_Deduplication_Audit.md) |

### Workflow

- **Create a step doc** (`docs/testing/steps/<Step_Name>.md`) for each new step.
  The doc should describe the goal, what changed, verification results, and any
  deferred items. Add a row to the step log table above linking to the new doc.
- **Commit and push** at the end of each step.
- **Wait for explicit go-ahead** before proceeding to the next step.
