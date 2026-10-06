import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon } from '@heroicons/react/24/outline';

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
  FilterSearchInput,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import { useAppointmentsList, type AppointmentStatus, type AppointmentSummary } from '@/features/appointments';
import { useDepartments } from '@/features/clinic/departments';
import { useDoctorsList } from '@/features/clinic/doctors';

import { STATUS_LABEL, STATUS_TONE } from './statusMeta';

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function AppointmentsListPage() {
  const navigate = useNavigate();

  const today = useMemo(() => toDateInputValue(new Date()), []);
  const [dateFrom, setDateFrom] = useState(today);
  const [dateTo, setDateTo] = useState(today);
  const [status, setStatus] = useState<AppointmentStatus | ''>('');
  const [doctorId, setDoctorId] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  const departments = useDepartments(false);
  const doctors = useDoctorsList({ pageNumber: 1, pageSize: 100, isActive: true });

  const params = useMemo(() => ({
    pageNumber,
    pageSize: 25,
    from: dateFrom ? new Date(`${dateFrom}T00:00:00`).toISOString() : undefined,
    to: dateTo ? new Date(`${dateTo}T23:59:59`).toISOString() : undefined,
    doctorId: doctorId || undefined,
    departmentId: departmentId || undefined,
    status: status || undefined,
    search: search || undefined,
  }), [pageNumber, dateFrom, dateTo, doctorId, departmentId, status, search]);

  const list = useAppointmentsList(params);

  const columns: Column<AppointmentSummary>[] = [
    {
      key: 'time',
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
      key: 'patient',
      header: 'Patient',
      render: (a) => (
        <div className="flex items-center gap-3">
          <Avatar name={a.patientName} size="sm" />
          <div className="min-w-0">
            <p className="text-body text-gray-900 dark:text-white truncate">{a.patientName}</p>
            <p className="text-caption text-gray-500 font-mono">{a.patientMrn}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'doctor',
      header: 'Doctor',
      render: (a) => (
        <div>
          <p className="text-body text-gray-900 dark:text-white truncate">{a.doctorName}</p>
          <p className="text-caption text-gray-500">{a.departmentName ?? '—'}</p>
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
        <Badge tone={STATUS_TONE[a.status]} variant="dot">
          {STATUS_LABEL[a.status]}
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
        eyebrow="Appointments"
        title="Schedule"
        description="Book, reschedule, and advance appointments through their lifecycle."
        actions={
          <Button onClick={() => navigate('/appointments/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
            Book appointment
          </Button>
        }
      />

      <FilterBar>
        <FilterSearchInput
          label="Search"
          placeholder="Number, MRN, or name"
          value={search}
          onClear={() => {
            setSearch('');
            setPageNumber(1);
          }}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <FilterField label="From">
          <Input type="date" value={dateFrom} className="h-9" onChange={(e) => { setDateFrom(e.target.value); setPageNumber(1); }} />
        </FilterField>
        <FilterField label="To">
          <Input type="date" value={dateTo} className="h-9" onChange={(e) => { setDateTo(e.target.value); setPageNumber(1); }} />
        </FilterField>
        <FilterField label="Doctor">
          <Select value={doctorId} className="h-9" onChange={(e) => { setDoctorId(e.target.value); setPageNumber(1); }}>
            <option value="">All doctors</option>
            {doctors.data?.items.map((d) => (
              <option key={d.id} value={d.id}>{d.fullName}</option>
            ))}
          </Select>
        </FilterField>
        <FilterField label="Department">
          <Select value={departmentId} className="h-9" onChange={(e) => { setDepartmentId(e.target.value); setPageNumber(1); }}>
            <option value="">All departments</option>
            {departments.data?.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </Select>
        </FilterField>
        <FilterField label="Status">
          <Select value={status} className="h-9" onChange={(e) => { setStatus(e.target.value as AppointmentStatus | ''); setPageNumber(1); }}>
            <option value="">All statuses</option>
            <option value="Requested">Requested</option>
            <option value="Booked">Booked</option>
            <option value="Confirmed">Confirmed</option>
            <option value="CheckedIn">Checked in</option>
            <option value="InConsultation">In consultation</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
            <option value="NoShow">No-show</option>
          </Select>
        </FilterField>
      </FilterBar>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(a) => a.id}
        isLoading={list.isPending}
        onRowClick={(a) => navigate(`/appointments/${a.id}`)}
        emptyState={
          <EmptyState
            title="No appointments"
            description="Adjust your filters or book a new appointment."
            action={
              <Button onClick={() => navigate('/appointments/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
                Book appointment
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
