# Enduser API Test Scenarios

This document describes the people, organizations, relationships, and resources used across the integration tests for the Enduser API (ConnectionsController and AuthorizedPartiesController).

## People

| Name | Description | Key Roles |
|------|-------------|-----------|
| **Malin Emilie** | Ambitious young entrepreneur who co-founded Dumbo Adventures. Handles all the administrative and financial paperwork. | Managing Director of Dumbo Adventures |
| **Thea BFF** | Malin's best friend since childhood. Runs her own dog grooming business but also helps out at Dumbo Adventures. Multi-tasker extraordinaire. | Managing Director of Mille Hundefrisor, Rightholder at Dumbo Adventures |
| **Josephine Yvonnesdottir** | Freelance graphic designer who does contract work for Kaos Magic Design and Arts. Has been given access to specific tax filing instances on behalf of Kaos. | Rightholder at Kaos Magic Design and Arts |
| **Jinx Arcane** | Creative director and founder of Kaos Magic Design and Arts. Known for bold visual designs and chaotic brainstorming sessions. | Managing Director of Kaos Magic Design and Arts |
| **Alex The Artist** | A uniquely skilled designer — there is nothing she can't do. Whether it's 3D modeling, typography, welding stage props, or coding interactive installations, Alex delivers. She's also Jinx's business partner and the more level-headed half of Kaos, keeping the books balanced while Jinx dreams big. | Chair of the Board at Kaos Magic Design and Arts |
| **Milena Solstad** | Thea's most trusted employee at the dog grooming salon. Handles the business side when Thea is off helping Malin. | Chair of the Board at Mille Hundefrisor |
| **Bodil Farmor** | A retired grandmother who occasionally helps with Dumbo Adventures events. Used in person lookup tests. | (no org role) |
| **Lars Bakke** | A seasoned bakery owner who keeps things running smoothly at Baker Johnsen. | Managing Director of Baker Johnsen |

## Organizations

| Name | Type | Description |
|------|------|-------------|
| **Dumbo Adventures AS** | AS | A small adventure tourism company run by Malin Emilie. Offers guided hiking, kayaking, and northern lights trips. |
| **Mille Hundefrisor** | ENK | Thea's sole proprietorship dog grooming salon, famous in the neighborhood for poodle haircuts. |
| **Kaos Magic Design and Arts** | AS | A creative agency founded by Jinx Arcane, specializing in event design, visual effects, and immersive art installations. |
| **Baker Johnsen** | AS | A traditional Norwegian bakery. Used in RemoveAssignment tests as a clean relationship target with no dependencies. |
| **Svendsen Automobil** | AS | An auto repair shop. Used to test removing non-existent connections (idempotent delete). |

## Relationships (Default Seed)

```
Dumbo Adventures AS
  |-- Malin Emilie .............. Managing Director (DAGL)
  |-- Thea BFF ................. Rightholder + SalarySpecialCategory package
  |-- Kaos Magic Design and Arts  Auditor (org-to-org)

Mille Hundefrisor
  |-- Thea BFF ................. Managing Director (DAGL)
  |-- Milena Solstad ........... Chair of the Board (LEDE)

Kaos Magic Design and Arts
  |-- Jinx Arcane .............. Managing Director (DAGL)
  |-- Alex The Artist .......... Chair of the Board (LEDE)
  |-- Josephine Yvonnesdottir .. Rightholder

Private Persons (self-to-self)
  Malin Emilie, Thea, Josephine, Milena, Jinx Arcane, Alex The Artist
```

### Relationships Added by Individual Tests

