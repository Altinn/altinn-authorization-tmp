# Code Coverage

The repo uses `dotnet-coverage` to produce Cobertura reports and a small
PowerShell wrapper to parse them and enforce per-assembly thresholds. CI fails
the build if any **enforced** assembly drops below its floor.

## Files

| File | What it is |
|---|---|
| [`eng/testing/coverage-thresholds.json`](../../eng/testing/coverage-thresholds.json) | Source of truth for per-assembly minimum line coverage |
| [`eng/testing/check-coverage-thresholds.ps1`](../../eng/testing/check-coverage-thresholds.ps1) | Parse-only script (reads a Cobertura file, pretty-prints a table, enforces thresholds, exits non-zero on failure) |
| [`eng/testing/run-coverage.ps1`](../../eng/testing/run-coverage.ps1) | **Local-dev** helper: builds every `*.Tests` project, runs each under `dotnet-coverage collect` in parallel, then invokes the check script |
| [`eng/testing/run-accessmanagement-coverage.ps1`](../../eng/testing/run-accessmanagement-coverage.ps1) | Local-dev helper scoped to the AccessManagement vertical (faster feedback) |
| [`eng/testing/coverage.settings`](../../eng/testing/coverage.settings) | `dotnet-coverage` collection settings (passed via `--settings`) that exclude generated code / migrations / tooling from the line totals, so the floors measure the same denominator as SonarCloud |

## Thresholds

See [`coverage-thresholds.json`](../../eng/testing/coverage-thresholds.json).
Two sections:

### `assemblies` — **enforced** (CI-breaking)

```json
{
  "Altinn.Authorization": 68,
  "Altinn.Authorization.ABAC": 60,
  "Altinn.Authorization.PEP": 75,
  "Altinn.AccessMgmt.PersistenceEF": 94,
  "Altinn.AccessManagement.Api.Maskinporten": 75,
  "Altinn.AccessManagement.Api.Enterprise": 67,
  "Altinn.AccessManagement.Api.Metadata": 75,
  "Altinn.AccessManagement.Api.Enduser": 73,
  "Altinn.AccessManagement.Api.Internal": 70,
  "Altinn.AccessManagement.Api.ServiceOwner": 65,
  "Altinn.AccessManagement.Integration": 68,
  "Altinn.AccessManagement.Core": 64
}
```

Dropping below the floor for any of these fails the pipeline.

### `warnings` — ratcheting (non-fatal)

```json
{
  "Altinn.AccessManagement": 60,
  "Altinn.AccessMgmt.Core": 50,
  "Altinn.Authorization.Integration.Platform": 50
}
```

