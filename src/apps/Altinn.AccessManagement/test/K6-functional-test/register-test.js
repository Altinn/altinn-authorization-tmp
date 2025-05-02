import { check, fail } from "k6";

import http from 'k6/http';
import { config } from './config.js';
import { getPersonalToken } from './token.js'; 

export default function getCustomerForPartyUuid(facilitatorPartyUuid) {
  const token = getPersonalToken();

  const url = `${config.baseUrl}/register/api/v1/internal/parties/${facilitatorPartyUuid}/customers/ccr/revisor`;
  console.log(url);

  const res = http.get(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Ocp-Apim-Subscription-Key": config.subscriptionKey,
    },
  });

  var checkName = "Register customer list for revisor should respond 200 OK";

  const ok = check(res, { [checkName]: (r) => r.status == 200 });
  if (!ok) {
    fail(`${checkName} (got: ${res.status})`);
  }
  const body = JSON.parse(res.body);
  const organizationIdentifier = body.data[0].organizationIdentifier;
  console.log("Returning this customer org id: " + organizationIdentifier);
  return organizationIdentifier;
}


