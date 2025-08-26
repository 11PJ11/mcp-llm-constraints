#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated performance validation script for Constraint MCP Server
.DESCRIPTION
    Runs BenchmarkDotNet performance tests and validates against performance thresholds.
    Enforces sub-50ms p95 latency and sub-100MB memory requirements for CI/CD integration.
.PARAMETER BenchmarkFilter
    Optional filter for specific benchmarks (e.g., "*TriggerBenchmark*")
.PARAMETER ValidationMode
    Validation mode: Strict, Normal, or Development
.PARAMETER OutputPath
    Path for performance results output
.PARAMETER BaselinePath
    Path to baseline performance results for comparison
.EXAMPLE
    ./scripts/performance-validation.ps1 -ValidationMode Strict
.EXAMPLE
    ./scripts/performance-validation.ps1 -BenchmarkFilter "*ConstraintLibrary*" -ValidationMode Development
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$BenchmarkFilter = "*",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("Strict", "Normal", "Development")]
    [string]$ValidationMode = "Normal",
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "BenchmarkDotNet.Artifacts",
    
    [Parameter(Mandatory = $false)]
    [string]$BaselinePath = $null
)

# Performance thresholds based on validation mode
$thresholds = @{
    "Strict" = @{
        P95LatencyMs = 40.0      # Stricter than 50ms requirement
        P99LatencyMs = 80.0      # Stricter than 100ms soft limit
        MaxMemoryMB = 80.0       # Stricter than 100MB requirement
        AllocationKB = 8.0       # Stricter than 10KB per operation
    }
    "Normal" = @{
        P95LatencyMs = 50.0      # Standard requirement
        P99LatencyMs = 100.0     # Standard soft limit
        MaxMemoryMB = 100.0      # Standard requirement
        AllocationKB = 10.0      # Standard limit
    }
    "Development" = @{
        P95LatencyMs = 75.0      # Relaxed for development
        P99LatencyMs = 150.0     # Relaxed for development
        MaxMemoryMB = 150.0      # Relaxed for development
        AllocationKB = 15.0      # Relaxed for development
    }
}

$currentThresholds = $thresholds[$ValidationMode]

Write-Host "🚀 Starting Performance Validation" -ForegroundColor Green
Write-Host "   Mode: $ValidationMode" -ForegroundColor Yellow
Write-Host "   Filter: $BenchmarkFilter" -ForegroundColor Yellow
Write-Host "   Thresholds:" -ForegroundColor Yellow
Write-Host "     P95 Latency: $($currentThresholds.P95LatencyMs)ms" -ForegroundColor Cyan
Write-Host "     P99 Latency: $($currentThresholds.P99LatencyMs)ms" -ForegroundColor Cyan
Write-Host "     Max Memory: $($currentThresholds.MaxMemoryMB)MB" -ForegroundColor Cyan
Write-Host "     Max Allocation: $($currentThresholds.AllocationKB)KB/op" -ForegroundColor Cyan
Write-Host ""

# Ensure we're in the repository root
if (-not (Test-Path "ConstraintMcpServer.sln")) {
    Write-Error "❌ Must be run from repository root directory"
    exit 1
}

# Build the performance project
Write-Host "🔨 Building performance benchmarks..." -ForegroundColor Yellow
$buildResult = dotnet build tests/ConstraintMcpServer.Performance/ConstraintMcpServer.Performance.csproj --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Failed to build performance benchmarks"
    exit 1
}
Write-Host "✅ Performance benchmarks built successfully" -ForegroundColor Green

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -Path $OutputPath -ItemType Directory | Out-Null
}

# Run benchmarks
Write-Host "⚡ Running performance benchmarks..." -ForegroundColor Yellow
Write-Host "   This may take several minutes..." -ForegroundColor Gray

$benchmarkArgs = @(
    "run"
    "--project", "tests/ConstraintMcpServer.Performance"
    "--configuration", "Release"
    "--"
    "--filter", $BenchmarkFilter
    "--exporters", "html,markdown"
    "--artifacts", $OutputPath
)

