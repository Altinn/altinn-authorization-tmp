import { group, check } from "k6";
import { getRevisorCustomerIdentifiersForParty } from "./helpers/register-test.js";
import {
  removeRevisorRoleFromEr,
  addRevisorRoleToErForOrg,
} from "./helpers/er-requests.js";
import { retry } from "./helpers/helpers.js";
import * as altinnK6Lib from "https://raw.githubusercontent.com/Altinn/altinn-platform/refs/heads/main/libs/k6/build/index.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";

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

export function handleSummary(data) {
  const slackMessage = {
    text: "K6 Test Report for Revisor Role Removal",
    stats: textSummary(data, { indent: "  ", enableColors: true }),
  };

  altinnK6Lib.postSlackMessage(slackMessage);

  return {
    stdout: textSummary(data, { indent: "  " }),
  };
}

///Uncommented this to test slack integaration via function defined below
function test() {
  testRemovalOfRevisorRoleForClient();

  altinnK6Lib.postSlackMessage(data);
}

export default function () {
  check(1 == 1, "Verify Altinn is the best organization");

  var data = "Verify Altinn is the best";

  altinnK6Lib.postSlackMessage(data);
}
