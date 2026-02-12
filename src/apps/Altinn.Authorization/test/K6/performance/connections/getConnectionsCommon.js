import http from 'k6/http';
import { SharedArray } from "k6/data";
import { getConnectionsUrl, tokenGeneratorEnv } from "../common/config.js";
import { expect, describe, randomItem, URL, getPersonalToken } from "../common/testimports.js";
import { buildOptions, getParams, readCsv } from "../common/commonFunctions.js";

const randomize = (__ENV.RANDOMIZE ?? 'false') === 'true';
const env = __ENV.API_ENVIRONMENT ?? 'yt01';
const partiesFilename = import.meta.resolve(`../testData/orgsIn-${env}-WithPartyUuid.csv`);

const parties = new SharedArray('parties', function () {
  return readCsv(partiesFilename);
});

const getConnectionsLabel = "Get connections";
const labels = [ getConnectionsLabel ];

export let options = buildOptions(labels);
  
function getToken(userId) {
  const tokenOpts = {
      scopes: "altinn:portal/enduser",
      userId: userId,
      env: tokenGeneratorEnv
  }
  const token = getPersonalToken(tokenOpts);
  return token;
}

export function getUserParty () {
  if (randomize) { return randomItem(parties) } 
  else { return parties[__ITER % parties.length] };
}

export default function () {
  const userParty = getUserParty();
  getConnections(userParty, getConnectionsLabel);
}

export function getConnections(userParty, label, accessPackagesPath = '') {
  const token = getToken(userParty.userId);
  const params = getParams(label);
  params.headers.Authorization = "Bearer " + token;
  const url = new URL(getConnectionsUrl + accessPackagesPath);
  url.searchParams.append('party', userParty.orgUuid);
  url.searchParams.append('to', userParty.orgUuid);
  describe('Get connections', () => {
      let r = http.get(url.toString(), params);
      if (r.timings.duration > 2000.0) {
        console.log(__ITER, userParty.orgNo, r.timings.duration);
      }
      expect(r.status, "response status").to.equal(200);
      expect(r, 'response').to.have.validJsonBody();
  });
}

