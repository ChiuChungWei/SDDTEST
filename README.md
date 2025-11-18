# Contract Review Scheduler - Backend Documentation

## Overview

The Contract Review Scheduler is an ASP.NET Core Web API application built with clean architecture principles. It manages appointment scheduling for contract reviews with features like:

- **User Authentication & Authorization**: LDAP/Active Directory integration with JWT tokens
- **Appointment Management**: Create, accept, reject, and cancel appointments
- **Calendar Management**: View available time slots for reviewers
- **Leave Management**: Handle reviewer leave schedules
- **Conflict Detection**: Prevent double-booking with intelligent conflict detection
- **Email Notifications**: Notify users of appointment status changes
- **Comprehensive Logging**: Structured logging with Serilog

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server 2019+
- **Authentication**: LDAP (Active Directory) + JWT Bearer
- **Logging**: Serilog with file rolling
- **Cache**: IMemoryCache
- **ORM**: Entity Framework Core 8.0

## Project Structure

```
backend/
├── Controllers/
│   ├── AuthController.cs          - Authentication endpoints
│   ├── AppointmentsController.cs   - Appointment CRUD operations
│   ├── CalendarController.cs       - Calendar availability
│   └── LeaveSchedulesController.cs - Leave schedule management
├── Services/
│   ├── LdapService.cs             - Active Directory integration
│   ├── JwtService.cs              - JWT token generation & validation
│   ├── AuthenticationService.cs    - Authentication logic
│   ├── AppointmentService.cs       - Appointment business logic
│   ├── ConflictDetectionService.cs - Conflict detection algorithm
│   ├── EmailService.cs            - Email notifications
│   ├── UserSyncService.cs         - User synchronization from AD
│   └── CacheService.cs            - Memory caching utility
├── Models/Domain/
│   ├── User.cs                    - User entity
│   ├── Appointment.cs             - Appointment entity
│   ├── LeaveSchedule.cs           - Leave schedule entity
│   ├── AppointmentHistory.cs      - Audit trail
│   └── NotificationLog.cs         - Email notification log
├── Data/
│   └── ApplicationDbContext.cs     - EF Core DbContext
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs  - Global exception handling
│   └── RoleAuthorizationMiddleware.cs  - Role-based authorization
├── appsettings.json               - Production configuration
├── appsettings.Development.json    - Development configuration
└── Program.cs                     - Application startup

```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Active Directory/LDAP server (or configure for testing)

### Installation

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd backend
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Settings**
   - Update `appsettings.Development.json` with your SQL Server connection string
   - Configure LDAP settings for your Active Directory
   - Set JWT secret key (min 32 characters)
   - Configure email SMTP settings

