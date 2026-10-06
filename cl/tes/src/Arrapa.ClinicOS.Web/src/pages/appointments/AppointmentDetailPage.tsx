import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  Drawer,
  FormField,
  Input,
  PageHeader,
  Select,
  Spinner,
  Textarea,
} from '@/components/ui';
import {
  rescheduleAppointmentSchema,
  useAppointment,
  useAvailableSlots,
  useCancelAppointment,
  useRescheduleAppointment,
  useTransitionAppointment,
  type RescheduleFormValues,
} from '@/features/appointments';
import { useDepartments } from '@/features/clinic/departments';
import { useDoctorsList } from '@/features/clinic/doctors';

import { STATUS_LABEL, STATUS_TONE, availableActions } from './statusMeta';

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function toTimeInputValue(date: Date) {
  const hh = String(date.getHours()).padStart(2, '0');
  const mm = String(date.getMinutes()).padStart(2, '0');
  return `${hh}:${mm}`;
}

export default function AppointmentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const appointmentId = id!;

  const appointment = useAppointment(appointmentId);
  const transition = useTransitionAppointment();
  const cancel = useCancelAppointment();
  const reschedule = useRescheduleAppointment(appointmentId);

  const [rescheduleOpen, setRescheduleOpen] = useState(false);
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  if (appointment.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading appointment…
      </div>
    );
  }
  if (appointment.isError || !appointment.data) {
    return <Alert variant="error">{humanizeError(appointment.error)}</Alert>;
  }

  const a = appointment.data;
  const actions = availableActions(a.status);
  const canReschedule = !['Completed', 'Cancelled', 'NoShow'].includes(a.status);
  const canCancel = !['Completed', 'Cancelled', 'NoShow'].includes(a.status);

  return (
    <div className="space-y-6 max-w-4xl">
      <PageHeader
        eyebrow={<Link to="/appointments" className="hover:text-brand-600">← Back to appointments</Link>}
        title={a.appointmentNumber}
        description={
          <span className="flex items-center gap-2 flex-wrap">
            <span>{new Date(a.scheduledStart).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}</span>
            <span className="text-gray-400">→</span>
            <span>{new Date(a.scheduledEnd).toLocaleTimeString(undefined, { timeStyle: 'short' })}</span>
            <Badge tone={STATUS_TONE[a.status]}>{STATUS_LABEL[a.status]}</Badge>
          </span>
        }
        actions={
          <>
            {actions.map((act) => (
              <Button
                key={act.action}
                variant={act.variant}
                onClick={() =>
                  transition.mutate({ id: a.id, action: act.action }, {
                    onSuccess: () => appointment.refetch(),
                  })
                }
                isLoading={transition.isPending}
              >
                {act.label}
              </Button>
            ))}
            {canReschedule && (
              <Button variant="secondary" onClick={() => setRescheduleOpen(true)}>Reschedule</Button>
            )}
            {canCancel && (
              <Button variant="danger" onClick={() => setCancelOpen(true)}>Cancel</Button>
            )}
          </>
        }
      />

      {transition.isError && <Alert variant="error">{humanizeError(transition.error)}</Alert>}

      <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card label="Patient">
          <Row label="Name" value={<Link to={`/patients/${a.patientId}`} className="text-brand-500 hover:text-brand-600">{a.patientName}</Link>} />
          <Row label="MRN" value={a.patientMrn} mono />
          <Row label="Phone" value={a.patientPhone ?? '—'} />
        </Card>
        <Card label="Doctor">
          <Row label="Name" value={a.doctorName} />
          <Row label="Specialty" value={a.doctorSpecialty ?? '—'} />
          <Row label="Department" value={a.departmentName ?? '—'} />
        </Card>
      </section>

      <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card label="Details">
          <Row label="Reason" value={a.reason ?? '—'} />
          <Row label="Notes" value={a.notes ?? '—'} />
          <Row
            label="Fee"
            value={
              a.consultationFee != null
                ? `${a.consultationFee.toFixed(2)} ${a.currency ?? ''}`
                : '—'
            }
          />
        </Card>
        <Card label="Timeline">
          <TimelineRow label="Booked" ts={a.createdOn} />
          <TimelineRow label="Confirmed" ts={a.confirmedAt} />
          <TimelineRow label="Checked in" ts={a.checkedInAt} />
          <TimelineRow label="Consultation started" ts={a.consultationStartedAt} />
          <TimelineRow label="Consultation ended" ts={a.consultationEndedAt} />
          {a.cancelledAt && <TimelineRow label="Cancelled" ts={a.cancelledAt} />}
        </Card>
      </section>

      {a.status === 'Cancelled' && a.cancellationReason && (
        <Alert variant="warning" title="Cancelled">
          Reason: {a.cancellationReason}
        </Alert>
      )}

      <RescheduleDrawer
        open={rescheduleOpen}
        onClose={() => setRescheduleOpen(false)}
        appointment={a}
        onSubmit={(payload) =>
          reschedule.mutate(payload, {
            onSuccess: () => {
              setRescheduleOpen(false);
              appointment.refetch();
            },
          })
        }
        isSubmitting={reschedule.isPending}
        error={reschedule.error}
      />

      <ConfirmDialog
        open={cancelOpen}
        onClose={() => { setCancelOpen(false); setCancelReason(''); }}
        onConfirm={() =>
          cancel.mutate(
            { id: a.id, reason: cancelReason.trim() },
            {
              onSuccess: () => {
                setCancelOpen(false);
                setCancelReason('');
                appointment.refetch();
              },
            },
          )
        }
        title="Cancel appointment"
        description={
          <div className="space-y-2">
            <p>Provide a reason. The patient will not be automatically notified.</p>
            <Textarea
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="Reason for cancellation"
            />
          </div>
        }
        variant="danger"
        confirmLabel="Cancel appointment"
        isLoading={cancel.isPending}
      />

      {cancel.isError && <Alert variant="error">{humanizeError(cancel.error)}</Alert>}
    </div>
  );
}

