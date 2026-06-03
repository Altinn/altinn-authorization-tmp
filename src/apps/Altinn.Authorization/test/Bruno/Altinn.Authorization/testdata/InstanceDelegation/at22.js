module.exports = 
{
    "env": "at22",
    "resources": {
        "appInstanceDelegation": {
            "resourceId": "app_ttd_ttd-bruno-tilgangspakke-app",
            "org": "ttd",
            "app":  "ttd-bruno-tilgangspakke-app"
        }
    },
    "instances": {
        "p2p": {
            "instanceId": "7ff72acc-1714-4991-a1d1-a7e89bd5e62d",
            "brukerstyrtInstanceId": "7e6c9a38-13e2-4e5d-84ff-86a2903eb596",
            "from": "5daf2382-adee-4cd6-9995-3c5f3fc058af",
            "to": "bd90e954-efe5-4b65-be8a-f9fccbb36342"
        },
        "main2p": {
            "instanceId": "6240aaf3-5795-4698-b1ac-1a18224eff22",
            "from": "01712d4f-cfb2-4d12-8aac-2eb7ad0d9c86",
            "to": "bd90e954-efe5-4b65-be8a-f9fccbb36342"
        },
        "sub2p": {
            "instanceId": "238dd66a-8357-4da9-969b-5c53f3246f38",
            "from": "e21a640b-9558-4d59-b47f-7b87d2cd9ee0",
            "to": "bd90e954-efe5-4b65-be8a-f9fccbb36342"
        },
        "o2o": {
            "instanceId": "bd91ee77-cfd0-4797-8e3c-553bd8a14ee9",
            "from": "f7e2eb77-6b5e-4aeb-9fe9-f261013401c9",
            "to": "01712d4f-cfb2-4d12-8aac-2eb7ad0d9c86"
        }
    },
    "organizations": {
        "mobilBeskjedenTiger": {
            "partyUuid": "f7e2eb77-6b5e-4aeb-9fe9-f261013401c9",
            "name": "MOBIL BESKJEDEN TIGER AS",
            "organizationNumber": "310705071",
            "partyId": 51373625,
            "subunits": {
                "mobilBeskjedenTiger": {
                    "partyUuid": "2177ee69-fd11-4b38-904a-e0e2f1958f86",
                    "name": "MOBIL BESKJEDEN TIGER AS",
                    "organizationNumber": "314410343",
                    "partyId": 51690212,
                    "dagl": null
                }
            },
            "dagl": {
                "name": "HAKKE DYR",
                "personId": "29917997339",
                "userId": 20012883,
                "partyUuid": "bd90e954-efe5-4b65-be8a-f9fccbb36342",
                "partyId": 50127680
            }
        },
        "legitimJusterbarTiger": {
            "partyUuid": "01712d4f-cfb2-4d12-8aac-2eb7ad0d9c86",
            "name": "LEGITIM JUSTERBAR TIGER AS",
            "organizationNumber": "311041371",
            "partyId": 51403263,
            "subunits": {
                "legitimJusterbarTiger": {
                    "partyUuid": "e21a640b-9558-4d59-b47f-7b87d2cd9ee0",
                    "name": "LEGITIM JUSTERBAR TIGER AS",
                    "organizationNumber": "315684617",
                    "partyId": 51806076,
                    "dagl": null
                }
            },
            "dagl": {
                "name": "ØVELSE EMPIRISK",
                "personId": "25884699358",
                "userId": 20012882,
                "partyUuid": "5daf2382-adee-4cd6-9995-3c5f3fc058af",
                "partyId": 51195601
            }
        }
    },
    "persons": {
        "ovelseEmpirisk": {
            "name": "ØVELSE EMPIRISK",
            "personId": "25884699358",
            "userId": 20012882,
            "partyUuid": "5daf2382-adee-4cd6-9995-3c5f3fc058af",
            "partyId": 51195601
        },
        "hakkeDyr": {
            "name": "HAKKE DYR",
            "personId": "29917997339",
            "userId": 20012883,
            "partyUuid": "bd90e954-efe5-4b65-be8a-f9fccbb36342",
            "partyId": 50127680
        }
    }
}