4. **Setup Database**
   ```bash
   # Create database and apply migrations
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001`

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ContractReviewScheduler;..."
  },
  "Jwt": {
    "Key": "your-32-character-minimum-secret-key",
    "Issuer": "ContractReviewScheduler",
    "Audience": "ContractReviewSchedulerClient",
    "ExpirationMinutes": 60
  },
  "Ldap": {
    "Path": "LDAP://company.local",
    "Domain": "company.local",
    "ReviewerGroup": "ReviewersGroup"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromAddress": "noreply@company.local",
    "FromName": "Contract Review System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "UseSSL": true,
    "MaxRetries": 3,
    "RetryDelaySeconds": 60
  }
}
```

## API Endpoints

### Authentication

```
POST /api/auth/login                    - User login
POST /api/auth/logout                   - User logout
POST /api/auth/verify-token             - Verify JWT token
GET  /api/auth/me                       - Get current user info
```

### Appointments

```
POST /api/appointments                  - Create new appointment
GET  /api/appointments/{id}             - Get appointment details
PUT  /api/appointments/{id}/accept      - Accept appointment
PUT  /api/appointments/{id}/reject      - Reject appointment
```

### Calendar

```
GET /api/calendar/{reviewerId}/{date}   - Get available time slots
```

### Leave Schedules

```
POST /api/leave-schedules               - Create leave schedule
GET  /api/leave-schedules/{id}          - Get leave schedule
DELETE /api/leave-schedules/{id}        - Delete leave schedule
GET  /api/leave-schedules/reviewer/{id} - List reviewer's leaves
```

## Data Models

### User
- Stores user information synchronized from Active Directory
- Roles: `applicant`, `reviewer`
- Tracks login history and account status

### Appointment
- Core entity for scheduling appointments
- States: `pending`, `accepted`, `rejected`, `delegated`, `cancelled`
- Tracks applicant, reviewer, and delegate relationships
- Maintains audit history through AppointmentHistory

### LeaveSchedule
- Reviewer leave periods
- Prevents appointments during leave
- Granular time-based (15-minute slots)

### AppointmentHistory
- Immutable audit trail
- Tracks all state changes with timestamp and actor
- Enables compliance and dispute resolution

### NotificationLog
- Email delivery tracking
- Retry mechanism for failed sends
- Notification type classification

## Key Features

### 1. Smart Conflict Detection
- Detects overlapping appointments
- Checks reviewer leave schedules
- Identifies weekend/holiday conflicts
- Prevents double-booking with 15-minute slot granularity

### 2. Email Notifications
- Automatic notifications on appointment creation
- Confirmation/rejection notifications
- HTML-formatted emails with appointment details
- Retry logic with configurable delays

### 3. User Synchronization
- Automatic sync from Active Directory
- Role detection from AD groups
- Cached user information (1-hour TTL)
- Email lookup and validation

### 4. Authentication & Authorization
- JWT Bearer token authentication
- Role-based access control (RBAC)
- LDAP credential validation
- Automatic user provisioning on first login

### 5. Comprehensive Logging
- Structured logging with Serilog
- File-based rolling logs (daily)
- Business event tracking
- Error and exception logging

## Development Workflows

### Creating an Appointment

```csharp
POST /api/appointments
{
  "reviewerId": 2,
  "date": "2025-11-20",
  "startTime": "09:00:00",
  "endTime": "10:00:00",
  "objectName": "Contract ABC-2025"
}
```

### Checking Availability

```csharp
GET /api/calendar/2/2025-11-20
```

Response includes all 15-minute available slots.

### Managing Leave

```csharp
POST /api/leave-schedules
{
  "date": "2025-11-25",
  "startTime": "09:00:00",
  "endTime": "17:00:00"
}
```

## Testing

### Unit Tests
Run the test suite:
```bash
dotnet test
```

### Integration Tests
The API uses in-memory cache and can be tested against a test database.

## Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

View pending migrations:
```bash
dotnet ef migrations list
```

## Error Handling

All endpoints return standardized error responses:

```json
{
  "error": "Description of the error",
  "timestamp": "2025-11-18T10:30:00Z",
  "status": 400
}
```

HTTP Status Codes:
- `200` - OK
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

## Logging

Logs are written to:
- **Console** (Development only)
- **File** (`logs/log-YYYY-MM-DD.txt`)

Configure log levels in `appsettings.json`:
```json
"Serilog": {
  "MinimumLevel": "Information"
}
```

## Security Considerations

1. **JWT Secret Key**: Must be at least 32 characters
2. **HTTPS**: Always use HTTPS in production
3. **CORS**: Restricted to specific origins (configurable)
4. **SQL Injection**: Protected by EF Core parameterized queries
5. **Authentication**: LDAP credentials validated server-side
6. **Authorization**: Role-based access control enforced

## Performance Optimization

1. **Caching Strategy**:
   - User information cached for 1 hour
   - Reviewer list cached for 1 hour
   - Database indexes on frequently queried columns

2. **Database Indexes**:
   - Composite index on (ReviewerId, Date, TimeStart, TimeEnd)
   - Index on ApplicantId and Status for queries

3. **Query Optimization**:
   - Use `.AsQueryable()` for deferred execution
   - Include related entities with `.Include()`
   - Pagination support for large result sets

## Troubleshooting

### Connection String Issues
- Verify SQL Server is running
- Check authentication (Integrated Security vs. SQL Auth)
- Ensure database exists or enable auto-creation

### LDAP Connection Failures
- Verify LDAP path and domain
- Check network connectivity to AD server
- Validate user credentials
- Check firewall rules (port 389/636)

### Email Sending Fails
- Verify SMTP server settings
- Check email credentials
- Ensure app passwords for Gmail
- Verify firewall allows SMTP ports (587/465)

### Token Expiration
- JWT tokens expire after configured minutes (default: 60)
- Client should handle 401 responses and re-authenticate
- Token refresh not yet implemented (future enhancement)

## Future Enhancements

- [ ] Token refresh endpoint
- [ ] Appointment delegation workflow
- [ ] Batch operations API
- [ ] Advanced filtering and search
- [ ] GraphQL API layer
- [ ] Background job processing (Hangfire)
- [ ] Multi-tenancy support
- [ ] API rate limiting

## Contributing

1. Follow clean architecture principles
2. Add unit tests for new features
3. Update documentation for API changes
4. Use meaningful commit messages

## License

Proprietary - Internal Use Only
