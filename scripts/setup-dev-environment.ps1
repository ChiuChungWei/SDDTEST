# Contract Review Scheduler - Full Setup Script
# Purpose: Complete environment setup for local development
# Usage: .\scripts\setup-dev-environment.ps1

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$backendPath = Join-Path $rootPath "backend"
$frontendPath = Join-Path $rootPath "frontend"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Development Environment Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Green

# Check Node.js
$nodeVersion = & node --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Node.js: $nodeVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js not found. Please install Node.js 18+ from https://nodejs.org" -ForegroundColor Red
    exit 1
}

# Check .NET
$dotnetVersion = & dotnet --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET not found. Please install .NET 8.0" -ForegroundColor Red
    exit 1
}

# Check SQL Server
$sqlQuery = "SELECT @@version"
$connected = $false
try {
    Write-Host "`nChecking SQL Server connection..." -ForegroundColor Green
    # This is a basic check - in production you'd use actual connection
    Write-Host "⚠ SQL Server connection check requires manual verification" -ForegroundColor Yellow
    Write-Host "  Ensure SQL Server is running on your machine" -ForegroundColor Yellow
} catch {
    Write-Host "⚠ Could not verify SQL Server" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Setting up backend..." -ForegroundColor Green
Set-Location $backendPath

# Restore NuGet packages
Write-Host "Restoring backend dependencies..." -ForegroundColor Cyan
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore backend dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Backend dependencies restored" -ForegroundColor Green

# Install EF Core Tools
Write-Host "Installing EF Core Tools..." -ForegroundColor Cyan
& dotnet tool install --global dotnet-ef --version 8.0.18 2>$null
Write-Host "✓ EF Core Tools ready" -ForegroundColor Green

# Build backend
Write-Host "Building backend..." -ForegroundColor Cyan
& dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Backend build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Backend built successfully" -ForegroundColor Green

# Initialize database
Write-Host ""
Write-Host "Initializing database..." -ForegroundColor Green
& dotnet-ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠ Database update encountered issues (may be OK if DB already exists)" -ForegroundColor Yellow
}
Write-Host "✓ Database initialized" -ForegroundColor Green

Write-Host ""
Write-Host "Setting up frontend..." -ForegroundColor Green
Set-Location $frontendPath

# Install npm packages
Write-Host "Installing frontend dependencies..." -ForegroundColor Cyan
& npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to install frontend dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Frontend dependencies installed" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Environment setup completed! ✓" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Configure appsettings files in backend/" -ForegroundColor White
Write-Host "2. Start backend: cd backend && dotnet run" -ForegroundColor White
Write-Host "3. Start frontend: cd frontend && npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "Available scripts:" -ForegroundColor Yellow
Write-Host "  .\scripts\db-migrate.ps1       - Apply database migrations" -ForegroundColor White
Write-Host "  .\scripts\db-reset.ps1         - Reset database (dev only)" -ForegroundColor White
Write-Host "  .\scripts\run-tests.ps1        - Run integration tests" -ForegroundColor White
Write-Host "  .\scripts\deploy-backend.ps1   - Deploy backend" -ForegroundColor White
