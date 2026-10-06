import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { z } from 'zod';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input, PageHeader, Select, Spinner, Switch } from '@/components/ui';
import { useAvailableSlots, useBookAppointment } from '@/features/appointments';
import { useDepartments } from '@/features/clinic/departments';
import { useDoctorsList } from '@/features/clinic/doctors';
import { useCreatePatient } from '@/features/patients/hooks';
import { GENDER_OPTIONS } from '@/features/patients/types';

const walkInSchema = z
  .object({
    // Patient (minimal required subset for walk-in)
    firstName: z.string().min(1, 'First name is required.').max(100),
    lastName: z.string().min(1, 'Last name is required.').max(100),
    dateOfBirth: z.string().min(1, 'Date of birth is required.'),
    gender: z.enum(['Unknown', 'Male', 'Female', 'Other', 'PreferNotToSay']),
    phone: z.string().max(30).optional().or(z.literal('')),
    email: z.string().email('Invalid email.').max(200).optional().or(z.literal('')),

    // Appointment
    bookAppointment: z.boolean(),
    doctorId: z.string().uuid().optional().or(z.literal('')),
    departmentId: z.string().uuid().optional().or(z.literal('')),
    scheduledDate: z.string().optional().or(z.literal('')),
    scheduledTime: z.string().optional().or(z.literal('')),
    durationMinutes: z.coerce.number().int().min(5).max(480).optional(),
    reason: z.string().max(500).optional().or(z.literal('')),
  })
  .refine((v) => !v.bookAppointment || (v.doctorId && v.scheduledDate && v.scheduledTime), {
    path: ['doctorId'],
    message: 'Doctor, date, and time are required when booking.',
  });

