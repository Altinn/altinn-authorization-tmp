---
step: 7
title: Realign Part 2 plan after architect + tech-lead feedback
phase: DOC
status: complete
linkedIssues:
  feature: 2979
  task: 2980
bugClassesCovered: []
verifiedTests: 0
touchedFiles: 3
auditIds: []
---

# Step 7 — Realign Part 2 plan after architect + tech-lead feedback

## Goal

Capture the testing-philosophy feedback received from the team
architect and the tech lead during review of [PR apps#2978](https://github.com/Altinn/altinn-authorization-tmp/pull/2978)
into the Part 2 plan documents, so anyone (human or agent) picking up
the overhaul next has a clear view of what's still actionable, what's
been re-scoped, and what's been deprioritized — without needing to
re-trace the PR-thread context.

This step writes no production code and no tests. It's pure
docs — the workflow rules in [`INDEX.md`](INDEX.md#getting-started--workflow)
explicitly anticipate this kind of step (`status: complete` with
`bugClassesCovered: []` and `verifiedTests: 0`).

## Trigger feedback (verbatim)

The first Phase B Task (#2977 / B.1 — Metadata Controller mock-only
unit tests) was opened, executed under the audit's per-assembly
coverage-% framing, and pushed as [PR apps#2978](https://github.com/Altinn/altinn-authorization-tmp/pull/2978).
Two pieces of feedback then arrived in succession:

### Architect (PR thread)

> *"Ikke veldig mye verdi i rene code coverage tester som bare tester
> en mock service respons."*

Translation: *Not much value in pure code-coverage tests that only
test a mocked service response.* The 15 tests added in PR #2978
match this description exactly — `PackagesController` actions are
thin pass-throughs over `IPackageService`, so mock-driven controller
unit tests verify framework wiring, not real logic.

### Tech lead (PR thread, follow-up)

> *"Vi ønsker jo selvsagt å ha best mulig testdekning. Men code coverage
> i seg selv er jo ikke ett kvalitetsmål som gir noe verdi.*
>
> *Tester på Metadata Controller (som også ville gitt oss code coverage
> dekningen) burde jo være tester som faktisk kjørte på database ingest
> dataene våre, og sjekket at respons modellen var populert som
> forventet i API responsen. Så det testet modell mappingen og
> translation f.eks."*

Translation: *We obviously want the best possible test coverage. But
code coverage itself isn't a quality measure that gives any value.
Tests on the Metadata Controller (which would also have given us the
coverage) should be tests that actually run against our database-
ingested data and check that the response model is populated as
expected in the API response — so they test model mapping and
translation, for example.*

The combined effect: PART_2.md's per-assembly coverage-target framing
is misaligned with team philosophy, and Phase F (which would lock
more assemblies into coverage-% gates in CI) becomes counter-
productive. PR #2978 was closed without merging; Task #2977 was
closed alongside it; Feature #2976 was left open with a re-scope
comment. Branch `feat/2977_metadata_controller_tests` was deleted.

## What changed

Three files edited; no source-code touched. Edits are surgical — no
rewrite of the audit's findings (`§§1`/`2`/`3`), only the response
sections that no longer reflect team philosophy.

### `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/TESTING_INFRASTRUCTURE_OVERHAUL_PART_2.md`

- **Top-of-doc banner** added immediately under the title, summarizing
  both feedback points (with the original Norwegian quotes preserved
  for fidelity), enumerating the practical effect on Phases A through
  F, and pointing to the new Decision Log entry.
- **Phase B section banner (§4)** marks the entire phase as needing
  per-candidate re-scope under a *"real logic vs pass-through wiring"*
  filter; ~~strikes through B.1~~ as closed without merging; reframes
  B.2 / B.3 from per-assembly % targets to "audit before opening a
  Task." The original bullets are kept (struck through where
  applicable) so the historical context of *what was previously
  planned* is preserved.
- **Phase F section banner (§4)** marks the phase as deprioritized;
  notes that any small Phase F Task that does open should reframe
  existing `coverage-thresholds.json` floors as *catastrophic-
  regression tripwires*, not as quality gates; cross-references the
  SonarCloud-side follow-up.
- **Decision Log entry** dated *2026-04-29 (Part 2 Step 7)* records
  both feedback quotes verbatim and enumerates the resulting
  decisions: B.1 closed, Phase B re-scope rule, Phase F
  deprioritized, step-doc convention change.

### `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/INDEX.md`

- **Top-of-doc banner** added immediately under the title pointing
  readers to the PART_2.md banner + Decision Log.
- **Step doc template** (`### Step doc template`) revised: adds
  `bugClassesCovered:` (a list of named bug classes the new tests
  defend against) as the new **lead measurement field**, ahead of
  `verifiedTests` and `coverageDelta`. `coverageDelta` is now
  explicitly marked *"informational only — not a goal"*. Adds a
  paragraph explaining that step docs unable to name at least one
  bug class probably fall under the architect's "low-value mock test"
  rule and shouldn't merge as written.
- **Step log row 7** added for this realignment (Phase/Tag `DOC`,
  CovΔ `n/a (docs-only)`).
- **Recommended Next Steps re-ordered.** New ordering puts
  bug-class-driven integration tests at the top (item 6, **B.1'**,
  the tech-lead's directly-stated alternative to the closed B.1),
  followed by Phase C live-DB items (items 7 / 8), Phase D new test
  projects (items 9 / 10 / 11), Phase B remainder needing re-scope
  (items 12 / 13), Phase E housekeeping (items 14–18), Phase F
  ~~items 19 / 20~~ deprioritized, and DOC/CI item 21.
- **Blocked Items** `Last re-checked` column refreshed to step 7 for
  all three rows. None unblocked.
- **Final Coverage table** preamble adds a banner explicitly framing
  it as *"informational snapshot — not a target list"*, with a note
  that assemblies below 60 % are not automatically next steps.

### `docs/testing/TESTING_INFRASTRUCTURE_OVERHAUL/STEPS_PART_2/07_Realign_Part_2_Plan.md`

- **New step doc** — this file.

## Verification

No tests were added or changed. Verification is doc-internal:

- `bugClassesCovered: []` in this step's front matter is the
  explicit form for steps that produce no new test coverage (per the
  revised step-doc template's escape hatch).
- `coverageDelta` omitted in the front matter — there's no coverage
  delta to record, and under the new framing absent ≠ broken.
- All four anchor / cross-reference targets newly introduced
  (`#decision-log`, `#step-doc-template`, the PART_2.md banner anchor,
  the Phase F section anchor) verified by reading the destination
  headings as written.

## Deferred / follow-up

- **Bug-class-driven Metadata integration tests** — the directly-
  stated tech-lead alternative to the closed B.1 is now item 6 in
  Recommended Next Steps as **B.1'**. To be filed JIT under Feature
  [#2979](https://github.com/Altinn/altinn-authorization-tmp/issues/2979)
  when the next chat picks it up. Step doc would lead with concrete
  bug classes (wrong joins, mapping gaps, translation fall-back, URN-
  lookup mismatch).
- **SonarCloud-side exclusions for pass-through code** — separate
  eventual follow-up Task under the same Feature #2979. Aligns
  SonarCloud's coverage gate with the architect's "no low-value mock
  tests" rule so the dismissed pattern doesn't re-emerge via Sonar
  pressure on the dashboard.
- **Phase B re-scope audits** — when B.2 / B.3 are eventually picked
  up, each candidate assembly needs the *"real logic vs pass-through
  wiring"* filter applied. The audit is intentionally not done in
  this step (out of scope; better done at the moment a Task starts so
  the result lands with concrete bug classes).
- **Phase F reframing as catastrophic-regression tripwires** — only
  if/when the team decides such a Task is worth opening; not on the
  Recommended Next Steps list pending that decision.
- **Audit doc §1.4 / §1.5 numeric tables** — kept as descriptive
  snapshots; under the new framing they don't drive planning, but
  they're not actively wrong either, so no edits made beyond the
  banners.
