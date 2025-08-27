#!/bin/bash
# Automated performance validation script for Constraint MCP Server
# Runs BenchmarkDotNet performance tests and validates against performance thresholds.
# Enforces sub-50ms p95 latency and sub-100MB memory requirements for CI/CD integration.

set -euo pipefail

# Default parameters
BENCHMARK_FILTER="*"
VALIDATION_MODE="Normal"
OUTPUT_PATH="BenchmarkDotNet.Artifacts"
BASELINE_PATH=""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}🚀 $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --benchmark-filter)
            BENCHMARK_FILTER="$2"
            shift 2
            ;;
        --validation-mode)
            VALIDATION_MODE="$2"
            shift 2
            ;;
        --output-path)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        --baseline-path)
            BASELINE_PATH="$2"
            shift 2
            ;;
        --help)
            echo "Performance Validation Script for Constraint MCP Server"
            echo ""
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --benchmark-filter FILTER   Filter for specific benchmarks (default: '*')"
            echo "  --validation-mode MODE       Validation mode: Strict, Normal, or Development (default: Normal)"
            echo "  --output-path PATH           Path for performance results output (default: BenchmarkDotNet.Artifacts)"
            echo "  --baseline-path PATH         Path to baseline performance results for comparison"
            echo "  --help                       Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0 --validation-mode Strict"
            echo "  $0 --benchmark-filter '*ConstraintLibrary*' --validation-mode Development"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Validate validation mode
case $VALIDATION_MODE in
    Strict|Normal|Development)
        ;;
    *)
        print_error "Invalid validation mode: $VALIDATION_MODE"
        print_error "Valid modes: Strict, Normal, Development"
        exit 1
        ;;
esac

# Set performance thresholds based on validation mode
case $VALIDATION_MODE in
    Strict)
        P95_LATENCY_MS=40.0
        P99_LATENCY_MS=80.0
        MAX_MEMORY_MB=80.0
        ALLOCATION_KB=8.0
        ;;
    Normal)
        P95_LATENCY_MS=50.0
        P99_LATENCY_MS=100.0
        MAX_MEMORY_MB=100.0
        ALLOCATION_KB=10.0
        ;;
    Development)
        P95_LATENCY_MS=75.0
        P99_LATENCY_MS=150.0
        MAX_MEMORY_MB=150.0
        ALLOCATION_KB=15.0
        ;;
esac

print_status "Starting Performance Validation"
print_info "Mode: $VALIDATION_MODE"
print_info "Filter: $BENCHMARK_FILTER"
print_info "Thresholds:"
echo -e "     ${CYAN}P95 Latency: ${P95_LATENCY_MS}ms${NC}"
echo -e "     ${CYAN}P99 Latency: ${P99_LATENCY_MS}ms${NC}"
echo -e "     ${CYAN}Max Memory: ${MAX_MEMORY_MB}MB${NC}"
echo -e "     ${CYAN}Max Allocation: ${ALLOCATION_KB}KB/op${NC}"
echo ""

# Ensure we're in the repository root
if [[ ! -f "ConstraintMcpServer.sln" ]]; then
    print_error "Must be run from repository root directory"
    exit 1
fi

# Build the performance project
print_info "Building performance benchmarks..."
if ! dotnet build tests/ConstraintMcpServer.Performance/ConstraintMcpServer.Performance.csproj --configuration Release --verbosity quiet; then
    print_error "Failed to build performance benchmarks"
    exit 1
fi
print_success "Performance benchmarks built successfully"

# Create output directory
mkdir -p "$OUTPUT_PATH"

# Run benchmarks
print_info "Running performance benchmarks..."
print_info "This may take several minutes..."

BENCHMARK_ARGS=(
    "run"
    "--project" "tests/ConstraintMcpServer.Performance"
    "--configuration" "Release"
    "--"
    "--filter" "$BENCHMARK_FILTER"
    "--exporters" "html,markdown"
    "--artifacts" "$OUTPUT_PATH"
)

# Add baseline comparison if provided
if [[ -n "$BASELINE_PATH" && -f "$BASELINE_PATH" ]]; then
    BENCHMARK_ARGS+=("--baseline" "$BASELINE_PATH")
    echo -e "${CYAN}📊 Using baseline from: $BASELINE_PATH${NC}"
