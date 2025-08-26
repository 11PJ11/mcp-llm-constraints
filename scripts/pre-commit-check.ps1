#!/usr/bin/env pwsh
param(
    [switch]$SkipMutationTesting
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Pre-commit Quality Check" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script runs the same quality gates that CI/CD runs."
Write-Host "Run this before committing to catch issues early and save time."
Write-Host ""

# Change to project root
$ProjectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $ProjectRoot

try {
    Write-Host "Running quality gates..." -ForegroundColor Yellow
    
    if ($SkipMutationTesting) {
        $env:RUN_MUTATION_TESTS = "false"
        Write-Host "⚠️  Skipping mutation testing (faster for development)" -ForegroundColor Yellow
    }
    
    # Run quality gates
    if ($IsWindows -or $env:OS -eq "Windows_NT") {
        & .\scripts\quality-gates.ps1
    } else {
        & .\scripts\quality-gates.sh
    }
    
    Write-Host ""
    Write-Host "✅ SUCCESS: All quality gates passed!" -ForegroundColor Green
    Write-Host "   Your code is ready to commit and should pass CI/CD." -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Next steps:" -ForegroundColor Cyan
    Write-Host "   git add -A"
    Write-Host "   git commit -m 'your commit message'"
    Write-Host "   git push"
    
} catch {
    Write-Host ""
    Write-Host "❌ FAILED: Quality gates did not pass" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Common fixes:" -ForegroundColor Cyan
    Write-Host "   - Run 'dotnet format' to fix formatting issues"
    Write-Host "   - Fix any failing tests with 'dotnet test'"
    Write-Host "   - Address build warnings/errors with 'dotnet build'"
    Write-Host "   - Check mutation testing results if failing"
    Write-Host ""
    Write-Host "🔄 After fixing issues, run this script again:" -ForegroundColor Cyan
    Write-Host "   .\scripts\pre-commit-check.ps1"
    Write-Host ""
    Write-Host "⚡ For faster development iteration (skip mutation tests):" -ForegroundColor Cyan
    Write-Host "   .\scripts\pre-commit-check.ps1 -SkipMutationTesting"
    
    exit 1
}