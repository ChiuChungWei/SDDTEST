import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { formatDateTime, getStatusBadgeColor, formatTime } from '../../utils/formatting';
import '../../styles/Dashboard.css';

interface MockAppointment {
  id: number;
  objectName: string;
  applicantName: string;
  reviewerName: string;
  date: Date;
  timeStart: string;
  timeEnd: string;
  status: string;
}

export const Dashboard: React.FC = () => {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [appointments, setAppointments] = useState<MockAppointment[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Load mock appointments for demonstration
    // In production, these would come from the API
    const mockAppointments: MockAppointment[] = [
      {
        id: 1,
        objectName: 'Contract ABC-2025',
        applicantName: 'John Doe',
        reviewerName: 'Jane Smith',
        date: new Date(2025, 10, 25),
        timeStart: '09:00:00',
        timeEnd: '10:00:00',
        status: 'pending',
      },
      {
        id: 2,
        objectName: 'Contract XYZ-2025',
        applicantName: 'John Doe',
        reviewerName: 'Jane Smith',
        date: new Date(2025, 10, 26),
        timeStart: '10:00:00',
        timeEnd: '11:00:00',
        status: 'accepted',
      },
    ];
    setAppointments(mockAppointments);
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const upcomingAppointments = appointments.filter(
    (apt) => new Date(apt.date) > new Date()
  );

  const pastAppointments = appointments.filter(
    (apt) => new Date(apt.date) <= new Date()
  );

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-content">
          <h1>Contract Review Scheduler</h1>
          <div className="user-info">
            <span className="user-name">{user?.name}</span>
            <span className="user-role">{user?.role}</span>
            <button className="logout-button" onClick={handleLogout}>
              Logout
            </button>
          </div>
        </div>
      </header>

      <main className="dashboard-main">
        <div className="dashboard-section">
          <div className="section-header">
            <h2>Upcoming Appointments</h2>
            <button
              className="create-button"
              onClick={() => navigate('/create-appointment')}
            >
              + Create Appointment
            </button>
          </div>

          {upcomingAppointments.length > 0 ? (
            <div className="appointments-table">
              <table>
                <thead>
                  <tr>
                    <th>Contract Object</th>
                    <th>Applicant</th>
                    <th>Reviewer</th>
                    <th>Date & Time</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {upcomingAppointments.map((apt) => (
                    <tr key={apt.id}>
                      <td className="object-name">{apt.objectName}</td>
                      <td>{apt.applicantName}</td>
                      <td>{apt.reviewerName}</td>
                      <td>
                        {formatDateTime(apt.date)} {formatTime(apt.timeStart)}-
                        {formatTime(apt.timeEnd)}
                      </td>
                      <td>
                        <span
                          className="status-badge"
                          style={{
                            backgroundColor: getStatusBadgeColor(apt.status),
                          }}
                        >
                          {apt.status}
                        </span>
                      </td>
                      <td>
                        <button
                          className="view-button"
                          onClick={() => navigate(`/appointment/${apt.id}`)}
                        >
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="empty-state">
              <p>No upcoming appointments</p>
              <button
                className="create-button-secondary"
                onClick={() => navigate('/create-appointment')}
              >
                Create your first appointment
              </button>
            </div>
          )}
        </div>

        {pastAppointments.length > 0 && (
          <div className="dashboard-section">
            <h2>Past Appointments</h2>
            <div className="appointments-table">
              <table>
                <thead>
                  <tr>
                    <th>Contract Object</th>
                    <th>Applicant</th>
                    <th>Reviewer</th>
                    <th>Date & Time</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {pastAppointments.map((apt) => (
                    <tr key={apt.id} className="past-appointment">
                      <td className="object-name">{apt.objectName}</td>
                      <td>{apt.applicantName}</td>
                      <td>{apt.reviewerName}</td>
                      <td>
                        {formatDateTime(apt.date)} {formatTime(apt.timeStart)}-
                        {formatTime(apt.timeEnd)}
                      </td>
                      <td>
                        <span
                          className="status-badge"
                          style={{
                            backgroundColor: getStatusBadgeColor(apt.status),
                          }}
                        >
                          {apt.status}
                        </span>
                      </td>
                      <td>
                        <button
                          className="view-button"
                          onClick={() => navigate(`/appointment/${apt.id}`)}
                        >
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </main>
    </div>
  );
};
