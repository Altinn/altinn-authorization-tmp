import { group, check } from "k6";
import { getRevisorCustomerIdentifiersForParty } from "./helpers/register-test.js";
import {
  removeRevisorRoleFromEr,
  addRevisorRoleToErForOrg,
} from "./helpers/er-requests.js";
import { retry } from "./helpers/helpers.js";

const facilitatorPartyUuidRevisor = "7c1170ec-8232-4998-a277-0ba224808541";
const facilitatorOrg = "314239458";

function testRemovalOfRevisorRoleForClient() {
  group("Revisor role end-to-end test", function () {
    const targetOrg = pickCustomerOrg();

    removeRoleFromEr(targetOrg);
    addRoleBackToEr(targetOrg);
  });
}

function pickCustomerOrg() {
  const currentOrgs = getRevisorCustomerIdentifiersForParty(
    facilitatorPartyUuidRevisor
  );
  console.log(`Initial number of revisor customers: ${currentOrgs.length}`);

  if (currentOrgs.length === 0) {
    throw new Error("No revisor customers found to test with.");
  }

  const targetOrg = currentOrgs[0];
  console.log(
    `Picked target client organizationIdentifier for test: ${targetOrg}`
  );
  return targetOrg;
}

function removeRoleFromEr(targetOrg) {
  group("Remove revisor role", function () {
    removeRevisorRoleFromEr(targetOrg, facilitatorOrg);
    console.log(
      `Requested removal of Revisor role between ${targetOrg} and ${facilitatorOrg}`
    );

    const success = retry(
      () => {
        const orgs = getRevisorCustomerIdentifiersForParty(
          facilitatorPartyUuidRevisor
        );
        const stillPresent = orgs.includes(targetOrg);
        console.log(
          `[remove role] Org ${targetOrg} is ${
            stillPresent ? "still" : "no longer"
          } in the list (${orgs.length})`
        );
        return !stillPresent;
      },
      {
        retries: 10,
        intervalSeconds: 30,
        testscenario: "remove revisor role",
      }
    );

    check(success, {
      "Revisor role was successfully removed": (s) => s === true,
    });
  });
}

function addRoleBackToEr(targetOrg) {
  group("Add revisor role back", function () {
    addRevisorRoleToErForOrg(targetOrg, facilitatorOrg);
    console.log(
      `Requested adding back Revisor role between ${targetOrg} and ${facilitatorOrg}`
    );

    const success = retry(
      () => {
        const orgs = getRevisorCustomerIdentifiersForParty(
          facilitatorPartyUuidRevisor
        );
        const nowPresent = orgs.includes(targetOrg);
        console.log(
          `[add role] Org ${targetOrg} is ${
            nowPresent ? "now" : "still not"
          } in the list (${orgs.length})`
        );
        return nowPresent;
      },
      {
        retries: 10,
        intervalSeconds: 30,
        testscenario: "add revisor role back",
      }
    );

    check(success, {
      "Revisor role was successfully added back": (s) => s === true,
    });
  });
}

export default function () {
  testRemovalOfRevisorRoleForClient();
}
