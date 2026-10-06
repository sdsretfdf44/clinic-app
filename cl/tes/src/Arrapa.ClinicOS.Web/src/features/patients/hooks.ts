import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import {
  allergiesApi,
  medicalHistoryApi,
  medicationsApi,
  patientsApi,
  type AllergyPayload,
  type MedicalHistoryPayload,
  type MedicationPayload,
  type PatientPayload,
} from './api';

const keys = {
  patients: ['patients'] as const,
  patientList: (p: unknown) => ['patients', 'list', p] as const,
  patient: (id: string) => ['patients', 'detail', id] as const,
  allergies: (id: string) => ['patients', id, 'allergies'] as const,
  history: (id: string) => ['patients', id, 'history'] as const,
  meds: (id: string, includeInactive: boolean) => ['patients', id, 'meds', includeInactive] as const,
};

export function usePatientsList(params: Parameters<typeof patientsApi.list>[0]) {
  return useQuery({ queryKey: keys.patientList(params), queryFn: () => patientsApi.list(params), staleTime: 15_000 });
}

export function usePatient(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.patient(id) : ['patients', 'detail', 'none'],
    queryFn: () => patientsApi.get(id!),
    enabled: !!id,
  });
}

export function useCreatePatient() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: PatientPayload) => patientsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.patients }),
  });
}

export function useUpdatePatient(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: PatientPayload) => patientsApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.patient(id) });
      qc.invalidateQueries({ queryKey: keys.patients });
    },
  });
}

export function useSetPatientActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? patientsApi.activate(id) : patientsApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.patients }),
  });
}

export function useDeletePatient() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: patientsApi.remove,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.patients }),
  });
}

// -------------------- Allergies --------------------
export function usePatientAllergies(patientId: string | undefined) {
  return useQuery({
    queryKey: patientId ? keys.allergies(patientId) : ['patients', 'none', 'allergies'],
    queryFn: () => allergiesApi.list(patientId!),
    enabled: !!patientId,
  });
}

export function useUpsertAllergy(patientId: string, allergyId?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: AllergyPayload) =>
      allergyId ? allergiesApi.update(patientId, allergyId, payload) : allergiesApi.add(patientId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.allergies(patientId) });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}

export function useDeleteAllergy(patientId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => allergiesApi.remove(patientId, id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.allergies(patientId) });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}

// -------------------- Medical history --------------------
export function usePatientMedicalHistory(patientId: string | undefined) {
  return useQuery({
    queryKey: patientId ? keys.history(patientId) : ['patients', 'none', 'history'],
    queryFn: () => medicalHistoryApi.list(patientId!),
    enabled: !!patientId,
  });
}

export function useUpsertMedicalHistory(patientId: string, entryId?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: MedicalHistoryPayload) =>
      entryId ? medicalHistoryApi.update(patientId, entryId, payload) : medicalHistoryApi.add(patientId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.history(patientId) });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}

export function useDeleteMedicalHistory(patientId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => medicalHistoryApi.remove(patientId, id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.history(patientId) });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}

// -------------------- Medications --------------------
export function usePatientMedications(patientId: string | undefined, includeInactive = false) {
  return useQuery({
    queryKey: patientId ? keys.meds(patientId, includeInactive) : ['patients', 'none', 'meds'],
    queryFn: () => medicationsApi.list(patientId!, includeInactive),
    enabled: !!patientId,
  });
}

export function useUpsertMedication(patientId: string, medicationId?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: MedicationPayload) =>
      medicationId ? medicationsApi.update(patientId, medicationId, payload) : medicationsApi.add(patientId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['patients', patientId] });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}

export function useDeleteMedication(patientId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => medicationsApi.remove(patientId, id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['patients', patientId] });
      qc.invalidateQueries({ queryKey: keys.patient(patientId) });
    },
  });
}
