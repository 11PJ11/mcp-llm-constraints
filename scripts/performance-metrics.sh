#!/bin/bash
# Performance Metrics and Kaizen Continuous Improvement Framework
# This script tracks pre-commit hook performance and suggests improvements

set -euo pipefail

METRICS_FILE=".git/hooks/performance-metrics.json"
BASELINE_FILE=".git/hooks/performance-baseline.json"

# Function to record performance metrics
function record_metrics() {
    local execution_time="$1"
    local mode="$2"
    local success="$3"
    local timestamp=$(date -Iseconds)
    
    # Create metrics directory if it doesn't exist
    mkdir -p "$(dirname "$METRICS_FILE")"
    
    # Initialize metrics file if it doesn't exist
    if [ ! -f "$METRICS_FILE" ]; then
        echo "[]" > "$METRICS_FILE"
    fi
    
    # Add new metric entry
    local new_entry=$(cat << EOF
{
  "timestamp": "$timestamp",
  "execution_time": $execution_time,
  "mode": "$mode",
  "success": $success,
  "git_commit_hash": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')"
}
EOF
    )
    
    # Update metrics file
    if command -v jq >/dev/null 2>&1; then
        # Use jq if available for proper JSON manipulation
        jq ". += [$new_entry]" "$METRICS_FILE" > "${METRICS_FILE}.tmp" && mv "${METRICS_FILE}.tmp" "$METRICS_FILE"
    else
        # Fallback to simple append (less robust but functional)
        echo "$new_entry" >> "$METRICS_FILE"
    fi
}

# Function to calculate performance statistics
function calculate_stats() {
    if [ ! -f "$METRICS_FILE" ]; then
        echo "No performance metrics found"
        return 1
    fi
    
    if command -v jq >/dev/null 2>&1; then
        local fast_mode_avg=$(jq -r '[.[] | select(.mode=="fast") | .execution_time] | add / length // 0' "$METRICS_FILE")
        local full_mode_avg=$(jq -r '[.[] | select(.mode=="full") | .execution_time] | add / length // 0' "$METRICS_FILE")
        local success_rate=$(jq -r '[.[] | select(.success==true)] | length as $successes | $successes / ([.[]] | length) * 100' "$METRICS_FILE")
        
        echo "📊 Performance Metrics:"
        echo "   ⚡ Fast mode average: ${fast_mode_avg}s"
        echo "   🔍 Full mode average: ${full_mode_avg}s"
        echo "   ✅ Success rate: ${success_rate}%"
    else
        echo "📊 Install 'jq' for detailed performance statistics"
    fi
}

# Function to suggest improvements based on metrics
function suggest_improvements() {
    if [ ! -f "$METRICS_FILE" ]; then
        return 0
    fi
    
    echo "💡 Kaizen Improvement Suggestions:"
    
    # Check if fast mode is consistently fast
    if command -v jq >/dev/null 2>&1; then
        local slow_fast_commits=$(jq -r '[.[] | select(.mode=="fast" and .execution_time > 30)] | length' "$METRICS_FILE")
        if [ "$slow_fast_commits" -gt 0 ]; then
            echo "   ⚠️ Fast mode occasionally exceeds 30s - consider further optimization"
        fi
        
        local recent_failures=$(jq -r '[.[] | select(.success==false)] | length' "$METRICS_FILE")
        if [ "$recent_failures" -gt 0 ]; then
            echo "   🔧 ${recent_failures} recent failures detected - review error patterns"
        fi
    fi
    
    echo "   📈 Consider these optimizations:"
    echo "     - Use build server: 'dotnet build-server shutdown' before commits"
    echo "     - Enable incremental builds in project files"
    echo "     - Profile individual test execution times"
}

# Function to display performance dashboard
function show_dashboard() {
    echo "🎯 Pre-commit Performance Dashboard"
    echo "===================================="
    calculate_stats
    echo ""
    suggest_improvements
}

# Main execution based on arguments
case "${1:-show}" in
    record)
        record_metrics "$2" "$3" "$4"
        ;;
    stats)
        calculate_stats
        ;;
    suggestions)
        suggest_improvements
        ;;
    dashboard|show)
        show_dashboard
        ;;
    *)
        echo "Usage: $0 {record|stats|suggestions|dashboard}"
        echo "  record <time> <mode> <success>  - Record performance metrics"
        echo "  stats                           - Show performance statistics"
        echo "  suggestions                     - Show improvement suggestions"
        echo "  dashboard                       - Show full performance dashboard"
        ;;
esac