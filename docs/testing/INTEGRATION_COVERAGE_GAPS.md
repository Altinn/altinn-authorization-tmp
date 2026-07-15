# Integration Coverage Gaps

This is a working document for issue #3378. It diffs the deep-assertion Bruno
scenarios against the existing C# `[IntegrationTest]` suites and names the
scenarios that are genuinely missing, ranked by value. It is the first slice of
#3378: a scoping report, no test code. Later slices turn the ranked table below
into PR-sized batches.

Read [BRUNO_API_TESTS.md](BRUNO_API_TESTS.md) and [TEST_PROJECTS.md](TEST_PROJECTS.md)
first. This doc assumes both.

## How the two sides were sampled

Numbers here come from the branch state on 2026-07-15.

**Bruno.** Two collections:

| Collection | Path | Request files (excl. `folder.bru`) |
|---|---|---|
| AccessManagement | `src/apps/Altinn.AccessManagement/test/Bruno/AccessMgmt/test/` | 493 |
| Authorization | `src/apps/Altinn.Authorization/test/Bruno/Altinn.Authorization/test/` | 90 |

The AccessManagement collection was inventoried by directory and by assertion
depth, not read file by file. Grouping every `.bru` under `test/` by whether its
`tests { }` block references the response body (`res.getBody()`, `data.`,
`.to.have.property`, etc.) rather than only `res.status`:

- ~197 files carry body-field assertions.
- ~251 files assert status only.
- ~105 files carry no `expect` at all (pure setup / token / lookup requests).

So roughly a third of the AccessManagement collection asserts anything about the
body, and the deep-delegation subset the issue cares about is a minority of that
third. Individual scenarios named in the gap table below were opened and read.

The Authorization collection is small enough to enumerate fully. 86 of its 90
requests assert on the decision outcome (`decision` equal to `Permit` / `Deny` /
`NotApplicable` / `Indeterminate`). It is the highest-value collection per file:
almost every request is a real authorization-correctness assertion, not a smoke
check.

**C# integration tests.** Every class tagged `[IntegrationTest]` was located
(92 files across the AccessManagement and Authorization test projects). The
decision/authorize suites, the thin controllers, and the client-delegation
suites were read to confirm what they actually assert, not inferred from names.
This repo has a history of "genuinely untested X" turning out to be covered, so
each claimed gap below was checked against the real test file. The
[already-covered section](#already-covered-do-not-re-port) records the checks
that came back covered.

## Existing C# integration coverage (Authorization decision path)

The decision/authorize path is the densest existing coverage. For context before
the gap table:

| Test class | Methods | What it asserts |
|---|---|---|
| `Integration/AltinnApps_DecisionTests.cs` | 31 | App-based XACML decisions (skd/tdd app policies) |
| `Integration/Xacml30ConformanceTests.cs` | 36 | XACML 3.0 spec conformance |
| `Integration/ResourceRegistry_DecisionTests.cs` | 25 | Resource-registry policies, including person-has-access-package Permit, no-package NotApplicable, ungranted-action NotApplicable, systemuser access-package multi-request |
| `Integration/ExternalDecisionTest.cs` | 21 | App-instance read/multi-action, resource-on-org/self/other, subject-org-in-policy, systemuser resource + app-instance delegation, indeterminate/too-many-subjects |
| `Integration/PolicyControllerTest.cs` | 16 | Policy upload/get, `roleswithaccess` |
| `Integration/PartiesControllerTest.cs` | 8 | Party list + validate |
| `Integration/AccessListAuthorizationControllerTest.cs` | 2 | One permit-without-action-filter, one missing-token unauthorized |
| `Integration/PdpDecisionTelemetryTests.cs` | 2 | Decision telemetry emission |

## Ranked gap list

Ranked by value: authorization-correctness and negative/boundary cases highest,
happy-path smoke lowest. "Endpoint" is the API under test; "Seed" is the state a
C# port has to build in the fixture (the Bruno IDs target live AT/TT02 and are
not reusable in-process, so seed the equivalent entities via `TestData.*` /
fixture builders).

