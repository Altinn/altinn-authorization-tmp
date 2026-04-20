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
| 7 | 🔄 | Maximize code coverage | Phase 6 | [Maximize_Coverage.md](Maximize_Coverage.md) |
| 8 | ✅ | CI coverage threshold (6.6) | Phase 6 | [CI_Coverage_Threshold.md](CI_Coverage_Threshold.md) |

### Next Step

**Step 7 — Maximize Code Coverage (Phase 6)** is in progress.
Completed sub-steps: **6.3** (PEP sprint, 60→79%), **6.4** (Authorization.Tests +66 tests), **6.6** (CI threshold enforcement), **6.7a** (Authorization infra/utility +30 tests), **6.7b** (Authorization models/services +23 tests), **6.7c** (PolicyInformationPoint +7 tests), and **6.7d** (AccessListAuthorization +5 tests).
Next actionable: additional Authorization service-layer coverage (`DelegationContextHandler`, `ContextHandler` gap analysis), **6.1 full baseline** (needs Docker), **6.5 Host.Lease tests** (needs storage account), or return to deferred work.

See [Maximize_Coverage.md → Next Step](Maximize_Coverage.md#next-step) for details.

Start by reading `docs/testing/steps/INDEX.md` and `docs/testing/steps/Maximize_Coverage.md`.

### Deferred Work

| Item | Reason | Tracked In |
|---|---|---|
| Phase 2.2–2.3: AccessMgmt.Tests WAF consolidation | Complex; different mock/data strategy | [Consolidate_WebApplicationFactory.md](Consolidate_WebApplicationFactory.md) |
| Phase 3.2–3.4: Mock dedup implementation | Blocked until AccessMgmt.Tests migrates to ApiFixture | [Mock_Deduplication_Audit.md](Mock_Deduplication_Audit.md) |

### Workflow

- **Commit and push** at the end of each step.
- **Wait for explicit go-ahead** before proceeding to the next step.
