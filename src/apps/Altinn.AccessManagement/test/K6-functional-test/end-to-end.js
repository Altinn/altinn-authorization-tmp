import { getRevisorCustomerIdentifiersForParty } from "./register-test.js";
import {
  removeRevisorRoleFromEr,
  addRevisorRoleToErForOrg,
} from "./er-requests.js";
import { retry } from "./helpers.js";
import { check } from "k6";


export default function removeAndAddRevisorRoleFromOrganization() {
  const facilitatorPartyUuidRevisor = "7c1170ec-8232-4998-a277-0ba224808541";
  const facilitatorOrg = "314239458";

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

  removeRevisorRoleFromEr(targetOrg, facilitatorOrg);
  console.log(
    `Requested removal of Revisor role between ${targetOrg} and ${facilitatorOrg}`
  );

  const removeSuccess = retry(
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

  check(removeSuccess, {
    "Revisor role was successfully removed": (s) => s === true,
  });

  addRevisorRoleToErForOrg(targetOrg, facilitatorOrg);
  console.log(
    `Requested adding back Revisor role between ${targetOrg} and ${facilitatorOrg}`
  );

  const addSuccess = retry(
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

  check(addSuccess, {
    "Revisor role was successfully added back": (s) => s === true,
  });
}