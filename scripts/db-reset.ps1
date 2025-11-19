# Contract Review Scheduler - Database Reset Script
# Purpose: Drop and recreate the database (Development only)
# Usage: .\scripts\db-reset.ps1

param(
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

$ErrorActionPreference = "Stop"
$backendPath = Join-Path $PSScriptRoot "..\backend"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Reset Script (Development Only)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $Force) {
    Write-Host "WARNING: This will DELETE all data in the development database!" -ForegroundColor Red
    $confirm = Read-Host "Continue? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }
}

if (-not (Test-Path $backendPath)) {
    Write-Host "ERROR: Backend directory not found at $backendPath" -ForegroundColor Red
    exit 1
}

Set-Location $backendPath

# Check if dotnet-ef is installed
Write-Host "Checking for dotnet-ef CLI..." -ForegroundColor Green
$efVersion = & dotnet-ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Installing dotnet-ef CLI..." -ForegroundColor Yellow
    & dotnet tool install --global dotnet-ef --version 8.0.18
}

# Build the project
Write-Host "`nBuilding project..." -ForegroundColor Green
& dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}

# Drop the database
Write-Host "`nDropping existing database..." -ForegroundColor Yellow
& dotnet-ef database drop --configuration Debug --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "Note: Database may not have existed or is already dropped" -ForegroundColor Yellow
}

# Apply migrations to create fresh database
Write-Host "`nCreating fresh database..." -ForegroundColor Green
& dotnet-ef database update --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create database" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Fresh database created" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Database reset completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
