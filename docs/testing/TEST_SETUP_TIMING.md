# Integration-test setup timing (measurement)

Measurement step for [#3379](https://github.com/Altinn/altinn-authorization-tmp/issues/3379):
size where integration-test wall-clock goes **before** investing in a
fixture-sharing refactor. Instrumented by `FixtureTiming` (linked alongside
`PostgresTestEngine`); see [#3446](https://github.com/Altinn/altinn-authorization-tmp/issues/3446).

## Result

`AccessMgmt.Tests`, `Category=Integration` — **972 tests, ~2m05s**, run locally
against Podman (`maxParallelThreads = 4`):

| Phase | Total | Count | Avg | Notes |
|---|---:|---:|---:|---|
| App-host build (`WebApplicationFactory`) | **92.2 s** | 68 | **1,356 ms** | one per `IClassFixture` |
| DB provision (`InitializeAsync` → factory) | 56.2 s | 68 | 826 ms | includes template + clone |
| DB clone (`CREATE DATABASE … WITH TEMPLATE`) | **18.4 s** | 80 | **230 ms** | per provisioned database |
| Template build (migrate + seed) | 9.3 s | 1 | — | one-time per process |

## Conclusion

- **Host build dominates setup — ~5× the total clone cost (92 s vs 18 s).**
  The DB layer is already cheap: a clone is ~230 ms, and the migrated/seeded
  template is built once (~9 s). This confirms the #3379 hypothesis — the
  bottleneck is the **68 unshared `WebApplicationFactory` host builds**, not the
  database.
- The first host build is much slower (~13 s cold, JIT + EF model warm-up); the
  amortized average across 68 fixtures is ~1.36 s.
- This run covers `AccessMgmt.Tests` only (68 host-building fixtures). The other
  AccessManagement test projects add more `ApiFixture` instances — the
  repo-wide figure cited in #3379 is 111 `IClassFixture` / 85 `ApiFixture`,
  every one a separate cold host build.

## Prototype: fixture sharing (validated)

Sharing one host across a read-only, additive-seed cohort via
`ICollectionFixture` collapses N cold host builds into one. Prototyped on the
`ConnectionsController` cluster (`Enduser.Api.Tests`): the **7** nested classes
that are read-only, do not call `ConfigureServices`, and apply no per-class
configuration (`CheckPackage`, `DelegationCheckRoles`, `GetAvailableUsers`,
`GetConnections`, `GetInstances`, `GetPackages`, `GetRoles`) now join a single
`ConnectionsReadOnlyCollection`. The other classes — `Add*`/`Remove*` (mutating)
and any calling `ConfigureServices` — stay on `IClassFixture`.

| Metric | Before | After |
|---|---:|---:|
| Test result | 143 passed / 5 skipped | **143 passed / 5 skipped** |
| Wall clock | 1m 02s | **35.8 s** (−42%) |
| Host builds (`host_build_n`) | 25 | **19** (−6) |
| Host-build work | 129 s | 52 s |

Identical pass/skip counts — no seed collisions, no isolation regression. The
saving exceeds the 6 eliminated builds because removing them also cut CPU
contention on the remaining 19 (avg build 5.2 s → 2.8 s). Clone/template work is
untouched; it is already cheap.

**Shareability rule (for rolling this out):** a class can join the shared
collection only if it (a) seeds additively via `EnsureSeedOnce<TSelf>`, (b) never
calls `ConfigureServices` / `WithAppsettings` / `With*FeatureFlag`, and (c) issues
no writes against rows other members read. Anything else keeps its own
`IClassFixture`. The next #3379 sub-task rolls this rule out across the broader
cohort.

## Reproduce

`FixtureTiming` is opt-out (disable with `FIXTURE_TIMING=off`) and writes one
summary line to stdout and, when `FIXTURE_TIMING_FILE` is set, appends it to
that file (stdout is swallowed by MTP / `dotnet-coverage` in CI).

```bash
# Local run against a container runtime (here: Podman; Docker Desktop works too).
FIXTURE_TIMING_FILE="$PWD/fixture-timing.txt" \
DOCKER_HOST="npipe://./pipe/podman-machine-default" \
TESTCONTAINERS_RYUK_DISABLED=true \
dotnet test src/apps/Altinn.AccessManagement/test/AccessMgmt.Tests/AccessMgmt.Tests.csproj \
  -c Release -- --filter-trait "Category=Integration" --ignore-exit-code 8
```

Numbers are from local Podman; absolute values differ on CI hardware, but the
**ratio** (host build ≫ clone) is the hardware-independent finding.
