export function buildAuthorizeBody(systemUserId, resourceId, customerOrgNo, action) {
    let body = {
        "Request": {
            "ReturnPolicyIdList": false,
            "AccessSubject": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:systemuser:uuid",
                            "Value": systemUserId
                        }
                    ]
                }
            ],
            "Action": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
                            "Value": action,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ],
            "Resource": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:resource",
                            "Value": resourceId
                        },
                        {
                            "AttributeId": "urn:altinn:party:uuid",
                            "Value": customerOrgNo,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ]
        }
    }
    return body;

}

export function buildOrgAuthorizeBody(ssn, resourceId, orgno, action) {
    let body = {
        "Request": {
            "ReturnPolicyIdList": false,
            "AccessSubject": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:person:identifier-no",
                            "Value": ssn
                        }
                    ]
                }
            ],
            "Action": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
                            "Value": action,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ],
            "Resource": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:resource",
                            "Value": resourceId
                        },
                        {
                            "AttributeId": "urn:altinn:organization:identifier-no",
                            "Value": orgno,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ]
        }
    }
    return body;

}

export function buildPrivAuthorizeBody(ssn, resourceId, action) {
    let body = {
        "Request": {
            "ReturnPolicyIdList": false,
            "AccessSubject": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:person:identifier-no",
                            "Value": ssn
                        }
                    ]
                }
            ],
            "Action": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:oasis:names:tc:xacml:1.0:action:action-id",
                            "Value": action,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ],
            "Resource": [
                {
                    "Attribute": [
                        {
                            "AttributeId": "urn:altinn:resource",
                            "Value": resourceId
                        },
                        {
                            "AttributeId": "urn:altinn:person:identifier-no",
                            "Value": ssn,
                            "DataType": "http://www.w3.org/2001/XMLSchema#string"
                        }
                    ]
                }
            ]
        }
    }
    return body;

}
