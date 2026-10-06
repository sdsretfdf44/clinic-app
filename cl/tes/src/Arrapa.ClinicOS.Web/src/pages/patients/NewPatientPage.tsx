import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, PageHeader } from '@/components/ui';
import { useCreatePatient } from '@/features/patients/hooks';
import { patientSchema, type PatientFormValues } from '@/features/patients/schemas';

import { PatientForm } from './PatientForm';
import { toPatientPayload } from './toPatientPayload';

const defaults: PatientFormValues = {
  firstName: '', middleName: '', lastName: '',
  dateOfBirth: '', gender: 'Unknown', bloodGroup: 'Unknown', maritalStatus: 'Unknown',
  phone: '', email: '', photoUrl: '',
  addressLine1: '', addressLine2: '', city: '', state: '', country: '', postalCode: '',
  occupation: '',
  emergencyContactName: '', emergencyContactPhone: '', emergencyContactRelationship: '',
  guardianName: '', guardianPhone: '', guardianRelationship: '',
  insuranceProvider: '', insurancePolicyNumber: '', insuranceGroupNumber: '', insuranceValidUntil: '',
  notes: '',
};

export default function NewPatientPage() {
  const navigate = useNavigate();
  const createPatient = useCreatePatient();
  const form = useForm<PatientFormValues>({
    resolver: zodResolver(patientSchema),
    defaultValues: defaults,
  });

  const onSubmit = form.handleSubmit((values) => {
    createPatient.mutate(toPatientPayload(values), {
      onSuccess: (patient) => navigate(`/patients/${patient.id}`),
    });
  });

  return (
    <div className="max-w-4xl">
      <PageHeader
        eyebrow="Patients"
        title="Register new patient"
        description="Walk-in registration. Fields marked with * are required; medical details can be added later from the patient chart."
      />

      <form onSubmit={onSubmit} noValidate>
        {createPatient.isError && <Alert variant="error" className="mb-4">{humanizeError(createPatient.error)}</Alert>}

        <PatientForm form={form} />

        <div className="flex items-center justify-end gap-3 mt-6">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={createPatient.isPending}>Register patient</Button>
        </div>
      </form>
    </div>
  );
}
