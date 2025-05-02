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
 * @param {string} options.name - Check label prefix for reporting (default: "retry check").
 * @returns {*} - Result of `fn()` on success, or null if all retries fail.
 */
export function retry(fn, { retries = 5, intervalSeconds = 2, name = "retry check" } = {}) {
  let lastError = null;

  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      const result = fn();
      check(null, {
        [`${name} succeeded at attempt ${attempt}`]: () => true,
      });
      return result;
    } catch (err) {
      lastError = err;
      console.warn(`Retry attempt ${attempt} failed: ${err.message}`);

      if (attempt < retries) {
        sleep(intervalSeconds);
      }
    }
  }

  check(null, {
    [`${name} failed after ${retries} attempts: ${lastError?.message}`]: () => false,
  });

  return null;
}