type WalkInFormValues = z.infer<typeof walkInSchema>;

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function WalkInRegistrationPage() {
  const navigate = useNavigate();
  const today = useMemo(() => toDateInputValue(new Date()), []);

  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 200, isActive: true });
  const departments = useDepartments(false);

  const createPatient = useCreatePatient();
  const bookAppointment = useBookAppointment();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<WalkInFormValues>({
    resolver: zodResolver(walkInSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      dateOfBirth: '',
      gender: 'Unknown',
      phone: '',
      email: '',
      bookAppointment: true,
      doctorId: '',
      departmentId: '',
      scheduledDate: today,
      scheduledTime: '',
      durationMinutes: 30,
      reason: 'Walk-in',
    },
  });

  const bookNow = form.watch('bookAppointment');
  const doctorId = form.watch('doctorId') || '';
  const scheduledDate = form.watch('scheduledDate') || today;
  const durationMinutes = Number(form.watch('durationMinutes')) || 30;

  const slots = useAvailableSlots(bookNow ? (doctorId || undefined) : undefined, scheduledDate, durationMinutes);

  const onSubmit = form.handleSubmit(async (values) => {
    setError(null);
    setIsSubmitting(true);
    try {
      const patient = await createPatient.mutateAsync({
        firstName: values.firstName.trim(),
        middleName: null,
        lastName: values.lastName.trim(),
        dateOfBirth: values.dateOfBirth,
        gender: values.gender,
        bloodGroup: 'Unknown',
        maritalStatus: 'Unknown',
        phone: values.phone?.trim() || null,
        email: values.email?.trim() || null,
        photoUrl: null,
        addressLine1: null,
        addressLine2: null,
        city: null,
        state: null,
        country: null,
        postalCode: null,
        occupation: null,
        emergencyContactName: null,
        emergencyContactPhone: null,
        emergencyContactRelationship: null,
        guardianName: null,
        guardianPhone: null,
        guardianRelationship: null,
        insuranceProvider: null,
        insurancePolicyNumber: null,
        insuranceGroupNumber: null,
        insuranceValidUntil: null,
        notes: null,
      });

      if (!values.bookAppointment) {
        navigate(`/patients/${patient.id}`);
        return;
      }

      const iso = new Date(`${values.scheduledDate}T${values.scheduledTime}`).toISOString();
      const appointment = await bookAppointment.mutateAsync({
        patientId: patient.id,
        doctorId: values.doctorId!,
        departmentId: values.departmentId || null,
        scheduledStart: iso,
        durationMinutes: Number(values.durationMinutes) || 30,
        reason: values.reason?.trim() || 'Walk-in',
        notes: null,
      });

      navigate(`/appointments/${appointment.id}`);
    } catch (err) {
      setError(humanizeError(err));
    } finally {
      setIsSubmitting(false);
    }
  });

  return (
    <div className="max-w-3xl">
      <PageHeader
        eyebrow="Reception"
        title="Walk-in registration"
        description="Register the patient and (optionally) book their appointment in one step."
      />

      <form onSubmit={onSubmit} noValidate>
        {error && <Alert variant="error" className="mb-4">{error}</Alert>}

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-4">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Patient</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="firstName" label="First name" required error={form.formState.errors.firstName?.message}>
              <Input id="firstName" invalid={!!form.formState.errors.firstName} {...form.register('firstName')} />
            </FormField>
            <FormField id="lastName" label="Last name" required error={form.formState.errors.lastName?.message}>
              <Input id="lastName" invalid={!!form.formState.errors.lastName} {...form.register('lastName')} />
            </FormField>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="dateOfBirth" label="Date of birth" required error={form.formState.errors.dateOfBirth?.message}>
              <Input id="dateOfBirth" type="date" {...form.register('dateOfBirth')} />
            </FormField>
            <FormField id="gender" label="Gender" required>
              <Select id="gender" {...form.register('gender')}>
                {GENDER_OPTIONS.map((g) => <option key={g} value={g}>{g}</option>)}
              </Select>
            </FormField>
            <FormField id="phone" label="Phone">
              <Input id="phone" type="tel" {...form.register('phone')} />
            </FormField>
          </div>
          <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
            <Input id="email" type="email" {...form.register('email')} />
          </FormField>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-4 mt-4">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Appointment</h3>
            <Switch
              checked={bookNow}
              onCheckedChange={(v) => form.setValue('bookAppointment', v, { shouldDirty: true })}
              label="Book now"
            />
          </div>

          {bookNow && (
            <div className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <FormField id="doctorId" label="Doctor" required error={form.formState.errors.doctorId?.message}>
                  <Select id="doctorId" invalid={!!form.formState.errors.doctorId} {...form.register('doctorId')}>
                    <option value="">Select a doctor…</option>
                    {doctors.data?.items.map((d) => (
                      <option key={d.id} value={d.id}>
                        {d.fullName}{d.specialty ? ` — ${d.specialty}` : ''}
                      </option>
                    ))}
                  </Select>
                </FormField>
                <FormField id="departmentId" label="Department">
                  <Select id="departmentId" {...form.register('departmentId')}>
                    <option value="">— auto —</option>
                    {departments.data?.map((d) => (
                      <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                  </Select>
                </FormField>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <FormField id="scheduledDate" label="Date">
                  <Input id="scheduledDate" type="date" min={today} {...form.register('scheduledDate')} />
                </FormField>
                <FormField id="scheduledTime" label="Time">
                  <Input id="scheduledTime" type="time" step={300} {...form.register('scheduledTime')} />
                </FormField>
                <FormField id="durationMinutes" label="Duration (min)">
                  <Select id="durationMinutes" {...form.register('durationMinutes')}>
                    <option value={15}>15 min</option>
                    <option value={30}>30 min</option>
                    <option value={45}>45 min</option>
                    <option value={60}>60 min</option>
                  </Select>
                </FormField>
              </div>

              {doctorId && (
                <div className="rounded-lg border border-gray-100 dark:border-gray-800 p-3">
                  <p className="text-xs uppercase tracking-widest text-gray-400 mb-2">
                    Available slots on {scheduledDate}
                  </p>
                  {slots.isPending && (
                    <div className="flex items-center text-sm text-gray-500">
                      <Spinner className="h-4 w-4 mr-2" /> Loading slots…
                    </div>
                  )}
                  {slots.data && slots.data.slots.length === 0 && (
                    <p className="text-sm text-gray-500">No available slots — pick a manual time or try another date.</p>
                  )}
                  {slots.data && slots.data.slots.length > 0 && (
                    <div className="flex flex-wrap gap-2">
                      {slots.data.slots.map((s) => {
                        const t = new Date(s.start);
                        const label = `${String(t.getHours()).padStart(2, '0')}:${String(t.getMinutes()).padStart(2, '0')}`;
                        const isSelected = form.watch('scheduledTime') === label;
                        return (
                          <button
                            key={s.start}
                            type="button"
                            onClick={() => form.setValue('scheduledTime', label, { shouldDirty: true })}
                            className={[
                              'px-3 py-1.5 rounded-lg text-xs border transition',
                              isSelected
                                ? 'bg-brand-500 text-white border-brand-500'
                                : 'bg-white text-gray-700 border-gray-200 hover:border-brand-400 dark:bg-gray-800 dark:text-gray-200 dark:border-gray-700',
                            ].join(' ')}
                          >
                            {label}
                          </button>
                        );
                      })}
                    </div>
                  )}
                </div>
              )}

              <FormField id="reason" label="Reason for visit">
                <Input id="reason" {...form.register('reason')} />
              </FormField>
            </div>
          )}
        </section>

        <div className="flex items-center justify-end gap-3 mt-6">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={isSubmitting}>
            {bookNow ? 'Register + Book' : 'Register patient'}
          </Button>
        </div>
      </form>
    </div>
  );
}
