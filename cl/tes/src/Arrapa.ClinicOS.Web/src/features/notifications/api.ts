import { getPaginated, postResource } from '@/features/admin/shared';

import type { NotificationLog } from './types';

interface ListParams {
  pageNumber: number;
  pageSize: number;
  appointmentId?: string;
  kind?: string;
  channel?: string;
  status?: string;
  search?: string;
}

export const notificationsApi = {
  list: (params: ListParams) => getPaginated<NotificationLog>('/v1/notifications/logs', params),
  resend: (appointmentId: string, channel?: string) =>
    postResource<{ appointmentId: string }, NotificationLog>(
      `/v1/notifications/resend${channel ? `?channel=${encodeURIComponent(channel)}` : ''}`,
      { appointmentId },
    ),
};
