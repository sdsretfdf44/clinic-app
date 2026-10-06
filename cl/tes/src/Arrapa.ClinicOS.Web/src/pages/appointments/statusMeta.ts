import type { BadgeTone } from '@/components/ui';
import type { AppointmentStatus } from '@/features/appointments';

export const STATUS_TONE: Record<AppointmentStatus, BadgeTone> = {
  Requested: 'amber',
  Booked: 'sky',
  Confirmed: 'brand',
  CheckedIn: 'amber',
  InConsultation: 'teal',
  Completed: 'green',
  Cancelled: 'gray',
  NoShow: 'red',
};

export const STATUS_LABEL: Record<AppointmentStatus, string> = {
  Requested: 'Requested',
  Booked: 'Booked',
  Confirmed: 'Confirmed',
  CheckedIn: 'Checked in',
  InConsultation: 'In consultation',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No-show',
};

/** Actions available from a given status, mapped to backend transition endpoint keys. */
export function availableActions(status: AppointmentStatus): Array<{
  action: 'confirm' | 'checkIn' | 'start' | 'complete' | 'noShow';
  label: string;
  variant: 'primary' | 'secondary' | 'danger';
}> {
  switch (status) {
    case 'Booked':
      return [
        { action: 'confirm', label: 'Confirm', variant: 'primary' },
        { action: 'checkIn', label: 'Check in', variant: 'secondary' },
        { action: 'noShow', label: 'No-show', variant: 'danger' },
      ];
    case 'Confirmed':
      return [
        { action: 'checkIn', label: 'Check in', variant: 'primary' },
        { action: 'noShow', label: 'No-show', variant: 'danger' },
      ];
    case 'CheckedIn':
      return [{ action: 'start', label: 'Start consultation', variant: 'primary' }];
    case 'InConsultation':
      return [{ action: 'complete', label: 'Complete', variant: 'primary' }];
    default:
      return [];
  }
}
