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

## Implication for the next step

Sharing one host across the read-only, additive-seed cohort via
`ICollectionFixture` collapses N cold host builds into one. If ~⅔ of the 68
fixtures here are shareable, that removes ~40–45 host builds (~60 s of
serial-equivalent build work) — the lever the next #3379 sub-task prototypes
and measures. Clone/template work is left alone; it is already cheap.

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
