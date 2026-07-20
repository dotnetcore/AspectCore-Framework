#!/bin/bash
# AspectCore Framework CI Benchmark Script
#
# Runs benchmarks in quick mode (ShortRunJob) and validates that the SourceGenerator
# path does not regress beyond an acceptable threshold compared to DynamicProxy.
#
# Exit codes:
#   0 - All benchmarks pass, no regression detected
#   1 - Regression detected or benchmark failure
#
# Usage:
#   ./ci-benchmark.sh              # Run all benchmarks
#   ./ci-benchmark.sh "*Sync*"     # Run only sync benchmarks
#
# Outputs:
#   benchmarks/results/ci-<timestamp>/  - Detailed results (Markdown + JSON)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj"
RESULTS_DIR="$SCRIPT_DIR/results"
TIMESTAMP="$(date +%Y-%m-%d_%H-%M-%S)"
CI_RESULTS="$RESULTS_DIR/ci-$TIMESTAMP"

# Parse arguments
FILTER="${1:-}"

echo "=============================================="
echo "  AspectCore CI Benchmark Validation"
echo "=============================================="
echo ""
echo "  Timestamp: $TIMESTAMP"
echo "  Filter:    ${FILTER:-"(all)"}"
echo "  Results:   $CI_RESULTS"
echo ""

# Build
echo "[1/3] Building in Release mode..."
dotnet build "$PROJECT" -c Release --nologo -v quiet

# Run benchmarks with quick config and JSON export
echo "[2/3] Running benchmarks (ShortRunJob)..."
echo ""

RUN_ARGS=("--quick")
if [[ -n "$FILTER" ]]; then
    RUN_ARGS+=("--filter" "$FILTER")
fi

mkdir -p "$CI_RESULTS"

dotnet run --project "$PROJECT" -c Release --no-build -- "${RUN_ARGS[@]}" 2>&1 | tee "$CI_RESULTS/output.log"

# Check for regression using baseline comparison (if baseline exists)
echo ""
echo "[3/3] Checking for regressions..."

BASELINE_FILE="$RESULTS_DIR/baseline.json"
EXIT_CODE=0

if [[ -f "$BASELINE_FILE" ]]; then
    echo "  Baseline found at $BASELINE_FILE"
    echo "  Running comparison..."
    echo ""

    # Run comparison mode
    dotnet run --project "$PROJECT" -c Release --no-build -- --compare-baseline "${RUN_ARGS[@]:1}" 2>&1 | tee "$CI_RESULTS/comparison.log"
    EXIT_CODE=${PIPESTATUS[0]}
else
    echo "  No baseline found at $BASELINE_FILE"
    echo "  Skipping regression check (run with --export-baseline to create one)"
    echo ""
    echo "  To create a baseline:"
    echo "    dotnet run --project $PROJECT -c Release -- --export-baseline"
fi

# Summary
echo ""
echo "=============================================="
if [[ $EXIT_CODE -eq 0 ]]; then
    echo "  CI Benchmark: PASSED"
else
    echo "  CI Benchmark: FAILED (regression detected)"
fi
echo "  Results:  $CI_RESULTS"
echo "=============================================="

exit $EXIT_CODE
