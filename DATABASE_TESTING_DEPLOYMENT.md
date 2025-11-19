# Database, Testing & Deployment Guide

Complete guide for managing database migrations, running integration tests, and deploying the Contract Review Scheduler backend.

## 📋 Table of Contents

1. [Database Setup & Migrations](#database-setup--migrations)
2. [Integration Tests](#integration-tests)
3. [Deployment](#deployment)
4. [Scripts Reference](#scripts-reference)
5. [Troubleshooting](#troubleshooting)

---

## Database Setup & Migrations

### Initial Setup

The project uses **Entity Framework Core** with **SQL Server** for database management. All database changes are tracked via migrations.

#### Prerequisites
- SQL Server 2019+ running on your machine
- Connection string configured in `appsettings.Development.json`

#### First-Time Setup

```bash
# Navigate to backend directory
cd backend

# Apply initial migration to create database
dotnet ef database update

# Verify migration was applied
dotnet ef migrations list
```

#### Configuration

Edit `backend/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ContractReviewScheduler_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "ContractReviewScheduler",
    "Audience": "ContractReviewSchedulerClient"
  }
}
```

### Creating New Migrations

When you modify domain models, create a new migration:

```bash
cd backend

# Create migration (name your migration descriptively)
dotnet ef migrations add AddNewFeature

# Review the generated migration file
# Located in: backend/Migrations/[timestamp]_AddNewFeature.cs

# Apply the migration
dotnet ef database update
```

### Migration Management

```bash
# List all migrations
dotnet ef migrations list

# Show details of a specific migration
dotnet ef migrations script --from InitialMigration --to AddNewFeature

# Remove the last migration (be careful!)
dotnet ef migrations remove

# Generate SQL script without applying
dotnet ef migrations script --output migration.sql
```

### Database Scripts

#### 1. Migrate Database (`db-migrate.ps1`)

Applies all pending migrations to the database.

```powershell
# Apply migrations with release configuration
.\scripts\db-migrate.ps1 -Environment Release

# Apply migrations with debug configuration
.\scripts\db-migrate.ps1 -Environment Development
```

#### 2. Reset Database (`db-reset.ps1`)

Drops and recreates the database from scratch (development only).

```powershell
# Reset with confirmation prompt
.\scripts\db-reset.ps1

# Force reset without confirmation
.\scripts\db-reset.ps1 -Force
```

---

## Integration Tests

### Overview

Integration tests verify that database operations, services, and APIs work correctly together. Tests use an in-memory database for isolation.

### Test Structure

Tests are located in:
```
backend/ContractReviewScheduler.Tests/
├── IntegrationTests/
│   ├── DatabaseIntegrationTests.cs
│   └── AppointmentServiceIntegrationTests.cs
```

### Writing Tests

Example test template:

```csharp
using Xunit;
using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Data;
using ContractReviewScheduler.Models;

namespace ContractReviewScheduler.Tests.IntegrationTests;

public class YourFeatureTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public YourFeatureTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task YourTest_WithScenario_ShouldExpectBehavior()
    {
        // Arrange
        var testData = new TestEntity { /* ... */ };
        _context.Add(testData);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.Entities.FindAsync(testData.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testData.Id, result.Id);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### Running Tests

```powershell
# Run all tests
.\scripts\run-tests.ps1

# Run specific test class
.\scripts\run-tests.ps1 -Filter "DatabaseIntegrationTests"

# Run with verbose output
.\scripts\run-tests.ps1 -Verbose

# Run directly with dotnet
cd backend\ContractReviewScheduler.Tests
dotnet test --configuration Debug
```

### Test Coverage

Current test coverage includes:

- **Database Operations**: User creation, appointment persistence, leave schedules
- **Service Integration**: Appointment service with mocked dependencies
- **Data Integrity**: Multi-entity relationships and referential integrity

### CI/CD Integration

Tests run automatically before deployment:

```bash
# In deployment pipeline
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

---

## Deployment

### Deployment Process

The deployment process includes:
1. Build the project
2. Run integration tests
3. Publish to output directory
4. Create deployment metadata

### Using Deployment Scripts

#### Full Backend Deployment (`deploy-backend.ps1`)

```powershell
# Build for Development
.\scripts\deploy-backend.ps1 -Environment Development

# Build for Production
.\scripts\deploy-backend.ps1 -Environment Production

# Build only (skip tests)
.\scripts\deploy-backend.ps1 -Environment Production -BuildOnly

# Skip tests
.\scripts\deploy-backend.ps1 -Environment Production -SkipTests

# Custom output path
.\scripts\deploy-backend.ps1 -Environment Production -OutputPath "C:\deploy\app"
```

#### Setup Development Environment (`setup-dev-environment.ps1`)

Complete one-time setup for development:

```powershell
.\scripts\setup-dev-environment.ps1
```

This script:
- Verifies Node.js and .NET installation
- Restores backend dependencies
- Restores frontend dependencies
- Builds backend
- Initializes database
- Installs EF Core Tools

### Production Deployment Steps

1. **Prepare Release Build**

```powershell
.\scripts\deploy-backend.ps1 -Environment Production -OutputPath "C:\builds\release"
```

2. **Review Deployment Info**

```
C:\builds\release\deployment-info.json
```

3. **Transfer Files**

Copy the contents of the publish folder to your server.

4. **Configure Settings**

Update `appsettings.Production.json` on the server with:
- SQL Server connection string
- JWT keys
- LDAP/Active Directory settings
- Email SMTP configuration

5. **Run Application**

```bash
dotnet ContractReviewScheduler.dll
```

### Environment-Specific Configuration

#### Development (`appsettings.Development.json`)
- Local SQL Server instance
- Test data enabled
- Detailed logging
- CORS: Allow all

#### Production (`appsettings.Production.json`)
- Production SQL Server with backups
- Logging to file only
- Restricted CORS
- HTTPS required
- Performance optimizations

### Health Checks

```bash
# Check API is running
curl https://localhost:5001/health

# Check database connection
curl https://localhost:5001/api/auth/me -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Scripts Reference

### Database Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| `db-migrate.ps1` | Apply pending migrations | `.\scripts\db-migrate.ps1 -Environment Development` |
| `db-reset.ps1` | Drop and recreate database | `.\scripts\db-reset.ps1 -Force` |

### Testing Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| `run-tests.ps1` | Run integration tests | `.\scripts\run-tests.ps1 -Verbose` |

### Deployment Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| `deploy-backend.ps1` | Full deployment pipeline | `.\scripts\deploy-backend.ps1 -Environment Production` |
| `setup-dev-environment.ps1` | First-time dev setup | `.\scripts\setup-dev-environment.ps1` |

### Script Parameters

All scripts support:

```powershell
# Get script help
Get-Help .\scripts\script-name.ps1 -Detailed

# List available parameters
Get-Help .\scripts\script-name.ps1 -Parameter *
```

---

## Troubleshooting

### Database Issues

#### Error: "Cannot connect to SQL Server"

```powershell
# Verify SQL Server is running
Get-Service MSSQL* | fl DisplayName, Status

# Check connection string
# Verify server name, database name, authentication method

# Test connection manually
sqlcmd -S YOUR_SERVER -U sa -P YOUR_PASSWORD
```

#### Error: "Migration failed"

```bash
# Verify migration syntax
dotnet ef migrations list

# Rollback to previous migration
dotnet ef database update PREVIOUS_MIGRATION

# Remove failed migration
dotnet ef migrations remove
```

#### Recreate Database

```powershell
# Full reset
.\scripts\db-reset.ps1 -Force

# Or manually
cd backend
dotnet ef database drop -f
dotnet ef database update
```

### Test Issues

#### Tests Not Found

```bash
cd backend/ContractReviewScheduler.Tests
dotnet test --logger "console;verbosity=detailed"
```

#### Test Timeout

Increase timeout in test project:

```xml
<!-- In ContractReviewScheduler.Tests.csproj -->
<PropertyGroup>
  <TestRunTimeout>60000</TestRunTimeout>
</PropertyGroup>
```

### Deployment Issues

#### Build Fails

```powershell
# Clean build
cd backend
dotnet clean
dotnet build --configuration Release

# Check for compilation errors
dotnet build -v d
```

#### Migration Fails During Deploy

```bash
# Apply migrations manually on server
cd /path/to/app
dotnet ContractReviewScheduler.dll

# Then run migrations
dotnet ef database update
```

### Common Commands

```bash
# Full clean rebuild
dotnet clean && dotnet build --configuration Release

# Run with detailed logging
dotnet run --configuration Debug --verbosity diag

# Generate migration SQL script
dotnet ef migrations script --output migration.sql

# Check EF Core version
dotnet ef --version

# Reinstall EF Core tools
dotnet tool uninstall -g dotnet-ef
dotnet tool install -g dotnet-ef --version 8.0.18
```

---

## Best Practices

### Migration Guidelines

✅ **Do:**
- Create one migration per logical change
- Use descriptive migration names
- Test migrations locally before deploying
- Keep migration files in version control

❌ **Don't:**
- Modify migration files after they're shared
- Skip migrations in production
- Use manual SQL migrations alongside EF Core

### Testing Guidelines

✅ **Do:**
- Write tests for new features
- Use in-memory database for unit/integration tests
- Mock external dependencies
- Run tests before committing

❌ **Don't:**
- Use production database in tests
- Skip tests for deployment
- Ignore test failures

### Deployment Guidelines

✅ **Do:**
- Test deployment process in staging first
- Backup database before production deployment
- Keep deployment scripts in source control
- Document any manual steps

❌ **Don't:**
- Deploy directly to production without testing
- Skip backups
- Deploy during peak usage hours without monitoring

---

## Support & Documentation

- **EF Core Docs**: https://learn.microsoft.com/ef/core/
- **xUnit Documentation**: https://xunit.net/
- **PowerShell Guide**: https://learn.microsoft.com/powershell/

For issues or questions, check backend logs in `backend/logs/` directory.
