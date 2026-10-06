import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  DataTable,
  Drawer,
  FormField,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import {
  inviteUserSchema,
  useChangeUserRole,
  useClinicUsers,
  useInviteClinicUser,
  useSetClinicUserActive,
  type ClinicUser,
  type InviteUserFormValues,
} from '@/features/clinic/users';

export default function ClinicUsersPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [search, setSearch] = useState('');
  const [role, setRole] = useState('');
  const [activeFilter, setActiveFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [inviteOpen, setInviteOpen] = useState(false);
  const [confirmAction, setConfirmAction] = useState<{ user: ClinicUser; action: 'activate' | 'deactivate' } | null>(null);

  const params = {
    pageNumber,
    pageSize: 20,
    search: search || undefined,
    role: role || undefined,
    isActive: activeFilter === 'all' ? undefined : activeFilter === 'active',
  };
  const users = useClinicUsers(params);
  const invite = useInviteClinicUser();
  const changeRole = useChangeUserRole();
  const setActive = useSetClinicUserActive();

  const columns: Column<ClinicUser>[] = [
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
    {
      key: 'role',
      header: 'Role',
      render: (u) => (
        <Select
          className="max-w-[10rem]"
          value={u.primaryRole}
          onChange={(e) => changeRole.mutate({ id: u.id, primaryRole: e.target.value })}
        >
          <option value="ClinicAdmin">ClinicAdmin</option>
          <option value="Doctor">Doctor</option>
          <option value="Receptionist">Receptionist</option>
          <option value="Patient">Patient</option>
        </Select>
      ),
    },
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
          size="sm"
          variant={u.isActive ? 'danger' : 'primary'}
          onClick={() => setConfirmAction({ user: u, action: u.isActive ? 'deactivate' : 'activate' })}
        >
          {u.isActive ? 'Deactivate' : 'Activate'}
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Clinic admin"
        title="Clinic users"
        description="Everyone who can sign into this clinic. Manage roles, activate, or invite new team members."
        actions={<Button onClick={() => setInviteOpen(true)}>Invite user</Button>}
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

      {users.isError && <Alert variant="error">{humanizeError(users.error)}</Alert>}
      {changeRole.isError && <Alert variant="error">{humanizeError(changeRole.error)}</Alert>}
      {setActive.isError && <Alert variant="error">{humanizeError(setActive.error)}</Alert>}

      <DataTable columns={columns} rows={users.data?.items ?? []} keyExtractor={(u) => u.id} isLoading={users.isPending} />

      {users.data && (
        <Pagination
          pageNumber={users.data.pageNumber}
          pageSize={users.data.pageSize}
          totalCount={users.data.totalCount}
          totalPages={users.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}

      <InviteUserDrawer open={inviteOpen} onClose={() => setInviteOpen(false)} invite={invite} />

      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={() => {
          if (!confirmAction) return;
          setActive.mutate(
            { id: confirmAction.user.id, isActive: confirmAction.action === 'activate' },
            { onSettled: () => setConfirmAction(null) },
          );
        }}
        title={confirmAction?.action === 'deactivate' ? 'Deactivate user' : 'Activate user'}
        description={
          confirmAction
            ? confirmAction.action === 'deactivate'
              ? `Deactivate ${confirmAction.user.fullName}? Their active sessions will be revoked.`
              : `Reactivate ${confirmAction.user.fullName}?`
            : null
        }
        variant={confirmAction?.action === 'deactivate' ? 'danger' : 'primary'}
        confirmLabel={confirmAction?.action === 'deactivate' ? 'Deactivate' : 'Activate'}
        isLoading={setActive.isPending}
      />
    </div>
  );
}

function InviteUserDrawer({
  open,
  onClose,
  invite,
}: {
  open: boolean;
  onClose: () => void;
  invite: ReturnType<typeof useInviteClinicUser>;
}) {
  const form = useForm<InviteUserFormValues>({
    resolver: zodResolver(inviteUserSchema),
    defaultValues: { firstName: '', lastName: '', email: '', primaryRole: 'Receptionist' },
  });

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await invite.mutateAsync({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        primaryRole: values.primaryRole,
      });
      onClose();
      form.reset();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title="Invite user"
      description="They'll receive an email with a temporary password."
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={invite.isPending}>Send invite</Button>
        </div>
      }
    >
      {invite.isError && <Alert variant="error" className="mb-4">{humanizeError(invite.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="firstName" label="First name" required error={form.formState.errors.firstName?.message}>
            <Input id="firstName" {...form.register('firstName')} />
          </FormField>
          <FormField id="lastName" label="Last name" required error={form.formState.errors.lastName?.message}>
            <Input id="lastName" {...form.register('lastName')} />
          </FormField>
        </div>
        <FormField id="email" label="Email" required error={form.formState.errors.email?.message}>
          <Input id="email" type="email" {...form.register('email')} />
        </FormField>
        <FormField id="primaryRole" label="Role" required>
          <Select id="primaryRole" {...form.register('primaryRole')}>
            <option value="ClinicAdmin">ClinicAdmin</option>
            <option value="Doctor">Doctor</option>
            <option value="Receptionist">Receptionist</option>
            <option value="Patient">Patient</option>
          </Select>
        </FormField>
      </form>
    </Drawer>
  );
}
