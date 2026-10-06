import type { PatientPayload } from '@/features/patients/api';
import type { PatientFormValues } from '@/features/patients/schemas';

function nullify(value: string | null | undefined): string | null {
  const trimmed = value?.trim();
  return !trimmed ? null : trimmed;
}

export function toPatientPayload(values: PatientFormValues): PatientPayload {
  return {
    firstName: values.firstName.trim(),
    middleName: nullify(values.middleName),
    lastName: values.lastName.trim(),
    dateOfBirth: values.dateOfBirth,
    gender: values.gender,
    bloodGroup: values.bloodGroup,
    maritalStatus: values.maritalStatus,
    phone: nullify(values.phone),
    email: nullify(values.email),
    photoUrl: nullify(values.photoUrl),
    addressLine1: nullify(values.addressLine1),
    addressLine2: nullify(values.addressLine2),
    city: nullify(values.city),
    state: nullify(values.state),
    country: nullify(values.country),
    postalCode: nullify(values.postalCode),
    occupation: nullify(values.occupation),
    emergencyContactName: nullify(values.emergencyContactName),
    emergencyContactPhone: nullify(values.emergencyContactPhone),
    emergencyContactRelationship: nullify(values.emergencyContactRelationship),
    guardianName: nullify(values.guardianName),
    guardianPhone: nullify(values.guardianPhone),
    guardianRelationship: nullify(values.guardianRelationship),
    insuranceProvider: nullify(values.insuranceProvider),
    insurancePolicyNumber: nullify(values.insurancePolicyNumber),
    insuranceGroupNumber: nullify(values.insuranceGroupNumber),
    insuranceValidUntil: nullify(values.insuranceValidUntil),
    notes: nullify(values.notes),
  };
}
