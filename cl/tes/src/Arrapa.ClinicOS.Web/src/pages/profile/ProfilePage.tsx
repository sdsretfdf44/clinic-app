import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  BuildingOffice2Icon,
  EnvelopeIcon,
  KeyIcon,
  ShieldCheckIcon,
  UserCircleIcon,
} from '@heroicons/react/24/outline';
import { Link } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  Card,
  FormField,
  Input,
  PageHeader,
  Spinner,
} from '@/components/ui';
import { useCurrentUser, useUpdateProfile } from '@/features/auth/hooks';
import { updateProfileSchema, type UpdateProfileFormValues } from '@/features/auth/schemas';

export default function ProfilePage() {
  const currentUser = useCurrentUser();
  const updateProfile = useUpdateProfile();
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<UpdateProfileFormValues>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      phoneNumber: '',
      avatarUrl: '',
    },
  });

  useEffect(() => {
    if (currentUser.data) {
      reset({
        firstName: currentUser.data.firstName,
        lastName: currentUser.data.lastName,
        phoneNumber: currentUser.data.phoneNumber ?? '',
        avatarUrl: currentUser.data.avatarUrl ?? '',
      });
    }
  }, [currentUser.data, reset]);

  const onSubmit = handleSubmit((values) => {
    setSuccessMessage(null);
    updateProfile.mutate(
      {
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        phoneNumber: values.phoneNumber?.trim() || null,
        avatarUrl: values.avatarUrl?.trim() || null,
      },
      {
        onSuccess: () => setSuccessMessage('Profile updated.'),
      },
    );
  });

  if (currentUser.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500 dark:text-gray-400">
        <Spinner className="h-6 w-6 mr-2" />
        Loading profile…
      </div>
    );
  }

  const user = currentUser.data;

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Account"
        title="Your profile"
        description="Update the name and contact information shown across ClinicOS."
      />

      <div className="grid grid-cols-1 lg:grid-cols-[320px_1fr] gap-6">
        {/* Left — identity summary */}
        <Card padding="lg" className="h-fit">
          <div className="flex flex-col items-center text-center">
            <Avatar name={user?.fullName} src={user?.avatarUrl} size="xl" />
            <p className="mt-4 text-title font-semibold text-gray-900 dark:text-white truncate max-w-full">
              {user?.fullName || '—'}
            </p>
            {user?.primaryRole && (
              <Badge tone="brand" variant="dot" className="mt-2">
                {user.primaryRole}
              </Badge>
            )}
          </div>

          <ul className="mt-6 pt-5 border-t border-gray-100 dark:border-gray-800 space-y-3 text-body">
            <li className="flex items-start gap-3">
              <EnvelopeIcon className="h-4 w-4 mt-0.5 text-gray-400 shrink-0" />
              <span className="text-gray-700 dark:text-gray-200 truncate min-w-0">{user?.email ?? '—'}</span>
            </li>
            {user?.tenantName && (
              <li className="flex items-start gap-3">
                <BuildingOffice2Icon className="h-4 w-4 mt-0.5 text-gray-400 shrink-0" />
                <span className="text-gray-700 dark:text-gray-200 truncate min-w-0">
                  {user.tenantName}
                  {user.tenantSlug && (
                    <span className="ml-1.5 text-caption text-gray-400 font-mono">/{user.tenantSlug}</span>
                  )}
                </span>
              </li>
            )}
          </ul>

          <div className="mt-6 pt-5 border-t border-gray-100 dark:border-gray-800 space-y-2">
            <Link
              to="/change-password"
              className="group flex items-center gap-3 rounded-lg px-3 py-2 hover:bg-gray-50 dark:hover:bg-gray-800/60 transition-colors"
            >
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300">
                <KeyIcon className="h-4 w-4" />
              </span>
              <span className="text-body font-medium text-gray-800 dark:text-gray-100">Change password</span>
            </Link>
            <Link
              to="/two-factor"
              className="group flex items-center gap-3 rounded-lg px-3 py-2 hover:bg-gray-50 dark:hover:bg-gray-800/60 transition-colors"
            >
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-teal-50 text-teal-600 dark:bg-teal-500/10 dark:text-teal-300">
                <ShieldCheckIcon className="h-4 w-4" />
              </span>
              <span className="text-body font-medium text-gray-800 dark:text-gray-100">Two-factor auth</span>
            </Link>
          </div>
        </Card>

        {/* Right — edit form */}
        <Card padding="lg">
          <div className="flex items-start justify-between gap-3 mb-5">
            <div>
              <h3 className="text-title font-semibold text-gray-900 dark:text-white">Personal details</h3>
              <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">
                Name and contact information shown on your account.
              </p>
            </div>
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-300">
              <UserCircleIcon className="h-5 w-5" />
            </span>
          </div>

          <form onSubmit={onSubmit} className="space-y-5" noValidate>
            {successMessage && <Alert variant="success">{successMessage}</Alert>}
            {updateProfile.isError && <Alert variant="error">{humanizeError(updateProfile.error)}</Alert>}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <FormField id="firstName" label="First name" required error={errors.firstName?.message}>
                <Input id="firstName" invalid={!!errors.firstName} {...register('firstName')} />
              </FormField>
              <FormField id="lastName" label="Last name" required error={errors.lastName?.message}>
                <Input id="lastName" invalid={!!errors.lastName} {...register('lastName')} />
              </FormField>
            </div>

            <FormField id="phoneNumber" label="Phone number" error={errors.phoneNumber?.message}>
              <Input id="phoneNumber" type="tel" {...register('phoneNumber')} />
            </FormField>

            <FormField
              id="avatarUrl"
              label="Avatar URL"
              hint="A publicly accessible image URL."
              error={errors.avatarUrl?.message}
            >
              <Input id="avatarUrl" type="url" {...register('avatarUrl')} />
            </FormField>

            <div className="flex items-center justify-end gap-3 pt-2 border-t border-gray-100 dark:border-gray-800">
              <Button
                type="button"
                variant="secondary"
                onClick={() =>
                  currentUser.data &&
                  reset({
                    firstName: currentUser.data.firstName,
                    lastName: currentUser.data.lastName,
                    phoneNumber: currentUser.data.phoneNumber ?? '',
                    avatarUrl: currentUser.data.avatarUrl ?? '',
                  })
                }
                disabled={!isDirty}
              >
                Discard
              </Button>
              <Button type="submit" isLoading={updateProfile.isPending} disabled={!isDirty}>
                Save changes
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </div>
  );
}
