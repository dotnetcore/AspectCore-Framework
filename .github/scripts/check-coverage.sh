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
readonly NATIVEAOT_UNIT_TEST_MODE="nativeaot-unit"
readonly NATIVEAOT_E2E_TEST_MODE="nativeaot-e2e"
readonly UT_THRESHOLD=95
readonly E2E_THRESHOLD=80
readonly NATIVEAOT_UT_THRESHOLD=100
readonly NATIVEAOT_E2E_THRESHOLD=95

usage() {
  cat >&2 <<EOF
Usage:
  $0 collect {${UNIT_TEST_MODE}|${E2E_TEST_MODE}|${NATIVEAOT_UNIT_TEST_MODE}|${NATIVEAOT_E2E_TEST_MODE}} --output <result-file>
  $0 assert {${UNIT_TEST_MODE}|${E2E_TEST_MODE}|${NATIVEAOT_UNIT_TEST_MODE}|${NATIVEAOT_E2E_TEST_MODE}} --input <result-file>
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
    "$NATIVEAOT_UNIT_TEST_MODE")
      printf '%s|%s|%s|%s\n' "NativeAOT Unit Test" "$NATIVEAOT_UT_THRESHOLD" "AspectCore.Core.Tests.csproj" ""
      ;;
    "$NATIVEAOT_E2E_TEST_MODE")
      printf '%s|%s|%s|%s\n' "NativeAOT E2E Test" "$NATIVEAOT_E2E_THRESHOLD" "AspectCore.E2E.Tests.csproj" ""
      ;;
    *)
      usage
      ;;
  esac
}

nativeaot_mode() {
  local mode="$1"
  [[ "$mode" == "$NATIVEAOT_UNIT_TEST_MODE" || "$mode" == "$NATIVEAOT_E2E_TEST_MODE" ]]
}

nativeaot_filter() {
  local mode="$1"

  case "$mode" in
    "$NATIVEAOT_UNIT_TEST_MODE")
      printf '%s\n' "FullyQualifiedName~SourceGeneratorDiagnosticTests"
      ;;
    "$NATIVEAOT_E2E_TEST_MODE")
      printf '%s\n' "FullyQualifiedName~NativeAotSourceGeneratedScenarios"
      ;;
    *)
      printf '%s\n' ""
      ;;
  esac
}

nativeaot_include_filter() {
  local mode="$1"

  case "$mode" in
    "$NATIVEAOT_UNIT_TEST_MODE")
      printf '%s\n' "[AspectCore.SourceGenerator]*"
      ;;
    "$NATIVEAOT_E2E_TEST_MODE")
      printf '%s\n' "[AspectCore.Core]*%2c[AspectCore.Abstractions]*"
      ;;
    *)
      printf '%s\n' ""
      ;;
  esac
}

target_framework() {
  local mode="$1"

  case "$mode" in
    "$NATIVEAOT_UNIT_TEST_MODE")
      printf '%s\n' "net10.0"
      ;;
    *)
      printf '%s\n' "net9.0"
      ;;
  esac
}

nativeaot_scope_files() {
  local mode="$1"

  case "$mode" in
    "$NATIVEAOT_UNIT_TEST_MODE")
      printf '%s\n' "Emit/NativeAotSignatureDiagnostic.cs"
      ;;
    "$NATIVEAOT_E2E_TEST_MODE")
      printf '%s\n' \
        "AspectCore.Core/DynamicProxy/AspectContextFactory.cs" \
        "AspectCore.Core/DynamicProxy/SourceGeneratedAspectContext.cs"
      ;;
  esac
}

coverage_from_cobertura() {
  local coverage_file="$1"
  shift

  python3 - "$coverage_file" "$@" <<'PY'
import sys
import xml.etree.ElementTree as ET

coverage_file = sys.argv[1]
scope_files = set(sys.argv[2:])
root = ET.parse(coverage_file).getroot()
covered = 0
valid = 0

for cls in root.findall(".//class"):
    filename = cls.attrib.get("filename", "")
    if scope_files and filename not in scope_files:
        continue
    for line in cls.findall("./lines/line"):
        valid += 1
        if int(line.attrib.get("hits", "0")) > 0:
            covered += 1

if valid == 0:
    print("0")
else:
    print((covered * 100) // valid)
PY
}

run_coverage() {
  local project="$1"
  local label="$2"
  local mode="$3"
  local project_dir
  local project_name
  local source_assembly
  local include_filter
  local coverage_file
  local filter
  local coverage_pct
  local test_log
  local test_status
  local dotnet_args
  local framework

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
  if nativeaot_mode "$mode"; then
    include_filter=$(nativeaot_include_filter "$mode")
    filter=$(nativeaot_filter "$mode")
  else
    filter=""
  fi

  # Do not use --no-build: coverlet.msbuild needs to instrument assemblies.
  # Capture the pipeline status explicitly because this function's output is
  # consumed through command substitution by collect_coverage.
  set +e
  test_log=$(mktemp)
  framework=$(target_framework "$mode")
  dotnet_args=("$project" --configuration Release -f "$framework")
  if [[ -n "$filter" ]]; then
    dotnet_args+=(--filter "$filter")
  fi
  dotnet test "${dotnet_args[@]}" \
    /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./TestResults/ \
    "/p:Include=${include_filter}" \
    > "$test_log" 2>&1
  test_status=$?
  tail -1 "$test_log" >&2
  rm -f "$test_log"
  set -e

  if [[ "$test_status" -ne 0 ]]; then
    echo "  Test execution failed for: $label" >&2
    return "$test_status"
  fi

  coverage_file=$(find "$project_dir/TestResults" -name "*.cobertura.xml" -print -quit)
  if [[ -n "$coverage_file" && -f "$coverage_file" ]]; then
    if nativeaot_mode "$mode"; then
      mapfile -t scope_files < <(nativeaot_scope_files "$mode")
      coverage_pct=$(coverage_from_cobertura "$coverage_file" "${scope_files[@]}")
    else
      line_rate=$(grep -o 'line-rate="[^"]*"' "$coverage_file" | head -1 | cut -d'"' -f2)
      if [[ -n "$line_rate" ]]; then
        coverage_pct=$(echo "$line_rate * 100" | bc | cut -d. -f1)
      fi
    fi
    if [[ -n "$coverage_pct" ]]; then
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
    if ! coverage=$(run_coverage "$test_project" "$(basename "$(dirname "$test_project")")" "$mode"); then
      echo "FAIL: Test execution failed for ${test_project}." >&2
      return 1
    fi

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
