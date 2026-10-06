import { deleteResource, getPaginated, getResource, postResource, putResource } from '@/features/admin/shared';

import type {
  MedicalHistoryEntry,
  PatientAllergy,
  PatientDetail,
  PatientMedication,
  PatientSummary,
} from './types';

export interface PatientPayload {
  firstName: string;
  middleName: string | null;
  lastName: string;
  dateOfBirth: string;
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
  guardianName: string | null;
  guardianPhone: string | null;
  guardianRelationship: string | null;
  insuranceProvider: string | null;
  insurancePolicyNumber: string | null;
  insuranceGroupNumber: string | null;
  insuranceValidUntil: string | null;
  notes: string | null;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  gender?: string;
  isActive?: boolean;
}

export const patientsApi = {
  list: (params: ListParams) => getPaginated<PatientSummary>('/v1/patients', params),
  get: (id: string) => getResource<PatientDetail>(`/v1/patients/${id}`),
  create: (payload: PatientPayload) => postResource<PatientPayload, PatientDetail>('/v1/patients', payload),
  update: (id: string, payload: PatientPayload) =>
    putResource<PatientPayload, PatientDetail>(`/v1/patients/${id}`, payload),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/patients/${id}/activate`, {}),
  deactivate: (id: string) => postResource<unknown, unknown>(`/v1/patients/${id}/deactivate`, {}),
  remove: (id: string) => deleteResource(`/v1/patients/${id}`),
};

export interface AllergyPayload {
  name: string;
  severity: string;
  reactionDescription: string | null;
  identifiedOn: string | null;
}
export const allergiesApi = {
  list: (patientId: string) => getResource<PatientAllergy[]>(`/v1/patients/${patientId}/allergies`),
  add: (patientId: string, payload: AllergyPayload) =>
    postResource<AllergyPayload, PatientAllergy>(`/v1/patients/${patientId}/allergies`, payload),
  update: (patientId: string, id: string, payload: AllergyPayload) =>
    putResource<AllergyPayload, PatientAllergy>(`/v1/patients/${patientId}/allergies/${id}`, payload),
  remove: (patientId: string, id: string) => deleteResource(`/v1/patients/${patientId}/allergies/${id}`),
};

export interface MedicalHistoryPayload {
  condition: string;
  since: string | null;
  notes: string | null;
  isChronic: boolean;
  isResolved: boolean;
}
export const medicalHistoryApi = {
  list: (patientId: string) => getResource<MedicalHistoryEntry[]>(`/v1/patients/${patientId}/medical-history`),
  add: (patientId: string, payload: MedicalHistoryPayload) =>
    postResource<MedicalHistoryPayload, MedicalHistoryEntry>(`/v1/patients/${patientId}/medical-history`, payload),
  update: (patientId: string, id: string, payload: MedicalHistoryPayload) =>
    putResource<MedicalHistoryPayload, MedicalHistoryEntry>(`/v1/patients/${patientId}/medical-history/${id}`, payload),
  remove: (patientId: string, id: string) => deleteResource(`/v1/patients/${patientId}/medical-history/${id}`),
};

export interface MedicationPayload {
  name: string;
  dosage: string | null;
  frequency: string | null;
  startedOn: string | null;
  notes: string | null;
  isActive: boolean;
}
export const medicationsApi = {
  list: (patientId: string, includeInactive = false) =>
    getResource<PatientMedication[]>(`/v1/patients/${patientId}/medications`, { includeInactive }),
  add: (patientId: string, payload: MedicationPayload) =>
    postResource<MedicationPayload, PatientMedication>(`/v1/patients/${patientId}/medications`, payload),
  update: (patientId: string, id: string, payload: MedicationPayload) =>
    putResource<MedicationPayload, PatientMedication>(`/v1/patients/${patientId}/medications/${id}`, payload),
  remove: (patientId: string, id: string) => deleteResource(`/v1/patients/${patientId}/medications/${id}`),
};
