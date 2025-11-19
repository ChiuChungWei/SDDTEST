import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { appointmentsApi } from '../../api/appointments';
import { Appointment } from '../../types/appointment';
import {
  formatDateTime,
  getStatusBadgeColor,
  getStatusLabel,
  formatTime,
} from '../../utils/formatting';
import { useAuthStore } from '../../store/authStore';
import '../../styles/AppointmentDetails.css';

export const AppointmentDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [appointment, setAppointment] = useState<Appointment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectForm, setShowRejectForm] = useState(false);
  const navigate = useNavigate();
  const { user } = useAuthStore();

  useEffect(() => {
    if (id) {
      fetchAppointment();
    }
  }, [id]);

  const fetchAppointment = async () => {
    try {
      setLoading(true);
      const data = await appointmentsApi.getById(Number(id));
      setAppointment(data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load appointment');
    } finally {
      setLoading(false);
    }
  };

  const handleAccept = async () => {
    if (!id) return;
    setActionLoading(true);
    try {
      const updated = await appointmentsApi.accept(Number(id));
      setAppointment(updated);
      alert('Appointment accepted successfully!');
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to accept appointment');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!id) return;
    setActionLoading(true);
    try {
      await appointmentsApi.reject(Number(id), rejectReason);
      alert('Appointment rejected successfully!');
      navigate('/dashboard');
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to reject appointment');
    } finally {
      setActionLoading(false);
      setShowRejectForm(false);
    }
  };

  if (loading) {
    return <div className="loading-container">Loading appointment...</div>;
  }

  if (error || !appointment) {
    return (
      <div className="error-container">
        <p>{error || 'Appointment not found'}</p>
        <button onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
      </div>
    );
  }

  const isReviewer = user?.id === appointment.reviewerId;
  const isPending = appointment.status === 'pending';
  const canManage = isReviewer && isPending;

  return (
    <div className="appointment-details-container">
      <button className="back-button" onClick={() => navigate('/dashboard')}>
        ← Back to Dashboard
      </button>

      <div className="appointment-details-card">
        <div className="details-header">
          <h1>{appointment.objectName}</h1>
          <span
            className="status-badge"
            style={{ backgroundColor: getStatusBadgeColor(appointment.status) }}
          >
            {getStatusLabel(appointment.status)}
          </span>
        </div>

        <div className="details-grid">
          <div className="detail-item">
            <label>Applicant</label>
            <p>{appointment.applicantName}</p>
          </div>

          <div className="detail-item">
            <label>Reviewer</label>
            <p>{appointment.reviewerName}</p>
          </div>

          <div className="detail-item">
            <label>Date</label>
            <p>{formatDateTime(appointment.date)}</p>
          </div>

          <div className="detail-item">
            <label>Time</label>
            <p>
              {formatTime(appointment.timeStart)} - {formatTime(appointment.timeEnd)}
            </p>
          </div>

          <div className="detail-item">
            <label>Created</label>
            <p>{formatDateTime(appointment.createdAt)}</p>
          </div>

          <div className="detail-item">
            <label>Last Updated</label>
            <p>{formatDateTime(appointment.updatedAt)}</p>
          </div>
        </div>

        {canManage && (
          <div className="action-section">
            <h3>Appointment Actions</h3>
            <div className="action-buttons">
              <button
                className="accept-button"
                onClick={handleAccept}
                disabled={actionLoading}
              >
                {actionLoading ? 'Processing...' : 'Accept'}
              </button>

              <button
                className="reject-button"
                onClick={() => setShowRejectForm(!showRejectForm)}
                disabled={actionLoading}
              >
                {showRejectForm ? 'Cancel Reject' : 'Reject'}
              </button>
            </div>

            {showRejectForm && (
              <div className="reject-form">
                <label htmlFor="rejectReason">Reason for Rejection</label>
                <textarea
                  id="rejectReason"
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder="Optional: Provide a reason for rejection"
                  disabled={actionLoading}
                  rows={4}
                />
                <button
                  className="confirm-reject"
                  onClick={handleReject}
                  disabled={actionLoading}
                >
                  {actionLoading ? 'Processing...' : 'Confirm Rejection'}
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
