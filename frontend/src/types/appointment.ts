export interface Appointment {
  id: number;
  applicantId: number;
  applicantName: string;
  reviewerId: number;
  reviewerName: string;
  date: string | Date;
  timeStart: string | Date;
  timeEnd: string | Date;
  objectName: string;
  status: 'pending' | 'accepted' | 'rejected' | 'delegated' | 'delegate_accepted' | 'delegate_rejected' | 'cancelled';
  createdAt: string | Date;
  updatedAt: string | Date;
}

export interface CreateAppointmentRequest {
  reviewerId: number;
  date: Date;
  startTime: Date | string;
  endTime: Date | string;
  objectName: string;
}