function Card({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
      <p className="text-xs uppercase tracking-widest text-gray-400 mb-3">{label}</p>
      <div className="space-y-2">{children}</div>
    </div>
  );
}

function Row({ label, value, mono }: { label: string; value: React.ReactNode; mono?: boolean }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-widest text-gray-400">{label}</p>
      <p className={`text-sm text-gray-900 dark:text-gray-100 ${mono ? 'font-mono' : ''}`}>{value}</p>
    </div>
  );
}

function TimelineRow({ label, ts }: { label: string; ts: string | null }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <span className="text-xs text-gray-500">{label}</span>
      <span className="text-xs text-gray-700 dark:text-gray-200">
        {ts ? new Date(ts).toLocaleString() : <span className="text-gray-400">—</span>}
      </span>
    </div>
  );
}

// -------------------- Reschedule drawer --------------------
function RescheduleDrawer({
  open,
  onClose,
  appointment,
  onSubmit,
  isSubmitting,
  error,
}: {
  open: boolean;
  onClose: () => void;
  appointment: import('@/features/appointments').AppointmentDetail;
  onSubmit: (payload: {
    doctorId: string;
    departmentId: string | null;
    scheduledStart: string;
    durationMinutes: number;
    reason: string | null;
    notes: string | null;
  }) => void;
  isSubmitting: boolean;
  error: unknown;
}) {
  const departments = useDepartments(false);
  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 100, isActive: true });

  const startDate = useMemo(() => new Date(appointment.scheduledStart), [appointment.scheduledStart]);
  const durationMinutes = Math.max(
    5,
    Math.round(
      (new Date(appointment.scheduledEnd).getTime() - startDate.getTime()) / 60000,
    ),
  );

  const form = useForm<RescheduleFormValues>({
    resolver: zodResolver(rescheduleAppointmentSchema),
    defaultValues: {
      doctorId: appointment.doctorId,
      departmentId: appointment.departmentId ?? '',
      scheduledDate: toDateInputValue(startDate),
      scheduledTime: toTimeInputValue(startDate),
      durationMinutes,
      reason: appointment.reason ?? '',
      notes: appointment.notes ?? '',
    },
  });

  useEffect(() => {
    if (open) {
      form.reset({
        doctorId: appointment.doctorId,
        departmentId: appointment.departmentId ?? '',
        scheduledDate: toDateInputValue(startDate),
        scheduledTime: toTimeInputValue(startDate),
        durationMinutes,
        reason: appointment.reason ?? '',
        notes: appointment.notes ?? '',
      });
    }
  }, [open, appointment, startDate, durationMinutes, form]);

  const selectedDoctor = form.watch('doctorId');
  const selectedDate = form.watch('scheduledDate');
  const selectedDuration = Number(form.watch('durationMinutes')) || 30;
  const slots = useAvailableSlots(selectedDoctor || undefined, selectedDate, selectedDuration);

  const handle = form.handleSubmit((values) => {
    const iso = new Date(`${values.scheduledDate}T${values.scheduledTime}`).toISOString();
    onSubmit({
      doctorId: values.doctorId,
      departmentId: values.departmentId || null,
      scheduledStart: iso,
      durationMinutes: Number(values.durationMinutes),
      reason: values.reason?.trim() || null,
      notes: values.notes?.trim() || null,
    });
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title="Reschedule appointment"
      size="lg"
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={handle} isLoading={isSubmitting}>Save</Button>
        </div>
      }
    >
      {error !== null && error !== undefined && (
        <Alert variant="error" className="mb-4">{humanizeError(error)}</Alert>
      )}
      <form onSubmit={handle} className="space-y-4" noValidate>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="doctorId" label="Doctor" required error={form.formState.errors.doctorId?.message}>
            <Select id="doctorId" {...form.register('doctorId')}>
              <option value="">Select…</option>
              {doctors.data?.items.map((d) => (
                <option key={d.id} value={d.id}>{d.fullName}</option>
              ))}
            </Select>
          </FormField>
          <FormField id="departmentId" label="Department">
            <Select id="departmentId" {...form.register('departmentId')}>
              <option value="">— none —</option>
              {departments.data?.map((d) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </Select>
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <FormField id="scheduledDate" label="Date" required>
            <Input id="scheduledDate" type="date" {...form.register('scheduledDate')} />
          </FormField>
          <FormField id="scheduledTime" label="Time" required>
            <Input id="scheduledTime" type="time" step={300} {...form.register('scheduledTime')} />
          </FormField>
          <FormField id="durationMinutes" label="Duration">
            <Select id="durationMinutes" {...form.register('durationMinutes')}>
              <option value={15}>15 min</option>
              <option value={30}>30 min</option>
              <option value={45}>45 min</option>
              <option value={60}>60 min</option>
            </Select>
          </FormField>
        </div>

        {selectedDoctor && slots.data && slots.data.slots.length > 0 && (
          <div className="rounded-lg border border-gray-100 dark:border-gray-800 p-3">
            <p className="text-xs uppercase tracking-widest text-gray-400 mb-2">Slots on {selectedDate}</p>
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
          </div>
        )}

        <FormField id="reason" label="Reason">
          <Input id="reason" {...form.register('reason')} />
        </FormField>
        <FormField id="notes" label="Notes">
          <Textarea id="notes" rows={3} {...form.register('notes')} />
        </FormField>
      </form>
    </Drawer>
  );
}
