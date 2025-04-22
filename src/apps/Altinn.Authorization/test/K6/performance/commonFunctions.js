import { uuidv4 } from "./common/k6-utils.js";

const traceCalls = __ENV.TRACE_CALLS == "true" || __ENV.TRACE_CALLS == "1" || __ENV.TRACE_CALLS == "yes" || __ENV.TRACE_CALLS == "YES" || __ENV.TRACE_CALLS == "Yes";

export function getParams(label) {
    const traceparent = uuidv4();
    const params = {
        headers: {
            traceparent: traceparent,
            Accept: 'application/json',
            'Content-Type': 'application/json',
            'User-Agent': 'systembruker-k6',
        },
        tags: { name: label }
    }

    if (traceCalls) {
        params.tags.traceparent = traceparent;
    }
    return params;
}
