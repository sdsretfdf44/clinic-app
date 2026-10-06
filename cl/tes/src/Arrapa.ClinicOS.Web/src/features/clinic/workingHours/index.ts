import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { getResource, putResource } from '../shared';

export interface WorkingHoursSlot {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

export interface WorkingHoursResponse {
  doctorId: string | null;
  slots: WorkingHoursSlot[];
}

interface SetPayload {
  slots: WorkingHoursSlot[];
}

const keys = {
  clinic: ['clinic', 'working-hours', 'clinic'] as const,
  doctor: (id: string) => ['clinic', 'working-hours', 'doctor', id] as const,
};

export const workingHoursApi = {
  getClinic: () => getResource<WorkingHoursResponse>('/v1/clinic/working-hours'),
  setClinic: (slots: WorkingHoursSlot[]) => putResource<SetPayload, WorkingHoursResponse>('/v1/clinic/working-hours', { slots }),
  getDoctor: (id: string) => getResource<WorkingHoursResponse>(`/v1/clinic/working-hours/doctors/${id}`),
  setDoctor: (id: string, slots: WorkingHoursSlot[]) =>
    putResource<SetPayload, WorkingHoursResponse>(`/v1/clinic/working-hours/doctors/${id}`, { slots }),
};

export function useClinicHours() {
  return useQuery({ queryKey: keys.clinic, queryFn: workingHoursApi.getClinic, staleTime: 30_000 });
}

export function useSetClinicHours() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (slots: WorkingHoursSlot[]) => workingHoursApi.setClinic(slots),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.clinic }),
  });
}

export function useDoctorHours(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.doctor(id) : ['clinic', 'working-hours', 'doctor', 'none'],
    queryFn: () => workingHoursApi.getDoctor(id!),
    enabled: !!id,
  });
}

export function useSetDoctorHours(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (slots: WorkingHoursSlot[]) => workingHoursApi.setDoctor(id, slots),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.doctor(id) }),
  });
}
