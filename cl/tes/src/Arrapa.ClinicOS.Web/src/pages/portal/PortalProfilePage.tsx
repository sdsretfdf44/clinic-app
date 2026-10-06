import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Button,
  FormField,
  Input,
  PageHeader,
  Spinner,
} from '@/components/ui';
import {
  updatePortalProfileSchema,
  usePortalProfile,
  useUpdatePortalProfile,
  type UpdatePortalProfileFormValues,
} from '@/features/portal';

export default function PortalProfilePage() {
  const profile = usePortalProfile();
  const update = useUpdatePortalProfile();
  const [saved, setSaved] = useState(false);

  const form = useForm<UpdatePortalProfileFormValues>({
    resolver: zodResolver(updatePortalProfileSchema),
    defaultValues: {
      phone: '', email: '', photoUrl: '',
      addressLine1: '', addressLine2: '',
      city: '', state: '', country: '', postalCode: '',
      occupation: '',
      emergencyContactName: '', emergencyContactPhone: '', emergencyContactRelationship: '',
    },
  });

  useEffect(() => {
    if (profile.data) {
      form.reset({
        phone: profile.data.phone ?? '',
        email: profile.data.email ?? '',
        photoUrl: profile.data.photoUrl ?? '',
        addressLine1: profile.data.addressLine1 ?? '',
        addressLine2: profile.data.addressLine2 ?? '',
        city: profile.data.city ?? '',
        state: profile.data.state ?? '',
        country: profile.data.country ?? '',
        postalCode: profile.data.postalCode ?? '',
        occupation: profile.data.occupation ?? '',
        emergencyContactName: profile.data.emergencyContactName ?? '',
        emergencyContactPhone: profile.data.emergencyContactPhone ?? '',
        emergencyContactRelationship: profile.data.emergencyContactRelationship ?? '',
      });
    }
  }, [profile.data, form]);

  if (profile.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading profile…
      </div>
    );
  }
  if (profile.isError || !profile.data) {
    return <Alert variant="error">{humanizeError(profile.error)}</Alert>;
  }

  const p = profile.data;

  const handle = form.handleSubmit((values) => {
    setSaved(false);
    update.mutate(
      {
        phone: values.phone?.trim() || null,
        email: values.email?.trim() || null,
        photoUrl: values.photoUrl?.trim() || null,
        addressLine1: values.addressLine1?.trim() || null,
        addressLine2: values.addressLine2?.trim() || null,
        city: values.city?.trim() || null,
        state: values.state?.trim() || null,
        country: values.country?.trim() || null,
        postalCode: values.postalCode?.trim() || null,
        occupation: values.occupation?.trim() || null,
        emergencyContactName: values.emergencyContactName?.trim() || null,
        emergencyContactPhone: values.emergencyContactPhone?.trim() || null,
        emergencyContactRelationship: values.emergencyContactRelationship?.trim() || null,
      },
      { onSuccess: () => setSaved(true) },
    );
  });

  return (
    <div className="space-y-6 max-w-3xl">
      <PageHeader
        eyebrow="Patient portal"
        title="My profile"
        description="Update your contact details and emergency contact. Clinical information is managed by the clinic."
      />

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <ReadField label="MRN" value={p.mrn} mono />
          <ReadField label="Full name" value={p.fullName} />
          <ReadField label="Date of birth" value={`${p.dateOfBirth} (${p.age})`} />
          <ReadField label="Gender" value={p.gender} />
          <ReadField label="Blood group" value={p.bloodGroup} />
          <ReadField label="Marital status" value={p.maritalStatus} />
        </div>
      </section>

      {saved && <Alert variant="success">Profile updated.</Alert>}
      {update.isError && <Alert variant="error">{humanizeError(update.error)}</Alert>}

      <form onSubmit={handle} className="space-y-6" noValidate>
        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Contact</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="phone" label="Phone" error={form.formState.errors.phone?.message}>
              <Input id="phone" {...form.register('phone')} />
            </FormField>
            <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
              <Input id="email" type="email" {...form.register('email')} />
            </FormField>
            <FormField id="photoUrl" label="Photo URL" error={form.formState.errors.photoUrl?.message}>
              <Input id="photoUrl" {...form.register('photoUrl')} />
            </FormField>
            <FormField id="occupation" label="Occupation">
              <Input id="occupation" {...form.register('occupation')} />
            </FormField>
          </div>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Address</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="addressLine1" label="Address line 1">
              <Input id="addressLine1" {...form.register('addressLine1')} />
            </FormField>
            <FormField id="addressLine2" label="Address line 2">
              <Input id="addressLine2" {...form.register('addressLine2')} />
            </FormField>
            <FormField id="city" label="City">
              <Input id="city" {...form.register('city')} />
            </FormField>
            <FormField id="state" label="State">
              <Input id="state" {...form.register('state')} />
            </FormField>
            <FormField id="country" label="Country">
              <Input id="country" {...form.register('country')} />
            </FormField>
            <FormField id="postalCode" label="Postal code">
              <Input id="postalCode" {...form.register('postalCode')} />
            </FormField>
          </div>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Emergency contact</h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="emergencyContactName" label="Name">
              <Input id="emergencyContactName" {...form.register('emergencyContactName')} />
            </FormField>
            <FormField id="emergencyContactPhone" label="Phone">
              <Input id="emergencyContactPhone" {...form.register('emergencyContactPhone')} />
            </FormField>
            <FormField id="emergencyContactRelationship" label="Relationship">
              <Input id="emergencyContactRelationship" {...form.register('emergencyContactRelationship')} />
            </FormField>
          </div>
        </section>

        {p.insuranceProvider && (
          <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
            <h2 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Insurance</h2>
            <p className="text-xs text-gray-500 mb-3">Managed by your clinic. Contact reception to change.</p>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <ReadField label="Provider" value={p.insuranceProvider ?? '—'} />
              <ReadField label="Policy number" value={p.insurancePolicyNumber ?? '—'} />
              <ReadField label="Group number" value={p.insuranceGroupNumber ?? '—'} />
              <ReadField label="Valid until" value={p.insuranceValidUntil ?? '—'} />
            </div>
          </section>
        )}

        <div className="flex justify-end">
          <Button type="submit" isLoading={update.isPending}>Save changes</Button>
        </div>
      </form>
    </div>
  );
}

function ReadField({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-widest text-gray-400">{label}</p>
      <p className={`text-sm text-gray-900 dark:text-gray-100 ${mono ? 'font-mono' : ''}`}>{value}</p>
    </div>
  );
}
