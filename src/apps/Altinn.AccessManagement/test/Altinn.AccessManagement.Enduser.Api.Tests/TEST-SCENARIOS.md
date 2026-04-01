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

## Organizations

| Name | Type | Description |
|------|------|-------------|
| **Dumbo Adventures AS** | AS | A small adventure tourism company run by Malin Emilie. Offers guided hiking, kayaking, and northern lights trips. |
| **Mille Hundefrisor** | ENK | Thea's sole proprietorship dog grooming salon, famous in the neighborhood for poodle haircuts. |
| **Kaos Magic Design and Arts** | AS | A creative agency founded by Jinx Arcane, specializing in event design, visual effects, and immersive art installations. |

## Relationships (Default Seed)

```
Dumbo Adventures AS
  |-- Malin Emilie .............. Managing Director (DAGL)
  |-- Thea BFF ................. Rightholder
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
| AddInstanceRights | Dumbo Adventures -> Kaos (Rightholder) | Malin delegates instance rights from Dumbo to Kaos |
| UpdateInstanceRights | Jinx Arcane -> Thea (Rightholder) | Jinx delegates and updates instance rights person-to-person |
| GetResourceRights | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Tests resource-level rights with delegation policies |
| GetResources | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Tests resource listing for a connection |
| AddResourceRights | Dumbo Adventures -> Mille Hundefrisor (Rightholder) | Malin delegates resource rights from Dumbo to Mille |

## Resources

| Name | RefId | Description |
|------|-------|-------------|
| **Sirius Skattemelding** | `app_skd_sirius-skattemelding-v1` | Tax return filing service from the Norwegian Tax Administration (Skatteetaten). The main resource used across instance delegation tests. Has 9 delegatable right keys covering instantiate, read, write, and confirm actions across multiple workflow stages. |
| **Mattilsynet Bakery Service** | `app_mat_mattilsynet-baker-konditorvare` | Food safety inspection reporting service from the Norwegian Food Safety Authority (Mattilsynet). Used as a secondary resource in instance tests. Has fewer delegatable actions (instantiate, read). |
| **Nav Sykepenger Dialog** | `nav_sykepenger_dialog` | Dialogue service for sickness benefits from NAV. Used in delegation check tests. |
| **Skattemelding** | `app_skd_skattemelding` | Generic tax return resource used in resource-level delegation tests. |
| **MVA-melding** | `app_skd_mva-melding` | VAT reporting resource used alongside Skattemelding in resource listing tests. |

## Instance Delegations (Default Seed)

Josephine has been given access to two specific instances on behalf of Kaos:

| From | To | Resource | Instance ID | Delegation Policy |
|------|----|----------|-------------|-------------------|
| Kaos | Josephine | Sirius Skattemelding | `urn:altinn:instance-id:50315678/b1a2c3d4-...` | read + write, Task_1 read + write |
| Kaos | Josephine | Mattilsynet Bakery | `urn:altinn:instance-id:50315678/a2b3c4d5-...` | read only |

## Test Coverage by Endpoint

### ConnectionsController

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /connections` | GetConnections | Various | Bidirectional scope enforcement, from/to-others direction |
| `GET /connections/resources` | GetResources | Malin, Thea | Resource listing for Dumbo<->Mille connection |
| `GET /connections/resources/rights` | GetResourceRights | Malin, Thea | Direct and indirect rights for delegated resources |
| `POST /connections/resources/rights` | AddResourceRights | Malin | Delegate resource rights from Dumbo to Mille |
| `PUT /connections/resources/rights` | UpdateResourceRights | Malin | Update delegated resource rights |
| `DELETE /connections/resources` | RemoveResource | Malin, Thea | Remove resource delegation |
| `GET /connections/resources/delegationcheck` | CheckResource | Malin | Check which resources can be delegated |
| `POST /connections` | AddRightholder | Malin | Add new rightholder to Dumbo |
| `GET /connections/users` | GetAvailableUsers | Malin | List available users for Dumbo |
| `GET /connections/resources/instances` | GetInstances | Jinx, Josephine | List instance delegations for Kaos<->Josephine |
| `GET /connections/resources/instances/rights` | GetInstanceRights | Jinx, Josephine | View specific instance rights with read/write details |
| `POST /connections/resources/instances/rights` | AddInstanceRights | Malin | Delegate instance rights from Dumbo to Kaos |
| `PUT /connections/resources/instances/rights` | UpdateInstanceRights | Jinx | Update instance rights from Jinx (person) to Thea (person) |

### AuthorizedPartiesController

| Endpoint | Test File | Actor | Scenario |
|----------|-----------|-------|----------|
| `GET /authorizedparties` | AuthorizedPartiesControllerTest | Malin, Thea, Josephine, Alex | List authorized parties with roles, packages, instances |

## Key Test Patterns

- **Scope enforcement**: Every endpoint tests that wrong scopes (from-others vs to-others, read vs write) return 403 Forbidden
- **Bidirectional queries**: Tests verify both the "giving" perspective (to-others) and "receiving" perspective (from-others)
- **Round-trip verification**: Add/Update tests verify the result by reading back via GET endpoints
- **XACML policy inspection**: Write tests parse the generated XACML delegation policy to verify subject (UUID for instances, partyId for resources), resource attributes, and action rules
