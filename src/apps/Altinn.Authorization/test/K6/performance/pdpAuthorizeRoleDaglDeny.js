import http from 'k6/http';
import exec from 'k6/execution';
import { randomIntBetween, URL} from './common/k6-utils.js';
import { expect, expectStatusFor } from "./common/testimports.js";
import { describe } from './common/describe.js';
import { postAuthorizeUrl } from './common/config.js';
import { dagl } from './common/readTestdata.js';
import { buildOrgAuthorizeBody } from './testData/buildAuthorizeBody.js';
import { buildOptions, getAuthorizeParams, getAuthorizeClientToken, getActionLabelAndExpectedResponseForDaglDeny } from "./commonFunctions.js";

const resource = "ttd-dialogporten-performance-test-02";

export let options = buildOptions();

export default function() {
    const part = exec.vu.idInTest % (dagl.length/50);
    const clientIndex = (randomIntBetween(part*50, ((part+1)*50)) - 1) % dagl.length;
    const orgIndex = (randomIntBetween(part*50, ((part+1)*50)) - 1) % dagl.length;
    const client = dagl[clientIndex];
    const org = dagl[orgIndex];
    const [action, label, expectedResponse] = getActionLabelAndExpectedResponseForDaglDeny(orgIndex, clientIndex); 
    const token = getAuthorizeClientToken(client);
    const params = getAuthorizeParams(label, token);
    const body = buildOrgAuthorizeBody(client.SSN, resource, org.OrgNr, action);
    const url = new URL(postAuthorizeUrl);
    describe('PDP Authorize', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        let response = JSON.parse(r.body);
        expectStatusFor(r).to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
        expect(response.response[0].decision).to.equal(expectedResponse); 
    });   
}

 