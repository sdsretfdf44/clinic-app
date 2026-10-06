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
  Select,
  Spinner,
  Switch,
  Textarea,
  type Column,
} from '@/components/ui';
import {
  upsertSettingSchema,
  useSettingsList,
  useUpsertSetting,
  type GlobalSetting,
  type UpsertSettingFormValues,
} from '@/features/admin/settings';

const defaults: UpsertSettingFormValues = {
  key: '',
  value: '',
  category: 'General',
  dataType: 'String',
  description: '',
  isSecret: false,
};

export default function GlobalSettingsPage() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<GlobalSetting | null>(null);

  const settings = useSettingsList();
  const upsert = useUpsertSetting();

  const form = useForm<UpsertSettingFormValues>({
    resolver: zodResolver(upsertSettingSchema),
    defaultValues: defaults,
  });

  useEffect(() => {
    if (!drawerOpen) return;
    if (editing) {
      form.reset({
        key: editing.key,
        value: editing.isSecret ? '' : editing.value,
        category: editing.category,
        dataType: editing.dataType,
        description: editing.description ?? '',
        isSecret: editing.isSecret,
      });
    } else {
      form.reset(defaults);
    }
  }, [drawerOpen, editing, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        key: values.key.trim().toLowerCase(),
        value: values.value,
        category: values.category.trim(),
        dataType: values.dataType,
        description: values.description?.trim() || null,
        isSecret: values.isSecret,
      });
      setDrawerOpen(false);
      setEditing(null);
    } catch {
      /* shown */
    }
  });

  const columns: Column<GlobalSetting>[] = [
    {
      key: 'key',
      header: 'Key',
      render: (s) => (
        <div>
          <p className="font-mono text-xs text-gray-900 dark:text-white">{s.key}</p>
          {s.description && <p className="text-xs text-gray-500 mt-1 line-clamp-1">{s.description}</p>}
        </div>
      ),
    },
    { key: 'category', header: 'Category', render: (s) => <Badge tone="brand">{s.category}</Badge> },
    { key: 'type', header: 'Type', render: (s) => <Badge tone="gray">{s.dataType}</Badge> },
    {
      key: 'value',
      header: 'Value',
      render: (s) => (
        <span className={s.isSecret ? 'text-gray-400 font-mono' : 'font-mono text-xs text-gray-700 dark:text-gray-100'}>
          {s.isSecret ? '••••••' : s.value}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (s) => (
        <Button
          size="sm"
          variant="secondary"
          onClick={(e) => {
            e.stopPropagation();
            setEditing(s);
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
        title="Global settings"
        description="Platform-wide configuration entries."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDrawerOpen(true);
            }}
          >
            New setting
          </Button>
        }
      />

      {settings.isPending && (
        <div className="flex items-center justify-center py-16 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading…
        </div>
      )}
      {settings.isError && <Alert variant="error">{humanizeError(settings.error)}</Alert>}
      {settings.data && settings.data.length === 0 && <EmptyState title="No settings yet" />}
      {settings.data && settings.data.length > 0 && (
        <DataTable columns={columns} rows={settings.data} keyExtractor={(s) => s.id} />
      )}

      <Drawer
        open={drawerOpen}
        onClose={() => {
          setDrawerOpen(false);
          setEditing(null);
        }}
        title={editing ? `Edit ${editing.key}` : 'New setting'}
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
          <FormField id="key" label="Key" required hint="Lowercase; letters, digits, underscores, dots." error={form.formState.errors.key?.message}>
            <Input id="key" invalid={!!form.formState.errors.key} {...form.register('key')} readOnly={!!editing} />
          </FormField>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="category" label="Category" required error={form.formState.errors.category?.message}>
              <Input id="category" {...form.register('category')} />
            </FormField>
            <FormField id="dataType" label="Data type" required>
              <Select id="dataType" {...form.register('dataType')}>
                <option value="String">String</option>
                <option value="Integer">Integer</option>
                <option value="Decimal">Decimal</option>
                <option value="Boolean">Boolean</option>
                <option value="Json">JSON</option>
              </Select>
            </FormField>
          </div>
          <FormField id="value" label="Value" required error={form.formState.errors.value?.message}>
            <Textarea id="value" rows={form.watch('dataType') === 'Json' ? 6 : 2} {...form.register('value')} />
          </FormField>
          <FormField id="description" label="Description" error={form.formState.errors.description?.message}>
            <Textarea id="description" rows={2} {...form.register('description')} />
          </FormField>
          <Switch
            checked={form.watch('isSecret')}
            onCheckedChange={(v) => form.setValue('isSecret', v, { shouldDirty: true })}
            label="Secret (value hidden in lists)"
          />
        </form>
      </Drawer>
    </div>
  );
}
