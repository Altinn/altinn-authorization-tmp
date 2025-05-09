import http from 'k6/http';
import exec from 'k6/execution';
import { randomIntBetween, URL} from './common/k6-utils.js';
import { expect, expectStatusFor } from "./common/testimports.js";
import { describe } from './common/describe.js';
import { postAuthorizeUrl } from './common/config.js';
import { dagl } from './common/readTestdata.js';
import { buildDaglAuthorizeBody } from './testData/buildAuthorizeBody.js';
import { buildOptions, getAuthorizeParams, getActionLabelAndExpectedResponse, getAuthorizeClientToken} from "./commonFunctions.js";

// resource with read/write for PRIV and DAGL
const resource = "ttd-dialogporten-performance-test-02";
const noOfClientsPerVu = 50;

export let options = buildOptions();

export default function() {
    // Use a limited number of clients pr VU, to avoid calling getToken for each authorize-request
    const part = exec.vu.idInTest % (dagl.length/noOfClientsPerVu);

    // Pick random client from the list of clients
    const clientIndex = (randomIntBetween(part*noOfClientsPerVu, ((part+1)*noOfClientsPerVu)) - 1) % dagl.length;
    const client = dagl[clientIndex];

    // Get action, label and expected response
    const [action, label, expectedResponse] = getActionLabelAndExpectedResponse(); 

    // Get params, token, request body and url
    const params = getAuthorizeParams(label, getAuthorizeClientToken(client));
    const body = buildDaglAuthorizeBody(client.SSN, resource, client.OrgNr, action);
    const url = new URL(postAuthorizeUrl);

    // Run request and check response
    describe('PDP Authorize', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        let response = JSON.parse(r.body);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
        expect(response.response[0].decision).to.equal(expectedResponse); 
    });   
}