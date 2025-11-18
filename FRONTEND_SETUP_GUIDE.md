# Contract Review Scheduler - Full Stack Deployment Guide

## Overview

This project consists of:
- **Backend**: ASP.NET Core 8 Web API (C#)
- **Frontend**: React 19 + TypeScript (Vite)
- **Database**: SQL Server 2019+
- **Authentication**: LDAP/Active Directory with JWT

## Project Structure

```
duotify-membership-v1/
├── backend/                    # ASP.NET Core API
│   ├── Controllers/           # API endpoints
│   ├── Services/              # Business logic
│   ├── Models/                # Domain entities
│   ├── Data/                  # Database context
│   ├── appsettings.json       # Configuration
│   └── Program.cs             # Startup
├── frontend/                  # React application
│   ├── src/
│   │   ├── api/              # API client
│   │   ├── components/       # React components
│   │   ├── store/            # State management
│   │   └── App.tsx           # Main app
│   ├── package.json          # Dependencies
│   └── .env.development      # Environment config
└── specs/                     # API specifications
```

## Backend Setup

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server 2019 or later
- LDAP/Active Directory server

### Installation Steps

1. **Navigate to backend directory**
   ```bash
   cd backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure settings** (`appsettings.Development.json`)
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ContractReviewScheduler;Integrated Security=true;"
     },
     "Jwt": {
       "Key": "your-secret-key-minimum-32-characters-long",
       "Issuer": "ContractReviewScheduler",
       "Audience": "ContractReviewSchedulerClient",
       "ExpirationMinutes": 60
     },
     "Ldap": {
       "Path": "LDAP://your-ad-server",
       "Domain": "your-domain.local",
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

4. **Create and apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   Backend API will be available at `https://localhost:5001`

### API Endpoints

**Authentication**
- `POST /api/auth/login` - Login with AD account
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Get current user
- `POST /api/auth/verify-token` - Verify JWT token

**Appointments**
- `POST /api/appointments` - Create appointment
- `GET /api/appointments/{id}` - Get appointment details
- `PUT /api/appointments/{id}/accept` - Accept appointment
- `PUT /api/appointments/{id}/reject` - Reject appointment

**Calendar**
- `GET /api/calendar/{reviewerId}/{date}` - Get available slots

**Leave Schedules**
- `POST /api/leave-schedules` - Create leave
- `GET /api/leave-schedules/{id}` - Get leave details
- `DELETE /api/leave-schedules/{id}` - Delete leave
- `GET /api/leave-schedules/reviewer/{id}` - List reviewer leaves

## Frontend Setup

### Prerequisites
- Node.js 18+ and npm/yarn
- Backend API running

### Installation Steps

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Development mode**
   ```bash
   npm run dev
   ```
   
   Application will be available at `http://localhost:5173`

4. **Build for production**
   ```bash
   npm run build
   ```

5. **Preview production build**
   ```bash
   npm run preview
   ```

### Environment Configuration

**Development** (`.env.development`):
```
VITE_API_BASE_URL=https://localhost:5001/api
```

**Production** (`.env.production`):
```
VITE_API_BASE_URL=https://api.yourdomain.com/api
```

## Running Full Stack

### Option 1: Terminal Windows

**Terminal 1 - Backend**
```bash
cd backend
dotnet run
# Runs on https://localhost:5001
```

**Terminal 2 - Frontend**
```bash
cd frontend
npm run dev
# Runs on http://localhost:5173
```

### Option 2: Docker Compose

Create `docker-compose.yml` at project root:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  backend:
    build: ./backend
    ports:
      - "5001:5001"
      - "5000:5000"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ContractReviewScheduler;User Id=sa;Password=YourPassword123!;"
    depends_on:
      - sqlserver

  frontend:
    build: ./frontend
    ports:
      - "3000:3000"
    environment:
      VITE_API_BASE_URL: http://backend:5001/api
    depends_on:
      - backend

volumes:
  sqlserver_data:
```

Run:
```bash
docker-compose up
```

## Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Lint
```bash
cd frontend
npm run lint
```

## Deployment

### Azure App Service

1. **Publish backend**
   ```bash
   cd backend
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to Azure**
   ```bash
   az webapp deployment source config-zip --resource-group myGroup --name myApp --src publish.zip
   ```

### Heroku (Frontend only)

```bash
heroku create contract-review-scheduler
heroku buildpacks:add heroku/nodejs
git push heroku main
```

### Self-Hosted

1. **Backend**: Host ASP.NET Core with IIS or Kestrel
2. **Frontend**: Serve dist folder with nginx or Apache
3. **Database**: SQL Server instance
4. **SSL**: Configure SSL certificates

## Configuration

### CORS Policy

Update backend's `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### JWT Configuration

Min 32 character secret key:
```bash
# Generate secure key (PowerShell)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

### Email Configuration

For Gmail:
1. Enable 2FA on Google Account
2. Generate app-specific password
3. Use in `appsettings.json`

## Monitoring & Logging

### Backend Logs
- Location: `logs/log-*.txt`
- Format: Structured Serilog output
- Retention: Daily rolling

### Frontend Console
- Check browser DevTools for errors
- Monitor Network tab for API calls

## Troubleshooting

### Backend Won't Start
- Check SQL Server connection
- Verify LDAP server accessibility
- Review logs for detailed errors
- Ensure JWT key is configured

### Frontend Won't Load
- Check API base URL in `.env`
- Verify CORS policy
- Clear browser cache
- Check Network tab for 401/403 errors

### Login Fails
- Verify AD credentials
- Check LDAP connectivity
- Review backend logs
- Ensure user exists in database

### Calendar/Appointments Not Loading
- Check API response in Network tab
- Verify reviewer ID is valid
- Check database has data
- Ensure token hasn't expired

## Performance Optimization

### Backend
- Enable response compression
- Use database connection pooling
- Configure appropriate log levels
- Add caching where appropriate

### Frontend
- Enable code splitting
- Optimize bundle size
- Use React lazy loading
- Configure CDN for assets

## Security Best Practices

1. **Secrets Management**
   - Never commit secrets
   - Use environment variables
   - Consider Azure Key Vault

2. **HTTPS**
   - Always use HTTPS in production
   - Configure proper SSL certificates

3. **Database**
   - Use strong SQL passwords
   - Enable SQL Server encryption
   - Regular backups

4. **API**
   - Validate all inputs
   - Implement rate limiting
   - Use CORS appropriately
   - Enable HTTPS only

## Maintenance

### Regular Tasks
- Monitor disk space
- Review logs for errors
- Update dependencies
- Backup database

### Updates
- Test updates in staging first
- Plan maintenance windows
- Keep .NET and Node updated
- Review security advisories

## Support & Documentation

- Backend API docs: `https://localhost:5001/swagger`
- Frontend source: `frontend/src/`
- Database schema: Check EF migrations
- API specifications: `specs/` directory

## License

Proprietary - Internal Use Only