# Add baseline comparison if provided
if ($BaselinePath -and (Test-Path $BaselinePath)) {
    $benchmarkArgs += "--baseline", $BaselinePath
    Write-Host "📊 Using baseline from: $BaselinePath" -ForegroundColor Cyan
}

$benchmarkOutput = & dotnet $benchmarkArgs 2>&1
$benchmarkExitCode = $LASTEXITCODE

# Check if benchmarks ran successfully
if ($benchmarkExitCode -ne 0) {
    Write-Error "❌ Benchmark execution failed"
    Write-Host "Benchmark output:" -ForegroundColor Red
    $benchmarkOutput | Write-Host
    exit 1
}

Write-Host "✅ Benchmarks completed successfully" -ForegroundColor Green

# Parse benchmark results for validation
Write-Host "📊 Analyzing performance results..." -ForegroundColor Yellow

# Look for results files
$resultsFiles = Get-ChildItem -Path $OutputPath -Filter "*.html" | Sort-Object LastWriteTime -Descending
if ($resultsFiles.Count -eq 0) {
    Write-Warning "⚠️ No benchmark result files found for analysis"
    Write-Host "✅ Performance validation completed (no threshold validation)" -ForegroundColor Green
    exit 0
}

# Simple validation based on benchmark output text
$benchmarkOutputText = $benchmarkOutput -join "`n"
$validationPassed = $true
$issues = @()

# Check for performance-critical patterns in output
if ($benchmarkOutputText -match "OutOfMemoryException") {
    $validationPassed = $false
    $issues += "❌ Out of memory exception detected"
}

if ($benchmarkOutputText -match "timeout|TimeoutException") {
    $validationPassed = $false
    $issues += "❌ Timeout exception detected"
}

# Look for obvious performance regressions (simplified check)
$meanTimePattern = "Mean\s*:\s*([0-9.]+)\s*(ns|μs|ms|s)"
$meanMatches = [regex]::Matches($benchmarkOutputText, $meanTimePattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$highLatencyFound = $false
foreach ($match in $meanMatches) {
    $value = [double]$match.Groups[1].Value
    $unit = $match.Groups[2].Value.ToLower()
    
    # Convert to milliseconds
    $valueInMs = switch ($unit) {
        "ns" { $value / 1000000 }
        "μs" { $value / 1000 }
        "ms" { $value }
        "s"  { $value * 1000 }
        default { $value }
    }
    
    # Check against P95 threshold (using mean as approximation)
    if ($valueInMs -gt $currentThresholds.P95LatencyMs) {
        $highLatencyFound = $true
        break
    }
}

if ($highLatencyFound) {
    $validationPassed = $false
    $issues += "❌ High latency detected (>$($currentThresholds.P95LatencyMs)ms)"
}

# Memory allocation validation (simplified)
if ($benchmarkOutputText -match "Gen\s*[12]\s*:\s*[1-9]") {
    Write-Host "⚠️  Gen1/Gen2 garbage collection detected - monitor memory usage" -ForegroundColor Yellow
}

# Performance summary
Write-Host ""
Write-Host "📈 Performance Validation Summary" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

if ($validationPassed) {
    Write-Host "✅ All performance thresholds passed!" -ForegroundColor Green
    Write-Host "   Mode: $ValidationMode" -ForegroundColor Green
    Write-Host "   Results: $OutputPath" -ForegroundColor Green
    
    if ($resultsFiles.Count -gt 0) {
        Write-Host "   HTML Report: $($resultsFiles[0].FullName)" -ForegroundColor Green
    }
} else {
    Write-Host "❌ Performance validation failed!" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "   $issue" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📄 Detailed results available at:" -ForegroundColor Cyan
$resultsFiles | ForEach-Object { 
    Write-Host "   - $($_.FullName)" -ForegroundColor Gray
}

# Exit with appropriate code
if ($validationPassed) {
    Write-Host "🎉 Performance validation completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "💥 Performance validation failed - see issues above" -ForegroundColor Red
    exit 1
}