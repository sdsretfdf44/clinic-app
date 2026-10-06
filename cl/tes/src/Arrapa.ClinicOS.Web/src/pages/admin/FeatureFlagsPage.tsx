import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
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
  upsertFlagSchema,
  useFlagsList,
  useUpsertFlag,
  type FeatureFlag,
  type UpsertFlagFormValues,
} from '@/features/admin/featureFlags';

const defaults: UpsertFlagFormValues = {
  key: '',
  name: '',
  description: '',
  defaultEnabled: false,
  isGlobal: false,
};

export default function FeatureFlagsPage() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<FeatureFlag | null>(null);

  const flags = useFlagsList();
  const upsert = useUpsertFlag(editing?.id);

  const form = useForm<UpsertFlagFormValues>({
    resolver: zodResolver(upsertFlagSchema),
    defaultValues: defaults,
  });

  useEffect(() => {
    if (!drawerOpen) return;
    if (editing) {
      form.reset({
        key: editing.key,
        name: editing.name,
        description: editing.description ?? '',
        defaultEnabled: editing.defaultEnabled,
        isGlobal: editing.isGlobal,
      });
    } else {
      form.reset(defaults);
    }
  }, [drawerOpen, editing, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        key: values.key.trim().toLowerCase(),
        name: values.name.trim(),
        description: values.description?.trim() || null,
        defaultEnabled: values.defaultEnabled,
        isGlobal: values.isGlobal,
      });
      setDrawerOpen(false);
      setEditing(null);
    } catch {
      /* shown via mutation error */
    }
  });

  const columns: Column<FeatureFlag>[] = [
    {
      key: 'flag',
      header: 'Flag',
      render: (f) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{f.name}</p>
          <p className="text-xs text-gray-500 font-mono">{f.key}</p>
        </div>
      ),
    },
    { key: 'scope', header: 'Scope', render: (f) => <Badge tone={f.isGlobal ? 'red' : 'brand'}>{f.isGlobal ? 'Global' : 'Per-tenant'}</Badge> },
    { key: 'default', header: 'Default', render: (f) => <Badge tone={f.defaultEnabled ? 'green' : 'gray'}>{f.defaultEnabled ? 'On' : 'Off'}</Badge> },
    { key: 'overrides', header: 'Overrides', align: 'right', render: (f) => f.overrideCount.toString() },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (f) => (
        <Button
          size="sm"
          variant="secondary"
          onClick={(e) => {
            e.stopPropagation();
            setEditing(f);
            setDrawerOpen(true);
          }}
        >
          Edit
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Super admin"
        title="Feature flags"
        description="Toggle features globally or per-tenant."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDrawerOpen(true);
            }}
          >
            New flag
          </Button>
        }
      />

      {flags.isPending && (
        <div className="flex items-center justify-center py-16 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading flags…
        </div>
      )}
      {flags.isError && <Alert variant="error">{humanizeError(flags.error)}</Alert>}
      {flags.data && flags.data.length === 0 && <EmptyState title="No flags defined yet" />}
      {flags.data && flags.data.length > 0 && (
        <DataTable columns={columns} rows={flags.data} keyExtractor={(f) => f.id} />
      )}

      <Drawer
        open={drawerOpen}
        onClose={() => {
          setDrawerOpen(false);
          setEditing(null);
        }}
        title={editing ? `Edit ${editing.name}` : 'New feature flag'}
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button variant="secondary" onClick={() => setDrawerOpen(false)}>Cancel</Button>
            <Button onClick={onSubmit} isLoading={upsert.isPending}>
              {editing ? 'Save' : 'Create'}
            </Button>
          </div>
        }
      >
        {upsert.isError && <Alert variant="error" className="mb-4">{humanizeError(upsert.error)}</Alert>}
        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <FormField id="key" label="Key" required hint="Lowercase, digits, underscores. Immutable once used in code." error={form.formState.errors.key?.message}>
            <Input id="key" invalid={!!form.formState.errors.key} {...form.register('key')} />
          </FormField>
          <FormField id="name" label="Display name" required error={form.formState.errors.name?.message}>
            <Input id="name" invalid={!!form.formState.errors.name} {...form.register('name')} />
          </FormField>
          <FormField id="description" label="Description" error={form.formState.errors.description?.message}>
            <Textarea id="description" rows={3} {...form.register('description')} />
          </FormField>
          <div className="flex items-center gap-6">
            <Switch
              checked={form.watch('defaultEnabled')}
              onCheckedChange={(v) => form.setValue('defaultEnabled', v, { shouldDirty: true })}
              label="Enabled by default"
            />
            <Switch
              checked={form.watch('isGlobal')}
              onCheckedChange={(v) => form.setValue('isGlobal', v, { shouldDirty: true })}
              label="Global (no per-tenant overrides)"
            />
          </div>
        </form>
      </Drawer>
    </div>
  );
}
