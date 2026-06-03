module.exports = 
{
    "env": "tt02",
    "resources": {
        "appInstanceDelegation": {
            "resourceId": "app_ttd_ttd-bruno-tilgangspakke-app",
            "org": "ttd",
            "app":  "ttd-bruno-tilgangspakke-app"
        }
    },
    "instances": {
        "p2p": {
            "instanceId": "3f7bfe70-ceb3-45c0-8650-905960f9a9f4",
            "brukerstyrtInstanceId": "cb0ac555-0286-461b-8612-2e53b0442864",
            "from": "f9b2750f-f2aa-442c-91b7-418e2f627a8b",
            "to": "07336d43-1f0e-40a8-a02b-2c6019013cc7"
        },
        "main2p": {
            "instanceId": "49986993-2cec-4e96-85a5-b07c5b7e4c5e",
            "from": "58c0acd2-793e-48f1-a19f-169c171066f4",
            "to": "07336d43-1f0e-40a8-a02b-2c6019013cc7"
        },
        "sub2p": {
            "instanceId": "3a875fb6-4610-4436-b179-df45bdbc9fa5",
            "from": "65018cec-9bd4-45ad-998d-357ac181eaf3",
            "to": "07336d43-1f0e-40a8-a02b-2c6019013cc7"
        },
        "o2o": {
            "instanceId": "632fe488-1ed3-4bff-ba9c-56f1df0839fc",
            "from": "9d263473-4f08-459e-bf27-1de535c40607",
            "to": "58c0acd2-793e-48f1-a19f-169c171066f4"
        }
    },
    "organizations": {
        "mobilBeskjedenTiger": {
            "partyUuid": "9d263473-4f08-459e-bf27-1de535c40607",
            "name": "MOBIL BESKJEDEN TIGER AS",
            "organizationNumber": "310705071",
            "partyId": 51587157,
            "subunits": {
                "mobilBeskjedenTiger": {
                    "partyUuid": "4f849f61-c36f-470a-b0b5-924568e3e10a",
                    "name": "MOBIL BESKJEDEN TIGER AS",
                    "organizationNumber": "314410343",
                    "partyId": 51903584,
                    "dagl": null
                }
            },
            "dagl": {
                "name": "HAKKE DYR",
                "personId": "29917997339",
                "userId": 1424053,
                "partyUuid": "07336d43-1f0e-40a8-a02b-2c6019013cc7",
                "partyId": 51015472
            }
        },
        "legitimJusterbarTiger": {
            "partyUuid": "58c0acd2-793e-48f1-a19f-169c171066f4",
            "name": "LEGITIM JUSTERBAR TIGER AS",
            "organizationNumber": "311041371",
            "partyId": 51616797,
            "subunits": {
                "legitimJusterbarTiger": {
                    "partyUuid": "65018cec-9bd4-45ad-998d-357ac181eaf3",
                    "name": "LEGITIM JUSTERBAR TIGER AS",
                    "organizationNumber": "315684617",
                    "partyId": 52019430,
                    "dagl": null
                }
            },
            "dagl": {
                "name": "ØVELSE EMPIRISK",
                "personId": "25884699358",
                "userId": 1424054,
                "partyUuid": "f9b2750f-f2aa-442c-91b7-418e2f627a8b",
                "partyId": 51443447
            }
        }
    },
    "persons": {
        "ovelseEmpirisk": {
            "name": "ØVELSE EMPIRISK",
            "personId": "25884699358",
            "userId": 1424054,
            "partyUuid": "f9b2750f-f2aa-442c-91b7-418e2f627a8b",
            "partyId": 51443447
        },
        "hakkeDyr": {
            "name": "HAKKE DYR",
            "personId": "29917997339",
            "userId": 1424053,
            "partyUuid": "07336d43-1f0e-40a8-a02b-2c6019013cc7",
            "partyId": 51015472
        }
    }
};