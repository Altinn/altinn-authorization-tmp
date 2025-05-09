import { uuidv4, randomIntBetween } from "./common/k6-utils.js";
import { getEnterpriseToken, getPersonalTokenSSN } from './common/token.js';

const subscription_key = __ENV.subscription_key;
const traceCalls = __ENV.TRACE_CALLS == "true" || __ENV.TRACE_CALLS == "1" || __ENV.TRACE_CALLS == "yes" || __ENV.TRACE_CALLS == "YES" || __ENV.TRACE_CALLS == "Yes";

export const pdpAuthorizeLabel = "PDP Authorize";
export const pdpAuthorizeLabelDenyPermit = "PDP Authorize Deny";
const tokenGenLabel = "Token generator";
const labels = [pdpAuthorizeLabel, pdpAuthorizeLabelDenyPermit];

const breakpoint = __ENV.breakpoint;
const stages_duration = (__ENV.stages_duration ?? '1m');
const stages_target = (__ENV.stages_target ?? '5');
const abort_on_fail = (__ENV.abort_on_fail ?? 'true') === 'true';

export function getParams(label) {
    const traceparent = uuidv4();
    const params = {
        headers: {
            traceparent: traceparent,
            Accept: 'application/json',
            'Content-Type': 'application/json',
            'User-Agent': 'systembruker-k6',
        },
        tags: { name: label }
    }

    if (traceCalls) {
        params.tags.traceparent = traceparent;
    }
    return params;
}

export function getAuthorizeParams(label, token) {
    const params = getParams(label);
    params.headers.Authorization = "Bearer " + token;
    params.headers['Ocp-Apim-Subscription-Key'] = subscription_key;
    return params
}

export function getAuthorizeToken(client) {
    const tokenOpts = {
        scopes: "altinn:authorization/authorize.admin",
        orgno: client.facilitatorOrgNo,
    }
    const token = getEnterpriseToken(tokenOpts);
    return token;
}

export function getAuthorizeClientToken(client) {
    const tokenOpts = {
        scopes: "altinn:authorization/authorize.admin",
        ssn: client.SSN,
    }
    const token = getPersonalTokenSSN(tokenOpts);
    return token;
}

export function buildOptions() {
    let options = {
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
    return options;
}

export function getActionLabelAndExpectedResponse() {  
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

export function getActionLabelAndExpectedResponseForDaglDeny(org, client) { 
    const randNumber = randomIntBetween(0, 10);
    if (org == client) {
        if (randNumber % 2 == 0) {
            return ["read", pdpAuthorizeLabel, 'Permit']; 
        }
        else {
            return ["write", pdpAuthorizeLabel, 'Permit'];
        }
    }
    return ["read", pdpAuthorizeLabelDenyPermit, 'NotApplicable'];
}   
