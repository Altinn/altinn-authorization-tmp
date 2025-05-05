import { sleep, check } from "k6";

/**
 * Retry a function until it succeeds or all retries fail.
 *
 * Uses `check()` to report pass/fail instead of throwing.
 *
 * @param {Function} fn - Function that throws an Error if the condition is not met.
 * @param {Object} options - Retry settings.
 * @param {number} options.retries - How many times to retry (default 5).
 * @param {number} options.intervalSeconds - Seconds between attempts (default 2).
 * @param {string} options.testscenario - Check label prefix for reporting (default: "retry check").
 * @returns {*} - Result of `fn()` on success, or null if all retries fail.
 */
export async function retry(conditionFn, options = {}) {
  const {
    retries = 10,
    intervalSeconds = 5,
    testscenario = "unnamed scenario",
  } = options;

  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      const result = await conditionFn();

      if (result) {
        console.log(`${testscenario}] Condition met on attempt ${attempt}`);
        return true;
      }

      console.log(`${testscenario}] Attempt ${attempt}/${retries} — condition not met, retrying...`);
    } catch (err) {
      const msg = err?.message ?? JSON.stringify(err) ?? "Unknown error";
      console.warn(`${testscenario}] Error on attempt ${attempt}: ${msg}`);
    }

    if (attempt < retries) {
      await new Promise((resolve) => setTimeout(resolve, intervalSeconds * 1000));
    }
  }

  console.error(`${testscenario}] Condition not met after ${retries} attempts.`);
  return false;
}