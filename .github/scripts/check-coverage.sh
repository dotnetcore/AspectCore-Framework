#!/bin/bash
# Coverage check script for CI.
# Runs either the unit-test or E2E-test coverage gate selected by its first argument.

set -euo pipefail

# Allow running on newer .NET runtimes.
export DOTNET_ROLL_FORWARD=Major

readonly UNIT_TEST_MODE="unit"
readonly E2E_TEST_MODE="e2e"
readonly UT_THRESHOLD=95
readonly E2E_THRESHOLD=80

usage() {
  echo "Usage: $0 {${UNIT_TEST_MODE}|${E2E_TEST_MODE}}" >&2
  exit 2
}

run_coverage() {
  local project="$1"
  local label="$2"
  local project_dir
  local project_name
  local source_assembly
  local include_filter
  local coverage_file
  local line_rate
  local coverage_pct

  echo "Measuring: $label..." >&2

  project_dir=$(dirname "$project")
  rm -rf "$project_dir/TestResults"

  # Derive the source assembly name from the test project name.
  # e.g. AspectCore.Extensions.Windsor.Test -> [AspectCore.Extensions.Windsor]*
  project_name=$(basename "$project_dir")
  source_assembly="${project_name%.Test}"
  source_assembly="${source_assembly%.Tests}"

  # E2E coverage intentionally includes only Core and Abstractions. Extension
  # assemblies have dedicated unit-test projects that measure them separately.
  include_filter="[${source_assembly}]*"
  if [[ "$project_name" == *"E2E"* ]]; then
    include_filter="[AspectCore.Core]*%2c[AspectCore.Abstractions]*"
  fi

  # Do not use --no-build: coverlet.msbuild needs to instrument assemblies.
  dotnet test "$project" --configuration Release -f net9.0 \
    /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./TestResults/ \
    "/p:Include=${include_filter}" \
    2>&1 | tail -1 >&2

  coverage_file=$(find "$project_dir/TestResults" -name "*.cobertura.xml" -print -quit)
  if [[ -n "$coverage_file" && -f "$coverage_file" ]]; then
    line_rate=$(grep -o 'line-rate="[^"]*"' "$coverage_file" | head -1 | cut -d'"' -f2)
    if [[ -n "$line_rate" ]]; then
      coverage_pct=$(echo "$line_rate * 100" | bc | cut -d. -f1)
      echo "  Coverage: ${coverage_pct}%" >&2
      printf '%s\n' "$coverage_pct"
      return
    fi
  fi

  echo "  No coverage data" >&2
  printf '0\n'
}

run_coverage_gate() {
  local mode="$1"
  local heading="$2"
  local threshold="$3"
  local project_pattern="$4"
  local exclude_pattern="${5:-}"
  local coverage
  local coverage_count=0
  local coverage_sum=0
  local coverage_average=0
  local find_arguments=(./tests -name "$project_pattern")
  local test_project

  if [[ -n "$exclude_pattern" ]]; then
    find_arguments+=( ! -name "$exclude_pattern" )
  fi

  echo "=== ${heading} Coverage ==="
  echo "Threshold: ${threshold}%"
  echo ""

  while IFS= read -r test_project; do
    coverage=$(run_coverage "$test_project" "$(basename "$(dirname "$test_project")")")
    if [[ "$coverage" == "0" || -z "$coverage" ]]; then
      continue
    fi

    coverage_count=$((coverage_count + 1))
    coverage_sum=$((coverage_sum + coverage))
  done < <(find "${find_arguments[@]}" | sort)

  if [[ "$coverage_count" -gt 0 ]]; then
    coverage_average=$((coverage_sum / coverage_count))
  fi

  echo ""
  echo "=== ${heading} Average Coverage: ${coverage_average}% (threshold: ${threshold}%) ==="

  if [[ "$coverage_count" -eq 0 ]]; then
    echo "FAIL: No ${mode} test projects produced coverage data."
    exit 1
  fi

  if [[ "$coverage_average" -lt "$threshold" ]]; then
    echo "FAIL: ${heading} coverage ${coverage_average}% is below threshold ${threshold}%"
    exit 1
  fi

  echo "PASS: ${heading} coverage ${coverage_average}% meets threshold ${threshold}%"
}

case "${1:-}" in
  "$UNIT_TEST_MODE")
    run_coverage_gate "$UNIT_TEST_MODE" "Unit Test" "$UT_THRESHOLD" "*.csproj" "*E2E*"
    ;;
  "$E2E_TEST_MODE")
    run_coverage_gate "$E2E_TEST_MODE" "E2E Test" "$E2E_THRESHOLD" "*E2E*.csproj"
    ;;
  *)
    usage
    ;;
esac
