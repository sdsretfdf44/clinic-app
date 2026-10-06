import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useSearchParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Button,
  FormField,
  Input,
  PageHeader,
  Select,
  Spinner,
  Textarea,
} from '@/components/ui';
import {
  bookAppointmentSchema,
  useAvailableSlots,
  useBookAppointment,
  type BookAppointmentFormValues,
} from '@/features/appointments';
import { useDepartments } from '@/features/clinic/departments';
import { useDoctorsList } from '@/features/clinic/doctors';
import { usePatientsList } from '@/features/patients/hooks';

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function NewAppointmentPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const today = useMemo(() => toDateInputValue(new Date()), []);

  const [patientSearch, setPatientSearch] = useState('');

  const patients = usePatientsList({
    pageNumber: 1,
    pageSize: 25,
    search: patientSearch || undefined,
    isActive: true,
  });

  const departments = useDepartments(false);
  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 100, isActive: true });

  const form = useForm<BookAppointmentFormValues>({
    resolver: zodResolver(bookAppointmentSchema),
    defaultValues: {
      patientId: params.get('patientId') ?? '',
      doctorId: '',
      departmentId: '',
      scheduledDate: today,
      scheduledTime: '',
      durationMinutes: 30,
      reason: '',
      notes: '',
    },
  });

  const selectedDoctor = form.watch('doctorId');
  const selectedDate = form.watch('scheduledDate');
  const selectedDuration = Number(form.watch('durationMinutes')) || 30;

  const slots = useAvailableSlots(selectedDoctor || undefined, selectedDate, selectedDuration);

  useEffect(() => {
    // When doctor changes, if their department is known, pre-fill it.
    if (selectedDoctor) {
      const d = doctors.data?.items.find((x) => x.id === selectedDoctor);
      if (d?.departmentId && !form.getValues('departmentId')) {
        form.setValue('departmentId', d.departmentId);
      }
    }
  }, [selectedDoctor, doctors.data, form]);

  const book = useBookAppointment();

  const onSubmit = form.handleSubmit((values) => {
    const iso = new Date(`${values.scheduledDate}T${values.scheduledTime}`).toISOString();
    book.mutate(
      {
        patientId: values.patientId,
        doctorId: values.doctorId,
        departmentId: values.departmentId || null,
        scheduledStart: iso,
        durationMinutes: Number(values.durationMinutes),
        reason: values.reason?.trim() || null,
        notes: values.notes?.trim() || null,
      },
      { onSuccess: (appt) => navigate(`/appointments/${appt.id}`) },
    );
  });

  return (
    <div className="max-w-3xl">
      <PageHeader
        eyebrow="Appointments"
        title="Book appointment"
        description="Pick a patient, doctor, and time. Available slots are computed from the doctor's working hours."
      />

      <form onSubmit={onSubmit} noValidate>
        {book.isError && <Alert variant="error" className="mb-4">{humanizeError(book.error)}</Alert>}

        <div className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-5">
          <section className="space-y-3">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Patient</h3>
            <Input
              placeholder="Search by MRN, name, phone…"
              value={patientSearch}
              onChange={(e) => setPatientSearch(e.target.value)}
            />
            <FormField id="patientId" label="" required error={form.formState.errors.patientId?.message}>
              <Select id="patientId" invalid={!!form.formState.errors.patientId} {...form.register('patientId')}>
                <option value="">Select a patient…</option>
                {patients.data?.items.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.fullName} — {p.mrn} · {p.age} yrs
                  </option>
                ))}
              </Select>
            </FormField>
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Doctor</h3>
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
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">When</h3>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <FormField id="scheduledDate" label="Date" required error={form.formState.errors.scheduledDate?.message}>
                <Input id="scheduledDate" type="date" min={today} {...form.register('scheduledDate')} />
              </FormField>
              <FormField id="scheduledTime" label="Time" required error={form.formState.errors.scheduledTime?.message}>
                <Input id="scheduledTime" type="time" step={300} {...form.register('scheduledTime')} />
              </FormField>
              <FormField id="durationMinutes" label="Duration (min)" required error={form.formState.errors.durationMinutes?.message}>
                <Select id="durationMinutes" {...form.register('durationMinutes')}>
                  <option value={15}>15 min</option>
                  <option value={30}>30 min</option>
                  <option value={45}>45 min</option>
                  <option value={60}>60 min</option>
                </Select>
              </FormField>
            </div>

            {selectedDoctor && (
              <div className="rounded-lg border border-gray-100 dark:border-gray-800 p-3">
                <p className="text-xs uppercase tracking-widest text-gray-400 mb-2">
                  Available slots on {selectedDate}
                </p>
                {slots.isPending && (
                  <div className="flex items-center text-sm text-gray-500">
                    <Spinner className="h-4 w-4 mr-2" /> Loading slots…
                  </div>
                )}
                {slots.isError && <Alert variant="error">{humanizeError(slots.error)}</Alert>}
                {slots.data && slots.data.slots.length === 0 && (
                  <p className="text-sm text-gray-500">No available slots — try another date or duration.</p>
                )}
                {slots.data && slots.data.slots.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {slots.data.slots.map((s) => {
                      const time = new Date(s.start);
                      const hh = String(time.getHours()).padStart(2, '0');
                      const mm = String(time.getMinutes()).padStart(2, '0');
                      const label = `${hh}:${mm}`;
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
          </section>

          <section className="space-y-3">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Details</h3>
            <FormField id="reason" label="Reason for visit" error={form.formState.errors.reason?.message}>
              <Input id="reason" {...form.register('reason')} />
            </FormField>
            <FormField id="notes" label="Notes" error={form.formState.errors.notes?.message}>
              <Textarea id="notes" rows={3} {...form.register('notes')} />
            </FormField>
          </section>
        </div>

        <div className="flex items-center justify-end gap-3 mt-6">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={book.isPending}>Book appointment</Button>
        </div>
      </form>
    </div>
  );
}