| # | Bruno source | Endpoint | Asserts | Why it matters | Seed a C# port needs |
|---|---|---|---|---|---|
| 1 | `Authorize/AccessList/AccessList_AC1..AC4_*` (9) | `POST authorization/api/v1/authorize` | AC1/AC2 Permit, AC3/AC4 Deny, each with and without a delegation, plus a systemuser resource-delegation permit | Authorization boundary with almost no C# coverage: `AccessListAuthorizationControllerTest` has 2 methods and no Deny case at all | Access-list resource + policy, list membership for the permit parties, a non-member for the deny cases, a delegation for the `_Delegation_` variants, a system user for the systemuser variant |
| 2 | `SystemUserClientDelegation/NegativeTests/*` (10) plus the positive `SystemUserClientDelegation/*` (18) | `SystemUserClientDelegationController` (AccessMgmt Internal API) | Forbidden when acting as client `Dagl`, rejected without a client delegation, invalid/unknown package rejected, delegation to a person rejected, non-BRL client org rejected; positive create/get/delete of client-delegated access packages | Controller has only `[UnitTest]` Moq coverage (`Altinn.AccessManagement.Api.Tests/Controllers/SystemUserClientDelegationControllerTest.cs`), zero integration coverage; these are authorization-boundary negatives on a client-delegation write path | System user, client and agent orgs (REGN/REVI/BRL/ESEK variants), access packages (FFØR etc.), an existing client delegation for the positive-path and delete cases |
| 3 | `Decision/SystemResource_Tests/*` (17) | `POST authorization/api/v1/decision` | For ClientAdministration / EnduserAccessManagement / InstanceDelegation / MainAdmin system resources, each decided as `Dagl` / `Hadm` / `Kladm` / `Admai`, returns the specific Permit vs NotApplicable | Role-to-system-resource authorization matrix; each role x resource has a defined Permit/NotApplicable outcome and none of it is in the C# decision suites | The four system-resource policies, a reportee org, and a subject holding each role (Dagl, Hadm, Kladm, Admai) plus a key-role subject for the multi-request case |
| 4 | `Decision/AccessPackageResource/*` (17) and `Authorize/AccessPackageResource/*` (11) | `POST decision` and `POST authorize` | Access-package resolution via key role, for a main unit vs sub unit, as business operator (Forretningsforer), as accountant sole-proprietor (RegnEnk / RegnEnk_Innh), as rightholder vs Dagl | Delegation/role-inheritance correctness. C# covers only the flat case (person directly holds the package) in `ResourceRegistry_DecisionTests`; the inheritance dimensions are untested | Person subjects, a main unit with sub units, key-role links, an access package granting the resource, a business-operator relationship, an accountant relationship, plus the resource policy |
| 5 | `Authorize/AppInstanceDelegation/*` (18) | `POST authorize` | P2P / P2Main / P2Sub / Main2Sub / Sub2Main / Sub2Sub / O2O app-instance delegation, as agent-of-to and dagl-of-to, plus MissingInstanceId and MissingTask returning NotApplicable | Org-hierarchy delegation matrix and the negative NotApplicable cases; `ExternalDecisionTest` covers app-instance read + multi-action Permit only, not the hierarchy or the negatives | App + instance, main/sub org hierarchy, an app-instance delegation between the parties, agent and dagl subjects, and requests missing instance-id / task for the negatives |
| 6 | `Decision/SystemUser/*` (4) | `POST decision` | System user with client-delegated access package Permit (org and ENK), NoDelegation NotApplicable, resource-delegation-via-access-list Permit | Fills the systemuser access-package + access-list decision cases; `ExternalDecisionTest` covers systemuser resource + app-instance delegation but not these | System user, client org, a client-delegated access package (and an ENK variant), plus a case with no delegation for the NotApplicable |
| 7 | `Roles/getRoles.bru` (1) | `RolesController` (Authorization) | Returns the role list | Controller has zero integration tests. Low value on its own (single thin Bruno request, small surface), but cheap and closes a zero-coverage controller | A reportee with known roles |
| 8 | `AuthorizedParties/AsServiceOwner/*` | ServiceOwner `AuthorizedPartiesController` | Authorized-parties list as a service owner | `ServiceOwner.Api.Tests/.../AuthorizedPartiesControllerTest.cs` has 2 methods vs 19 on the Enduser side; the service-owner variant is thin | Reportee with assignments/delegations, service-owner token |
| 9 | PIP base requests under `EnduserAPI` / `InternalAPI` | `PolicyInformationPointController`, `PolicyInformationPointResourcesAndInstancesTest` surface | Resources-and-instances PIP responses | Thin: `PolicyInformationPointControllerTest` and `...ResourcesAndInstancesTest` have 2 methods each (the roles/access-packages PIP is already deep at 15). Lower value; the roles/packages half is done | Parties, resources, instances matching the existing PIP fixture data |

## Already covered (do not re-port)

Verified against the actual test files. The perceived gap here is illusory.

- **App and resource decisions.** `AltinnApps_DecisionTests` (31),
  `Xacml30ConformanceTests` (36) and `ExternalDecisionTest` (21) already cover
  app-based decisions, XACML conformance, resource-on-org / self / other-person,
  unknown-action NotApplicable, and subject-org-in-policy. The `Decision/` and
  `Authorize/` root Bruno requests (`postDecisionRead*`, `postAuthorizeTtd*`,
  `MultiReq*`) map onto these. Do not re-port them.