| Test Class | Relationship | Purpose |
|------------|-------------|---------|
| GetResourceRights | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Tests resource-level rights with delegation policies |
| GetResources | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Tests resource listing for a connection |
| AddResourceRights | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Malin delegates resource rights from Dumbo to Mille |
| UpdateResourceRights | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Malin updates resource rights from Dumbo to Mille |
| RemoveResource | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Remove resource delegation from both perspectives |
| AddInstanceRights | Dumbo Adventures -> Kaos (Rightholder) | Malin delegates instance rights from Dumbo to Kaos |
| RemoveInstance | Dumbo Adventures -> Kaos (Rightholder) | Add then remove instance delegation, from both perspectives |
| UpdateInstanceRights | Jinx Arcane -> Thea (Rightholder) | Jinx delegates and updates instance rights person-to-person |
| RemoveAssignment | Dumbo Adventures -> Baker Johnsen (Rightholder) | Clean connection for basic remove test |

## Resources

| Name | RefId | Description |
|------|-------|-------------|
| **Sirius Skattemelding** | `app_skd_sirius-skattemelding-v1` | Tax return filing service from the Norwegian Tax Administration (Skatteetaten). The main resource used across instance delegation tests. Has 9 delegatable right keys covering instantiate, read, write, and confirm actions across multiple workflow stages. |
| **Mattilsynet Bakery Service** | `app_mat_mattilsynet-baker-konditorvare` | Food safety inspection reporting service from the Norwegian Food Safety Authority (Mattilsynet). Used as a secondary resource in instance tests. Has fewer delegatable actions (instantiate, read). |
| **Nav Sykepenger Dialog** | `nav_sykepenger_dialog` | Dialogue service for sickness benefits from NAV. Used in delegation check tests. |
| **Nav Sykepenger Sykmelding** | `nav_sykepenger_sykmelding` | Sick leave notification service from NAV. Used in RemoveResource tests. |
| **Skattemelding** | `app_skd_skattemelding` | Generic tax return resource used in resource-level delegation tests. |
| **MVA-melding** | `app_skd_mva-melding` | VAT reporting resource used alongside Skattemelding in resource listing tests. |

## Access Packages

| Name | URN | Description |
|------|-----|-------------|
| **SalarySpecialCategory** | `urn:altinn:accesspackage:lonn-personopplysninger-saerlig-kategori` | Access to salary services with sensitive personal data. Pre-assigned to Dumbo->Thea Rightholder connection. |
| **AccountingAndEconomicReporting** | `urn:altinn:accesspackage:regnskap-okonomi-rapport` | Access to accounting and financial reporting services. Used in AddAssignmentPackage tests. |
| **Toll** | `urn:altinn:accesspackage:toll` | Access to customs services. Used in AddAssignmentPackage URN-based test. |

## Instance Delegations (Default Seed)

Josephine has been given access to two specific instances on behalf of Kaos:

| From | To | Resource | Instance ID | Delegation Policy |
|------|----|----------|-------------|-------------------|
| Kaos | Josephine | Sirius Skattemelding | `urn:altinn:instance-id:50315678/b1a2c3d4-...` | read + write, Task_1 read + write |
| Kaos | Josephine | Mattilsynet Bakery | `urn:altinn:instance-id:50315678/a2b3c4d5-...` | read only |

## Test Coverage by Endpoint

### ConnectionsController — Connections

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections` | GetConnections | Various | Bidirectional scope enforcement, from/to-others direction |
| `POST /connections` | AddRightholder | Malin | Add new rightholder to Dumbo (org and person via PersonInput) |
| `DELETE /connections` | RemoveAssignment | Malin | Remove rightholder, cascade vs non-cascade with packages, idempotent delete |
| `GET /connections/users` | GetAvailableUsers | Malin | List available users for Dumbo |

### ConnectionsController — Access Packages

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections/accesspackages` | GetPackages | Malin, Thea | Package listing from both perspectives, empty connection |
| `POST /connections/accesspackages` | AddAssignmentPackage | Jinx | Add package by ID and URN, invalid package, round-trip via GetPackages |
| `DELETE /connections/accesspackages` | — | — | **Not yet tested** |
| `GET /connections/accesspackages/delegationcheck` | — | — | **Not yet tested** |

