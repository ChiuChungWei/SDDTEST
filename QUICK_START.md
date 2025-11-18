# Quick Start Guide - Contract Review Scheduler

## 🚀 Start the Application

### Backend (C# / ASP.NET Core)
```bash
cd backend
dotnet run
# API runs on: https://localhost:5001
# Swagger UI: https://localhost:5001/swagger
```

### Frontend (React / TypeScript)
```bash
cd frontend
npm install          # First time only
npm run dev
# App runs on: http://localhost:5173
# Open browser and login
```

## 🔐 Login Credentials

Use your **Active Directory (AD) account**:
- **Username**: Your AD account (e.g., `domain\username`)
- **Password**: Your AD password

*Note: Credentials are validated against your LDAP/Active Directory server*

## 📱 Main Features

### Dashboard
- View all appointments (upcoming and past)
- Quick appointment creation button
- Click "View" to see appointment details

### Create Appointment
1. Click "+ Create Appointment"
2. Enter Reviewer ID (numeric)
3. Enter Contract Object Name
4. Select date from calendar
5. Choose available time slot
6. Click "Create Appointment"

### Manage Appointment
1. Click "View" on an appointment
2. If you're the reviewer and it's pending:
   - Click "Accept" to approve
   - Click "Reject" to decline with optional reason
3. View full appointment details

## 🛠 Development

### Run Tests
```bash
# Backend tests
cd backend
dotnet test

# Frontend lint
cd frontend
npm run lint
```

### Build Production
```bash
# Backend
cd backend
dotnet publish -c Release

# Frontend
cd frontend
npm run build
# Output: dist/ folder
```

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Get current user
- `POST /api/auth/logout` - Logout

### Appointments
- `POST /api/appointments` - Create
- `GET /api/appointments/{id}` - Get details
- `PUT /api/appointments/{id}/accept` - Accept
- `PUT /api/appointments/{id}/reject` - Reject

### Calendar
- `GET /api/calendar/{reviewerId}/{date}` - Get available slots

## 🔧 Configuration

### Backend Settings
File: `backend/appsettings.Development.json`

Key settings:
```json
{
  "ConnectionStrings.DefaultConnection": "SQL Server connection",
  "Jwt.Key": "Secret key (32+ chars)",
  "Ldap.Path": "LDAP server path",
  "Email.SmtpServer": "SMTP for notifications"
}
```

### Frontend Settings
File: `frontend/.env.development`

```
VITE_API_BASE_URL=https://localhost:5001/api
```

## 🐛 Troubleshooting

### Backend Won't Start
- Check SQL Server is running
- Verify connection string in appsettings
- Ensure LDAP server is accessible
- Check .NET SDK version: `dotnet --version`

### Frontend Won't Load
- Check backend is running (https://localhost:5001)
- Verify API URL in `.env`
- Clear browser cache (Ctrl+Shift+Delete)
- Check browser console for errors

### Login Fails
- Verify AD credentials
- Check LDAP connectivity from backend
- Look at backend logs
- Try a simple test with another AD user

### Calendar/Appointments Not Showing
- Check Network tab for API errors
- Verify JWT token is valid
- Check backend logs
- Ensure database has data

## 📚 Documentation

- **Frontend README**: `frontend/README.md`
- **Backend README**: `backend/README.md`
- **Setup Guide**: `FRONTEND_SETUP_GUIDE.md`
- **Completion Report**: `FRONTEND_COMPLETION.md`

## 🔑 Key Technologies

| Component | Technology |
|-----------|-----------|
| Backend | ASP.NET Core 8 (C#) |
| Frontend | React 19 + TypeScript |
| Build | Vite |
| State | Zustand |
| HTTP | Axios |
| Database | SQL Server |
| Auth | LDAP/JWT |

## 📞 Support

### Check These First
1. Terminal/console output for errors
2. Browser DevTools (F12)
3. Network tab in DevTools
4. Backend Swagger UI

### Debug Commands

```bash
# Check Node version
node --version

# Check .NET version
dotnet --version

# List running processes
Get-Process | grep -E "node|dotnet"

# Check port usage
netstat -ano | findstr :5001
netstat -ano | findstr :5173
```

## ✅ Verification Checklist

- [ ] Backend running on https://localhost:5001
- [ ] Frontend running on http://localhost:5173
- [ ] Can login with AD account
- [ ] Dashboard loads with appointments
- [ ] Can create new appointment
- [ ] Can view appointment details
- [ ] Responsive on mobile (test with DevTools)

## 🚢 Deployment

### Production Build

```bash
# Backend
cd backend
dotnet publish -c Release -o ./publish

# Frontend
cd frontend
npm run build
# dist/ folder ready to deploy
```

### Environment Setup

Update API URL for production:
- Frontend: `.env.production` → `VITE_API_BASE_URL`
- Backend: `appsettings.Production.json` → All settings

## 💡 Tips

- **Hot Reload**: Save files to auto-refresh during development
- **TypeScript**: Full type checking prevents runtime errors
- **Responsive**: Test on mobile with DevTools (F12 → Toggle device toolbar)
- **Logs**: Check `backend/logs/` folder for detailed backend logs

---

**Happy Coding! 🎉**

For detailed information, see the comprehensive guides in the project root.
