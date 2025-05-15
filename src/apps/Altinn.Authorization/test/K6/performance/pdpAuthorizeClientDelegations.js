import http from 'k6/http';
import { expect, expectStatusFor, describe, randomItem, URL } from "./common/testimports.js";
import { postAuthorizeUrl } from './common/config.js';
import { systemUsers } from './common/readTestdata.js';
import { buildClientDelegationAuthorizeBody } from './testData/buildAuthorizeBody.js';
import { buildOptions, getAuthorizeParams, getActionLabelAndExpectedResponse, getAuthorizeToken } from "./commonFunctions.js";

const regnResources = "ttd-performance-clientdelegation";
const fforResource = "ttd-performance-clientdelegation-ffor";
const revResource = "ttd-performance-clientdelegation-revisor";

export let options = buildOptions();

export default function() {
    const client = randomItem(systemUsers);
    const resource = getResource(client);
    if (!resource) {
        console.log(`No resource for ${client.role}`);
        return;
    }
    const [action, label, expectedResponse] = getActionLabelAndExpectedResponse(); 
    const token = getAuthorizeToken(client);
    const params = getAuthorizeParams(label, token);
    const body = buildClientDelegationAuthorizeBody(client.systemUserId, resource, client.customerOrgNo, action);
    const url = new URL(postAuthorizeUrl);
    describe('PDP Authorize', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        let response = JSON.parse(r.body);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
        expect(response.response[0].decision).to.equal(expectedResponse); 
    });   
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