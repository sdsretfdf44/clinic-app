import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  DataTable,
  EmptyState,
  PageHeader,
  Pagination,
  type Column,
} from '@/components/ui';
import { usePortalPrescriptions, type PortalPrescription } from '@/features/portal';

export default function PortalPrescriptionsPage() {
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(1);

  const params = useMemo(() => ({ pageNumber, pageSize: 20 }), [pageNumber]);
  const list = usePortalPrescriptions(params);

  const columns: Column<PortalPrescription>[] = [
    {
      key: 'issued',
      header: 'Issued',
      render: (p) => (
        <p className="text-sm font-medium text-gray-900 dark:text-white">
          {new Date(p.issuedOn).toLocaleDateString(undefined, { dateStyle: 'medium' })}
        </p>
      ),
    },
    {
      key: 'doctor',
      header: 'Doctor',
      render: (p) => (
        <div>
          <p className="text-sm text-gray-900 dark:text-white">{p.doctorName || '—'}</p>
          <p className="text-xs text-gray-500">{p.doctorSpecialty ?? '—'}</p>
        </div>
      ),
    },
    {
      key: 'items',
      header: 'Medications',
      render: (p) => <span className="text-sm text-gray-700 dark:text-gray-200">{p.items.length}</span>,
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Patient portal"
        title="My prescriptions"
        description="All medications prescribed to you during past visits."
      />

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(p) => p.id}
        isLoading={list.isPending}
        onRowClick={(p) => navigate(`/portal/prescriptions/${p.id}`)}
        emptyState={
          <EmptyState
            title="No prescriptions yet"
            description="Any prescriptions issued to you will appear here."
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
