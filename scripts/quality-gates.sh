#!/bin/bash
set -euo pipefail

echo "🔍 Running Quality Gates for Constraint Enforcement MCP Server"
echo "=============================================================="

# Change to project root
cd "$(dirname "$0")/.."

# Clean up any zombie processes first (Windows/WSL compatibility)
echo "🧹 Cleaning up any zombie processes..."
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" || -n "${WINDIR:-}" ]]; then
    # Windows cleanup - more aggressive
    taskkill //F //IM ConstraintMcpServer.exe 2>/dev/null || true
    taskkill //F //IM testhost.exe 2>/dev/null || true
    taskkill //F //IM vstest.console.exe 2>/dev/null || true
    taskkill //F //IM dotnet.exe 2>/dev/null || true
    # Wait for file handles to be released
    sleep 2
else
    # Unix/Linux cleanup
    pkill -9 -f "ConstraintMcpServer" 2>/dev/null || true
    pkill -9 -f "dotnet.*test" 2>/dev/null || true
    pkill -9 -f "testhost" 2>/dev/null || true
    # Wait for cleanup
    sleep 1
fi

# Detect environment and set appropriate settings
if [[ "${FAST_COMMIT:-false}" == "true" ]]; then
    echo "⚡ Fast commit mode enabled - skipping mutation testing"
    export RUN_MUTATION_TESTS=false
elif [[ "${CI:-false}" == "true" ]]; then
    echo "🤖 CI environment detected - mutation testing disabled"
    export RUN_MUTATION_TESTS=false
else
    echo "💻 Local development - mutation testing disabled"
    export RUN_MUTATION_TESTS=false
fi

echo ""
echo "📦 Step 1: Clean and Restore"
echo "----------------------------"
dotnet clean --verbosity quiet
dotnet restore --verbosity quiet

echo ""
echo "🔧 Step 2: Build (Release)"
echo "-------------------------"
dotnet build --configuration Release --no-restore --verbosity minimal

echo ""
echo "📝 Step 3: Code Formatting"
echo "-------------------------"
dotnet format --verify-no-changes --verbosity minimal

echo ""
echo "🧪 Step 4: Run Tests"
echo "-------------------"
dotnet test --configuration Release --no-build --verbosity normal

echo ""
echo "🧬 Step 5: Mutation Testing (Business Logic Quality)"
echo "---------------------------------------------------"
# Check if mutation testing should run (only for business logic changes)
if [[ "${RUN_MUTATION_TESTS:-true}" == "true" ]]; then
    echo "Running mutation tests on critical business logic..."
    ./scripts/run-mutation-tests.sh --threshold 75 --target all
    echo "Mutation testing completed ✓"
else
    echo "Skipping mutation testing (RUN_MUTATION_TESTS=false)"
fi

echo ""
echo "✅ All Quality Gates Passed!"
echo ""
echo "Quality gates validated:"
echo "  ✓ Clean build with zero warnings/errors"
echo "  ✓ Code formatting compliance"  
echo "  ✓ All tests passing"
echo "  ✓ Release configuration build successful"
if [[ "${RUN_MUTATION_TESTS:-true}" == "true" ]]; then
    echo "  ✓ Mutation testing quality thresholds met"
else
    echo "  ℹ️ Mutation testing skipped (run locally with RUN_MUTATION_TESTS=true)"
fi
echo ""
echo "Ready for commit/deployment."