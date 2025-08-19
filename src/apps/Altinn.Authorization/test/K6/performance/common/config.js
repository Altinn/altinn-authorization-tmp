const testBaseUrl = "https://platform.at22.altinn.cloud/authorization/";
const stagingBaseUrl = "https://platform.tt02.altinn.no/authorization/";
const yt01BaseUrl = "https://platform.yt01.altinn.cloud/authorization/";
const testAuthenticationBaseUrl = "https://platform.at22.altinn.cloud/authentication/"
const stagingAuthenticationBaseUrl = "https://platform.tt02.altinn.no/authentication/";
const yt01AuthenticationBaseUrl = "https://platform.yt01.altinn.cloud/authentication/";
const testAmBaseUrl = "https://platform.at22.altinn.cloud/accessmanagement/";
const stagingAmBaseUrl = "https://platform.tt02.altinn.no/accessmanagement/";
const yt01AmBaseUrl = "https://platform.yt01.altinn.cloud/accessmanagement/";

const authorizeUrl = "api/v1/authorize";
const readSystemsUrl = "api/v1/systemregister"
const systemUsersUrl = "api/v1/systemuser/vendor/bysystem/"
const amDelegationUrl = "api/v1/internal/systemuserclientdelegation"
const amAuthorizedPartiesUrl = "api/v1/resourceowner/authorizedparties";
const amConsentUrl = "api/v1/enterprise/consentrequests/"
const amConsentRequest = "api/v1/consent/request/"; // Example:

//https://am.ui.at22.altinn.cloud/accessmanagement/api/v1/consent/request/a005d4e7-78b3-42b2-ce79-dc68cc5348ec/approve

export const urls = {
    v1: {
        authorizeUrl: {
            test: testBaseUrl + authorizeUrl,
            staging: stagingBaseUrl + authorizeUrl,
            yt01: yt01BaseUrl + authorizeUrl
        },
        readSystemsUrl: {
            test: testAuthenticationBaseUrl + readSystemsUrl,
            staging: stagingAuthenticationBaseUrl + readSystemsUrl,
            yt01: yt01AuthenticationBaseUrl + readSystemsUrl
        },
        systemUsersUrl: {
            test: testAuthenticationBaseUrl + systemUsersUrl,
            staging: stagingAuthenticationBaseUrl + systemUsersUrl,
            yt01: yt01AuthenticationBaseUrl + systemUsersUrl
        },
        amDelegationUrl: {
            test: testAmBaseUrl + amDelegationUrl,
            staging: stagingAmBaseUrl + amDelegationUrl,
            yt01: yt01AmBaseUrl + amDelegationUrl
        },
        authorizedPartiesUrl: {
            test: testAmBaseUrl + amAuthorizedPartiesUrl,
            staging: stagingAmBaseUrl + amAuthorizedPartiesUrl,
            yt01: yt01AmBaseUrl + amAuthorizedPartiesUrl
        },
        consentUrl: {
            test: testAmBaseUrl + amConsentUrl,
            staging: stagingAmBaseUrl + amConsentUrl,
            yt01: yt01AmBaseUrl + amConsentUrl
        },
        consentRequestUrl: {
            test: testAmBaseUrl + amConsentRequest,
            yt01: yt01AmBaseUrl + amConsentRequest
        }
    }
};

if (!urls[__ENV.API_VERSION]) {
    throw new Error(`Invalid API version: ${__ENV.API_VERSION}. Please ensure it's set correctly in your environment variables.`);
}

if (!urls[__ENV.API_VERSION]["authorizeUrl"][__ENV.API_ENVIRONMENT]) {
    throw new Error(`Invalid API environment: ${__ENV.API_ENVIRONMENT}. Please ensure it's set correctly in your environment variables.`);
}

export const postAuthorizeUrl = urls[__ENV.API_VERSION]["authorizeUrl"][__ENV.API_ENVIRONMENT];
export const getSystemsUrl = urls[__ENV.API_VERSION]["readSystemsUrl"][__ENV.API_ENVIRONMENT];
export const getSystemUsersUrl = urls[__ENV.API_VERSION]["systemUsersUrl"][__ENV.API_ENVIRONMENT];
export const getAmDelegationUrl = urls[__ENV.API_VERSION]["amDelegationUrl"][__ENV.API_ENVIRONMENT];
export const getAuthorizedPartiesUrl = urls[__ENV.API_VERSION]["authorizedPartiesUrl"][__ENV.API_ENVIRONMENT];
export const postConsent = urls[__ENV.API_VERSION]["consentUrl"][__ENV.API_ENVIRONMENT];
export const postConsentRequest = urls[__ENV.API_VERSION]["consentRequestUrl"][__ENV.API_ENVIRONMENT];
export const tokenGeneratorEnv = (() => {
  switch (__ENV.API_ENVIRONMENT) {
    case 'yt01':
      return 'yt01';
    case 'staging':
      return 'tt02';
    case 'test':
      return 'at22';
    default:
      return 'yt01';
  }
})();
