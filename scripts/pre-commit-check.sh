#!/bin/bash
set -euo pipefail

echo "🔍 Pre-commit Quality Check"
echo "==========================="
echo ""
echo "This script runs the same quality gates that CI/CD runs."
echo "Run this before committing to catch issues early and save time."
echo ""

# Change to project root
cd "$(dirname "$0")/.."

# Validate shell scripts first
echo "🔍 Validating shell script line endings..."
validation_failed=false
for script in scripts/*.sh; do
    if [[ -f "$script" ]]; then
        if file "$script" | grep -q "CRLF"; then
            echo "❌ ERROR: $script contains CRLF line endings (will cause CI/CD failures)"
            echo "   Fix with: sed -i 's/\r$//' $script"
            validation_failed=true
        fi
    fi
done

if [[ "$validation_failed" == "true" ]]; then
    echo ""
    echo "❌ FAILED: Shell script validation failed"
    echo "Fix line ending issues and run this script again."
    exit 1
fi

echo "✅ Shell scripts validated successfully"
echo ""

# Run the quality gates
echo "Running quality gates..."
if ./scripts/quality-gates.sh; then
    echo ""
    echo "✅ SUCCESS: All quality gates passed!"
    echo "   Your code is ready to commit and should pass CI/CD."
    echo ""
    echo "💡 Next steps:"
    echo "   git add -A"
    echo "   git commit -m 'your commit message'"
    echo "   git push"
else
    echo ""
    echo "❌ FAILED: Quality gates did not pass"
    echo ""
    echo "💡 Common fixes:"
    echo "   - Run 'dotnet format' to fix formatting issues"
    echo "   - Fix any failing tests with 'dotnet test'"
    echo "   - Address build warnings/errors with 'dotnet build'"
    echo "   - Check mutation testing results if failing"
    echo ""
    echo "🔄 After fixing issues, run this script again:"
    echo "   ./scripts/pre-commit-check.sh"
    exit 1
fi