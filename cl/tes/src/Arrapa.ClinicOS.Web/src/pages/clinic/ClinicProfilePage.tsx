import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import { Alert, Badge, Button, FormField, Input, PageHeader, Spinner } from '@/components/ui';
import {
  updateClinicProfileSchema,
  useClinicProfile,
  useUpdateClinicProfile,
  type UpdateClinicProfileFormValues,
} from '@/features/clinic/profile';

export default function ClinicProfilePage() {
  const profile = useClinicProfile();
  const update = useUpdateClinicProfile();

  const form = useForm<UpdateClinicProfileFormValues>({
    resolver: zodResolver(updateClinicProfileSchema),
    defaultValues: {
      name: '', contactEmail: '', contactPhone: '', logoUrl: '',
      timeZone: 'UTC', locale: 'en-US', currency: 'USD', defaultConsultationFee: '',
    },
  });

  useEffect(() => {
    if (profile.data) {
      form.reset({
        name: profile.data.name,
        contactEmail: profile.data.contactEmail ?? '',
        contactPhone: profile.data.contactPhone ?? '',
        logoUrl: profile.data.logoUrl ?? '',
        timeZone: profile.data.timeZone,
        locale: profile.data.locale,
        currency: profile.data.currency,
        defaultConsultationFee: profile.data.defaultConsultationFee ?? '',
      });
    }
  }, [profile.data, form]);

  const onSubmit = form.handleSubmit((values) => {
    update.mutate({
      name: values.name.trim(),
      contactEmail: values.contactEmail?.trim() || null,
      contactPhone: values.contactPhone?.trim() || null,
      logoUrl: values.logoUrl?.trim() || null,
      timeZone: values.timeZone,
      locale: values.locale,
      currency: values.currency.toUpperCase(),
      defaultConsultationFee:
        values.defaultConsultationFee === '' || values.defaultConsultationFee === undefined
          ? null
          : Number(values.defaultConsultationFee),
    });
  });

  if (profile.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading…
      </div>
    );
  }
  if (profile.isError || !profile.data) {
    return <Alert variant="error">{humanizeError(profile.error)}</Alert>;
  }

  return (
    <div className="max-w-3xl">
      <PageHeader
        eyebrow="Clinic admin"
        title={profile.data.name}
        description={<span>/{profile.data.slug} <Badge tone="brand" className="ml-2">{profile.data.status}</Badge></span>}
      />

      <div className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 sm:p-8">
        {update.isSuccess && <Alert variant="success" className="mb-4">Clinic profile updated.</Alert>}
        {update.isError && <Alert variant="error" className="mb-4">{humanizeError(update.error)}</Alert>}

        <form onSubmit={onSubmit} className="space-y-5" noValidate>
          <FormField id="name" label="Clinic name" required error={form.formState.errors.name?.message}>
            <Input id="name" invalid={!!form.formState.errors.name} {...form.register('name')} />
          </FormField>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="contactEmail" label="Contact email" error={form.formState.errors.contactEmail?.message}>
              <Input id="contactEmail" type="email" {...form.register('contactEmail')} />
            </FormField>
            <FormField id="contactPhone" label="Contact phone" error={form.formState.errors.contactPhone?.message}>
              <Input id="contactPhone" type="tel" {...form.register('contactPhone')} />
            </FormField>
          </div>
          <FormField id="logoUrl" label="Logo URL" error={form.formState.errors.logoUrl?.message}>
            <Input id="logoUrl" type="url" {...form.register('logoUrl')} />
          </FormField>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="timeZone" label="Time zone" required error={form.formState.errors.timeZone?.message}>
              <Input id="timeZone" {...form.register('timeZone')} />
            </FormField>
            <FormField id="locale" label="Locale" required error={form.formState.errors.locale?.message}>
              <Input id="locale" {...form.register('locale')} />
            </FormField>
            <FormField id="currency" label="Currency" required error={form.formState.errors.currency?.message}>
              <Input id="currency" {...form.register('currency')} />
            </FormField>
          </div>

          <FormField
            id="defaultConsultationFee"
            label="Default consultation fee"
            hint="Optional. Departments and doctors can override this."
            error={form.formState.errors.defaultConsultationFee?.message}
          >
            <Input id="defaultConsultationFee" type="number" step="0.01" min="0" {...form.register('defaultConsultationFee')} />
          </FormField>

          <div className="flex justify-end pt-2">
            <Button type="submit" isLoading={update.isPending} disabled={!form.formState.isDirty}>
              Save changes
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
