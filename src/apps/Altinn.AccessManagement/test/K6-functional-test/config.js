const env = __ENV.ENVIRONMENT || "at22"; // default to "at22" if not set

const baseUrls = {
  at22: "https://platform.at22.altinn.cloud",
  tt02: "https://tt02.altinn.no",
};

export const config = {
  env: env,
  baseUrl: baseUrls[env], // Pick based on environment
  subscriptionKey:
    env === "tt02"
      ? __ENV.TT02_REGISTER_SUBSCRIPTION_KEY
      : __ENV.AT22_REGISTER_SUBSCRIPTION_KEY,
  soapUsername: __ENV.SOAP_ER_USERNAME,
  soapPassword: __ENV.SOAP_ER_PASSWORD,
  tokenUsername: __ENV.TOKEN_GENERATOR_USERNAME,
  tokenPassword: __ENV.TOKEN_GENERATOR_PASSWORD,
};
