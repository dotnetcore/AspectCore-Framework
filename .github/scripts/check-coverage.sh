#!/bin/bash
# Coverage check script for CI
# Checks unit test and E2E test coverage against thresholds

set -e

# Allow running on newer .NET runtimes
export DOTNET_ROLL_FORWARD=Major

UT_THRESHOLD=95
E2E_THRESHOLD=80

echo "=== Coverage Check ==="
echo "UT threshold: ${UT_THRESHOLD}%"
echo "E2E threshold: ${E2E_THRESHOLD}%"
echo ""

# Function to run tests with coverage and extract coverage percentage
run_coverage() {
  local project="$1"
  local label="$2"

  echo "Measuring: $label..."

  # Clean up previous coverage files in the test project's TestResults directory
  project_dir=$(dirname "$project")
  rm -rf "$project_dir/TestResults" 2>/dev/null || true

  # Derive the source assembly name from the test project name
  # e.g. AspectCore.Extensions.Windsor.Test -> [AspectCore.Extensions.Windsor]*
  project_name=$(basename "$project_dir")
  # Remove .Test or .Tests suffix
  source_assembly="${project_name%.Test}"
  source_assembly="${source_assembly%.Tests}"

  # For E2E tests, include only Core and Abstractions assemblies.
  # Extension assemblies (Reflection, DependencyInjection, Autofac, Windsor, etc.)
  # have their own dedicated unit test projects and are measured separately.
  local include_filter="[${source_assembly}]*"
  if [[ "$project_name" == *"E2E"* ]]; then
    include_filter="[AspectCore.Core]*%2c[AspectCore.Abstractions]*"
  fi

  # Run test with coverage collection (net9.0 only for speed)
  # Do NOT use --no-build: coverlet.msbuild needs to instrument the assembly at build time
  # Include only the source assembly to get accurate per-assembly coverage
  dotnet test "$project" --configuration Release -f net9.0 \
    /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./TestResults/ \
    "/p:Include=${include_filter}" \
    2>&1 | tail -1 || true

  # Find the generated coverage file - it's in the test project's TestResults directory
  coverage_file=$(find "$project_dir/TestResults" -name "*.cobertura.xml" 2>/dev/null | head -1)

  if [ -n "$coverage_file" ] && [ -f "$coverage_file" ]; then
    # Extract line-rate from cobertura XML (e.g. line-rate="0.8278")
    line_rate=$(grep -o 'line-rate="[^"]*"' "$coverage_file" | head -1 | cut -d'"' -f2)
    if [ -n "$line_rate" ]; then
      coverage_pct=$(echo "$line_rate * 100" | bc | cut -d. -f1)
      echo "  Coverage: ${coverage_pct}%"
      echo "$coverage_pct"
      return
    fi
  fi

  echo "  No coverage data"
  echo "0"
}

# Measure unit test coverage
echo "=== Unit Test Coverage ==="
ut_coverages=""
ut_count=0

for test_project in $(find ./tests -name "*.csproj" ! -name "*E2E*"); do
  cov=$(run_coverage "$test_project" "$(basename $(dirname $test_project))")
  last_line=$(echo "$cov" | tail -1)
  if [ "$last_line" != "0" ] && [ -n "$last_line" ]; then
    ut_coverages="$ut_coverages $last_line"
    ut_count=$((ut_count + 1))
  fi
done

# Calculate average UT coverage
if [ $ut_count -gt 0 ]; then
  ut_sum=0
  for c in $ut_coverages; do
    ut_sum=$((ut_sum + c))
  done
  ut_avg=$((ut_sum / ut_count))
else
  ut_avg=0
fi

echo ""
echo "=== UT Average Coverage: ${ut_avg}% (threshold: ${UT_THRESHOLD}%) ==="

# Measure E2E test coverage
echo ""
echo "=== E2E Test Coverage ==="
e2e_coverages=""
e2e_count=0

for test_project in $(find ./tests -name "*E2E*.csproj"); do
  cov=$(run_coverage "$test_project" "$(basename $(dirname $test_project))")
  last_line=$(echo "$cov" | tail -1)
  if [ "$last_line" != "0" ] && [ -n "$last_line" ]; then
    e2e_coverages="$e2e_coverages $last_line"
    e2e_count=$((e2e_count + 1))
  fi
done

# Calculate average E2E coverage
if [ $e2e_count -gt 0 ]; then
  e2e_sum=0
  for c in $e2e_coverages; do
    e2e_sum=$((e2e_sum + c))
  done
  e2e_avg=$((e2e_sum / e2e_count))
else
  e2e_avg=0
fi

echo ""
echo "=== E2E Average Coverage: ${e2e_avg}% (threshold: ${E2E_THRESHOLD}%) ==="

# Final check
echo ""
echo "=== Results ==="
failed=0

if [ "$ut_avg" -lt "$UT_THRESHOLD" ]; then
  echo "FAIL: UT coverage ${ut_avg}% is below threshold ${UT_THRESHOLD}%"
  failed=1
else
  echo "PASS: UT coverage ${ut_avg}% meets threshold ${UT_THRESHOLD}%"
fi

if [ "$e2e_avg" -lt "$E2E_THRESHOLD" ]; then
  echo "FAIL: E2E coverage ${e2e_avg}% is below threshold ${E2E_THRESHOLD}%"
  failed=1
else
  echo "PASS: E2E coverage ${e2e_avg}% meets threshold ${E2E_THRESHOLD}%"
fi

if [ "$failed" -eq 1 ]; then
  echo ""
  echo "Coverage check FAILED. Please add more tests to meet the thresholds."
  exit 1
fi

echo ""
echo "All coverage checks passed!"
exit 0
