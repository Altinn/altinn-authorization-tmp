---
step: 7
title: Coverage — Api.Metadata `PackagesController` direct unit tests
phase: B
status: complete
linkedIssues:
  feature: 2976
  task: 2977
coverageDelta:
  # Baseline is the post-Step-6 merged-cobertura figure
  # (51.53% line / 51.11% branch). Step 7 run-coverage.ps1 result:
  # 79.04% line, 77.78% branch — see Verification § Coverage.
  Altinn.AccessManagement.Api.Metadata:
    linePp: +27.51
    branchPp: +26.67
verifiedTests: 26
testFailures: 0
testSkips: 0
touchedFiles: 1
auditIds: [M6']
---

# Step 7 — Coverage: `Altinn.AccessManagement.Api.Metadata` `PackagesController`

## Goal

Resolve audit finding **M6'** for the Metadata host: close the remaining
controller gap in `Altinn.AccessManagement.Api.Metadata` by adding direct
Moq-based unit tests for the six untested actions on
`PackagesController`. Same direct-controller pattern established in
[`overhaul part-1 step 49`](../STEPS_PART_1/49_Coverage_Api_Internal_Controllers.md)
and [`step 53`](../STEPS_PART_1/53_Coverage_6_7d_Part7.md): controllers
are instantiated with mocked `IPackageService` + `ITranslationService`,
no `WebApplicationFactory`, no Postgres.

`RolesController` (covered by [`RolesControllerTest.cs`](../../../src/apps/Altinn.AccessManagement/test/AccessMgmt.Tests/Controllers/Metadata/RolesControllerTest.cs))
and `TypesController.GetOrganizationSubTypes` (covered by
[`TypesControllerTest.cs`](../../../src/apps/Altinn.AccessManagement/test/AccessMgmt.Tests/Controllers/Metadata/TypesControllerTest.cs))
already have full direct-unit-test coverage; this step extends
`PackagesControllerTest.cs`.

## What changed

### `src/apps/Altinn.AccessManagement/test/AccessMgmt.Tests/Controllers/Metadata/PackagesControllerTest.cs`

15 new tests appended (11 → 26 in this class), each with the same
shape: instantiate `PackagesController` with a mocked
`IPackageService` (and a pass-through `ITranslationService`),
invoke the action, assert on the `ActionResult.Result` discriminator
(`OkObjectResult` / `NoContentResult` / `NotFoundResult`).

Section coverage added:

| Section | Tests | Branches covered |
|---|---|---|
| `GetGroupAreas(id)` | 3 | results found → 200; empty + group exists → 204; empty + group missing → 404 |
| `GetArea(id)` | 2 | found → 200; missing → 404 |
| `GetAreaPackages(id)` | 3 | results found → 200; empty + area exists → 204; empty + area missing → 404 |
| `GetPackage(id)` | 2 | found → 200; missing → 404 |
| `GetPackageByUrn(urnValue)` | 2 | found → 200; missing → 404 |
| `GetPackageResources(id)` | 3 | results found → 200; empty + package exists → 204; empty + package missing → 404 |

Additional change: `PassThroughTranslation()` was extended to also
register pass-through stubs for `AreaDto`, `ResourceDto`, `TypeDto`,
and `ProviderDto` — without these, `DeepTranslationExtensions`'
`await translationService.TranslateAsync(item, …)` calls return a
default-valued `ValueTask<T>` (i.e. `null`) on Moq-uninitialized
mocks, which then NREs inside the deep-translation traversal. The
existing `PackageDto` / `AreaGroupDto` setups are preserved unchanged.

The 200-path tests on the three multi-branch actions also assert
that the fallback service call (e.g. `GetAreaGroup` for
`GetGroupAreas`) is **never** invoked when results are found —
guarding the "happy path skips fallback lookup" invariant explicitly
rather than relying on `Verify(Times.Never)` only on regression.

## Verification

### Tests

```
PackagesControllerTest (focused):  26 /   26 passed (0 failed, 0 skipped) — 690 ms
AccessMgmt.Tests (full project): 1225 / 1225 passed (0 failed, 0 skipped) — 1m 50s
Suite total (run-coverage.ps1):  2575 / 2559 passed (0 failed, 16 skipped)
```

The 16 suite-wide skips are unchanged from Step 6's run (Azurite-blocked
`Host.Lease` × 2, the two carry-over `Sender_Confirms…` /
`Receiver_Approves…` rewrite skips, and the 9 + 5 environmentally
blocked tests in `Enduser.Api.Tests` and `Integration.Tests` already
catalogued under L1' / Phase E.3). No new sibling regressions.

The +39 net-new test count vs Step 6's 2536 baseline includes the 15
new tests added by this step plus 24 tests that arrived via main since
Step 6 ([apps#2959](https://github.com/Altinn/altinn-authorization-tmp/pull/2959),
[apps#2965](https://github.com/Altinn/altinn-authorization-tmp/pull/2965),
[apps#2928](https://github.com/Altinn/altinn-authorization-tmp/pull/2928)).

### Coverage

Baseline (post-Step-6 merged-cobertura, see
[`INDEX.md` § Final Coverage](INDEX.md#final-coverage-measured)):

| Assembly | Line% | Branch% |
|---|---:|---:|
| `Altinn.AccessManagement.Api.Metadata` | 51.53 | 51.11 |

After Step 7 (merged-cobertura at `TestResults/coverage.cobertura.xml`):

| Assembly | Line% | Branch% | ΔLine | ΔBranch |
|---|---:|---:|---:|---:|
| `Altinn.AccessManagement.Api.Metadata` | **79.04** | **77.78** | **+27.51 pp** | **+26.67 pp** |

`check-coverage-thresholds.ps1` exit: **0** ✅. `Api.Metadata` is not on
the enforced floors list, so the gain is pure progress; at 79.04 % it is
now a **stronger Phase F.1 candidate** than the original 70 % floor
sketched in [`INDEX.md` § F.1](INDEX.md#recommended-next-steps-priority-order)
— consider promoting at floor 75 instead.

## Deferred / follow-up

- **`AccessManagementMetadataHost.cs` and `Program.cs`** —
  bootstrap files that today read as ~0 % covered for the Metadata
  host. Reaching them requires `WebApplicationFactory`-style
  integration tests, not pure-logic Moq tests; out of scope for
  Phase B.1. May be picked up under Phase D if a Metadata-host
  integration suite is built.
- **`TypesController.GetAllTypes` and `GetSubTypes`** — both
  declared `private` in the source, yet decorated (in commented-out
  attribute markers) with `[Route]` / `[HttpGet]`. ASP.NET Core
  routing cannot reach them; they are dead code that contributes a
  small amount of uncovered surface to the Metadata assembly. The
  cleanup (delete or expose-and-test) is a separate code-hygiene
  concern outside the testing-infrastructure overhaul, so no
  test was added here.
- **Phase B.2 / B.3** remain the next pure-logic targets per
  [`INDEX.md` § Recommended Next Steps](INDEX.md#recommended-next-steps-priority-order):
  `Persistence.Core` (largest gap), then `Integration.Platform`.
- **Phase F.1 promotion** — `Api.Metadata` was added to the F.1
  candidate list in [`INDEX.md` § Recommended Next Steps](INDEX.md#recommended-next-steps-priority-order)
  at floor **75** (rather than the 70 originally sketched, since the
  measured coverage of 79.04 % gives ~4 pp headroom). The Final
  Coverage row was repositioned from rank 17 to rank 6.
- **Drift in non-Metadata coverage rows** — the `run-coverage.ps1`
  output also showed small shifts in five non-Metadata assemblies
  vs the Step 6 baseline (Enterprise +2.31 pp, AccessManagement.Persistence
  +1.86 pp, Api.Enduser +0.49 pp, Api.ServiceOwner +0.36 pp,
  AccessMgmt.Core −0.61 pp). These reflect intervening main commits
  ([apps#2959](https://github.com/Altinn/altinn-authorization-tmp/pull/2959),
  [apps#2965](https://github.com/Altinn/altinn-authorization-tmp/pull/2965),
  [apps#2928](https://github.com/Altinn/altinn-authorization-tmp/pull/2928))
  rather than Step 7 work, so the rows were **not** edited inline —
  a future closing sweep should re-baseline the whole Final Coverage
  table in one pass to avoid drift accumulating row-by-row. Notes
  added to the table preamble.
