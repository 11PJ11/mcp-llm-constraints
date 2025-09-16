#!/usr/bin/env pwsh
# Quick test runner to check current E2E test status

Write-Host "=== Running E2E Tests to Check Current State ===" -ForegroundColor Green

try {
    # Run E2E tests specifically
    dotnet test --filter "Category=E2E" --verbosity normal

    Write-Host "`n=== E2E Test Status Complete ===" -ForegroundColor Green
} catch {
    Write-Host "Error running tests: $_" -ForegroundColor Red
    exit 1
}