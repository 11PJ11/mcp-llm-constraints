#!/usr/bin/env pwsh
# Mutation Testing Script for MCP Constraint Server
# Runs Stryker.NET mutation testing with business-focused quality gates

param(
    [switch]$SkipBuild,
    [string]$Target = "all",
    [int]$Threshold = 75
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "🧬 Running Mutation Testing for MCP Constraint Server" -ForegroundColor Cyan
Write-Host "Target: $Target | Threshold: $Threshold%" -ForegroundColor Gray

# Ensure we're in the right directory
if (-not (Test-Path "src/ConstraintMcpServer")) {
    Write-Error "❌ Must run from repository root directory"
    exit 1
}

# Build if not skipped
if (-not $SkipBuild) {
    Write-Host "🔨 Building solution..." -ForegroundColor Yellow
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Build failed"
        exit 1
    }
}

# Run unit tests first to ensure baseline
Write-Host "🧪 Running baseline tests..." -ForegroundColor Yellow
dotnet test tests/ConstraintMcpServer.Tests --configuration Release --no-build --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Baseline tests failed - mutation testing requires passing tests"
    exit 1
}

# Clean previous mutation results
if (Test-Path "StrykerOutput") {
    Remove-Item "StrykerOutput" -Recurse -Force
    Write-Host "🧹 Cleaned previous mutation results" -ForegroundColor Gray
}

# Run mutation testing based on target
Write-Host "🧬 Starting mutation testing..." -ForegroundColor Green

switch ($Target) {
    "scheduler" {
        Write-Host "Mutating Scheduler business logic..." -ForegroundColor Yellow
        dotnet dotnet-stryker --config-file stryker-config.json `
            --mutate "**/Scheduling/Scheduler.cs" `
            --break-at $Threshold
    }
    "selector" {
        Write-Host "Mutating ConstraintSelector business logic..." -ForegroundColor Yellow
        dotnet dotnet-stryker --config-file stryker-config.json `
            --mutate "**/Selection/ConstraintSelector.cs" `
            --break-at $Threshold
    }
    "domain" {
        Write-Host "Mutating Domain entities..." -ForegroundColor Yellow
        dotnet dotnet-stryker --config-file stryker-config.json `
            --mutate "**/Domain/**/*.cs" `
            --break-at $Threshold
    }
    "all" {
        Write-Host "Mutating all business logic..." -ForegroundColor Yellow
        dotnet dotnet-stryker --config-file stryker-config.json `
            --break-at $Threshold
    }
    default {
        Write-Error "❌ Invalid target: $Target. Use: scheduler, selector, domain, or all"
        exit 1
    }
}

$mutationExitCode = $LASTEXITCODE

# Display results
Write-Host ""
if ($mutationExitCode -eq 0) {
    Write-Host "✅ Mutation Testing PASSED" -ForegroundColor Green
    Write-Host "Mutation score meets threshold of $Threshold%" -ForegroundColor Green
    
    if (Test-Path "StrykerOutput/reports") {
        Write-Host ""
        Write-Host "📊 Reports generated:" -ForegroundColor Cyan
        Get-ChildItem "StrykerOutput/reports" -Recurse -File | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "❌ Mutation Testing FAILED" -ForegroundColor Red
    Write-Host "Mutation score below threshold of $Threshold%" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Business-focused improvement suggestions:" -ForegroundColor Yellow
    Write-Host "  1. Add property tests for untested business invariants" -ForegroundColor Gray
    Write-Host "  2. Add model tests for missing state transitions" -ForegroundColor Gray  
    Write-Host "  3. Add edge case tests for boundary conditions" -ForegroundColor Gray
    Write-Host "  4. Review killed mutants to identify missing business rules" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Check StrykerOutput/reports/mutation-report.html for details" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🧬 Mutation testing completed" -ForegroundColor Cyan
exit $mutationExitCode