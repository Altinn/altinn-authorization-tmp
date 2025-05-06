import { check } from "k6";

import http from "k6/http";
import { config } from "./../config.js";
import { getPersonalToken } from "./token.js";

export function getRevisorCustomerIdentifiersForParty(facilitatorPartyUuid) {
  const token = getPersonalToken();
  const url = `${config.baseUrl}/register/api/v1/internal/parties/${facilitatorPartyUuid}/customers/ccr/revisor`;

  console.log(url);

  const res = http.get(url, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Ocp-Apim-Subscription-Key": config.subscriptionKey,
    },
  });

  check(res, {
    "Register customer list for revisor should respond 200 OK": (r) =>
      r.status === 200,
  });

  const body = JSON.parse(res.body);
  return (body.data ?? []).map((entry) => entry.organizationIdentifier);
}
