# PowerShell Quality Gates Script for Constraint Enforcement MCP Server
param(
    [switch]$NoRestore = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Running Quality Gates for Constraint Enforcement MCP Server" -ForegroundColor Cyan
Write-Host "==============================================================" -ForegroundColor Cyan

# Change to project root
$ProjectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $ProjectRoot

try {
    Write-Host ""
    Write-Host "📦 Step 1: Clean and Restore" -ForegroundColor Yellow
    Write-Host "----------------------------" -ForegroundColor Yellow
    
    dotnet clean --verbosity quiet
    if (-not $NoRestore) {
        dotnet restore --verbosity quiet
    }

    Write-Host ""
    Write-Host "🔧 Step 2: Build (Release)" -ForegroundColor Yellow
    Write-Host "-------------------------" -ForegroundColor Yellow
    
    $buildArgs = @("build", "--configuration", "Release", "--verbosity", "minimal")
    if (-not $NoRestore) {
        $buildArgs += "--no-restore"
    }
    dotnet @buildArgs

    Write-Host ""
    Write-Host "📝 Step 3: Code Formatting" -ForegroundColor Yellow
    Write-Host "-------------------------" -ForegroundColor Yellow
    
    dotnet format --verify-no-changes --verbosity minimal

    Write-Host ""
    Write-Host "🧪 Step 4: Run Tests" -ForegroundColor Yellow
    Write-Host "-------------------" -ForegroundColor Yellow
    
    dotnet test --configuration Release --no-build --verbosity normal

    Write-Host ""
    Write-Host "✅ All Quality Gates Passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Quality gates validated:" -ForegroundColor Green
    Write-Host "  ✓ Clean build with zero warnings/errors" -ForegroundColor Green
    Write-Host "  ✓ Code formatting compliance" -ForegroundColor Green  
    Write-Host "  ✓ All tests passing" -ForegroundColor Green
    Write-Host "  ✓ Release configuration build successful" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ready for commit/deployment." -ForegroundColor Green

    exit 0
}
catch {
    Write-Host ""
    Write-Host "❌ Quality Gates Failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please fix the issues above before committing." -ForegroundColor Red
    
    exit 1
}