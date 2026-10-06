import { useState } from 'react';
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
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import { usePatientsList } from '@/features/patients/hooks';
import { BLOOD_GROUP_LABEL, GENDER_OPTIONS, type PatientSummary } from '@/features/patients/types';

export default function PatientsListPage() {
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(1);
  const [search, setSearch] = useState('');
  const [gender, setGender] = useState('');
  const [activeFilter, setActiveFilter] = useState<'all' | 'active' | 'inactive'>('active');

  const params = {
    pageNumber,
    pageSize: 20,
    search: search || undefined,
    gender: gender || undefined,
    isActive: activeFilter === 'all' ? undefined : activeFilter === 'active',
  };
  const list = usePatientsList(params);

  const columns: Column<PatientSummary>[] = [
    {
      key: 'patient',
      header: 'Patient',
      render: (p) => (
        <div className="flex items-center gap-3">
          <Avatar name={p.fullName} src={p.photoUrl} size="sm" />
          <div className="min-w-0">
            <p className="font-medium text-gray-900 dark:text-white truncate">{p.fullName}</p>
            <p className="text-caption text-gray-500 font-mono">{p.mrn}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'demo',
      header: 'Demographics',
      render: (p) => (
        <div className="text-body text-gray-700 dark:text-gray-200">
          <span className="tabular-nums">{p.age} yrs</span>
          <span className="text-gray-400"> · </span>
          <span>{p.gender}</span>
        </div>
      ),
    },
    { key: 'blood', header: 'Blood', render: (p) => <Badge tone="red">{BLOOD_GROUP_LABEL[p.bloodGroup]}</Badge> },
    {
      key: 'contact',
      header: 'Contact',
      render: (p) => (
        <div className="text-body">
          {p.phone ? (
            <p className="text-gray-800 dark:text-gray-100">{p.phone}</p>
          ) : (
            <p className="text-gray-400">—</p>
          )}
          {p.email && <p className="text-caption text-gray-500 truncate max-w-[200px]">{p.email}</p>}
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (p) => (
        <Badge tone={p.isActive ? 'green' : 'gray'} variant="dot">
          {p.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'last',
      header: 'Last visit',
      align: 'right',
      render: (p) =>
        p.lastVisitAt ? (
          <span className="text-body text-gray-700 dark:text-gray-200 tabular-nums">
            {new Date(p.lastVisitAt).toLocaleDateString()}
          </span>
        ) : (
          <span className="text-gray-400">—</span>
        ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Patients"
        title="Registered patients"
        description="Search, register, and manage patient records. Click a row for the full chart."
        actions={
          <Button onClick={() => navigate('/patients/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
            Register patient
          </Button>
        }
      />

      <FilterBar>
        <FilterSearchInput
          label="Search"
          placeholder="MRN, name, phone, or email"
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
        <FilterField label="Gender">
          <Select
            value={gender}
            onChange={(e) => {
              setGender(e.target.value);
              setPageNumber(1);
            }}
            className="h-9"
          >
            <option value="">All</option>
            {GENDER_OPTIONS.filter((g) => g !== 'Unknown').map((g) => (
              <option key={g} value={g}>
                {g}
              </option>
            ))}
          </Select>
        </FilterField>
        <FilterField label="Status">
          <Select
            value={activeFilter}
            onChange={(e) => {
              setActiveFilter(e.target.value as typeof activeFilter);
              setPageNumber(1);
            }}
            className="h-9"
          >
            <option value="all">All</option>
            <option value="active">Active only</option>
            <option value="inactive">Inactive only</option>
          </Select>
        </FilterField>
      </FilterBar>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(p) => p.id}
        isLoading={list.isPending}
        onRowClick={(p) => navigate(`/patients/${p.id}`)}
        emptyState={
          <EmptyState
            title="No patients yet"
            description="Register your first patient to start building your clinic's chart."
            action={
              <Button onClick={() => navigate('/patients/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
                Register patient
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
