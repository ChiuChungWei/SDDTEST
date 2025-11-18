import client from './client';
import { CalendarData } from '../types/calendar';

export const calendarApi = {
  getAvailableSlots: async (reviewerId: number, date: Date): Promise<CalendarData> => {
    const dateString = date.toISOString().split('T')[0];
    const response = await client.get<CalendarData>(
      `/calendar/${reviewerId}/${dateString}`
    );
    return response.data;
  },
};
