import http from 'k6/http';
import { expect, describe, URL, getEnterpriseToken } from "./common/testimports.js";
import { postAuthorizeUrl, tokenGeneratorEnv } from './common/config.js';
import { buildOptions, getAuthorizeParams, pdpAuthorizeLabel } from "./commonFunctions.js";


const body_tt02 = 
{
  "Request": {
    "ReturnPolicyIdList": false,
    "AccessSubject": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:altinn:systemuser:uuid",
            "Value": "174bcebf-0834-4803-add0-b3a0958a5b3a",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ],
    "Action": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
            "Value": "write",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ],
    "Resource": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:altinn:task",
            "Value": "Utfylling",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:instance-id",
            "Value": "51581575/70452cd7-9522-4c98-a054-099099252aab",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:partyid",
            "Value": "51581575",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:org",
            "Value": "brg",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:resource:instance-id",
            "Value": "70452cd7-9522-4c98-a054-099099252aab",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ]
  }
}

const body_at22 = {
  "Request": {
    "ReturnPolicyIdList": false,
    "AccessSubject": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:altinn:systemuser:uuid",
            "Value": "9075629d-b117-4cad-b0ca-2c602a6e81bb",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ],
    "Action": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
            "Value": "read",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ],
    "Resource": [
      {
        "Attribute": [
          {
            "AttributeId": "urn:altinn:org",
            "Value": "ttd",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:app",
            "Value": "endring-av-navn-v2",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          },
          {
            "AttributeId": "urn:altinn:partyid",
            "Value": "51428801",
            "DataType": "http://www.w3.org/2001/XMLSchema#string"
          }
        ]
      }
    ]
  }
}

const body = (() => {
  switch (__ENV.API_ENVIRONMENT) {
    case 'staging':
      return body_tt02;
    case 'test':
      return body_at22;
    default:
      throw new Error(`Unknown API environment: ${__ENV.API_ENVIRONMENT}`);
  }
})();
  
export let options = buildOptions();

export default function() {
    const token = getAuthorizeToken();
    const params = getAuthorizeParams(pdpAuthorizeLabel, token);
    const url = new URL(postAuthorizeUrl);
    describe('PDP Authorize', () => {
        let r = http.post(url.toString(), JSON.stringify(body), params);
        let response = JSON.parse(r.body);
        console.log(response.response[0].decision, r.timings.duration);
        expect(r.status, "response status").to.equal(200);
        expect(r, 'response').to.have.validJsonBody(); 
        expect(response.response[0].decision).to.equal('Permit'); 
    });   
}

function getAuthorizeToken() {
  const tokenOpts = {
      scopes: "altinn:authorization/authorize.admin",
      orgNo: 314258916,
  }
  const token = getEnterpriseToken(tokenOpts, tokenGeneratorEnv);
  return token;
}
