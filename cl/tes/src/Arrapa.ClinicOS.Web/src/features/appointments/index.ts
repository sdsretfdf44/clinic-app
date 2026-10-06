import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getPaginated, getResource, postResource, putResource } from '@/features/admin/shared';

export type AppointmentStatus =
  | 'Requested'
  | 'Booked'
  | 'Confirmed'
  | 'CheckedIn'
  | 'InConsultation'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export interface AppointmentSummary {
  id: string;
  appointmentNumber: string;
  patientId: string;
  patientName: string;
  patientMrn: string;
  doctorId: string;
  doctorName: string;
  departmentId: string | null;
  departmentName: string | null;
  scheduledStart: string;
  scheduledEnd: string;
  status: AppointmentStatus;
  reason: string | null;
  consultationFee: number | null;
  currency: string | null;
}

export interface AppointmentDetail {
  id: string;
  appointmentNumber: string;
  patientId: string;
  patientName: string;
  patientMrn: string;
  patientPhone: string | null;
  doctorId: string;
  doctorName: string;
  doctorSpecialty: string | null;
  departmentId: string | null;
  departmentName: string | null;
  scheduledStart: string;
  scheduledEnd: string;
  status: AppointmentStatus;
  reason: string | null;
  notes: string | null;
  consultationFee: number | null;
  currency: string | null;
  confirmedAt: string | null;
  checkedInAt: string | null;
  consultationStartedAt: string | null;
  consultationEndedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  createdOn: string;
  updatedOn: string | null;
}

export interface AppointmentSlot {
  start: string;
  end: string;
}

export interface AvailableSlots {
  doctorId: string;
  date: string;
  slotMinutes: number;
  slots: AppointmentSlot[];
}

export const bookAppointmentSchema = z.object({
  patientId: z.string().uuid('Choose a patient.'),
  doctorId: z.string().uuid('Choose a doctor.'),
  departmentId: z.string().uuid().optional().or(z.literal('')),
  scheduledDate: z.string().min(1, 'Choose a date.'),
  scheduledTime: z.string().min(1, 'Choose a time.'),
  durationMinutes: z.coerce.number().int().min(5).max(480),
  reason: z.string().max(500).optional().or(z.literal('')),
  notes: z.string().max(2000).optional().or(z.literal('')),
});
export type BookAppointmentFormValues = z.infer<typeof bookAppointmentSchema>;

export const rescheduleAppointmentSchema = bookAppointmentSchema.omit({ patientId: true });
export type RescheduleFormValues = z.infer<typeof rescheduleAppointmentSchema>;

interface BookPayload {
  patientId: string;
  doctorId: string;
  departmentId: string | null;
  scheduledStart: string;
  durationMinutes: number;
  reason: string | null;
  notes: string | null;
}

interface ReschedulePayload {
  doctorId: string;
  departmentId: string | null;
  scheduledStart: string;
  durationMinutes: number;
  reason: string | null;
  notes: string | null;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  from?: string;
  to?: string;
  doctorId?: string;
  patientId?: string;
  departmentId?: string;
  status?: string;
  search?: string;
}

const keys = {
  all: ['appointments'] as const,
  list: (p: unknown) => ['appointments', 'list', p] as const,
  detail: (id: string) => ['appointments', 'detail', id] as const,
  slots: (doctorId: string, date: string, slotMinutes: number) =>
    ['appointments', 'slots', doctorId, date, slotMinutes] as const,
};

export const appointmentsApi = {
  list: (params: ListParams) => getPaginated<AppointmentSummary>('/v1/appointments', params),
  get: (id: string) => getResource<AppointmentDetail>(`/v1/appointments/${id}`),
  book: (payload: BookPayload) => postResource<BookPayload, AppointmentDetail>('/v1/appointments', payload),
  reschedule: (id: string, payload: ReschedulePayload) =>
    putResource<ReschedulePayload, AppointmentDetail>(`/v1/appointments/${id}`, payload),
  confirm: (id: string) => postResource<unknown, unknown>(`/v1/appointments/${id}/confirm`, {}),
  checkIn: (id: string) => postResource<unknown, unknown>(`/v1/appointments/${id}/check-in`, {}),
  start: (id: string) => postResource<unknown, unknown>(`/v1/appointments/${id}/start`, {}),
  complete: (id: string) => postResource<unknown, unknown>(`/v1/appointments/${id}/complete`, {}),
  noShow: (id: string) => postResource<unknown, unknown>(`/v1/appointments/${id}/no-show`, {}),
  cancel: (id: string, reason: string) =>
    postResource<{ reason: string }, unknown>(`/v1/appointments/${id}/cancel`, { reason }),
  remove: (id: string) => deleteResource(`/v1/appointments/${id}`),
  availableSlots: (doctorId: string, date: string, slotMinutes: number) =>
    getResource<AvailableSlots>('/v1/appointments/available-slots', { doctorId, date, slotMinutes }),
};

export function useAppointmentsList(params: ListParams) {
  return useQuery({ queryKey: keys.list(params), queryFn: () => appointmentsApi.list(params), staleTime: 10_000 });
}

export function useAppointment(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.detail(id) : ['appointments', 'detail', 'none'],
    queryFn: () => appointmentsApi.get(id!),
    enabled: !!id,
  });
}

export function useBookAppointment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: appointmentsApi.book,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useRescheduleAppointment(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: ReschedulePayload) => appointmentsApi.reschedule(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useTransitionAppointment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, action }: { id: string; action: 'confirm' | 'checkIn' | 'start' | 'complete' | 'noShow' }) => {
      switch (action) {
        case 'confirm': return appointmentsApi.confirm(id);
        case 'checkIn': return appointmentsApi.checkIn(id);
        case 'start': return appointmentsApi.start(id);
        case 'complete': return appointmentsApi.complete(id);
        case 'noShow': return appointmentsApi.noShow(id);
      }
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useCancelAppointment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => appointmentsApi.cancel(id, reason),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useAvailableSlots(doctorId: string | undefined, date: string, slotMinutes: number) {
  return useQuery({
    queryKey: doctorId && date ? keys.slots(doctorId, date, slotMinutes) : ['appointments', 'slots', 'none'],
    queryFn: () => appointmentsApi.availableSlots(doctorId!, date, slotMinutes),
    enabled: !!doctorId && !!date,
    staleTime: 5_000,
  });
}
