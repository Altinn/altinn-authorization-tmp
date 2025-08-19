import http from 'k6/http';
import { SharedArray } from "k6/data";
import { postConsent, postConsentRequest } from './common/config.js';
import { expect, describe, randomItem, randomIntBetween, URL, uuidv4 } from "./common/testimports.js";
import { buildOptions, readCsv, getConsentToken, getApproveToken, getAuthorizeParams } from './commonFunctions.js';

const orgsDaglFilename = `./testData/OrgsDaglUserId.csv`;
const orgsDagl = new SharedArray('orgsDagl', function () {
  return readCsv(orgsDaglFilename);
});

const handledByOrg = "713431400"; // Altinn

const requestlabel = "consentrequests";
const approvelabel = "consentapprove";
const labels = [requestlabel, approvelabel];

export let options = buildOptions(labels);

export default function() {
    const from = randomItem(orgsDagl);
    let to;
    do {
        to = randomItem(orgsDagl);
    } while (to === from);
    const id = requestConsent(from, to);
    console.log(id);
    //approveConsent(from, id);
    
}

function requestConsent(from, to) {
    const resource = "samtykke-performance-test";
    const token = getConsentToken(to.OrgNr);
    //const token = getConsentToken(handledByOrg);
    const params = getAuthorizeParams(requestlabel, token);
    const fromOrg = `urn:altinn:organization:identifier-no:${from.OrgNr}`;
    const toOrg = `urn:altinn:organization:identifier-no:${to.OrgNr}`;
    //const toPerson = `urn:altinn:person:identifier-no:${to.SSN}`;
    const fromPerson = `urn:altinn:person:identifier-no:${from.SSN}`;
    const handledBy = `urn:altinn:organization:identifier-no:${handledByOrg}`;
    let fromSome = fromOrg;
    if (randomIntBetween(0, 1) === 0) {
        fromSome = fromPerson;
    }
    const body = getBody(fromSome, toOrg, null, null, resource, "consent");
    const url = new URL(postConsent);
    describe('POST Consent', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        expect(r.status, "response status").to.equal(201);
        expect(r, 'response').to.have.validJsonBody(); 
    });
    return body.id;   
}

function approveConsent(from, id) {
    const token = getApproveToken(from.UserId);
    const params = getAuthorizeParams(approvelabel, token);
    const body = { "language": "nb" };
    console.log(`Approving consent request with id: ${id} for org: ${from.OrgNr}`);
    const url = new URL(`${postConsentRequest}${id}/approve`);
    console.log(url.toString());
    describe('POST Consent Approve', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        console.log(`Response: ${JSON.stringify(r.json(), null, 2)}`);
        expect(r.status, "response status").to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
    });
}

function getBody(from, to, delegator, handledBy, resource, action) {
    const id = uuidv4();
    const body = {
        "id": id,
        "from": from, 
        "to": to,
        "validTo": new Date(Date.now() + 7*24*60*60*1000).toISOString(),
        "consentRights": [
            {
                "action": [action],
                "resource": [
                    {
                        "type": "urn:altinn:resource",
                        "value": resource
                    } //Vil alltid være bare en
                ],
                "metaData": {
                  "inntektsaar" : "2026"
                }
            }
        ],
        "redirectUrl": "https://altinn.no"
    }
    if (delegator) {
        body.requiredDelegator = delegator
    }
    if (handledBy) {
        body.handledBy = handledBy;
    }
    return body;
}