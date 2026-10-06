import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  Drawer,
  FormField,
  Input,
  PageHeader,
  Select,
  Spinner,
  Textarea,
} from '@/components/ui';
import { usePlansList } from '@/features/admin/plans';
import {
  useActivateTenant,
  useAssignPlan,
  useDeleteTenant,
  useSuspendTenant,
  useTenant,
  useUpdateTenant,
} from '@/features/admin/tenants/hooks';
import {
  assignPlanSchema,
  suspendTenantSchema,
  updateTenantSchema,
  type AssignPlanFormValues,
  type SuspendTenantFormValues,
  type UpdateTenantFormValues,
} from '@/features/admin/tenants/schemas';

export default function TenantDetailPage() {
  const params = useParams<{ id: string }>();
  const tenantId = params.id!;
  const navigate = useNavigate();

  const tenantQuery = useTenant(tenantId);
  const plans = usePlansList(true);
  const updateTenant = useUpdateTenant(tenantId);
  const suspendTenant = useSuspendTenant(tenantId);
  const activateTenant = useActivateTenant(tenantId);
  const assignPlan = useAssignPlan(tenantId);
  const deleteTenant = useDeleteTenant();

  const [suspendOpen, setSuspendOpen] = useState(false);
  const [assignPlanOpen, setAssignPlanOpen] = useState(false);
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

  const updateForm = useForm<UpdateTenantFormValues>({
    resolver: zodResolver(updateTenantSchema),
    defaultValues: {
      name: '', contactEmail: '', contactPhone: '', logoUrl: '', timeZone: 'UTC', locale: 'en-US', currency: 'USD',
    },
  });

  const suspendForm = useForm<SuspendTenantFormValues>({
    resolver: zodResolver(suspendTenantSchema),
    defaultValues: { reason: '' },
  });

  const assignPlanForm = useForm<AssignPlanFormValues>({
    resolver: zodResolver(assignPlanSchema),
    defaultValues: { subscriptionPlanId: '', expiresAt: '' },
  });

  useEffect(() => {
    if (tenantQuery.data) {
      updateForm.reset({
        name: tenantQuery.data.name,
        contactEmail: tenantQuery.data.contactEmail ?? '',
        contactPhone: tenantQuery.data.contactPhone ?? '',
        logoUrl: tenantQuery.data.logoUrl ?? '',
        timeZone: tenantQuery.data.timeZone,
        locale: tenantQuery.data.locale,
        currency: tenantQuery.data.currency,
      });
      assignPlanForm.reset({
        subscriptionPlanId: tenantQuery.data.subscriptionPlanId ?? '',
        expiresAt: tenantQuery.data.subscriptionExpiresAt?.slice(0, 10) ?? '',
      });
    }
  }, [tenantQuery.data, updateForm, assignPlanForm]);

  if (tenantQuery.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading tenant…
      </div>
    );
  }

  if (tenantQuery.isError || !tenantQuery.data) {
    return <Alert variant="error">{humanizeError(tenantQuery.error)}</Alert>;
  }

  const tenant = tenantQuery.data;
  const statusTone = tenant.status === 'Active' ? 'green' : tenant.status === 'Suspended' ? 'red' : 'amber';

  const onSaveDetails = updateForm.handleSubmit((values) => {
    updateTenant.mutate({
      name: values.name.trim(),
      contactEmail: values.contactEmail?.trim() || null,
      contactPhone: values.contactPhone?.trim() || null,
      logoUrl: values.logoUrl?.trim() || null,
      timeZone: values.timeZone,
      locale: values.locale,
      currency: values.currency.toUpperCase(),
    });
  });

  return (
    <div className="space-y-6 max-w-5xl">
      <PageHeader
        eyebrow={<Link to="/admin/tenants" className="hover:text-brand-600">← Back to tenants</Link>}
        title={tenant.name}
        description={
          <span className="flex items-center gap-2">
            <span>/{tenant.slug}</span>
            <Badge tone={statusTone}>{tenant.status}</Badge>
          </span>
        }
        actions={
          <>
            <Button variant="secondary" onClick={() => setAssignPlanOpen(true)}>Assign plan</Button>
            {tenant.status === 'Suspended' ? (
              <Button onClick={() => activateTenant.mutate()} isLoading={activateTenant.isPending}>Activate</Button>
            ) : (
              <Button variant="danger" onClick={() => setSuspendOpen(true)}>Suspend</Button>
            )}
          </>
        }
      />

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Tenant details</h3>
          <span className="text-xs text-gray-500">{tenant.userCount} users</span>
        </div>

        {updateTenant.isError && <Alert variant="error" className="mb-4">{humanizeError(updateTenant.error)}</Alert>}
        {updateTenant.isSuccess && <Alert variant="success" className="mb-4">Tenant updated.</Alert>}

        <form onSubmit={onSaveDetails} className="space-y-4" noValidate>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="name" label="Name" required error={updateForm.formState.errors.name?.message}>
              <Input id="name" invalid={!!updateForm.formState.errors.name} {...updateForm.register('name')} />
            </FormField>
            <FormField id="contactEmail" label="Contact email" error={updateForm.formState.errors.contactEmail?.message}>
              <Input id="contactEmail" type="email" {...updateForm.register('contactEmail')} />
            </FormField>
            <FormField id="contactPhone" label="Contact phone" error={updateForm.formState.errors.contactPhone?.message}>
              <Input id="contactPhone" type="tel" {...updateForm.register('contactPhone')} />
            </FormField>
            <FormField id="logoUrl" label="Logo URL" error={updateForm.formState.errors.logoUrl?.message}>
              <Input id="logoUrl" type="url" {...updateForm.register('logoUrl')} />
            </FormField>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="timeZone" label="Time zone" required error={updateForm.formState.errors.timeZone?.message}>
              <Input id="timeZone" {...updateForm.register('timeZone')} />
            </FormField>
            <FormField id="locale" label="Locale" required error={updateForm.formState.errors.locale?.message}>
              <Input id="locale" {...updateForm.register('locale')} />
            </FormField>
            <FormField id="currency" label="Currency" required error={updateForm.formState.errors.currency?.message}>
              <Input id="currency" {...updateForm.register('currency')} />
            </FormField>
          </div>
          <div className="flex justify-end gap-3 pt-1">
            <Button variant="danger" type="button" onClick={() => setConfirmDeleteOpen(true)}>
              Delete tenant
            </Button>
            <div className="flex-1" />
            <Button type="submit" isLoading={updateTenant.isPending} disabled={!updateForm.formState.isDirty}>
              Save changes
            </Button>
          </div>
        </form>
      </section>

      {tenant.status === 'Suspended' && (
        <Alert variant="warning" title="Tenant is suspended">
          Suspended on {tenant.suspendedAt ? new Date(tenant.suspendedAt).toLocaleString() : '—'}.
          {tenant.suspensionReason && <span> Reason: {tenant.suspensionReason}</span>}
        </Alert>
      )}

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Subscription</h3>
        <dl className="mt-4 grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm">
          <div>
            <dt className="text-xs uppercase tracking-widest text-gray-400">Plan</dt>
            <dd className="mt-1 text-gray-900 dark:text-white">{tenant.subscriptionPlanName ?? '—'}</dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-widest text-gray-400">Expires</dt>
            <dd className="mt-1 text-gray-900 dark:text-white">
              {tenant.subscriptionExpiresAt ? new Date(tenant.subscriptionExpiresAt).toLocaleDateString() : '—'}
            </dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-widest text-gray-400">Created</dt>
            <dd className="mt-1 text-gray-900 dark:text-white">{new Date(tenant.createdOn).toLocaleString()}</dd>
          </div>
        </dl>
      </section>

      <Drawer
        open={assignPlanOpen}
        onClose={() => setAssignPlanOpen(false)}
        title="Assign subscription plan"
        description="Change the plan or extend the current subscription."
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button variant="secondary" onClick={() => setAssignPlanOpen(false)}>Cancel</Button>
            <Button
              onClick={assignPlanForm.handleSubmit((v) => {
                assignPlan.mutate(
                  {
                    subscriptionPlanId: v.subscriptionPlanId,
                    expiresAt: v.expiresAt ? new Date(v.expiresAt).toISOString() : null,
                  },
                  { onSuccess: () => setAssignPlanOpen(false) },
                );
              })}
              isLoading={assignPlan.isPending}
            >
              Assign
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          {assignPlan.isError && <Alert variant="error">{humanizeError(assignPlan.error)}</Alert>}
          <FormField id="subscriptionPlanId" label="Plan" required error={assignPlanForm.formState.errors.subscriptionPlanId?.message}>
            <Select id="subscriptionPlanId" invalid={!!assignPlanForm.formState.errors.subscriptionPlanId} {...assignPlanForm.register('subscriptionPlanId')}>
              <option value="">Choose a plan</option>
              {plans.data?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} — {p.price} {p.currency}/{p.interval}{!p.isActive && ' (inactive)'}
                </option>
              ))}
            </Select>
          </FormField>
          <FormField id="expiresAt" label="Expires on" hint="Optional. Leave empty for indefinite.">
            <Input id="expiresAt" type="date" {...assignPlanForm.register('expiresAt')} />
          </FormField>
        </div>
      </Drawer>

      <Drawer
        open={suspendOpen}
        onClose={() => setSuspendOpen(false)}
        title="Suspend tenant"
        description="Users will be signed out and unable to sign back in."
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button variant="secondary" onClick={() => setSuspendOpen(false)}>Cancel</Button>
            <Button
              variant="danger"
              onClick={suspendForm.handleSubmit((v) => {
                suspendTenant.mutate(v.reason, { onSuccess: () => setSuspendOpen(false) });
              })}
              isLoading={suspendTenant.isPending}
            >
              Suspend
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          {suspendTenant.isError && <Alert variant="error">{humanizeError(suspendTenant.error)}</Alert>}
          <FormField id="reason" label="Reason" required error={suspendForm.formState.errors.reason?.message}>
            <Textarea id="reason" rows={4} invalid={!!suspendForm.formState.errors.reason} {...suspendForm.register('reason')} />
          </FormField>
        </div>
      </Drawer>

      <ConfirmDialog
        open={confirmDeleteOpen}
        onClose={() => setConfirmDeleteOpen(false)}
        onConfirm={() =>
          deleteTenant.mutate(tenant.id, {
            onSuccess: () => navigate('/admin/tenants', { replace: true }),
          })
        }
        title="Delete tenant"
        description={
          <>
            This soft-deletes the tenant, revoking access for all its users. Are you sure?
          </>
        }
        confirmLabel="Delete tenant"
        variant="danger"
        isLoading={deleteTenant.isPending}
      />
    </div>
  );
}
