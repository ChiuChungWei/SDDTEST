# Contract Review Scheduler - Full Deployment Script
# Purpose: Complete backend deployment with database migration and tests
# Usage: .\scripts\deploy-backend.ps1 -Environment Production -BuildOnly

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Development",
    
    [Parameter(Mandatory=$false)]
    [switch]$BuildOnly = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$backendPath = Join-Path $rootPath "backend"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

if (-not $OutputPath) {
    $OutputPath = Join-Path $backendPath "publish_$timestamp"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend Deployment Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host "Build Only: $BuildOnly" -ForegroundColor Yellow
Write-Host "Skip Tests: $SkipTests" -ForegroundColor Yellow
Write-Host ""

# Validate backend path
if (-not (Test-Path $backendPath)) {
    Write-Host "ERROR: Backend directory not found at $backendPath" -ForegroundColor Red
    exit 1
}

Set-Location $backendPath

# Run tests if not skipped
if (-not $SkipTests) {
    Write-Host "Running integration tests..." -ForegroundColor Green
    & dotnet test --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Tests failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ All tests passed" -ForegroundColor Green
    Write-Host ""
}

# Build the project
Write-Host "Building project for $Environment..." -ForegroundColor Green
$buildConfig = if ($Environment -eq "Development") { "Debug" } else { "Release" }
& dotnet build --configuration $buildConfig
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
Write-Host ""

if ($BuildOnly) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Build completed (Build-Only mode)" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    exit 0
}

# Publish the project
Write-Host "Publishing project..." -ForegroundColor Green
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

& dotnet publish -c $buildConfig -o $OutputPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Published to $OutputPath" -ForegroundColor Green
Write-Host ""

# Create deployment configuration file
$deployConfig = @{
    Environment = $Environment
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    BuildConfiguration = $buildConfig
    PublishPath = $OutputPath
    Version = "1.0.0"
}

$configPath = Join-Path $OutputPath "deployment-info.json"
$deployConfig | ConvertTo-Json | Out-File -FilePath $configPath -Encoding UTF8
Write-Host "✓ Created deployment info at $configPath" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment preparation completed! ✓" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review deployment info: $configPath" -ForegroundColor White
Write-Host "2. Deploy files from: $OutputPath" -ForegroundColor White
Write-Host "3. Update appsettings.$Environment.json on target server" -ForegroundColor White
Write-Host "4. Run: dotnet ContractReviewScheduler.dll" -ForegroundColor White
