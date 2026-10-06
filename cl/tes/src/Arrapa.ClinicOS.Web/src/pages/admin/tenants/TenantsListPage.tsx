import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  DataTable,
  EmptyState,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import { useTenantsList } from '@/features/admin/tenants/hooks';
import type { TenantSummary } from '@/features/admin/tenants/types';

const statusTone = (status: string): 'green' | 'amber' | 'red' | 'gray' => {
  switch (status) {
    case 'Active': return 'green';
    case 'Trial': return 'amber';
    case 'Suspended': return 'red';
    default: return 'gray';
  }
};

export default function TenantsListPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const navigate = useNavigate();

  const params = { pageNumber, pageSize, search: search || undefined, status: status || undefined };
  const query = useTenantsList(params);

  const columns: Column<TenantSummary>[] = [
    {
      key: 'name',
      header: 'Tenant',
      render: (t) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{t.name}</p>
          <p className="text-xs text-gray-500">/{t.slug}</p>
        </div>
      ),
    },
    { key: 'status', header: 'Status', render: (t) => <Badge tone={statusTone(t.status)}>{t.status}</Badge> },
    { key: 'plan', header: 'Plan', render: (t) => t.subscriptionPlanName ?? <span className="text-gray-400">—</span> },
    { key: 'contact', header: 'Contact', render: (t) => t.contactEmail ?? <span className="text-gray-400">—</span> },
    { key: 'users', header: 'Users', align: 'right', render: (t) => t.userCount.toString() },
    {
      key: 'created',
      header: 'Created',
      render: (t) => new Date(t.createdOn).toLocaleDateString(),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Super admin"
        title="Tenants"
        description="Provision, manage, and suspend clinics on the platform."
        actions={
          <Button onClick={() => navigate('/admin/tenants/new')}>New tenant</Button>
        }
      />

      <div className="flex flex-col sm:flex-row gap-3">
        <Input
          placeholder="Search by name, slug, or email"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <Select
          className="sm:max-w-xs"
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPageNumber(1);
          }}
        >
          <option value="">All statuses</option>
          <option value="Active">Active</option>
          <option value="Trial">Trial</option>
          <option value="Suspended">Suspended</option>
          <option value="Cancelled">Cancelled</option>
          <option value="Expired">Expired</option>
        </Select>
      </div>

      {query.isError && <Alert variant="error">{humanizeError(query.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={query.data?.items ?? []}
        keyExtractor={(t) => t.id}
        isLoading={query.isPending}
        emptyState={
          <EmptyState
            title="No tenants yet"
            description="Provision your first tenant to get started."
            action={<Link to="/admin/tenants/new"><Button>Create tenant</Button></Link>}
          />
        }
        onRowClick={(t) => navigate(`/admin/tenants/${t.id}`)}
      />

      {query.data && (
        <Pagination
          pageNumber={query.data.pageNumber}
          pageSize={query.data.pageSize}
          totalCount={query.data.totalCount}
          totalPages={query.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}
    </div>
  );
}
