export interface TimeSlot {
  start: string;
  end: string;
}

export interface CalendarData {
  reviewerId: number;
  date: string | Date;
  availableSlots: TimeSlot[];
}
