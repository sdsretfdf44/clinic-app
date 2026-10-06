import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusCircleIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  DataTable,
  EmptyState,
  FilterBar,
  FilterField,
  PageHeader,
  Pagination,
  Select,
  Switch,
  type Column,
} from '@/components/ui';
import {
  PORTAL_APPT_STATUS_LABEL,
  PORTAL_APPT_STATUS_TONE,
  usePortalAppointments,
  type PortalAppointmentStatus,
  type PortalAppointmentSummary,
} from '@/features/portal';

export default function PortalAppointmentsListPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState<PortalAppointmentStatus | ''>('');
  const [upcomingOnly, setUpcomingOnly] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);

  const params = useMemo(() => ({
    pageNumber,
    pageSize: 20,
    status: status || undefined,
    upcomingOnly: upcomingOnly || undefined,
  }), [pageNumber, status, upcomingOnly]);

  const list = usePortalAppointments(params);

  const columns: Column<PortalAppointmentSummary>[] = [
    {
      key: 'when',
      header: 'When',
      render: (a) => (
        <div>
          <p className="text-body font-medium text-gray-900 dark:text-white tabular-nums">
            {new Date(a.scheduledStart).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}
          </p>
          <p className="text-caption text-gray-500 font-mono">{a.appointmentNumber}</p>
        </div>
      ),
    },
    {
      key: 'doctor',
      header: 'Doctor',
      render: (a) => (
        <div className="flex items-center gap-3">
          <Avatar name={a.doctorName} size="sm" />
          <div className="min-w-0">
            <p className="text-body text-gray-900 dark:text-white truncate">{a.doctorName}</p>
            <p className="text-caption text-gray-500">{a.doctorSpecialty ?? a.departmentName ?? '—'}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'reason',
      header: 'Reason',
      render: (a) =>
        a.reason ? (
          <span className="text-body text-gray-700 dark:text-gray-200 line-clamp-1">{a.reason}</span>
        ) : (
          <span className="text-gray-400">—</span>
        ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (a) => (
        <Badge tone={PORTAL_APPT_STATUS_TONE[a.status]} variant="dot">
          {PORTAL_APPT_STATUS_LABEL[a.status]}
        </Badge>
      ),
    },
    {
      key: 'fee',
      header: 'Fee',
      align: 'right',
      render: (a) =>
        a.consultationFee != null ? (
          <span className="text-body text-gray-900 dark:text-white tabular-nums">
            {a.consultationFee.toFixed(2)} {a.currency ?? ''}
          </span>
        ) : (
          <span className="text-gray-400">—</span>
        ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Patient portal"
        title="My appointments"
        description="Track requests, confirmations, and past visits."
        actions={
          <Button
            onClick={() => navigate('/portal/appointments/new')}
            leftIcon={<PlusCircleIcon className="h-4 w-4" />}
          >
            Request appointment
          </Button>
        }
      />

      <FilterBar>
        <FilterField label="Status">
          <Select
            value={status}
            className="h-9 w-56"
            onChange={(e) => {
              setStatus(e.target.value as PortalAppointmentStatus | '');
              setPageNumber(1);
            }}
          >
            <option value="">All statuses</option>
            <option value="Requested">Requested</option>
            <option value="Booked">Booked</option>
            <option value="Confirmed">Confirmed</option>
            <option value="CheckedIn">Checked in</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
            <option value="NoShow">No-show</option>
          </Select>
        </FilterField>
        <div className="ml-auto flex items-center gap-2 pb-1">
          <Switch
            checked={upcomingOnly}
            onCheckedChange={(v) => {
              setUpcomingOnly(v);
              setPageNumber(1);
            }}
            label="Upcoming only"
          />
        </div>
      </FilterBar>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(a) => a.id}
        isLoading={list.isPending}
        onRowClick={(a) => navigate(`/portal/appointments/${a.id}`)}
        emptyState={
          <EmptyState
            title="No appointments yet"
            description="Request an appointment and your clinic will confirm the time."
            action={
              <Button
                onClick={() => navigate('/portal/appointments/new')}
                leftIcon={<PlusCircleIcon className="h-4 w-4" />}
              >
                Request appointment
              </Button>
            }
          />
        }
      />

      {list.data && (
        <Pagination
          pageNumber={list.data.pageNumber}
          pageSize={list.data.pageSize}
          totalCount={list.data.totalCount}
          totalPages={list.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}
    </div>
  );
}
