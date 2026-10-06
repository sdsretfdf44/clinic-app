import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  Drawer,
  EmptyState,
  FormField,
  Input,
  PageHeader,
  Select,
  Spinner,
  Switch,
  Textarea,
} from '@/components/ui';
import {
  upsertPlanSchema,
  useCreatePlan,
  usePlansList,
  useUpdatePlan,
  type SubscriptionPlan,
  type UpsertPlanFormValues,
} from '@/features/admin/plans';

const defaultValues: UpsertPlanFormValues = {
  code: '',
  name: '',
  description: '',
  interval: 'Monthly',
  price: 0,
  currency: 'USD',
  maxUsers: '',
  maxPatients: '',
  maxAppointmentsPerMonth: '',
  features: [],
  isActive: true,
  isDefault: false,
  sortOrder: 0,
};

function toNullableNumber(value: number | '' | undefined): number | null {
  if (value === '' || value === undefined || Number.isNaN(Number(value))) {
    return null;
  }
  return Number(value);
}

export default function SubscriptionPlansPage() {
  const [includeInactive, setIncludeInactive] = useState(true);
  const [editing, setEditing] = useState<SubscriptionPlan | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [featuresInput, setFeaturesInput] = useState('');

  const plansQuery = usePlansList(includeInactive);
  const createPlan = useCreatePlan();
  const updatePlan = useUpdatePlan(editing?.id ?? '');

  const form = useForm<UpsertPlanFormValues>({
    resolver: zodResolver(upsertPlanSchema),
    defaultValues,
  });

  useEffect(() => {
    if (drawerOpen) {
      if (editing) {
        form.reset({
          code: editing.code,
          name: editing.name,
          description: editing.description ?? '',
          interval: editing.interval,
          price: editing.price,
          currency: editing.currency,
          maxUsers: editing.maxUsers ?? '',
          maxPatients: editing.maxPatients ?? '',
          maxAppointmentsPerMonth: editing.maxAppointmentsPerMonth ?? '',
          features: editing.features,
          isActive: editing.isActive,
          isDefault: editing.isDefault,
          sortOrder: editing.sortOrder,
        });
        setFeaturesInput(editing.features.join(', '));
      } else {
        form.reset(defaultValues);
        setFeaturesInput('');
      }
    }
  }, [drawerOpen, editing, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    const features = featuresInput
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);

    const payload = {
      code: values.code.trim().toLowerCase(),
      name: values.name.trim(),
      description: values.description?.trim() || null,
      interval: values.interval,
      price: Number(values.price),
      currency: values.currency.trim().toUpperCase(),
      maxUsers: toNullableNumber(values.maxUsers),
      maxPatients: toNullableNumber(values.maxPatients),
      maxAppointmentsPerMonth: toNullableNumber(values.maxAppointmentsPerMonth),
      features,
      isActive: values.isActive,
      isDefault: values.isDefault,
      sortOrder: Number(values.sortOrder),
    };

    try {
      if (editing) {
        await updatePlan.mutateAsync(payload);
      } else {
        await createPlan.mutateAsync(payload);
      }
      setDrawerOpen(false);
      setEditing(null);
    } catch {
      // shown via mutation error state
    }
  });

  const openNew = () => {
    setEditing(null);
    setDrawerOpen(true);
  };

  const openEdit = (plan: SubscriptionPlan) => {
    setEditing(plan);
    setDrawerOpen(true);
  };

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Super admin"
        title="Subscription plans"
        description="Configure the catalog of plans tenants can subscribe to."
        actions={
          <div className="flex items-center gap-3">
            <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-300">
              <input
                type="checkbox"
                className="rounded border-gray-300"
                checked={includeInactive}
                onChange={(e) => setIncludeInactive(e.target.checked)}
              />
              Include inactive
            </label>
            <Button onClick={openNew}>New plan</Button>
          </div>
        }
      />

      {plansQuery.isPending && (
        <div className="flex items-center justify-center py-16 text-gray-500">
          <Spinner className="h-5 w-5 mr-2" /> Loading plans…
        </div>
      )}

      {plansQuery.isError && <Alert variant="error">{humanizeError(plansQuery.error)}</Alert>}

      {plansQuery.data && plansQuery.data.length === 0 && (
        <EmptyState title="No plans yet" description="Create your first plan to start selling subscriptions." action={<Button onClick={openNew}>New plan</Button>} />
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {plansQuery.data?.map((plan) => (
          <button
            key={plan.id}
            type="button"
            onClick={() => openEdit(plan)}
            className="text-left rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 hover:border-brand-400 transition"
          >
            <div className="flex items-start justify-between">
              <div>
                <p className="text-sm font-semibold text-gray-900 dark:text-white">{plan.name}</p>
                <p className="text-xs text-gray-500">/{plan.code}</p>
              </div>
              <div className="flex items-center gap-2">
                {plan.isDefault && <Badge tone="brand">Default</Badge>}
                <Badge tone={plan.isActive ? 'green' : 'gray'}>{plan.isActive ? 'Active' : 'Inactive'}</Badge>
              </div>
            </div>
            <p className="mt-3 text-2xl font-semibold text-gray-900 dark:text-white">
              {plan.price} {plan.currency}
              <span className="text-sm font-normal text-gray-500">/{plan.interval === 'Monthly' ? 'mo' : 'yr'}</span>
            </p>
            {plan.description && <p className="mt-2 text-sm text-gray-500 dark:text-gray-400 line-clamp-2">{plan.description}</p>}
            <dl className="mt-3 grid grid-cols-3 gap-2 text-xs text-gray-500 dark:text-gray-400">
              <div>
                <dt className="uppercase tracking-widest">Users</dt>
                <dd className="text-gray-800 dark:text-gray-100">{plan.maxUsers ?? '∞'}</dd>
              </div>
              <div>
                <dt className="uppercase tracking-widest">Patients</dt>
                <dd className="text-gray-800 dark:text-gray-100">{plan.maxPatients ?? '∞'}</dd>
              </div>
              <div>
                <dt className="uppercase tracking-widest">Clinics</dt>
                <dd className="text-gray-800 dark:text-gray-100">{plan.tenantCount}</dd>
              </div>
            </dl>
          </button>
        ))}
      </div>

      <Drawer
        open={drawerOpen}
        onClose={() => {
          setDrawerOpen(false);
          setEditing(null);
        }}
        title={editing ? `Edit ${editing.name}` : 'New subscription plan'}
        size="lg"
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button variant="secondary" onClick={() => setDrawerOpen(false)}>Cancel</Button>
            <Button onClick={onSubmit} isLoading={createPlan.isPending || updatePlan.isPending}>
              {editing ? 'Save changes' : 'Create plan'}
            </Button>
          </div>
        }
      >
        {(createPlan.isError || updatePlan.isError) && (
          <Alert variant="error" className="mb-4">
            {humanizeError(createPlan.error ?? updatePlan.error)}
          </Alert>
        )}

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="code" label="Code" required error={form.formState.errors.code?.message}>
              <Input id="code" invalid={!!form.formState.errors.code} {...form.register('code')} />
            </FormField>
            <FormField id="name" label="Name" required error={form.formState.errors.name?.message}>
              <Input id="name" invalid={!!form.formState.errors.name} {...form.register('name')} />
            </FormField>
          </div>
          <FormField id="description" label="Description" error={form.formState.errors.description?.message}>
            <Textarea id="description" rows={3} {...form.register('description')} />
          </FormField>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="interval" label="Interval">
              <Select id="interval" {...form.register('interval')}>
                <option value="Monthly">Monthly</option>
                <option value="Annual">Annual</option>
              </Select>
            </FormField>
            <FormField id="price" label="Price" required error={form.formState.errors.price?.message}>
              <Input id="price" type="number" step="0.01" min="0" {...form.register('price')} />
            </FormField>
            <FormField id="currency" label="Currency" required error={form.formState.errors.currency?.message}>
              <Input id="currency" {...form.register('currency')} />
            </FormField>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="maxUsers" label="Max users" hint="Blank = unlimited">
              <Input id="maxUsers" type="number" min="0" {...form.register('maxUsers')} />
            </FormField>
            <FormField id="maxPatients" label="Max patients" hint="Blank = unlimited">
              <Input id="maxPatients" type="number" min="0" {...form.register('maxPatients')} />
            </FormField>
            <FormField id="maxAppointmentsPerMonth" label="Appointments / mo" hint="Blank = unlimited">
              <Input id="maxAppointmentsPerMonth" type="number" min="0" {...form.register('maxAppointmentsPerMonth')} />
            </FormField>
          </div>
          <FormField id="features" label="Features" hint="Comma-separated feature keys (e.g. appointments, billing, reports).">
            <Textarea id="features" rows={2} value={featuresInput} onChange={(e) => setFeaturesInput(e.target.value)} />
          </FormField>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 items-center">
            <FormField id="sortOrder" label="Sort order">
              <Input id="sortOrder" type="number" min="0" {...form.register('sortOrder')} />
            </FormField>
            <div className="flex items-center gap-3">
              <Switch
                checked={form.watch('isActive')}
                onCheckedChange={(v) => form.setValue('isActive', v, { shouldDirty: true })}
                label="Active"
              />
            </div>
            <div className="flex items-center gap-3">
              <Switch
                checked={form.watch('isDefault')}
                onCheckedChange={(v) => form.setValue('isDefault', v, { shouldDirty: true })}
                label="Default plan"
              />
            </div>
          </div>
        </form>
      </Drawer>
    </div>
  );
}
