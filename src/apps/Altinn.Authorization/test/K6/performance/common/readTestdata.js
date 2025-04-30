/**
 * This file contains the implementation of reading test data from CSV files.
 * The test data includes service owners, end users, and end users with tokens.
 * The data is read using the PapaParse library and stored in SharedArray variables.
 * 
 * @module readTestdata
 */

import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { SharedArray } from "k6/data";

/**
 * Function to read the CSV file specified by the filename parameter.
 * @param {} filename 
 * @returns 
 */
function readCsv(filename) {
  try {
    return papaparse.parse(open(filename), { header: true, skipEmptyLines: true }).data;
  } catch (error) {
    console.log(`Error reading CSV file: ${error}`);
    return [];
  } 
}

if (!__ENV.API_ENVIRONMENT) {
  throw new Error('API_ENVIRONMENT must be set');
}
const systemUsersFilename = `../testData/customers.csv`;
const orgOwnersFilename = `../testData/orgsInYt01.csv`;
const daglFilename = `../testData/OrgsDagl.csv`;

/**
 * SharedArray variable that stores the service owners data.
 * The data is parsed from the CSV file specified by the filenameServiceowners variable.
 * 
 * @name systemUsers
 * @type {SharedArray}
 */
export const systemUsers = new SharedArray('systemUsers', function () {
  return readCsv(systemUsersFilename);
});

export const dagl = new SharedArray('dagl', function () {
  return readCsv(daglFilename);
});

export const orgOwners = new SharedArray('orgOwners', function () {
  const csv = readCsv(orgOwnersFilename);
  let orgOwnersDict = new Map();
  for (const row of csv) {
    const orgNo = parseInt(row['OrgNr']);
  
    orgOwnersDict[orgNo] = row;
  }
  return [orgOwnersDict];
} );

