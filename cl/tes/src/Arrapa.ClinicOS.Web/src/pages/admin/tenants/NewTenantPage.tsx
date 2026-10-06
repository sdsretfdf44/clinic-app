import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input, PageHeader, Select } from '@/components/ui';
import { usePlansList } from '@/features/admin/plans';
import { useCreateTenant } from '@/features/admin/tenants/hooks';
import { createTenantSchema, type CreateTenantFormValues } from '@/features/admin/tenants/schemas';

export default function NewTenantPage() {
  const navigate = useNavigate();
  const plans = usePlansList(false);
  const createTenant = useCreateTenant();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateTenantFormValues>({
    resolver: zodResolver(createTenantSchema),
    defaultValues: {
      name: '',
      slug: '',
      contactEmail: '',
      contactPhone: '',
      timeZone: 'UTC',
      locale: 'en-US',
      currency: 'USD',
      subscriptionPlanId: '',
      adminFirstName: '',
      adminLastName: '',
      adminEmail: '',
    },
  });

  const onSubmit = handleSubmit((values) => {
    createTenant.mutate(
      {
        name: values.name.trim(),
        slug: values.slug.trim().toLowerCase(),
        contactEmail: values.contactEmail?.trim() || null,
        contactPhone: values.contactPhone?.trim() || null,
        timeZone: values.timeZone,
        locale: values.locale,
        currency: values.currency.toUpperCase(),
        subscriptionPlanId: values.subscriptionPlanId || null,
        adminFirstName: values.adminFirstName.trim(),
        adminLastName: values.adminLastName.trim(),
        adminEmail: values.adminEmail.trim(),
      },
      {
        onSuccess: (tenant) => navigate(`/admin/tenants/${tenant.id}`),
      },
    );
  });

  return (
    <div className="max-w-3xl">
      <PageHeader
        eyebrow="Super admin"
        title="Provision new tenant"
        description="Creates the clinic workspace and the initial clinic-admin user. A temporary password is emailed to the admin."
      />

      <form onSubmit={onSubmit} className="space-y-6" noValidate>
        {createTenant.isError && <Alert variant="error">{humanizeError(createTenant.error)}</Alert>}

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-4">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Clinic</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="name" label="Clinic name" required error={errors.name?.message}>
              <Input id="name" invalid={!!errors.name} {...register('name')} />
            </FormField>
            <FormField id="slug" label="Tenant slug" required hint="Lowercase, digits, hyphens." error={errors.slug?.message}>
              <Input id="slug" invalid={!!errors.slug} {...register('slug')} />
            </FormField>
            <FormField id="contactEmail" label="Contact email" error={errors.contactEmail?.message}>
              <Input id="contactEmail" type="email" invalid={!!errors.contactEmail} {...register('contactEmail')} />
            </FormField>
            <FormField id="contactPhone" label="Contact phone" error={errors.contactPhone?.message}>
              <Input id="contactPhone" type="tel" invalid={!!errors.contactPhone} {...register('contactPhone')} />
            </FormField>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="timeZone" label="Time zone" required error={errors.timeZone?.message}>
              <Input id="timeZone" {...register('timeZone')} />
            </FormField>
            <FormField id="locale" label="Locale" required error={errors.locale?.message}>
              <Input id="locale" {...register('locale')} />
            </FormField>
            <FormField id="currency" label="Currency" required error={errors.currency?.message}>
              <Input id="currency" {...register('currency')} />
            </FormField>
          </div>

          <FormField id="subscriptionPlanId" label="Subscription plan" hint="Defaults to the plan flagged as default.">
            <Select id="subscriptionPlanId" {...register('subscriptionPlanId')}>
              <option value="">Default plan</option>
              {plans.data?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} — {p.price} {p.currency}/{p.interval}
                </option>
              ))}
            </Select>
          </FormField>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-4">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Initial administrator</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="adminFirstName" label="First name" required error={errors.adminFirstName?.message}>
              <Input id="adminFirstName" invalid={!!errors.adminFirstName} {...register('adminFirstName')} />
            </FormField>
            <FormField id="adminLastName" label="Last name" required error={errors.adminLastName?.message}>
              <Input id="adminLastName" invalid={!!errors.adminLastName} {...register('adminLastName')} />
            </FormField>
          </div>
          <FormField id="adminEmail" label="Admin email" required error={errors.adminEmail?.message}>
            <Input id="adminEmail" type="email" invalid={!!errors.adminEmail} {...register('adminEmail')} />
          </FormField>
        </section>

        <div className="flex items-center justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={createTenant.isPending}>Provision tenant</Button>
        </div>
      </form>
    </div>
  );
}
