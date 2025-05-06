export const config = {
  env: __ENV.ENVIRONMENT,
  baseUrl: __ENV.BASE_URL,
  subscriptionKey: __ENV.REGISTER_SUBSCRIPTION_KEY,
  soapUsername: __ENV.SOAP_ER_USERNAME,
  soapPassword: __ENV.SOAP_ER_PASSWORD,
  tokenUsername: __ENV.TOKEN_GENERATOR_USERNAME,
  tokenPassword: __ENV.TOKEN_GENERATOR_PASSWORD,
};
