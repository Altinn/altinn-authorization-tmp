import http from "k6/http";
import { check, fail } from "k6";

// Function to send SOAP request with dynamic organisasjonsnummer
export async function removeRevisorRoleFromEr(clientOrg, facilitatorOrg) {
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

  if (
    !check(res, {
      "status code MUST be 200": (res) => res.status == 200,
    })
  ) {
    fail("status code was *not* 200 with response body:  " + res.body);
  }

  check(res, {
    "response contains status OK_ER_DATA_PROCESSED": (r) =>
      r.body.includes('status="OK_ER_DATA_PROCESSED"'),
  });

  check(res, {
    "response code was 200": (res) => res.status == 200,
  });
}

export async function addRevisorRoleToErForOrg(clientOrg, facilitatorOrg) {
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
  if (
    !check(res, {
      "status code MUST be 200": (res) => res.status == 200,
    })
  ) {
    "status code was *not* 200 with response body:  " + res.body;
  }

  check(res, {
    "response contains status OK_ER_DATA_PROCESSED": (r) =>
      r.body.includes('status="OK_ER_DATA_PROCESSED"'),
  });
}
