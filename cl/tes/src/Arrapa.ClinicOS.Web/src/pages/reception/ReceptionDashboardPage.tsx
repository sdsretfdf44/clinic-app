import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import { Alert, Badge, Button, EmptyState, PageHeader, Select, Spinner } from '@/components/ui';
import { useTransitionAppointment } from '@/features/appointments';
import { useDepartments } from '@/features/clinic/departments';
import { useDoctorsList } from '@/features/clinic/doctors';
import { useReceptionQueue, type ReceptionQueueBucket, type ReceptionQueueEntry } from '@/features/reception';

import { STATUS_LABEL, STATUS_TONE } from '../appointments/statusMeta';

function timeOf(iso: string) {
  return new Date(iso).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
}

function waitMinutes(from: string | null) {
  if (!from) return null;
  const ms = Date.now() - new Date(from).getTime();
  return Math.max(0, Math.floor(ms / 60000));
}

export default function ReceptionDashboardPage() {
  const navigate = useNavigate();
  const [doctorId, setDoctorId] = useState('');
  const [departmentId, setDepartmentId] = useState('');

  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 200, isActive: true });
  const departments = useDepartments(false);

  const queue = useReceptionQueue({
    doctorId: doctorId || undefined,
    departmentId: departmentId || undefined,
  });

  const transition = useTransitionAppointment();

  const columns = useMemo(
    () => [
      { key: 'expectedToArrive', title: 'Expected today', tone: 'sky' as const, action: 'checkIn' as const, actionLabel: 'Check in' },
      { key: 'waitingForConsultation', title: 'Waiting for doctor', tone: 'amber' as const, action: null },
      { key: 'inConsultation', title: 'In consultation', tone: 'brand' as const, action: null },
      { key: 'doneToday', title: 'Done today', tone: 'green' as const, action: null },
    ],
    [],
  );

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Reception"
        title="Today's front desk"
        description={
          queue.data
            ? `Auto-refreshes every 30 seconds. As of ${new Date(queue.data.asOf).toLocaleTimeString()}.`
            : 'Live queue for today.'
        }
        actions={
          <>
            <Button variant="secondary" onClick={() => navigate('/reception/search')}>Search patient</Button>
            <Button onClick={() => navigate('/reception/walk-in')}>Register walk-in</Button>
          </>
        }
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div>
          <label className="text-xs text-gray-500 dark:text-gray-400 block mb-1">Doctor</label>
          <Select value={doctorId} onChange={(e) => setDoctorId(e.target.value)}>
            <option value="">All doctors</option>
            {doctors.data?.items.map((d) => (
              <option key={d.id} value={d.id}>{d.fullName}</option>
            ))}
          </Select>
        </div>
        <div>
          <label className="text-xs text-gray-500 dark:text-gray-400 block mb-1">Department</label>
          <Select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
            <option value="">All departments</option>
            {departments.data?.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </Select>
        </div>
      </div>

      {queue.isError && <Alert variant="error">{humanizeError(queue.error)}</Alert>}
      {transition.isError && <Alert variant="error">{humanizeError(transition.error)}</Alert>}

      {queue.isPending && (
        <div className="flex items-center justify-center py-16 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading queue…
        </div>
      )}

      {queue.data && (
        <>
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
            {columns.map((col) => {
              const bucket: ReceptionQueueBucket = queue.data![col.key as keyof typeof queue.data] as ReceptionQueueBucket;
              return (
                <QueueColumn
                  key={col.key}
                  title={col.title}
                  tone={col.tone}
                  bucket={bucket}
                  onCheckIn={
                    col.action === 'checkIn'
                      ? (id) =>
                          transition.mutate(
                            { id, action: 'checkIn' },
                            { onSuccess: () => queue.refetch() },
                          )
                      : undefined
                  }
                  onNoShow={
                    col.key === 'expectedToArrive'
                      ? (id) =>
                          transition.mutate(
                            { id, action: 'noShow' },
                            { onSuccess: () => queue.refetch() },
                          )
                      : undefined
                  }
                  transitionPending={transition.isPending}
                />
              );
            })}
          </div>

          {queue.data.noShow.count > 0 && (
            <div className="mt-4">
              <QueueColumn title="No-show" tone="red" bucket={queue.data.noShow} />
            </div>
          )}
        </>
      )}
    </div>
  );
}

function QueueColumn({
  title,
  tone,
  bucket,
  onCheckIn,
  onNoShow,
  transitionPending,
}: {
  title: string;
  tone: 'sky' | 'amber' | 'brand' | 'green' | 'red';
  bucket: ReceptionQueueBucket;
  onCheckIn?: (appointmentId: string) => void;
  onNoShow?: (appointmentId: string) => void;
  transitionPending?: boolean;
}) {
  return (
    <div className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-4 flex flex-col min-h-[280px]">
      <div className="flex items-center justify-between mb-3">
        <p className="text-xs uppercase tracking-widest text-gray-400">{title}</p>
        <Badge tone={tone}>{bucket.count}</Badge>
      </div>

      {bucket.items.length === 0 ? (
        <div className="flex-1 flex items-center justify-center">
          <EmptyState title="—" description="Nothing here" />
        </div>
      ) : (
        <ul className="space-y-3 flex-1">
          {bucket.items.map((entry) => (
            <QueueCard
              key={entry.appointmentId}
              entry={entry}
              onCheckIn={onCheckIn}
              onNoShow={onNoShow}
              transitionPending={transitionPending}
            />
          ))}
        </ul>
      )}
    </div>
  );
}

function QueueCard({
  entry,
  onCheckIn,
  onNoShow,
  transitionPending,
}: {
  entry: ReceptionQueueEntry;
  onCheckIn?: (appointmentId: string) => void;
  onNoShow?: (appointmentId: string) => void;
  transitionPending?: boolean;
}) {
  const navigate = useNavigate();
  const waited = waitMinutes(entry.checkedInAt);

  return (
    <li className="rounded-lg border border-gray-100 dark:border-gray-800 p-3 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <button
            type="button"
            onClick={() => navigate(`/patients/${entry.patientId}`)}
            className="text-sm font-medium text-gray-900 dark:text-white hover:text-brand-500 truncate block text-left"
          >
            {entry.patientName}
          </button>
          <p className="text-xs text-gray-500 font-mono">{entry.patientMrn}</p>
        </div>
        <Badge tone={STATUS_TONE[entry.status as keyof typeof STATUS_TONE]}>
          {STATUS_LABEL[entry.status as keyof typeof STATUS_LABEL]}
        </Badge>
      </div>

      <div className="mt-2 flex items-center justify-between text-xs text-gray-500">
        <span>{timeOf(entry.scheduledStart)} · {entry.doctorName}</span>
        {waited != null && <span className="tabular-nums">wait {waited}m</span>}
      </div>

      {entry.reason && (
        <p className="mt-1.5 text-xs text-gray-600 dark:text-gray-300 line-clamp-2">{entry.reason}</p>
      )}

      <div className="mt-3 flex items-center gap-2">
        {onCheckIn && (
          <Button
            size="sm"
            onClick={() => onCheckIn(entry.appointmentId)}
            isLoading={transitionPending}
          >
            Check in
          </Button>
        )}
        {onNoShow && (
          <Button
            size="sm"
            variant="ghost"
            onClick={() => onNoShow(entry.appointmentId)}
            isLoading={transitionPending}
          >
            No-show
          </Button>
        )}
        <Button
          size="sm"
          variant="secondary"
          onClick={() => navigate(`/appointments/${entry.appointmentId}`)}
        >
          Open
        </Button>
      </div>
    </li>
  );
}