### ConnectionsController — Roles

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections/roles` | — | — | **Not yet tested** |
| `DELETE /connections/roles` | — | — | **Not yet tested** |
| `GET /connections/roles/delegationcheck` | — | — | **Not yet tested** |

### ConnectionsController — Resources

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections/resources` | GetResources | Malin, Thea | Resource listing for Dumbo<->Mille connection |
| `GET /connections/resources/rights` | GetResourceRights | Malin, Thea | Direct and indirect rights for delegated resources |
| `POST /connections/resources/rights` | AddResourceRights | Malin | Delegate resource rights from Dumbo to Mille |
| `PUT /connections/resources/rights` | UpdateResourceRights | Malin | Update delegated resource rights |
| `DELETE /connections/resources` | RemoveResource | Malin, Thea | Remove resource delegation from both perspectives |
| `GET /connections/resources/delegationcheck` | CheckResource | Malin, Thea | Check delegatable rights (full, partial, package-only access) |

### ConnectionsController — Instances

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections/resources/instances` | GetInstances | Jinx, Josephine | List instance delegations, empty list, bidirectional |
| `GET /connections/resources/instances/rights` | GetInstanceRights | Jinx, Josephine | View specific instance rights with permission details |
| `POST /connections/resources/instances/rights` | AddInstanceRights | Malin | Delegate instance rights Dumbo->Kaos, XACML policy verification, round-trip via GetInstanceRights |
| `PUT /connections/resources/instances/rights` | UpdateInstanceRights | Jinx | Update instance rights Jinx->Thea (person-to-person), lifecycle: add->verify->update->verify |
| `DELETE /connections/resources/instances` | RemoveInstance | Malin, Jinx | Remove instance delegation, round-trip verify gone, bidirectional (to-others + from-others) |
| `GET /connections/resources/instances/delegationcheck` | CheckInstance | Malin | Check delegatable instance right keys for Sirius and Mattilsynet |
| `GET /connections/resources/instances/users` | GetInstanceUsers | Jinx | List users with access to instance (returns Josephine), empty for unknown instance |

### AuthorizedPartiesController

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /authorizedparties` | AuthorizedPartiesControllerTest | Malin, Thea, Josephine, Alex | List authorized parties with roles, packages, instances; includeInstances flag; key-role inheritance |

## Key Test Patterns

- **Scope enforcement**: Every endpoint tests that wrong scopes (from-others vs to-others, read vs write) return 403 Forbidden
- **Bidirectional queries**: Tests verify both the "giving" perspective (to-others) and "receiving" perspective (from-others)
- **Round-trip verification**: Add/Update tests verify the result by reading back via GET endpoints
- **XACML policy inspection**: Write tests parse the generated XACML delegation policy to verify subject (UUID for instances, partyId for resources), resource attributes, and action rules
- **Cascade behavior**: RemoveAssignment tests both non-cascade (fails with packages) and cascade (succeeds) deletion
- **Idempotent deletes**: Removing non-existent connections/instances returns 204 NoContent

## Mock Infrastructure

| Mock | Purpose |
|------|---------|
| `PermitPdpMock` | Always returns Permit for PDP decisions (registered globally in ApiFixture) |
| `PolicyRetrievalPointMock` | Loads XACML policies from test data files on disk |
| `PolicyRetrievalPointWithWrittenPoliciesMock` | Extends above to also check in-memory `PolicyFactoryMock.WrittenPolicies` — used in tests that write then read policies in the same test |
| `PolicyFactoryMock` | Captures written delegation policies in `WrittenPolicies` dictionary for assertion |
| `Altinn2RightsClientMock` | Prevents real HTTP calls to Altinn 2 SBL Bridge |
| `ResourceRegistryClientMock` | Provides resource registry policy lookups from test data |
| `UserProfileLookupServiceMock` | Handles person lookup by SSN + last name |
| `AltinnRolesClientMock` | Provides role definitions for authorized parties tests |
| `ProfileClientMock` | Provides user profile data for authorized parties tests |
