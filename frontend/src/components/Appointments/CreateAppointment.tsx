import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { appointmentsApi } from '../api/appointments';
import { CalendarView } from '../components/Calendar/CalendarView';
import { dateToTimeString, formatDateTime } from '../utils/formatting';
import { useAuthStore } from '../store/authStore';
import '../styles/AppointmentForm.css';

interface CreateAppointmentProps {
  reviewerId?: number;
}

export const CreateAppointment: React.FC<CreateAppointmentProps> = ({
  reviewerId: initialReviewerId,
}) => {
  const [reviewerId, setReviewerId] = useState(initialReviewerId || 0);
  const [objectName, setObjectName] = useState('');
  const [selectedStartTime, setSelectedStartTime] = useState<Date | null>(null);
  const [selectedEndTime, setSelectedEndTime] = useState<Date | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const handleSlotSelect = (start: Date, end: Date) => {
    setSelectedStartTime(start);
    setSelectedEndTime(end);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!reviewerId) {
      setError('Please select a reviewer');
      return;
    }
    if (!objectName.trim()) {
      setError('Please enter the contract object name');
      return;
    }
    if (!selectedStartTime || !selectedEndTime) {
      setError('Please select a time slot');
      return;
    }

    setLoading(true);
    try {
      await appointmentsApi.create({
        reviewerId,
        date: selectedStartTime,
        startTime: dateToTimeString(selectedStartTime),
        endTime: dateToTimeString(selectedEndTime),
        objectName,
      });

      setSuccess('Appointment created successfully!');
      setTimeout(() => {
        navigate('/dashboard');
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create appointment');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="appointment-form-container">
      <div className="appointment-form">
        <h1>Create New Appointment</h1>

        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="reviewerId">Reviewer ID *</label>
              <input
                type="number"
                id="reviewerId"
                value={reviewerId}
                onChange={(e) => setReviewerId(Number(e.target.value))}
                placeholder="Enter reviewer ID"
                disabled={loading}
                required
                min="1"
              />
            </div>

            <div className="form-group">
              <label htmlFor="objectName">Contract Object Name *</label>
              <input
                type="text"
                id="objectName"
                value={objectName}
                onChange={(e) => setObjectName(e.target.value)}
                placeholder="e.g., Contract ABC-2025"
                disabled={loading}
                required
              />
            </div>
          </div>

          {reviewerId > 0 && (
            <div className="calendar-section">
              <CalendarView
                reviewerId={reviewerId}
                onSlotSelect={handleSlotSelect}
                loading={loading}
              />
            </div>
          )}

          {selectedStartTime && selectedEndTime && (
            <div className="selected-slot-info">
              <h3>Selected Time Slot</h3>
              <p>
                <strong>Date & Time:</strong>{' '}
                {formatDateTime(selectedStartTime)} - {formatDateTime(selectedEndTime)}
              </p>
            </div>
          )}

          {error && <div className="error-message">{error}</div>}
          {success && <div className="success-message">{success}</div>}

          <div className="form-actions">
            <button
              type="submit"
              disabled={loading || !reviewerId}
              className="submit-button"
            >
              {loading ? 'Creating...' : 'Create Appointment'}
            </button>
            <button
              type="button"
              className="cancel-button"
              onClick={() => navigate('/dashboard')}
              disabled={loading}
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
