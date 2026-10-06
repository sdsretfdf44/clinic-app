import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  PageHeader,
  Spinner,
  Textarea,
} from '@/components/ui';
import {
  PORTAL_APPT_STATUS_LABEL,
  PORTAL_APPT_STATUS_TONE,
  useCancelPortalAppointment,
  usePortalAppointment,
} from '@/features/portal';

export default function PortalAppointmentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const appointment = usePortalAppointment(id);
  const cancel = useCancelPortalAppointment();

  const [cancelOpen, setCancelOpen] = useState(false);
  const [reason, setReason] = useState('');

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

  return (
    <div className="space-y-6 max-w-4xl">
      <PageHeader
        eyebrow={<Link to="/portal/appointments" className="hover:text-brand-600">← Back to appointments</Link>}
        title={a.appointmentNumber}
        description={
          <span className="flex items-center gap-2 flex-wrap">
            <span>{new Date(a.scheduledStart).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}</span>
            <span className="text-gray-400">→</span>
            <span>{new Date(a.scheduledEnd).toLocaleTimeString(undefined, { timeStyle: 'short' })}</span>
            <Badge tone={PORTAL_APPT_STATUS_TONE[a.status]}>{PORTAL_APPT_STATUS_LABEL[a.status]}</Badge>
          </span>
        }
        actions={
          a.canCancel && (
            <Button variant="danger" onClick={() => setCancelOpen(true)}>Cancel appointment</Button>
          )
        }
      />

      {a.status === 'Requested' && (
        <Alert variant="info">
          Your request is pending confirmation. The clinic will review it and get back to you.
        </Alert>
      )}
      {a.status === 'Cancelled' && a.cancellationReason && (
        <Alert variant="warning" title="Cancelled">
          Reason: {a.cancellationReason}
        </Alert>
      )}

      <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card label="Doctor">
          <Row label="Name" value={a.doctorName} />
          <Row label="Specialty" value={a.doctorSpecialty ?? '—'} />
          <Row label="Department" value={a.departmentName ?? '—'} />
        </Card>
        <Card label="Details">
          <Row label="Reason" value={a.reason ?? '—'} />
          <Row
            label="Fee"
            value={
              a.consultationFee != null
                ? `${a.consultationFee.toFixed(2)} ${a.currency ?? ''}`
                : '—'
            }
          />
          <Row label="Requested on" value={new Date(a.createdOn).toLocaleString()} />
        </Card>
      </section>

      {cancel.isError && <Alert variant="error">{humanizeError(cancel.error)}</Alert>}

      <ConfirmDialog
        open={cancelOpen}
        onClose={() => { setCancelOpen(false); setReason(''); }}
        onConfirm={() =>
          cancel.mutate(
            { id: a.id, reason: reason.trim() },
            {
              onSuccess: () => {
                setCancelOpen(false);
                setReason('');
                appointment.refetch();
              },
            },
          )
        }
        title="Cancel this appointment?"
        description={
          <div className="space-y-2">
            <p>Tell us why you're cancelling. This helps your clinic follow up.</p>
            <Textarea
              rows={3}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Reason for cancellation"
            />
          </div>
        }
        variant="danger"
        confirmLabel="Cancel appointment"
        isLoading={cancel.isPending}
      />
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

function Row({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-widest text-gray-400">{label}</p>
      <p className="text-sm text-gray-900 dark:text-gray-100">{value}</p>
    </div>
  );
}
