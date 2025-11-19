import React, { useState, useEffect } from 'react';
import Calendar from 'react-calendar';
import { calendarApi } from '../../api/calendar';
import { formatDate, formatTime, timeStringToDate } from '../../utils/formatting';
import { TimeSlot } from '../../types/calendar';
import 'react-calendar/dist/Calendar.css';
import '../../styles/Calendar.css';

interface CalendarViewProps {
  reviewerId: number;
  onSlotSelect: (start: Date, end: Date) => void;
  loading?: boolean;
}

export const CalendarView: React.FC<CalendarViewProps> = ({
  reviewerId,
  onSlotSelect,
  loading = false,
}) => {
  const [date, setDate] = useState(new Date());
  const [availableSlots, setAvailableSlots] = useState<TimeSlot[]>([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState<TimeSlot | null>(null);

  useEffect(() => {
    if (date && reviewerId > 0) {
      fetchAvailableSlots();
    }
  }, [date, reviewerId]);

  const fetchAvailableSlots = async () => {
    setLoadingSlots(true);
    try {
      const calendar = await calendarApi.getAvailableSlots(reviewerId, date);
      setAvailableSlots(calendar.availableSlots);
    } catch (error) {
      console.error('Failed to fetch available slots:', error);
      setAvailableSlots([]);
    } finally {
      setLoadingSlots(false);
    }
  };

  const handleSlotSelect = (slot: TimeSlot) => {
    setSelectedSlot(slot);
    const startDate = timeStringToDate(slot.start, date);
    const endDate = timeStringToDate(slot.end, date);
    onSlotSelect(startDate, endDate);
  };

  return (
    <div className="calendar-container">
      <div className="calendar-wrapper">
        <h3>Select Date</h3>
        <Calendar
          value={date}
          onChange={(value) => setDate(value as Date)}
          minDate={new Date()}
          className="calendar"
        />
      </div>

      <div className="slots-wrapper">
        <h3>Available Time Slots for {formatDate(date)}</h3>
        {loadingSlots ? (
          <div className="loading">Loading available slots...</div>
        ) : availableSlots.length > 0 ? (
          <div className="slots-grid">
            {availableSlots.map((slot, index) => (
              <button
                key={index}
                className={`slot-button ${
                  selectedSlot?.start === slot.start ? 'selected' : ''
                }`}
                onClick={() => handleSlotSelect(slot)}
                disabled={loading}
              >
                {formatTime(slot.start)} - {formatTime(slot.end)}
              </button>
            ))}
          </div>
        ) : (
          <div className="no-slots">No available time slots on this date.</div>
        )}
      </div>
    </div>
  );
};
