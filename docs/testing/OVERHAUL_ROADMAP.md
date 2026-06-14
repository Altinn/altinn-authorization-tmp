# Testing-layer overhaul — roadmap

Tracking issue: **#3486 — Harden and consolidate the testing layer ahead of the production release.**

This is the living plan for the final major pass on the test stack before the
upcoming production release. The bar is thoroughness: infrastructure, coverage
of high-stakes code, conventions, and CI are each reviewed and brought to a
known-good state. Work is sequenced by **release risk** — additive tests and CI
reliability land first; structural churn is phased so it never lands on the eve
of a release cut.

New work items below become tracked Task issues **when they are picked up**, not
upfront — this doc is the exhaustive backlog; the board stays just-in-time.

## Coverage map (where the stack stands)

| Production area | Test home | State |
|---|---|---|
| `Altinn.Authorization` (PDP, controllers) | `Altinn.Authorization.Tests` | Covered; 60% floor |
| `Altinn.Authorization.ABAC` (XACML engine) | none dedicated; exercised via Authorization.Tests conformance suite | Indirect only (~63%); **direct project to be added** |
| `Altinn.Authorization.PEP` | `Altinn.Authorization.PEP.Tests` | Covered; 75% floor |
| `Altinn.AccessMgmt.Core` | `Altinn.AccessMgmt.Core.Tests` | Covered; 60% floor |
| `Altinn.AccessMgmt.PersistenceEF` | `Altinn.AccessMgmt.PersistenceEF.Tests` | Covered; 90% floor |
| AccessManagement APIs (Enduser/ServiceOwner/Internal/Enterprise/Maskinporten) | the AccessManagement test projects | Covered; Enterprise 60 / Maskinporten 75 floors |
| AccessManagement Metadata API | none | **Gap — not in coverage floors** |
| `Altinn.Authorization.Host.Database`, `.MassTransit` | none | **Gap — untested** |
| `Altinn.Authorization.Host.Pipeline` | `...Host.Pipeline.Tests` | Scaffold / smoke only |
| `Altinn.Authorization.Host.Lease` | `...Host.Lease.Tests` | Needs Azurite; excluded from CI matrix |
| `Altinn.Register` | none | Out of scope (vendored copy; real repo elsewhere) |

## Completed under this umbrella

- Shared `PostgresTestEngine` extracted and adopted (template-clone provisioning). — #3412 stream
- Repo-wide test-method naming normalized to `MethodUnderTest_Scenario_ExpectedResult`. — #3463
- Integration-test measurement, fixture sharing, parallelism, Task.Delay audit, mock-catalog. — #3379 / #3452 stream (PR #3456)
- Validation-asserter defects fixed (`Any` never failed; wrong error key). — #3468
- Consent `Expired` status added to the enterprise API contract enum + sync test. — #3473 (PR #3474)
- Input-guard pure-logic tests (SystemUserId, IdentifierUtil, URN parsers, etc.). — #3475 (PR #3476)
- ABAC `string-is-in` corrected to bag membership, conformance-validated 36/36. — #3481 (PR #3482)
- Flaky wall-clock assertions removed; shared-mock reset hazards fixed. — #3464
- Stateless mocks defaulted in the base fixture; redundant per-class registrations removed; host-build count reduced + baselined. — #3465
- Dead `AssertionUtil` helpers removed (the rest are live method-group delegates, so full retirement = rewriting `AssertCollections` call sites — deferred). — #3466
- Test-naming CI guard added (`.github/scripts/check-test-naming.sh`), gated to the AccessManagement vertical. — #3467
- Two-lane (unit/integration) CI, per-assembly coverage ratchet, SonarCloud nightly-on-main, docs/testing de-stale.

## Phase 1 — Harden & cover (release-safe)

Additive tests and CI-reliability fixes only; no change to how existing tests are wired.

1. **Stand up a dedicated ABAC test project** for the XACML decision engine —
   rule-combining, target match, condition eval, datatype mismatch, fail-open
   (missing attribute → Indeterminate), and the attribute-match functions. The
   engine is the highest-stakes code in the repo and is currently covered only
   indirectly. — #3132 (builds on #3481)
2. **Convert remaining no-Docker fixture paths to per-test skips.**
   `EFPostgresFactory` / `ApiFixture` still surface a missing container runtime
   as a fixture-init failure rather than a skip (only `AuthorizationDbFixture`
   was fixed). — #2810
3. **Close concrete pure-logic / API coverage gaps.** — #2975 (GetConsentStatusChanges),
   #2976 (AM API + persistence cores), plus the Metadata API gap.
4. **Audit the skipped tests** (~10 `[Fact(Skip)]`) — re-enable or document a
   justification for each.
5. **Close documentation gaps** (see below).

## Phase 2 — Consolidate (structural; land with soak time)

Touches shared test plumbing; must not land immediately before a release cut.

1. **Audit and consolidate test infrastructure** across all test projects. — #3412
2. **Restructure the AccessManagement suite around host profiles + owned data**;
   settle the host-profile architecture (see open decision). — #3452
3. **Speed up runs** — amortize host setup, tune parallelism and container
   concurrency / connection-pool sizing under full CI fan-out. — #3379
4. **Retire `LegacyApiFixture`** — migrate the remaining Yuniql-dependent tests
   to EF seed data. — follow-up to #3410
5. **Extend the test-naming guard** beyond the AccessManagement vertical — the
   guard (`.github/scripts/check-test-naming.sh`, #3467) is in place but gated to
   one vertical; widen its coverage to the rest.
6. **Retire the legacy `AssertionUtil` helpers** (~1,500 lines across two
   classes) in favour of FluentAssertions — the dead helpers are gone (#3466);
   the remaining ones are live method-group delegates, so this means rewriting
   the `AssertCollections` call sites to `BeEquivalentTo`.
7. **Reorganize PR #3456** into reviewable, theme-aligned PRs.

## Open decisions

- **Host-profile architecture.** The original design doc was removed when the
  earlier tracking docs were cleaned up. Before Phase 2.2 we re-establish (or
  formally drop) the target host-profile catalog and the owned-data model under
  #3452.
- **PR #3456 split.** A mega-PR is a review bottleneck close to a release.
  Decide split vs. land-as-is before the release window closes.

## Documentation gaps to close (Phase 1.5)

- Host-profile catalog and the seed-data contract (what the template actually seeds).
- The `dotnet test --filter` → `-- --filter-class` quirk for the MTP projects
  (GETTING_STARTED currently shows a misleading `--filter` example).
- Coverage *philosophy*: coverage % is a ratchet, not a goal; tests target named
  bug classes; avoid low-value pass-through mock tests.
- A flaky-test register (e.g. the historical seed race fixed by the advisory lock).
- Remove the orphaned "Phase 4.2d" pointer in FLUENT_ASSERTIONS_GUIDELINES.md.
- Drop the stale 9.0-SDK requirement note in GETTING_STARTED.

## Open testing PRs

| PR | Scope | Phase |
|---|---|---|
| #3482 | ABAC `string-is-in` membership fix | done (P1) |
| #3476 | pure-logic input-guard tests | done (P1) |
| #3474 | consent `Expired` enum fix | done (P1) |
| #3456 | measurement / fixture-sharing / parallelism / naming | P2 (split candidate) |
