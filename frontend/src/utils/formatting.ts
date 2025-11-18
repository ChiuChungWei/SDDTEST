export const formatDate = (date: Date | string): string => {
  const d = new Date(date);
  return d.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
};

export const formatDateTime = (date: Date | string): string => {
  const d = new Date(date);
  return d.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export const formatTime = (time: string | Date): string => {
  if (typeof time === 'string') {
    const [hour, minute] = time.split(':');
    return `${hour}:${minute}`;
  }
  return time.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
};

export const timeStringToDate = (time: string, date: Date = new Date()): Date => {
  const [hour, minute, second] = time.split(':').map(Number);
  const d = new Date(date);
  d.setHours(hour, minute, second || 0);
  return d;
};

export const dateToTimeString = (date: Date): string => {
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  return `${hours}:${minutes}:${seconds}`;
};

export const getStatusBadgeColor = (status: string): string => {
  switch (status) {
    case 'pending':
      return '#ffa500';
    case 'accepted':
      return '#28a745';
    case 'rejected':
      return '#dc3545';
    case 'delegated':
      return '#17a2b8';
    default:
      return '#6c757d';
  }
};

export const getStatusLabel = (status: string): string => {
  return status.charAt(0).toUpperCase() + status.slice(1);
};
