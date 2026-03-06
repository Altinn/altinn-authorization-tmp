module.exports = 
{
    "scopes": {
        "read": "altinn:instances:read",
        "k6Read": "test:am/k6.read"
    },
    "auth_scopes": {
        "portalEnduser": "altinn:portal/enduser",
        "read": "altinn:instances:read",
        "maskinportenDelegations": "altinn:maskinporten/delegations",
        "maskinportenAdmin": "altinn:maskinporten/delegations.admin",
        "authorizedParties": "altinn:accessmanagement/authorizedparties",
        "authorizedPartiesResourceOwner": "altinn:accessmanagement/authorizedparties.resourceowner",
        "authorizedPartiesAdmin": "altinn:accessmanagement/authorizedparties.admin",
        "enduserSystemToOthers.read": "altinn:accessmanagement/enduser:connections:toothers.read",
        "enduserSystemToOthers.write": "altinn:accessmanagement/enduser:connections:toothers.write",
        "enduserSystemFromOthers.read": "altinn:accessmanagement/enduser:connections:fromothers.read",
        "enduserSystemFromOthers.write": "altinn:accessmanagement/enduser:connections:fromothers.write",
        "clientdelegations.read": "altinn:clientdelegations.read",
        "clientdelegations.write": "altinn:clientdelegations.write",
        "myclients.read": "altinn:clientdelegations/myclients.read",
        "myclients.write": "altinn:clientdelegations/myclients.write"
    },
    "authTokenType": {
        "personal": "Personal",
        "enterprise": "Enterprise",
        "enterpriseUser": "EnterpriseUser",
        "platformAccess": "PlatformAccess",
        "platformToken": "PlatformToken",
        "systemUser": "SystemUser",
        "selfRegisteredEmailUser": "SelfRegisteredEmailUser"
    },
    "serviceOwners": {
        "ttd": {
            "org": "ttd",
            "orgno": "991825827"
        },
        "digdir": {
            "org": "digdir",
            "orgno": "991825827"
        }
    },
    "expectedDaglAccessPackageCount": 126
};
