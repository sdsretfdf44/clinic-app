import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import {
  getPaginated,
  getResource,
  postResource,
  putResource,
} from '@/features/admin/shared';

// ==================== Types ====================
export type PortalAppointmentStatus =
  | 'Requested'
  | 'Booked'
  | 'Confirmed'
  | 'CheckedIn'
  | 'InConsultation'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export interface PortalProfile {
  id: string;
  mrn: string;
  firstName: string;
  middleName: string | null;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  bloodGroup: string;
  maritalStatus: string;
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
  addressLine1: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  country: string | null;
  postalCode: string | null;
  occupation: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
  emergencyContactRelationship: string | null;
  insuranceProvider: string | null;
  insurancePolicyNumber: string | null;
  insuranceGroupNumber: string | null;
  insuranceValidUntil: string | null;
  lastVisitAt: string | null;
}

export interface PortalDoctor {
  id: string;
  fullName: string;
  avatarUrl: string | null;
  specialty: string | null;
  departmentId: string | null;
  departmentName: string | null;
  consultationFee: number | null;
  currency: string;
  bio: string | null;
}

export interface PortalAppointmentSummary {
  id: string;
  appointmentNumber: string;
  doctorId: string;
  doctorName: string;
  doctorSpecialty: string | null;
  departmentId: string | null;
  departmentName: string | null;
  scheduledStart: string;
  scheduledEnd: string;
  status: PortalAppointmentStatus;
  reason: string | null;
  consultationFee: number | null;
  currency: string | null;
}

export interface PortalAppointmentDetail extends PortalAppointmentSummary {
  notes: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  createdOn: string;
  canCancel: boolean;
}

export interface PortalPrescriptionItem {
  id: string;
  medicine: string;
  strength: string | null;
  dosage: string | null;
  frequency: string | null;
  duration: string | null;
  instructions: string | null;
}

export interface PortalPrescription {
  id: string;
  visitId: string;
  issuedOn: string;
  doctorName: string;
  doctorSpecialty: string | null;
  generalInstructions: string | null;
  items: PortalPrescriptionItem[];
}

export type PortalInvoiceStatus =
  | 'Draft'
  | 'Issued'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Cancelled'
  | 'Refunded';

