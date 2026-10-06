import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  DataTable,
  EmptyState,
  PageHeader,
  Spinner,
  type Column,
} from '@/components/ui';
import { useAppointmentsList, useTransitionAppointment, type AppointmentSummary } from '@/features/appointments';
import { useDoctorsList } from '@/features/clinic/doctors';
import { useAuthStore } from '@/store/authStore';

import { STATUS_LABEL, STATUS_TONE, availableActions } from '../appointments/statusMeta';

function todayBoundsIso() {
  const now = new Date();
  const start = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);
  const end = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59);
  return { from: start.toISOString(), to: end.toISOString() };
}

export default function TodayQueuePage() {
  const navigate = useNavigate();
  const currentUser = useAuthStore((s) => s.user);

  // Find the Doctor record whose UserId matches the currently signed-in user.
  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 200, isActive: true });
  const currentDoctor = useMemo(
    () => doctors.data?.items.find((d) => d.userId === currentUser?.id),
    [doctors.data, currentUser?.id],
  );

  const { from, to } = useMemo(todayBoundsIso, []);
  const list = useAppointmentsList({
    pageNumber: 1,
    pageSize: 100,
    from,
    to,
    doctorId: currentDoctor?.id,
  });

  const transition = useTransitionAppointment();

  const columns: Column<AppointmentSummary>[] = [
    {
      key: 'time',
      header: 'Time',
      render: (a) => (
        <div>
          <p className="text-sm font-medium text-gray-900 dark:text-white">
            {new Date(a.scheduledStart).toLocaleTimeString(undefined, { timeStyle: 'short' })}
          </p>
          <p className="text-xs text-gray-500 font-mono">{a.appointmentNumber}</p>
        </div>
      ),
    },
    {
      key: 'patient',
      header: 'Patient',
      render: (a) => (
        <div>
          <p className="text-sm text-gray-900 dark:text-white">{a.patientName}</p>
          <p className="text-xs text-gray-500 font-mono">{a.patientMrn}</p>
        </div>
      ),
    },
    { key: 'reason', header: 'Reason', render: (a) => a.reason ?? <span className="text-gray-400">—</span> },
    { key: 'status', header: 'Status', render: (a) => <Badge tone={STATUS_TONE[a.status]}>{STATUS_LABEL[a.status]}</Badge> },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (a) => {
        const actions = availableActions(a.status);
        if (a.status === 'InConsultation') {
          return (
            <Button
              size="sm"
              onClick={(e) => { e.stopPropagation(); navigate(`/doctor/consultation/${a.id}`); }}
            >
              Open consultation
            </Button>
          );
        }
        if (a.status === 'CheckedIn') {
          return (
            <Button
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                transition.mutate(
                  { id: a.id, action: 'start' },
                  { onSuccess: () => navigate(`/doctor/consultation/${a.id}`) },
                );
              }}
              isLoading={transition.isPending}
            >
              Start consultation
            </Button>
          );
        }
        return (
          <div className="flex items-center justify-end gap-2">
            {actions.slice(0, 2).map((act) => (
              <Button
                key={act.action}
                size="sm"
                variant={act.variant}
                onClick={(e) => {
                  e.stopPropagation();
                  transition.mutate({ id: a.id, action: act.action });
                }}
              >
                {act.label}
              </Button>
            ))}
          </div>
        );
      },
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Doctor"
        title="Today's queue"
        description={
          currentDoctor
            ? `Your appointments for today, ${new Date().toLocaleDateString(undefined, { dateStyle: 'full' })}.`
            : 'Your appointments for today.'
        }
      />

      {doctors.isPending && (
        <div className="flex items-center justify-center py-10 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading doctor profile…
        </div>
      )}
      {doctors.data && !currentDoctor && (
        <Alert variant="warning" title="No doctor profile">
          Your account is not linked to a doctor record. Ask your clinic admin to set this up.
        </Alert>
      )}
      {transition.isError && <Alert variant="error">{humanizeError(transition.error)}</Alert>}
      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      {currentDoctor && (
        <DataTable
          columns={columns}
          rows={list.data?.items ?? []}
          keyExtractor={(a) => a.id}
          isLoading={list.isPending}
          onRowClick={(a) => navigate(`/appointments/${a.id}`)}
          emptyState={
            <EmptyState
              title="No appointments today"
              description="Enjoy the calm — or check the full schedule."
              action={<Button onClick={() => navigate('/appointments')}>View all appointments</Button>}
            />
          }
        />
      )}
    </div>
  );
}
