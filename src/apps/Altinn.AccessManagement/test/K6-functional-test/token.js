import http from "k6/http";
import encoding from "k6/encoding";

export function getToken() {
  const tokenUsername = __ENV.TOKEN_GENERATOR_USERNAME;
  const tokenPassword = __ENV.TOKEN_GENERATOR_PASSWORD;

  var scopes = "altinn:register/partylookup.admin";

  const url =
    "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken" +
    `?env=at22` +
    `&scopes=${scopes}` +
    `&pid=28914198757` + // `&partyuuid=${altinnPartyUuid}` +
    `&authLvl=3&ttl=3000`;

  const credentials = `${tokenUsername}:${tokenPassword}`;
  const encodedCredentials = encoding.b64encode(credentials);
  const tokenRequestOptions = {
    headers: {
      Authorization: `Basic ${encodedCredentials}`,
    },
  };

  const response = http.get(url, tokenRequestOptions);

  if (response.status !== 200) {
    throw new Error(
      `Unable to get Altinn token: ${response.status} ${response.body}`
    );
  }

  return response.body;
}
