export type Gender = 'Unknown' | 'Male' | 'Female' | 'Other' | 'PreferNotToSay';
export type BloodGroup =
  | 'Unknown' | 'APositive' | 'ANegative' | 'BPositive' | 'BNegative'
  | 'ABPositive' | 'ABNegative' | 'OPositive' | 'ONegative';
export type MaritalStatus = 'Unknown' | 'Single' | 'Married' | 'Divorced' | 'Widowed' | 'Separated' | 'Other';
export type AllergySeverity = 'Unknown' | 'Mild' | 'Moderate' | 'Severe' | 'LifeThreatening';

export interface PatientSummary {
  id: string;
  mrn: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: Gender;
  bloodGroup: BloodGroup;
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
  isActive: boolean;
  lastVisitAt: string | null;
  createdOn: string;
}

export interface PatientDetail {
  id: string;
  mrn: string;
  firstName: string;
  middleName: string | null;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: Gender;
  bloodGroup: BloodGroup;
  maritalStatus: MaritalStatus;
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
  isActive: boolean;
  lastVisitAt: string | null;
  createdOn: string;
  updatedOn: string | null;
  allergyCount: number;
  medicalHistoryCount: number;
  activeMedicationCount: number;
}

export interface PatientAllergy {
  id: string;
  patientId: string;
  name: string;
  severity: AllergySeverity;
  reactionDescription: string | null;
  identifiedOn: string | null;
  createdOn: string;
}

export interface MedicalHistoryEntry {
  id: string;
  patientId: string;
  condition: string;
  since: string | null;
  notes: string | null;
  isChronic: boolean;
  isResolved: boolean;
  createdOn: string;
}

export interface PatientMedication {
  id: string;
  patientId: string;
  name: string;
  dosage: string | null;
  frequency: string | null;
  startedOn: string | null;
  notes: string | null;
  isActive: boolean;
  createdOn: string;
}

export const GENDER_OPTIONS: Gender[] = ['Unknown', 'Male', 'Female', 'Other', 'PreferNotToSay'];
export const BLOOD_GROUP_OPTIONS: BloodGroup[] = [
  'Unknown', 'APositive', 'ANegative', 'BPositive', 'BNegative',
  'ABPositive', 'ABNegative', 'OPositive', 'ONegative',
];
export const MARITAL_OPTIONS: MaritalStatus[] = [
  'Unknown', 'Single', 'Married', 'Divorced', 'Widowed', 'Separated', 'Other',
];
export const ALLERGY_SEVERITY_OPTIONS: AllergySeverity[] = [
  'Unknown', 'Mild', 'Moderate', 'Severe', 'LifeThreatening',
];

export const BLOOD_GROUP_LABEL: Record<BloodGroup, string> = {
  Unknown: 'Unknown',
  APositive: 'A+', ANegative: 'A-',
  BPositive: 'B+', BNegative: 'B-',
  ABPositive: 'AB+', ABNegative: 'AB-',
  OPositive: 'O+', ONegative: 'O-',
};
