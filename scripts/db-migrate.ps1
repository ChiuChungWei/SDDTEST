# Contract Review Scheduler - Database Migration Script
# Purpose: Initialize and migrate the SQL Server database
# Usage: .\scripts\db-migrate.ps1 -Environment Development

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Development",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = ""
)

$ErrorActionPreference = "Stop"
$backendPath = Join-Path $PSScriptRoot "..\backend"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Migration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

# Navigate to backend directory
if (-not (Test-Path $backendPath)) {
    Write-Host "ERROR: Backend directory not found at $backendPath" -ForegroundColor Red
    exit 1
}

Set-Location $backendPath

# Check if dotnet-ef is installed
Write-Host "Checking for dotnet-ef CLI..." -ForegroundColor Green
$efVersion = & dotnet-ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ dotnet-ef found: $efVersion" -ForegroundColor Green
} else {
    Write-Host "Installing dotnet-ef CLI..." -ForegroundColor Yellow
    & dotnet tool install --global dotnet-ef --version 8.0.18
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install dotnet-ef" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ dotnet-ef installed" -ForegroundColor Green
}

# Build the project
Write-Host "`nBuilding project..." -ForegroundColor Green
& dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green

# Apply migrations
Write-Host "`nApplying database migrations..." -ForegroundColor Green
& dotnet-ef database update --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Migration failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Migrations applied successfully" -ForegroundColor Green

# List applied migrations
Write-Host "`nApplied migrations:" -ForegroundColor Green
& dotnet-ef migrations list --configuration Release

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Database migration completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
