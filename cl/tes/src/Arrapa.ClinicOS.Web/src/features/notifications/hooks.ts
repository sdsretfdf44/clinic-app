import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { notificationsApi } from './api';

export function useNotificationLogs(params: {
  pageNumber: number;
  pageSize: number;
  kind?: string;
  channel?: string;
  status?: string;
  search?: string;
}) {
  return useQuery({
    queryKey: ['notifications', 'logs', params],
    queryFn: () => notificationsApi.list(params),
    staleTime: 15_000,
  });
}

export function useResendReminder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ appointmentId, channel }: { appointmentId: string; channel?: string }) =>
      notificationsApi.resend(appointmentId, channel),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notifications', 'logs'] });
    },
  });
}
