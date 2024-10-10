import { $ } from "zx";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import { getApp } from "./_meta.mts";

const argv = yargs(hideBin(process.argv))
  .positional("name", {
    type: "string",
    required: true,
  })
  .option("tag", {
    type: "string",
    required: true,
  })
  .parse();

const app = getApp(argv.name);

if (!app.image) {
  throw new Error(`No image config found for ${app.name}`);
}

const imgCfg = app.image;
if (imgCfg.type !== "dotnet") {
  throw new Error(`Unsupported image type (not implemented): ${imgCfg.type}`);
}

const source = imgCfg.source.replaceAll("\\", "/");
$.cwd = app.path;
await $`dotnet publish ${source} --os linux-musl -t:PublishContainer -p:ContainerImageTag=${argv.tag} -bl`.verbose(
  true
);
