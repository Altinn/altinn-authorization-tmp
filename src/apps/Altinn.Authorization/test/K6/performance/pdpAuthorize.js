import http from 'k6/http';
import { randomItem, randomIntBetween, URL} from './common/k6-utils.js';
import { expect, expectStatusFor } from "./common/testimports.js";
import { describe } from './common/describe.js';
import { getEnterpriseToken } from './common/token.js';
import { postAuthorizeUrl } from './common/config.js';
import { systemUsers } from './common/readTestdata.js';
import { buildAuthorizeBody } from './testData/buildAuthorizeBody.js';
import { getParams } from "./commonFunctions.js";

const subscription_key = __ENV.subscription_key;

const pdpAuthorizeLabel = "PDP Authorize";
const pdpAuthorizeLabelDenyPermit = "PDP Authorize Deny";
const labels = [pdpAuthorizeLabel, pdpAuthorizeLabelDenyPermit];
const resources = [
    "ttd-performance-clientdelegation"
]

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

export default function() {
    const client = randomItem(systemUsers);
    const resource = randomItem(resources);
    const [action, label, expectedResponse] = getActionLabelAndExpectedResponse(); 
    const token = getAuthorizeToken(client);
    const params = getParams(label);
    params.headers.Authorization = "Bearer " + token;
    params.headers['Ocp-Apim-Subscription-Key'] = subscription_key;
    const body = buildAuthorizeBody(client.systemUserId, resource, client.customerOrgNo, action);
    const url = new URL(postAuthorizeUrl);
    describe('PDP Authorize', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        let response = JSON.parse(r.body);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
        expect(response.response[0].decision).to.equal(expectedResponse); 
    });   
}

function getActionLabelAndExpectedResponse() {  
    const randNumber = randomIntBetween(0, 10);
    switch (randNumber) {
        case 0:
            return ["sign", pdpAuthorizeLabelDenyPermit, 'NotApplicable']; 
        case 1,3,5,7,9:
            return ["read", pdpAuthorizeLabel, 'Permit'];
        default:
            return ["write", pdpAuthorizeLabel, 'Permit'];
    }
}

function getAuthorizeToken(client) {
    const tokenOpts = {
        scopes: "altinn:authorization/authorize.admin",
        orgno: client.facilitatorOrgNo,
    }
    const token = getEnterpriseToken(tokenOpts);
    return token;
}