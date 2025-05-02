import http from "k6/http";
import { check } from "k6";

// Function to send SOAP request with dynamic organisasjonsnummer
export function removeRevisorRoleFromEr(clientOrg, facilitatorOrg) {
  const soapReqBody = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.altinn.no/services/Register/ER/2013/06">
     <soapenv:Header/>
     <soapenv:Body>
        <ns:SubmitERDataBasic>
           <ns:systemUserName>${__ENV.SOAP_ER_USERNAME}</ns:systemUserName>
           <ns:systemPassword>${__ENV.SOAP_ER_PASSWORD}</ns:systemPassword>
           <ns:ERData><![CDATA[<?xml version="1.0" encoding="UTF-8"?>
<batchAjourholdXML>
  <head avsender="BRG" dato="20170714" kjoerenr="00001" mottaker="ALT" type="A" />
  <enhet organisasjonsnummer="${clientOrg}" organisasjonsform="AS" hovedsakstype="N" undersakstype="NY" foersteOverfoering="N" datoFoedt="20210315" datoSistEndret="20210315">
    <samendringer felttype="REVI" endringstype="U" type="K" data="D">
      <knytningOrganisasjonsnummer>${facilitatorOrg}</knytningOrganisasjonsnummer>
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
      headers: {
        "Content-Type": "text/xml",
        SOAPAction:
          '"http://www.altinn.no/services/Register/ER/2013/06/IRegisterERExternalBasic/SubmitERDataBasic"',
      },
    }
  );

  check(res, {
    "status is 200 for remove revisor": (r) => r.status === 200,
    "response contains status OK_ER_DATA_PROCESSED": (r) =>
      r.body.includes('status="OK_ER_DATA_PROCESSED"'), // fallback if not escaped
    "response contains message 'ER data processed ok'": (r) =>
      r.body.includes("ER data processed ok"),
  });

}

export function addRevisorRoleToErForOrg(clientOrg, facilitatorOrg) {
  const soapBody = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ns="http://www.altinn.no/services/Register/ER/2013/06">
   <soapenv:Header/>
   <soapenv:Body>
      <ns:SubmitERDataBasic>
         <ns:systemUserName>${__ENV.SOAP_ER_USERNAME}</ns:systemUserName>
         <ns:systemPassword>${__ENV.SOAP_ER_PASSWORD}</ns:systemPassword>
         <ns:ERData><![CDATA[<?xml version="1.0" encoding="UTF-8"?>
<batchAjourholdXML>
  <head avsender="BRG" dato="20170714" kjoerenr="00001" mottaker="ALT" type="A" />
  <enhet organisasjonsnummer="${clientOrg}" organisasjonsform="AS" hovedsakstype="N" undersakstype="NY" foersteOverfoering="N" datoFoedt="20210315" datoSistEndret="20210315">
    <samendringer felttype="REVI" endringstype="N" type="K" data="D">
      <knytningOrganisasjonsnummer>${facilitatorOrg}</knytningOrganisasjonsnummer>
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
      headers: {
        "Content-Type": "text/xml",
        SOAPAction:
          '"http://www.altinn.no/services/Register/ER/2013/06/IRegisterERExternalBasic/SubmitERDataBasic"',
      },
    }
  );

  check(res, {
    "status is 200 for add revisor": (r) => r.status === 200,
  });
}