export interface PortalInvoiceLineItem {
  id: string;
  description: string;
  serviceCode: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PortalInvoicePayment {
  id: string;
  amount: number;
  method: string;
  reference: string | null;
  paidAt: string;
}

export interface PortalInvoiceSummary {
  id: string;
  invoiceNumber: string;
  appointmentId: string | null;
  appointmentNumber: string | null;
  status: PortalInvoiceStatus;
  issuedAt: string | null;
  dueAt: string | null;
  total: number;
  amountPaid: number;
  amountDue: number;
  currency: string;
}

export interface PortalInvoiceDetail extends PortalInvoiceSummary {
  cancelledAt: string | null;
  cancellationReason: string | null;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  notes: string | null;
  lineItems: PortalInvoiceLineItem[];
  payments: PortalInvoicePayment[];
}

export interface PortalDashboard {
  nextAppointment: PortalAppointmentSummary | null;
  upcomingAppointmentCount: number;
  pendingRequestCount: number;
  prescriptionCount: number;
  outstandingInvoiceCount: number;
  outstandingAmount: number;
  currency: string;
}

// ==================== Schemas ====================
export const requestAppointmentSchema = z.object({
  doctorId: z.string().uuid('Choose a doctor.'),
  scheduledDate: z.string().min(1, 'Choose a date.'),
  scheduledTime: z.string().min(1, 'Choose a time.'),
  durationMinutes: z.coerce.number().int().min(5).max(240),
  reason: z.string().max(500).optional().or(z.literal('')),
});
export type RequestAppointmentFormValues = z.infer<typeof requestAppointmentSchema>;

export const updatePortalProfileSchema = z.object({
  phone: z.string().max(30).optional().or(z.literal('')),
  email: z.string().max(200).email('Enter a valid email.').optional().or(z.literal('')),
  photoUrl: z.string().max(500).url('Enter a valid URL.').optional().or(z.literal('')),
  addressLine1: z.string().max(200).optional().or(z.literal('')),
  addressLine2: z.string().max(200).optional().or(z.literal('')),
  city: z.string().max(100).optional().or(z.literal('')),
  state: z.string().max(100).optional().or(z.literal('')),
  country: z.string().max(100).optional().or(z.literal('')),
  postalCode: z.string().max(20).optional().or(z.literal('')),
  occupation: z.string().max(200).optional().or(z.literal('')),
  emergencyContactName: z.string().max(200).optional().or(z.literal('')),
  emergencyContactPhone: z.string().max(30).optional().or(z.literal('')),
  emergencyContactRelationship: z.string().max(100).optional().or(z.literal('')),
});
export type UpdatePortalProfileFormValues = z.infer<typeof updatePortalProfileSchema>;

// ==================== API ====================
interface RequestAppointmentPayload {
  doctorId: string;
  departmentId: string | null;
  scheduledStart: string;
  durationMinutes: number;
  reason: string | null;
}

interface UpdatePortalProfilePayload {
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
  addressLine1: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  country: string | null;
  postalCode: string | null;
  occupation: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
  emergencyContactRelationship: string | null;
}

interface ListAppointmentsParams {
  pageNumber: number;
  pageSize: number;
  status?: string;
  upcomingOnly?: boolean;
}

interface ListInvoicesParams {
  pageNumber: number;
  pageSize: number;
  status?: string;
}

interface ListPrescriptionsParams {
  pageNumber: number;
  pageSize: number;
}

interface ListDoctorsParams {
  departmentId?: string;
  search?: string;
}

const keys = {
  dashboard: ['portal', 'dashboard'] as const,
  profile: ['portal', 'profile'] as const,
  doctors: (p: unknown) => ['portal', 'doctors', p] as const,
  appointments: {
    all: ['portal', 'appointments'] as const,
    list: (p: unknown) => ['portal', 'appointments', 'list', p] as const,
    detail: (id: string) => ['portal', 'appointments', 'detail', id] as const,
  },
  prescriptions: {
    all: ['portal', 'prescriptions'] as const,
    list: (p: unknown) => ['portal', 'prescriptions', 'list', p] as const,
    detail: (id: string) => ['portal', 'prescriptions', 'detail', id] as const,
  },
  invoices: {
    all: ['portal', 'invoices'] as const,
    list: (p: unknown) => ['portal', 'invoices', 'list', p] as const,
    detail: (id: string) => ['portal', 'invoices', 'detail', id] as const,
  },
};

export const portalApi = {
  dashboard: () => getResource<PortalDashboard>('/v1/portal/dashboard'),
  profile: () => getResource<PortalProfile>('/v1/portal/profile'),
  updateProfile: (payload: UpdatePortalProfilePayload) =>
    putResource<UpdatePortalProfilePayload, PortalProfile>('/v1/portal/profile', payload),
  doctors: (params: ListDoctorsParams) => getResource<PortalDoctor[]>('/v1/portal/doctors', params),
  appointments: {
    list: (params: ListAppointmentsParams) =>
      getPaginated<PortalAppointmentSummary>('/v1/portal/appointments', params),
    get: (id: string) => getResource<PortalAppointmentDetail>(`/v1/portal/appointments/${id}`),
    request: (payload: RequestAppointmentPayload) =>
      postResource<RequestAppointmentPayload, PortalAppointmentDetail>('/v1/portal/appointments', payload),
    cancel: (id: string, reason: string) =>
      postResource<{ reason: string }, unknown>(`/v1/portal/appointments/${id}/cancel`, { reason }),
  },
  prescriptions: {
    list: (params: ListPrescriptionsParams) =>
      getPaginated<PortalPrescription>('/v1/portal/prescriptions', params),
    get: (id: string) => getResource<PortalPrescription>(`/v1/portal/prescriptions/${id}`),
  },
  invoices: {
    list: (params: ListInvoicesParams) => getPaginated<PortalInvoiceSummary>('/v1/portal/invoices', params),
    get: (id: string) => getResource<PortalInvoiceDetail>(`/v1/portal/invoices/${id}`),
  },
};

// ==================== Hooks ====================
export function usePortalDashboard() {
  return useQuery({ queryKey: keys.dashboard, queryFn: portalApi.dashboard, staleTime: 10_000 });
}

export function usePortalProfile() {
  return useQuery({ queryKey: keys.profile, queryFn: portalApi.profile, staleTime: 30_000 });
}

export function useUpdatePortalProfile() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: portalApi.updateProfile,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.profile });
      qc.invalidateQueries({ queryKey: keys.dashboard });
    },
  });
}

