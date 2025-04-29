import GetCustomerForPartyUuid from "./register-test.js";
import {
  removeRevisorRoleFromEr,
  addRevisorRoleToErForOrg,
} from "./er-requests.js";
import { retry } from "./helpers.js";

export default function removeAndAddRevisorRoleFromOrganization() {
  const facilitatorPartyUuidRevisor = "368f5a82-97f5-4f33-b372-ac998a4d6b22";
  const facilitatorOrg = "314239458"; // Facilitator's org

  // 1. Fetch client org dynamically
  const clientOrg = GetCustomerForPartyUuid(facilitatorPartyUuidRevisor);
  console.log(`Fetched client organizationIdentifier: ${clientOrg}`);

  // 2. Remove Revisor role
  removeRevisorRoleFromEr(clientOrg, facilitatorOrg);
  console.log(
    `Requested removal of Revisor role between ${clientOrg} and ${facilitatorOrg}`
  );

  // 3. Retry until Revisor role is removed
  retry(
    () => {
      const orgIdentifier = GetCustomerForPartyUuid(
        facilitatorPartyUuidRevisor
      );
      if (orgIdentifier) {
        throw new Error("Revisor role still exists");
      }
      console.log("Revisor role successfully removed!");
    },
    { retries: 10, intervalSeconds: 2 }
  );

  // 4. Add Revisor role back
  addRevisorRoleToErForOrg(clientOrg, facilitatorOrg);
  console.log(
    `Requested adding back Revisor role between ${clientOrg} and ${facilitatorOrg}`
  );

  // 5. Retry until Revisor role is added again
  retry(
    () => {
      const orgIdentifier = GetCustomerForPartyUuid(
        facilitatorPartyUuidRevisor
      );
      if (!orgIdentifier) {
        throw new Error("Revisor role not yet added back");
      }
      console.log(
        `Revisor role added back successfully with organizationIdentifier: ${orgIdentifier}`
      );
    },
    { retries: 10, intervalSeconds: 2 }
  );
}
