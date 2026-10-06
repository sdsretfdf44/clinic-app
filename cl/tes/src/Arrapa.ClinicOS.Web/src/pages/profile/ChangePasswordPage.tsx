import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input } from '@/components/ui';
import { useChangePassword } from '@/features/auth/hooks';
import { changePasswordSchema, type ChangePasswordFormValues } from '@/features/auth/schemas';

export default function ChangePasswordPage() {
  const changePassword = useChangePassword();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
  });

  const onSubmit = handleSubmit((values) => {
    changePassword.mutate(
      { currentPassword: values.currentPassword, newPassword: values.newPassword },
      { onSuccess: () => reset({ currentPassword: '', newPassword: '', confirmPassword: '' }) },
    );
  });

  return (
    <div className="max-w-xl">
      <header className="mb-6">
        <p className="text-xs uppercase tracking-widest text-brand-500 font-semibold">Security</p>
        <h1 className="mt-1 text-2xl font-semibold text-gray-900 dark:text-white">Change password</h1>
        <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Use a strong password you don't reuse elsewhere.
        </p>
      </header>

      <div className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 sm:p-8">
        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          {changePassword.isSuccess && (
            <Alert variant="success">Your password has been changed.</Alert>
          )}
          {changePassword.isError && <Alert variant="error">{humanizeError(changePassword.error)}</Alert>}

          <FormField id="currentPassword" label="Current password" required error={errors.currentPassword?.message}>
            <Input
              id="currentPassword"
              type="password"
              autoComplete="current-password"
              allowRevealPassword
              invalid={!!errors.currentPassword}
              {...register('currentPassword')}
            />
          </FormField>

          <FormField
            id="newPassword"
            label="New password"
            required
            hint="At least 10 characters, mix of upper, lower, digit, and symbol."
            error={errors.newPassword?.message}
          >
            <Input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              allowRevealPassword
              invalid={!!errors.newPassword}
              {...register('newPassword')}
            />
          </FormField>

          <FormField id="confirmPassword" label="Confirm new password" required error={errors.confirmPassword?.message}>
            <Input
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              allowRevealPassword
              invalid={!!errors.confirmPassword}
              {...register('confirmPassword')}
            />
          </FormField>

          <div className="flex justify-end pt-2">
            <Button type="submit" isLoading={changePassword.isPending}>
              Update password
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
