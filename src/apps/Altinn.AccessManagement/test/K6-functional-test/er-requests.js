import http from "k6/http";
import { check, sleep } from "k6";
//import { GetCustomerForPartyUuid } from "./register-test.js";

// Function to send SOAP request with dynamic organisasjonsnummer
function removeRevisorRole(organisasjonsnummer, knytningOrganisasjonsnummer) {
  const soapReqBody = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.altinn.no/services/Register/ER/2013/06">
     <soapenv:Header/>
     <soapenv:Body>
        <ns:SubmitERDataBasic>
           <ns:systemUserName>${__ENV.SOAP_ER_USERNAME}</ns:systemUserName>
           <ns:systemPassword>${__ENV.SOAP_ER_PASSWORD}</ns:systemPassword>
           <ns:ERData><![CDATA[<?xml version="1.0" encoding="UTF-8"?>
<batchAjourholdXML>
  <head avsender="BRG" dato="20170714" kjoerenr="00001" mottaker="ALT" type="A" />
  <enhet organisasjonsnummer="${organisasjonsnummer}" organisasjonsform="AS" hovedsakstype="N" undersakstype="NY" foersteOverfoering="N" datoFoedt="20210315" datoSistEndret="20210315">
    <samendringer felttype="REVI" endringstype="U" type="K" data="D">
      <knytningOrganisasjonsnummer>${knytningOrganisasjonsnummer}</knytningOrganisasjonsnummer>
    </samendringer> 
  </enhet>
  <trai antallEnheter="1" avsender="BRG" />
</batchAjourholdXML>]]></ns:ERData>
        </ns:SubmitERDataBasic>
     </soapenv:Body>
  </soapenv:Envelope>`;

  const res = http.post(
    "https://at22.altinn.cloud/RegisterExternal/RegisterERExternalBasic.svc",
    soapReqBody,
    {
      headers: { "Content-Type": "text/xml" },
    }
  );

  // Check response
  check(res, {
    "status is 200": (r) => r.status === 200,
    "response logged": (r) => {
      console.log(r.body);
      return true;
    },
  });

  sleep(1);
}

function addRevisorRole(organisasjonsnummer, knytningOrganisasjonsnummer) {
  const soapBody = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.altinn.no/services/Register/ER/2013/06">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:SubmitERDataBasic>c
         <ns:systemUserName>${__ENV.SOAP_ER_USERNAME}</ns:systemUserName>
         <ns:systemPassword>${__ENV.SOAP_ER_PASSWORD}</ns:systemPassword>
         <ns:ERData><![CDATA[<?xml version="1.0" encoding="UTF-8"?>
<batchAjourholdXML>
  <head avsender="BRG" dato="20170714" kjoerenr="00001" mottaker="ALT" type="A" />
  <enhet organisasjonsnummer="${organisasjonsnummer}" organisasjonsform="AS" hovedsakstype="N" undersakstype="NY" foersteOverfoering="N" datoFoedt="20210315" datoSistEndret="20210315">
    <samendringer felttype="REVI" endringstype="N" type="K" data="D">
      <knytningOrganisasjonsnummer>${knytningOrganisasjonsnummer}</knytningOrganisasjonsnummer>
    </samendringer>
  </enhet>
  <trai antallEnheter="1" avsender="BRG" />
</batchAjourholdXML>]]></ns:ERData>
      </ns:SubmitERDataBasic>
   </soapenv:Body>
</soapenv:Envelope>`;

  const res = http.post(
    "https://at22.altinn.cloud/RegisterExternal/RegisterERExternalBasic.svc",
    soapBody,
    {
      headers: { "Content-Type": "text/xml" },
    }
  );

  check(res, {
    "status is 200": (r) => r.status === 200,
    "response logged": (r) => {
      console.log(r.body);
      return true;
    },
  });
}

// Run end to end test
export default function removeAndAddRevisorRoleFromOrganization() {
  const orgnr = "213633082";
  const facilitatorPartyUuidRevisor = "368f5a82-97f5-4f33-b372-ac998a4d6b22"; //Todo
  const knytningOrganisasjonsnummer = "314239458";

  //Look up role to make sure it has revisor in Register

  //Wait by running these till you have control
  removeRevisorRole(orgnr, knytningOrganisasjonsnummer);
  //Look up person in Register to make sure role was removed
  addRevisorRole(orgnr, knytningOrganisasjonsnummer);
}