- **Basic access-package decision.** `ResourceRegistry_DecisionTests` already
  covers person-has-access-package Permit, subject-without-package NotApplicable,
  access-package-ungranted-action NotApplicable, and systemuser-with-access-package
  multi-request Permit. Only the *inheritance* dimensions (gap #4) are missing.
- **SystemUser resource + app-instance delegation decision.**
  `ExternalDecisionTest` covers systemuser resource delegation (with and without
  event log), systemuser app delegation (including the multiple-obligations bug
  fix), and the too-many-subjects Indeterminate. Only the client-delegated
  access-package and access-list systemuser cases (gap #6) are missing.
- **Enduser Connections.** `ConnectionsControllerTest` is split across ~30
  partial files (`GetPackages`, `GetResources`, `CheckPackage`, `CheckResource`,
  `AddAssignmentPackage`, `AddResourceRights`, `RemovePackages`, `RemoveResource`,
  `DelegationCheckRoles`, and more) with real body assertions. The large
  `EnduserAPI/Connections/*` Bruno subtree (210 requests) largely maps here.
  Spot-check any specific Connections scenario against these partials before
  assuming a gap.
- **Enduser ClientDelegation.** `ClientDelegationControllerTest` has 27
  integration methods. The `KlientDelegation/` Bruno collection (22 requests) is
  largely covered on the enduser side. Check per scenario before porting.
- **Enduser AuthorizedParties.** `Enduser.Api.Tests/.../AuthorizedPartiesControllerTest.cs`
  has 19 integration methods. Most of the 64-request `AuthorizedParties/` Bruno
  subtree is the deployed-environment variant of this. The enduser side is
  covered; only the service-owner variant is thin (gap #8).
- **Consent.** BFF (25), Enterprise (18) and Maskinporten (7) consent controller
  integration tests total 50 methods with body assertions. The Consent Bruno
  requests are well covered.
- **Maskinporten Consumers / Suppliers.** Dedicated integration test files exist
  (`MaskinportenConsumersControllerIntegrationTest`,
  `MaskinportenSuppliersControllerIntegrationTest`, plus resource-filter and
  resource-delegation variants). Scenarios that need a live Maskinporten are out
  of scope for in-process tests per the issue.
- **PIP roles and access packages.**
  `PolicyInformationPointRolesAndAccessPackagesTest` has 15 methods. Only the
  base PIP controller and the resources-and-instances half are thin (gap #9).

## Proposed slicing into PR-sized batches

Each batch is one Task + PR. Ordering front-loads the highest-value,
lowest-seed-cost work and defers the batches that need the most fixture building.

- **Batch A. Prototype slice (issue's second bullet).** Port 3 to 5
  representative Decision/Authorize scenarios across Permit / Deny / NotApplicable,
  drawn from gaps #1 and #3, building the seed data as reusable fixture helpers.
  Proves the pattern and measures per-test effort before the bigger matrices.
- **Batch B. AccessList authorization (gap #1).** Fill the AC1 to AC4
  Permit/Deny matrix on `AccessListAuthorizationController`, with and without
  delegation, plus the systemuser resource-delegation case. Small seed surface,
  large correctness win on a near-uncovered boundary controller.
- **Batch C. SystemResource decision matrix (gap #3).** The role x
  system-resource Permit/NotApplicable grid. Reuses the role-seeding from Batch A.
- **Batch D. SystemUserClientDelegation integration + negatives (gap #2).**
  Stand up integration coverage for the controller that has only unit tests
  today, leading with the authorization-boundary negatives.
- **Batch E. AccessPackageResource inheritance (gap #4).** The key-role /
  main-unit / business-operator / accountant / rightholder matrix. Highest seed
  cost (org hierarchies + inheritance links), so it comes after the pattern and
  the role-seeding helpers are proven.
- **Batch F. AppInstanceDelegation authorization matrix (gap #5).** Org-hierarchy
  delegation grid plus the MissingInstanceId / MissingTask NotApplicable
  negatives.
- **Batch G. SystemUser decision + thin controllers (gaps #6, #7, #8, #9).**
  The systemuser client-delegation/access-list decisions plus the cheap
  zero/thin-coverage closers (Authorization `RolesController`, service-owner
  AuthorizedParties, base PIP).

Seed-data builders extracted along the way (the issue's last bullet) are shared
across batches rather than re-derived per PR: role-holder seeding (A, B, C),
org-hierarchy + inheritance links (E, F), and system-user + client-delegation
state (D, G).
