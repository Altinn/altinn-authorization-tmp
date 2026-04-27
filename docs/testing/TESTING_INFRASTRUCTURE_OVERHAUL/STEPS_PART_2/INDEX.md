# Testing Infrastructure Overhaul — Part 2 Step Log

## Getting Started & Workflow

**New chat?** Read these docs **in order** to get full context:

1. **This file** (`docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/INDEX.md`) —
   step log, coverage results, recommended next steps, deferred work, and
   workflow rules for **Part 2**.
2. **[TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md)** —
   the Part 2 audit & phase plan (populated by the kickoff step).
3. **Historical / background context (optional):**
   - [`../TESTING_INFRASTRUCTURE_OVERHAUL_PART_1.md`](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_1.md)
     and [`../STEPS_PART_1/INDEX.md`](../STEPS_PART_1/INDEX.md) — the closed
     Part 1 plan and step log.
   - [`../TRACKING_RETROSPECTIVE.md`](../TRACKING_RETROSPECTIVE.md) —
     post-mortem of Part 1's tracking workflow. Its recommendations have
     already been folded into this file (see [Conventions](#conventions)
     and the Decision Log in PART_2.md). Read only if you are reshaping the
     workflow itself.
4. **The step doc for the work you're about to do** (linked in the
   [Step log](#step-log) table or in
   [Recommended Next Steps](#recommended-next-steps-priority-order) below).

**Handoff prompt** (paste this into a new chat to resume the work):

> _Handoff prompt v2026-04-27 (Part 2 scaffold)._  Continue Part 2 of the
> testing infrastructure overhaul on branch
> `feature/2842_Optimize_Test_Infrastructure_and_Performance_Part_Two`.
>
> Start by reading
> `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/INDEX.md` —
> it's the entry point and tells you exactly what to read next, the
> tracking conventions, the workflow rules for completing a step, and how
> to pick the next step.
>
> Then execute the highest-priority item from
> `### Recommended Next Steps (priority order)` in that file.

If you change the prompt, bump the `v<date>` tag and call it out in the
commit message (e.g. `docs(testing): update Part 2 handoff prompt to vYYYY-MM-DD`).

**Before starting a step:**

- **Create the GitHub Issues for the work** — see
  [Conventions § GitHub Issue tracking](#github-issue-tracking). Issues
  are created **just-in-time** (when the step is *starting*), not
  upfront from the Recommended Next Steps list. Reference the Task
  issue number in the step-doc front matter (`linkedIssues:`), in the
  commit message (`overhaul part-2 step N (#<task>): ...`), and in the
  PR body (`Closes #<task>`).
- **Branch from `main` named after the Task issue** — see
  [Conventions § Branch naming](#branch-naming). Format:
  `feat/<task-issue#>_<short_name_with_underscores>`. **No** `claude/`
  prefixes, **no** random names. All commits for the Task land on this
  branch; new Tasks get their own branch.

**When completing a step:**

- **Create a step doc** at
  `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/<NN>_<Name>.md`
  using the [front-matter template](#step-doc-template) (zero-padded step
  number — `01`, `02`, …). The doc must cover: Goal, What changed (file by
  file), Verification (tests run + results), Deferred / follow-up, plus
  the YAML front-matter block.
- **Add a row** to the [Step log](#step-log) table linking to the new doc.
  Fill `Phase/Tag`, `CovΔ (line)`, and a one-line summary dense enough to
  be useful as a grep target (class names, fix summaries, test counts,
  coverage deltas).
- **Run all tests changed or impacted by the step** and record results in
  the step doc. Update the [Final Coverage (measured)](#final-coverage-measured)
  table if the step affected coverage of any listed assembly (or a new
  assembly that should be tracked).
- **Re-check the [Blocked Items](#blocked-items) section.** If something is
  now unblocked, move it into Recommended Next Steps at an appropriate
  priority and remove it from the Blocked Items table. For every entry that
  remains blocked, refresh the `Last re-checked` cell with the current step
  number — do **not** leave a row sitting at a stale step number.
- **Sweep for obsoleted docs.** Walk
  `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/` *and*
  [`../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md`](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md)
  for items the new step has superseded. For each, either delete the doc
  and update inbound links, or leave a banner / strike-through that points
  to the resolving step.
- **Commit and push** at the end of each step. Reference both the step
  number and the Task issue in the commit message (e.g.
  `overhaul part-2 step 7 (#NNNN): ...`) so `git log --oneline` works as
  a second step log *and* GitHub auto-cross-links commits to the issue.
  Commits stay granular — push to the working branch; do **not** open
  a PR per step.
- **Open a PR only when a coherent bundle of steps is ready** for tech
  lead review (see the [Granularity rule](#github-issue-tracking)).
  PR title is plain English, no internal IDs. PR body lists the
  bundled audit IDs, links the step docs, and uses:
  - `Updates #<task>` for in-progress PRs that don't fully complete the
    Task (the Task stays open after merge), or
  - `Closes #<task>` for the **final** PR of the Task's bundle.
- **After merge** — if the PR closed the Task, also check whether the
  parent Feature has any remaining open Tasks; if not, close the
  Feature with a one-line summary.
- **Recommend whether a new chat should be started** for the next step,
  based on complexity and remaining context.
- **Wait for explicit go-ahead** before proceeding to the next step.

**Picking the next step (when the list below is thinning):**

1. If [Recommended Next Steps](#recommended-next-steps-priority-order) still
   has actionable items, take the highest-priority one.
2. If that list is empty or only contains blocked/unactionable items,
   consult [TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md)
   for the next actionable item from the phase plan, and add it back to the
   list below before starting.
3. If `TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md` is also exhausted, the
   next step should itself be **a fresh audit of the current testing
   infrastructure** to identify the next most valuable improvements —
   produce an updated audit doc (Part 3) and a refreshed
   recommended-next-steps list, then resume the cycle.

**Splitting parts:** When the [Step log](#step-log) approaches **~50
steps**, proactively close Part 2 and start Part 3 (`STEPS_PART_3/`,
`TESTING_INFRASTRUCTURE_OVERHAUL_PART_3.md`). Part 1 ended at 61 steps and
the resulting table was just barely readable. Part numbers are cheap; long
tables are expensive.

---

## Conventions

These conventions are lifted from
[`../TRACKING_RETROSPECTIVE.md`](../TRACKING_RETROSPECTIVE.md) and apply to
every Part 2 step. Adopting them up-front avoids the silent drift that hit
Part 1 around step 50.

### Step doc template

Every step doc starts with this YAML block, followed by the standard
sections (Goal / What changed / Verification / Deferred):

```yaml
---
step: 7
title: Coverage — Api.Internal controllers
phase: 6.7d            # short tag matching the Phase/Tag column
status: complete       # complete | partial | blocked | reverted
linkedIssues:
  feature: 1234        # Feature issue (Phase-level, e.g. "Phase B")
  task: 1237           # Task issue for THIS step (created when step started)
coverageDelta:
  AccessManagement.Api.Internal:
    linePp: +3.40      # signed delta in percentage points; or `n/a`
verifiedTests: 34      # count of tests run and passing
touchedFiles: 12
---
```

`linkedIssues` is omitted (or both fields set to `null`) for
audit-only / pre-issue-tracking steps such as Step 1.

### Step doc filenames

`STEPS_PART_2/<NN>_<Name>.md` — `NN` is the zero-padded step number that
matches the `#` column in the Step log. Lexicographic order then matches
chronological order; no more `1_…`, `10_…`, `2_…` mis-sorting.

### Source-code cross-references

Source-code comments that reference a step use the short form:

```csharp
// See: overhaul part-2 step 7
// See: overhaul part-1 step 17
```

`docs/testing/README.md` (or this INDEX) is the single place that maps
"part-N step M" → the actual file path. When the folder is reorganized,
only that mapping needs updating — no bulk find-and-replace across the
test projects.

### Branch naming

All work branches must follow:

```
feat/<github-issue#>_<short_name_with_underscores>
```

- **`feat/`** prefix (lowercase, no other prefixes — no `claude/`, no
  `bugfix/`, no `chore/`).
- **`<github-issue#>`** is the **Task** issue number (not the Feature).
  Issue must exist before the branch is created — see
  [GitHub Issue tracking](#github-issue-tracking) below.
- **`<short_name_with_underscores>`** is a 2–5 word lowercase
  description, words joined with `_`. Avoid auto-generated /
  random / project-internal codenames (e.g. *not*
  `peaceful-diffie-18e815`).

Examples:
- `feat/3001_test_infrastructure_critical_fixes`
- `feat/3157_repository_tests_live_postgres`
- `feat/3204_consolidate_duplicate_test_mocks`

One branch per Task. PRs from this branch use `Updates #<task>` until
the final PR closes the Task with `Closes #<task>`.

### GitHub Issue tracking

Work in this repo is tracked through **GitHub Issue Types** (not labels)
with a **Feature → Task** hierarchy. Issues are created **just-in-time**
— when a step is *starting*, not upfront from the Recommended Next Steps
list.

**Granularity rule.** The issue tracker is a shared team backlog **and
every PR consumes tech-lead review time**. Keep both counts low:

- **Features** are theme-level deliverables (typically one per Phase, or
  per coherent value slice within a Phase). Expected lifetime: weeks to
  months.
- **Tasks bundle related work that spans multiple PRs.** Tasks are
  **not** atomic per-PR items, and a step doc is **not** a Task.
  Expected scope per Task: roughly 1–3 weeks of work, 1–3 PRs.
- **PRs bundle related steps.** A PR is **not** "one per step" — it
  groups commits/steps that a tech lead can review as a coherent unit.
  Aim for ~1–3 PRs per Task; each PR should be substantial enough to
  justify a review cycle but small enough to be reviewable in a single
  sitting.
- **Step docs and commits stay granular** (per audit-ID or per
  in-flight piece of work) — they're our internal tracking. The
  team-visible surface is the Issue tracker (Features + Tasks) and the
  PR list, both of which must be small.
- **Avoid one Task per audit ID.** The audit's `C1'`/`M1'`/`L1'` IDs
  are tracking handles for the docs; bundle related ones (e.g. several
  `C*'` correctness fixes) into a single Task whose title describes
  the outcome.
- For Part 2, expect roughly **3–5 Features** total over the program's
  lifetime, **2–4 Tasks** per Feature, and **1–3 PRs** per Task — *not*
  the 6 Phases × 21 steps the docs enumerate. Internally we may write
  ~21 step docs and ~30+ commits over the lifetime, but only ~10
  team-visible PRs.

**Title rule (both Feature and Task):** titles must read as
self-contained plain English to any team member browsing the issue list
— no `Test infra Part 2`, `Phase X`, `A.3`, `C5'`, or other doc-internal
prefixes/IDs. Those identifiers belong in the *body* (which links back
to the audit doc and the step log). The title says **what changes for
the reader / user**.

- **Feature issue** — type `Feature`. Title is a plain description of
  the outcome, e.g. `Fix critical issues from the test-infrastructure
  audit (April 2026)` rather than `Phase A: Critical fixes`. Body links
  back to the relevant section of
  `TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md` §4, names the audit-doc
  Phase ID(s) for traceability, and lists the planned Tasks (initially
  as a checklist; each box becomes a sub-issue link as the corresponding
  Task is created).
- **Task issue** — type `Task`. Title is a plain description of a
  multi-PR bundle of work, e.g. `Resolve critical bugs and dead code
  in test infrastructure` rather than `Fix
  check-coverage-thresholds.ps1 false-positive`. Body lists the
  bundled audit IDs (e.g. "covers C1', C2', C3', C5'"), the
  step-doc/PR plan, links back to
  `TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md` §2, the entry in
  `INDEX.md`'s Recommended Next Steps, and the relevant source paths.

**PR–Task linkage:**

- In-progress PRs use `Updates #<task>` in the body so the Task stays
  open while sub-work lands.
- Only the **final** PR for the bundle uses `Closes #<task>` — or
  close the Task manually after the last PR merges.
- **Linkage** — link Tasks as sub-issues of the parent Feature via the
  REST API:
  ```bash
  gh api repos/Altinn/altinn-authorization-tmp/issues/<feature>/sub_issues \
    --method POST --field sub_issue_id=<task-numeric-id>
  ```
  Note: `sub_issue_id` is the issue's *numeric internal ID* (from
  `gh api repos/Altinn/altinn-authorization-tmp/issues/<task> -q .id`),
  not its `#NNNN` number.
- **Closing** — PRs include `Closes #<task>` in the body so the merge
  auto-closes the Task. Once all Tasks under a Feature close, close the
  Feature manually with a one-line summary.
- **Do not pre-create the backlog.** The 21-item Recommended Next Steps
  list in this file is the backlog; only the *next* step (or two
  parallelizable steps) should have live Task issues at any given time.

### Phase/Tag column

Short tag identifying the phase or topic of the step. Examples once the
audit lands: `A` (live-DB Npgsql), `B` (Host.Lease/Azurite), `CI`, `DOC`,
`FIX`. Lets a reader filter the step log by topic at a glance instead of
re-reading every row.

### CovΔ (line) column

Either a signed delta (`+3.18pp`), an `n/a` marker (no coverage impact),
or the absolute new percentage if the step adds a previously-untracked
assembly. Numbers in this column are the source of truth for ratchet
decisions; do not bury coverage moves only in prose.

### Structural check (recommended early step)

A small script (PowerShell or bash) wired into a pre-commit / CI hook
should validate this file:

- Every Step log row has exactly **6** cells.
- Step numbers are strictly increasing with no gaps.
- Every `Doc` link points to an existing file.
- Every internal anchor referenced elsewhere in the file resolves to an
  existing heading.

This catches the duplicate-row, orphan-stub, and missing-heading bugs
that slipped through Part 1. Authoring the script is a good early Part 2
step (probably `DOC` / `CI` tag) but is **not** a blocker for the kickoff
audit.

### Deferred (land later if needed)

- **Machine-readable sidecar** (`STEPS_PART_2/steps.json` generated from
  the front-matter blocks). Useful once tooling needs structural queries;
  defer until a concrete consumer exists.

---

## Step log

Steps are listed in the order they were **actually completed**, not by
the phase numbers in the
[Part 2 overhaul plan](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md).

| # | Completed | Phase/Tag | Topic | CovΔ (line) | Doc |
|---|-----------|-----------|-------|-------------|-----|
| 1 | 2026-04-27 | KICKOFF | Part 2 kickoff audit — measured 2535 tests across 11 projects (1 fail, 16 skip), 23 owned assemblies; surfaced **5 critical** (`ABAC.Tests` empty (C1'); `ValidateParty…Forbidden` failing 200≠403 (C2'); `Host.Pipeline` 0% / no test project (~1.4k LOC, C3'); `AutoMapper 14.0.0` CVE GHSA-rvv3-g6hj-g44x (C4'); `check-coverage-thresholds.ps1` false-positive in workstation mode (C5')), **8 medium**, **5 low** issues | n/a (audit-only) | [01_Part_2_Kickoff_Audit.md](01_Part_2_Kickoff_Audit.md) |
| 2 | 2026-04-27 | A | Fix **C5'** workstation false-positive coverage failures — `run-coverage.ps1` now invokes `dotnet-coverage merge` to produce one canonical `coverage.cobertura.xml` before threshold check + ReportGenerator; `check-coverage-thresholds.ps1` hardened with two-phase aggregate-then-check (max line%/branch%, union Owned) so multi-file inputs no longer cause one spurious failure per per-test-project view. Side effect: merged-cobertura aggregation reveals the Step 1 audit *under-counted* several assemblies (e.g. `Api.Internal` 48.56% → **73.63%** ✅, `Persistence` 44.90% → 57.29%, `AccessMgmt.Core` 33.66% → 44.96%) — audit baseline refresh deferred to T1 closing | n/a (tooling fix; baseline refresh deferred) | [02_Fix_Coverage_Threshold_Aggregation.md](02_Fix_Coverage_Threshold_Aggregation.md) |
| 3 | 2026-04-27 | A | Resolve **C1'** by deleting the empty `Altinn.Authorization.ABAC.Tests` project (only auto-generated `.cs` files; the test runner discovered 0 tests). Removed the project + the orphan `test/Directory.Build.props` from both the root `Altinn.Authorization.sln` and the per-package `src/pkgs/Altinn.Authorization.ABAC/Altinn.Authorization.ABAC.sln`; updated `docs/testing/TEST_PROJECTS.md` § `pkg: ABAC` to document that ABAC is exercised indirectly via `Altinn.Authorization.Tests` (~63 % line / 61 % branch). ABAC's centrally-enforced 60 % threshold continues to gate the indirect coverage. Per-package `dotnet build` clean (net8.0 + net9.0). Test-project count: 11 → **10** | n/a (no production code; ABAC indirect coverage unchanged) | [03_Delete_Empty_ABAC_Tests.md](03_Delete_Empty_ABAC_Tests.md) |

### Recommended Next Steps (priority order)

All items below are actionable unless otherwise noted. Ordering follows
[`../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md` §5](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md#5-execution-order--dependencies)
(critical → pure-logic → live-DB → new projects → housekeeping → ratchet).

**Phase A — Critical fixes (block Phase F):**

1. ~~**A.3 — Fix `check-coverage-thresholds.ps1` false-positive (C5').**~~ —
   **Done in Step 2** ([02_Fix_Coverage_Threshold_Aggregation.md](02_Fix_Coverage_Threshold_Aggregation.md)).
   Phase F (L2') is now unblocked.
2. **A.2 — Triage failing test (C2').**
   `ValidateParty_NotAsAuthenticatedUser_Forbidden` in
   [`PartiesControllerTest.cs:167`](../../../../src/apps/Altinn.Authorization/test/Altinn.Authorization.Tests/PartiesControllerTest.cs:167)
   expects 403, returns 200. Determine whether it's a real auth
   regression in `PartiesController.ValidateParty(...)` or `PartiesMock`
   drift — fix root cause, not the assertion. *Possibly a security
   finding* — handle ahead of E.x cleanups.
3. ~~**A.1 — Resolve empty `ABAC.Tests` (C1').**~~ — **Done in Step 3**
   ([03_Delete_Empty_ABAC_Tests.md](03_Delete_Empty_ABAC_Tests.md)):
   project + orphan props deleted; removed from both sln files;
   `docs/testing/TEST_PROJECTS.md` updated; ABAC's 63 % coverage
   continues indirectly via `Altinn.Authorization.Tests`.
4. ~~**A.4 — Investigate `AutoMapper 14.0.0` CVE (C4').**~~ — **Dropped
   2026-04-27**: AutoMapper went paid in ≥ 15.x and the advisory
   doesn't apply to this codebase. See dismissal in
   [PART_2 §2 C4'](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md#critical-correctness-security-dead-code).
5. **A.5 — Scaffold empty `Altinn.Authorization.Host.Pipeline.Tests`
   project (C3').** No tests yet — just the csproj wired into the
   central infrastructure. Unblocks D.1.

**Phase B — Pure-logic coverage (parallel with A):**

6. **B.1 — Controller gaps (M6').** Direct Moq tests for
   `Altinn.AccessManagement.Api.Internal` (48.56%) and
   `Altinn.AccessManagement.Api.Metadata` (51.53%). Mirror the Part 1
   Step 49 / Step 53 pattern. Estimated +10–20pp each.
7. **B.2 — Domain pure-logic (M5').** Identify remaining reachable
   targets in `Altinn.AccessMgmt.Core` (33.66%) and
   `Altinn.AccessMgmt.Persistence.Core` (25.34%). Continues Part 1
   Steps 42–60.
8. **B.3 — `Integration.Platform` tail (M7').**
   `PaginatorStream<T>` and (if feasible without key-vault)
   `TokenGenerator` in `Altinn.Authorization.Integration.Platform`
   (45.38%).

**Phase C — Live-DB coverage (M3'):**

9. **C.1 — Design `RepositoryDbCollection` xUnit collection.** Shares
   the `EFPostgresFactory` template-clone fixture across the Npgsql-repo
   tests in `Altinn.AccessMgmt.Persistence` (47.32%) and
   `Altinn.AccessManagement.Persistence` (44.90%).
10. **C.2 — Per-repository tests against the live Postgres.**

**Phase D — New test projects (parallel with B/C):**

11. **D.1 — `Host.Pipeline` smoke suite.** Now that A.5 has scaffolded
    the project, fill it with builders / pipeline-services / segment
    coverage. Largest single coverage win available.
12. **D.2 — Azurite Testcontainers fixture (M4').** Mirror the
    `PostgresServer` pattern (singleton + `Assert.Skip` outage guard) to
    unblock `Altinn.Authorization.Host.Lease.Tests` (currently 6.87% /
    2 skipped).
13. **D.3 — Decide on `Host.Database` / `Host.MassTransit` (M8').**
    Either thin smoke suites or accept-the-gap (architectural glue).

**Phase E — Housekeeping & drift (interleavable):**

14. **E.1 — Mock consolidation across verticals (M1').** Promote the
    9+ near-duplicate mock/stub classes between
    `Altinn.AccessManagement.TestUtils/Mocks/` and
    `Altinn.Authorization.Tests/MockServices/` into a shared package.
15. **E.2 — Investigate main-app coverage regression (M2').**
    `Altinn.AccessManagement` dropped from 58.19% (Part 1 Step 12) to
    56.47% — find which production files grew without matching tests.
16. **E.3 — Audit the 16 skipped tests (L1').** Categorize each as
    environmentally-blocked (Azurite, etc.) vs cleanup-able.
17. **E.4 — Migrate Part 1 source-code `// See: ...` comments to
    short-form (L4').** Sweep `// See docs/testing/...STEPS_PART_1/...`
    → `// See: overhaul part-1 step N` per
    [Conventions](#source-code-cross-references).
18. **E.5 — Drop dead xUnit v2 package pins (L5').** Remove `xunit`
    2.9.3 + `xunit.runner.visualstudio` 3.1.5 from
    [`src/Directory.Packages.props`](../../../../src/Directory.Packages.props)
    — no test project sets `XUnitVersion=v2` anymore.

**Phase F — Coverage threshold ratchet (last; depends on A.3):**

19. **F.1 — Promote tier-2 enforced (L2').** Add to `eng/testing/coverage-thresholds.json`:
    `Altinn.AccessManagement.Integration` (88.35% → floor 80),
    `Altinn.AccessManagement.Api.Enduser` (73.88% → 70),
    `Altinn.AccessManagement.Api.ServiceOwner` (69.58% → 65),
    `Altinn.Authorization.Host` (91.67% → 85). Consider raising
    Maskinporten 75→80, PEP 75→78, AccessMgmt.PersistenceEF 90→95.
20. **F.2 — Resolve `Altinn.AccessManagement` warn-only floor
    (L3').** Either drop to 55% (interim, while M2' is investigated)
    or hold at 60% and treat the warning as an action item.

**Recommended early hygiene (any time during A–E, low-effort):**

21. **DOC/CI — Author the structural-check script for `INDEX.md`.** See
    [Conventions § Structural check](#structural-check-recommended-early-step).
    Catches the Part 1 silent-table-drift bugs automatically.

### Blocked Items

`Last re-checked` records the most recent step number that revisited the
row. Refresh it as part of every step's blocked-items sweep so neglect
shows up immediately.

| Item | Blocker | Notes | Last re-checked |
|---|---|---|---|
| `Host.Lease` tests (Part 1 Phase 6.5 carry-over) | Azurite / Azure Storage Emulator required | Confirmed at Step 1 audit: 2 tests, both `Skip`ped, `Altinn.Authorization.Host.Lease` at 6.87% line. Tracked as **M4'** in [PART_2 §2](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md#2-findings--issues); Phase D.2 unblocks (Azurite Testcontainers fixture). | step 3 |
| `Sender_ConfirmsDraftRequest_ReturnsPending` (Part 1 carry-over) | Environmental investigation needed | `[Skip]`ped during Part 1 Step 51 after the `ResourceRegistryMock` cache-hit fix landed. Confirmed still skipped at Step 1 audit. Will be reviewed under **L1'** / Phase E.3. | step 3 |
| `Receiver_ApprovesPendingPackageRequest_ReturnsApproved` (Part 1 carry-over) | Fixture mis-seed — needs rewrite | `[Skip]`ped during Part 1 Step 62 with a TODO describing the proper rewrite (auth as MD of receiver + pre-existing Rightholder connection). Confirmed still skipped at Step 1. Will be reviewed under **L1'** / Phase E.3. | step 3 |

### Final Coverage (measured)

Baseline at Step 1 (2026-04-27). Owned assemblies only (foreign /
indirect packages omitted). Sorted by line% descending. Numbers are
**max-across-files** from the 11 per-test-project cobertura inputs —
the workstation `run-coverage.ps1` does not yet aggregate (issue
**C5'**); CI's single-cobertura output will agree by construction once
Phase A.3 lands. See [PART_2 §1.4](../TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md#14-coverage-baseline-line--branch-owned-assemblies-max-across-files)
for full classification + Part 1 deltas.

| # | Assembly | Line% | Branch% | Threshold (json) | Step 1 status |
|---|---|---:|---:|---|---|
| 1 | `Altinn.AccessMgmt.PersistenceEF` | 98.78 | 91.75 | 90 (enf) | ✅ |
| 2 | `Altinn.Authorization.Host` | 91.67 | 50.00 | — | ✅ (newly measured) |
| 3 | `Altinn.AccessManagement.Integration` | 88.35 | 87.50 | — | ✅ (Phase F candidate L2') |
| 4 | `Altinn.AccessManagement.Api.Maskinporten` | 80.36 | 80.00 | 75 (enf) | ✅ |
| 5 | `Altinn.Authorization.PEP` | 78.99 | 78.68 | 75 (enf) | ✅ |
| 6 | `Altinn.AccessManagement.Api.Enduser` | 73.88 | 65.32 | — | ✅ (Phase F candidate L2') |
| 7 | `Altinn.AccessManagement.Api.ServiceOwner` | 69.58 | 57.94 | — | ✅ (Phase F candidate L2') |
| 8 | `Altinn.Authorization` | 69.00 | 72.61 | 60 (enf) | ✅ |
| 9 | `Altinn.AccessManagement.Api.Enterprise` | 66.39 | 56.52 | 60 (enf) | ✅ |
| 10 | `Altinn.AccessManagement.Core` | 63.29 | 60.38 | 60 (enf) | ✅ |
| 11 | `Altinn.Authorization.ABAC` | 63.17 | 61.29 | 60 (enf) | ✅ (but coverage is *indirect* — see C1') |
| 12 | `Altinn.Authorization.Host.Database` | 59.42 | 65.62 | — | ⚠ Just under 60 (M8') |
| 13 | `Altinn.AccessManagement` (main app) | 56.47 | 59.35 | 60 (warn) | ⚠ Warn-only ratchet failing; -1.72pp regression (M2'/L3') |
| 14 | `Altinn.AccessManagement.Api.Metadata` | 51.53 | 51.11 | — | ⏫ M6' Phase B.1 |
| 15 | `Altinn.AccessManagement.Api.Internal` | 48.56 | 49.46 | — | ⏫ M6' Phase B.1 |
| 16 | `Altinn.AccessMgmt.Persistence` | 47.32 | 33.40 | — | ⏫ M3' Phase C (live-DB) |
| 17 | `Altinn.Authorization.Integration.Platform` | 45.38 | 64.12 | — | ⏫ M7' Phase B.3 |
| 18 | `Altinn.AccessManagement.Persistence` | 44.90 | 29.05 | — | ⏫ M3' Phase C (live-DB) |
| 19 | `Altinn.AccessMgmt.Core` | 33.66 | 25.37 | — | ⏫ M5' Phase B.2 |
| 20 | `Altinn.AccessMgmt.Persistence.Core` | 25.34 | 24.30 | — | ⏫ M5' Phase B.2 |
| 21 | `Altinn.Authorization.Api.Contracts` | 23.57 | 12.58 | — | DTO-heavy; low priority |
| 22 | `Altinn.Authorization.Host.Lease` | 6.87 | 7.41 | — | 🚫 Blocked on Azurite (M4' / Phase D.2) |
| 23 | `Altinn.Authorization.Host.Pipeline` | 0.00 | 0.00 | — | 🚫 No test project (~1.4k LOC; C3' / Phase D.1) |
