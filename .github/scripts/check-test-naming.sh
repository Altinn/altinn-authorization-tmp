#!/usr/bin/env bash
#
# Warns (without failing CI) when an AccessManagement test method name does not follow
# the naming convention (docs/testing/TEST_NAMING_CONVENTION.md). The convention is
# MethodUnderTest_Scenario_ExpectedResult, where the result segment describes the
# observable outcome (what is returned / what changes), not merely a status code.
#
# Deliberately NARROW: it flags only objective anti-patterns, never the subjective
# "is the description rich enough" question (that stays a review concern). It flags:
#   * opaque IDs: a "_TCxx" segment, a trailing pure-numeric segment, or a trailing
#     "<word><4 digits>" id;
#   * a numeric status with no name ("Returns400");
#   * a bare HTTP status word, optionally Return/Returns-prefixed ("BadRequest",
#     "ReturnsOk");
#   * a status-only HTTP result ("Returns200Ok", "Returns400BadRequest") that should
#     instead name the body/effect ("Returns400WithInvalidPartyUrnError"). This last
#     check is a soft NUDGE: skip it when the response genuinely has no body. 204
#     NoContent is exempt, being bodyless by definition.
# It does NOT police service-outcome results (ReturnsTrue/False/Null/Empty, ...).
#
# Input: $1 = repo root (defaults to CWD). No-ops (exit 0) when the AccessManagement
# test directory is absent, so non-AccessManagement verticals are unaffected.
# Reports warnings only; it always exits 0.
set -uo pipefail

root="${1:-.}"
testdir="$root/src/apps/Altinn.AccessManagement/test"

if [[ ! -d "$testdir" ]]; then
  echo "No AccessManagement test directory at '$testdir'; test-naming guard skipped."
  exit 0
fi

# HTTP status words. NoContent is excluded from the status-only nudge below: a
# Returns204NoContent is legitimately bodyless, so it must not be nudged.
http='Ok|OK|BadRequest|NotFound|Forbidden|Unauthorized|NoContent|Created|Accepted|Conflict|InternalServerError|PartialContent'
http_bare_re="^(Returns|Return)?(${http})\$"
status_only_re="^Returns[0-9]{3}(Ok|OK|BadRequest|NotFound|Forbidden|Unauthorized|Created|Accepted|Conflict|InternalServerError|PartialContent)\$"

mapfile -t files < <(grep -rlE '\[(Fact|Theory)' "$testdir" --include='*.cs')

declare -i violations=0
declare -i nudges=0
while IFS=: read -r file lineno name; do
  [[ -z "${name:-}" ]] && continue
  last="${name##*_}"
  viol=""; kind="violation"
  if [[ "$name" =~ _TC[0-9]+ ]]; then
    viol="opaque test-case id (TCxx): use a descriptive Scenario_Result"
  elif [[ "$last" =~ ^[0-9]{3,}$ ]]; then
    viol="opaque numeric final segment: describe the result instead"
  elif [[ "$last" =~ ^[A-Za-z]+[0-9]{4}$ ]]; then
    viol="opaque id final segment: describe the result instead"
  elif [[ "$last" =~ ^Returns[0-9]{3}$ ]]; then
    viol="numeric status with no name: use Returns<code>With<Outcome> (e.g. Returns400WithInvalidPartyUrnError)"
  elif [[ "$last" =~ $http_bare_re ]]; then
    viol="bare HTTP status: use Returns<code>With<Outcome> (e.g. Returns404WithResourceNotFoundProblem)"
  elif [[ "$last" =~ $status_only_re ]]; then
    viol="result names only the status: describe the body or effect, e.g. Returns200WithDelegatedRights (skip if the response has no body)"
    kind="nudge"
  fi
  if [[ -n "$viol" ]]; then
    rel="${file#"$root"/}"
    echo "::warning file=${rel},line=${lineno}::Test '${name}': ${viol}. See docs/testing/TEST_NAMING_CONVENTION.md."
    if [[ "$kind" == "nudge" ]]; then nudges+=1; else violations+=1; fi
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

total=$((violations + nudges))
if (( total > 0 )); then
  echo "::warning::Test naming (non-blocking): ${violations} convention violation(s) and ${nudges} describe-the-result nudge(s) in AccessManagement tests. See docs/testing/TEST_NAMING_CONVENTION.md."
else
  echo "Test naming: all AccessManagement test names follow the convention."
fi

# Always succeed: this guard reports warnings only, it never fails the build.
exit 0
