#!/usr/bin/env bash
#
# Fails CI when an integration-test assembly builds more WebApplicationFactory
# hosts than its committed baseline — i.e. someone added a per-class test host
# instead of reusing a shared profile / collection fixture. Host build is the
# dominant integration-test setup cost (see docs/testing/TEST_ARCHITECTURE.md).
#
# Inputs:
#   $1  FixtureTiming output file (FIXTURE_TIMING_FILE). One line per test
#       process: "===FIXTURE_TIMING=== assembly=<name> host_build_n=<N> ...".
#   $2  Baseline file: "<assembly> <max>" lines ('#' comments allowed).
#
# No-ops (exit 0) when either file is absent, so verticals without FixtureTiming
# or without a baseline entry are unaffected.
set -uo pipefail

timing="${1:?usage: check-host-build-count.sh <timing-file> <baseline-file>}"
baseline="${2:?usage: check-host-build-count.sh <timing-file> <baseline-file>}"

if [[ ! -f "$timing" ]]; then
  echo "No FixtureTiming output at '$timing'; host-build guard skipped."
  exit 0
fi
if [[ ! -f "$baseline" ]]; then
  echo "No baseline file at '$baseline'; host-build guard skipped."
  exit 0
fi

status=0
while read -r asm max _rest; do
  [[ -z "${asm:-}" || "$asm" == \#* ]] && continue

  # Skip assemblies that did not run in this vertical (no matching timing line).
  if ! grep -q "assembly=$asm " "$timing"; then
    continue
  fi

  count=$(grep -h "assembly=$asm " "$timing" \
    | grep -oE 'host_build_n=[0-9]+' | cut -d= -f2 \
    | awk '{ s += $1 } END { print s + 0 }')

  echo "host builds: $asm = $count (baseline $max)"
  if (( count > max )); then
    echo "::error::Integration host-build count for $asm ($count) exceeds the baseline ($max). A new per-class WebApplicationFactory host was added — reuse a shared profile/collection fixture (see docs/testing/TEST_ARCHITECTURE.md), or bump the baseline in docs/testing/host-build-baseline.txt if the new host is genuinely justified."
    status=1
  fi
done < "$baseline"

exit $status
