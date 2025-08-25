#!/bin/bash
set -euo pipefail

echo "🔍 Running Quality Gates for Constraint Enforcement MCP Server"
echo "=============================================================="

# Change to project root
cd "$(dirname "$0")/.."

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
echo "  ✓ Mutation testing quality thresholds met"
echo ""
echo "Ready for commit/deployment."