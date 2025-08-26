#!/bin/bash
# Mutation Testing Script for MCP Constraint Server
# Runs Stryker.NET mutation testing with business-focused quality gates

set -euo pipefail

# Default parameters
SKIP_BUILD=false
TARGET="all"
THRESHOLD=75

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --target)
            TARGET="$2"
            shift 2
            ;;
        --threshold)
            THRESHOLD="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--skip-build] [--target scheduler|selector|domain|all] [--threshold 75]"
            exit 1
            ;;
    esac
done

echo "🧬 Running Mutation Testing for MCP Constraint Server"
echo "Target: $TARGET | Threshold: $THRESHOLD%"

# Ensure we're in the right directory
if [ ! -d "src/ConstraintMcpServer" ]; then
    echo "❌ Must run from repository root directory"
    exit 1
fi

# Build if not skipped
if [ "$SKIP_BUILD" = false ]; then
    echo "🔨 Building solution..."
    dotnet build --configuration Release --no-restore
fi

# Run unit tests first to ensure baseline
echo "🧪 Running baseline tests..."
dotnet test tests/ConstraintMcpServer.Tests --configuration Release --no-build --logger "console;verbosity=minimal"

# Clean previous mutation results
if [ -d "StrykerOutput" ]; then
    rm -rf StrykerOutput
    echo "🧹 Cleaned previous mutation results"
fi

# Set environment variables for better VsTest reliability
export VSTEST_CONNECTION_TIMEOUT=180
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# Run mutation testing based on target
echo "🧬 Starting mutation testing..."
echo "VsTest connection timeout: $VSTEST_CONNECTION_TIMEOUT seconds"

case $TARGET in
    "scheduler")
        echo "Mutating Scheduler business logic..."
        dotnet dotnet-stryker --config-file stryker-config.json \
            --mutate "**/Scheduling/Scheduler.cs" \
            --break-at $THRESHOLD
        ;;
    "selector")
        echo "Mutating ConstraintSelector business logic..."
        dotnet dotnet-stryker --config-file stryker-config.json \
            --mutate "**/Selection/ConstraintSelector.cs" \
            --break-at $THRESHOLD
        ;;
    "domain")
        echo "Mutating Domain entities..."
        dotnet dotnet-stryker --config-file stryker-config.json \
            --mutate "**/Domain/**/*.cs" \
            --break-at $THRESHOLD
        ;;
    "all")
        echo "Mutating all business logic..."
        dotnet dotnet-stryker --config-file stryker-config.json \
            --break-at $THRESHOLD
        ;;
    *)
        echo "❌ Invalid target: $TARGET. Use: scheduler, selector, domain, or all"
        exit 1
        ;;
esac

mutation_exit_code=$?

# Display results
echo ""
if [ $mutation_exit_code -eq 0 ]; then
    echo "✅ Mutation Testing PASSED"
    echo "Mutation score meets threshold of $THRESHOLD%"
    
    if [ -d "StrykerOutput/reports" ]; then
        echo ""
        echo "📊 Reports generated:"
        find StrykerOutput/reports -type f -exec echo "  {}" \;
    fi
else
    echo "❌ Mutation Testing FAILED"
    echo "Mutation score below threshold of $THRESHOLD%"
    echo ""
    echo "💡 Business-focused improvement suggestions:"
    echo "  1. Add property tests for untested business invariants"
    echo "  2. Add model tests for missing state transitions"
    echo "  3. Add edge case tests for boundary conditions"
    echo "  4. Review killed mutants to identify missing business rules"
    echo ""
    echo "Check StrykerOutput/reports/mutation-report.html for details"
fi

echo ""
echo "🧬 Mutation testing completed"
exit $mutation_exit_code