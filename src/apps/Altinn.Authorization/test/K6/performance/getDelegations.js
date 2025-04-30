import http from 'k6/http';
import { getSystemsUrl, getSystemUsersUrl, getAmDelegationUrl } from "./common/config.js";
import { expect, expectStatusFor } from "./common/testimports.js";
import { describe } from './common/describe.js';
import { getParams } from "./commonFunctions.js";
import { URL } from "./common/k6-utils.js";
import { getEnterpriseToken, getAmToken } from './common/token.js';
import { randomItem } from './common/k6-utils.js';
import { orgOwners } from './common/readTestdata.js'; 

const getSystemsLabel = "Get systems";
const getSystemUsersLabel = "Get system users";
const getDelegationsLabel = "Get delegations";
const getPartyLabel = "Get party";
const labels = [getSystemsLabel, getSystemUsersLabel, getDelegationsLabel, getPartyLabel];

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'count'],
    thresholds: {
        checks: ['rate>=1.0']
    }
};

for (var label of labels) {
    options.thresholds[[`http_req_duration{name:${label}}`]] = [];
    options.thresholds[[`http_req_failed{name:${label}}`]] = ['rate<=0.0'];
}

export function setup() {
  return getSystemUsers();
}

export default function (data) {
    const systemUser = randomItem(data);
    const facilitatorUuid = getOrgParty(systemUser.reporteeOrgNo)
    const delegations = getDelegations(facilitatorUuid, systemUser.id, systemUser.reporteeOrgNo);
    for (const delegation of delegations) {
        console.log(`${systemUser.id},${delegation.from.id},${systemUser.reporteeOrgNo},${delegation.role.code}`);
    }
}

function getSystems() {
    const params = getParams(getSystemsLabel);
    const url = getSystemsUrl
    let customer_list = null;
    describe('Get systems', () => {
        let r = http.get(url, params);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();  
        customer_list = r.json();      
    });
    if (!customer_list) {
        return null;
    }
    customer_list = customer_list.filter(item => item.accessPackages.length > 0);
    return customer_list;
}

function getSystemUsers() {
    const systems = getSystems()
    if (!systems) {
        return [];
    }
    const systemUsers = [];
    for (const system of systems) {
        const users = getSystemUsersForSystem(system.systemId, system.systemVendorOrgNumber);
        systemUsers.push(...users);
    }
    return systemUsers;
}

function getSystemUsersForSystem(systemId, systemOwner) {
    const params = getParams(getSystemUsersLabel);
    params.headers.Authorization = "Bearer " + getSystemOwnerToken(systemOwner);
    const url = new URL(getSystemUsersUrl + systemId);
    let systemUsers = null;
    describe('Get system users', () => {
        let r = http.get(url.toString(), params);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        systemUsers = r.json();
    });
    return systemUsers.data;
}

function getOrgParty(orgNo) {
    const params = getParams(getPartyLabel);
    params.headers.Authorization = "Bearer " + getPartyToken(orgNo);
    params.headers['Ocp-Apim-Subscription-Key'] = __ENV.subscription_key;
    const body = {"data": ["urn:altinn:organization:identifier-no:" +orgNo]}
    const url = new URL('https://platform.yt01.altinn.cloud/register/api/v1/dialogporten/parties/query');
    let orgUuid = null;
    describe('Get party', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
        const response = r.json();
        orgUuid = response.data[0].partyUuid;
    });
    return orgUuid;
}


function getDelegations(facilitatorUuid, systemUserId, orgno) {
    const params = getParams(getDelegationsLabel);
    const now = Date.now();
    const key = parseInt(orgno);
    const dagl = orgOwners[0][key]; //orgOwners.filter(item => item.OrgNr === orgno)[0];
    params.headers.Authorization = "Bearer " + getAmToken(facilitatorUuid, dagl.UserId);

    const url = new URL(getAmDelegationUrl);
    url.searchParams.append('party', facilitatorUuid);
    url.searchParams.append('systemUser', systemUserId);
    let delegations = [];
    console.log(url.toString());
    describe('Get delegations', () => {
        let r = http.get(url.toString(), params);
        delegations = r.json();
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody();
    });
    return delegations;
}

function getSystemOwnerToken(systemOwner) {
    const tokenOptions = {
        scopes: "altinn:authentication/systemregister.write altinn:authentication/systemuser.request.write altinn:authentication/systemuser.request.read altinn:authorization/authorize",
        orgno: systemOwner
    }
    const token = getEnterpriseToken(tokenOptions);
    return token;   
}

function getPartyToken (systemOwner) {
    const tokenOptions = {
        scopes: "altinn:register/partylookup.admin",
        orgno: systemOwner
    }
    const token = getEnterpriseToken(tokenOptions);
    return token;   
}