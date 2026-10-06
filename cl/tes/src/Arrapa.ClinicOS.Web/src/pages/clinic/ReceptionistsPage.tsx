import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  DataTable,
  Drawer,
  EmptyState,
  FormField,
  Input,
  PageHeader,
  Spinner,
  type Column,
} from '@/components/ui';
import {
  createReceptionistSchema,
  updateReceptionistSchema,
  useCreateReceptionist,
  useDeleteReceptionist,
  useReceptionists,
  useSetReceptionistActive,
  useUpdateReceptionist,
  type CreateReceptionistFormValues,
  type Receptionist,
  type UpdateReceptionistFormValues,
} from '@/features/clinic/receptionists';

export default function ReceptionistsPage() {
  const list = useReceptionists(true);
  const create = useCreateReceptionist();
  const setActive = useSetReceptionistActive();
  const remove = useDeleteReceptionist();

  const [mode, setMode] = useState<'create' | 'edit'>('create');
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<Receptionist | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Receptionist | null>(null);

  const columns: Column<Receptionist>[] = [
    {
      key: 'name',
      header: 'Name',
      render: (r) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{r.fullName}</p>
          <p className="text-xs text-gray-500">{r.email}</p>
        </div>
      ),
    },
    { key: 'title', header: 'Title', render: (r) => r.title ?? <span className="text-gray-400">—</span> },
    { key: 'phone', header: 'Phone', render: (r) => r.phoneNumber ?? <span className="text-gray-400">—</span> },
    { key: 'status', header: 'Status', render: (r) => <Badge tone={r.isActive ? 'green' : 'gray'}>{r.isActive ? 'Active' : 'Inactive'}</Badge> },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (r) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={() => {
              setEditing(r);
              setMode('edit');
              setDrawerOpen(true);
            }}
          >
            Edit
          </Button>
          <Button
            size="sm"
            variant={r.isActive ? 'danger' : 'primary'}
            onClick={() => setActive.mutate({ id: r.id, isActive: !r.isActive })}
          >
            {r.isActive ? 'Deactivate' : 'Activate'}
          </Button>
          <Button size="sm" variant="ghost" onClick={() => setConfirmDelete(r)}>Remove</Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Clinic admin"
        title="Receptionists"
        description="Front-desk staff who handle registrations, check-ins, and billing."
        actions={
          <Button
            onClick={() => {
              setMode('create');
              setEditing(null);
              setDrawerOpen(true);
            }}
          >
            New receptionist
          </Button>
        }
      />

      {list.isPending && <div className="flex items-center justify-center py-16 text-gray-500"><Spinner className="h-5 w-5 mr-2" /> Loading…</div>}
      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}
      {list.data && list.data.length === 0 && <EmptyState title="No receptionists yet" description="Add your first receptionist." />}
      {list.data && list.data.length > 0 && (
        <DataTable columns={columns} rows={list.data} keyExtractor={(r) => r.id} />
      )}

      {mode === 'create' && (
        <CreateReceptionistDrawer open={drawerOpen} onClose={() => setDrawerOpen(false)} create={create} />
      )}
      {mode === 'edit' && editing && (
        <EditReceptionistDrawer
          open={drawerOpen}
          onClose={() => {
            setDrawerOpen(false);
            setEditing(null);
          }}
          receptionist={editing}
        />
      )}

      <ConfirmDialog
        open={!!confirmDelete}
        onClose={() => setConfirmDelete(null)}
        onConfirm={() => {
          if (!confirmDelete) return;
          remove.mutate(confirmDelete.id, { onSettled: () => setConfirmDelete(null) });
        }}
        title="Remove receptionist"
        description={confirmDelete ? `Remove ${confirmDelete.fullName}?` : null}
        variant="danger"
        confirmLabel="Remove"
        isLoading={remove.isPending}
      />
    </div>
  );
}

function CreateReceptionistDrawer({
  open,
  onClose,
  create,
}: {
  open: boolean;
  onClose: () => void;
  create: ReturnType<typeof useCreateReceptionist>;
}) {
  const form = useForm<CreateReceptionistFormValues>({
    resolver: zodResolver(createReceptionistSchema),
    defaultValues: { firstName: '', lastName: '', email: '', phoneNumber: '', title: '' },
  });

  useEffect(() => {
    if (open) form.reset();
  }, [open, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await create.mutateAsync({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        phoneNumber: values.phoneNumber?.trim() || null,
        title: values.title?.trim() || null,
      });
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title="New receptionist"
      description="A user account will be provisioned; a temporary password is emailed."
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={create.isPending}>Create</Button>
        </div>
      }
    >
      {create.isError && <Alert variant="error" className="mb-4">{humanizeError(create.error)}</Alert>}
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
        <FormField id="phoneNumber" label="Phone">
          <Input id="phoneNumber" type="tel" {...form.register('phoneNumber')} />
        </FormField>
        <FormField id="title" label="Title">
          <Input id="title" {...form.register('title')} />
        </FormField>
      </form>
    </Drawer>
  );
}

function EditReceptionistDrawer({
  open,
  onClose,
  receptionist,
}: {
  open: boolean;
  onClose: () => void;
  receptionist: Receptionist;
}) {
  const update = useUpdateReceptionist(receptionist.id);
  const form = useForm<UpdateReceptionistFormValues>({
    resolver: zodResolver(updateReceptionistSchema),
    defaultValues: {
      firstName: receptionist.firstName,
      lastName: receptionist.lastName,
      phoneNumber: receptionist.phoneNumber ?? '',
      avatarUrl: receptionist.avatarUrl ?? '',
      title: receptionist.title ?? '',
      sortOrder: receptionist.sortOrder,
    },
  });

  useEffect(() => {
    form.reset({
      firstName: receptionist.firstName,
      lastName: receptionist.lastName,
      phoneNumber: receptionist.phoneNumber ?? '',
      avatarUrl: receptionist.avatarUrl ?? '',
      title: receptionist.title ?? '',
      sortOrder: receptionist.sortOrder,
    });
  }, [receptionist, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await update.mutateAsync({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        phoneNumber: values.phoneNumber?.trim() || null,
        avatarUrl: values.avatarUrl?.trim() || null,
        title: values.title?.trim() || null,
        sortOrder: Number(values.sortOrder),
      });
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={receptionist.fullName}
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={update.isPending}>Save changes</Button>
        </div>
      }
    >
      {update.isError && <Alert variant="error" className="mb-4">{humanizeError(update.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="firstName" label="First name" required>
            <Input id="firstName" {...form.register('firstName')} />
          </FormField>
          <FormField id="lastName" label="Last name" required>
            <Input id="lastName" {...form.register('lastName')} />
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="phoneNumber" label="Phone">
            <Input id="phoneNumber" type="tel" {...form.register('phoneNumber')} />
          </FormField>
          <FormField id="avatarUrl" label="Avatar URL">
            <Input id="avatarUrl" type="url" {...form.register('avatarUrl')} />
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="title" label="Title">
            <Input id="title" {...form.register('title')} />
          </FormField>
          <FormField id="sortOrder" label="Sort order">
            <Input id="sortOrder" type="number" min="0" {...form.register('sortOrder')} />
          </FormField>
        </div>
      </form>
    </Drawer>
  );
}