Below-threshold assemblies here emit a warning in the CI log. Use this section
as a one-way ratchet for assemblies approaching promotion to `assemblies`.
`Altinn.AccessMgmt.Core` and `Altinn.Authorization.Integration.Platform` are
ratcheting toward enforcement as their pure-logic coverage gaps close (#2976).

### `globalThreshold`

Currently `0` — only the explicitly listed assemblies are gated. We
deliberately do **not** enforce coverage on every assembly in the repo;
many are thin adapters, generated code, or host wiring where line coverage
isn't a meaningful target.

## Philosophy

Coverage percentage is a guardrail against regression, not a quality goal in
itself. New tests should be driven by a concrete risk — a named bug class
(mapping, translation, validation, an authorization boundary), a regression we
want pinned, or a branch whose failure would matter — not by a number to hit.

Consequences:

- A mock-based test is worth writing when it exercises real branching,
  validation, or decision logic in the code under test. It is **not** worth
  writing when its only assertion is that a pass-through maps its input to its
  output (response-shape mapping over a thin adapter) — that adds a percentage
  point and a maintenance burden without guarding behaviour.
- Prefer covering pure logic (validators, mappers with real rules, decision
  paths) over plumbing/I-O orchestration that can only be reached by mocking
  out everything it does.
- The floors exist to stop covered code from rotting, so raise a floor (or
  promote a `warnings` entry to `assemblies`) when real coverage lands — but do
  not lower a floor to make a thin test "count".

## Running locally

Full repo:

```
pwsh eng/testing/run-coverage.ps1
```

Just AccessManagement:

```
pwsh eng/testing/run-accessmanagement-coverage.ps1
```

Scope to specific projects:

```
pwsh eng/testing/run-coverage.ps1 -Projects @(
    "src/apps/Altinn.AccessManagement/test/Altinn.AccessManagement.Api.Tests/Altinn.AccessManagement.Api.Tests.csproj"
)
```

Outputs land in `TestResults/`:

- `*.cobertura.xml` — one per test project
- `*.coverage.log` — captured test stdout (last N lines are echoed on failure)

## How CI runs it (single pass)

CI does **not** run `run-coverage.ps1`. Instead, it runs the test suite once
under `dotnet-coverage collect` and then invokes the parse-only check:

```yaml
# Step A — run the tests once, producing TRX + coverage in one pass.
# --settings excludes generated code / migrations / tooling (see below).
dotnet-coverage collect --settings eng/testing/coverage.settings \
    --output TestResults/coverage.cobertura.xml \
    --output-format cobertura -- \
    dotnet test -- --report-xunit-trx --ignore-exit-code 8

# Step B — parse-only threshold enforcement (seconds)
pwsh eng/testing/check-coverage-thresholds.ps1 \
    -CoverageFile TestResults/coverage.cobertura.xml \
    -ThresholdsFile eng/testing/coverage-thresholds.json
```

Running the tests twice (once for Test, once for Coverage) would double
pipeline time, so the suite runs once and the coverage report is parsed from
that single pass.

## What is excluded from the denominator

Both coverage systems exclude the same non-product code so a floor reflects
coverage of real logic:

- **SonarCloud** via `sonar.coverage.exclusions` in
  [`SonarQube.Analysis.xml`](../../SonarQube.Analysis.xml).
- **CI floors** via [`eng/testing/coverage.settings`](../../eng/testing/coverage.settings),
  passed to `dotnet-coverage collect --settings`.

Excluded in both: test/mock assemblies, generated code (`*.g.cs`,
`*.Designer.cs`, `Properties/AssemblyInfo.cs`), EF migrations, the Aspire host
composition root (`*.AppHost`), developer tooling (`Altinn.Authorization.Cli`),
and the `Altinn.AccessMgmt.FFB` / `Altinn.AccessMgmt.WebComponents` front-end
modules. (`check-coverage-thresholds.ps1` independently scores only "Owned"
`Altinn.*` assemblies, so Tests/TestUtils/Mocks never count regardless.)

Deliberately **not** excluded:

- `Program.cs` / `Startup.cs` — in the controller APIs the minimal-hosting
  bootstrap is large and genuinely exercised by integration tests (excluding it
  dropped `Api.ServiceOwner` from 68% to 54%, below its floor). The floors are
  calibrated with it counted, so it stays in the denominator in both systems.
- EF entity-type configurations (`PersistenceEF/Configurations`) — well-covered
  declarative schema, so dropping them would only lower the measured percentage
  and risk the 90% floor.

Keep the two lists in sync: when you add an exclusion to one, mirror it in the
other. `coverage.settings` uses the VSTest `.runsettings` shape with a
`DataCollector` named `Code Coverage`, and its `ModulePaths` / `Attributes` /
`Sources` elements must appear in that order or `dotnet-coverage` rejects the
whole file.

## Adjusting a threshold

- **Raising** an existing threshold: open a PR that bumps the number in
  `coverage-thresholds.json`. No other change needed.
- **Adding** a new enforced assembly: add it to `assemblies` with a
  realistic floor. If current coverage is close but not quite there, add
  it to `warnings` first and promote later.
- **Lowering** a threshold: discouraged. Include a short rationale in the PR
  description (e.g. "deleted an entire well-covered module"). Prefer adding
  tests.

## Next: [CI.md](CI.md)
