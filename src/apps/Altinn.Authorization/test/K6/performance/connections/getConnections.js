import { buildOptions } from "../common/commonFunctions.js";
import { getConnections, getUserParty } from "./getConnectionsCommon.js"

const getConnectionsLabel = "Get connections";
const labels = [ getConnectionsLabel ];

export let options = buildOptions(labels);
  
export default function () {
  const userParty = getUserParty();
  getConnections(userParty, getConnectionsLabel);
}


