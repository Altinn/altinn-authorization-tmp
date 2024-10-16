import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import { WebSocket } from "ws";
import { readFileSync } from "node:fs";

const argv = yargs(hideBin(process.argv))
  .option("deploy-api", {
    description: "Deploy API base path",
  })
  .option("subscription-id", {
    type: "string",
    required: false,
  })
  .option("resource-group", {
    type: "string",
    required: false,
  })
  .option("server-name", {
    type: "string",
    required: false,
  })
  .option("key-vault-name", {
    type: "string",
    required: false,
  })
  .option("user", {
    type: "string",
    required: true,
  })
  .option("database", {
    type: "string",
    required: true,
  })
  .option("role-prefix", {
    type: "string",
    required: true,
  })
  .option("schema", {
    type: "array",
    required: true,
  })
  .option("tf-outfile", {
    type: "string",
    required: false,
  })
  .parse();

const request = {
  resources: {
    subscriptionId: argv.subscriptionId,
    resourceGroup: argv.resourceGroup,
    serverName: argv.serverName,
    keyVaultName: argv.keyVaultName,
    user: argv.user,
  },
  databaseName: argv.database,
  userPrefix: argv.rolePrefix,
  schemas: Object.fromEntries(argv.schema.map((n) => [n, {}])),
};

if (argv.tfOutfile) {
  const buffer = readFileSync(argv.tfOutfile, "utf8");
  const tfout = JSON.parse(buffer);
  request.resources.subscriptionId ||= tfout.subscription_id.value;
  request.resources.resourceGroup ||= tfout.resource_group_name.value;
  request.resources.serverName ||= tfout.postgres_server_name.value;
  request.resources.keyVaultName ||= tfout.key_vault_name.value;
}

// console.log(request);
// process.exit(1);

const baseUrl = new URL(argv.deployApi);
if (baseUrl.protocol === "http:") {
  baseUrl.protocol = "ws:";
} else if (baseUrl.protocol === "https:") {
  baseUrl.protocol = "wss:";
} else {
  throw new Error(`Unsupported protocol: ${baseUrl.protocol}`);
}

const ws = new WebSocket(
  new URL("deployapi/api/v1/database/bootstrap", baseUrl),
  "altinn.task-pipeline"
);

let pending = Promise.resolve();

const write = (data: Blob) => {
  const buffer = data.arrayBuffer();
  pending = pending
    .then(() => buffer)
    .then((buffer) => {
      var arr = new Uint8Array(buffer);
      process.stderr.write(arr);
    })
    .catch((err) => {
      console.error(err);
      process.exit(1);
    });
};

ws.addEventListener("open", (e) => {
  ws.send(JSON.stringify(request));
});

ws.addEventListener("message", (ev) => {
  write(ev.data);
});

ws.addEventListener("close", (ev) => {
  console.log("closed", ev.code, ev.reason);
  ws.close();

  if (ev.code !== 4000) {
    process.exit(1);
  }
});

ws.addEventListener("error", (ev) => {
  if (ev && "message" in ev) {
    console.error("error", ev.message);
  }
});
