#!/bin/bash
# Fast commit script that skips mutation testing for quick commits
# Usage: ./scripts/fast-commit.sh "commit message"

set -euo pipefail

if [ $# -eq 0 ]; then
    echo "Usage: $0 \"commit message\""
    echo ""
    echo "This script performs a fast commit that skips mutation testing:"
    echo "  ✓ Clean build"
    echo "  ✓ Code formatting" 
    echo "  ✓ Unit and integration tests"
    echo "  ⏭ Skips mutation testing (saves 5-10 minutes)"
    echo ""
    echo "For full quality gates including mutation testing, use regular 'git commit'"
    exit 1
fi

COMMIT_MESSAGE="$1"

echo "⚡ Fast Commit Mode - Skipping Mutation Testing"
echo "=============================================="
echo "Commit message: $COMMIT_MESSAGE"
echo ""

# Set environment variable to skip mutation testing
export FAST_COMMIT=true

# Stage all changes
echo "📋 Staging changes..."
git add .

# Commit with fast mode (pre-commit hook will detect FAST_COMMIT=true)
echo "🚀 Committing with fast quality gates..."
git commit -m "$COMMIT_MESSAGE"

echo ""
echo "✅ Fast commit completed successfully!"
echo ""
echo "💡 Note: Mutation testing was skipped for speed."
echo "   Run full quality gates before important releases:"
echo "   RUN_MUTATION_TESTS=true ./scripts/quality-gates.sh"