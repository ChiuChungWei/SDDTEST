import client from './client';
import { Appointment, CreateAppointmentRequest } from '../types/appointment';

export const appointmentsApi = {
  create: async (data: CreateAppointmentRequest): Promise<Appointment> => {
    const response = await client.post<Appointment>('/appointments', {
      ...data,
      startTime: data.startTime instanceof Date ? data.startTime.toTimeString().slice(0, 8) : data.startTime,
      endTime: data.endTime instanceof Date ? data.endTime.toTimeString().slice(0, 8) : data.endTime,
    });
    return response.data;
  },

  getById: async (id: number): Promise<Appointment> => {
    const response = await client.get<Appointment>(`/appointments/${id}`);
    return response.data;
  },

  accept: async (id: number): Promise<Appointment> => {
    const response = await client.put<{ message: string; appointment: Appointment }>(
      `/appointments/${id}/accept`,
      {}
    );
    return response.data.appointment;
  },

  reject: async (id: number, reason?: string): Promise<void> => {
    await client.put(`/appointments/${id}/reject`, { reason });
  },
};
