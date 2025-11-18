# Contract Review Scheduler - Frontend

A modern, responsive React and TypeScript web application for managing contract review appointments.

## Features

- **User Authentication**: Secure login with AD/LDAP credentials via JWT tokens
- **Calendar View**: Interactive calendar for selecting appointment dates and available time slots
- **Appointment Management**: Create, view, and manage contract review appointments
- **Dashboard**: Overview of upcoming and past appointments
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices

## Tech Stack

- **Framework**: React 19 with TypeScript
- **Build Tool**: Vite
- **Routing**: React Router v7
- **State Management**: Zustand
- **HTTP Client**: Axios

## Getting Started

### Install and Run

`bash
cd frontend
npm install
npm run dev
`

Application will be available at \http://localhost:5173\

### Build for Production

`bash
npm run build
`

## Configuration

Frontend communicates with backend at \https://localhost:5001/api\

Update \.env.development\ or \.env.production\ if needed.

## Structure

- \src/api/\ - API client services
- \src/components/\ - React components
- \src/store/\ - Zustand state management
- \src/styles/\ - CSS stylesheets
- \src/types/\ - TypeScript type definitions
- \src/utils/\ - Utility functions

## License

Proprietary - Internal Use Only
