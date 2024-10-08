import { verticals } from "./_meta.mts";
import * as actions from "@actions/core";

const type = process.argv[2];

let output = verticals;
if (type) {
  output = verticals.filter((v) => v.type === type);
}

actions.setOutput("verticals", JSON.stringify(output.map((v) => v.name)));
