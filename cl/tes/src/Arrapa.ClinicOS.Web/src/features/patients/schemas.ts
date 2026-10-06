import { z } from 'zod';

export const patientSchema = z.object({
  firstName: z.string().min(1, 'First name is required.').max(100),
  middleName: z.string().max(100).optional().or(z.literal('')),
  lastName: z.string().min(1, 'Last name is required.').max(100),
  dateOfBirth: z.string().min(1, 'Date of birth is required.'),
  gender: z.enum(['Unknown', 'Male', 'Female', 'Other', 'PreferNotToSay']),
  bloodGroup: z.enum([
    'Unknown', 'APositive', 'ANegative', 'BPositive', 'BNegative',
    'ABPositive', 'ABNegative', 'OPositive', 'ONegative',
  ]),
  maritalStatus: z.enum(['Unknown', 'Single', 'Married', 'Divorced', 'Widowed', 'Separated', 'Other']),
  phone: z.string().max(30).optional().or(z.literal('')),
  email: z.string().email('Invalid email.').max(200).optional().or(z.literal('')),
  photoUrl: z.string().max(500).url('Invalid URL.').optional().or(z.literal('')),
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
  guardianName: z.string().max(200).optional().or(z.literal('')),
  guardianPhone: z.string().max(30).optional().or(z.literal('')),
  guardianRelationship: z.string().max(100).optional().or(z.literal('')),
  insuranceProvider: z.string().max(200).optional().or(z.literal('')),
  insurancePolicyNumber: z.string().max(100).optional().or(z.literal('')),
  insuranceGroupNumber: z.string().max(100).optional().or(z.literal('')),
  insuranceValidUntil: z.string().optional().or(z.literal('')),
  notes: z.string().max(4000).optional().or(z.literal('')),
});
export type PatientFormValues = z.infer<typeof patientSchema>;

export const allergySchema = z.object({
  name: z.string().min(1).max(200),
  severity: z.enum(['Unknown', 'Mild', 'Moderate', 'Severe', 'LifeThreatening']),
  reactionDescription: z.string().max(500).optional().or(z.literal('')),
  identifiedOn: z.string().optional().or(z.literal('')),
});
export type AllergyFormValues = z.infer<typeof allergySchema>;

export const medicalHistorySchema = z.object({
  condition: z.string().min(1).max(200),
  since: z.string().optional().or(z.literal('')),
  notes: z.string().max(1000).optional().or(z.literal('')),
  isChronic: z.boolean(),
  isResolved: z.boolean(),
});
export type MedicalHistoryFormValues = z.infer<typeof medicalHistorySchema>;

export const medicationSchema = z.object({
  name: z.string().min(1).max(200),
  dosage: z.string().max(100).optional().or(z.literal('')),
  frequency: z.string().max(100).optional().or(z.literal('')),
  startedOn: z.string().optional().or(z.literal('')),
  notes: z.string().max(500).optional().or(z.literal('')),
  isActive: z.boolean(),
});
export type MedicationFormValues = z.infer<typeof medicationSchema>;
