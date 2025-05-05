import http from "k6/http";
import encoding from "k6/encoding";
import { config } from './config.js';

const tokenTimeToLive = 3600; // seconds
const tokenMargin = 10;

const cachedTokens = {};
const cachedTokensIssuedAt = {};

function getCacheKey(scopes, pid) {
  return `personal|${scopes}|${pid}`;
}

export function getPersonalToken() {
  const tokenUsername = config.tokenUsername;
  const tokenPassword = config.tokenPassword;

  const scopes = "altinn:register/partylookup.admin";
  const pid = "22877497392";

  const cacheKey = getCacheKey(scopes, pid);
  const currentTime = Math.floor(Date.now() / 1000);

  if (!cachedTokens[cacheKey] || (currentTime - cachedTokensIssuedAt[cacheKey] >= tokenTimeToLive - tokenMargin)) {
    const url =
      "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken" +
      `?env=at22` +
      `&scopes=${scopes}` +
      `&pid=${pid}` +
      `&authLvl=3` +
      `&ttl=${tokenTimeToLive}`;

    const credentials = `${tokenUsername}:${tokenPassword}`;
    const encodedCredentials = encoding.b64encode(credentials);
    const tokenRequestOptions = {
      headers: {
        Authorization: `Basic ${encodedCredentials}`,
      },
    };

    const response = http.get(url, tokenRequestOptions);

    if (response.status !== 200) {
      console.log(response.body)
      throw new Error(
        `Unable to get Altinn token: ${response.status} ${response.body}`
      );
    }

    cachedTokens[cacheKey] = response.body;
    cachedTokensIssuedAt[cacheKey] = currentTime;
  }

  return cachedTokens[cacheKey];
}