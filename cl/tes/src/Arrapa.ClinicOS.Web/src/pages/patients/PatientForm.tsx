import type { UseFormReturn } from 'react-hook-form';

import { FormField, Input, Select, Textarea } from '@/components/ui';
import {
  BLOOD_GROUP_LABEL,
  BLOOD_GROUP_OPTIONS,
  GENDER_OPTIONS,
  MARITAL_OPTIONS,
} from '@/features/patients/types';
import type { PatientFormValues } from '@/features/patients/schemas';

interface PatientFormProps {
  form: UseFormReturn<PatientFormValues>;
}

export function PatientForm({ form }: PatientFormProps) {
  const { register, formState: { errors } } = form;

  return (
    <div className="space-y-6">
      <Section title="Identity">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <FormField id="firstName" label="First name" required error={errors.firstName?.message}>
            <Input id="firstName" invalid={!!errors.firstName} {...register('firstName')} />
          </FormField>
          <FormField id="middleName" label="Middle name">
            <Input id="middleName" {...register('middleName')} />
          </FormField>
          <FormField id="lastName" label="Last name" required error={errors.lastName?.message}>
            <Input id="lastName" invalid={!!errors.lastName} {...register('lastName')} />
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
          <FormField id="dateOfBirth" label="Date of birth" required error={errors.dateOfBirth?.message}>
            <Input id="dateOfBirth" type="date" {...register('dateOfBirth')} />
          </FormField>
          <FormField id="gender" label="Gender" required>
            <Select id="gender" {...register('gender')}>
              {GENDER_OPTIONS.map((g) => (
                <option key={g} value={g}>{g}</option>
              ))}
            </Select>
          </FormField>
          <FormField id="bloodGroup" label="Blood group">
            <Select id="bloodGroup" {...register('bloodGroup')}>
              {BLOOD_GROUP_OPTIONS.map((b) => (
                <option key={b} value={b}>{BLOOD_GROUP_LABEL[b]}</option>
              ))}
            </Select>
          </FormField>
          <FormField id="maritalStatus" label="Marital status">
            <Select id="maritalStatus" {...register('maritalStatus')}>
              {MARITAL_OPTIONS.map((m) => (
                <option key={m} value={m}>{m}</option>
              ))}
            </Select>
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="phone" label="Phone">
            <Input id="phone" type="tel" {...register('phone')} />
          </FormField>
          <FormField id="email" label="Email" error={errors.email?.message}>
            <Input id="email" type="email" invalid={!!errors.email} {...register('email')} />
          </FormField>
        </div>
        <FormField id="photoUrl" label="Photo URL" error={errors.photoUrl?.message}>
          <Input id="photoUrl" type="url" invalid={!!errors.photoUrl} {...register('photoUrl')} />
        </FormField>
        <FormField id="occupation" label="Occupation">
          <Input id="occupation" {...register('occupation')} />
        </FormField>
      </Section>

      <Section title="Address">
        <FormField id="addressLine1" label="Address line 1">
          <Input id="addressLine1" {...register('addressLine1')} />
        </FormField>
        <FormField id="addressLine2" label="Address line 2">
          <Input id="addressLine2" {...register('addressLine2')} />
        </FormField>
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
          <FormField id="city" label="City">
            <Input id="city" {...register('city')} />
          </FormField>
          <FormField id="state" label="State / Region">
            <Input id="state" {...register('state')} />
          </FormField>
          <FormField id="country" label="Country">
            <Input id="country" {...register('country')} />
          </FormField>
          <FormField id="postalCode" label="Postal code">
            <Input id="postalCode" {...register('postalCode')} />
          </FormField>
        </div>
      </Section>

      <Section title="Emergency contact">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <FormField id="emergencyContactName" label="Name">
            <Input id="emergencyContactName" {...register('emergencyContactName')} />
          </FormField>
          <FormField id="emergencyContactPhone" label="Phone">
            <Input id="emergencyContactPhone" type="tel" {...register('emergencyContactPhone')} />
          </FormField>
          <FormField id="emergencyContactRelationship" label="Relationship">
            <Input id="emergencyContactRelationship" {...register('emergencyContactRelationship')} />
          </FormField>
        </div>
      </Section>

      <Section title="Guardian (for minors)">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <FormField id="guardianName" label="Name">
            <Input id="guardianName" {...register('guardianName')} />
          </FormField>
          <FormField id="guardianPhone" label="Phone">
            <Input id="guardianPhone" type="tel" {...register('guardianPhone')} />
          </FormField>
          <FormField id="guardianRelationship" label="Relationship">
            <Input id="guardianRelationship" {...register('guardianRelationship')} />
          </FormField>
        </div>
      </Section>

      <Section title="Insurance">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="insuranceProvider" label="Provider">
            <Input id="insuranceProvider" {...register('insuranceProvider')} />
          </FormField>
          <FormField id="insurancePolicyNumber" label="Policy number">
            <Input id="insurancePolicyNumber" {...register('insurancePolicyNumber')} />
          </FormField>
          <FormField id="insuranceGroupNumber" label="Group number">
            <Input id="insuranceGroupNumber" {...register('insuranceGroupNumber')} />
          </FormField>
          <FormField id="insuranceValidUntil" label="Valid until">
            <Input id="insuranceValidUntil" type="date" {...register('insuranceValidUntil')} />
          </FormField>
        </div>
      </Section>

      <Section title="Notes">
        <FormField id="notes" label="Free-form notes">
          <Textarea id="notes" rows={4} {...register('notes')} />
        </FormField>
      </Section>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 sm:p-6 space-y-4">
      <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{title}</h3>
      {children}
    </section>
  );
}
