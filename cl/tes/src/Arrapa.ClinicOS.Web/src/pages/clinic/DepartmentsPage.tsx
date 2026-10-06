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
  Switch,
  Textarea,
  type Column,
} from '@/components/ui';
import {
  upsertDepartmentSchema,
  useDeleteDepartment,
  useDepartments,
  useUpsertDepartment,
  type Department,
  type UpsertDepartmentFormValues,
} from '@/features/clinic/departments';

const defaults: UpsertDepartmentFormValues = {
  name: '',
  description: '',
  defaultConsultationFee: '',
  isActive: true,
  sortOrder: 0,
};

export default function DepartmentsPage() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Department | null>(null);

  const departments = useDepartments(true);
  const upsert = useUpsertDepartment(editing?.id);
  const remove = useDeleteDepartment();

  const form = useForm<UpsertDepartmentFormValues>({
    resolver: zodResolver(upsertDepartmentSchema),
    defaultValues: defaults,
  });

  useEffect(() => {
    if (!drawerOpen) return;
    if (editing) {
      form.reset({
        name: editing.name,
        description: editing.description ?? '',
        defaultConsultationFee: editing.defaultConsultationFee ?? '',
        isActive: editing.isActive,
        sortOrder: editing.sortOrder,
      });
    } else {
      form.reset(defaults);
    }
  }, [drawerOpen, editing, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        name: values.name.trim(),
        description: values.description?.trim() || null,
        defaultConsultationFee:
          values.defaultConsultationFee === '' || values.defaultConsultationFee === undefined
            ? null
            : Number(values.defaultConsultationFee),
        isActive: values.isActive,
        sortOrder: Number(values.sortOrder),
      });
      setDrawerOpen(false);
      setEditing(null);
    } catch {
      /* shown in mutation error */
    }
  });

  const columns: Column<Department>[] = [
    {
      key: 'name',
      header: 'Name',
      render: (d) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{d.name}</p>
          {d.description && <p className="text-xs text-gray-500 line-clamp-1">{d.description}</p>}
        </div>
      ),
    },
    { key: 'active', header: 'Status', render: (d) => <Badge tone={d.isActive ? 'green' : 'gray'}>{d.isActive ? 'Active' : 'Inactive'}</Badge> },
    { key: 'fee', header: 'Default fee', render: (d) => (d.defaultConsultationFee != null ? d.defaultConsultationFee.toFixed(2) : '—') },
    { key: 'doctors', header: 'Doctors', align: 'right', render: (d) => d.doctorCount.toString() },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (d) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={(e) => {
              e.stopPropagation();
              setEditing(d);
              setDrawerOpen(true);
            }}
          >
            Edit
          </Button>
          <Button
            size="sm"
            variant="danger"
            onClick={(e) => {
              e.stopPropagation();
              setConfirmDelete(d);
            }}
          >
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Clinic admin"
        title="Departments"
        description="Organize your clinic by specialty. Departments can be assigned to doctors and used as default fee sources."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDrawerOpen(true);
            }}
          >
            New department
          </Button>
        }
      />

      {departments.isPending && (
        <div className="flex items-center justify-center py-16 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading departments…
        </div>
      )}
      {departments.isError && <Alert variant="error">{humanizeError(departments.error)}</Alert>}
      {departments.data && departments.data.length === 0 && (
        <EmptyState
          title="No departments yet"
          description="Add your first department to organize your clinic."
          action={
            <Button
              onClick={() => {
                setEditing(null);
                setDrawerOpen(true);
              }}
            >
              New department
            </Button>
          }
        />
      )}
      {departments.data && departments.data.length > 0 && (
        <DataTable columns={columns} rows={departments.data} keyExtractor={(d) => d.id} />
      )}

      <Drawer
        open={drawerOpen}
        onClose={() => {
          setDrawerOpen(false);
          setEditing(null);
        }}
        title={editing ? `Edit ${editing.name}` : 'New department'}
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button variant="secondary" onClick={() => setDrawerOpen(false)}>Cancel</Button>
            <Button onClick={onSubmit} isLoading={upsert.isPending}>{editing ? 'Save' : 'Create'}</Button>
          </div>
        }
      >
        {upsert.isError && <Alert variant="error" className="mb-4">{humanizeError(upsert.error)}</Alert>}
        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <FormField id="name" label="Name" required error={form.formState.errors.name?.message}>
            <Input id="name" invalid={!!form.formState.errors.name} {...form.register('name')} />
          </FormField>
          <FormField id="description" label="Description" error={form.formState.errors.description?.message}>
            <Textarea id="description" rows={3} {...form.register('description')} />
          </FormField>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="defaultConsultationFee" label="Default fee" error={form.formState.errors.defaultConsultationFee?.message}>
              <Input id="defaultConsultationFee" type="number" step="0.01" min="0" {...form.register('defaultConsultationFee')} />
            </FormField>
            <FormField id="sortOrder" label="Sort order" error={form.formState.errors.sortOrder?.message}>
              <Input id="sortOrder" type="number" min="0" {...form.register('sortOrder')} />
            </FormField>
          </div>
          <Switch
            checked={form.watch('isActive')}
            onCheckedChange={(v) => form.setValue('isActive', v, { shouldDirty: true })}
            label="Active"
          />
        </form>
      </Drawer>

      <ConfirmDialog
        open={!!confirmDelete}
        onClose={() => setConfirmDelete(null)}
        onConfirm={() => {
          if (!confirmDelete) return;
          remove.mutate(confirmDelete.id, { onSettled: () => setConfirmDelete(null) });
        }}
        title="Delete department"
        description={
          confirmDelete ? `Delete "${confirmDelete.name}"? Departments with doctors assigned cannot be deleted.` : null
        }
        variant="danger"
        confirmLabel="Delete"
        isLoading={remove.isPending}
      />
    </div>
  );
}
