#!/bin/bash
# Coverage check script for CI
# Checks unit test and E2E test coverage against thresholds

set -e

UT_THRESHOLD=50
E2E_THRESHOLD=50

echo "=== Coverage Check ==="
echo "UT threshold: ${UT_THRESHOLD}%"
echo "E2E threshold: ${E2E_THRESHOLD}%"
echo ""

# Build all projects
echo "Building projects..."
for project in $(find ./src -name "*.csproj"); do
  dotnet build --configuration Release "$project" 2>/dev/null
done
echo ""

# Function to run tests with coverage and extract coverage percentage
run_coverage() {
  local project="$1"
  local label="$2"

  echo "Measuring: $label..."

  # Run test with coverage collection (net9.0 only for speed)
  dotnet test "$project" --configuration Release --no-build -f net9.0 \
    /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./TestResults/coverage/ 2>&1 | tail -1

  # Find the generated coverage file (net9.0)
  coverage_file=$(find . -name "coverage.net9.0.cobertura.xml" -path "*TestResults*" 2>/dev/null | head -1)

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
  # Clean up coverage file for next iteration
  rm -f ./TestResults/coverage/coverage.net9.0.cobertura.xml 2>/dev/null || true
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
  rm -f ./TestResults/coverage/coverage.net9.0.cobertura.xml 2>/dev/null || true
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
