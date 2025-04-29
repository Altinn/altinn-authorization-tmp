import http from 'k6/http';
import { randomItem, randomIntBetween, URL} from './common/k6-utils.js';
import { expect, expectStatusFor } from "./common/testimports.js";
import { describe } from './common/describe.js';
import { getPersonalTokenSSN } from './common/token.js';
import { postAuthorizeUrl } from './common/config.js';
import { dagl } from './common/readTestdata.js';
import { buildRoleAuthorizeBody } from './testData/buildAuthorizeBody.js';
import { getParams } from "./commonFunctions.js";

const subscription_key = __ENV.subscription_key;
const breakpoint = __ENV.breakpoint;
const stages_duration = (__ENV.stages_duration ?? '1m');
const stages_target = (__ENV.stages_target ?? '5');
const abort_on_fail = (__ENV.abort_on_fail ?? 'true') === 'true';

const pdpAuthorizeLabel = "PDP Authorize";
const pdpAuthorizeLabelDenyPermit = "PDP Authorize Deny";
const tokenGenLabel = "Token generator";
const labels = [pdpAuthorizeLabel, pdpAuthorizeLabelDenyPermit];
const regnResource = "ttd-dialogporten-performance-test-02";
const fforResource = "ttd-performance-clientdelegation-ffor";
const revResource = "ttd-performance-clientdelegation-revisor";

export let options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)', 'count'],
    thresholds: {
        checks: ['rate>=1.0'],
        [`http_req_duration{name:${tokenGenLabel}}`]: [],
        [`http_req_failed{name:${tokenGenLabel}}`]: ['rate<=0.0']
    }
};
if (breakpoint) {
    for (var label of labels) {
        options.thresholds[[`http_req_duration{name:${label}}`]] = [{ threshold: "max<5000", abortOnFail: abort_on_fail }];
        options.thresholds[[`http_req_failed{name:${label}}`]] = [{ threshold: 'rate<=0.0', abortOnFail: abort_on_fail }];
    }
    //options.executor = 'ramping-arrival-rate';
    options.stages = [
        { duration: stages_duration, target: stages_target },
    ];
}
else {
    for (var label of labels) {
        options.thresholds[[`http_req_duration{name:${label}}`]] = [];
        options.thresholds[[`http_req_failed{name:${label}}`]] = ['rate<=0.0'];
    }
}

options.thresholds[[`http_req_duration{name:${label}}`]] = [];
options.thresholds[[`http_req_failed{name:${label}}`]] = ['rate<=0.0'];


export default function() {
    const client = randomItem(dagl);
    const resource = regnResource;
    const [action, label, expectedResponse] = getActionLabelAndExpectedResponse(); 
    const token = getAuthorizeToken(client);
    const params = getParams(label);
    params.headers.Authorization = "Bearer " + token;
    params.headers['Ocp-Apim-Subscription-Key'] = subscription_key;
    const body = buildRoleAuthorizeBody(client.SSN, resource, client.OrgNr, action);
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
        ssn: client.SSN,
    }
    const token = getPersonalTokenSSN(tokenOpts);
    return token;
}

function getResource(client) {
    switch (client.role) {
        case "regnskapsforer":
            return regnResources;
        case "forretningsforer":
            return fforResource;
        case "revisor":
            return revResource;
        default:
            return null;
    }
}   