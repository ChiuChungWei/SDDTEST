# Contract Review Scheduler - Integrated Tests Script
# Purpose: Run all integration tests with detailed reporting
# Usage: .\scripts\run-tests.ps1 -Filter "CategoryName"

param(
    [Parameter(Mandatory=$false)]
    [string]$Filter = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$backendPath = Join-Path $PSScriptRoot "..\backend"
$testProjectPath = Join-Path $backendPath "ContractReviewScheduler.Tests"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Integration Tests Runner" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $testProjectPath)) {
    Write-Host "ERROR: Test project not found at $testProjectPath" -ForegroundColor Red
    exit 1
}

Set-Location $testProjectPath

# Build test project
Write-Host "Building test project..." -ForegroundColor Green
& dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "Running integration tests..." -ForegroundColor Green
$testArgs = @("test", "--configuration", "Debug", "--verbosity", "minimal", "--logger", "console;verbosity=minimal")

if ($Filter) {
    $testArgs += @("--filter", $Filter)
    Write-Host "Filter: $Filter" -ForegroundColor Yellow
}

if ($Verbose) {
    $testArgs[-1] = "detailed"
    Write-Host "Verbose mode enabled" -ForegroundColor Yellow
}

Write-Host ""
& dotnet @testArgs
$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($testExitCode -eq 0) {
    Write-Host "All tests passed successfully! ✓" -ForegroundColor Green
} else {
    Write-Host "Some tests failed! ✗" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
