import { sleep } from "k6";

/**
 * Retry a function until it succeeds or times out.
 *
 * @param {Function} fn - Function that should succeed (throw if not ready).
 * @param {Object} options - Retry settings.
 * @param {number} options.retries - How many times to retry (default 5).
 * @param {number} options.intervalSeconds - How long to wait between retries (default 2s).
 * @returns {*} - The return value of fn if it succeeds.
 * @throws {Error} - If all retries fail.
 */
export async function retry(fn, { retries = 5, intervalSeconds = 2 } = {}) {
  for (let attempt = 1; attempt <= retries; attempt++) {
    try {
      const result = fn();
      return result; // ✅ success
    } catch (err) {
      console.warn(`Retry attempt ${attempt} failed: ${err.message}`);
      if (attempt === retries) {
        throw new Error(`All ${retries} retries failed: ${err.message}`);
      }
      sleep(intervalSeconds);
    }
  }
}
