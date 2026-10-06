import { useQuery } from '@tanstack/react-query';

import { getResource } from '@/features/admin/shared';

export type ReceptionBucketKey =
  | 'expectedToArrive'
  | 'waitingForConsultation'
  | 'inConsultation'
  | 'doneToday'
  | 'noShow';

export interface ReceptionQueueEntry {
  appointmentId: string;
  appointmentNumber: string;
  patientId: string;
  patientName: string;
  patientMrn: string;
  patientPhone: string | null;
  doctorId: string;
  doctorName: string;
  departmentName: string | null;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
  reason: string | null;
  checkedInAt: string | null;
}

export interface ReceptionQueueBucket {
  count: number;
  items: ReceptionQueueEntry[];
}

export interface ReceptionQueue {
  asOf: string;
  expectedToArrive: ReceptionQueueBucket;
  waitingForConsultation: ReceptionQueueBucket;
  inConsultation: ReceptionQueueBucket;
  doneToday: ReceptionQueueBucket;
  noShow: ReceptionQueueBucket;
}

interface QueueParams {
  doctorId?: string;
  departmentId?: string;
}

export const receptionApi = {
  queue: (params: QueueParams) => getResource<ReceptionQueue>('/v1/reception/queue', params),
};

export function useReceptionQueue(params: QueueParams) {
  return useQuery({
    queryKey: ['reception', 'queue', params],
    queryFn: () => receptionApi.queue(params),
    staleTime: 10_000,
    refetchInterval: 30_000,
    refetchOnWindowFocus: true,
  });
}
