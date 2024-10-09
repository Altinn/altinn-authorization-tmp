import { verticals } from "./_meta.mts";
import * as actions from "@actions/core";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";

const argv = yargs(hideBin(process.argv))
  .option("type", {
    type: "array",
    required: true,
  })
  .parse();

let output = verticals;
if (argv.type) {
  output = verticals.filter((v) => argv.type.includes(v.type));
}

var matrix = {
  path: output.map((v) => v.relPath),
  include: output.map((v) => ({
    vertical: v.relPath,
    name: v.name,
    type: v.type,
  })),
};

actions.setOutput("matrix", JSON.stringify(matrix));
actions.setOutput("verticals", JSON.stringify(matrix.path));
