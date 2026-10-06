import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getResource, postResource, putResource } from '@/features/admin/shared';

export interface Vitals {
  id: string;
  bloodPressureSystolic: number | null;
  bloodPressureDiastolic: number | null;
  pulseBpm: number | null;
  temperatureCelsius: number | null;
  heightCm: number | null;
  weightKg: number | null;
  bmi: number | null;
  spO2Percent: number | null;
  respirationRate: number | null;
  notes: string | null;
}

export interface PrescriptionItem {
  id: string;
  medicine: string;
  strength: string | null;
  dosage: string | null;
  frequency: string | null;
  duration: string | null;
  instructions: string | null;
  sortOrder: number;
}

export interface Prescription {
  id: string;
  issuedOn: string;
  generalInstructions: string | null;
  items: PrescriptionItem[];
}

export interface VisitDetail {
  id: string;
  appointmentId: string | null;
  appointmentNumber: string | null;
  patientId: string;
  patientName: string;
  patientMrn: string;
  patientAge: number;
  doctorId: string;
  doctorName: string;
  doctorSpecialty: string | null;
  visitDate: string;
  symptoms: string | null;
  diagnosis: string | null;
  doctorNotes: string | null;
  isLocked: boolean;
  vitals: Vitals | null;
  prescriptions: Prescription[];
  createdOn: string;
  updatedOn: string | null;
}

// -------------------- Schemas --------------------
export const vitalsSchema = z.object({
  bloodPressureSystolic: z.union([z.coerce.number().int().min(30).max(300), z.literal('')]).optional(),
  bloodPressureDiastolic: z.union([z.coerce.number().int().min(20).max(200), z.literal('')]).optional(),
  pulseBpm: z.union([z.coerce.number().int().min(20).max(250), z.literal('')]).optional(),
  temperatureCelsius: z.union([z.coerce.number().min(25).max(45), z.literal('')]).optional(),
  heightCm: z.union([z.coerce.number().min(20).max(250), z.literal('')]).optional(),
  weightKg: z.union([z.coerce.number().min(1).max(500), z.literal('')]).optional(),
  spO2Percent: z.union([z.coerce.number().int().min(50).max(100), z.literal('')]).optional(),
  respirationRate: z.union([z.coerce.number().int().min(5).max(60), z.literal('')]).optional(),
  notes: z.string().max(1000).optional().or(z.literal('')),
});
export type VitalsFormValues = z.infer<typeof vitalsSchema>;

export const diagnosisSchema = z.object({
  symptoms: z.string().max(2000).optional().or(z.literal('')),
  diagnosis: z.string().max(2000).optional().or(z.literal('')),
  doctorNotes: z.string().max(4000).optional().or(z.literal('')),
});
export type DiagnosisFormValues = z.infer<typeof diagnosisSchema>;

export const prescriptionItemSchema = z.object({
  medicine: z.string().min(1, 'Required.').max(200),
  strength: z.string().max(100).optional().or(z.literal('')),
  dosage: z.string().max(100).optional().or(z.literal('')),
  frequency: z.string().max(100).optional().or(z.literal('')),
  duration: z.string().max(100).optional().or(z.literal('')),
  instructions: z.string().max(500).optional().or(z.literal('')),
});

export const prescriptionSchema = z.object({
  generalInstructions: z.string().max(2000).optional().or(z.literal('')),
  items: z.array(prescriptionItemSchema).min(1, 'At least one medication is required.'),
});
export type PrescriptionFormValues = z.infer<typeof prescriptionSchema>;

// -------------------- API --------------------
interface VitalsPayload {
  bloodPressureSystolic: number | null;
  bloodPressureDiastolic: number | null;
  pulseBpm: number | null;
  temperatureCelsius: number | null;
  heightCm: number | null;
  weightKg: number | null;
  spO2Percent: number | null;
  respirationRate: number | null;
  notes: string | null;
}

interface DiagnosisPayload {
  symptoms: string | null;
  diagnosis: string | null;
  doctorNotes: string | null;
}

interface PrescriptionItemPayload {
  medicine: string;
  strength: string | null;
  dosage: string | null;
  frequency: string | null;
  duration: string | null;
  instructions: string | null;
  sortOrder: number;
}

interface PrescriptionPayload {
  generalInstructions: string | null;
  items: PrescriptionItemPayload[];
}

const keys = {
  all: ['visits'] as const,
  visit: (id: string) => ['visits', 'detail', id] as const,
  byAppointment: (id: string) => ['visits', 'by-appointment', id] as const,
};

export const visitsApi = {
  get: (id: string) => getResource<VisitDetail>(`/v1/visits/${id}`),
  getByAppointment: (appointmentId: string) =>
    getResource<VisitDetail>(`/v1/visits/by-appointment/${appointmentId}`),
  upsertVitals: (id: string, payload: VitalsPayload) =>
    putResource<VitalsPayload, Vitals>(`/v1/visits/${id}/vitals`, payload),
  upsertDiagnosis: (id: string, payload: DiagnosisPayload) =>
    putResource<DiagnosisPayload, VisitDetail>(`/v1/visits/${id}/diagnosis`, payload),
  addPrescription: (id: string, payload: PrescriptionPayload) =>
    postResource<PrescriptionPayload, Prescription>(`/v1/visits/${id}/prescriptions`, payload),
  updatePrescription: (prescriptionId: string, payload: PrescriptionPayload) =>
    putResource<PrescriptionPayload, Prescription>(`/v1/visits/prescriptions/${prescriptionId}`, payload),
  deletePrescription: (prescriptionId: string) =>
    deleteResource(`/v1/visits/prescriptions/${prescriptionId}`),
};

// -------------------- Hooks --------------------
export function useVisit(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.visit(id) : ['visits', 'detail', 'none'],
    queryFn: () => visitsApi.get(id!),
    enabled: !!id,
  });
}

export function useVisitByAppointment(appointmentId: string | undefined) {
  return useQuery({
    queryKey: appointmentId ? keys.byAppointment(appointmentId) : ['visits', 'by-appointment', 'none'],
    queryFn: () => visitsApi.getByAppointment(appointmentId!),
    enabled: !!appointmentId,
    retry: false,
  });
}

export function useUpsertVitals(visitId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: VitalsPayload) => visitsApi.upsertVitals(visitId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.visit(visitId) }),
  });
}

export function useUpsertDiagnosis(visitId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: DiagnosisPayload) => visitsApi.upsertDiagnosis(visitId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.visit(visitId) }),
  });
}

export function useAddPrescription(visitId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: PrescriptionPayload) => visitsApi.addPrescription(visitId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.visit(visitId) }),
  });
}

export function useUpdatePrescription(visitId: string, prescriptionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: PrescriptionPayload) => visitsApi.updatePrescription(prescriptionId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.visit(visitId) }),
  });
}

export function useDeletePrescription(visitId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (prescriptionId: string) => visitsApi.deletePrescription(prescriptionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.visit(visitId) }),
  });
}
