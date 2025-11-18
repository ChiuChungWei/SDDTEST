import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import { Login } from './components/Auth/Login';
import { ProtectedRoute } from './components/Auth/ProtectedRoute';
import { Dashboard } from './components/Dashboard/Dashboard';
import { CreateAppointment } from './components/Appointments/CreateAppointment';
import { AppointmentDetails } from './components/Appointments/AppointmentDetails';
import './App.css';

function App() {
  const { loadFromStorage } = useAuthStore();

  useEffect(() => {
    loadFromStorage();
  }, [loadFromStorage]);

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/create-appointment"
          element={
            <ProtectedRoute>
              <CreateAppointment />
            </ProtectedRoute>
          }
        />
        <Route
          path="/appointment/:id"
          element={
            <ProtectedRoute>
              <AppointmentDetails />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </Router>
  );
}

export default App;
