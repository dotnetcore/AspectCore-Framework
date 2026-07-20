#!/bin/bash
# AspectCore Framework Benchmark Runner
#
# Usage: ./run-benchmarks.sh [filter] [--quick]
#   filter: BenchmarkDotNet filter (e.g. "*Sync*", "*Pipeline*")
#   --quick: Use ShortRunJob instead of full run (faster, less accurate)
#
# Examples:
#   ./run-benchmarks.sh                    # Run all benchmarks (full accuracy)
#   ./run-benchmarks.sh "*Sync*"           # Run only sync benchmarks
#   ./run-benchmarks.sh --quick            # Quick run of all benchmarks
#   ./run-benchmarks.sh "*MSDI*" --quick   # Quick run of MSDI benchmarks
#
# Results are output to benchmarks/results/ with timestamps.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$SCRIPT_DIR/AspectCore.Benchmarks/AspectCore.Benchmarks.csproj"
RESULTS_DIR="$SCRIPT_DIR/results"

# Parse arguments
FILTER=""
QUICK=""
EXTRA_ARGS=()

for arg in "$@"; do
    case "$arg" in
        --quick)
            QUICK="--quick"
            ;;
        --help|-h)
            echo "Usage: $0 [filter] [--quick]"
            echo ""
            echo "Arguments:"
            echo "  filter    BenchmarkDotNet filter pattern (e.g. '*Sync*', '*Pipeline*')"
            echo "  --quick   Use ShortRunJob for fast iteration (less accurate)"
            echo ""
            echo "Available categories:"
            echo "  *Sync*       Synchronous method calls"
            echo "  *Async*      Async method calls (Task/ValueTask)"
            echo "  *Property*   Property getter/setter"
            echo "  *Generic*    Generic method calls"
            echo "  *Interface*  Interface proxy"
            echo "  *Creation*   Proxy creation overhead"
            echo "  *MSDI*       Microsoft.Extensions.DependencyInjection integration"
            echo "  *Pipeline*   Multi-interceptor pipeline depth"
            echo "  *ColdStart*  Cold start / first call latency"
            exit 0
            ;;
        *)
            if [[ -z "$FILTER" ]]; then
                FILTER="$arg"
            else
                EXTRA_ARGS+=("$arg")
            fi
            ;;
    esac
done

# Ensure results directory exists
mkdir -p "$RESULTS_DIR"

echo "=============================================="
echo "  AspectCore Framework Benchmark Suite"
echo "=============================================="
echo ""
echo "  Project:  $PROJECT"
echo "  Results:  $RESULTS_DIR"
echo "  Filter:   ${FILTER:-"(all)"}"
echo "  Mode:     ${QUICK:+"Quick (ShortRunJob)"}${QUICK:-"Full accuracy"}"
echo ""
echo "=============================================="
echo ""

# Build arguments
RUN_ARGS=()
if [[ -n "$FILTER" ]]; then
    RUN_ARGS+=("--filter" "$FILTER")
fi
if [[ -n "$QUICK" ]]; then
    RUN_ARGS+=("$QUICK")
fi
RUN_ARGS+=("${EXTRA_ARGS[@]+"${EXTRA_ARGS[@]}"}")

# Build and run
echo "[1/2] Building in Release mode..."
dotnet build "$PROJECT" -c Release --nologo -v quiet

echo "[2/2] Running benchmarks..."
echo ""

dotnet run --project "$PROJECT" -c Release --no-build -- "${RUN_ARGS[@]}"

echo ""
echo "=============================================="
echo "  Benchmarks complete!"
echo "  Results saved to: $RESULTS_DIR"
echo "=============================================="
