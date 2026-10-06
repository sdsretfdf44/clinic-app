import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';

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
  requestAppointmentSchema,
  useRequestAppointment,
  usePortalDoctors,
  type RequestAppointmentFormValues,
} from '@/features/portal';

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function RequestAppointmentPage() {
  const navigate = useNavigate();
  const [departmentFilter, setDepartmentFilter] = useState('');
  const doctors = usePortalDoctors({ departmentId: departmentFilter || undefined });
  const request = useRequestAppointment();

  const tomorrow = useMemo(() => {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return toDateInputValue(d);
  }, []);

  const form = useForm<RequestAppointmentFormValues>({
    resolver: zodResolver(requestAppointmentSchema),
    defaultValues: {
      doctorId: '',
      scheduledDate: tomorrow,
      scheduledTime: '10:00',
      durationMinutes: 30,
      reason: '',
    },
  });

  const selectedDoctorId = form.watch('doctorId');
  const selectedDoctor = doctors.data?.find((d) => d.id === selectedDoctorId) ?? null;

  const departments = useMemo(() => {
    const seen = new Map<string, string>();
    doctors.data?.forEach((d) => {
      if (d.departmentId && d.departmentName) seen.set(d.departmentId, d.departmentName);
    });
    return Array.from(seen, ([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name));
  }, [doctors.data]);

  const handle = form.handleSubmit((values) => {
    const iso = new Date(`${values.scheduledDate}T${values.scheduledTime}`).toISOString();
    const doctor = doctors.data?.find((d) => d.id === values.doctorId);
    request.mutate(
      {
        doctorId: values.doctorId,
        departmentId: doctor?.departmentId ?? null,
        scheduledStart: iso,
        durationMinutes: Number(values.durationMinutes),
        reason: values.reason?.trim() || null,
      },
      { onSuccess: (data) => navigate(`/portal/appointments/${data.id}`) },
    );
  });

  return (
    <div className="space-y-6 max-w-3xl">
      <PageHeader
        eyebrow={<Link to="/portal/appointments" className="hover:text-brand-600">← Back to appointments</Link>}
        title="Request an appointment"
        description="Your clinic will review the request and confirm or suggest an alternate time."
      />

      {request.isError && <Alert variant="error">{humanizeError(request.error)}</Alert>}

      <form onSubmit={handle} className="space-y-6" noValidate>
        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Choose a doctor</h2>

          {departments.length > 0 && (
            <FormField id="departmentFilter" label="Filter by department">
              <Select
                id="departmentFilter"
                value={departmentFilter}
                onChange={(e) => setDepartmentFilter(e.target.value)}
              >
                <option value="">All departments</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>{d.name}</option>
                ))}
              </Select>
            </FormField>
          )}

          {doctors.isPending ? (
            <div className="flex items-center text-sm text-gray-500 py-6">
              <Spinner className="h-4 w-4 mr-2" /> Loading doctors…
            </div>
          ) : doctors.isError ? (
            <Alert variant="error">{humanizeError(doctors.error)}</Alert>
          ) : (doctors.data ?? []).length === 0 ? (
            <p className="text-sm text-gray-500 py-4">No doctors are accepting appointments right now.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {doctors.data!.map((d) => {
                const active = selectedDoctorId === d.id;
                return (
                  <button
                    key={d.id}
                    type="button"
                    onClick={() => form.setValue('doctorId', d.id, { shouldValidate: true })}
                    className={[
                      'text-left rounded-xl border p-4 transition',
                      active
                        ? 'border-brand-500 bg-brand-50/50 dark:bg-brand-500/10'
                        : 'border-gray-100 dark:border-gray-800 hover:border-brand-300',
                    ].join(' ')}
                  >
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{d.fullName}</p>
                    <p className="text-xs text-gray-500 mt-0.5">
                      {d.specialty ?? '—'}{d.departmentName ? ` · ${d.departmentName}` : ''}
                    </p>
                    {d.consultationFee != null && (
                      <p className="text-xs text-gray-500 mt-1">
                        Fee: {d.consultationFee.toFixed(2)} {d.currency}
                      </p>
                    )}
                  </button>
                );
              })}
            </div>
          )}
          {form.formState.errors.doctorId?.message && (
            <p className="text-xs text-red-600">{form.formState.errors.doctorId.message}</p>
          )}
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Preferred time</h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="scheduledDate" label="Date" required error={form.formState.errors.scheduledDate?.message}>
              <Input id="scheduledDate" type="date" min={toDateInputValue(new Date())} {...form.register('scheduledDate')} />
            </FormField>
            <FormField id="scheduledTime" label="Time" required error={form.formState.errors.scheduledTime?.message}>
              <Input id="scheduledTime" type="time" step={300} {...form.register('scheduledTime')} />
            </FormField>
            <FormField id="durationMinutes" label="Expected duration">
              <Select id="durationMinutes" {...form.register('durationMinutes')}>
                <option value={15}>15 min</option>
                <option value={30}>30 min</option>
                <option value={45}>45 min</option>
                <option value={60}>60 min</option>
              </Select>
            </FormField>
          </div>
          {selectedDoctor && (
            <p className="text-xs text-gray-500">
              You've selected <strong>{selectedDoctor.fullName}</strong>. The clinic will confirm your slot.
            </p>
          )}
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Reason (optional)</h2>
          <FormField id="reason" label="Briefly describe your symptoms or reason for visit">
            <Textarea id="reason" rows={4} {...form.register('reason')} />
          </FormField>
        </section>

        <div className="flex items-center justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate('/portal/appointments')}>
            Cancel
          </Button>
          <Button type="submit" isLoading={request.isPending}>Submit request</Button>
        </div>
      </form>
    </div>
  );
}
