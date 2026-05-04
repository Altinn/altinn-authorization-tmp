import { buildOptions } from "../common/commonFunctions.js";
import { getConnections, getUserParty } from "./getConnectionsCommon.js"

const getConnectionsLabel = "Get connections/accesspackages";
const labels = [ getConnectionsLabel ];

export let options = buildOptions(labels);

export default function () {
  const userParty = getUserParty();
  getConnections(userParty, getConnectionsLabel, '/accesspackages');
}
