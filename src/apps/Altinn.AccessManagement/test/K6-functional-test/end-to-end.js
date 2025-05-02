import getCustomerForPartyUuid from "./register-test.js";
import {
  removeRevisorRoleFromEr as removeRevisorRoleFromER,
  addRevisorRoleToErForOrg,
} from "./er-requests.js";
import { retry } from "./helpers.js";

export default function removeAndAddRevisorRoleFromOrganization() {
  const facilitatorPartyUuidRevisor = "368f5a82-97f5-4f33-b372-ac998a4d6b22";
  const facilitatorOrg = "314239458";

  const clientOrg = getCustomerForPartyUuid(facilitatorPartyUuidRevisor);
  console.log(`Fetched client organizationIdentifier: ${clientOrg}`);

  removeRevisorRoleFromER(clientOrg, facilitatorOrg);
  console.log(`Requested removal of Revisor role between ${clientOrg} and ${facilitatorOrg}`);

  retry(() => {
    const orgIdentifier = getCustomerForPartyUuid(facilitatorPartyUuidRevisor);
    if (orgIdentifier === clientOrg) {
      throw new Error("Revisor role still exists for Org");
    }
  }, { retries: 2, intervalSeconds: 2, name: "remove revisor role" });

  addRevisorRoleToErForOrg(clientOrg, facilitatorOrg);
  console.log(`Requested adding back Revisor role between ${clientOrg} and ${facilitatorOrg}`);

  retry(() => {
    const orgIdentifier = getCustomerForPartyUuid(facilitatorPartyUuidRevisor);
    if (orgIdentifier !== clientOrg) {
      throw new Error("Revisor role not yet added back");
    }
  }, { retries: 2, intervalSeconds: 2, name: "add revisor role" });
}