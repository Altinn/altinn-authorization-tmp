import { sleep, check } from "k6";

/**
 * Retry a function until it succeeds or all retries fail.
 *
 * Uses `check()` to report pass/fail instead of throwing.
 *
 * @param {Function} conditionFn - Function that returns true on success, false otherwise.
 * @param {Object} options - Retry settings.
 * @param {number} options.retries - How many times to retry (default 10).
 * @param {number} options.intervalSeconds - Seconds between attempts (default 5).
 * @param {string} options.testscenario - Prefix used in log/check output.
 * @returns {boolean} - true if success within retry limit, false otherwise.
 */
export function retry(conditionFn, options = {}) {
  const {
    retries = 10,
    intervalSeconds = 5,
    testscenario = "retry check",
  } = options;

  let success = false;

  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      const result = conditionFn();

      if (result) {
        console.log(`${testscenario}] condition met on attempt ${attempt}`);
        success = true;
        break;
      }

      console.log(
        `${testscenario}] Attempt ${attempt}/${retries} — condition not met, retrying...`
      );
    } catch (err) {
      console.warn(`${testscenario}: Error on attempt ${attempt}:`);
    }

    if (attempt < retries) {
      sleep(intervalSeconds);
    }
  }

  check(success, {
    [`${testscenario} succeeded within ${retries} retries`]: (s) => s === true,
  });

  return success;
}
