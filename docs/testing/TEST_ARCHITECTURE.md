# Test Architecture — Target Structure

Status: **in progress** — Feature #3452 with sized migration Tasks; Phase 2a (the
project-local mock-catalog fixture) is implemented in PR #3456.
Scope: the AccessManagement integration/web-app test suite. Authorization is
referenced as the pattern to copy — specifically its **project-local** fixture
(`AuthorizationApiFixture`), which bakes its own mock graph in rather than
centralising mocks in `TestUtils`.

## 1. Why

The suite's cost and flakiness are dominated by one number — **how many web-host
builds it does** (~71, at ~1.36 s each; DB clones are cheap at 230 ms, template is
a one-time ~9 s). The #3379 measurement proved this, and the incremental fixture
sharing there confirmed that ad-hoc cohort sharing plateaus, because the structure
fights it. This document is the structure those measurements point to.

## 2. Current state (measured)

| Fact | Value |
|---|---|
| `IClassFixture<ApiFixture>` classes (≈ host builds) | ~71 across 45 files |
| Per-fixture host build (dominant cost) | ~1.36 s avg (12.9 s cold) |
| DB clone / template build | 230 ms / 9 s one-time |
| Classes calling `ConfigureServices` | 39 files |
| Distinct mock interfaces across *all* those calls (the catalog) | **15** (+ 4 `RemoveAll`) |
| API feature-flag axes | ~5 |
| Behavioral-override files (`RemoveAll<…>`, non-default behavior) | ~12 |
| Fixture files that never seed/query the DB | 19 (4 clearly mock the repo layer) |
| `ApiFixture` / `LegacyApiFixture` files | 52 / 8 |

## 3. Root cause

**The test class is the unit of everything** — host config, seed data, isolation,
and assertion scope are all bound to it. Every wall the incremental work hit is a
symptom:

1. **Under-provisioned base host → artificial DI divergence.** `ApiFixture`
   registers only auth stubs + a permit-PDP, so each class patches in whichever of
   the **same 15-mock catalog** it needs via `ConfigureServices`. The 39 calls are
   *subsets of one catalog*, not 39 configs — but each subset forces a unique host.
   **`AuthorizationApiFixture` already bakes its full mock graph in**, proving the
   divergence is avoidable.
2. **Grouping by API operation, not compatibility.** `Connections` is ~20 classes
   (`Get`/`Check`/`Add`/`Remove`/`Update`), each rebuilding the whole host.
3. **Class-coupled data + global assertions.** `EnsureSeedOnce<TSelf>` + exact /
   count assertions mean shared data breaks tests — sharing is hostile by
   construction.
4. **DB clone even when unused.** `InitializeAsync` always clones Postgres; ~19
   files mock the data layer.
5. **No single convention.** `ApiFixture`, `LegacyApiFixture` (8), scenario/E2E
   tests, and the mock-based Authorization fixture coexist.

**Headline: ~71 host builds are not ~71 configs — they are ~10–12 real profiles,
multiplied out by per-class mock-patching and per-operation splitting.**

## 4. Principles

- **Decouple three axes:** host configuration, data, isolation.
- **Shared-by-default, divergence-by-exception.**
- **Own your data:** a test reads/writes only entities it uniquely owns and asserts
  only against them — so sharing is tolerant by construction.
- **Pay only for what you use:** no DB for tests that don't touch it.

## 5. Target architecture

**A. Host profiles (finite, named).** A **project-local** base fixture
(`AccessMgmtApiFixture : ApiFixture`, mirroring `AuthorizationApiFixture`) registers
the external-platform client catalog once — the mocks stay in the test project
beside their data, not centralised in `TestUtils`. Most classes then need zero
`ConfigureServices` and join one shared host. Genuine variation becomes a small
enumerated set of collection-fixture profiles:

| Profile | Driver | Approx. classes |
|---|---|---|
| `Default` (full mocks, permit PDP) | no flags, no override | ~40+ |
| `Altinn2RoleRevoke` (on/off) | feature flag | ~7 |
| `EnableRequestAssignmentResource` / `…Package` / `EnableEnduserMaskinportenAdminApi` | feature flags | ~3 each |
| PDP / resource-registry / http-context overrides | `RemoveAll<…>` behavior | ~12 spread over a few |

→ **~71 host builds collapse to ~10–12.**

**B. Data ownership.** Rich shared baseline template + each test creating entities
under **unique IDs** and asserting only against them. Additive seeds never collide;
exact assertions become safe; **read/write distinction dissolves** (a test that
mutates only its own rows can share too).

**C. Isolation by exception.** Shared host + owned data is the default. Only tests
that mutate *global* state or need a divergent host get a dedicated fixture.

**D. Two tiers.**
- **Web-app tier (no DB):** mock-the-data-layer tests — no Postgres clone, no
  DB-connected host.
- **DB-integration tier:** real `AppDbContext` + migrations.

