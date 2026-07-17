#!/bin/bash
# Coverage collection and assertion helpers for CI.
# `collect` executes a test group and stores its aggregate coverage result.
# `assert` validates a previously collected result so test execution and the
# coverage gate are exposed as separate GitHub Actions jobs.

set -euo pipefail

# Allow running on newer .NET runtimes.
export DOTNET_ROLL_FORWARD=Major

readonly UNIT_TEST_MODE="unit"
readonly E2E_TEST_MODE="e2e"
readonly UT_THRESHOLD=95
readonly E2E_THRESHOLD=80

usage() {
  cat >&2 <<EOF
Usage:
  $0 collect {${UNIT_TEST_MODE}|${E2E_TEST_MODE}} --output <result-file>
  $0 assert {${UNIT_TEST_MODE}|${E2E_TEST_MODE}} --input <result-file>
EOF
  exit 2
}

mode_metadata() {
  local mode="$1"

  case "$mode" in
    "$UNIT_TEST_MODE")
      printf '%s|%s|%s|%s\n' "Unit Test" "$UT_THRESHOLD" "*.csproj" "*E2E*"
      ;;
    "$E2E_TEST_MODE")
      printf '%s|%s|%s|%s\n' "E2E Test" "$E2E_THRESHOLD" "*E2E*.csproj" ""
      ;;
    *)
      usage
      ;;
  esac
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

collect_coverage() {
  local mode="$1"
  local output_file="$2"
  local metadata
  local heading
  local threshold
  local project_pattern
  local exclude_pattern
  local coverage
  local coverage_count=0
  local coverage_sum=0
  local coverage_average=0
  local find_arguments
  local test_project

  metadata=$(mode_metadata "$mode")
  IFS='|' read -r heading threshold project_pattern exclude_pattern <<< "$metadata"
  find_arguments=(./tests -name "$project_pattern")
  if [[ -n "$exclude_pattern" ]]; then
    find_arguments+=( ! -name "$exclude_pattern" )
  fi

  echo "=== ${heading} Coverage Collection ==="
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

  mkdir -p "$(dirname "$output_file")"
  cat > "$output_file" <<EOF
mode=${mode}
coverage=${coverage_average}
threshold=${threshold}
projects=${coverage_count}
EOF

  echo "=== ${heading} Average Coverage: ${coverage_average}% (threshold: ${threshold}%) ==="
  write_output coverage "$coverage_average"
  write_output threshold "$threshold"
  write_output projects "$coverage_count"
  write_output passed "true"

  if [[ "$coverage_count" -eq 0 ]]; then
    echo "FAIL: No ${mode} test projects produced coverage data."
    return 1
  fi
}

result_value() {
  local key="$1"
  local input_file="$2"

  awk -F= -v key="$key" '$1 == key { print substr($0, length(key) + 2); exit }' "$input_file"
}

write_output() {
  local key="$1"
  local value="$2"

  if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
    printf '%s=%s\n' "$key" "$value" >> "$GITHUB_OUTPUT"
  fi
}

assert_coverage() {
  local mode="$1"
  local input_file="$2"
  local metadata
  local heading
  local expected_threshold
  local ignored
  local result_mode=""
  local coverage="unavailable"
  local threshold
  local projects=0
  local passed=false

  metadata=$(mode_metadata "$mode")
  IFS='|' read -r heading expected_threshold ignored <<< "$metadata"

  if [[ -f "$input_file" ]]; then
    result_mode=$(result_value mode "$input_file")
    coverage=$(result_value coverage "$input_file")
    threshold=$(result_value threshold "$input_file")
    projects=$(result_value projects "$input_file")
  fi

  if [[ "$result_mode" == "$mode" && "$threshold" == "$expected_threshold" && "$coverage" =~ ^[0-9]+$ && "$projects" =~ ^[1-9][0-9]*$ ]]; then
    if (( coverage >= threshold )); then
      passed=true
    fi
  fi

  write_output coverage "$coverage"
  write_output threshold "$expected_threshold"
  write_output projects "$projects"
  write_output passed "$passed"

  echo "=== ${heading} Coverage Result ==="
  echo "Current coverage: ${coverage}%"
  echo "Threshold: ${expected_threshold}%"

  if [[ "$passed" == true ]]; then
    echo "PASS: ${heading} coverage ${coverage}% meets threshold ${expected_threshold}%"
    return 0
  fi

  if [[ "$coverage" =~ ^[0-9]+$ ]]; then
    echo "FAIL: ${heading} coverage ${coverage}% is below threshold ${expected_threshold}%"
  else
    echo "FAIL: ${heading} coverage result is missing or invalid."
  fi
  return 1
}

command="${1:-}"
mode="${2:-}"
[[ -n "$command" && -n "$mode" ]] || usage
shift 2

case "$command" in
  collect)
    [[ "${1:-}" == "--output" && -n "${2:-}" && "$#" -eq 2 ]] || usage
    collect_coverage "$mode" "$2"
    ;;
  assert)
    [[ "${1:-}" == "--input" && -n "${2:-}" && "$#" -eq 2 ]] || usage
    assert_coverage "$mode" "$2"
    ;;
  *)
    usage
    ;;
esac
