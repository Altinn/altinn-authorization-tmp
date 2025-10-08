const testBaseUrl = "https://platform.at22.altinn.cloud/authorization/";
const stagingBaseUrl = "https://platform.tt02.altinn.no/authorization/";
const yt01BaseUrl = "https://platform.yt01.altinn.cloud/authorization/";
const testAuthenticationBaseUrl = "https://platform.at22.altinn.cloud/authentication/"
const stagingAuthenticationBaseUrl = "https://platform.tt02.altinn.no/authentication/";
const yt01AuthenticationBaseUrl = "https://platform.yt01.altinn.cloud/authentication/";
const testAmBaseUrl = "https://platform.at22.altinn.cloud/accessmanagement/";
const stagingAmBaseUrl = "https://platform.tt02.altinn.no/accessmanagement/";
const yt01AmBaseUrl = "https://platform.yt01.altinn.cloud/accessmanagement/";
const stagingAmUiBaseUrl = "https://am.ui.tt02.altinn.no/accessmanagement/"
const yt01AmUiBaseUrl = "https://am.ui.yt01.altinn.cloud/accessmanagement/"

const authorizeUrl = "api/v1/authorize";
const readSystemsUrl = "api/v1/systemregister"
const systemUsersUrl = "api/v1/systemuser/vendor/bysystem/"
const amDelegationUrl = "api/v1/internal/systemuserclientdelegation"
const amAuthorizedPartiesUrl = "api/v1/resourceowner/authorizedparties";
const amConsentUrl = "api/v1/enterprise/consentrequests/"
const amRightholders = "api/v1/user/rightholders";
const amConsentApprove = "api/v1/bff/consentrequests/";

export const apiVersion = __ENV.API_VERSION ?? 'v1';
export const apiEnvironment = __ENV.API_ENVIRONMENT ?? 'yt01';


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
        consentApproveUrl: {
            test: testAmBaseUrl + amConsentApprove,
            staging: stagingAmBaseUrl + amConsentApprove,
            yt01: yt01AmBaseUrl + amConsentApprove
        },
        rightHoldersUrl: {
            test: testAmBaseUrl + amRightholders,
            staging: stagingAmUiBaseUrl + amRightholders,
            yt01: yt01AmUiBaseUrl + amRightholders
        }
    }
};

if (!urls[apiVersion]) {
    throw new Error(`Invalid API version: ${apiVersion}. Please ensure it's set correctly in your environment variables.`);
}

if (!urls[apiVersion]["authorizeUrl"][apiEnvironment]) {
    throw new Error(`Invalid API environment: ${apiEnvironment}. Please ensure it's set correctly in your environment variables.`);
}

export const postAuthorizeUrl = urls[apiVersion]["authorizeUrl"][apiEnvironment];
export const getSystemsUrl = urls[apiVersion]["readSystemsUrl"][apiEnvironment];
export const getSystemUsersUrl = urls[apiVersion]["systemUsersUrl"][apiEnvironment];
export const getAmDelegationUrl = urls[apiVersion]["amDelegationUrl"][apiEnvironment];
export const getAuthorizedPartiesUrl = urls[apiVersion]["authorizedPartiesUrl"][apiEnvironment];
export const postConsent = urls[apiVersion]["consentUrl"][apiEnvironment];
export const postConsentApprove = urls[apiVersion]["consentApproveUrl"][apiEnvironment];
export const getRightHoldersUrl = urls[apiVersion]["rightHoldersUrl"][apiEnvironment];
export const tokenGeneratorEnv = (() => {
  switch (apiEnvironment) {
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
