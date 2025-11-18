# Contract Review Scheduler - Project Index

## 📋 Project Overview

A complete contract review scheduler application with:
- **Backend**: ASP.NET Core 8 Web API (C#)
- **Frontend**: React 19 + TypeScript (Vite)
- **Database**: SQL Server 2019+
- **Authentication**: LDAP/Active Directory with JWT

---

## 📁 Directory Structure

```
duotify-membership-v1/
├── backend/                           # ASP.NET Core API
│   ├── Controllers/                   # API endpoints
│   ├── Services/                      # Business logic
│   ├── Models/Domain/                 # Data entities
│   ├── Data/                          # EF Core context
│   ├── Middleware/                    # Middleware
│   ├── HostedServices/                # Background services
│   ├── Program.cs                     # Startup
│   └── appsettings.*.json            # Configuration
│
├── frontend/                          # React Application
│   ├── src/
│   │   ├── api/                      # API client services
│   │   │   ├── client.ts             # Axios config
│   │   │   ├── auth.ts               # Auth endpoints
│   │   │   ├── appointments.ts       # CRUD ops
│   │   │   └── calendar.ts           # Calendar queries
│   │   ├── components/               # React components
│   │   │   ├── Auth/
│   │   │   │   ├── Login.tsx
│   │   │   │   └── ProtectedRoute.tsx
│   │   │   ├── Calendar/
│   │   │   │   └── CalendarView.tsx
│   │   │   ├── Appointments/
│   │   │   │   ├── CreateAppointment.tsx
│   │   │   │   └── AppointmentDetails.tsx
│   │   │   └── Dashboard/
│   │   │       └── Dashboard.tsx
│   │   ├── store/
│   │   │   └── authStore.ts          # Zustand store
│   │   ├── types/                    # TypeScript types
│   │   │   ├── auth.ts
│   │   │   ├── appointment.ts
│   │   │   └── calendar.ts
│   │   ├── utils/
│   │   │   └── formatting.ts         # Helpers
│   │   ├── styles/                   # CSS files
│   │   │   ├── Auth.css
│   │   │   ├── Calendar.css
│   │   │   ├── AppointmentForm.css
│   │   │   ├── AppointmentDetails.css
│   │   │   └── Dashboard.css
│   │   ├── App.tsx                   # Main app
│   │   ├── main.tsx                  # Entry point
│   │   └── index.css                 # Global styles
│   ├── .env.development              # Dev config
│   ├── .env.production               # Prod config
│   ├── package.json                  # Dependencies
│   ├── vite.config.ts                # Vite config
│   ├── tsconfig.json                 # TS config
│   ├── README.md                     # Frontend guide
│   └── index.html                    # HTML template
│
├── specs/                            # API specifications
├── QUICK_START.md                    # ⭐ Start here
├── FRONTEND_SETUP_GUIDE.md           # Complete setup
├── FRONTEND_COMPLETION.md            # Completion report
└── README.md                         # Project overview
```

---

## 🚀 Getting Started

### Option 1: Quick Start (Recommended)
See **QUICK_START.md** for immediate setup

### Option 2: Detailed Setup
See **FRONTEND_SETUP_GUIDE.md** for comprehensive guide

### Quick Commands

**Backend**
```bash
cd backend
dotnet run
# Runs on https://localhost:5001
```

**Frontend**
```bash
cd frontend
npm install
npm run dev
# Runs on http://localhost:5173
```

---

## 📄 Documentation Files

| File | Purpose |
|------|---------|
| **QUICK_START.md** | Quick reference guide (5 mins read) |
| **FRONTEND_SETUP_GUIDE.md** | Complete setup instructions |
| **FRONTEND_COMPLETION.md** | Detailed completion report |
| **frontend/README.md** | Frontend project documentation |
| **backend/README.md** | Backend project documentation |

---

## 🎯 Key Features

### ✅ Implemented
- User Authentication (AD/LDAP + JWT)
- Interactive Calendar View
- Appointment Creation & Management
- Dashboard with Filtering
- Protected Routes
- Responsive Design
- Error Handling
- Loading States
- Token Management

---

## 🛠 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server 2019+
- **Authentication**: LDAP + JWT
- **ORM**: Entity Framework Core
- **Logging**: Serilog

### Frontend
- **Framework**: React 19.2.0
- **Language**: TypeScript 5.9
- **Build Tool**: Vite 7.2
- **Routing**: React Router 7.9
- **State**: Zustand 5.0
- **HTTP**: Axios 1.13
- **Calendar**: React Calendar 6.0

---

## 📊 Component Summary

### React Components (6)
1. **Login** - Authentication form
2. **ProtectedRoute** - Route protection
3. **CalendarView** - Date/time selection
4. **CreateAppointment** - Appointment form
5. **AppointmentDetails** - Details & management
6. **Dashboard** - Main hub

### API Services (4)
1. **auth.ts** - Authentication
2. **appointments.ts** - CRUD operations
3. **calendar.ts** - Availability
4. **client.ts** - HTTP configuration

### Type Definitions (3)
1. **auth.ts** - Auth types
2. **appointment.ts** - Appointment models
3. **calendar.ts** - Calendar types

### Styles (5)
1. **Auth.css** - Login page
2. **Calendar.css** - Calendar view
3. **AppointmentForm.css** - Forms
4. **AppointmentDetails.css** - Details
5. **Dashboard.css** - Dashboard

---

## 🔌 API Integration

### Authentication Endpoints
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Current user
- `POST /api/auth/logout` - Logout
- `POST /api/auth/verify-token` - Token validation

### Appointment Endpoints
- `POST /api/appointments` - Create
- `GET /api/appointments/{id}` - Get details
- `PUT /api/appointments/{id}/accept` - Accept
- `PUT /api/appointments/{id}/reject` - Reject

### Calendar Endpoints
- `GET /api/calendar/{reviewerId}/{date}` - Available slots

---

## 🔒 Security Features

✅ JWT Token Authentication  
✅ LDAP Credential Validation  
✅ Protected Routes  
✅ Automatic Token Expiration  
✅ Secure Token Storage (localStorage)  
✅ CORS Policy  
✅ Input Validation  

---

## 📱 Responsive Design

- ✅ Desktop (1920px+)
- ✅ Tablet (768px - 1024px)
- ✅ Mobile (320px - 767px)
- ✅ Touch-friendly UI
- ✅ Accessible form controls

---

## 🧪 Testing

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

---

## 🚢 Deployment

### Production Build

**Backend**
```bash
cd backend
dotnet publish -c Release -o ./publish
```

**Frontend**
```bash
cd frontend
npm run build
# Output: dist/ directory
```

### Supported Platforms
- Azure App Service
- AWS EC2
- Docker
- Heroku
- Self-hosted servers
- Static hosting (frontend only)

---

## 🔧 Configuration

### Backend Settings
File: `backend/appsettings.Development.json`
- SQL Server connection
- JWT secrets
- LDAP configuration
- Email SMTP settings

### Frontend Settings
File: `frontend/.env.development`
- API Base URL
- Environment mode

---

## 📈 File Statistics

| Category | Count |
|----------|-------|
| React Components | 6 |
| TypeScript Files | 22 |
| CSS Stylesheets | 5 |
| Type Definitions | 3 |
| API Services | 4 |
| Config Files | 2 |
| Documentation Files | 3 |
| **Total** | **45+** |

---

## ✨ Key Highlights

✨ **Type-Safe**: Full TypeScript coverage  
✨ **Modern UI**: React 19 with responsive design  
✨ **Fast Build**: Vite for instant HMR  
✨ **State Management**: Zustand for simplicity  
✨ **API Integration**: Axios with interceptors  
✨ **Production Ready**: Error handling & loading states  

---

## 🚦 Status

### ✅ FRONTEND: COMPLETE
- All components implemented
- Full API integration
- Responsive design
- Documentation complete
- Ready for production

### ✅ BACKEND: COMPLETE
- API endpoints working
- Database configured
- LDAP authentication ready
- Email notifications functional

### ✅ DOCUMENTATION: COMPLETE
- Setup guides written
- API documentation ready
- Component documentation included
- Quick start guide available

---

## 📞 Support

### Troubleshooting
1. Check QUICK_START.md for common issues
2. Review FRONTEND_SETUP_GUIDE.md for detailed help
3. Check browser console (F12) for errors
4. Check backend logs in `logs/` directory

### Additional Resources
- Frontend README: `frontend/README.md`
- Backend README: `backend/README.md`
- API Swagger: `https://localhost:5001/swagger` (when running)

---

## 🎓 Learning Resources

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Vite Guide](https://vitejs.dev/guide/)
- [Zustand Documentation](https://zustand-demo.vercel.app/)
- [React Router Guide](https://reactrouter.com/)

---

## 📝 License

Proprietary - Internal Use Only

---

## ✅ Verification Checklist

Before deploying:
- [ ] Backend running on https://localhost:5001
- [ ] Frontend running on http://localhost:5173
- [ ] Can login with AD credentials
- [ ] Dashboard displays appointments
- [ ] Can create new appointment
- [ ] Can view appointment details
- [ ] Responsive on mobile devices
- [ ] All error messages display correctly

---

## 🎉 Ready to Start?

1. Read **QUICK_START.md** (5 minutes)
2. Follow setup instructions
3. Test the application
4. Deploy to production

**Happy scheduling! 📅**
