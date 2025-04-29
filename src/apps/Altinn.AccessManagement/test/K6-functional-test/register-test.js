import http from "k6/http";
import { getToken } from "./token.js";
import { check, fail, group } from "k6";

const env = __ENV.ENVIRONMENT || "at22";

const subscription_key =
  env === "tt02"
    ? __ENV.TT02_REGISTER_SUBSCRIPTION_KEY
    : __ENV.AT22_REGISTER_SUBSCRIPTION_KEY;

const baseUrl = env === "tt02" ? __ENV.BASE_URL_TT02 : __ENV.BASE_URL_AT;

export default function GetCustomerForPartyUuid(facilitatorPartyUuid) {
  var token = getToken();

  const url = `${baseUrl}/register/api/v1/internal/parties/${facilitatorPartyUuid}/customers/ccr/revisor`;
  console.log(url);
  const res = http.get(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Ocp-Apim-Subscription-Key": subscription_key,
    },
  });

  group("Get customers from Register", function () {
    const checkName = "status code MUST be 200";

    const ok = check(res, { [checkName]: (r) => r.status == 200 });
    if (!ok) {
      fail(`${checkName} (got: ${res.status})`);
    }
  });

  // Log the result (optional, for debugging)
  console.log(`Status: ${res.status}\nResponse: ${res.body}`);

  //Return customer
  //assert that partyuuid is present + org we're looking up
}

// export default { GetCustomerList, RunRegisterTest };