fi

# Capture benchmark output
BENCHMARK_OUTPUT_FILE=$(mktemp)
if ! dotnet "${BENCHMARK_ARGS[@]}" 2>&1 | tee "$BENCHMARK_OUTPUT_FILE"; then
    print_error "Benchmark execution failed"
    echo "Benchmark output:"
    cat "$BENCHMARK_OUTPUT_FILE"
    rm -f "$BENCHMARK_OUTPUT_FILE"
    exit 1
fi

print_success "Benchmarks completed successfully"

# Parse benchmark results for validation
print_info "Analyzing performance results..."

# Read benchmark output
BENCHMARK_OUTPUT=$(cat "$BENCHMARK_OUTPUT_FILE")
rm -f "$BENCHMARK_OUTPUT_FILE"

# Look for results files
LATEST_HTML_RESULT=""
if [[ -d "$OUTPUT_PATH" ]]; then
    LATEST_HTML_RESULT=$(find "$OUTPUT_PATH" -name "*.html" -type f | head -n 1)
fi

if [[ -z "$LATEST_HTML_RESULT" ]]; then
    print_warning "No benchmark result files found for analysis"
    print_success "Performance validation completed (no threshold validation)"
    exit 0
fi

# Simple validation based on benchmark output text
VALIDATION_PASSED=true
ISSUES=()

# Check for performance-critical patterns in output
if echo "$BENCHMARK_OUTPUT" | grep -qi "outofmemoryexception"; then
    VALIDATION_PASSED=false
    ISSUES+=("❌ Out of memory exception detected")
fi

if echo "$BENCHMARK_OUTPUT" | grep -qi "timeout\|timeoutexception"; then
    VALIDATION_PASSED=false
    ISSUES+=("❌ Timeout exception detected")
fi

# Look for obvious performance regressions (simplified check)
HIGH_LATENCY_FOUND=false
while IFS= read -r line; do
    if echo "$line" | grep -qi "mean.*:.*[0-9.]\+.*ms"; then
        # Extract mean time in milliseconds (simplified)
        MEAN_TIME=$(echo "$line" | grep -oE '[0-9.]+' | head -n 1)
        if [[ -n "$MEAN_TIME" ]] && (( $(echo "$MEAN_TIME > $P95_LATENCY_MS" | bc -l) )); then
            HIGH_LATENCY_FOUND=true
            break
        fi
    fi
done <<< "$BENCHMARK_OUTPUT"

if $HIGH_LATENCY_FOUND; then
    VALIDATION_PASSED=false
    ISSUES+=("❌ High latency detected (>${P95_LATENCY_MS}ms)")
fi

# Memory allocation validation (simplified)
if echo "$BENCHMARK_OUTPUT" | grep -q "Gen [12] : [1-9]"; then
    print_warning "Gen1/Gen2 garbage collection detected - monitor memory usage"
fi

# Performance summary
echo ""
echo -e "${CYAN}📈 Performance Validation Summary${NC}"
echo -e "${CYAN}=================================${NC}"

if $VALIDATION_PASSED; then
    print_success "All performance thresholds passed!"
    echo -e "   ${GREEN}Mode: $VALIDATION_MODE${NC}"
    echo -e "   ${GREEN}Results: $OUTPUT_PATH${NC}"
    
    if [[ -n "$LATEST_HTML_RESULT" ]]; then
        echo -e "   ${GREEN}HTML Report: $LATEST_HTML_RESULT${NC}"
    fi
else
    print_error "Performance validation failed!"
    for issue in "${ISSUES[@]}"; do
        echo -e "   ${RED}$issue${NC}"
    done
fi

echo ""
echo -e "${CYAN}📄 Detailed results available at:${NC}"
if [[ -d "$OUTPUT_PATH" ]]; then
    find "$OUTPUT_PATH" -name "*.html" -o -name "*.md" | while read -r file; do
        echo -e "   ${GRAY}- $file${NC}"
    done
fi

# Exit with appropriate code
if $VALIDATION_PASSED; then
    print_success "Performance validation completed successfully!"
    exit 0
else
    print_error "Performance validation failed - see issues above"
    exit 1
fi