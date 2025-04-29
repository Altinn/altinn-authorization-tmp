import { check, fail, group } from "k6";

import http from 'k6/http';
import { config } from './config.js';
import { getToken } from './token.js';  // if you have a token helper

export default function GetCustomerForPartyUuid() {
  var facilitatorPartyUuid = '368f5a82-97f5-4f33-b372-ac998a4d6b22';
  const token = getToken();

  const url = `${config.baseUrl}/register/api/v1/internal/parties/${facilitatorPartyUuid}/customers/ccr/revisor`;
  console.log(url)

  const res = http.get(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Ocp-Apim-Subscription-Key": config.subscriptionKey,
    },
  });

  console.log(`Response status: ${res.status}`);
  var checkName = 'Register customer list for revisor should respond 200 OK';

    const ok = check(res, { [checkName]: (r) => r.status == 200 });
    if (!ok) {
      fail(`${checkName} (got: ${res.status})`);
    }
  

  // Log the result (optional, for debugging)
  console.log(`Status: ${res.status}`);
  //console.log(`\nResponse: ${res.body}`);

  //Return customer
  //assert that partyuuid is present + org we're looking up
}


