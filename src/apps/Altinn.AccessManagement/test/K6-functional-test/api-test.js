import http from "k6/http";
import { sleep } from "k6";
import { check, fail } from "k6";

function doesThisRunInParallell() {
  const url =
    "https://platform.at22.altinn.cloud/authentication/api/v1/systemregister";

  const res = http.get(url);
  // Define your check message
  if (
    !check(res, {
      "status code MUST be 200": (res) => res.status == 200,
    })
  ) {
    fail("status code was *not* 200");
  }
}

export default function () {
  doesThisRunInParallell();
}