**E. One convention.** Retire `LegacyApiFixture`; fold scenario/E2E in.

## 6. Before / after (author-facing)

```csharp
// BEFORE — own host, patches mocks, seeds + asserts globally
public class GetInstanceRights : IClassFixture<ApiFixture> {
    public GetInstanceRights(ApiFixture f) {
        f.ConfigureServices(s => {                 // forces a unique host
            s.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
            s.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
            s.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        });
        f.EnsureSeedOnce<GetInstanceRights>(db => /* shared entities */);
    }
}

// AFTER — shared host (catalog pre-registered), owns its data, scoped assert
[Collection(DefaultHost.Name)]                     // one host for the whole profile
public class GetInstanceRights(DefaultHost host) {
    [Fact] public async Task ... {
        var party = host.NewOwnedOrg();            // unique id; no collision possible
        // seed under `party`; assert only on `party`'s results
    }
}
```

## 7. Expected impact

- **Host builds ~71 → ~10–12** — a structural ~6× cut (vs ~9 recovered by cohort
  sharing).
- **Flakiness:** the seed-interference failure class disappears; #3376 pressure drops.
- **Parallelism:** with far fewer host builds, the single-Postgres-container plateau
  stops being the ceiling.
- **DB-less tier** removes unnecessary clones + DB-connected host builds.

## 8. Migration plan (sized → Tasks)

Each phase is independently shippable, kept green, and measured with `FixtureTiming`.
Sizing is files/classes touched in AccessManagement.

| # | Phase | Scope (sized) | Win | Risk | Task granularity |
|---|---|---|---|---|---|
| 1 | Enumerate profiles | done (this doc): ~10–12 | unblocks the rest | — | folded into the Feature |
| 2 | **Provision the `Default` host** — bake the external-client catalog into a project-local base fixture (`AccessMgmtApiFixture`; mocks stay in the test project, not `TestUtils`); delete now-redundant `ConfigureServices` | 39 files | collapses ~40+ classes → 1 host (largest single win) | low — additive registration, validate per project | 1 Task (1 PR) |
| 3 | **Owned-data + scoped assertions** — replace `EnsureSeedOnce<TSelf>` global seeds/asserts with per-test owned entities | 33 seeding files | removes condition-4 fragility; enables write-sharing | med — most careful; per-controller | 3–4 Tasks (per project/controller cluster) |
| 4 | **DB-less web-app tier** — no-DB fixture for mock-the-data-layer tests | 4 clear, up to 19 candidates | drops clones + DB hosts | low | 1 Task |
| 5 | **Define profiles as collection fixtures; convert classes to `[Collection]`** — *supersedes #3449's per-controller cohorts* | all ~71 | realizes the 71 → ~12 collapse | low after 2–3 | 2 Tasks (per project) |
| 6 | **Retire `LegacyApiFixture`; unify scenario/E2E** | 8 + scenario | one convention | low | 1 Task |

Suggested Feature → ~8–9 Tasks. Order matters: **2 → 3 → 4/5 → 6** (provision the
host first; then make data shareable; then collapse onto profiles).

Status: Phase 1 and Phase 2a are done — Phase 2a is the project-local
`AccessMgmtApiFixture` catalog (#3454), shipped in PR #3456 (972 integration tests
green). Phase 2b (#3453) is in progress. Remaining: #3459 (owned data), #3458
(DB-less tier), #3460 (profiles), #3461 (retire `LegacyApiFixture`); plus #3457 (CI
host-build guard) and #3462 (robust test-data paths).

## 9. Risks & open items

- **Scope:** touches most test classes — phase per project, stay green.
- **Discipline:** owned-data + no-per-class-DI must be conventions (review/lint) or
  it rots back.
- **Open numbers to confirm during Phase 1/2:** exact profile count (≤~12); the
  DB-less set (4 confirmed, 19 candidates); any mock needing *per-test behavior*
  (vs one impl) — those stay isolated.
- **Relationship to PR #3456:** that PR's measurement, `FixtureTiming`, the two
  validated cohorts, and Phase 2a (the `AccessMgmtApiFixture` catalog) ship there.
  Phase 5 supersedes the per-controller rollout (#3449) with profile-based collections.
- **Project-local mocks, not `TestUtils`:** the base catalog lives in a project-local
  fixture (`AccessMgmtApiFixture`, the `AuthorizationApiFixture` pattern). Moving the
  AccessMgmt mocks up into `TestUtils` was rejected — shared test data (`Data/Parties`
  is also read by `TestDataUtil`), ~14 registration sites, and fragile cross-assembly
  `..` path navigation (see #3462) make centralisation costly for no benefit.

## Related

- [TEST_SETUP_TIMING.md](TEST_SETUP_TIMING.md) — the measurements this is built on.
- [CI.md](CI.md), [../SONARCLOUD.md](../SONARCLOUD.md).
