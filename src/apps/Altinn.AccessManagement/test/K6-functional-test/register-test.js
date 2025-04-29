import { check, fail, group } from "k6";

import http from 'k6/http';
import { config } from './config.js';
import { getToken } from './token.js';  // if you have a token helper

export default function GetCustomerForPartyUuid(facilitatorPartyUuid) {
  const token = getToken();

  const url = `${config.baseUrl}/register/api/v1/internal/parties/${facilitatorPartyUuid}/customers/ccr/revisor`;
  console.log(url);

  const res = http.get(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Ocp-Apim-Subscription-Key": config.subscriptionKey,
    },
  });

  console.log(`Response status: ${res.status}`);
  var checkName = "Register customer list for revisor should respond 200 OK";

  const ok = check(res, { [checkName]: (r) => r.status == 200 });
  if (!ok) {
    fail(`${checkName} (got: ${res.status})`);
  }
  const body = JSON.parse(res.body);
  // Extract organizationIdentifier
  const organizationIdentifier = body.data[0].organizationIdentifier;
  console.log(organizationIdentifier);
  return organizationIdentifier;
}


