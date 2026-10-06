import { useState } from 'react';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  DataTable,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import {
  useActivateUser,
  useDeactivateUser,
  useUsersList,
  type AdminUser,
} from '@/features/admin/users';

export default function UsersListPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [search, setSearch] = useState('');
  const [role, setRole] = useState('');
  const [activeFilter, setActiveFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [confirmAction, setConfirmAction] = useState<{ user: AdminUser; action: 'activate' | 'deactivate' } | null>(null);

  const params = {
    pageNumber,
    pageSize: 20,
    search: search || undefined,
    role: role || undefined,
    isActive: activeFilter === 'all' ? undefined : activeFilter === 'active',
  };
  const query = useUsersList(params);
  const activateUser = useActivateUser();
  const deactivateUser = useDeactivateUser();

  const columns: Column<AdminUser>[] = [
    {
      key: 'user',
      header: 'User',
      render: (u) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{u.fullName}</p>
          <p className="text-xs text-gray-500">{u.email}</p>
        </div>
      ),
    },
    { key: 'tenant', header: 'Tenant', render: (u) => u.tenantName },
    { key: 'role', header: 'Role', render: (u) => <Badge tone="brand">{u.primaryRole}</Badge> },
    {
      key: 'status',
      header: 'Status',
      render: (u) => (
        <div className="flex items-center gap-2">
          <Badge tone={u.isActive ? 'green' : 'gray'}>{u.isActive ? 'Active' : 'Inactive'}</Badge>
          {u.isLockedOut && <Badge tone="red">Locked</Badge>}
        </div>
      ),
    },
    { key: 'lastLogin', header: 'Last sign-in', render: (u) => (u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString() : '—') },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (u) => (
        <Button
          type="button"
          variant={u.isActive ? 'danger' : 'primary'}
          size="sm"
          onClick={(e) => {
            e.stopPropagation();
            setConfirmAction({ user: u, action: u.isActive ? 'deactivate' : 'activate' });
          }}
        >
          {u.isActive ? 'Deactivate' : 'Activate'}
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Super admin"
        title="Users"
        description="Cross-tenant view of every user on the platform."
      />

      <div className="flex flex-col sm:flex-row gap-3">
        <Input
          placeholder="Search by name or email"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <Select
          className="sm:max-w-xs"
          value={role}
          onChange={(e) => {
            setRole(e.target.value);
            setPageNumber(1);
          }}
        >
          <option value="">All roles</option>
          <option value="SuperAdmin">SuperAdmin</option>
          <option value="ClinicAdmin">ClinicAdmin</option>
          <option value="Doctor">Doctor</option>
          <option value="Receptionist">Receptionist</option>
          <option value="Patient">Patient</option>
        </Select>
        <Select
          className="sm:max-w-xs"
          value={activeFilter}
          onChange={(e) => {
            setActiveFilter(e.target.value as typeof activeFilter);
            setPageNumber(1);
          }}
        >
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </Select>
      </div>

      {query.isError && <Alert variant="error">{humanizeError(query.error)}</Alert>}

      <DataTable columns={columns} rows={query.data?.items ?? []} keyExtractor={(u) => u.id} isLoading={query.isPending} />

      {query.data && (
        <Pagination
          pageNumber={query.data.pageNumber}
          pageSize={query.data.pageSize}
          totalCount={query.data.totalCount}
          totalPages={query.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}

      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={() => {
          if (!confirmAction) return;
          const { user, action } = confirmAction;
          const mutation = action === 'activate' ? activateUser : deactivateUser;
          mutation.mutate(user.id, { onSettled: () => setConfirmAction(null) });
        }}
        title={confirmAction?.action === 'deactivate' ? 'Deactivate user' : 'Activate user'}
        description={
          confirmAction ? (
            <>
              {confirmAction.action === 'deactivate'
                ? `Deactivate ${confirmAction.user.fullName}? Their active sessions will be revoked.`
                : `Reactivate ${confirmAction.user.fullName}?`}
            </>
          ) : null
        }
        variant={confirmAction?.action === 'deactivate' ? 'danger' : 'primary'}
        confirmLabel={confirmAction?.action === 'deactivate' ? 'Deactivate' : 'Activate'}
        isLoading={activateUser.isPending || deactivateUser.isPending}
      />
    </div>
  );
}
