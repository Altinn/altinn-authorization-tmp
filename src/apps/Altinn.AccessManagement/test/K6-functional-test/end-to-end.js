import GetCustomerForPartyUuid from "./register-test.js";
import {
  removeRevisorRoleFromEr,
  addRevisorRoleToErForOrg,
} from "./er-requests.js";

import { retry } from "./helpers.js";

export default function removeAndAddRevisorRoleFromOrganization() {
  const facilitatorPartyUuidRevisor = "368f5a82-97f5-4f33-b372-ac998a4d6b22";
  const facilitatorOrg = "314239458"; // change to read this from file at some point to provide more test data

  // 1. Fetch customer from Register to make sure we have test user
  const clientOrg = GetCustomerForPartyUuid(facilitatorPartyUuidRevisor);

  console.log(`Fetched client organizationIdentifier: ${clientOrg}`);

  // 2. Remove Revisor role
  removeRevisorRoleFromEr(clientOrg, facilitatorOrg);
  console.log(
    `Requested removal of Revisor role between client ${clientOrg} and facilitator ${facilitatorOrg}`
  );

  // 3. Wait until Revisor role is removed
  retry(
    () => {
      const lookupResponse = GetCustomerForPartyUuid(
        facilitatorPartyUuidRevisor
      );
      extractOrganizationIdentifier(lookupResponse.body); // Will throw if found
      throw new Error("Revisor role still exists");
    },
    { retries: 10, intervalSeconds: 2 }
  ).catch(() => {
    console.log("Revisor role successfully removed!");
  });

  // 4. Add Revisor role back
  addRevisorRoleToErForOrg(clientOrg, facilitatorOrg);
  console.log(
    `Requested adding back Revisor role between client ${clientOrg} and facilitator ${facilitatorOrg}`
  );

  // 5. Wait until Revisor role is added
  retry(
    () => {
      const lookupResponse = GetCustomerForPartyUuid(
        facilitatorPartyUuidRevisor
      );
      const orgIdentifier = extractOrganizationIdentifier(lookupResponse.body);
      console.log(
        `Found organizationIdentifier after adding back: ${orgIdentifier}`
      );
      return orgIdentifier;
    },
    { retries: 10, intervalSeconds: 2 }
  );
}