export function usePortalDoctors(params: ListDoctorsParams) {
  return useQuery({
    queryKey: keys.doctors(params),
    queryFn: () => portalApi.doctors(params),
    staleTime: 60_000,
  });
}

export function usePortalAppointments(params: ListAppointmentsParams) {
  return useQuery({
    queryKey: keys.appointments.list(params),
    queryFn: () => portalApi.appointments.list(params),
    staleTime: 10_000,
  });
}

export function usePortalAppointment(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.appointments.detail(id) : ['portal', 'appointments', 'detail', 'none'],
    queryFn: () => portalApi.appointments.get(id!),
    enabled: !!id,
  });
}

export function useRequestAppointment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: portalApi.appointments.request,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.appointments.all });
      qc.invalidateQueries({ queryKey: keys.dashboard });
    },
  });
}

export function useCancelPortalAppointment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => portalApi.appointments.cancel(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.appointments.all });
      qc.invalidateQueries({ queryKey: keys.dashboard });
    },
  });
}

export function usePortalPrescriptions(params: ListPrescriptionsParams) {
  return useQuery({
    queryKey: keys.prescriptions.list(params),
    queryFn: () => portalApi.prescriptions.list(params),
    staleTime: 30_000,
  });
}

export function usePortalPrescription(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.prescriptions.detail(id) : ['portal', 'prescriptions', 'detail', 'none'],
    queryFn: () => portalApi.prescriptions.get(id!),
    enabled: !!id,
  });
}

export function usePortalInvoices(params: ListInvoicesParams) {
  return useQuery({
    queryKey: keys.invoices.list(params),
    queryFn: () => portalApi.invoices.list(params),
    staleTime: 15_000,
  });
}

export function usePortalInvoice(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.invoices.detail(id) : ['portal', 'invoices', 'detail', 'none'],
    queryFn: () => portalApi.invoices.get(id!),
    enabled: !!id,
  });
}

// ==================== Metadata ====================
import type { BadgeTone } from '@/components/ui';

export const PORTAL_APPT_STATUS_LABEL: Record<PortalAppointmentStatus, string> = {
  Requested: 'Requested',
  Booked: 'Booked',
  Confirmed: 'Confirmed',
  CheckedIn: 'Checked in',
  InConsultation: 'In consultation',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No-show',
};

export const PORTAL_APPT_STATUS_TONE: Record<PortalAppointmentStatus, BadgeTone> = {
  Requested: 'amber',
  Booked: 'sky',
  Confirmed: 'brand',
  CheckedIn: 'amber',
  InConsultation: 'amber',
  Completed: 'green',
  Cancelled: 'gray',
  NoShow: 'red',
};

export const PORTAL_INVOICE_STATUS_LABEL: Record<PortalInvoiceStatus, string> = {
  Draft: 'Draft',
  Issued: 'Issued',
  PartiallyPaid: 'Partially paid',
  Paid: 'Paid',
  Cancelled: 'Cancelled',
  Refunded: 'Refunded',
};

export const PORTAL_INVOICE_STATUS_TONE: Record<PortalInvoiceStatus, BadgeTone> = {
  Draft: 'gray',
  Issued: 'sky',
  PartiallyPaid: 'amber',
  Paid: 'green',
  Cancelled: 'red',
  Refunded: 'brand',
};
