#!/usr/bin/env bash
#
# Warns (without failing CI) when an AccessManagement test method name violates the
# result-vocabulary of the naming convention (docs/testing/TEST_NAMING_CONVENTION.md):
# an HTTP result segment must be the numeric+named form (Returns200Ok, Returns404NotFound,
# ...), not a bare status word or a bare numeric code, and names must not use opaque IDs.
#
# Deliberately NARROW to avoid false positives. It flags ONLY:
#   * a final segment that is an unambiguous HTTP status word (optionally
#     Return/Returns-prefixed) with no numeric code: ...Ok, ...BadRequest,
#     ...NotFound, ...Forbidden, ...Unauthorized, ...NoContent, ...Created,
#     ...Accepted, ...Conflict, ...InternalServerError;
#   * a final segment "Returns<NNN>" (a numeric code with no name);
#   * opaque IDs: a "_TCxx" segment, a final pure-numeric segment, or a
#     trailing "<word><4 digits>" id.
# It does NOT police domain-outcome results (ReturnsTrue/False/Null/Empty/Problem,
# Success, Valid, ...): those are legitimate and ambiguous, left to review.
#
# Input: $1 = repo root (defaults to CWD). No-ops (exit 0) when the AccessManagement
# test directory is absent, so non-AccessManagement verticals are unaffected.
set -uo pipefail

root="${1:-.}"
testdir="$root/src/apps/Altinn.AccessManagement/test"

if [[ ! -d "$testdir" ]]; then
  echo "No AccessManagement test directory at '$testdir'; test-naming guard skipped."
  exit 0
fi

# Unambiguous HTTP status words that must instead appear in numeric+named form.
http='Ok|OK|BadRequest|NotFound|Forbidden|Unauthorized|NoContent|Created|Accepted|Conflict|InternalServerError'
http_re="^(Returns|Return)?(${http})\$"

mapfile -t files < <(grep -rlE '\[(Fact|Theory)' "$testdir" --include='*.cs')

violations=0
while IFS=: read -r file lineno name; do
  [[ -z "${name:-}" ]] && continue
  last="${name##*_}"
  viol=""
  if [[ "$name" =~ _TC[0-9]+ ]]; then
    viol="opaque test-case id (TCxx): use a descriptive Scenario_Result"
  elif [[ "$last" =~ ^[0-9]{3,}$ ]]; then
    viol="opaque numeric final segment: use a descriptive result"
  elif [[ "$last" =~ ^[A-Za-z]+[0-9]{4}$ ]]; then
    viol="opaque id final segment: use a descriptive result"
  elif [[ "$last" =~ ^Returns[0-9]{3}$ ]]; then
    viol="numeric status without a name: use Returns<code><Name> (e.g. Returns400BadRequest)"
  elif [[ "$last" =~ $http_re ]]; then
    viol="bare HTTP status: use the numeric+named form (e.g. Returns404NotFound)"
  fi
  if [[ -n "$viol" ]]; then
    rel="${file#"$root"/}"
    echo "::warning file=${rel},line=${lineno}::Test name '${name}': ${viol}. See docs/testing/TEST_NAMING_CONVENTION.md."
    violations=$((violations + 1))
  fi
done < <(
  awk '
    FNR == 1 { pending = 0 }
    /^[[:space:]]*\[(Fact|Theory)/ { pending = 1; next }
    pending {
      if (match($0, /[A-Za-z_][A-Za-z0-9_]*[ \t]*\(/)) {
        s = substr($0, RSTART, RLENGTH); sub(/[ \t]*\(.*/, "", s);
        print FILENAME ":" FNR ":" s; pending = 0; next
      }
      if ($0 ~ /[^[:space:]]/ && $0 !~ /^[[:space:]]*\[/) { pending = 0 }
    }
  ' "${files[@]}"
)

if (( violations > 0 )); then
  echo "::warning::${violations} AccessManagement test name(s) do not follow the naming convention (non-blocking). See docs/testing/TEST_NAMING_CONVENTION.md."
fi

# Always succeed: this guard reports as a warning only, it never fails the build.
exit 0
